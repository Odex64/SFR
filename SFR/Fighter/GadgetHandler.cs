using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Objects;

namespace SFR.Fighter;

/// <summary>
///     Here we handle all the HUD or visual effects regarding players, such as dev icons.
/// </summary>
[HarmonyPatch]
internal static class GadgetHandler
{
    private static DevIcon _devIcon;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectStreetsweeper), nameof(ObjectStreetsweeper.GetOwnerTeam))]
    private static bool FixDroneTeam(ref Team __result)
    {
        __result = _devIcon.Team;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.GetTeamIcon))]
    private static bool DrawDevIcon(Team team, ref Texture2D __result)
    {
        int num = (int)team;
        if (num == -1 && _devIcon.Account != null)
        {
            __result = NameIconHandler.GetDeveloperIcon(_devIcon.Account);
            return false;
        }

        return true;
    }

    internal static Team GetActualTeam(this Player player)
    {
        if (!player.IsBot)
        {
            var user = player.GetGameUser();
            if (user != null && _devIcon.Account == user.Account)
            {
                return _devIcon.Team;
            }
        }

        return player.CurrentTeam;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.DrawPlates))]
    private static bool DrawPlates(Player __instance)
    {
        if (__instance is { IsBot: false, IsDead: false })
        {
            var user = __instance.GetGameUser();
            if (user != null && NameIconHandler.IsDeveloper(user.Account))
            {
                if (_devIcon.Account == null)
                {
                    _devIcon = new DevIcon(__instance.CurrentTeam, user.Account);
                }
                else if (_devIcon.Account != user.Account)
                {
                    _devIcon.Account = user.Account;
                }
                else if (__instance.CurrentTeam >= 0)
                {
                    _devIcon.Team = __instance.CurrentTeam;
                }

                __instance.m_currentTeam = (Team)(-1);
            }
        }

        return true;
    }
}

internal struct DevIcon
{
    internal Team Team;
    internal string Account;

    internal DevIcon(Team team, string account)
    {
        Team = team;
        Account = account;
    }
}