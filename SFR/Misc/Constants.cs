using System;
using Microsoft.Xna.Framework;

namespace SFR.Misc;

internal static class Constants
{
    internal const string SFRVersion = "v.1.0.4_dev";
    internal const int Build = 0;
    internal static readonly string ServerVersion = SFRVersion.Replace("v.1", "v.2");
    internal static readonly Random Random = new();
    internal static int? Slots = null;
    internal static Color RageBoost = new(210, 130, 50);
    internal static bool FastStart = false;
    internal static string DebugMap = string.Empty;

    internal static bool IsDev()
    {
        return SFRVersion.EndsWith("_dev");
    }
}