using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LivingWorldMod.DataStructures.Classes;

/// <summary>
///     Represents an array of fixed size that will remember only the
///     specified amount of items. Adding an object to this array will
///     replace the oldest object. Objects cannot be directly removed,
///     either.
/// </summary>
public class ForgetfulArray<T> : IList<T> {
    private readonly T[] _array;
    private int _addPointer;
    public int Count => _array.Length;

    public bool IsReadOnly => _array.IsReadOnly;

    public T this[int index] {
        get => _array[index];
        set { }
    }

    public ForgetfulArray(int sizeLimit) {
        _array = new T[sizeLimit];
        _addPointer = 0;
    }

    public IEnumerator<T> GetEnumerator() => (IEnumerator<T>)_array.GetEnumerator();

    public void Add(T item) {
        if (_addPointer >= _array.Length) {
            _addPointer = 0;
        }

        _array[_addPointer++] = item;
    }

    public void Clear() {
        for (int i = 0; i < _array.Length; i++) {
            _array[i] = default(T);
        }

        _addPointer = 0;
    }

    public bool Contains(T item) => _array.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) {
        _array.CopyTo(array, arrayIndex);
    }

    /// <summary>
    ///     Functionally useless. Objects cannot be directly removed
    ///     from this array.
    /// </summary>
    public bool Remove(T item) => false;

    public int IndexOf(T item) {
        if (item is null) {
            return -1;
        }

        for (int i = 0; i < _array.Length; i++) {
            if (_array[i].Equals(item)) {
                return i;
            }
        }

        return -1;
    }

    public void Insert(int index, T item) {
        Add(item);
    }

    /// <summary>
    ///     Functionally useless. Objects cannot be directly removed
    ///     from this array.
    /// </summary>
    public void RemoveAt(int index) { }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}