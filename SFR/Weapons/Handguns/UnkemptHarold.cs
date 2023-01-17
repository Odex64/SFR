using Microsoft.Xna.Framework;
using SFD;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Handguns;

internal sealed class UnkemptHarold : RWeapon
{
    private bool _ejectShells = true;

    internal UnkemptHarold()
    {
        RWeaponProperties weaponProperties = new(85, "Unkempt_Harold", "WpnUnkemptHarold", false, WeaponCategory.Secondary)
        {
            MaxMagsInWeapon = 1,
            MaxRoundsInMag = 6,
            MaxCarriedSpareMags = 4,
            StartMags = 2,
            CooldownBeforePostAction = 1000,
            CooldownAfterPostAction = 0,
            ExtraAutomaticCooldown = 200,
            ShellID = "",
            AccuracyDeflection = 0.06f,
            ProjectileID = 85,
            MuzzlePosition = new Vector2(5f, -3f),
            MuzzleEffectTextureID = "MuzzleFlashS",
            BlastSoundID = "Magnum",
            DrawSoundID = "MagnumDraw",
            GrabAmmoSoundID = "MagnumReload",
            OutOfAmmoSoundID = "OutOfAmmoHeavy",
            CursorAimOffset = new Vector2(0f, 3.5f),
            LazerPosition = new Vector2(6f, -0.5f),
            AimStartSoundID = "PistolAim",
            AI_DamageOutput = DamageOutputType.High,
            AI_HasOneShotPotential = true,
            BreakDebris = new[] { "ItemDebrisShiny01" },
            SpecialAmmoBulletsRefill = 18
        };

        RWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("UnkemptHaroldM");
        weaponVisuals.SetDrawnTexture("UnkemptHaroldD");
        weaponVisuals.SetSheathedTexture("UnkemptHaroldS");
        weaponVisuals.SetThrowingTexture("UnkemptHaroldThrowing");
        weaponVisuals.AnimIdleUpper = "UpperIdleHandgun";
        weaponVisuals.AnimCrouchUpper = "UpperCrouchHandgun";
        weaponVisuals.AnimJumpKickUpper = "UpperJumpKickHandgun";
        weaponVisuals.AnimJumpUpper = "UpperJumpHandgun";
        weaponVisuals.AnimJumpUpperFalling = "UpperJumpFallingHandgun";
        weaponVisuals.AnimKickUpper = "UpperKickHandgun";
        weaponVisuals.AnimStaggerUpper = "UpperStaggerHandgun";
        weaponVisuals.AnimRunUpper = "UpperRunHandgun";
        weaponVisuals.AnimWalkUpper = "UpperWalkHandgun";
        weaponVisuals.AnimUpperHipfire = "UpperHipfireHandgun";
        weaponVisuals.AnimFireArmLength = 7f;
        weaponVisuals.AnimDraw = "UpperDrawMagnum";
        weaponVisuals.AnimManualAim = "ManualAimHandgun";
        weaponVisuals.AnimManualAimStart = "ManualAimHandgunStart";
        weaponVisuals.AnimReloadUpper = "UpperReload";
        weaponVisuals.AnimFullLand = "FullLandHandgun";
        weaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
        weaponProperties.VisualText = "Unkempt Harold";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private UnkemptHarold(RWeaponProperties weaponProperties, RWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    public override void GrabAmmo(Player player)
    {
        _ejectShells = true;
        base.GrabAmmo(player);
    }

    public override void OnSubAnimationEvent(Player player, AnimationEvent animationEvent, AnimationData animationData, int currentFrameIndex)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animationEvent == AnimationEvent.EnterFrame)
        {
            if (animationData.Name == "UpperDrawMagnum")
            {
                switch (currentFrameIndex)
                {
                    case 1:
                        SoundHandler.PlaySound("Draw1", player.GameWorld);
                        break;
                    case 6:
                        SoundHandler.PlaySound("MagnumDraw", player.GameWorld);
                        break;
                }
            }
        }
    }

    public override bool CheckDrawLazerAttachment(string subAnimation, int subFrame) => subAnimation is not "UpperReload";

    public override RWeapon Copy()
    {
        UnkemptHarold unkemptHarold = new(Properties, Visuals);
        unkemptHarold.CopyStatsFrom(this);
        return unkemptHarold;
    }

    public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
    {
        if (animEvent == AnimationEvent.EnterFrame && subAnim.GetCurrentFrameIndex() == 1)
        {
            if (player.GameOwner != GameOwnerEnum.Server)
            {
                if (_ejectShells)
                {
                    const int num = 6;
                    for (int i = 0; i < num; i++)
                    {
                        SpawnUnsyncedShell(player, "ShellSmall");
                    }
                }

                SoundHandler.PlaySound("MagnumReloadStart", player.Position, player.GameWorld);
            }

            _ejectShells = false;
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
}