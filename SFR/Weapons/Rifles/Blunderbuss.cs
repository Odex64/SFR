using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Rifles;

internal sealed class Blunderbuss : RWeapon
{
    internal Blunderbuss()
    {
        RWeaponProperties weaponProperties = new(95, "Blunderbuss", "WpnBlunderbuss", false, WeaponCategory.Primary)
        {
            MaxMagsInWeapon = 1,
            MaxRoundsInMag = 1,
            MaxCarriedSpareMags = 7,
            StartMags = 4,
            ProjectilesEachBlast = 10,
            CooldownBeforePostAction = 1500,
            CooldownAfterPostAction = 0,
            ExtraAutomaticCooldown = 200,
            ShellID = string.Empty,
            AccuracyDeflection = 0.3f,
            ProjectileID = 95,
            MuzzlePosition = new Vector2(13f, -2.5f),
            CursorAimOffset = new Vector2(0f, 2.5f),
            LazerPosition = new Vector2(12f, -1.5f),
            MuzzleEffectTextureID = "MuzzleFlashShotgun",
            BlastSoundID = "Explosion",
            DrawSoundID = "CarbineDraw",
            GrabAmmoSoundID = "CarbineReload",
            OutOfAmmoSoundID = "OutOfAmmoHeavy",
            AimStartSoundID = "PistolAim",
            AI_DamageOutput = DamageOutputType.Standard,
            AI_GravityArcingEffect = 0.66f,
            BreakDebris = new[] { "ItemDebrisStockWood00", "ItemDebrisWood00", "ItemDebrisShiny00" },
            SpecialAmmoBulletsRefill = 4
        };

        RWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("BlunderbussM");
        weaponVisuals.SetDrawnTexture("BlunderbussD");
        weaponVisuals.SetSheathedTexture("BlunderbussS");
        weaponVisuals.SetThrowingTexture("BlunderbussThrowing");
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
        weaponVisuals.AnimReloadUpper = "UpperReloadBazooka";
        weaponVisuals.AnimFullLand = "FullLandHandgun";
        weaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
        weaponProperties.VisualText = "Blunderbuss";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private Blunderbuss(RWeaponProperties weaponProperties, RWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    public override RWeapon Copy()
    {
        Blunderbuss wpnBlunderbuss = new(Properties, Visuals);
        wpnBlunderbuss.CopyStatsFrom(this);
        return wpnBlunderbuss;
    }

    public override void ConsumeAmmoFromFire(Player player)
    {
        for (int i = 0; i < 16; i++)
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

    public override void OnRecoilEvent(Player player)
    {
        base.OnRecoilEvent(player);
        Vector2 force = new(player.ScriptBridge.AimVector.X, player.ScriptBridge.AimVector.Y);
        force.Normalize();
        force *= -8;
        force += player.CurrentVelocity * 0.75f;
        player.FallWithSpeed(force);
    }

    public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animEvent == AnimationEvent.EnterFrame)
        {
            if (subAnim.GetCurrentFrameIndex() == 1)
            {
                SoundHandler.PlaySound("MagnumReloadEnd", player.Position, player.GameWorld);
            }
            else if (subAnim.GetCurrentFrameIndex() == 4)
            {
                SoundHandler.PlaySound("CarbineReload", player.Position, player.GameWorld);
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
                    SoundHandler.PlaySound("CarbineDraw", player.GameWorld);
                    break;
            }
        }
    }

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * 0.8f);
        var linearVelocity = thrownWeaponItem.Body.GetLinearVelocity() * 0.85f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
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