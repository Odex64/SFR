using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Rifles;

internal sealed class Winchester : RWeapon
{
    private bool _reload;

    internal Winchester()
    {
        RWeaponProperties weaponProperties = new(101, "Winchester", "WpnWinchester", false, WeaponCategory.Primary)
        {
            MaxMagsInWeapon = 7,
            MaxRoundsInMag = 1,
            MaxCarriedSpareMags = 28,
            StartMags = 14,
            CooldownBeforePostAction = 300,
            CooldownAfterPostAction = 700,
            ExtraAutomaticCooldown = 200,
            ProjectilesEachBlast = 1,
            ShellID = "ShellBig",
            AccuracyDeflection = 0.01f,
            ProjectileID = 101,
            MuzzlePosition = new Vector2(11f, -2.5f),
            CursorAimOffset = new Vector2(0f, 2.5f),
            MuzzleEffectTextureID = "MuzzleFlashL",
            BlastSoundID = "Carbine",
            DrawSoundID = "ShotgunDraw",
            GrabAmmoSoundID = "PistolReload",
            OutOfAmmoSoundID = "OutOfAmmoHeavy",
            ClearRoundsOnReloadStart = false,
            ReloadPostCooldown = 650f,
            LazerPosition = new Vector2(13f, -0.5f),
            AimStartSoundID = "PistolAim",
            AI_EffectiveRange = 400f,
            BreakDebris =
            [
                "ItemDebrisStockWood00",
                "ItemDebrisShiny01",
                "ItemDebrisDark00"
            ],
            SpecialAmmoBulletsRefill = 14,
            AI_DamageOutput = DamageOutputType.Standard,
            VisualText = "Winchester"
        };

        RWeaponVisuals weaponVisuals = new()
        {
            AnimIdleUpper = "UpperIdleRifle",
            AnimCrouchUpper = "UpperCrouchRifle",
            AnimJumpKickUpper = "UpperJumpKickRifle",
            AnimJumpUpper = "UpperJumpRifle",
            AnimJumpUpperFalling = "UpperJumpFallingRifle",
            AnimKickUpper = "UpperKickRifle",
            AnimStaggerUpper = "UpperStaggerHandgun",
            AnimRunUpper = "UpperRunRifle",
            AnimWalkUpper = "UpperWalkRifle",
            AnimUpperHipfire = "UpperHipfireRifle",
            AnimFireArmLength = 2f,
            AnimDraw = "UpperDrawShotgun",
            AnimManualAim = "ManualAimRifle",
            AnimManualAimStart = "ManualAimRifleStart",
            AnimReloadUpper = "UpperReloadShell",
            AnimFullLand = "FullLandHandgun",
            AnimToggleThrowingMode = "UpperToggleThrowing"
        };

        weaponVisuals.SetModelTexture("WinchesterM");
        weaponVisuals.SetDrawnTexture("WinchesterD");
        weaponVisuals.SetSheathedTexture("WinchesterS");
        weaponVisuals.SetThrowingTexture("WinchesterThrowing");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

        CacheDrawnTextures(["Pump"]);
    }

    private Winchester(RWeaponProperties weaponProperties, RWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public override void OnSubAnimationEvent(Player player, AnimationEvent animationEvent, AnimationData animationData, int currentFrameIndex)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animationEvent == AnimationEvent.EnterFrame)
        {
            if (animationData.Name == "UpperDrawShotgun")
            {
                switch (currentFrameIndex)
                {
                    case 0:
                        SoundHandler.PlaySound("Draw1", player.GameWorld);
                        return;
                    case 3:
                        SoundHandler.PlaySound("ShotgunPump1", player.GameWorld);
                        return;
                    case 5:
                        SoundHandler.PlaySound("ShotgunPump2", player.GameWorld);
                        break;
                }
            }
        }
    }

    public override void OnSetPostFireAction(Player player)
    {
        if (player.CurrentAction == PlayerAction.ManualAim)
        {
            player.AnimationUpperOverride = new PlayerPostFireAnimation_BoltActionManualAim(player, this);
            return;
        }

        player.AnimationUpperOverride = new PlayerPostFireAnimation_BoltActionHipFire(player, this);
    }

    public override void OnRecoilEvent(Player player)
    {
        if (player.GameOwner != GameOwnerEnum.Server)
        {
            SoundHandler.PlaySound(Properties.BlastSoundID, player.Position, player.GameWorld);
            EffectHandler.PlayEffect("MZLED", Vector2.Zero, player.GameWorld, player, Properties.MuzzleEffectTextureID);
        }
    }

    public override void OnPostFireAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
    {
        if (player.GameOwner != GameOwnerEnum.Server)
        {
            if (animEvent == AnimationEvent.EnterFrame)
            {
                if (subAnim.GetCurrentFrameIndex() == 1)
                {
                    if (Properties.ShellID != string.Empty)
                    {
                        RWeapon currentRifleWeapon = player.CurrentRifleWeapon;
                        if (currentRifleWeapon is not null)
                        {
                            if (currentRifleWeapon.CurrentRoundsInWeapon <= 0)
                            {
                                if (!_reload)
                                {
                                    _reload = true;
                                    SpawnUnsyncedShell(player, Properties.ShellID);
                                }
                            }
                            else
                            {
                                _reload = false;
                                SpawnUnsyncedShell(player, Properties.ShellID);
                            }
                        }
                    }

                    SoundHandler.PlaySound("ShotgunPump1", player.GameWorld);
                }

                if (subAnim.GetCurrentFrameIndex() == 3)
                {
                    SoundHandler.PlaySound("ShotgunPump2", player.GameWorld);
                }
            }
        }
    }

    public override void OnReloadAnimationFinished(Player player)
    {
        player.AnimationUpperOverride = new PlayerEmptyBoltActionAnimation();
        _reload = false;
    }

    public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animEvent == AnimationEvent.EnterFrame)
        {
            if (subAnim.GetCurrentFrameIndex() == 3)
            {
                SoundHandler.PlaySound("ShotgunReload", player.GameWorld);
            }
        }
    }

    public override RWeapon Copy()
    {
        Winchester winchester = new(Properties, Visuals);
        winchester.CopyStatsFrom(this);
        winchester._reload = _reload;
        return winchester;
    }

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * 0.8f);
        Vector2 linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
        linearVelocity.X *= 0.85f;
        linearVelocity.Y *= 0.85f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }
}