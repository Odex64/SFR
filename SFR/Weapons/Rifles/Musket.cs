using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Rifles;

internal sealed class Musket : RWeapon
{
    internal Musket()
    {
        RWeaponProperties weaponProperties = new(98, "Musket", "WpnMusket", false, WeaponCategory.Primary)
        {
            MaxMagsInWeapon = 1,
            MaxRoundsInMag = 1,
            MaxCarriedSpareMags = 7,
            StartMags = 3,
            CooldownBeforePostAction = 400,
            CooldownAfterPostAction = 0,
            ExtraAutomaticCooldown = 200,
            ShellID = string.Empty,
            AccuracyDeflection = 0.09f,
            ProjectileID = 98,
            MuzzlePosition = new Vector2(13f, -2.5f),
            CursorAimOffset = new Vector2(0f, 2.5f),
            LazerPosition = new Vector2(12f, -1.5f),
            MuzzleEffectTextureID = "MuzzleFlashS",
            BlastSoundID = "Carbine",
            DrawSoundID = "CarbineDraw",
            GrabAmmoSoundID = "CarbineReload",
            OutOfAmmoSoundID = "OutOfAmmoHeavy",
            AimStartSoundID = "PistolAim",
            AI_DamageOutput = DamageOutputType.Standard,
            AI_GravityArcingEffect = 0.66f,
            AI_EffectiveRange = 200,
            BreakDebris =
            [
                "ItemDebrisStockWood00",
                "ItemDebrisWood00",
                "ItemDebrisShiny00"
            ],
            SpecialAmmoBulletsRefill = 8,
            VisualText = "Musket"
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
            AnimReloadUpper = "UpperReloadBazooka",
            AnimFullLand = "FullLandHandgun",
            AnimToggleThrowingMode = "UpperToggleThrowing"
        };

        weaponVisuals.SetModelTexture("MusketM");
        weaponVisuals.SetDrawnTexture("MusketD");
        weaponVisuals.SetSheathedTexture("MusketS");
        weaponVisuals.SetThrowingTexture("MusketThrowing");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

        CacheDrawnTextures(["Reload"]);
    }

    private Musket(RWeaponProperties weaponProperties, RWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public override RWeapon Copy()
    {
        Musket wpnMusket = new(Properties, Visuals);
        wpnMusket.CopyStatsFrom(this);
        return wpnMusket;
    }

    public override void ConsumeAmmoFromFire(Player player)
    {
        for (int i = 0; i < 12; i++)
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
        if (player.GameOwner != GameOwnerEnum.Server)
        {
            if (animEvent == AnimationEvent.EnterFrame)
            {
                if (subAnim.GetCurrentFrameIndex() == 1)
                {
                    SoundHandler.PlaySound("MagnumReloadEnd", player.Position, player.GameWorld);
                }
            }

            if (animEvent == AnimationEvent.EnterFrame && subAnim.GetCurrentFrameIndex() == 4)
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
        Vector2 linearVelocity = thrownWeaponItem.Body.GetLinearVelocity() * 0.85f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }

    public override Texture2D GetDrawnTexture(ref GetDrawnTextureArgs args)
    {
        string subAnimation;
        if ((subAnimation = args.SubAnimation) is not null && subAnimation == "UpperReloadBazooka")
        {
            if (args.SubFrame is >= 1 and <= 5)
            {
                args.Postfix = "Reload";
            }
        }

        return base.GetDrawnTexture(ref args);
    }
}