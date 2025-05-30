﻿using Microsoft.Xna.Framework;
using SFD;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Handguns;

internal sealed class Anaconda : RWeapon
{
    private const int _shellsToSpawn = 5;
    private bool _spawnShells;

    internal Anaconda()
    {
        RWeaponProperties weaponProperties = new(109, "Anaconda", "WpnAnaconda", false, WeaponCategory.Secondary)
        {
            MaxMagsInWeapon = 1,
            MaxRoundsInMag = 5,
            MaxCarriedSpareMags = 4,
            StartMags = 2,
            CooldownBeforePostAction = 750,
            CooldownAfterPostAction = 0,
            ExtraAutomaticCooldown = 200,
            ShellID = "",
            AccuracyDeflection = 0.04f,
            ProjectileID = 28,
            MuzzlePosition = new Vector2(5f, -2f),
            MuzzleEffectTextureID = "MuzzleFlashS",
            BlastSoundID = "Revolver",
            DrawSoundID = "RevolverDraw",
            GrabAmmoSoundID = "RevolverReload",
            OutOfAmmoSoundID = "OutOfAmmoLight",
            CursorAimOffset = new Vector2(0f, 3.5f),
            LazerPosition = new Vector2(6f, -0.5f),
            AimStartSoundID = "PistolAim",
            AI_DamageOutput = DamageOutputType.High,
            BreakDebris = [],
            SpecialAmmoBulletsRefill = 10,
            VisualText = "Anaconda"
        };

        RWeaponVisuals weaponVisuals = new()
        {
            AnimIdleUpper = "UpperIdleHandgun",
            AnimCrouchUpper = "UpperCrouchHandgun",
            AnimJumpKickUpper = "UpperJumpKickHandgun",
            AnimJumpUpper = "UpperJumpHandgun",
            AnimJumpUpperFalling = "UpperJumpFallingHandgun",
            AnimKickUpper = "UpperKickHandgun",
            AnimStaggerUpper = "UpperStaggerHandgun",
            AnimRunUpper = "UpperRunHandgun",
            AnimWalkUpper = "UpperWalkHandgun",
            AnimUpperHipfire = "UpperHipfireHandgun",
            AnimFireArmLength = 7f,
            AnimDraw = "UpperDrawMagnum",
            AnimManualAim = "ManualAimHandgun",
            AnimManualAimStart = "ManualAimHandgunStart",
            AnimReloadUpper = "UpperReload",
            AnimFullLand = "FullLandHandgun",
            AnimToggleThrowingMode = "UpperToggleThrowing"
        };

        weaponVisuals.SetModelTexture("AnacondaM");
        weaponVisuals.SetDrawnTexture("AnacondaD");
        weaponVisuals.SetThrowingTexture("AnacondaThrowing");
        weaponVisuals.SetHolsterTexture("AnacondaH");
        weaponVisuals.SetSheathedTexture("AnacondaS");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

        CacheDrawnTextures(["Reload"]);
    }

    private Anaconda(RWeaponProperties rwp, RWeaponVisuals rwv) => SetPropertiesAndVisuals(rwp, rwv);

    public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
    {
        if (animEvent == AnimationEvent.EnterFrame && subAnim.GetCurrentFrameIndex() == 1)
        {
            if (player.GameOwner != GameOwnerEnum.Server)
            {
                if (_spawnShells)
                {
                    for (int i = 0; i < _shellsToSpawn; i++)
                    {
                        SpawnUnsyncedShell(player, "ShellSmall");
                    }
                }

                SoundHandler.PlaySound("MagnumReloadStart", player.Position, player.GameWorld);
            }

            _spawnShells = false;
        }
    }

    public override void GrabAmmo(Player player)
    {
        _spawnShells = true;
        base.GrabAmmo(player);
    }

    public override void OnSubAnimationEvent(Player player, AnimationEvent animationEvent, AnimationData animationData, int currentFrameIndex)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animationEvent == AnimationEvent.EnterFrame && animationData.Name == "UpperDrawMagnum")
        {
            switch (currentFrameIndex)
            {
                case 1:
                    SoundHandler.PlaySound("Draw1", player.GameWorld);
                    break;
                case 6:
                    SoundHandler.PlaySound("MagnumDraw", player.GameWorld);
                    break;
            }
        }
    }

    public override bool CheckDrawLazerAttachment(string subAnimation, int subFrame) => subAnimation is not "UpperReload";

    public override RWeapon Copy()
    {
        Anaconda wpnAnaconda = new(Properties, Visuals);
        wpnAnaconda.CopyStatsFrom(this);
        return wpnAnaconda;
    }

    public override void OnReloadAnimationFinished(Player player)
    {
        _spawnShells = true;
        if (player.GameOwner != GameOwnerEnum.Server)
        {
            SoundHandler.PlaySound("MagnumReloadEnd", player.Position, player.GameWorld);
        }
    }

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        Vector2 linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
        linearVelocity.X *= 1.2f;
        linearVelocity.Y *= 1f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
    }
}