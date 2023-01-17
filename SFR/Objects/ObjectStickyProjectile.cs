using System.Linq;
using Box2D.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Objects;
using SFD.Projectiles;
using SFD.Sounds;
using Explosion = SFD.Explosion;

namespace SFR.Objects;

internal sealed class ObjectStickyProjectile : ObjectGrenadeThrown
{
    private Filter _originalFilter;
    private bool _stickied;
    private float _stickiedAngle;
    private ObjectData _stickiedObject;
    private Vector2 _stickiedOffset;
    internal bool Detonated;
    internal float TimeStamp;

    internal ObjectStickyProjectile(ObjectDataStartParams startParams) : base(startParams) { }

    private bool Stickied
    {
        get => _stickied;
        set
        {
            _stickied = value;
            Stick();
        }
    }

    public override void Initialize()
    {
        Body.GetFixtureByIndex(0).GetFilterData(out _originalFilter);
        Body.SetBullet(true);
        GameWorld.PortalsObjectsToKeepTrackOf.Add(this);
        EnableUpdateObject();
        Body.SetAngularDamping(3f);
        TimeStamp = GameWorld.ElapsedTotalGameTime;
    }

    public override void OnRemoveObject()
    {
        GameWorld.PortalsObjectsToKeepTrackOf.Remove(this);
    }

    public override void BeforePlayerMeleeHit(Player player, PlayerBeforeHitEventArgs e)
    {
        if (m_timeBeforeEnablePlayerHit > 0f)
        {
            e.Cancel = true;
        }
    }

    public override void PlayerMeleeHit(Player player, PlayerHitEventArgs e)
    {
        ObjectDataMethods.DefaultPlayerHitBaseballEffect(this, player, e);
    }

    public override void ExplosionHit(Explosion explosionData, ExplosionHitEventArgs e)
    {
        if (GameOwner != GameOwnerEnum.Client && explosionData.SourceExplosionDamage > 0f)
        {
            Destroy();
        }
    }

    public override void ProjectileHit(Projectile projectile, ProjectileHitEventArgs e)
    {
        if (GameOwner != GameOwnerEnum.Client && projectile.Properties.ProjectileID != 64)
        {
            Destroy();
        }
    }

    public override void SetProperties()
    {
        Properties.Add(ObjectPropertyID.Grenade_DudChance);
    }

    public override void UpdateObject(float ms)
    {
        m_timeBeforeEnablePlayerHit -= ms;

        if (Stickied)
        {
            if (_stickiedObject is { Body: not null, RemovalInitiated: false })
            {
                var gamePos = _stickiedOffset;
                SFDMath.RotatePosition(ref gamePos, _stickiedObject.GetAngle() - _stickiedAngle, out gamePos);
                gamePos += _stickiedObject.GetWorldPosition();
                Vector2 newPos = new(Converter.WorldToBox2D(gamePos.X), Converter.WorldToBox2D(gamePos.Y));
                Body.SetTransform(newPos, _stickiedObject.GetAngle() - _stickiedAngle);
            }
            else
            {
                Stickied = false;
            }
        }
    }

    private void Stick()
    {
        ChangeBodyType(Stickied ? BodyType.Static : BodyType.Dynamic);

        if (Stickied)
        {
            Filter filter = new()
            {
                categoryBits = 0,
                aboveBits = 0,
                maskBits = 0,
                blockMelee = false,
                projectileHit = true,
                absorbProjectile = true
            };

            Body.GetFixtureByIndex(0).SetFilterData(ref filter);
        }
        else
        {
            Body.GetFixtureByIndex(0).SetFilterData(ref _originalFilter);
        }
    }

    public override void OnDestroyObject()
    {
        Detonated = true;
        GameWorld.TriggerExplosion(GetWorldPosition(), 80f);
    }

    public override void Draw(SpriteBatch spriteBatch, float ms)
    {
        foreach (var objectDecal in m_objectDecals.Where(o => o != null))
        {
            var position = objectDecal.HaveOffset ? Body.GetWorldPoint(objectDecal.LocalOffset) : Body.Position;
            Camera.ConvertBox2DToScreen(ref position, out position);
            float rotation = -Body.GetAngle();
            spriteBatch.Draw(objectDecal.Texture, position, null, Color.Gray, rotation, objectDecal.TextureOrigin, Camera.Zoom, m_faceDirectionSpriteEffect, 0f);
        }
    }

    public override void ImpactHit(ObjectData otherObject, ImpactHitEventArgs e)
    {
        base.ImpactHit(otherObject, e);
        if (GameOwner != GameOwnerEnum.Server)
        {
            SoundHandler.PlaySound(Tile.ImpactSound, GetWorldPosition(), GameWorld);
            EffectHandler.PlayEffect(Tile.ImpactEffect, GetWorldPosition(), GameWorld);
        }

        if (!Stickied && otherObject is { RemovalInitiated: false, IsPlayer: false, Body: not null } and not ObjectStickyProjectile)
        {
            Stickied = true;
            _stickiedObject = otherObject;
            _stickiedOffset = GetWorldPosition() - otherObject.GetWorldPosition();
            _stickiedAngle = otherObject.GetAngle();
        }
    }

    public override void MissileHitPlayer(Player player, MissileHitEventArgs e)
    {
        if (player != null)
        {
            base.MissileHitPlayer(player, e);
            Body.SetLinearVelocity(Body.GetLinearVelocity() * new Vector2(0.2f, 1f));
            Body.SetAngularVelocity(Body.GetAngularVelocity() * 0.2f);
        }
    }

    internal void Detonate()
    {
        SoundHandler.PlaySound("C4Arm", GameWorld);
        Destroy();
    }
}