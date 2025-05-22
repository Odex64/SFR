using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Objects;
using SFD.Projectiles;
using SFD.Sounds;
using SFR.Helper;
using SFR.Misc;
using Explosion = SFD.Explosion;
using Math = System.Math;
using Player = SFD.Player;

namespace SFR.Objects;

internal sealed class ObjectFragGrenadeThrown : ObjectGrenadeThrown
{
    private const int _fragments = 24;

    internal ObjectFragGrenadeThrown(ObjectDataStartParams startParams) : base(startParams) => ExplosionTimer = 3000f;

    public override void Initialize()
    {
        EnableUpdateObject();
        GameWorld.PortalsObjectsToKeepTrackOf.Add(this);
        Body.SetAngularDamping(3f);
        Body.SetBullet(true);
    }

    public override void OnRemoveObject() => GameWorld.PortalsObjectsToKeepTrackOf.Remove(this);

    public override void MissileHitPlayer(Player player, MissileHitEventArgs e)
    {
        base.MissileHitPlayer(player, e);
        Body.SetLinearVelocity(Body.GetLinearVelocity() * new Vector2(0.2f, 1f));
        Body.SetAngularVelocity(Body.GetAngularVelocity() * 0.2f);
    }

    public override void BeforePlayerMeleeHit(Player player, PlayerBeforeHitEventArgs e)
    {
        if (m_timeBeforeEnablePlayerHit > 0f)
        {
            e.Cancel = true;
        }
    }

    public override void PlayerMeleeHit(Player player, PlayerHitEventArgs e) => ObjectDataMethods.DefaultPlayerHitBaseballEffect(this, player, e);

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

    public override void SetProperties() => Properties.Add(ObjectPropertyID.Grenade_DudChance);

    public override void UpdateObject(float ms)
    {
        m_timeBeforeEnablePlayerHit -= ms;
        if (GameOwner != GameOwnerEnum.Client)
        {
            ExplosionTimer -= ms;
            if (ExplosionTimer <= 0f)
            {
                m_timeBeforeEnablePlayerHit = 0f;
                DisableUpdateObject();
                if (Globals.Random.NextFloat() < GetDudChance())
                {
                    EffectHandler.PlayEffect("GR_D", GetWorldPosition(), GameWorld);
                    SoundHandler.PlaySound("GrenadeDud", GameWorld);
                    ExplosionResultedInDud = true;
                    return;
                }

                Destroy();
            }
        }
    }

    public override void OnDestroyObject()
    {
        _ = GameWorld.TriggerExplosion(GetWorldPosition(), 70f);
        for (int i = 0; i < _fragments; i++)
        {
            double angle = 2f * Math.PI / _fragments * i;
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            Vector2 vec = new(0, 10);
            Vector2 direction = new((float)(cosAngle * vec.X - sinAngle * vec.Y), (float)(sinAngle * vec.X + cosAngle * vec.Y));
            _ = GameWorld.SpawnProjectile(10, GetWorldPosition(), direction, ObjectID);
        }
    }

    public override void Draw(SpriteBatch spriteBatch, float ms)
    {
        foreach (ObjectDecal objectDecal in m_objectDecals)
        {
            Vector2 position = objectDecal.HaveOffset ? Body.GetWorldPoint(objectDecal.LocalOffset) : Body.Position;
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
    }
}