namespace SFR.Helper;

internal static class ExtendedMath
{
    internal static float Lerp(float a, float b, float f) => a * (1.0f - f) + b * f;

    internal static float InverseLerp(float a, float b, float f) => (f - a) / (b - a);

    internal static float Clamp(float a)
    {
        return a switch
        {
            < 0 => 0,
            > 1 => 1,
            _ => a
        };
    }
}