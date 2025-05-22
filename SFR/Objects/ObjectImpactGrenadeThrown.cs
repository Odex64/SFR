using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Objects;
using SFD.Projectiles;

namespace SFR.Objects;

internal sealed class ObjectImpactGrenadeThrown : ObjectGrenadeThrown
{
    internal ObjectImpactGrenadeThrown(ObjectDataStartParams startParams) : base(startParams) => ExplosionTimer = 0f;

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
        if (GameOwner != GameOwnerEnum.Client)
        {
            Destroy();
        }
    }

    public override void BeforePlayerMeleeHit(Player player, PlayerBeforeHitEventArgs e)
    {
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
    }

    public override void OnDestroyObject() => GameWorld.TriggerExplosion(GetWorldPosition(), 80f);

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
        if (GameOwner != GameOwnerEnum.Client)
        {
            Destroy();
        }
    }
}