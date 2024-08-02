using System.Numerics;

namespace LivingWorldMod.DataStructures.Structs;

/// <summary>
///     Wrapper struct holding a value that has an INCLUSIVE upper and lower bound. Whenever this value is updated, it will be rounded to the lower/upper bound if necessary.
/// </summary>
public readonly struct BoundedNumber<T>(T value, T lowerBound, T upperBound) where T : INumber<T> {
    public T Value {
        get;
    } = Utils.Clamp(value, lowerBound, upperBound);

    public T LowerBound {
        get;
    } = lowerBound;

    public T UpperBound {
        get;
    } = upperBound;

    private BoundedNumber(T value, BoundedNumber<T> oldNumber) : this(value, oldNumber.LowerBound, oldNumber.UpperBound) { }

    public static implicit operator T (BoundedNumber<T> number) => number.Value;

    public static BoundedNumber<T> operator +(BoundedNumber<T> baseNumber, T operand) => new ((T)baseNumber + operand, baseNumber);

    public static BoundedNumber<T> operator -(BoundedNumber<T> baseNumber, T operand) => new ((T)baseNumber - operand, baseNumber);

    public static BoundedNumber<T> operator *(BoundedNumber<T> baseNumber, T operand) => new ((T)baseNumber * operand, baseNumber);

    public static BoundedNumber<T> operator /(BoundedNumber<T> baseNumber, T operand) => new ((T)baseNumber / operand, baseNumber);

    public static BoundedNumber<T> operator %(BoundedNumber<T> baseNumber, T operand) => new ((T)baseNumber % operand, baseNumber);

    public static bool operator ==(BoundedNumber<T> baseNumber, T operand) => baseNumber.Value == operand;

    public static bool operator !=(BoundedNumber<T> baseNumber, T operand) => !(baseNumber == operand);

    public static bool operator >(BoundedNumber<T> baseNumber, T operand) => baseNumber.Value > operand;

    public static bool operator <(BoundedNumber<T> baseNumber, T operand) => baseNumber.Value < operand;

    public static bool operator >=(BoundedNumber<T> baseNumber, T operand) => baseNumber.Value >= operand;

    public static bool operator <=(BoundedNumber<T> baseNumber, T operand) => baseNumber.Value <= operand;
}