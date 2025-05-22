using Microsoft.Xna.Framework;
using SFD;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Handguns;

internal sealed class NailGun : RWeapon
{
    internal NailGun()
    {
        RWeaponProperties weaponProperties = new(70, "Nailgun", 1, 18, 4, 2, -1, 150, 0, 1, 70, string.Empty, 0.13f, new Vector2(8f, -2f), string.Empty, "OutOfAmmoLight", "PistolDraw", "PistolReload", "OutOfAmmoHeavy", "WpnNailgun", false, WeaponCategory.Secondary)
        {
            SpecialAmmoBulletsRefill = 18,
            LazerPosition = new Vector2(7f, -0.5f),
            CursorAimOffset = new Vector2(0f, 4f),
            AimStartSoundID = "PistolAim",
            BreakDebris = ["ItemDebrisDark00", "ItemDebrisDark01"],
            AI_DamageOutput = DamageOutputType.High,
            VisualText = "Nailgun"
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
            AnimDraw = "UpperDrawHandgun",
            AnimManualAim = "ManualAimHandgun",
            AnimManualAimStart = "ManualAimHandgunStart",
            AnimReloadUpper = "UpperReload",
            AnimFullLand = "FullLandHandgun",
            AnimToggleThrowingMode = "UpperToggleThrowing"
        };

        weaponVisuals.SetModelTexture("NailgunM");
        weaponVisuals.SetDrawnTexture("NailgunD");
        weaponVisuals.SetThrowingTexture("NailgunThrowing");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

        CacheDrawnTextures(["Reload"]);
    }

    private NailGun(RWeaponProperties weaponProperties, RWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
    {
        if (player.GameOwner != GameOwnerEnum.Server)
        {
            if (animEvent == AnimationEvent.EnterFrame && subAnim.GetCurrentFrameIndex() == 1)
            {
                SpawnMagazine(player, "MagSmall", new Vector2(-6f, -5f));
                SoundHandler.PlaySound("MagnumReloadEnd", player.Position, player.GameWorld);
            }

            if (animEvent == AnimationEvent.EnterFrame && subAnim.GetCurrentFrameIndex() == 4)
            {
                SoundHandler.PlaySound("PistolReload", player.Position, player.GameWorld);
            }
        }
    }

    public override RWeapon Copy()
    {
        NailGun wpnMachinePistol = new(Properties, Visuals);
        wpnMachinePistol.CopyStatsFrom(this);
        return wpnMachinePistol;
    }

    public override void OnRecoilEvent(Player player)
    {
        if (player.GameOwner != GameOwnerEnum.Server)
        {
            if (Properties.ShellID != string.Empty && Constants.EFFECT_LEVEL_FULL)
            {
                SpawnUnsyncedShell(player, Properties.ShellID);
            }

            SoundHandler.PlaySound(Properties.BlastSoundID, player.Position, player.GameWorld);
        }
    }

    public override void OnSubAnimationEvent(Player player, AnimationEvent animationEvent, AnimationData animationData, int currentFrameIndex)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animationEvent == AnimationEvent.EnterFrame)
        {
            if (animationData.Name == "UpperDrawHandgun")
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
    }

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        Vector2 linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
        linearVelocity.X *= 1.2f;
        linearVelocity.Y *= 1f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
    }
}