using System;
using System.Collections;
using System.Collections.Generic;
using LivingWorldMod.Utilities;
using Terraria.Utilities;

namespace LivingWorldMod.DataStructures.Classes;

/// <summary>
///     A implementation of a weighted randomizer which uses (for lack of a better term) a weighted self-balancing binary tree.
///     Adding/removing/updating items, and calls to both NextWithRemoval() and NextWithReplacement(), and are relatively fast (O(log n))
/// </summary>
/// <remarks>
///     Taken and slightly adapted by MutantWafflez for Terraria usage. Original code can be found here: https://github.com/BlueRaja/Weighted-Item-Randomizer-for-C-Sharp (MIT License my beloved)
/// </remarks>
/// <typeparam name="TKey">The type of the objects to choose at random</typeparam>
public class DynamicWeightedRandom<TKey> : ICollection<TKey> where TKey : IComparable<TKey> {
    #region Node class

    private class Node {
        // AA-tree data
        internal int level;
        internal Node left;
        internal Node right;

        //Weighted Randomizer data
        internal TKey key;
        internal double weight;
        internal double subtreeWeight;

        // constructor for the sentinel node
        internal Node() {
            level = 0;
            left = this;
            right = this;
            weight = 0;
            subtreeWeight = 0;
        }

        // constructor for regular nodes (that all start life as leaf nodes)
        internal Node(TKey key, double weight, Node sentinel) {
            level = 1;
            left = sentinel;
            right = sentinel;
            this.key = key;
            this.weight = weight;
            subtreeWeight = weight;
        }
    }

    #endregion

    private readonly Node _sentinel;
    private readonly UnifiedRandom _random;
    private Node _root;
    private Node _deleted;

    public DynamicWeightedRandom(UnifiedRandom random) {
        _root = _sentinel = new Node();
        _deleted = null;
        _random = random;
    }

    #region AA-Tree code

    //This weighted randomizer can be used with any self-balancing tree. I chose an AA-tree written by Aleksey Demakov
    //(with his permission of course), found here:  http://demakov.com/snippets/aatree.html
    //I chose this one because it was easy to understand and well-written.

    private void RotateRight(ref Node node) {
        if (node.level != node.left.level) {
            return;
        }

        //Update the subtreeWeights before moving the nodes
        double oldSubtreeWeight = node.subtreeWeight;
        if (node != _sentinel) {
            node.subtreeWeight = oldSubtreeWeight - node.left.subtreeWeight + node.left.right.subtreeWeight;
        }

        if (node.left != _sentinel) {
            node.left.subtreeWeight = oldSubtreeWeight;
        }

        // rotate right
        Node left = node.left;
        node.left = left.right;
        left.right = node;
        node = left;
    }

    private void RotateLeft(ref Node node) {
        if (node.right.right.level != node.level) {
            return;
        }

        //Update the subtreeWeights before moving the nodes
        double oldSubtreeWeight = node.subtreeWeight;
        if (node != _sentinel) {
            node.subtreeWeight = oldSubtreeWeight - node.right.subtreeWeight + node.right.left.subtreeWeight;
        }

        if (node.right != _sentinel) {
            node.right.subtreeWeight = oldSubtreeWeight;
        }

        // rotate left
        Node right = node.right;
        node.right = right.left;
        right.left = node;
        node = right;
        node.level++;
    }

    private void InsertNode(ref Node node, TKey key, double weight) {
        if (weight < 0) {
            throw new ArgumentOutOfRangeException(nameof(weight), weight, "Cannot add a key with weight < 0!");
        }

        if (key == null) {
            throw new ArgumentNullException(nameof(key), "Cannot add a null key");
        }

        if (node == _sentinel) {
            node = new Node(key, weight, _sentinel);
            UpdateSubtreeWeightsForInsertion(node);
            Count++;
            return;
        }

        int compare = key.CompareTo(node.key);
        switch (compare) {
            case < 0:
                InsertNode(ref node.left, key, weight);
                break;
            case > 0:
                InsertNode(ref node.right, key, weight);
                break;
            default:
                throw new ArgumentException($"Key already exists in {nameof(DynamicWeightedRandom<TKey>)}: {key}");
        }

        RotateRight(ref node);
        RotateLeft(ref node);
    }

