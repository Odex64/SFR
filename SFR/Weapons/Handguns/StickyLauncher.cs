using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;
using SFR.Helper;
using SFR.Objects;
using Constants = SFR.Misc.Constants;

namespace SFR.Weapons.Handguns;

internal sealed class StickyLauncher : RWeapon, IExtendedWeapon
{
    private readonly List<ObjectStickyProjectile> _stickies = new();
    private bool _ejectShells = true;

    internal StickyLauncher()
    {
        RWeaponProperties weaponProperties = new(86, "Sticky_Launcher", "WpnStickyLauncher", false, WeaponCategory.Secondary)
        {
            MaxMagsInWeapon = 8,
            MaxRoundsInMag = 1,
            MaxCarriedSpareMags = 24,
            StartMags = 32,
            CooldownBeforePostAction = 600,
            CooldownAfterPostAction = 0,
            ExtraAutomaticCooldown = 0,
            ShellID = "",
            AccuracyDeflection = 0.05f,
            ProjectileID = -1,
            MuzzlePosition = new Vector2(8f, -2f),
            MuzzleEffectTextureID = "MuzzleFlashS",
            BlastSoundID = "",
            DrawSoundID = "GLauncherDraw",
            GrabAmmoSoundID = "GLauncherReload",
            OutOfAmmoSoundID = "OutOfAmmoHeavy",
            CursorAimOffset = new Vector2(0f, 3.5f),
            LazerPosition = new Vector2(10f, -0.5f),
            AimStartSoundID = "PistolAim",
            AI_DamageOutput = DamageOutputType.None,
            CanRefilAtAmmoStashes = true,
            AI_MaxRange = 0f,
            BreakDebris = new[]
            {
                "ItemDebrisWood00",
                "ItemDebrisShiny00",
                "MetalDebris00C"
            },
            SpecialAmmoBulletsRefill = 8
        };

        RWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("StickyLauncherM");
        weaponVisuals.SetDrawnTexture("StickyLauncherD");
        weaponVisuals.SetSheathedTexture("StickyLauncherS");
        weaponVisuals.SetThrowingTexture("StickyLauncherThrowing");
        weaponVisuals.AnimIdleUpper = "UpperIdleRifle";
        weaponVisuals.AnimCrouchUpper = "UpperCrouchRifle";
        weaponVisuals.AnimJumpKickUpper = "UpperJumpKickRifle";
        weaponVisuals.AnimJumpUpper = "UpperJumpRifle";
        weaponVisuals.AnimJumpUpperFalling = "UpperJumpFallingRifle";
        weaponVisuals.AnimKickUpper = "UpperKickRifle";
        weaponVisuals.AnimStaggerUpper = "UpperStaggerRifle";
        weaponVisuals.AnimRunUpper = "UpperRunRifle";
        weaponVisuals.AnimWalkUpper = "UpperWalkRifle";
        weaponVisuals.AnimUpperHipfire = "UpperHipfireHandgun";
        weaponVisuals.AnimFireArmLength = 7f;
        weaponVisuals.AnimDraw = "UpperDrawRifle";
        weaponVisuals.AnimManualAim = "ManualAimRifle";
        weaponVisuals.AnimManualAimStart = "ManualAimRifleStart";
        weaponVisuals.AnimReloadUpper = "UpperReload";
        weaponVisuals.AnimFullLand = "FullLandHandgun";
        weaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
        weaponProperties.VisualText = "Sticky Launcher";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
        CacheDrawnTextures(new[] { "Reload" });
    }

    private StickyLauncher(RWeaponProperties weaponProperties, RWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    public void Update(Player player, float ms, float realMs)
    {
        if (player.VirtualKeyboard.PressingKey(14))
        {
            foreach (var s in GetRealStickies())
            {
                if (s.TimeStamp < player.GameWorld.ElapsedTotalGameTime - 600)
                {
                    s.Detonate();
                }
            }
        }
    }

    public void GetDealtDamage(Player player, float damage) { }

    public void OnHit(Player player, Player target) { }

    public void OnHitObject(Player player, PlayerHitEventArgs args, ObjectData obj) { }

    public void DrawExtra(SpriteBatch spritebatch, Player player, float ms) { }

    public override void GrabAmmo(Player player)
    {
        _ejectShells = true;
        base.GrabAmmo(player);
    }

    public override void BeforeCreateProjectile(BeforeCreateProjectileArgs args)
    {
        var pipe = (ObjectStickyProjectile)ObjectData.CreateNew(new ObjectDataStartParams(args.GameWorld.IDCounter.NextID(), 0, 0, "ProjectileSticky", args.GameWorld.GameOwner));
        args.GameWorld.CreateTile(new SpawnObjectInformation(pipe, args.WorldPosition, 0, 1, args.Direction.GetRotatedVector(0.2f * args.Player.LastDirectionX * (1 - args.Direction.Y)) * 9, (float)Constants.Random.NextDouble()));
        _stickies.Add(pipe);

        // if (args.GameWorld.GameOwner == GameOwnerEnum.Server)
        // {
        //     GenericData.SendGenericDataToClients(new GenericData(DataType.StickyLauncher, pipe.ObjectID, pipe.GetWorldPosition().X, pipe.GetWorldPosition().Y, pipe.GetAngle()));
        // }

        args.Handled = true;
        args.FireResult = true;
        SoundHandler.PlaySound("GLFire", args.Player.GameWorld);
    }

    public override void OnSubAnimationEvent(Player player, AnimationEvent animationEvent, AnimationData animationData, int currentFrameIndex)
    {
        if (player.GameOwner != GameOwnerEnum.Server && animationEvent == AnimationEvent.EnterFrame)
        {
            if (animationData.Name == "UpperDrawRifle")
            {
                switch (currentFrameIndex)
                {
                    case 1:
                        SoundHandler.PlaySound("Draw1", player.GameWorld);
                        break;
                    case 6:
                        SoundHandler.PlaySound("GLauncherDraw", player.GameWorld);
                        break;
                }
            }
        }
    }

    public override bool CheckDrawLazerAttachment(string subAnimation, int subFrame) => subAnimation is not "UpperReload";

    public override RWeapon Copy()
    {
        StickyLauncher stickyLauncher = new(Properties, Visuals);
        stickyLauncher.CopyStatsFrom(this);
        return stickyLauncher;
    }

    public override void OnReloadAnimationEvent(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
    {
        if (animEvent == AnimationEvent.EnterFrame)
        {
            if (subAnim.GetCurrentFrameIndex() == 1)
            {
                if (player.GameOwner != GameOwnerEnum.Server)
                {
                    if (_ejectShells)
                    {
                        SpawnUnsyncedShell(player, "ShellGLauncher");
                    }

                    SoundHandler.PlaySound("MagnumReloadStart", player.Position, player.GameWorld);
                }

                _ejectShells = false;
            }
            else
            {
                _ejectShells = true;
            }
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

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * 0.6f);
        var linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
        linearVelocity.X *= 0.8f;
        linearVelocity.Y *= 0.8f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }

    private IEnumerable<ObjectStickyProjectile> GetRealStickies()
    {
        return _stickies.Where(s => s is { RemovalInitiated: false, Detonated: false }).ToArray();
    }
}