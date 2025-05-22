using System;
using Microsoft.Xna.Framework;

namespace SFR.Misc;

internal static class Globals
{
    internal const string SFRVersion = "v.1.0.5_dev";
    internal const int Build = 0;
    internal static readonly Random Random = new();
    internal static readonly Color RageBoost = new(210, 130, 50);
    internal static bool QuickStart = false;
    internal static string DebugMap = string.Empty;
    internal static bool IsDev => SFRVersion.EndsWith("_dev");
}