using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;
using SFR.Fighter;
using SFR.Misc;
using SFR.Sync.Generic;

namespace SFR.Weapons.Melee;

internal sealed class Sledgehammer : MWeapon, IExtendedWeapon
{
    private const float _heavyDamagePlayer = 35f;
    private const float _heavyDamageObject = 100f;
    private const float _weakDamagePlayer = 8f;
    private const float _weakDamageObject = 50f;
    private const float _heavyChargeTime = 800f;
    private const float _attackCooldown = 300f;
    private const float _weakBlinkTime = 200f;
    private const float _heavyBlinkTime = 450f;
    private float _currentAttackCooldown;
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
            BreakDebris = ["SledgehammerDebris1", "WoodDebris00A"],
            AI_DamageOutput = DamageOutputType.Low,
            VisualText = "Sledgehammer"
        };

        MWeaponVisuals weaponVisuals = new()
        {
            AnimBlockUpper = "UpperBlockMelee2H",
            AnimMeleeAttack1 = "UpperMelee1H1",
            AnimMeleeAttack2 = "UpperMelee1H4",
            AnimMeleeAttack3 = "UpperMelee2H3",
            AnimFullJumpAttack = "FullJumpAttackMelee",
            AnimDraw = "UpperDrawMeleeSheathed",
            AnimCrouchUpper = "UpperCrouchMelee2H",
            AnimIdleUpper = "UpperIdleMelee2H",
            AnimJumpKickUpper = "UpperJumpKickMelee",
            AnimJumpUpper = "UpperJumpMelee2H",
            AnimJumpUpperFalling = "UpperJumpFallingMelee2H",
            AnimKickUpper = "UpperKickMelee2H",
            AnimStaggerUpper = "UpperStagger",
            AnimRunUpper = "UpperRunMelee2H",
            AnimWalkUpper = "UpperWalkMelee2H",
            AnimFullLand = "FullLandMelee",
            AnimToggleThrowingMode = "UpperToggleThrowing"
        };

        weaponVisuals.SetModelTexture("SledgehammerM");
        weaponVisuals.SetDrawnTexture("SledgehammerD");
        weaponVisuals.SetSheathedTexture("SledgeHammerS");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

        CacheDrawnTextures(["Blink"]);
    }

    private Sledgehammer(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public void GetDealtDamage(Player player, float damage)
    {
    }

    public void OnHit(Player player, Player target)
    {
        if (target is not null && player is not null)
        {
            if (player.Position.Y >= target.Position.Y && target.IsDead)
            {
                if (player.CurrentAction == PlayerAction.MeleeAttack3 || player.CurrentAction == PlayerAction.JumpAttack && Globals.Random.Next(2) == 0)
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

    public void Update(Player player, float ms, float realMs)
    {
    }

    public void DrawExtra(SpriteBatch spritebatch, Player player, float ms)
    {
    }

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
        ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, center, 8f, ["SledgehammerDebris1", "WoodDebris00A"]);
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

        if (_currentAttackCooldown > player.GameWorld.ElapsedTotalGameTime)
        {
            return true;
        }

        if (onKeyEvent && player.CurrentAction is PlayerAction.Idle && !_isCharging)
        {
            _isCharging = true;
            _chargedTimer = player.GameWorld.ElapsedTotalGameTime + _heavyChargeTime;
            SyncBlinkTime(player, _weakBlinkTime);
            SoundHandler.PlaySound("LightCharge", player.Position, player.GameWorld);
        }

        return true;
    }

    private void SyncBlinkTime(Player player, float time)
    {
        if (player.GameOwner != GameOwnerEnum.Client)
        {
            BlinkTimer = time;
            GenericData.SendGenericDataToClients(new GenericData(DataType.SledgehammerBlink, [], player.ObjectData.BodyID, time));
        }
    }

    private void SwingTheHammer(Player player, bool isHeavy)
    {
        _isCharging = false;
        _currentAttackCooldown = player.GameWorld.ElapsedTotalGameTime + _attackCooldown;

        if (isHeavy)
        {
            Properties.DamagePlayers = _heavyDamagePlayer;
            Properties.DamageObjects = _heavyDamageObject;
            player.DrainEnergy(10);
            player.CurrentAction = PlayerAction.MeleeAttack3;
            SyncBlinkTime(player, _heavyBlinkTime);
            EffectHandler.PlayEffect("GLM", player.Position + new Vector2(-22f * (player.AimVector().X > 0 ? 1 : -1), 13f), player.GameWorld);
            SoundHandler.PlaySound("HeavyCharge", player.Position, player.GameWorld);
        }
        else
        {
            Properties.DamagePlayers = _weakDamagePlayer;
            Properties.DamageObjects = _weakDamageObject;
            player.CurrentAction = PlayerAction.MeleeAttack2;
        }
    }

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * 0.8f);
        Vector2 linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
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