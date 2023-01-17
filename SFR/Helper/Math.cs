namespace SFR.Helper;

internal static class Math
{
    internal static float Lerp(float a, float b, float f) => a * (1.0f - f) + b * f;

    internal static float InverseLerp(float a, float b, float f) => (f - a) / (b - a);

    internal static float Clamp(float a)
    {
        switch (a)
        {
            case < 0:
                return 0;
            case > 1:
                return 1;
            default:
                return a;
        }
    }
}