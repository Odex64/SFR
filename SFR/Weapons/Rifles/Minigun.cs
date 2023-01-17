using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;
using SFR.Sync.Generic;

namespace SFR.Weapons.Rifles;

internal sealed class Minigun : RWeapon, IExtendedWeapon
{
    private const int RevUpRounds = 50;
    private const float AfterFireThreshold = 1000;
    private bool _clientRevUp;
    private string _revState;
    private int _revUpCurrent;
    private float _soundTimeStamp;
    private bool _synced;
    private float _timeStamp;

    internal Minigun()
    {
        RWeaponProperties weaponProperties = new(102, "Minigun", 1, 200, 0, 1, -1, 25, 0, 1, 102, "ShellSmall", 0.3f, new Vector2(15f, 1f), "MuzzleFlashL", "M60", "TommyGunDraw", "TommyGunReload", "OutOfAmmoHeavy", "WpnMinigun", false, WeaponCategory.Primary)
        {
            CursorAimOffset = new Vector2(0f, 1f),
            LazerPosition = new Vector2(14f, -0.5f),
            AimStartSoundID = "PistolAim",
            BreakDebris = new[]
            {
                "MetalDebris00A",
                "ItemDebrisShiny00"
            },
            SpecialAmmoBulletsRefill = 200,
            AI_DamageOutput = DamageOutputType.High,
            AI_EffectiveRange = 80,
            AI_MaxRange = 200
        };

        RWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("MinigunM");
        weaponVisuals.SetDrawnTexture("MinigunD");
        weaponVisuals.SetSheathedTexture("MinigunS");
        weaponVisuals.SetThrowingTexture("MinigunThrowing");
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
        weaponVisuals.AnimReloadUpper = "UpperReload";
        weaponVisuals.AnimFullLand = "FullLandHandgun";
        weaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
        weaponProperties.VisualText = "Minigun";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
        CacheDrawnTextures(new[] { "F" });
    }

    private Minigun(RWeaponProperties weaponProperties, RWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    public void Update(Player player, float ms, float realMs)
    {
        if (player.GameOwner != GameOwnerEnum.Client)
        {
            if (player.GameWorld.ElapsedTotalRealTime > _soundTimeStamp)
            {
                if (_revState == "MinigunSpin")
                {
                    SoundHandler.PlaySound("MinigunSpin", player.GameWorld);
                    _soundTimeStamp = player.GameWorld.ElapsedTotalRealTime + 200;
                    _revState = string.Empty;
                }
                else if (_timeStamp + AfterFireThreshold / 2 < player.GameWorld.ElapsedTotalGameTime && _timeStamp + AfterFireThreshold > player.GameWorld.ElapsedTotalGameTime)
                {
                    SoundHandler.PlaySound("MinigunDown", player.GameWorld);
                    _soundTimeStamp = player.GameWorld.ElapsedTotalRealTime + 500;
                    _revState = string.Empty;
                }
            }
        }
    }

    public void GetDealtDamage(Player player, float damage) { }

    public void OnHit(Player player, Player target) { }

    public void OnHitObject(Player player, PlayerHitEventArgs args, ObjectData obj) { }

    public void DrawExtra(SpriteBatch spritebatch, Player player, float ms) { }

    public void BeforeHit(Player player, Player target) { }

    public override RWeapon Copy()
    {
        Minigun wpnMinigun = new(Properties, Visuals);
        wpnMinigun.CopyStatsFrom(this);
        return wpnMinigun;
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
        var linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
        linearVelocity.X *= 0.7f;
        linearVelocity.Y *= 0.7f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }

    public override void ConsumeAmmoFromFire(Player player)
    {
        if (_revUpCurrent >= RevUpRounds)
        {
            base.ConsumeAmmoFromFire(player);
        }
    }

    public override void BeforeCreateProjectile(BeforeCreateProjectileArgs args)
    {
        if (_timeStamp + AfterFireThreshold < args.Player.GameWorld.ElapsedTotalGameTime)
        {
            _revUpCurrent = 0;
            _synced = false;
            GenericData.SendGenericDataToClients(new GenericData(DataType.Minigun, args.Player.ObjectID, "SYNC_MINIGUN_UNREV"));
            // args.Player.ObjectData.SyncedMethod(new ObjectDataSyncedMethod(ObjectDataSyncedMethod.Methods.AnimationSetFrame, args.Player.GameWorld.ElapsedTotalGameTime, "SYNC_MINIGUN_UNREV"));
        }

        if (_revUpCurrent >= RevUpRounds)
        {
            if (!_synced)
            {
                _synced = true;
                GenericData.SendGenericDataToClients(new GenericData(DataType.Minigun, args.Player.ObjectID, "SYNC_MINIGUN_REVUP"));
                // args.Player.ObjectData.SyncedMethod(new ObjectDataSyncedMethod(ObjectDataSyncedMethod.Methods.AnimationSetFrame, args.Player.GameWorld.ElapsedTotalGameTime, "SYNC_MINIGUN_REVUP"));
            }

            Properties.MuzzleEffectTextureID = "MuzzleFlashL";
            SoundHandler.PlaySound("M60", args.Player.GameWorld);
            base.BeforeCreateProjectile(args);
            _revUpCurrent++;
            _revState = "MinigunSpin";
        }
        else
        {
            args.Handled = true;
            Properties.MuzzleEffectTextureID = string.Empty;
            _revUpCurrent++;
            if (_revUpCurrent == 1)
            {
                SoundHandler.PlaySound("MinigunUp", args.Player.GameWorld);
                _soundTimeStamp = args.Player.GameWorld.ElapsedTotalRealTime + 500;
            }
            else
            {
                _revState = "MinigunSpin";
            }
        }

        _timeStamp = args.Player.GameWorld.ElapsedTotalGameTime;
    }

    public override void OnRecoilEvent(Player player)
    {
        if (_revUpCurrent >= RevUpRounds && _timeStamp + AfterFireThreshold > player.GameWorld.ElapsedTotalGameTime && player.GameOwner != GameOwnerEnum.Client)
        {
            base.OnRecoilEvent(player);
        }

        if (player.GameOwner == GameOwnerEnum.Client && _clientRevUp)
        {
            base.OnRecoilEvent(player);
        }
    }

    public override Texture2D GetDrawnTexture(ref GetDrawnTextureArgs args)
    {
        if (_revUpCurrent >= RevUpRounds)
        {
            if (_revUpCurrent % 2 == 0)
            {
                args.Postfix = "F";
            }
        }

        return base.GetDrawnTexture(ref args);
    }

    public void ClientSyncRev(bool value)
    {
        _clientRevUp = value;
    }
}