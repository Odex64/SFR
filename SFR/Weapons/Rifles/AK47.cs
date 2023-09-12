using Microsoft.Xna.Framework;
using SFD;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Rifles
{
    internal sealed class AK47 : RWeapon
    {
        internal AK47()
        {
            RWeaponProperties rWeaponProperties = new(107, "AK47", 1, 18, 4, 2, -1, 95, 0, 1, 5, "ShellSmall", 0.15f, new Vector2(10f, -2f), "MuzzleFlashM", "TommyGun", "TommyGunDraw", "TommyGunReload", "OutOfAmmoHeavy", "WpnAK47", false, WeaponCategory.Primary)
            {
                BurstRoundsToFire = 3,
                BurstCooldown = 70,
                CooldownAfterPostAction = 325,
                ExtraAutomaticCooldown = 150,
                CursorAimOffset = new(0f, 1f),
                LazerPosition = new(12f, -0.5f),
                AimStartSoundID = "PistolAim",
                BreakDebris = new[] { "MetalDebris00A", "ItemDebrisStockWood00", "ItemDebrisShiny00" },
                SpecialAmmoBulletsRefill = 35
            };
            RWeaponVisuals rWeaponVisuals = new();
            rWeaponVisuals.SetModelTexture("AK47M");
            rWeaponVisuals.SetDrawnTexture("AK47D");
            rWeaponVisuals.SetSheathedTexture("AK47S");
            rWeaponVisuals.SetThrowingTexture("AK47Throwing");
            rWeaponVisuals.AnimIdleUpper = "UpperIdleRifle";
            rWeaponVisuals.AnimCrouchUpper = "UpperCrouchRifle";
            rWeaponVisuals.AnimJumpKickUpper = "UpperJumpKickRifle";
            rWeaponVisuals.AnimJumpUpper = "UpperJumpRifle";
            rWeaponVisuals.AnimJumpUpperFalling = "UpperJumpFallingRifle";
            rWeaponVisuals.AnimKickUpper = "UpperKickRifle";
            rWeaponVisuals.AnimStaggerUpper = "UpperStaggerHandgun";
            rWeaponVisuals.AnimRunUpper = "UpperRunRifle";
            rWeaponVisuals.AnimWalkUpper = "UpperWalkRifle";
            rWeaponVisuals.AnimUpperHipfire = "UpperHipfireRifle";
            rWeaponVisuals.AnimFireArmLength = 2f;
            rWeaponVisuals.AnimDraw = "UpperDrawRifle";
            rWeaponVisuals.AnimManualAim = "ManualAimRifle";
            rWeaponVisuals.AnimManualAimStart = "ManualAimRifleStart";
            rWeaponVisuals.AnimReloadUpper = "UpperReload";
            rWeaponVisuals.AnimFullLand = "FullLandHandgun";
            rWeaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
            rWeaponProperties.VisualText = "AK47";
            SetPropertiesAndVisuals(rWeaponProperties, rWeaponVisuals);
            CacheDrawnTextures(new[] { "Reload" });
        }

        public AK47(RWeaponProperties rwp, RWeaponVisuals rwv)
        {
            SetPropertiesAndVisuals(rwp, rwv);
        }

        public override RWeapon Copy()
        {
            AK47 aK47 = new(Properties, Visuals);
            aK47.CopyStatsFrom(this);
            return aK47;
        }

        public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
        {
            if (player.GameOwner != GameOwnerEnum.Server)
            {
                if (animEvent == AnimationEvent.EnterFrame && subAnim.GetCurrentFrameIndex() == 1)
                {
                    SpawnMagazine(player, "MagDrum", new Vector2(-8f, -3f));
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
                if (currentFrameIndex == 1)
                {
                    SoundHandler.PlaySound("Draw1", player.GameWorld);
                }
                if (currentFrameIndex == 6)
                {
                    SoundHandler.PlaySound("TommyGunDraw", player.GameWorld);
                }
            }
        }

        public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
        {
            thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * 0.8f);
            Vector2 linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
            linearVelocity.X *= 0.9f;
            linearVelocity.Y *= 0.9f;
            thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
            base.OnThrowWeaponItem(player, thrownWeaponItem);
        }
    }
}
