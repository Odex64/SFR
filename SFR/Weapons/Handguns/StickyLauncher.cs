﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;
using SFR.Helper;
using SFR.Misc;
using SFR.Objects;
using Player = SFD.Player;

namespace SFR.Weapons.Handguns;

internal sealed class StickyLauncher : RWeapon, IExtendedWeapon
{
    private readonly List<ObjectStickyProjectile> _stickies = [];
    private bool _ejectShells = true;

    internal StickyLauncher()
    {
        RWeaponProperties weaponProperties = new(86, "Sticky_Launcher", "WpnStickyLauncher", false, WeaponCategory.Secondary)
        {
            MaxMagsInWeapon = 1,
            MaxRoundsInMag = 3,
            MaxCarriedSpareMags = 3,
            StartMags = 2,
            CooldownBeforePostAction = 600,
            CooldownAfterPostAction = 0,
            ExtraAutomaticCooldown = 0,
            ShellID = "",
            AccuracyDeflection = 0.05f,
            ProjectileID = -1,
            MuzzlePosition = new Vector2(8f, -2f),
            MuzzleEffectTextureID = "MuzzleFlashS",
            BlastSoundID = "",
            DrawSoundID = "GLauncherDraw",
            GrabAmmoSoundID = "GLauncherReload",
            OutOfAmmoSoundID = "OutOfAmmoHeavy",
            CursorAimOffset = new Vector2(0f, 3.5f),
            LazerPosition = new Vector2(10f, -0.5f),
            AimStartSoundID = "PistolAim",
            AI_DamageOutput = DamageOutputType.None,
            CanRefilAtAmmoStashes = true,
            AI_MaxRange = 0f,
            BreakDebris = ["ItemDebrisWood00", "ItemDebrisShiny00", "MetalDebris00C"],
            SpecialAmmoBulletsRefill = 8,
            VisualText = "Sticky Launcher"
        };

        RWeaponVisuals weaponVisuals = new()
        {
            AnimIdleUpper = "UpperIdleRifle",
            AnimCrouchUpper = "UpperCrouchRifle",
            AnimJumpKickUpper = "UpperJumpKickRifle",
            AnimJumpUpper = "UpperJumpRifle",
            AnimJumpUpperFalling = "UpperJumpFallingRifle",
            AnimKickUpper = "UpperKickRifle",
            AnimStaggerUpper = "UpperStaggerRifle",
            AnimRunUpper = "UpperRunRifle",
            AnimWalkUpper = "UpperWalkRifle",
            AnimUpperHipfire = "UpperHipfireHandgun",
            AnimFireArmLength = 7f,
            AnimDraw = "UpperDrawRifle",
            AnimManualAim = "ManualAimRifle",
            AnimManualAimStart = "ManualAimRifleStart",
            AnimReloadUpper = "UpperReload",
            AnimFullLand = "FullLandHandgun",
            AnimToggleThrowingMode = "UpperToggleThrowing"
        };

        weaponVisuals.SetModelTexture("StickyLauncherM");
        weaponVisuals.SetDrawnTexture("StickyLauncherD");
        weaponVisuals.SetSheathedTexture("StickyLauncherS");
        weaponVisuals.SetThrowingTexture("StickyLauncherThrowing");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
        CacheDrawnTextures(["Reload"]);
    }

    private StickyLauncher(RWeaponProperties weaponProperties, RWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public void Update(Player player, float ms, float realMs)
    {
        if (player.VirtualKeyboard.PressingKey(14))
        {
            foreach (ObjectStickyProjectile s in GetRealStickies())
            {
                if (s.TimeStamp < player.GameWorld.ElapsedTotalGameTime - 600)
                {
                    s.Detonate();
                }
            }
        }
    }

    public void GetDealtDamage(Player player, float damage)
    {
    }

    public void OnHit(Player player, Player target)
    {
    }

    public void OnHitObject(Player player, PlayerHitEventArgs args, ObjectData obj)
    {
    }

    public void DrawExtra(SpriteBatch spritebatch, Player player, float ms)
    {
    }

    public override void GrabAmmo(Player player)
    {
        _ejectShells = true;
        base.GrabAmmo(player);
    }

    public override void BeforeCreateProjectile(BeforeCreateProjectileArgs args)
    {
        ObjectStickyProjectile pipe = (ObjectStickyProjectile)ObjectData.CreateNew(new ObjectDataStartParams(args.GameWorld.IDCounter.NextID(), 0, 0, "ProjectileSticky", args.GameWorld.GameOwner));
        _ = args.GameWorld.CreateTile(new SpawnObjectInformation(pipe, args.WorldPosition, 0, 1, args.Direction.GetRotatedVector(0.2f * args.Player.LastDirectionX * (1 - args.Direction.Y)) * 9, (float)Globals.Random.NextDouble()));
        _stickies.Add(pipe);

        args.Handled = true;
        args.FireResult = true;

        SoundHandler.PlaySound("GLFire", args.Player.GameWorld);
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
                        SoundHandler.PlaySound("GLauncherDraw", player.GameWorld);
                        break;
                }
            }
        }
    }

    public override bool CheckDrawLazerAttachment(string subAnimation, int subFrame) => subAnimation is not "UpperReload";

    public override RWeapon Copy()
    {
        StickyLauncher stickyLauncher = new(Properties, Visuals);
        stickyLauncher.CopyStatsFrom(this);
        return stickyLauncher;
    }

    public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
    {
        if (animEvent == AnimationEvent.EnterFrame)
        {
            if (subAnim.GetCurrentFrameIndex() == 1)
            {
                if (player.GameOwner != GameOwnerEnum.Server)
                {
                    if (_ejectShells)
                    {
                        SpawnUnsyncedShell(player, "ShellGLauncher");
                    }

                    SoundHandler.PlaySound("MagnumReloadStart", player.Position, player.GameWorld);
                }

                _ejectShells = false;
            }
            else
            {
                _ejectShells = true;
            }
        }
    }

    public override void OnReloadAnimationFinished(Player player)
    {
        _ejectShells = true;
        if (player.GameOwner != GameOwnerEnum.Server)
        {
            SoundHandler.PlaySound("MagnumReloadEnd", player.Position, player.GameWorld);
        }
    }

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * 0.6f);
        Vector2 linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
        linearVelocity.X *= 0.8f;
        linearVelocity.Y *= 0.8f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }

    private IEnumerable<ObjectStickyProjectile> GetRealStickies() => _stickies.Where(s => s is { RemovalInitiated: false, Detonated: false }).ToArray();
}