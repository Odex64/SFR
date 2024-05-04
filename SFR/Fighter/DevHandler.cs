using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using SFD.Tiles;

namespace SFR.Fighter;

/// <summary>
/// Official team members will have a special icon in-game.
/// </summary>
internal static class DevHandler
{
    private static readonly Dictionary<string, string> _developerIcons = new()
    {
        { "913199347", "Odex" }, // Odex
        { "962495701", "Dxse" }, // Dxse
        { "319194249", "Motto73" }, // Motto
        { "887205100", "Danila015" }, // Danila01
        { "390932643", "Eiga" }, // Eiga
        { "156328261", "Samwow" }, // Samwow
        { "340339546", "Vixfor" }, // Shock
        { "310827315", "Mimyuu" }, // Mimyuu
        { "294075097", string.Empty }, // Argon
        { "457000463", "KLI" } // KLI
    };

    internal static bool IsDeveloper(string accountId) => accountId.Length > 1 && _developerIcons.ContainsKey(accountId.Substring(1));

    internal static Texture2D GetDeveloperIcon(string accountId)
    {
        if (!IsDeveloper(accountId))
        {
            return null;
        }

        // User accounts start with S. Remove it before checking it's a dev.
        string iconName = _developerIcons[accountId.Substring(1)];
        return Textures.GetTexture(iconName == string.Empty ? "developer" : iconName);
    }
}