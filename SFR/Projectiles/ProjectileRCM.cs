using SFD;
using SFD.Effects;
using SFD.Projectiles;
using SFD.Sounds;
using SFD.Tiles;
using SFDGameScriptInterface;
using ProjectileProperties = SFD.Projectiles.ProjectileProperties;
using WeaponItemType = SFD.Weapons.WeaponItemType;

namespace SFR.Projectiles;

internal sealed class ProjectileRCM : ProjectileBazooka
{
    private const float MaxDistance = 2000;
    private float _effectTimer;
    private bool _normalRocket;

    internal ProjectileRCM()
    {
        Visuals = new ProjectileVisuals(Textures.GetTexture("RCMRocket"), Textures.GetTexture("RCMRocket"));
        Properties = new ProjectileProperties(100, 300f, 0f, 10f, 10f, 0f, 0f, 15f, 0.5f)
        {
            DodgeChance = 0f,
            CanBeAbsorbedOrBlocked = false,
            PowerupTotalBounces = 3,
            PowerupBounceRandomAngle = 0f,
            PowerupFireType = ProjectilePowerupFireType.Fireplosion,
            PowerupFireIgniteValue = 56f
        };
    }

    private ProjectileRCM(ProjectileProperties projectileProperties, ProjectileVisuals projectileVisuals) : base(projectileProperties, projectileVisuals) { }

    public override Projectile Copy()
    {
        Projectile projectile = new ProjectileRCM(Properties, Visuals);
        projectile.CopyBaseValuesFrom(this);
        return projectile;
    }

    public override void Update(float ms)
    {
        if (_normalRocket)
        {
            base.Update(ms);
        }
        else if (PlayerOwner != null)
        {
            if (OwnerCanControl())
            {
                var velocity = Velocity;
                var keyboard = PlayerOwner.VirtualKeyboard;
                if (keyboard!.PressingKey(2)) //Left
                {
                    SFDMath.RotatePosition(ref velocity, 0.009f * ms, out velocity);
                }
                else if (keyboard.PressingKey(3)) //Right
                {
                    SFDMath.RotatePosition(ref velocity, -0.009f * ms, out velocity);
                }

                Velocity = velocity;
                ImportantUpdate = true;

                if (keyboard.PressingKey(13) || TotalDistanceTraveled >= MaxDistance)
                {
                    ConvertToNormal(true);
                    return;
                }

                if (GameWorld.GameOwner != GameOwnerEnum.Server)
                {
                    _effectTimer -= ms;
                    if (_effectTimer < 0)
                    {
                        EffectHandler.PlayEffect("FNFTST", Position, GameWorld, Direction.X, Direction.Y); //TR_F
                        _effectTimer = Constants.EFFECT_LEVEL_FULL ? 10f : 20f;
                    }
                }
            }
            else
            {
                ConvertToNormal();
            }
        }
    }

    private bool OwnerCanControl() => PlayerOwner is
    {
        IsDead: false, CurrentWeaponDrawn: WeaponItemType.Rifle, CurrentAction: PlayerAction.ManualAim, MeleeHit: false, Staggering: false,
        InputMode: PlayerInputMode.ReadOnly
    } && GetRocketRidePlayer() == null;

    public override void OutsideWorld()
    {
        ConvertToNormal();
        base.OutsideWorld();
    }

    public override void BeforeHitObject(ProjectileHitEventArgs args)
    {
        if (args.HitObject.GetCollisionFilter().AbsorbProjectile)
        {
            ConvertToNormal();
        }

        base.BeforeHitObject(args);
    }

    public override void HitObject(ObjectData objectData, ProjectileHitEventArgs e)
    {
        if (e.HitObject.GetCollisionFilter().AbsorbProjectile)
        {
            ConvertToNormal();
        }

        base.HitObject(objectData, e);
    }

    public override void HitPlayer(Player player, ObjectData playerObjectData)
    {
        ConvertToNormal();
        base.HitPlayer(player, playerObjectData);
    }

    private void ConvertToNormal(bool beep = false)
    {
        if (GameOwner != GameOwnerEnum.Server && beep)
        {
            SoundHandler.PlaySound("C4Detonate", GameWorld);
        }

        _normalRocket = true;
        if (PlayerOwner is { IsRemoved: false, IsDead: false })
        {
            PlayerOwner.SetInputMode(PlayerInputMode.Enabled);
        }

        ImportantUpdate = true;
    }
}