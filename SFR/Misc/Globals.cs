using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SFR.Misc;

internal static class Globals
{
    internal const string SFRVersion = "v.1.0.5_dev";
    internal const string VanillaVersion = "v.1.3.7d";
    internal const int Build = 0;
    internal static readonly string ServerVersion = SFRVersion.Replace("v.1", "v.2");
    internal static readonly Random Random = new();
    internal static int? Slots = null;
    internal static readonly Color RageBoost = new(210, 130, 50);
    internal static bool QuickStart = false;
    internal static string DebugMap = string.Empty;
    internal static readonly Color Team5 = new(112, 59, 168);
    internal static readonly Color Team6 = new(0, 121, 137);
    internal static Texture2D TeamIcon5;
    internal static Texture2D TeamIcon6;
    internal static bool IsDev => SFRVersion.EndsWith("_dev");
}