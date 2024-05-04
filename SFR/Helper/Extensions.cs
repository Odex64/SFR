using System;
using SFD;
using SFDGameScriptInterface;
using SFR.Fighter;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using WeaponItemType = SFD.Weapons.WeaponItemType;

namespace SFR.Helper;

internal static class Extensions
{
    /// <summary>
    /// Helper method to return the current weapon in use
    /// </summary>
    /// <param name="player">A player</param>
    /// <returns>The current weapon in use</returns>
    internal static object GetCurrentWeapon(this Player player) => player.CurrentWeaponDrawn switch
    {
        WeaponItemType.Melee => player.GetCurrentMeleeWeaponInUse(),
        WeaponItemType.Handgun or WeaponItemType.Rifle => player.GetCurrentRangedWeaponInUse(),
        WeaponItemType.Thrown => player.GetCurrentThrownWeaponInUse(),
        _ => null
    };

    internal static ExtendedPlayer GetExtension(this Player player)
    {
        if (ExtendedPlayer.ExtendedPlayersTable.TryGetValue(player, out var existingExtendedPlayer))
        {
            return existingExtendedPlayer;
        }

        var extendedPlayer = new ExtendedPlayer(player);
        ExtendedPlayer.ExtendedPlayersTable.Add(player, extendedPlayer);
        return extendedPlayer;
    }

    internal static ExtendedModifiers GetExtension(this PlayerModifiers modifiers)
    {
        if (ExtendedModifiers.ExtendedModifiersTable.TryGetValue(modifiers, out var existingExtendedModifiers))
        {
            return existingExtendedModifiers;
        }

        var extendedModifiers = new ExtendedModifiers(modifiers);
        ExtendedModifiers.ExtendedModifiersTable.Add(modifiers, extendedModifiers);
        return extendedModifiers;
    }

    internal static float NextFloat(this Random random, float minimum, float maximum) => (float)NextDouble(random, minimum, maximum);

    internal static double NextDouble(this Random random, double minimum, double maximum) => random.NextDouble() * (maximum - minimum) + minimum;

    internal static float NextFloat(this Random random) => (float)random.NextDouble();

    internal static bool NextBool(this Random random) => random.NextFloat() > 0.5f;

    internal static Vector2 GetRotatedVector(this Vector2 vector, double angle)
    {
        double cosAngle = Math.Cos(angle);
        double sinAngle = Math.Sin(angle);
        return new((float)(cosAngle * vector.X - sinAngle * vector.Y), (float)(sinAngle * vector.X + cosAngle * vector.Y));
    }

    internal static float GetAngle(this Vector2 vector) => (float)Math.Atan2(vector.Y, vector.X);

    internal static float GetRotatedAngle(this Vector2 vector) => (float)Math.Atan2(vector.Y, vector.X) - (float)Math.PI / 2;
}