namespace LivingWorldMod.DataStructures.Records;

/// <summary>
///     Record that represents a "Gradient" of some type, where some kind of interpolation function is used to determine the output at a given "time". Common usage/example is with colors, where
///     interpolation is used to determine a color that is "between" two other colors.
/// </summary>
public readonly record struct Gradient<T>(Gradient<T>.InterpolationDelegate InterpolationFunction, params (float, T)[] GradientPoints) {
    public delegate T InterpolationDelegate(T valueOne, T valueTwo, float time);

    /// <summary>
    ///     Returns the interpolated result of the provided gradient points with the passed in time value.
    /// </summary>
    public T GetValue(float timeValue) {
        int leftIndex = 0;
        int rightIndex = 1;
        while (GradientPoints[leftIndex].Item1 > timeValue || GradientPoints[rightIndex].Item1 < timeValue) {
            leftIndex++;
            rightIndex++;
        }

        (float leftTime, T leftValue) = GradientPoints[leftIndex];
        (float rightTime, T rightValue) = GradientPoints[rightIndex];
        return InterpolationFunction(leftValue, rightValue, (timeValue - leftTime) / (rightTime - leftTime));
    }
}