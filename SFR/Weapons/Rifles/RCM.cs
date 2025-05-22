using SFD;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;
using SFDGameScriptInterface;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SFR.Weapons.Rifles;

internal sealed class RCM : RWeapon
{
    internal RCM()
    {
        RWeaponProperties weaponProperties = new(100, "RCM", "WpnRCM", false, WeaponCategory.Primary)
        {
            MaxMagsInWeapon = 1,
            MaxRoundsInMag = 1,
            MaxCarriedSpareMags = 2,
            StartMags = 2,
            CooldownAfterPostAction = 500,
            CooldownBeforePostAction = 0,
            ExtraAutomaticCooldown = 200,
            ProjectilesEachBlast = 1,
            ProjectileID = 100,
            ShellID = string.Empty,
            AccuracyDeflection = 0f,
            MuzzlePosition = new Vector2(6f, -4.5f),
            MuzzleEffectTextureID = "MuzzleFlashBazooka",
            BlastSoundID = "Bazooka",
            DrawSoundID = "ShotgunDraw",
            GrabAmmoSoundID = "ShotgunReload",
            OutOfAmmoSoundID = "OutOfAmmoHeavy",
            CursorAimOffset = new Vector2(0f, 2f),
            LazerPosition = new Vector2(8f, -3.5f),
            AimStartSoundID = "PistolAim",
            AI_ImpactAoERadius = 1.43999994f,
            AI_DamageOutput = DamageOutputType.High,
            AI_EffectiveRange = 80,
            CanRefilAtAmmoStashes = false,
            AI_HasOneShotPotential = true,
            BreakDebris =
            [
                "ItemDebrisBazooka00",
                "MetalDebris00B",
                "ItemDebrisBazooka00"
            ],
            SpecialAmmoBulletsRefill = 2,
            VisualText = "RCM"
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
            AnimDraw = "UpperDrawRifle",
            AnimManualAim = "ManualAimRifle",
            AnimManualAimStart = "ManualAimRifleStart",
            AnimFullLand = "FullLandHandgun",
            AnimReloadUpper = "UpperReloadBazooka",
            AnimToggleThrowingMode = "UpperToggleThrowing"
        };

        weaponVisuals.SetModelTexture("RCMM");
        weaponVisuals.SetDrawnTexture("RCMD");
        weaponVisuals.SetSheathedTexture("RCMS");
        weaponVisuals.SetThrowingTexture("RCMThrowing");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

        LazerUpgrade = 1;
    }

    private RCM(RWeaponProperties weaponProperties, RWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public override RWeapon Copy()
    {
        RCM wpnRCM = new(Properties, Visuals);
        wpnRCM.CopyStatsFrom(this);
        return wpnRCM;
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

    public override void OnRecoilEvent(Player player)
    {
        if (player.CurrentAction == PlayerAction.ManualAim)
        {
            player.SetInputMode(PlayerInputMode.ReadOnly);
        }

        base.OnRecoilEvent(player);
    }

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * 0.6f);
        Vector2 linearVelocity = thrownWeaponItem.Body.GetLinearVelocity() * 0.8f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }
}