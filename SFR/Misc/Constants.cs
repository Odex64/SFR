using System;

namespace SFR.Misc;

public static class Constants
{
    public const string SFRVersion = "v.1.0.3c_dev";
    internal static readonly string ServerVersion = SFRVersion.Replace("v.1", "v.2");
    internal static readonly Random Random = new();
    internal static int? Slots = null;
}