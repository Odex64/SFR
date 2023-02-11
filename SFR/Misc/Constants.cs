using System;

namespace SFR.Misc;

public static class Constants
{
    internal const string SFRVersion = "v.1.0.3c_dev";
    internal const int Build = 0;
    internal static readonly string ServerVersion = SFRVersion.Replace("v.1", "v.2");
    internal static readonly Random Random = new();
    internal static int? Slots = null;
}