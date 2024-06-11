using SFD;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Rifles;

internal sealed class AK47 : RWeapon
{
    internal AK47()
    {
        RWeaponProperties weaponProperties = new(107, "AK47", 1, 18, 4, 2, -1, 95, 0, 1, 5, "ShellSmall", 0.15f, new(10f, -2f), "MuzzleFlashM", "TommyGun", "TommyGunDraw", "TommyGunReload", "OutOfAmmoHeavy", "WpnAK47", false, WeaponCategory.Primary)
        {
            BurstRoundsToFire = 3,
            BurstCooldown = 70,
            CooldownAfterPostAction = 325,
            ExtraAutomaticCooldown = 150,
            CursorAimOffset = new(0f, 1f),
            LazerPosition = new(12f, -0.5f),
            AimStartSoundID = "PistolAim",
            BreakDebris = ["MetalDebris00A", "ItemDebrisStockWood00", "ItemDebrisShiny00"],
            SpecialAmmoBulletsRefill = 35,
            VisualText = "AK47"
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
            AnimReloadUpper = "UpperReload",
            AnimFullLand = "FullLandHandgun",
            AnimToggleThrowingMode = "UpperToggleThrowing"
        };

        weaponVisuals.SetModelTexture("AK47M");
        weaponVisuals.SetDrawnTexture("AK47D");
        weaponVisuals.SetSheathedTexture("AK47S");
        weaponVisuals.SetThrowingTexture("AK47Throwing");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

        CacheDrawnTextures(["Reload"]);
    }

    private AK47(RWeaponProperties rwp, RWeaponVisuals rwv) => SetPropertiesAndVisuals(rwp, rwv);

    public override RWeapon Copy()
    {
        AK47 ak47 = new(Properties, Visuals);
        ak47.CopyStatsFrom(this);
        return ak47;
    }

    public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
    {
        if (player.GameOwner != GameOwnerEnum.Server)
        {
            if (animEvent == AnimationEvent.EnterFrame && subAnim.GetCurrentFrameIndex() == 1)
            {
                SpawnMagazine(player, "MagDrum", new(-8f, -3f));
                SoundHandler.PlaySound("MagnumReloadEnd", player.Position, player.GameWorld);
            }

            if (animEvent == AnimationEvent.EnterFrame && subAnim.GetCurrentFrameIndex() == 4)
            {
                SoundHandler.PlaySound("PistolReload", player.Position, player.GameWorld);
            }
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
                    SoundHandler.PlaySound("TommyGunDraw", player.GameWorld);
                    break;
            }
        }
    }

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * 0.8f);
        var linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
        linearVelocity.X *= 0.9f;
        linearVelocity.Y *= 0.9f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }
}