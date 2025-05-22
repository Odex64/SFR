using Microsoft.Xna.Framework;
using SFD;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Rifles;

internal sealed class AA12 : RWeapon
{
    internal AA12()
    {
        RWeaponProperties weaponProperties = new(93, "AA12", 1, 8, 3, 2, -1, 300, 0, 5, 10, "ShellShotgun", 0.3f, new Vector2(12f, -2f), "MuzzleFlashShotgun", "SawedOff", "TommyGunDraw", "TommyGunReload", "OutOfAmmoHeavy", "WpnAA12", false, WeaponCategory.Primary)
        {
            CursorAimOffset = new Vector2(0f, 1f),
            LazerPosition = new Vector2(14f, -0.5f),
            AimStartSoundID = "PistolAim",
            BreakDebris =
            [
                "MetalDebris00A",
                "ItemDebrisShiny00"
            ],
            SpecialAmmoBulletsRefill = 10,
            AI_DamageOutput = DamageOutputType.High,
            VisualText = "AA12"
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

        weaponVisuals.SetModelTexture("AA12M");
        weaponVisuals.SetDrawnTexture("AA12D");
        weaponVisuals.SetSheathedTexture("AA12S");
        weaponVisuals.SetThrowingTexture("AA12Throwing");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

        CacheDrawnTextures(["Reload"]);
    }

    private AA12(RWeaponProperties weaponProperties, RWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public override RWeapon Copy()
    {
        AA12 wpnAA12 = new(Properties, Visuals);
        wpnAA12.CopyStatsFrom(this);
        return wpnAA12;
    }

    public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animEvent == AnimationEvent.EnterFrame)
        {
            if (subAnim.GetCurrentFrameIndex() == 1)
            {
                SpawnMagazine(player, "MagDrum", new Vector2(-8f, -3f));
                SoundHandler.PlaySound("MagnumReloadEnd", player.Position, player.GameWorld);
            }
            else if (subAnim.GetCurrentFrameIndex() == 4)
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
        Vector2 linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
        linearVelocity.X *= 0.75f;
        linearVelocity.Y *= 0.75f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }
}