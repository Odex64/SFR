using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using SFD.Tiles;

namespace SFR.Fighter;

/// <summary>
///     Official team members will have a special icon in-game.
/// </summary>
internal static class NameIconHandler
{
    private static readonly Dictionary<string, string> DeveloperIcons = new()
    {
        { "913199347", "Odex" }, // Odex (Started the project)
        { "962495701", "Dxse" }, // Dxse (Tiles)
        { "319194249", "Motto73" }, // Motto (Weapons, Gore, idk B))
        { "928249560", string.Empty }, // NearHuscarl (Editors)
        { "887205100", "Danila015" }, // Danila015 (Helps in spirit)
        { "836312603", "DK" }, // Dark (Freeloader)
        { "390932643", string.Empty }, // Eiga (Maps, balance)
        { "169254469", "Chickibo" }, // Chickibo (Music)
        { "156328261", "Samwow" }, // Samwow (Music)
        { "340339546", "Vixfor" }, // Shock (Colors, tiles)
        { "310827315", "Mimyuu" }, // Mimyuu (Font)
        { "913149671", "Casey_Price" }, // Casey Price (Artist)
        { "457000463", "Clown" }, // Clown (Artist)
        { "1162748893", string.Empty }, // Flames (Additional code)
        { "174151764", "Heli0s" }, // Heli0s (QA)
        { "354191981", "Pricey" }, // Pricey (QA)
        { "452459892", "GoreDem" }, // GoreDem (QA)
        { "131803071", "Emmett" }, // Emmett Brown (QA)
        { "106015973", "Relgap" }, // Relgap (QA)
        { "463609548", "Scouty" }, // Scouty (Maps)
        { "201718776", "Heapons" }, // Heapons (Mod)
        { "294075097", string.Empty } // Argon (Coder)
    };

    internal static bool IsDeveloper(string accountID) => DeveloperIcons.ContainsKey(accountID.Substring(1));

    internal static Texture2D GetDeveloperIcon(string accountID)
    {
        if (!IsDeveloper(accountID))
        {
            return null;
        }

        // User accounts start with S. Remove it before checking it's a dev.
        string iconName = DeveloperIcons[accountID.Substring(1)];
        return Textures.GetTexture(iconName == string.Empty ? "developer" : iconName);
    }
}