using System;

namespace SFR.Helper;

internal static class Randomizer
{
    internal static float NextFloat(this Random random, float minimum, float maximum) => (float)NextDouble(random, minimum, maximum);

    internal static double NextDouble(this Random random, double minimum, double maximum) => random.NextDouble() * (maximum - minimum) + minimum;

    internal static float NextFloat(this Random random) => (float)random.NextDouble();

    internal static bool NextBool(this Random random) => random.NextFloat() > 0.5f;
}