using Microsoft.Xna.Framework;
using SFD.Sounds;
using SFD;
using SFD.Weapons;
using SFD.Objects;

namespace SFR.Weapons.Handguns
{
    internal sealed class Anaconda : RWeapon
    {
        public bool m_spawnShells;

        public Anaconda()
        {
            RWeaponProperties rweaponProperties = new(109, "Anaconda", "WpnAnaconda", false, WeaponCategory.Secondary)
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
                AI_DamageOutput = DamageOutputType.Low,
                BreakDebris = new string[0],
                SpecialAmmoBulletsRefill = 10
            };
            RWeaponVisuals rweaponVisuals = new();
            rweaponVisuals.SetModelTexture("AnacondaM");
            rweaponVisuals.SetDrawnTexture("AnacondaD");
            rweaponVisuals.SetThrowingTexture("AnacondaThrowing");
            rweaponVisuals.SetHolsterTexture("AnacondaH");
            rweaponVisuals.SetSheathedTexture("AnacondaS");
            rweaponVisuals.AnimIdleUpper = "UpperIdleHandgun";
            rweaponVisuals.AnimCrouchUpper = "UpperCrouchHandgun";
            rweaponVisuals.AnimJumpKickUpper = "UpperJumpKickHandgun";
            rweaponVisuals.AnimJumpUpper = "UpperJumpHandgun";
            rweaponVisuals.AnimJumpUpperFalling = "UpperJumpFallingHandgun";
            rweaponVisuals.AnimKickUpper = "UpperKickHandgun";
            rweaponVisuals.AnimStaggerUpper = "UpperStaggerHandgun";
            rweaponVisuals.AnimRunUpper = "UpperRunHandgun";
            rweaponVisuals.AnimWalkUpper = "UpperWalkHandgun";
            rweaponVisuals.AnimUpperHipfire = "UpperHipfireHandgun";
            rweaponVisuals.AnimFireArmLength = 7f;
            rweaponVisuals.AnimDraw = "UpperDrawMagnum";
            rweaponVisuals.AnimManualAim = "ManualAimHandgun";
            rweaponVisuals.AnimManualAimStart = "ManualAimHandgunStart";
            rweaponVisuals.AnimReloadUpper = "UpperReload";
            rweaponVisuals.AnimFullLand = "FullLandHandgun";
            rweaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
            rweaponProperties.VisualText = "Anaconda";
            SetPropertiesAndVisuals(rweaponProperties, rweaponVisuals);
            CacheDrawnTextures(new string[] { "Reload" });
        }

        public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
        {
            if (animEvent == AnimationEvent.EnterFrame && subAnim.GetCurrentFrameIndex() == 1)
            {
                if (player.GameOwner != GameOwnerEnum.Server)
                {
                    if (m_spawnShells)
                    {
                        int num = 5;
                        for (int i = 0; i < num; i++)
                        {
                            SpawnUnsyncedShell(player, "ShellSmall");
                        }
                    }
                    SoundHandler.PlaySound("MagnumReloadStart", player.Position, player.GameWorld);
                }
                m_spawnShells = false;
            }
        }

        public override void GrabAmmo(Player player)
        {
            m_spawnShells = true;
            base.GrabAmmo(player);
        }

        public override void OnSubAnimationEvent(Player player, AnimationEvent animationEvent, AnimationData animationData, int currentFrameIndex)
        {
            if (player.GameOwner != GameOwnerEnum.Server && animationEvent == AnimationEvent.EnterFrame && animationData.Name == "UpperDrawMagnum")
            {
                if (currentFrameIndex == 1)
                {
                    SoundHandler.PlaySound("Draw1", player.GameWorld);
                }
                if (currentFrameIndex == 6)
                {
                    SoundHandler.PlaySound("MagnumDraw", player.GameWorld);
                }
            }
        }

        public override bool CheckDrawLazerAttachment(string subAnimation, int subFrame)
        {
            return subAnimation == null || !(subAnimation == "UpperReload");
        }

        public Anaconda(RWeaponProperties rwp, RWeaponVisuals rwv)
        {
            SetPropertiesAndVisuals(rwp, rwv);
        }

        public override RWeapon Copy()
        {
            Anaconda wpnAnaconda = new(Properties, Visuals);
            wpnAnaconda.CopyStatsFrom(this);
            return wpnAnaconda;
        }

        public override void OnReloadAnimationFinished(Player player)
        {
            m_spawnShells = true;
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
}
