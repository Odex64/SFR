using SFD;
using SFD.Weapons;
using SFDGameScriptInterface;
using SFR.Fighter;

namespace SFR.Helper;

/// <summary>
///     Class that contains extension methods regarding players.
/// </summary>
internal static class Fighter
{
    // internal static Team GetActualTeam(this Player player)
    // {
    //     if (!player.IsBot)
    //     {
    //         var user = player.GetGameUser();
    //         if (user != null && GadgetHandler.DevIcon.Account == user.Account)
    //         {
    //             return GadgetHandler.DevIcon.Team;
    //         }
    //     }
    //
    //     return player.CurrentTeam;
    // }

    /// <summary>
    ///     Helper method to return the current weapon in use
    /// </summary>
    /// <param name="player">A player</param>
    /// <returns>The current weapon in use</returns>
    internal static object GetCurrentWeapon(this Player player)
    {
        return player.CurrentWeaponDrawn switch
        {
            SFD.Weapons.WeaponItemType.Melee => player.GetCurrentMeleeWeaponInUse(),
            SFD.Weapons.WeaponItemType.Handgun or SFD.Weapons.WeaponItemType.Rifle => player.GetCurrentRangedWeaponInUse(),
            SFD.Weapons.WeaponItemType.Thrown => player.GetCurrentThrownWeaponInUse(),
            _ => null
        };
    }

    internal static ExtendedPlayer GetExtension(this Player player)
    {
        if (ExtendedPlayer.ExtendedPlayers.TryGetValue(player, out var existingExtendedPlayer))
        {
            return existingExtendedPlayer;
        }

        var extendedPlayer = new ExtendedPlayer(player);
        ExtendedPlayer.ExtendedPlayers.Add(player, extendedPlayer);
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