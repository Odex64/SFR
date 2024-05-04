using HarmonyLib;
using Microsoft.Xna.Framework;
using SFD;
using SFR.Helper;
using SFR.Misc;
using Player = SFD.Player;

namespace SFR.Fighter;

/// <summary>
/// Here we handle all the HUD or visual effects regarding players, such as dev icons.
/// </summary>
[HarmonyPatch]
internal static class GadgetHandler
{
    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(Player), nameof(Player.DrawPlates))]
    // private static bool DrawExtraInfo(float ms, Player __instance)
    // {
    //     var vector = Camera.ConvertWorldToScreen(__instance.Position + new Vector2(0f, 24f));
    //     float num = MathHelper.Max(Camera.Zoom * 0.4f, 1f);
    //
    //     NameIconHandler.Draw(__instance, vector, num);
    //
    //     // Handle message icons.
    //     if (__instance is { IsDead: false, IsRemoved: false, ChatActive: true })
    //     {
    //         if (__instance.m_chatIconTimer > 250f)
    //         {
    //             __instance.m_chatIconFrame = (__instance.m_chatIconFrame + 1) % 4;
    //             __instance.m_chatIconTimer -= 250f;
    //         }
    //         else
    //         {
    //             __instance.m_chatIconTimer += ms;
    //         }
    //
    //         __instance.m_spriteBatch.Draw(Constants.ChatIcon,
    //             new(vector.X + __instance.m_nameTextSize.X * 0.25f * num, vector.Y - __instance.m_nameTextSize.Y * num),
    //             new Rectangle(1 + __instance.m_chatIconFrame * 13, 1, 12, 12), ColorCorrection.FromXNAToCustom(Constants.COLORS.CHAT_ICON), 0f, Vector2.Zero,
    //             num, SpriteEffects.None, 1f);
    //     }
    //
    //     StatusBarHandler.Draw(__instance, vector, num);
    //
    //     return false;
    // }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.DrawColor), MethodType.Getter)]
    private static bool CustomDrawColor(Player __instance, ref Color __result)
    {
        var extendedPlayer = __instance.GetExtension();

        if (extendedPlayer.AdrenalineBoost)
        {
            __result = ColorCorrection.CreateCustom(Globals.RageBoost);
            return false;
        }

        return true;
    }
}