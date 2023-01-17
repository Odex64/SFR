using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Projectiles;
using SFD.Tiles;
using SFR.Helper;

namespace SFR.Projectiles;

internal sealed class ProjectileUnkemptHarold : Projectile
{
    private const float FrameTime = 30f;
    private const int AnimationFrames = 6;
    private const float ExplosionPower = 20f;
    private const float SplitDistance = 64;
    private float _animationTimer;
    private int _currentFrame = 1;
    private float _effectTimer;
    private int _splits = 2;

    internal ProjectileUnkemptHarold()
    {
        Visuals = new ProjectileVisuals(Textures.GetTexture("BulletUnkemptHarold"), Textures.GetTexture("BulletUnkemptHarold"));
        Visuals.BulletTraceOrigin = new Vector2(Visuals.BulletTraceTexture.Width - Visuals.BulletTraceOrigin.X + 8, Visuals.BulletTraceOrigin.Y);
        Properties = new ProjectileProperties(85, 200f, 1f, 1f, 1f, 0.2f, 0f, 0f, 0.1f)
        {
            PowerupBounceRandomAngle = 0.2f,
            PowerupFireType = ProjectilePowerupFireType.Default,
            PowerupFireIgniteValue = 10,
            PlayerForce = 17,
            ObjectForce = 0.2f * 0.04f
        };
    }

    private ProjectileUnkemptHarold(ProjectileProperties projectileProperties, ProjectileVisuals projectileVisuals) : base(projectileProperties, projectileVisuals) { }

    public override Projectile Copy()
    {
        ProjectileUnkemptHarold projectileUnkempt = new(Properties, Visuals);
        projectileUnkempt.CopyBaseValuesFrom(this);
        return projectileUnkempt;
    }

    public override void Update(float ms)
    {
        if (_splits > 0 && TotalDistanceTraveled > SplitDistance)
        {
            if (GameOwner != GameOwnerEnum.Client)
            {
                var newProj = (ProjectileUnkemptHarold)GameWorld.CreateProjectile(85, null, Position, Direction.GetRotatedVector(-0.1), 0);
                newProj._splits = _splits - 1;
                newProj = (ProjectileUnkemptHarold)GameWorld.CreateProjectile(85, null, Position, Direction.GetRotatedVector(0.1), 0);
                newProj._splits = _splits - 1;

                _splits = 0;
            }
        }

        _effectTimer += ms;
        if (_effectTimer > 20)
        {
            EffectHandler.PlayEffect("TR_S", Position, GameWorld);
        }
    }

    public override void HitObject(ObjectData objectData, ProjectileHitEventArgs e)
    {
        base.HitObject(objectData, e);
        if (HitFlag && GameOwner != GameOwnerEnum.Client && e.ReflectionStatus != ProjectileReflectionStatus.WillBeReflected)
        {
            GameWorld.TriggerExplosion(Position - Direction * 2, ExplosionPower, true);
        }
    }

    public override void HitPlayer(Player player, ObjectData playerObjectData)
    {
        base.HitPlayer(player, playerObjectData);
        if (HitFlag)
        {
            GameWorld.TriggerExplosion(Position - Direction * 2, ExplosionPower, true);
        }
    }

    public override void Draw(SpriteBatch spriteBatch, float ms)
    {
        _animationTimer += ms;
        _currentFrame = (int)(_animationTimer % (FrameTime * AnimationFrames) / FrameTime);
        spriteBatch.Draw(Visuals.BulletTraceTexture, Camera.ConvertWorldToScreen(Position), new Rectangle(_currentFrame * 16, 0, 16, 16), new Color(0.6f, 0.6f, 0.6f, 1f), GetAngle(), Visuals.BulletTraceOrigin, Camera.Zoom, SpriteEffects.None, 0f);
    }
}