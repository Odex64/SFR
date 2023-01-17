using Box2D.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Projectiles;
using SFD.Sounds;
using SFD.Tiles;
using SFR.Helper;
using SFR.Projectiles;
using Constants = SFR.Misc.Constants;
using Explosion = SFD.Explosion;
using Math = System.Math;

namespace SFR.Objects;

internal sealed class ObjectClaymoreThrown : ObjectData
{
    private float _armTimer = 3000f;
    private bool _blink;
    private float _blinkInterval = 100f;
    private Texture2D _blinkTexture;
    private float _blinkTimer;
    private float _explosionTimer = 150f;
    private bool _filterApplied;
    private bool _isTripped;
    private float _normalAngle;
    private Texture2D _normalTexture;
    private Filter _originalFilter;
    private int _status;
    private bool _stickied;
    private float _stickiedAngle;
    private ObjectData _stickiedObject;
    private Vector2 _stickiedOffset = Vector2.Zero;

    internal ObjectClaymoreThrown(ObjectDataStartParams startParams) : base(startParams) { }

    public override void Initialize()
    {
        EnableUpdateObject();
        GameWorld.PortalsObjectsToKeepTrackOf.Add(this);
        Body.SetBullet(true);
        FaceDirection = 1;
        _status = (int)Properties.Get(ObjectPropertyID.Mine_Status).Value;
        _normalTexture = Textures.GetTexture("ClaymoreM");
        _blinkTexture = Textures.GetTexture("ClaymoreMBlink");
    }

    public override void OnRemoveObject()
    {
        GameWorld.PortalsObjectsToKeepTrackOf.Remove(this);
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
        if (GameOwner != GameOwnerEnum.Client && projectile.Properties.ProjectileID != 64 && !(projectile is IExtendedProjectile))
        {
            Destroy();
        }
    }

    public override void SetProperties()
    {
        Properties.Add(ObjectPropertyID.Mine_DudChance);
        Properties.Add(ObjectPropertyID.Mine_Status);
    }

    private float GetDudChance() => (float)Properties.Get(ObjectPropertyID.Mine_DudChance).Value;

    private void SetDudChance(float value)
    {
        Properties.Get(ObjectPropertyID.Mine_DudChance).Value = value;
    }

    public override void PropertyValueChanged(ObjectPropertyInstance propertyChanged)
    {
        if (propertyChanged.Base.PropertyID == 212)
        {
            _status = (int)Properties.Get(ObjectPropertyID.Mine_Status).Value;
            _blink = false;
            _blinkTimer = 0f;
            if (_status == 3)
            {
                _blinkInterval = 10f;
            }
        }
    }

    public override void UpdateObject(float ms)
    {
        //Tripwire code
        if (GameOwner != GameOwnerEnum.Client && _status >= 2)
        {
            Vector2 theVector = new(1, 0);
            float num = Camera.GetDistanceToEdge(GetWorldPosition(), theVector.GetRotatedVector(-GetAngle()));
            if (num != -1f)
            {
                num += 16f;
                var rayCastResult = GameWorld.RayCast(GetWorldPosition(), theVector.GetRotatedVector(-GetAngle()), 0f, num, LazerRaycastCollision, _ => true);
                if (rayCastResult.EndFixture != null)
                {
                    var objectData = Read(rayCastResult.EndFixture);
                    if (objectData.IsPlayer)
                    {
                        _isTripped = true;
                    }
                }
            }
        }

        //Sticky code
        if (_stickied)
        {
            if (_stickiedObject is { RemovalInitiated: false })
            {
                if (!_filterApplied)
                {
                    ApplyFilter();
                }

                if (_stickiedObject.Body != null)
                {
                    var gamePos = _stickiedOffset;
                    SFDMath.RotatePosition(ref gamePos, _stickiedObject.GetAngle() - _stickiedAngle, out gamePos);
                    gamePos += _stickiedObject.GetWorldPosition();
                    Vector2 newPos = new(Converter.WorldToBox2D(gamePos.X), Converter.WorldToBox2D(gamePos.Y));
                    Body.SetTransform(newPos, -_stickiedObject.GetAngle() + _stickiedAngle - _normalAngle);
                    SyncTransform();
                }
                else
                {
                    _stickied = false;
                    Body.SetType(BodyType.Dynamic);
                }
            }
        }

        //Mine code
        if (_status <= 1)
        {
            _armTimer -= ms;
            if (_status == 0)
            {
                if (_blinkTimer <= 0f)
                {
                    _blink = !_blink;
                    _blinkTimer += _blinkInterval;
                    if (_blink)
                    {
                        SoundHandler.PlaySound("MineTick", GameWorld);
                    }
                }

                _blinkTimer -= ms;
            }

            if (GameOwner != GameOwnerEnum.Client)
            {
                if (_armTimer <= 500f)
                {
                    Properties.Get(ObjectPropertyID.Mine_Status).Value = 1;
                }

                if (_armTimer <= 0f)
                {
                    Properties.Get(ObjectPropertyID.Mine_Status).Value = 2;
                }
            }
        }
        else
        {
            if (GameOwner == GameOwnerEnum.Client)
            {
                return;
            }

            switch (_status)
            {
                case 2:
                {
                    if (_isTripped)
                    {
                        Properties.Get(ObjectPropertyID.Mine_Status).Value = 3;
                        SoundHandler.PlaySound("MineTrigger", GameWorld);
                    }

                    break;
                }
                case 3:
                {
                    if (_blinkTimer <= 0f)
                    {
                        _blink = !_blink;
                        _blinkTimer += _blinkInterval;
                    }

                    _blinkTimer -= ms;
                    if (GameOwner != GameOwnerEnum.Client)
                    {
                        _explosionTimer -= ms;
                        if (_explosionTimer <= 0f)
                        {
                            if (Constants.Random.NextFloat() < GetDudChance())
                            {
                                EffectHandler.PlayEffect("GR_D", GetWorldPosition(), GameWorld);
                                SoundHandler.PlaySound("GrenadeDud", GameWorld);
                                Properties.Get(ObjectPropertyID.Mine_Status).Value = -1;
                                Body.SetType(BodyType.Dynamic);
                                Body.GetFixtureByIndex(0).SetFilterData(ref _originalFilter);
                            }
                            else
                            {
                                Destroy();
                            }

                            DisableUpdateObject();
                        }
                    }

                    break;
                }
            }
        }
    }

