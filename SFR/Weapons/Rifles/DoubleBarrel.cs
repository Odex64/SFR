﻿using Microsoft.Xna.Framework;
using SFD;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Rifles;

internal sealed class DoubleBarrel : RWeapon
{
    private int _consumedShells;

    internal DoubleBarrel()
    {
        RWeaponProperties weaponProperties = new(97, "DoubleBarrel", 1, 1, 15, 4, 3000, 100, 200, 12, 97, string.Empty, 0.3f, new Vector2(11f, -2.5f), "MuzzleFlashShotgun", "SawedOff", "SawedOffDraw", "SawedOffReload", "OutOfAmmoHeavy", "WpnDoubleBarrel", false, WeaponCategory.Primary)
        {
            CursorAimOffset = new Vector2(0f, 2.5f),
            ClearRoundsOnReloadStart = false,
            CooldownBeforePostAction = 1000,
            LazerPosition = new Vector2(13f, -0.5f),
            AimStartSoundID = "PistolAim",
            AI_EffectiveRange = 120f,
            CooldownAfterPostAction = 250,
            BreakDebris = ["ItemDebrisWood00", "MetalDebris00C"],
            SpecialAmmoBulletsRefill = 4,
            AI_DamageOutput = DamageOutputType.High
        };

        RWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("DoubleBarrelM");
        weaponVisuals.SetDrawnTexture("DoubleBarrelD");
        weaponVisuals.SetSheathedTexture("DoubleBarrelS");
        weaponVisuals.SetThrowingTexture("DoubleBarrelThrowing");
        weaponVisuals.AnimIdleUpper = "UpperIdleRifle";
        weaponVisuals.AnimCrouchUpper = "UpperCrouchRifle";
        weaponVisuals.AnimJumpKickUpper = "UpperJumpKickRifle";
        weaponVisuals.AnimJumpUpper = "UpperJumpRifle";
        weaponVisuals.AnimJumpUpperFalling = "UpperJumpFallingRifle";
        weaponVisuals.AnimKickUpper = "UpperKickRifle";
        weaponVisuals.AnimStaggerUpper = "UpperStaggerHandgun";
        weaponVisuals.AnimRunUpper = "UpperRunRifle";
        weaponVisuals.AnimWalkUpper = "UpperWalkRifle";
        weaponVisuals.AnimUpperHipfire = "UpperHipfireRifle";
        weaponVisuals.AnimFireArmLength = 2f;
        weaponVisuals.AnimDraw = "UpperDrawRifle";
        weaponVisuals.AnimManualAim = "ManualAimRifle";
        weaponVisuals.AnimManualAimStart = "ManualAimRifleStart";
        weaponVisuals.AnimReloadUpper = "UpperReloadShell";
        weaponVisuals.AnimFullLand = "FullLandHandgun";
        weaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
        weaponProperties.VisualText = "Double Barrel Shotgun";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
        CacheDrawnTextures(["Reload"]);
    }

    private DoubleBarrel(RWeaponProperties weaponProperties, RWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animEvent == AnimationEvent.EnterFrame)
        {
            if (subAnim.GetCurrentFrameIndex() == 1)
            {
                for (int i = 0; i < _consumedShells; i++)
                {
                    SpawnUnsyncedShell(player, "ShellShotgun");
                }

                _consumedShells = 0;
                SoundHandler.PlaySound("MagnumReloadStart", player.Position, player.GameWorld);
            }
            else if (subAnim.GetCurrentFrameIndex() == 3)
            {
                SoundHandler.PlaySound("ShotgunReload", player.Position, player.GameWorld);
            }
        }
    }

    public override void ConsumeAmmoFromFire(Player player)
    {
        _consumedShells += 2;
        base.ConsumeAmmoFromFire(player);
        SoundHandler.PlaySound("Shotgun", player.Position, player.GameWorld);
    }

    public override void GrabAmmo(Player player)
    {
        base.GrabAmmo(player);
        _consumedShells = (MaxRoundsInWeapon - CurrentRoundsInWeapon) * 2;
    }

    public override void OnSubAnimationEvent(Player player, AnimationEvent animationEvent, AnimationData animationData, int currentFrameIndex)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animationEvent == AnimationEvent.EnterFrame)
        {
            if (animationData.Name == "UpperDrawRifle")
            {
                switch (currentFrameIndex)
                {
                    case 1:
                        SoundHandler.PlaySound("Draw1", player.GameWorld);
                        break;
                    case 6:
                        SoundHandler.PlaySound("SawedOffDraw", player.GameWorld);
                        break;
                }
            }
        }
    }

    public override bool CheckDrawLazerAttachment(string subAnimation, int subFrame) => subAnimation is not "UpperReloadShell";

    public override void OnReloadAnimationFinished(Player player)
    {
        if (player.GameOwner != GameOwnerEnum.Server)
        {
            SoundHandler.PlaySound("MagnumReloadEnd", player.GameWorld);
        }
    }

    public override RWeapon Copy()
    {
        DoubleBarrel doubleBarrel = new(Properties, Visuals);
        doubleBarrel.CopyStatsFrom(this);
        return doubleBarrel;
    }
}