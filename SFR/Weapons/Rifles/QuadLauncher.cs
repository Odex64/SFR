using SFD;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Rifles;

internal sealed class QuadLauncher : RWeapon
{
    internal QuadLauncher()
    {
        RWeaponProperties weaponProperties = new(99, "Quad_Launcher", "WpnQuadLauncher", false, WeaponCategory.Primary)
        {
            MaxMagsInWeapon = 4,
            MaxRoundsInMag = 1,
            MaxCarriedSpareMags = 4,
            StartMags = 8,
            CooldownAfterPostAction = 100,
            CooldownBeforePostAction = 100,
            ExtraAutomaticCooldown = 250,
            ProjectilesEachBlast = 1,
            ProjectileID = 99,
            ShellID = "",
            AccuracyDeflection = 0f,
            MuzzlePosition = new(6f, -2f),
            MuzzleEffectTextureID = "MuzzleFlashBazooka",
            BlastSoundID = "Bazooka",
            DrawSoundID = "ShotgunDraw",
            GrabAmmoSoundID = "ShotgunReload",
            OutOfAmmoSoundID = "OutOfAmmoHeavy",
            CursorAimOffset = new(0f, 2f),
            LazerPosition = new(8f, -4.5f),
            AimStartSoundID = "PistolAim",
            AI_ImpactAoERadius = 1.43999994f,
            AI_DamageOutput = DamageOutputType.High,
            CanRefilAtAmmoStashes = false,
            AI_HasOneShotPotential = true,
            BreakDebris =
            [
                "ItemDebrisBazooka00",
                "MetalDebris00B",
                "ItemDebrisBazooka00"
            ],
            SpecialAmmoBulletsRefill = 4
        };

        RWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("QuadLauncherM");
        weaponVisuals.SetDrawnTexture("QuadLauncherD");
        weaponVisuals.SetSheathedTexture("QuadLauncherS");
        weaponVisuals.SetThrowingTexture("QuadLauncherThrowing");
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
        weaponVisuals.AnimFullLand = "FullLandHandgun";
        weaponVisuals.AnimReloadUpper = "UpperReloadBazooka";
        weaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
        weaponProperties.VisualText = "Quad Launcher";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private QuadLauncher(RWeaponProperties weaponProperties, RWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public override RWeapon Copy()
    {
        QuadLauncher wpn = new(Properties, Visuals);
        wpn.CopyStatsFrom(this);
        return wpn;
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
                    SoundHandler.PlaySound("PistolDraw", player.GameWorld);
                    break;
            }
        }
    }

    public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animEvent == AnimationEvent.EnterFrame)
        {
            if (subAnim.GetCurrentFrameIndex() == 5)
            {
                SoundHandler.PlaySound("PistolReload", player.Position, player.GameWorld);
            }
        }
    }

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * 0.6f);
        var linearVelocity = thrownWeaponItem.Body.GetLinearVelocity() * 0.8f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }
}