using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;
using SFR.Fighter;
using SFR.Sync.Generic;
using Constants = SFR.Misc.Constants;

namespace SFR.Weapons.Melee;

internal sealed class Sledgehammer : MWeapon, IExtendedWeapon
{
    private const float HeavyDamagePlayer = 35f;
    private const float HeavyDamageObject = 100f;
    private const float WeakDamagePlayer = 8f;
    private const float WeakDamageObject = 50f;
    private const float HeavyChargeTime = 800f;
    private const float AttackCooldown = 300f;
    private const float WeakBlinkTime = 200f;
    private const float HeavyBlinkTime = 450f;
    private float _attackCooldown;
    private float _chargedTimer;
    private bool _isCharging;
    internal float BlinkTimer;

    internal Sledgehammer()
    {
        MWeaponProperties weaponProperties = new(83, "Sledgehammer", 14f, 8f, "Sledgehammer", "MeleeHitBlunt", "HIT_B", "MeleeBlock", "HIT", "MeleeDraw", "WpnSledgehammer", false, WeaponCategory.Melee, false)
        {
            DamageObjects = 30f,
            MeleeWeaponType = MeleeWeaponTypeEnum.TwoHanded,
            Handling = MeleeHandlingType.Custom,
            WeaponMaterial = MaterialDatabase.Get("metal"),
            DurabilityLossOnHitObjects = 4f,
            DurabilityLossOnHitPlayers = 8f,
            DurabilityLossOnHitBlockingPlayers = 4f,
            ThrownDurabilityLossOnHitPlayers = 8f,
            ThrownDurabilityLossOnHitBlockingPlayers = 6f,
            DeflectionDuringBlock =
            {
                DeflectType = DeflectBulletType.Deflect,
                DurabilityLoss = 4f
            },
            DeflectionOnAttack =
            {
                DeflectType = DeflectBulletType.Deflect,
                DurabilityLoss = 4f
            },
            BreakDebris = new[] { "SledgehammerDebris1", "WoodDebris00A" },
            AI_DamageOutput = DamageOutputType.Low
        };

        MWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("SledgehammerM");
        weaponVisuals.SetDrawnTexture("SledgehammerD");
        weaponVisuals.SetSheathedTexture("SledgeHammerS");
        weaponVisuals.AnimBlockUpper = "UpperBlockMelee2H";
        weaponVisuals.AnimMeleeAttack1 = "UpperMelee1H1";
        weaponVisuals.AnimMeleeAttack2 = "UpperMelee1H4";
        weaponVisuals.AnimMeleeAttack3 = "UpperMelee2H3";
        weaponVisuals.AnimFullJumpAttack = "FullJumpAttackMelee";
        weaponVisuals.AnimDraw = "UpperDrawMeleeSheathed";
        weaponVisuals.AnimCrouchUpper = "UpperCrouchMelee2H";
        weaponVisuals.AnimIdleUpper = "UpperIdleMelee2H";
        weaponVisuals.AnimJumpKickUpper = "UpperJumpKickMelee";
        weaponVisuals.AnimJumpUpper = "UpperJumpMelee2H";
        weaponVisuals.AnimJumpUpperFalling = "UpperJumpFallingMelee2H";
        weaponVisuals.AnimKickUpper = "UpperKickMelee2H";
        weaponVisuals.AnimStaggerUpper = "UpperStagger";
        weaponVisuals.AnimRunUpper = "UpperRunMelee2H";
        weaponVisuals.AnimWalkUpper = "UpperWalkMelee2H";
        weaponVisuals.AnimFullLand = "FullLandMelee";
        weaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
        weaponProperties.VisualText = "Sledgehammer";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
        CacheDrawnTextures(new[] { "Blink" });
    }

