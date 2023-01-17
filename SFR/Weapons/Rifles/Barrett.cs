using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Rifles;

internal sealed class Barrett : RWeapon
{
    private bool _reload;

    internal Barrett()
    {
        RWeaponProperties weaponProperties = new(94, "Anti-Materiel_Rifle", "WpnBarrett", false, WeaponCategory.Primary)
        {
            MaxMagsInWeapon = 1,
            MaxRoundsInMag = 1,
            MaxCarriedSpareMags = 3,
            StartMags = 3,
            CooldownBeforePostAction = 400,
            CooldownAfterPostAction = 800,
            ExtraAutomaticCooldown = 200,
            ProjectilesEachBlast = 1,
            ShellID = "ShellBig",
            AccuracyDeflection = 0.005f,
            ProjectileID = 94,
            MuzzlePosition = new Vector2(19f, -1.5f),
            CursorAimOffset = new Vector2(0f, 1.5f),
            LazerPosition = new Vector2(5f, -4.5f),
            MuzzleEffectTextureID = "MuzzleFlashShotgun",
            BlastSoundID = "Sniper",
            DrawSoundID = "SniperDraw",
            GrabAmmoSoundID = "SniperReload",
            OutOfAmmoSoundID = "OutOfAmmoHeavy",
            AimStartSoundID = "PistolAim",
            AI_DamageOutput = DamageOutputType.High,
            CanRefilAtAmmoStashes = false,
            ReloadPostCooldown = 650f,
            BreakDebris = new[]
            {
                "MetalDebris00C",
                "ItemDebrisStockDark00",
                "ItemDebrisShiny01"
            },
            SpecialAmmoBulletsRefill = 3
        };

        RWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("BarrettM");
        weaponVisuals.SetDrawnTexture("BarrettD");
        weaponVisuals.SetSheathedTexture("BarrettThrowing");
        weaponVisuals.SetThrowingTexture("BarrettThrowing");
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
        weaponVisuals.AnimReloadUpper = "UpperReload";
        weaponVisuals.AnimFullLand = "FullLandHandgun";
        weaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
        weaponProperties.VisualText = "Anti-Materiel Rifle";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
        CacheDrawnTextures(new[] { "Reload" });
        LazerUpgrade = 1;
    }

    private Barrett(RWeaponProperties weaponProperties, RWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    public override void OnRecoilEvent(Player player)
    {
        if (player.GameOwner != GameOwnerEnum.Server)
        {
            SoundHandler.PlaySound(Properties.BlastSoundID, player.Position, player.GameWorld);
            SoundHandler.PlaySound("Carbine", player.Position, player.GameWorld);
            SoundHandler.PlaySound("Shotgun", player.Position, player.GameWorld);
            EffectHandler.PlayEffect("MZLED", Vector2.Zero, player.GameWorld, player, Properties.MuzzleEffectTextureID);
            EffectHandler.PlayEffect("MZLED", Vector2.Zero, player.GameWorld, player, "MuzzleFlashS");
            EffectHandler.PlayEffect("CAM_S", Vector2.Zero, player.GameWorld, 0.75f, 100f, false);
        }

        if (player.GameOwner != GameOwnerEnum.Client)
        {
            player.GameWorld.SlowmotionHandler.AddSlowmotion(new Slowmotion(0f, 250f, 350f, 0.2f, 0));
        }
    }

    public override RWeapon Copy()
    {
        Barrett barrett = new(Properties, Visuals);
        barrett.CopyStatsFrom(this);
        return barrett;
    }

    public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animEvent == AnimationEvent.EnterFrame)
        {
            if (subAnim.GetCurrentFrameIndex() == 1)
            {
                SpawnMagazine(player, "MagAssaultRifle", new Vector2(-11f, -3f));
                SoundHandler.PlaySound("MagnumReloadEnd", player.Position, player.GameWorld);
            }
        }
        else if (subAnim.GetCurrentFrameIndex() == 4)
        {
            SoundHandler.PlaySound("PistolReload", player.Position, player.GameWorld);
        }
    }

    public override void OnSubAnimationEvent(Player player, AnimationEvent animationEvent, AnimationData animationData, int currentFrameIndex)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animationEvent == AnimationEvent.EnterFrame && animationData.Name == "UpperDrawRifle")
        {
            switch (currentFrameIndex)
            {
                case 1:
                    SoundHandler.PlaySound("Draw1", player.GameWorld);
                    break;
                case 6:
                    SoundHandler.PlaySound("SniperDraw", player.GameWorld);
                    break;
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

    public override void OnPostFireAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animEvent == AnimationEvent.EnterFrame)
        {
            if (subAnim.GetCurrentFrameIndex() == 1)
            {
                if (Properties.ShellID != string.Empty)
                {
                    var currentRifleWeapon = player.CurrentRifleWeapon;
                    if (currentRifleWeapon != null)
                    {
                        if (currentRifleWeapon.CurrentRoundsInWeapon > 0)
                        {
                            _reload = false;
                            SpawnUnsyncedShell(player, Properties.ShellID);
                        }
                        else if (!_reload)
                        {
                            _reload = true;
                            SpawnUnsyncedShell(player, Properties.ShellID);
                        }
                    }
                }

                SoundHandler.PlaySound("SniperBoltAction1", player.GameWorld);
            }

            if (subAnim.GetCurrentFrameIndex() == 3)
            {
                SoundHandler.PlaySound("SniperBoltAction2", player.GameWorld);
            }
        }
    }

    public override void OnReloadAnimationFinished(Player player)
    {
        player.AnimationUpperOverride = new PlayerEmptyBoltActionAnimation();
        _reload = false;
    }

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * 0.1f);
        var linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
        linearVelocity.X *= 0.5f;
        linearVelocity.Y *= 0.5f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }
}