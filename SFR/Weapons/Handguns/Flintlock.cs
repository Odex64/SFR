using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Handguns;

internal sealed class Flintlock : RWeapon
{
    internal Flintlock()
    {
        RWeaponProperties weaponProperties = new(69, "Flintlock", 1, 1, 10, 6, 1000, 1000, 0, 1, 69, string.Empty, 0.2f, new Vector2(12f, -2f), "MuzzleFlashShotgun", "SawedOff", "MagnumDraw", "MagnumReload", "OutOfAmmoHeavy", "WpnFlintlock", false, WeaponCategory.Secondary)
        {
            CooldownAfterPostAction = 1000,
            CursorAimOffset = new Vector2(0f, 1f),
            LazerPosition = new Vector2(6f, -0.5f),
            AimStartSoundID = "PistolAim",
            AI_DamageOutput = DamageOutputType.High,
            BreakDebris = new[] { "MetalDebris00A" },
            SpecialAmmoBulletsRefill = 3
        };

        RWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("FlintlockM");
        weaponVisuals.SetDrawnTexture("FlintlockD");
        weaponVisuals.SetThrowingTexture("FlintlockThrowing");
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
        weaponVisuals.AnimReloadUpper = "UpperReloadBazooka";
        weaponVisuals.AnimFullLand = "FullLandHandgun";
        weaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
        weaponProperties.VisualText = "Flintlock";
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
        CacheDrawnTextures(new[] { "Reload" });
    }

    private Flintlock(RWeaponProperties weaponProperties, RWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    public override RWeapon Copy()
    {
        Flintlock wpnFlintlock = new(Properties, Visuals);
        wpnFlintlock.CopyStatsFrom(this);
        return wpnFlintlock;
    }

    public override void ConsumeAmmoFromFire(Player player)
    {
        for (int i = 0; i < 8; i++)
        {
            if (player.CurrentAction == PlayerAction.ManualAim)
            {
                EffectHandler.PlayEffect("TR_S", player.Position + player.AimVector() * 16 + new Vector2(0f, player.Crouching ? 11f : 16f), player.GameWorld);
            }
            else
            {
                EffectHandler.PlayEffect("TR_S", player.Position + new Vector2(20f * player.LastDirectionX, player.Crouching ? 11f : 16f), player.GameWorld);
            }
        }

        base.ConsumeAmmoFromFire(player);
    }

    public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animEvent == AnimationEvent.EnterFrame)
        {
            if (subAnim.GetCurrentFrameIndex() == 3)
            {
                SoundHandler.PlaySound("MagnumReload", player.Position, player.GameWorld);
                for (int i = 0; i < 3; i++)
                {
                    EffectHandler.PlayEffect("TR_S", player.Position + new Vector2(6f * player.LastDirectionX, player.Crouching ? 11f : 16f), player.GameWorld);
                }
            }
        }
    }

    public override void OnSubAnimationEvent(Player player, AnimationEvent animationEvent, AnimationData animationData, int currentFrameIndex)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animationEvent == AnimationEvent.EnterFrame && animationData.Name == "UpperDrawMagnum")
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

    public override Texture2D GetDrawnTexture(ref GetDrawnTextureArgs args)
    {
        //Show custom animation
        string subAnimation;
        if ((subAnimation = args.SubAnimation) != null && subAnimation == "UpperReloadBazooka")
        {
            if (args.SubFrame is >= 1 and <= 5)
            {
                args.Postfix = "Reload";
            }
        }

        return base.GetDrawnTexture(ref args);
    }
}