    private Sledgehammer(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    public void GetDealtDamage(Player player, float damage) { }

    public void OnHit(Player player, Player target)
    {
        if (target != null && player != null)
        {
            if (player.Position.Y >= target.Position.Y && target.IsDead)
            {
                if (player.CurrentAction == PlayerAction.MeleeAttack3 || (player.CurrentAction == PlayerAction.JumpAttack && Constants.Random.Next(2) == 0))
                {
                    GoreHandler.ApplyHeadshot(target, target.Position + new Vector2(0, 18));
                }
            }
        }
    }

    public void OnHitObject(Player player, PlayerHitEventArgs args, ObjectData obj)
    {
        // On heavy attack, detonate explosives
        if (obj is ObjectExplosive or ObjectBarrelExplosive && player.CurrentAction == PlayerAction.MeleeAttack3)
        {
            ((ObjectDestructible)obj).Properties.Get(ObjectPropertyID.BarrelExplosive_Exploding).Value = true;
            if (obj is ObjectExplosive explosive)
            {
                explosive.time = 80f;
            }
            else
            {
                ((ObjectBarrelExplosive)obj).time = 80f;
            }
        }
    }

    public void Update(Player player, float ms, float realMs) { }
    public void DrawExtra(SpriteBatch spritebatch, Player player, float ms) { }

    public override MWeapon Copy() => new Sledgehammer(Properties, Visuals)
    {
        Durability =
        {
            CurrentValue = Durability.CurrentValue
        }
    };

    public override void Destroyed(Player ownerPlayer)
    {
        EffectHandler.PlayEffect("DestroyWood", ownerPlayer.Position, ownerPlayer.GameWorld);
        Vector2 center = new(ownerPlayer.Position.X, ownerPlayer.Position.Y + 16f);
        ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, center, 8f, new[] { "SledgehammerDebris1", "WoodDebris00A" });
    }

    public override void CustomHandlingPostUpdate(Player player, float totalMs)
    {
        if (player.GameOwner == GameOwnerEnum.Client)
        {
            return;
        }

        if (player.LayingOnGround)
        {
            _chargedTimer = 0f;
            _isCharging = false;
        }
        else if (_isCharging)
        {
            if (player.VirtualKeyboard.PressingKey(4, true))
            {
                _chargedTimer -= totalMs;
                if (_chargedTimer <= player.GameWorld.ElapsedTotalGameTime)
                {
                    SwingTheHammer(player, true);
                }
            }
            else
            {
                SwingTheHammer(player, false);
            }
        }
    }

    public override bool CustomHandlingOnAttackKey(Player player, bool onKeyEvent)
    {
        if (player.GameOwner == GameOwnerEnum.Client)
        {
            return true;
        }

        if (_attackCooldown > player.GameWorld.ElapsedTotalGameTime)
        {
            return true;
        }

        if (onKeyEvent && player.CurrentAction is PlayerAction.Idle && !_isCharging)
        {
            _isCharging = true;
            _chargedTimer = player.GameWorld.ElapsedTotalGameTime + HeavyChargeTime;
            SyncBlinkTime(player, WeakBlinkTime);
            SoundHandler.PlaySound("LightCharge", player.Position, player.GameWorld);
        }

        return true;
    }

    private void SyncBlinkTime(Player player, float time)
    {
        if (player.GameOwner != GameOwnerEnum.Client)
        {
            BlinkTimer = time;
            GenericData.SendGenericDataToClients(new GenericData(DataType.SledgehammerBlink, new SyncFlag[] { }, player.ObjectData.BodyID, time));
        }
    }

    private void SwingTheHammer(Player player, bool isHeavy)
    {
        _isCharging = false;
        _attackCooldown = player.GameWorld.ElapsedTotalGameTime + AttackCooldown;

        if (isHeavy)
        {
            Properties.DamagePlayers = HeavyDamagePlayer;
            Properties.DamageObjects = HeavyDamageObject;
            player.DrainEnergy(10);
            player.CurrentAction = PlayerAction.MeleeAttack3;
            SyncBlinkTime(player, HeavyBlinkTime);
            EffectHandler.PlayEffect("GLM", player.Position + new Vector2(-22f * (player.AimVector().X > 0 ? 1 : -1), 13f), player.GameWorld);
            SoundHandler.PlaySound("HeavyCharge", player.Position, player.GameWorld);
        }
        else
        {
            Properties.DamagePlayers = WeakDamagePlayer;
            Properties.DamageObjects = WeakDamageObject;
            player.CurrentAction = PlayerAction.MeleeAttack2;
        }
    }

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * 0.8f);
        var linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
        linearVelocity.X *= 0.8f;
        linearVelocity.Y *= 0.8f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }

    public override Texture2D GetDrawnTexture(ref GetDrawnTextureArgs args)
    {
        if (BlinkTimer > args.TimeMs)
        {
            BlinkTimer -= args.TimeMs;
            args.Postfix = "Blink";
        }

        return base.GetDrawnTexture(ref args);
    }
}