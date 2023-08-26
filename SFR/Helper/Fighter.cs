using SFD;
using SFDGameScriptInterface;
using SFR.Fighter;
using WeaponItemType = SFD.Weapons.WeaponItemType;

namespace SFR.Helper;

/// <summary>
///     Class that contains extension methods regarding players.
/// </summary>
internal static class Fighter
{
    /// <summary>
    ///     Helper method to return the current weapon in use
    /// </summary>
    /// <param name="player">A player</param>
    /// <returns>The current weapon in use</returns>
    internal static object GetCurrentWeapon(this Player player)
    {
        return player.CurrentWeaponDrawn switch
        {
            WeaponItemType.Melee => player.GetCurrentMeleeWeaponInUse(),
            WeaponItemType.Handgun or WeaponItemType.Rifle => player.GetCurrentRangedWeaponInUse(),
            WeaponItemType.Thrown => player.GetCurrentThrownWeaponInUse(),
            _ => null
        };
    }

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
}