    public override void OnDestroyObject()
    {
        if (GameOwner != GameOwnerEnum.Client)
        {
            Vector2 vec = new(10, 0);
            for (int i = 0; i < 12; i++)
            {
                var dir = vec.GetRotatedVector(-GetAngle() + Constants.Random.NextFloat(-0.25f, 0.25f));
                var projectile = GameWorld.SpawnProjectile(61, GetWorldPosition(), dir, BodyID);
                projectile.CritChanceDealtModifier = 0f;
                projectile.Properties.DodgeChance = 0;
            }

            SoundHandler.PlaySound("Explosion", GetWorldPosition(), GameWorld);
            EffectHandler.PlayEffect("EXP", GetWorldPosition(), GameWorld);
            EffectHandler.PlayEffect("CAM_S", GetWorldPosition(), GameWorld, 8f, 250f, false);
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

        if (!_stickied && otherObject is { RemovalInitiated: false, IsPlayer: false })
        {
            ChangeBodyType(BodyType.Static);
            _stickiedObject = otherObject;
            _stickied = true;
            _stickiedOffset = GetWorldPosition() - otherObject.GetWorldPosition();
            _stickiedAngle = otherObject.GetAngle();
            _normalAngle = (float)Math.Atan2(e.WorldNormal.Y, e.WorldNormal.X);
        }
    }

    private void ApplyFilter()
    {
        GetFixtureByIndex(0).GetFilterData(out _originalFilter);
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
        _filterApplied = true;
    }

    public override void Draw(SpriteBatch spriteBatch, float ms)
    {
        var texture2D = _blink ? _blinkTexture : _normalTexture;
        var vector = Body.Position;
        vector += GameWorld.DrawingBox2DSimulationTimestepOver * Body.GetLinearVelocity();
        Camera.ConvertBox2DToScreen(ref vector, out vector);
        spriteBatch.Draw(texture2D, vector, null, Color.Gray, GetAngle(), new Vector2(texture2D.Width / 2, texture2D.Height / 2), Camera.ZoomUpscaled, m_faceDirectionSpriteEffect, 0f);

        if (_status >= 2 || Body.GetType() == BodyType.Static)
        {
            Vector2 theVector = new(1f, 0f);
            float angle = Camera.GetDistanceToEdge(GetWorldPosition(), theVector.GetRotatedVector(-GetAngle()));
            if (angle != -1f)
            {
                angle += 16f;
                var rayCastResult = GameWorld.RayCast(GetWorldPosition(), theVector.GetRotatedVector(-GetAngle()), 0f, angle, LazerRaycastCollision, _ => true);
                GameWorld.DrawLazer(spriteBatch, _isTripped || (_blink && _status < 2), rayCastResult.StartPosition, rayCastResult.EndPosition, rayCastResult.Direction);
                if (rayCastResult.EndFixture != null)
                {
                    var objectData = Read(rayCastResult.EndFixture);
                    if (objectData.IsPlayer)
                    {
                        _isTripped = true;
                    }
                }
            }
        }
    }

    private bool LazerRaycastCollision(Fixture fixture)
    {
        if (!fixture.IsCloud())
        {
            var objectData = Read(fixture);
            fixture.GetFilterData(out var filter);
            if ((filter.categoryBits & 15) > 0 || objectData.IsPlayer)
            {
                if (this != objectData)
                {
                    var tileFixtureMaterial = objectData.Tile.GetTileFixtureMaterial(fixture.TileFixtureIndex);
                    return !tileFixtureMaterial.Transparent;
                }

                return false;
            }
        }

        return false;
    }
}