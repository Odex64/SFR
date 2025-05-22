using System.Collections.Generic;
using HarmonyLib;
using SFD;
using SFD.MenuControls;
using SFD.States;
using SFD.Weapons;
using SFDGameScriptInterface;
using SFR.Helper;
using SFR.Misc;
using SFR.Weapons.Others;
using Math = System.Math;
using Player = SFD.Player;
using WeaponItemType = SFD.Weapons.WeaponItemType;

namespace SFR.Fighter;

/// <summary>
/// Here we load or programmatically create custom animations to be used in-game.
/// </summary>
[HarmonyPatch]
internal static class AnimHandler
{
    private static List<AnimationData> _customAnimations;

    internal static List<AnimationData> GetAnimations(AnimationsData data)
    {
        _customAnimations ??=
        [
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
        ];

        return _customAnimations;
    }

    private static AnimationData ChangeAnimationTime(AnimationData data, float newTime, string newName)
    {
        AnimationFrameData[] frames = data.Frames;
        AnimationFrameData[] frameData = new AnimationFrameData[frames.Length];
        for (int i = 0; i < frameData.Length; i++)
        {
            AnimationCollisionData[] newCollisions = new AnimationCollisionData[frames[i].Collisions.Length];
            for (int j = 0; j < frames[i].Collisions.Length; j++)
            {
                newCollisions[j] = frames[i].Collisions[j];
            }

            AnimationPartData[] newParts = new AnimationPartData[frames[i].Parts.Length];
            foreach (AnimationPartData x in frames[i].Parts)
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
        if (GameSFD.Handle.CurrentState == State.MainMenu)
        {
            return true;
        }

        string animUpperOverride = __instance.GetAnimUpperOverride("UpperWalk", out bool flag);
        if (flag)
        {
            __result = animUpperOverride;
            return false;
        }

        IProfile profile = __instance.GetProfile().ToSFDProfile();
        if (profile.Skin.Name.Contains("Zombie") && (__instance.CurrentMeleeWeapon is null || __instance.CurrentMeleeWeapon.Properties.WeaponID != 59)) // Add chainsaw support
        {
            switch (__instance.Equipment.WeaponDrawn)
            {
                case WeaponItemType.NONE:
                    __result = "UpperWalkZombie";
                    return false;
                case WeaponItemType.Handgun:
                    __result = "UpperWalkZombieHandgun";
                    return false;

                case WeaponItemType.Rifle:
                    __result = "UpperWalkZombieRifle";
                    return false;

                case WeaponItemType.Thrown:
                    __result = __instance.CurrentThrownWeapon.Visuals.AnimWalkUpper;
                    return false;

                case WeaponItemType.Melee:
                    __result = __instance.CurrentVisualMeleeWeapon.Visuals.AnimWalkUpper.Insert(__instance.CurrentVisualMeleeWeapon.Visuals.AnimWalkUpper.IndexOf("Walk") + 4, "Zombie");
                    return false;
            }

            __result = "UpperWalkZombie";
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.GetAnimIdleUpper))]
    private static bool IdleAnimation(Player __instance, ref string __result)
    {
        if (GameSFD.Handle.CurrentState == State.MainMenu)
        {
            return true;
        }

        string animUpperOverride = __instance.GetAnimUpperOverride("UpperIdle", out bool flag);
        if (flag)
        {
            __result = animUpperOverride;
            return false;
        }

        IProfile profile = __instance.GetProfile().ToSFDProfile();
        if (!__instance.InThrowingMode && (__instance.CurrentMeleeWeapon is null || __instance.CurrentMeleeWeapon.Properties.WeaponID != 59)) // Add chainsaw support
        {
            if (profile.Skin.Name.Contains("Zombie") && !__instance.Crouching && !__instance.TakingCover)
            {
                switch (__instance.Equipment.WeaponDrawn)
                {
                    case WeaponItemType.NONE:
                        __result = "UpperIdleZombie";
                        return false;
                    case WeaponItemType.Handgun:
                        __result = "UpperIdleZombieHandgun";
                        return false;

                    case WeaponItemType.Rifle:
                        __result = "UpperIdleZombieRifle";
                        return false;

                    case WeaponItemType.Thrown:
                        __result = __instance.CurrentThrownWeapon.Visuals.AnimIdleUpper;
                        return false;

                    case WeaponItemType.Melee:
                        __result = __instance.CurrentVisualMeleeWeapon.Visuals.AnimIdleUpper.Insert(__instance.CurrentVisualMeleeWeapon.Visuals.AnimIdleUpper.IndexOf("Idle") + 4, "Zombie");
                        return false;
                }

                __result = "UpperIdleZombie";
                return false;
            }
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerUpperUseSyringeAnimation), nameof(PlayerUpperUseSyringeAnimation.OverrideUpperAnimationEnterFrame))]
    private static bool OnSyringeAnimation(Player player, SubAnimationPlayer subAnim, PlayerUpperUseSyringeAnimation __instance)
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
                case AdrenalineBoost adrenalineBoost:
                    adrenalineBoost.OnEffectStart(player);
                    break;
            }
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateAnimation))]
    private static bool UpdateAnimation(Player __instance)
    {
        if (__instance.IsRemoved)
        {
            return false;
        }

        if (__instance.m_subAnimations[1].IsStopped)
        {
            if (__instance.CurrentAction == PlayerAction.DrawWeapon)
            {
                __instance.m_subAnimations[1].Play();
            }

            if (__instance.GameOwner == GameOwnerEnum.Server && __instance.FreezeAnimationOnAction != __instance.CurrentAction && (__instance.CurrentAction == PlayerAction.MeleeAttack1 || __instance.CurrentAction == PlayerAction.MeleeAttack2 || __instance.CurrentAction == PlayerAction.MeleeAttack3 || __instance.CurrentAction == PlayerAction.ThrowThrowing) && !__instance.m_subAnimations[1].IsLastFrame())
            {
                if (__instance.CurrentAction == PlayerAction.ThrowThrowing)
                {
                    __instance.m_subAnimations[1].SetFrame(0);
                    ConsoleOutput.ShowMessage(ConsoleOutputType.PlayerAction, __instance.GameOwner + ": Resetting player action ThrowThrowing as it's stuck");
                }

                __instance.m_subAnimations[1].Play();
            }
        }

        if (__instance.RocketRideProjectileWorldID <= 0)
        {
            if (__instance.CurrentAction == PlayerAction.ManualAim)
            {
                __instance.m_subAnimations[0].Rotation = 0f;
                __instance.m_subAnimations[1].Rotation = __instance.AnimationUpperOverride is not null && __instance.AnimationUpperOverride.ResetRotation()
                    ? 0f
                    : __instance.LastDirectionX == 1
                        ? __instance.AimAngle
                        : -__instance.AimAngle;
            }
            else if (__instance.Diving)
            {
                __instance.m_subAnimations[0].Rotation = __instance.DiveRotation;
                __instance.m_subAnimations[1].Rotation = 0f;
            }
            else if (__instance.CaughtByPlayer is not null)
            {
                __instance.m_subAnimations[0].Rotation = 0f;
                __instance.m_subAnimations[1].Rotation = 0f;
            }
            else if (__instance.GrabbedByPlayer is { GrabThrowing: true })
            {
                __instance.m_subAnimations[0].Rotation = 1.5707964f * __instance.GrabbedByPlayer.LastDirectionX;
                __instance.m_subAnimations[1].Rotation = 0f;
            }
            else
            {
                __instance.m_subAnimations[0].Rotation = 0f;
                __instance.m_subAnimations[1].Rotation = 0f;
            }

            if (__instance.IsGrabbedByPlayer)
            {
                __instance.SetAnimation(Animation.GrabbedByPlayer);
            }
            else if (__instance.IsCaughtByPlayer)
            {
                __instance.SetAnimation(Animation.CaughtInDive);
            }
            else if (__instance.FullLanding & __instance.StandingOnGround)
            {
                if (__instance.LastMeleeAction == PlayerAction.JumpAttack && __instance.m_currentAnimation == Animation.JumpAttack && !__instance.m_subAnimations[0].IsLastFrame())
                {
                    __instance.SetAnimation(Animation.JumpAttack);
                }
                else
                {
                    __instance.SetAnimation(Animation.FullLanding);
                }
            }
            else if (__instance.DeathKneeling)
            {
                __instance.SetAnimation(Animation.DeathKneel);
            }
            else if (__instance.LayingOnGround)
            {
                int num = (int)(__instance.LastFallingRotation / 6.2831855f);
                float num2 = __instance.LastFallingRotation - num * 6.2831855f + 0.01f;
                num2 *= __instance.LastDirectionXVisual;
                if ((num2 <= -3.1415927f) | ((num2 >= 0f) & (num2 <= 3.1415927f)))
                {
                    __instance.SetAnimation(Animation.LayOnGroundF);
                }
                else
                {
                    __instance.SetAnimation(Animation.LayOnGroundB);
                }
            }
            else if (__instance.Falling)
            {
                if (__instance.m_currentAnimation == Animation.LayOnGroundB)
                {
                    __instance.Rotation = __instance.LastDirectionXAnimation == -1 ? 1.5707964f : -1.5707964f;

                    __instance.LastFallingRotation = __instance.Rotation;
                    __instance.UpdateRotationDirection();
                }
                else if (__instance.m_currentAnimation == Animation.LayOnGroundF)
                {
                    __instance.Rotation = __instance.LastDirectionXAnimation == -1 ? -1.5707964f : 1.5707964f;

                    __instance.LastFallingRotation = __instance.Rotation;
                    __instance.UpdateRotationDirection();
                }
                else if (__instance.m_currentAnimation == Animation.DeathKneel)
                {
                    __instance.Rotation = __instance.LastDirectionXAnimation == -1 ? -0.3926991f : 0.3926991f;

                    __instance.LastFallingRotation = __instance.Rotation;
                    __instance.UpdateRotationDirection(__instance.LastDirectionXAnimation);
                }

                __instance.SetAnimation(Animation.Falling);
            }
            else if (__instance.GrabThrowing)
            {
                __instance.SetAnimation(Animation.HoldingPlayerInGrabThrowing);
            }
            else if (__instance.GrabAttacking)
            {
                __instance.SetAnimation(Animation.HoldingPlayerInGrabAttack);
            }
            else if (__instance.HoldingPlayerInGrabID != 0)
            {
                __instance.SetAnimation(__instance.Movement != PlayerMovement.Idle ? Animation.HoldingPlayerInGrabWalk : Animation.HoldingPlayerInGrab);
            }
            else if (__instance.CurrentAction == PlayerAction.Kick)
            {
                __instance.SetAnimation(Animation.Kick);
            }
            else if (__instance.CurrentAction == PlayerAction.JumpKick)
            {
                __instance.SetAnimation(Animation.JumpKick);
            }
            else if (__instance.CurrentAction == PlayerAction.JumpAttack)
            {
                __instance.SetAnimation(Animation.JumpAttack);
            }
            else if (__instance.GrabTelegraphing)
            {
                __instance.SetAnimation(Animation.GrabTelegraphing);
            }
            else if (__instance.GrabCharging)
            {
                __instance.SetAnimation(Animation.GrabCharging);
            }
            else if (__instance.Staggering)
            {
                __instance.SetAnimation(Animation.Stagger);
            }
            else if (__instance.Diving)
            {
                __instance.SetAnimation(Animation.Dive);
            }
            else if (__instance.Rolling)
            {
                __instance.SetAnimation(Animation.Roll);
            }
            else if (__instance.Crouching)
            {
                __instance.SetAnimation(Animation.Crouch);
            }
            else if (__instance.Climbing)
            {
                if (__instance.ClimbingDirection == -1)
                {
                    __instance.SetAnimation(Animation.LadderSlide);
                }
                else if (__instance.ClimbingDirection == 1 || __instance.Movement != PlayerMovement.Idle)
                {
                    __instance.SetAnimation(Animation.LadderClimb);
                    __instance.PlayAnimation();
                }
                else
                {
                    __instance.SetAnimation(Animation.LadderClimb);
                    __instance.StopAnimation();
                }
            }
            else if (__instance.CurrentAction == PlayerAction.ManualAim)
            {
                __instance.SetAnimation(Animation.Aiming);
            }
            else if (__instance.LedgeGrabbing)
            {
                __instance.SetAnimation(__instance.LedgeGrabbingTurn ? Animation.LedgeGrabTurn : Animation.LedgeGrab);
            }
            else if (!__instance.StandingOnGround && !__instance.HaveTouchedGroundSinceLastInAir)
            {
                if (__instance.CurrentAction is PlayerAction.MeleeAttack1 or PlayerAction.MeleeAttack2 or PlayerAction.MeleeAttack3)
                {
                    __instance.SetAnimation(Animation.Idle);
                }
                else if ((__instance.WorldBody.GetLinearVelocity().Y > 0f) & (__instance.m_currentAnimation == Animation.Jump) || __instance.WorldBody.GetLinearVelocity().Y > 3f)
                {
                    __instance.SetAnimation(Animation.Jump);
                }
                else
                {
                    __instance.SetAnimation(Animation.JumpFalling);
                }
            }
            else if (__instance.Movement != PlayerMovement.Idle)
            {
                if (__instance.Sprinting)
                {
                    if (__instance.m_modifiers.SprintSpeedModifier < 0.3f)
                    {
                        __instance.SetAnimation(Animation.Walk);
                    }
                    else if (__instance.m_modifiers.SprintSpeedModifier < 0.6f)
                    {
                        __instance.SetAnimation(Animation.Run);
                    }
                    else
                    {
                        __instance.SetAnimation(Animation.Sprint);
                    }
                }
                else if (__instance.Walking)
                {
                    __instance.SetAnimation(Animation.Walk);
                }
                else if (__instance.m_modifiers.RunSpeedModifier < 0.6f)
                {
                    __instance.SetAnimation(Animation.Walk);
                }
                else if (__instance.m_modifiers.RunSpeedModifier > 1.4f && __instance.AnimationUpperOverride is null && __instance.CurrentAction == PlayerAction.Idle && !__instance.IsUsingChainsaw)
                {
                    __instance.SetAnimation(Animation.Sprint);
                }
                else
                {
                    __instance.SetAnimation(Animation.Run);
                }
            }
            else if (__instance.CurrentAction == PlayerAction.ThrowThrowing)
            {
                __instance.SetAnimation(Animation.Idle);
            }
            else if (__instance.TakingCover)
            {
                if (!__instance.IsInCoverPosition())
                {
                    __instance.ManualAimStart = false;
                    __instance.SetAnimation(Animation.Aiming);
                }
                else
                {
                    __instance.SetAnimation(Animation.Cover);
                }
            }
            else if (__instance.MeleeHit)
            {
                __instance.SetAnimation(Animation.Idle);
            }
            else if (__instance.CurrentAction != PlayerAction.Disabled)
            {
                __instance.SetAnimation(Animation.Idle);
            }
            else
            {
                __instance.SetAnimation(Animation.Idle);
            }

            __instance.LastDirectionXAnimation = __instance.LastDirectionX;
            return false;
        }

        __instance.Rotation = 0f;
        __instance.DiveRotation = 0f;
        __instance.SetAnimation(Animation.RocketRide);
        if (__instance.RocketRideProjectile is null)
        {
            __instance.m_subAnimations[0].Rotation = 0f;
            return false;
        }

        if (__instance.RocketRideProjectile.Direction.X >= 0f)
        {
            __instance.m_subAnimations[0].Rotation = (float)Math.Atan2(-(double)__instance.RocketRideProjectile.Direction.Y, __instance.RocketRideProjectile.Direction.X);
            return false;
        }

        __instance.m_subAnimations[0].Rotation = 3.1415927f + (float)Math.Atan2(-(double)__instance.RocketRideProjectile.Direction.Y, __instance.RocketRideProjectile.Direction.X);

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.SetAnimation), typeof(Animation), typeof(PlayerAction))]
    private static bool SetAnimation(Animation animation, PlayerAction animationPlayerMode, Player __instance)
    {
        __instance.Equipment.AutoSheatheWeapons = (animation == Animation.Sprint) | (animation == Animation.Roll) | (animation == Animation.CaughtInDive) | (animation == Animation.GrabbedByPlayer) | (animation == Animation.HoldingPlayerInGrab) | (animation == Animation.HoldingPlayerInGrabAttack) | (animation == Animation.Dive) | (animation == Animation.LadderClimb) | (animation == Animation.LadderSlide) | (animation == Animation.LedgeGrab) | (animation == Animation.LedgeGrabTurn) | (animation == Animation.GrabTelegraphing) | (animation == Animation.GrabCharging);
        __instance.Equipment.RenderMainInOffhand = __instance.InThrowingMode;
        __instance.Equipment.InThrowingMode = __instance.InThrowingMode;
        if (__instance.m_currentAnimation == animation && !__instance.m_softUpdate && __instance.m_currentAnimationPlayerAction == animationPlayerMode && __instance.m_animationUpperOverrideLastValue == __instance.m_animationUpperOverride)
        {
            return false;
        }

        if (__instance.m_currentAnimation != animation)
        {
            __instance.m_softUpdate = false;
        }
        else if (__instance.m_currentAnimation is Animation.Jump or Animation.JumpFalling)
        {
            __instance.m_softUpdatePart = 1;
        }
        else if (animation == Animation.Roll)
        {
            __instance.m_softUpdate = false;
        }
        else if (__instance.CurrentAction == PlayerAction.DrawWeapon)
        {
            __instance.m_softUpdate = false;
        }

        bool flag = __instance.m_currentAnimation != animation;
        bool flag2 = !__instance.m_forceHardAubAnimationReset && !__instance.MeleeWeaponBroken && __instance.m_currentAnimationPlayerAction == animationPlayerMode && (animationPlayerMode == PlayerAction.DrawWeapon) | (animationPlayerMode == PlayerAction.MeleeAttack1) | (animationPlayerMode == PlayerAction.MeleeAttack2) | (animationPlayerMode == PlayerAction.MeleeAttack3) | (animationPlayerMode == PlayerAction.Block) | (animationPlayerMode == PlayerAction.ThrowThrowing);
        __instance.m_forceHardAubAnimationReset = false;
        if (__instance.m_currentAnimation == Animation.None)
        {
            for (short num = 0; num < __instance.m_subAnimationsLength; num += 1)
            {
                if (num != 1 || !flag2)
                {
                    __instance.m_subAnimations[num].SetFrame(0, false);
                }
            }
        }

        __instance.m_currentAnimation = animation;
        bool flag3 = false;
        if (__instance.FreezeAnimationOnAction != PlayerAction.None)
        {
            if (__instance.CurrentAction == __instance.FreezeAnimationOnAction)
            {
                flag3 = true;
            }
            else
            {
                __instance.FreezeAnimationOnAction = PlayerAction.None;
                __instance.PlayAnimation();
            }
        }

        if (__instance.m_currentAnimationPlayerAction != animationPlayerMode)
        {
            __instance.MeleeWeaponBroken = false;
        }

        __instance.m_currentAnimationPlayerAction = animationPlayerMode;
        if (__instance.AnimationUpperOverride is not null)
        {
            flag2 = __instance.m_animationUpperOverrideLastValue == __instance.m_animationUpperOverride;
        }

        bool flag4 = true;
        if (__instance.SpawnAnimation != Player.PlayerSpawnAnimation.None && (__instance.m_currentAnimation != Animation.Idle) | __instance.Disabled)
        {
            __instance.SpawnAnimation = Player.PlayerSpawnAnimation.None;
        }

        switch (__instance.m_currentAnimation)
        {
            case Animation.Idle:
                if (__instance.SpawnAnimation == Player.PlayerSpawnAnimation.Zombie)
                {
                    __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullZombieSpawn"));
                    __instance.m_subAnimationsLength = 1;
                }
                else
                {
                    __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimIdleLower()));
                    __instance.m_subAnimations[1].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimIdleUpper()));
                    __instance.m_subAnimationsLength = 2;
                }

                break;
            case Animation.Run:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("BaseRun"));
                __instance.m_subAnimations[1].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimRunUpper()));
                __instance.m_subAnimationsLength = 2;
                break;
            case Animation.Walk:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("BaseWalk"));
                __instance.m_subAnimations[1].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimWalkUpper()));
                __instance.m_subAnimationsLength = 2;
                break;
            case Animation.Jump:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("BaseJump"));
                __instance.m_subAnimations[1].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimJumpUpper()));
                __instance.m_subAnimationsLength = 2;
                break;
            case Animation.JumpFalling:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("BaseJumpFalling"));
                __instance.m_subAnimations[1].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimJumpUpper(true)));
                __instance.m_subAnimationsLength = 2;
                break;
            case Animation.JumpKick:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("BaseJumpKick"));
                __instance.m_subAnimations[1].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimJumpKickUpper()));
                __instance.m_subAnimationsLength = 2;
                flag4 = false;
                break;
            case Animation.JumpAttack:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimJumpAttack()));
                __instance.m_subAnimationsLength = 1;
                flag4 = false;
                break;
            case Animation.Crouch:
            case Animation.Cover:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("BaseCrouch"));
                __instance.m_subAnimations[1].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimIdleUpper()));
                __instance.m_subAnimationsLength = 2;
                break;
            case Animation.Roll:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullRoll"));
                __instance.m_subAnimations[1].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimJumpUpper()));
                __instance.m_subAnimationsLength = 2;
                flag4 = false;
                break;
            case Animation.Dive:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullDive"));
                __instance.m_subAnimations[0].SetFrame(0);
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.Falling:
                {
                    string text = "FullFallF";
                    float num2 = 1f;
                    if (__instance.WorldBody is not null)
                    {
                        num2 = __instance.WorldBody.GetLinearVelocity().X;
                        if (float.IsNaN(num2) || float.IsInfinity(num2))
                        {
                            num2 = 0f;
                        }
                    }

                    if (num2 != 0f && Math.Sign(num2) != __instance.LastDirectionX)
                    {
                        text = "FullFallB";
                    }

                    __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation(text));
                    __instance.m_subAnimationsLength = 1;
                    break;
                }
            case Animation.LayOnGroundF:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullKnockdownF"));
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.LayOnGroundB:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullKnockdownB"));
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.Sprint:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("BaseSprint"));
                __instance.m_subAnimations[1].SetAnimation(Animations.Data.GetAnimation("UpperSprint"));
                __instance.m_subAnimationsLength = 2;
                break;
            case Animation.Aiming:
                if (__instance.AnimationUpperOverride is not null)
                {
                    __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimIdleLower()));
                    __instance.m_subAnimations[1].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimIdleUpper()));
                    __instance.m_subAnimationsLength = 2;
                }
                else
                {
                    if (__instance.CurrentWeaponDrawn == WeaponItemType.Thrown || __instance.InThrowingMode)
                    {
                        __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("ManualAimBaseThrown"));
                    }
                    else
                    {
                        __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("ManualAimBase"));
                    }

                    string text2;
                    if (__instance.InThrowingMode)
                    {
                        text2 = __instance.ManualAimStart ? Player.ThrowingModeVisuals.AnimManualAimStart : Player.ThrowingModeVisuals.AnimManualAim;
                    }
                    else
                    {
                        text2 = __instance.CurrentWeaponDrawn switch
                        {
                            WeaponItemType.Rifle => "ManualAimRifle",
                            WeaponItemType.Thrown => "ManualAimThrown",
                            _ => "ManualAimHandgun"
                        };

                        switch (__instance.CurrentWeaponDrawn)
                        {
                            case WeaponItemType.Handgun:
                                if (__instance.CurrentHandgunWeapon is not null)
                                {
                                    text2 = __instance.ManualAimStart ? __instance.CurrentHandgunWeapon.Visuals.AnimManualAimStart : __instance.CurrentHandgunWeapon.Visuals.AnimManualAim;
                                }

                                break;
                            case WeaponItemType.Rifle:
                                if (__instance.CurrentRifleWeapon is not null)
                                {
                                    text2 = __instance.ManualAimStart ? __instance.CurrentRifleWeapon.Visuals.AnimManualAimStart : __instance.CurrentRifleWeapon.Visuals.AnimManualAim;
                                }

                                break;
                            case WeaponItemType.Thrown:
                                if (__instance.CurrentThrownWeapon is not null)
                                {
                                    text2 = __instance.ManualAimStart ? __instance.CurrentThrownWeapon.Visuals.AnimManualAimStart : __instance.CurrentThrownWeapon.Visuals.AnimManualAim;
                                }

                                break;
                        }
                    }

                    __instance.m_subAnimations[1].SetAnimation(Animations.Data.GetAnimation(text2));
                    __instance.m_subAnimationsLength = 2;
                }

                break;
            case Animation.LedgeGrab:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullLedgeGrab"));
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.LedgeGrabTurn:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullLedgeGrabTurn"));
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.Kick:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("BaseKick"));
                __instance.m_subAnimations[1].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimKickUpper()));
                __instance.m_subAnimationsLength = 2;
                flag4 = false;
                break;
            case Animation.Stagger:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("BaseStagger"));
                __instance.m_subAnimations[1].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimStaggerUpper()));
                __instance.m_subAnimationsLength = 2;
                break;
            case Animation.CaughtInDive:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullFallF"));
                __instance.m_subAnimations[0].SetFrame(0);
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.GrabbedByPlayer:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation(__instance.IsDead ? "FullGrabbedCorpse" : "FullFallF"));
                __instance.m_subAnimations[0].SetFrame(0);
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.HoldingPlayerInGrab:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullGrab"));
                __instance.m_subAnimations[0].SetFrame(0);
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.HoldingPlayerInGrabWalk:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullGrabWalk"));
                __instance.m_subAnimations[0].SetFrame(0);
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.HoldingPlayerInGrabAttack:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullGrabPunch"));
                __instance.m_subAnimations[0].SetFrame(0);
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.HoldingPlayerInGrabThrowing:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullGrabThrow"));
                __instance.m_subAnimations[0].SetFrame(0);
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.LadderSlide:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullLadderSlide"));
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.LadderClimb:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullLadderClimb"));
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.RocketRide:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullRocketRide"));
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.FullLanding:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimFullLanding()));
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.DeathKneel:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation(__instance.GetAnimDeathKneel()));
                __instance.m_subAnimations[0].SetFrame(0);
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.GrabTelegraphing:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullChargeA"));
                __instance.m_subAnimationsLength = 1;
                break;
            case Animation.GrabCharging:
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation("FullChargeB"));
                __instance.m_subAnimationsLength = 1;
                break;
            default:
                __instance.m_subAnimationsLength = 0;
                break;
        }

        if (__instance.AnimationUpperOverride is not null)
        {
            string text3 = __instance.AnimationUpperOverride.OverrideLowerAnimation();
            if (!string.IsNullOrEmpty(text3))
            {
                __instance.m_subAnimations[0].SetAnimation(Animations.Data.GetAnimation(text3));
            }

            if (__instance.AnimationUpperOverride.OverrideUpperAnimation() != __instance.m_subAnimations[1].GetAnimation().Name || __instance.m_subAnimationsLength < 2)
            {
                if (__instance.AnimationUpperOverride.OverrideUpperAnimationType() == IPlayerUpperAnimationTypeEnum.WaitToContinue)
                {
                    if (__instance.m_animationUpperOverrideLastWaitFrame == -1)
                    {
                        __instance.m_animationUpperOverrideLastWaitFrame = __instance.m_subAnimations[1].GetCurrentFrameIndex();
                    }
                }
                else
                {
                    IPlayerUpperAnimationOverride animationUpperOverride = __instance.AnimationUpperOverride;
                    __instance.AnimationUpperOverride = null;
                    animationUpperOverride.OverrideUpperAnimationAborted(__instance, __instance.m_currentAnimation);
                }
            }
            else if (__instance.m_animationUpperOverrideLastWaitFrame != -1)
            {
                __instance.m_subAnimations[1].SetFrameSilent(__instance.m_animationUpperOverrideLastWaitFrame);
                __instance.m_animationUpperOverrideLastWaitFrame = -1;
            }
            else if (!flag2)
            {
                __instance.m_subAnimations[1].SetFrame(0);
            }
        }

        if (__instance.m_softUpdatePart != -1)
        {
            for (short num3 = 0; num3 < __instance.m_subAnimationsLength; num3 += 1)
            {
                if (num3 != 1 || !flag2)
                {
                    if (num3 == __instance.m_softUpdatePart - 1)
                    {
                        __instance.m_subAnimations[__instance.m_softUpdatePart].SetFrame(0);
                    }
                    else
                    {
                        __instance.m_subAnimations[num3].SetUpdatedFrame();
                    }
                }
            }

            __instance.m_softUpdatePart = -1;
        }
        else if (!__instance.m_softUpdate)
        {
            for (short num4 = 0; num4 < __instance.m_subAnimationsLength; num4 += 1)
            {
                if ((num4 != 1 || !flag2) && (num4 != 0 || flag4 || flag))
                {
                    __instance.m_subAnimations[num4].SetFrame(0);
                }
            }
        }
        else
        {
            for (int i = 0; i < __instance.m_subAnimationsLength; i++)
            {
                if (i != 1 || !flag2)
                {
                    __instance.m_subAnimations[i].SetUpdatedFrame();
                }
            }
        }

        if (__instance.AnimationUpperOverride is not null)
        {
            __instance.m_subAnimations[1].Play();
        }

        if (flag3)
        {
            __instance.StopAnimation();
        }

        __instance.m_animationUpperOverrideLastValue = __instance.AnimationUpperOverride;
        __instance.m_softUpdate = false;

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ProfileGridItem), nameof(ProfileGridItem.RefreshPreviewPlayer))]
    private static void RefreshAnimationMenu(ProfileGridItem __instance)
    {
        if (__instance.Profile is not null)
        {
            bool walkingAnimation = Globals.Random.NextBool();
            __instance.m_previewPlayer.SetAnimation(walkingAnimation ? Animation.Walk : Animation.Idle, PlayerAction.Disabled);
        }
    }
}