    private bool DeleteNode(ref Node node, TKey key) {
        if (node == _sentinel) {
            return _deleted != null;
        }

        int compare = key.CompareTo(node.key);
        if (compare < 0) {
            if (!DeleteNode(ref node.left, key)) {
                return false;
            }
        }
        else {
            if (compare == 0) {
                _deleted = node;
            }

            if (!DeleteNode(ref node.right, key)) {
                return false;
            }
        }

        if (_deleted != null) {
            UpdateSubtreeWeightsForDeletion(_deleted, node);
            _deleted.key = node.key;
            _deleted.weight = node.weight;
            _deleted = null;
            node = node.right;
            Count--;
        }
        else if (node.left.level < node.level - 1
            || node.right.level < node.level - 1) {
            --node.level;
            if (node.right.level > node.level) {
                node.right.level = node.level;
            }

            RotateRight(ref node);
            RotateRight(ref node.right);
            RotateRight(ref node.right.right);
            RotateLeft(ref node);
            RotateLeft(ref node.right);
        }

        return true;
    }

    private Node FindNode(Node node, TKey key) {
        while (node != _sentinel) {
            int compare = key.CompareTo(node.key);
            switch (compare) {
                case < 0:
                    node = node.left;
                    break;
                case > 0:
                    node = node.right;
                    break;
                default:
                    return node;
            }
        }

        return null;
    }

    #endregion

    #region Updating weights

    //When inserting, all the subtree's parent subtrees go up by insertedNode.weight
    //Should be called AFTER the insertion
    private void UpdateSubtreeWeightsForInsertion(Node insertedNode) {
        //Search down the tree for the inserted node, updating the weights as we go
        Node currentNode = _root;
        while (currentNode != insertedNode) {
            currentNode.subtreeWeight += insertedNode.weight;

            int compare = insertedNode.key.CompareTo(currentNode.key);
            currentNode = compare < 0 ? currentNode.left : currentNode.right;
        }
    }

    //When deleting a node, we swap it with its leftmost-descendant in the RIGHT subtree, then remove it.
    //Thus, the weight of deletedNode's subtree (and all parent subtrees) goes down by deletedNode.weight,
    //while all the children subtrees go that contain leftmostRightDescendant go down by leftmostRightDescendant.weight
    //Should be called BEFORE either the swap or deletion
    private void UpdateSubtreeWeightsForDeletion(Node deletedNode, Node leftmostRightDescendant) {
        //Search down the tree for the deletedNode, updating the weights as we go
        Node currentNode = _root;
        while (currentNode != deletedNode) {
            currentNode.subtreeWeight -= deletedNode.weight;

            int compare = deletedNode.key.CompareTo(currentNode.key);
            currentNode = compare < 0 ? currentNode.left : currentNode.right;
        }

        //Right now, currentNode == deletedNode
        currentNode.subtreeWeight -= deletedNode.weight;

        //Deleted node gets replaced by leftmostRightDescendant, so we need to continue searching the subtree for it instead,
        //again updating the nodes' weights as we go
        while (currentNode != leftmostRightDescendant && currentNode != _sentinel) {
            int compare = leftmostRightDescendant.key.CompareTo(currentNode.key);
            currentNode = compare < 0 ? currentNode.left : currentNode.right;

            currentNode.subtreeWeight -= leftmostRightDescendant.weight;
        }

        //At this point, the subtreeWeights have been correctly subtracted from, but the
        //actual node values haven't been swapped yet
    }

    #endregion

    #region ICollection<T> stuff

    /// <summary>
    ///     Returns the number of items currently in the list
    /// </summary>
    public int Count {
        get;
        private set;
    }

    /// <summary>
    ///     Remove all items from the list
    /// </summary>
    public void Clear() {
        _root = _sentinel;
        Count = 0;
    }

    /// <summary>
    ///     Returns false.  Necessary for the ICollection&lt;T&gt; interface.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    ///     Copies the keys to an array, in order
    /// </summary>
    public void CopyTo(TKey[] array, int startingIndex) {
        int currentIndex = startingIndex;
        foreach (TKey key in this) {
            array[currentIndex] = key;
            currentIndex++;
        }
    }

    /// <summary>
    ///     Returns true if the given item has been added to the list; false otherwise
    /// </summary>
    public bool Contains(TKey key) {
        Node node = FindNode(_root, key);
        return node != null && node != _sentinel;
    }

    /// <summary>
    ///     Adds the given item with a default weight of 1
    /// </summary>
    public void Add(TKey key) {
        InsertNode(ref _root, key, 1);
    }

    /// <summary>
    ///     Adds the given item with the given weight.  Higher weights are more likely to be chosen.
    /// </summary>
    public void Add(TKey key, double weight) {
        InsertNode(ref _root, key, weight);
    }

