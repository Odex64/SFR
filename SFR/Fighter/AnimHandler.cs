using System.Collections.Generic;
using HarmonyLib;
using SFD;
using SFD.MenuControls;
using SFD.Weapons;
using SFR.Helper;
using SFR.Weapons.Others;
using Constants = SFR.Misc.Constants;

namespace SFR.Fighter;

/// <summary>
///     Here we load or programmatically create custom animations to be used in-game.
/// </summary>
[HarmonyPatch]
internal static class AnimHandler
{
    private static List<AnimationData> _animations;

    internal static List<AnimationData> GetAnimations(AnimationsData data)
    {
        _animations ??= new List<AnimationData>
        {
            ChangeAnimationTime(data.GetAnimation("UpperMelee2H1"), 1.75f, "UpperMelee2H1VerySlow"),
            ChangeAnimationTime(data.GetAnimation("UpperMelee2H2"), 1.75f, "UpperMelee2H2VerySlow"),
            ChangeAnimationTime(data.GetAnimation("UpperMelee2H3"), 1.75f, "UpperMelee2H3VerySlow"),
            ChangeAnimationTime(data.GetAnimation("FullJumpAttackMelee"), 3f, "FullJumpAttackMeleeVerySlow"),
            ChangeAnimationTime(data.GetAnimation("UpperBlockMelee2H"), 1.5f, "UpperBlockMelee2HVerySlow"),
            ChangeAnimationTime(data.GetAnimation("UpperMelee2H1"), 1.25f, "UpperMelee2H1Slow"),
            ChangeAnimationTime(data.GetAnimation("UpperMelee2H2"), 1.25f, "UpperMelee2H2Slow"),
            ChangeAnimationTime(data.GetAnimation("UpperMelee2H3"), 1.25f, "UpperMelee2H3Slow"),
            ChangeAnimationTime(data.GetAnimation("FullJumpAttackMelee"), 2f, "FullJumpAttackMeleeSlow"),
            ChangeAnimationTime(data.GetAnimation("UpperBlockMelee2H"), 1.2f, "UpperBlockMelee2HSlow"),
            ChangeAnimationTime(data.GetAnimation("UpperMelee1H1"), 0.75f, "UpperMelee1H1Fast"),
            ChangeAnimationTime(data.GetAnimation("UpperMelee1H2"), 0.75f, "UpperMelee1H2Fast"),
            ChangeAnimationTime(data.GetAnimation("UpperMelee1H3"), 0.75f, "UpperMelee1H3Fast"),
            ChangeAnimationTime(data.GetAnimation("UpperBlockMelee"), 0.5f, "UpperBlockMeleeFast")
        };

        return _animations;
    }

    private static AnimationData ChangeAnimationTime(AnimationData data, float newTime, string newName)
    {
        var frames = data.Frames;
        var frameData = new AnimationFrameData[frames.Length];
        for (int i = 0; i < frameData.Length; i++)
        {
            var newCollisions = new AnimationCollisionData[frames[i].Collisions.Length];
            for (int j = 0; j < frames[i].Collisions.Length; j++)
            {
                newCollisions[j] = frames[i].Collisions[j];
            }

            var newParts = new AnimationPartData[frames[i].Parts.Length];
            foreach (var x in frames[i].Parts)
            {
                newParts[i] = new AnimationPartData(x.LocalId, x.X, x.Y, x.Rotation, x.Flip, x.Scale.X, x.Scale.Y, x.PostFix);
            }

            AnimationFrameData newFrame = new(frames[i].Parts, frames[i].Collisions, frames[i].Event, (int)(frames[i].Time * newTime))
            {
                IsRecoil = frames[i].IsRecoil
            };
            frameData[i] = newFrame;
        }

        return new AnimationData(frameData, newName);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.GetAnimWalkUpper))]
    private static bool WalkAnimation(Player __instance, ref string __result)
    {
        string animUpperOverride = __instance.GetAnimUpperOverride("UpperWalk", out bool flag);
        if (flag)
        {
            __result = animUpperOverride;
            return false;
        }

        var profile = __instance.GetProfile().ToSFDProfile();
        if (profile.Skin.Name.Contains("Zombie"))
        {
            __result = "UpperWalkZombie";
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.GetAnimIdleUpper))]
    private static bool IdleAnimation(Player __instance, ref string __result)
    {
        string animUpperOverride = __instance.GetAnimUpperOverride("UpperIdle", out bool flag);
        if (flag)
        {
            __result = animUpperOverride;
            return false;
        }

        var profile = __instance.GetProfile().ToSFDProfile();
        if (profile.Skin.Name.Contains("Zombie"))
        {
            __result = "UpperIdleZombie";
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerUpperUseSyringeAnimation), nameof(PlayerUpperUseSyringeAnimation.OverrideUpperAnimationEnterFrame))]
    private static bool OnSyringeAnimation(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim, PlayerUpperUseSyringeAnimation __instance)
    {
        if (__instance.CheckAbort(player))
        {
            return false;
        }

        if (!__instance.m_useFramePlayed && subAnim.GetCurrentFrameIndex() >= 3)
        {
            __instance.m_useFramePlayed = true;
            switch (player.CurrentPowerupItem)
            {
                case WpnStrengthBoost strengthBoost:
                    strengthBoost.OnEffectStart(player);
                    break;
                case WpnSpeedBoost speedBoost:
                    speedBoost.OnEffectStart(player);
                    break;
                case AdrenalineBoost rageBoost:
                    rageBoost.OnEffectStart(player);
                    break;
            }
        }

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ProfileGridItem), nameof(ProfileGridItem.RefreshPreviewPlayer))]
    private static void RefreshAnimationMenu(ProfileGridItem __instance)
    {
        if (__instance.Profile != null)
        {
            bool walkingAnimation = Constants.Random.NextBool();
            __instance.m_previewPlayer.SetAnimation(walkingAnimation ? Animation.Walk : Animation.Idle, PlayerAction.Disabled);
        }
    }
}