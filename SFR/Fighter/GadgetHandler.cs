using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFR.Helper;

namespace SFR.Fighter;

/// <summary>
///     Here we handle all the HUD or visual effects regarding players, such as dev icons.
/// </summary>
[HarmonyPatch]
internal static class GadgetHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.DrawPlates))]
    private static bool DrawExtraInfo(float ms, Player __instance)
    {
        var vector = Camera.ConvertWorldToScreen(__instance.Position + new Vector2(0f, 24f));
        float num = MathHelper.Max(Camera.Zoom * 0.4f, 1f);

        NameIconHandler.Draw(__instance, vector, num);

        // Handle message icons.
        if (__instance is { IsDead: false, IsRemoved: false, ChatActive: true })
        {
            if (__instance.m_chatIconTimer > 250f)
            {
                __instance.m_chatIconFrame = (__instance.m_chatIconFrame + 1) % 4;
                __instance.m_chatIconTimer -= 250f;
            }
            else
            {
                __instance.m_chatIconTimer += ms;
            }

            __instance.m_spriteBatch.Draw(Constants.ChatIcon,
                new Vector2(vector.X + __instance.m_nameTextSize.X * 0.25f * num, vector.Y - __instance.m_nameTextSize.Y * num),
                new Rectangle(1 + __instance.m_chatIconFrame * 13, 1, 12, 12), ColorCorrection.FromXNAToCustom(Constants.COLORS.CHAT_ICON), 0f, Vector2.Zero,
                num, SpriteEffects.None, 1f);
        }

        StatusBarHandler.Draw(__instance, vector, num);

        return false;
    }


    // internal static DevIcon DevIcon;

    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(ObjectStreetsweeper), nameof(ObjectStreetsweeper.GetOwnerTeam))]
    // private static bool FixDroneTeam(ref Team __result)
    // {
    //     __result = DevIcon.Team;
    //     return false;
    // }

    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(Constants), nameof(Constants.GetTeamIcon))]
    // private static bool DrawDevIcon(Team team, ref Texture2D __result)
    // {
    //     int num = (int)team;
    //     if (num == -1 && DevIcon.Account != null)
    //     {
    //         __result = NameIconHandler.GetDeveloperIcon(DevIcon.Account);
    //         return false;
    //     }
    //
    //     return true;
    // }
    //
    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(Player), nameof(Player.DrawPlates))]
    // private static bool DrawPlates(Player __instance)
    // {
    //     if (__instance is { IsBot: false, IsDead: false })
    //     {
    //         var user = __instance.GetGameUser();
    //         if (user != null && NameIconHandler.IsDeveloper(user.Account))
    //         {
    //             if (DevIcon.Account == null)
    //             {
    //                 DevIcon = new DevIcon(__instance.CurrentTeam, user.Account);
    //             }
    //             else if (DevIcon.Account != user.Account)
    //             {
    //                 DevIcon.Account = user.Account;
    //             }
    //             else if (__instance.CurrentTeam >= 0)
    //             {
    //                 DevIcon.Team = __instance.CurrentTeam;
    //             }
    //
    //             __instance.m_currentTeam = (Team)(-1);
    //         }
    //     }
    //
    //     return true;
    // }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(Player), nameof(Player.DrawPlates))]
    // private static void AfterDrawPlates(Player __instance)
    // {
    //     if (DevIcon.Account != null && __instance is { IsBot: false, IsDead: false })
    //     {
    //         var user = __instance.GetGameUser();
    //         if (user != null && NameIconHandler.IsDeveloper(user.Account))
    //         {
    //             __instance.m_currentTeam = DevIcon.Team;
    //         }
    //     }
    // }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.DrawColor), MethodType.Getter)]
    private static bool CustomDrawColor(Player __instance, ref Color __result)
    {
        var extendedPlayer = __instance.GetExtension();

        if (extendedPlayer.RageBoost)
        {
            __result = ColorCorrection.CreateCustom(Misc.Constants.RageBoost);
            return false;
        }

        return true;
    }
}