    /// <summary>
    ///     Removes the given item from the list.
    /// </summary>
    /// <returns>Returns true if the item was successfully deleted, or false if it was not found</returns>
    public bool Remove(TKey key) {
        _deleted = null;
        return DeleteNode(ref _root, key);
    }

    #endregion

    #region IEnumerable<T> stuff

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<TKey> GetEnumerator() => InorderTraversal(_root);

    private IEnumerator<TKey> InorderTraversal(Node node) {
        //The obvious way of doing this - calling itself recursively - ends up creating an enormous number
        // of iterators (as many as there are items in the tree).  So, we have to emulate recursion manually >_<
        Stack<Node> stack = new ();
        while (stack.Count != 0 || node != _sentinel) {
            if (node != _sentinel) {
                stack.Push(node);
                node = node.left;
            }
            else {
                node = stack.Pop();
                yield return node.key;
                node = node.right;
            }
        }
    }

    #endregion

    #region IWeightedRandomizer<T> stuff

    /// <summary>
    ///     The total weight of all the items added so far
    /// </summary>
    private double TotalWeight => _root.subtreeWeight;

    /// <summary>
    ///     Returns an item chosen randomly by weight (higher weights are more likely),
    ///     and replaces it so that it can be chosen again
    /// </summary>
    public TKey NextWithReplacement() {
        VerifyHaveItemsToChooseFrom();

        Node currentNode = _root;
        double randomNumber = _random.NextDouble(TotalWeight);
        while (true) {
            if (currentNode.left.subtreeWeight >= randomNumber) {
                currentNode = currentNode.left;
            }
            else {
                randomNumber -= currentNode.left.subtreeWeight;
                if (currentNode.right.subtreeWeight >= randomNumber) {
                    currentNode = currentNode.right;
                }
                else {
                    return currentNode.key;
                }
            }
        }
    }

    /// <summary>
    ///     Returns an item chosen randomly by weight (higher weights are more likely),
    ///     and removes it so it cannot be chosen again
    /// </summary>
    public TKey NextWithRemoval() {
        VerifyHaveItemsToChooseFrom();

        TKey randomKey = NextWithReplacement();
        Remove(randomKey);
        return randomKey;
    }

    /// <summary>
    ///     Throws an exception if the Count or TotalWeight are 0, meaning that are no items to choose from.
    /// </summary>
    private void VerifyHaveItemsToChooseFrom() {
        if (Count <= 0) {
            throw new InvalidOperationException($"There are no items in the {nameof(DynamicWeightedRandom<TKey>)}");
        }

        if (TotalWeight <= 0) {
            throw new InvalidOperationException($"There are no items with positive weight in the {nameof(DynamicWeightedRandom<TKey>)}");
        }
    }

    /// <summary>
    ///     Shortcut syntax to add, remove, and update an item
    /// </summary>
    public double this[TKey key] {
        get => GetWeight(key);
        set => SetWeight(key, value);
    }

    /// <summary>
    ///     Returns the weight of the given item.  Throws an exception if the item is not added
    ///     (use .Contains to check first if unsure)
    /// </summary>
    public double GetWeight(TKey key) {
        if (key == null) {
            throw new ArgumentNullException(nameof(key), "key cannot be null");
        }

        Node node = FindNode(_root, key);
        if (node == null) {
            throw new KeyNotFoundException($"Key not found in {nameof(DynamicWeightedRandom<TKey>)}: {key}");
        }

        return node.weight;
    }

    /// <summary>
    ///     Updates the weight of the given item, or adds it if it has not already been added.
    ///     If weight &lt;= 0, the item is removed.
    /// </summary>
    public void SetWeight(TKey key, double weight) {
        if (weight < 0) {
            throw new ArgumentOutOfRangeException(nameof(weight), weight, "Cannot add a weight with value < 0");
        }

        Node node = FindNode(_root, key);
        if (node == null) {
            Add(key, weight);
        }
        else {
            double weightDelta = weight - node.weight;

            //This is a hack.  The point is to update this node's and all it's ancestors' subtreeWeights.
            //We already have a method that will do that; however, it uses the value of node.weight, rather
            //than a parameter.
            node.weight = weightDelta;
            UpdateSubtreeWeightsForInsertion(node);

            //Finally, set the node.weight to what it should be
            node.weight = weight;
            node.subtreeWeight += weightDelta;
        }
    }

    #endregion
}