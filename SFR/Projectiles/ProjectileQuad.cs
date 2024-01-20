using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Projectiles;
using SFD.Tiles;
using SFR.Helper;
using Math = System.Math;
using Player = SFD.Player;

namespace SFR.Projectiles;

internal sealed class ProjectileQuad : ProjectileBazooka
{
    private const float ExplosionValue = 60;
    private const float MaxRange = 600;
    private bool _dataReflected;
    private float _effectTimer;
    private Vector2 _originalDirection;
    private bool _reflected;
    private int _seed;

    internal ProjectileQuad()
    {
        Visuals = new ProjectileVisuals(Textures.GetTexture("QuadRocket"), Textures.GetTexture("QuadRocket"));
        Properties = new ProjectileProperties(99, 300f, 0f, 10f, 10f, 0f, 0f, 15f, 0.5f)
        {
            DodgeChance = 0f,
            CanBeAbsorbedOrBlocked = false,
            PowerupTotalBounces = 3,
            PowerupBounceRandomAngle = 0f,
            PowerupFireType = ProjectilePowerupFireType.Fireplosion,
            PowerupFireIgniteValue = 36f
        };
    }

    private ProjectileQuad(ProjectileProperties projectileProperties, ProjectileVisuals projectileVisuals) : base(projectileProperties, projectileVisuals) { }

    public override Projectile Copy()
    {
        Projectile projectile = new ProjectileQuad(Properties, Visuals);
        projectile.CopyBaseValuesFrom(this);
        return projectile;
    }

    public override void HitPlayer(Player player, ObjectData playerObjectData)
    {
        if (player.GameOwner != GameOwnerEnum.Client)
        {
            GameWorld.TriggerExplosion(Position, ExplosionValue, true);
            HitFlag = true;
            GameWorld.RemovedProjectiles.Add(this);
        }
    }

    public override void Update(float ms)
    {
        if (GameWorld.GameOwner != GameOwnerEnum.Server)
        {
            _effectTimer -= ms;
            if (_effectTimer < 0)
            {
                EffectHandler.PlayEffect("TR_F", Position, GameWorld, Direction.X, Direction.Y); //TR_F
                _effectTimer = Constants.EFFECT_LEVEL_FULL ? 10f : 20f;
            }
        }

        if (_reflected)
        {
            _reflected = false;
            _originalDirection = Direction;
            _dataReflected = true;
        }

        if (TotalDistanceTraveled > MaxRange)
        {
            GameWorld.TriggerExplosion(Position, ExplosionValue, true);
            HitFlag = true;
            GameWorld.RemovedProjectiles.Add(this);
            return;
        }

        float move = (float)Math.Sin((TotalDistanceTraveled + 30 * _seed) / 20) / (5 / (TotalDistanceTraveled / 100));
        move += (float)Math.Sin((TotalDistanceTraveled + 95 * _seed) / 100) / 20;
        Direction = _originalDirection.GetRotatedVector(move);
    }

    public override void HitObject(ObjectData objectData, ProjectileHitEventArgs e)
    {
        if (!ProjectileGrenadeLauncher.SpecialIgnoreObjectsForExplosiveProjectiles(objectData))
        {
            if (GameOwner != GameOwnerEnum.Client && HitFlag)
            {
                if (e.ReflectionStatus != ProjectileReflectionStatus.WillBeReflected)
                {
                    GameWorld.TriggerExplosion(Position - Direction * 2, ExplosionValue, true);
                }
                else
                {
                    _reflected = true;
                }
            }

            if (GameOwner == GameOwnerEnum.Client) //Local handled above
            {
                base.HitObject(objectData, e);
                _reflected = true;
            }
        }
        else
        {
            e.CustomHandled = true;
            e.ReflectionStatus = ProjectileReflectionStatus.None;
            HitFlag = false;
        }
    }


    public override void AfterCreated()
    {
        _originalDirection = Direction;
        if (GameOwner != GameOwnerEnum.Client)
        {
            _seed = PlayerOwner.Statisticts.m_TotalShotsFired % 4;
            WriteAdditionalData();
        }
    }

    public override byte[] WriteAdditionalData()
    {
        if (_dataReflected)
        {
            _dataReflected = false;
            return [(byte)_seed, (byte)((_originalDirection.X + 1f) * 128f), (byte)((_originalDirection.Y + 1f) * 128f)];
        }

        return [(byte)_seed];
    }

    public override void ReadAdditionalData(byte[] bytes)
    {
        if (bytes.Length == 1 && GameOwner != GameOwnerEnum.Server)
        {
            _seed = bytes[0];
        }

        if (bytes.Length == 3 && GameOwner != GameOwnerEnum.Server)
        {
            _seed = bytes[0];
            float x = bytes[1] / 128f - 1f;
            float y = bytes[2] / 128f - 1f;
            _originalDirection = new Vector2(x, y);
        }
    }
}