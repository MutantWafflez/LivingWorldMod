using System.Numerics;

namespace LivingWorldMod.DataStructures.Records;

/// <summary>
///     Wrapper struct holding a value that has an INCLUSIVE upper and lower bound. Whenever this value is updated, it will be rounded to the lower/upper bound if necessary.
/// </summary>
public readonly record struct BoundedNumber<T>(T Value, T LowerBound, T UpperBound) where T : INumber<T> {
    public T Value {
        get;
    } = Utils.Clamp(Value, LowerBound, UpperBound);

    public T LowerBound {
        get;
    } = LowerBound;

    public T UpperBound {
        get;
    } = UpperBound;

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

    public BoundedNumber<T> ResetToBound(bool lowerBound) => new (lowerBound ? LowerBound : UpperBound, LowerBound, UpperBound);
}