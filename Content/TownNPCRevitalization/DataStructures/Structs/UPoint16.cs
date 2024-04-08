using System;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;

/// <summary>
/// Unsigned version of <see cref="Terraria.DataStructures.Point16"/>
/// </summary>
public struct UPoint16(ushort x, ushort y) {
    public ushort x = x;
    public ushort y = y;

    public UPoint16(int x, int y) : this((ushort)x, (ushort)y) { }

    public UPoint16(Point16 point) : this(point.X, point.Y) { }

    public UPoint16(Point point) : this(point.X, point.Y) { }

    public static UPoint16 operator +(UPoint16 pointOne, UPoint16 pointTwo) => new((ushort)(pointOne.x + pointTwo.x), (ushort)(pointOne.y + pointTwo.x));

    public static bool operator ==(UPoint16 pointOne, UPoint16 pointTwo) => pointOne.Equals(pointTwo);

    public static bool operator !=(UPoint16 pointOne, UPoint16 pointTwo) => !(pointOne == pointTwo);

    public readonly Point16 Point16 => new(x, y);

    public readonly bool Equals(UPoint16 other) => x == other.x && y == other.y;

    public override readonly bool Equals(object obj) => obj is UPoint16 other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(x, y);
}