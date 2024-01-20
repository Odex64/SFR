using System;
using Box2D.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Sounds;
using SFD.Tiles;
using SFR.Helper;
using Math = System.Math;
using Player = SFD.Player;

namespace SFR.Objects;

internal sealed class ObjectSnowballThrown : ObjectData
{
    private const float SnowballDamage = 6f;
    private TrailSpawner _trailSpawner;

    internal ObjectSnowballThrown(ObjectDataStartParams startParams) : base(startParams) { }

    public override void Initialize()
    {
        Body.SetBullet(true);
        GameWorld.PortalsObjectsToKeepTrackOf.Add(this);
        EnableUpdateObject();
        //Body.SetAngularDamping(3f);
        Body.SetAngularVelocity(Body.GetAngularVelocity() * 6f);

        _trailSpawner = new TrailSpawner(GameWorld, Body, new Vector2(0f, 0f), "F_S");
    }

    public override void OnRemoveObject()
    {
        GameWorld.PortalsObjectsToKeepTrackOf.Remove(this);
    }

    public override void UpdateObject(float ms)
    {
        _trailSpawner.Update(ms);
    }

    public override void SolveImpactContactHit(Contact contact, ObjectData otherObject, Fixture myFixture, Fixture otherFixture,
        ref WorldManifold worldManifold, ref FixedArray2<PointState> pointStates, ref Manifold manifold)
    {
        if (GameOwner != GameOwnerEnum.Client)
        {
            if ((int)pointStates[0] == 1)
            {
                if (otherObject.MapObjectID.StartsWith("GLASSSHEET"))
                {
                    otherObject.Destroy();
                    return;
                }

                if (otherObject.MapObjectID == "WPNSNOWBALLTHROWN")
                {
                    otherObject.Destroy();
                    return;
                }

                var linearVelocity = GetLinearVelocity();
                if (Math.Abs(linearVelocity.X) > 0.1f || Math.Abs(linearVelocity.Y) > 0.1f)
                {
                    linearVelocity.Normalize();
                    float num = Vector2.Dot(linearVelocity, worldManifold._normal);
                    if (num < -0.1f)
                    {
                        Destroy();
                    }
                }
            }
            else if (otherObject.MapObjectID.StartsWith("GLASSSHEET"))
            {
                contact.SetEnabled(false);
            }
        }
    }

    public override void ImpactHit(ObjectData otherObject, ImpactHitEventArgs e)
    {
        base.ImpactHit(otherObject, e);

        if (GameOwner != GameOwnerEnum.Client && otherObject.IsPlayer)
        {
            var player = (Player)otherObject.InternalData;
            player.TakeMiscDamage(SnowballDamage);
            Destroy();
        }
    }

    public override void OnDestroyObject()
    {
        if (GameOwner != GameOwnerEnum.Client)
        {
            SoundHandler.PlaySound("FootstepSnow", GetWorldPosition(), GameWorld);
            EffectHandler.PlayEffect("STM", GetWorldPosition(), GameWorld);
        }
    }

    public override void Dispose()
    {
        if (!IsDisposed && _trailSpawner != null)
        {
            _trailSpawner.Dispose();
            _trailSpawner = null;
        }

        base.Dispose();
    }
}

internal sealed class TrailSpawner
{
    private static Texture2D _texture;
    private readonly float _distanceSpawn;
    private readonly string _effect;
    private readonly Vector2 _localOffset;
    private readonly float _maxDistanceSpawn;
    private Body _body;
    private GameWorld _gameWorld;
    private Vector2 _lastBodyPosition;
    private float _nextTrace;

    internal TrailSpawner(GameWorld gameWorld, Body body, Vector2 localOffset, string effect)
    {
        // TODO: edit these numbers
        _distanceSpawn = Converter.ConvertWorldToBox2D(Constants.EFFECT_LEVEL_FULL ? 12f : 20f);

        _maxDistanceSpawn = Converter.ConvertWorldToBox2D(36f);
        _gameWorld = gameWorld;
        _body = body;
        _localOffset = localOffset;
        _effect = effect;
        _lastBodyPosition = body.GetWorldPoint(localOffset);
        _texture ??= Textures.GetTexture("STM");
    }

    internal void Update(float ms)
    {
        if (_gameWorld.GameOwner != GameOwnerEnum.Server)
        {
            _nextTrace -= ms;
            float num = 0f;
            var worldPoint = _body.GetWorldPoint(_localOffset);
            if (Constants.EFFECT_LEVEL_FULL)
            {
                num = Vector2.Distance(worldPoint, _lastBodyPosition);
            }

            if (_nextTrace <= 0f || num > _distanceSpawn)
            {
                int num2 = num > _maxDistanceSpawn ? 0 : (int)Math.Round(num / _distanceSpawn);
                if (Constants.EFFECT_LEVEL_LOW)
                {
                    num2 = (int)Math.Round(num2 * 0.5f, MidpointRounding.AwayFromZero);
                }

                Vector2 value = new(Misc.Constants.Random.NextFloat(-2f, 2f), Misc.Constants.Random.NextFloat(-2f, 2f));
                if (num2 > 1)
                {
                    float scaleFactor = num / num2;
                    var value2 = worldPoint - _lastBodyPosition;
                    value2.Normalize();
                    for (int i = 1; i <= num2; i++)
                    {
                        EffectHandler.PlayEffect(_effect, Converter.ConvertBox2DToWorld(_lastBodyPosition + value2 * scaleFactor * i) + value, _gameWorld);
                    }
                }
                else
                {
                    EffectHandler.PlayEffect(_effect, Converter.ConvertBox2DToWorld(worldPoint) + value, _gameWorld);
                }

                _nextTrace = 100f;
                _lastBodyPosition = worldPoint;
            }
        }
    }

    internal void Dispose()
    {
        _body = null;
        _texture?.Dispose();
        _texture = null;
        _gameWorld = null;
    }
}