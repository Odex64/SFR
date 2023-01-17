using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SFD;

namespace SFR.Objects;

internal sealed class ObjectCrossbowBolt : ObjectData
{
    private Player _boltPlayer;
    private float _playerAngle;
    private bool _playerBolt;
    private int _playerFace;
    private Vector2 _playerOffset;
    internal int FilterObjectId = -1;
    internal float Timer;

    internal ObjectCrossbowBolt(ObjectDataStartParams startParams) : base(startParams) { }

    public override void Initialize() { }

    public override void UpdateObject(float ms)
    {
        if (Timer <= GameWorld.ElapsedTotalGameTime)
        {
            Destroy();
            return;
        }

        if (_playerBolt)
        {
            if (_boltPlayer is { IsRemoved: false, IsDead: false, Rolling: false, Falling: false, Diving: false })
            {
                var pos = Vector2.Zero;
                var the = _boltPlayer.Position + new Vector2(_boltPlayer.LastDirectionX * _playerFace * _playerOffset.X, _playerOffset.Y);
                FaceDirection = (short)(_boltPlayer.LastDirectionX * _playerFace);
                if (_boltPlayer.Crouching)
                {
                    pos += new Vector2(0, -10);
                }

                Converter.ConvertWorldToBox2D(the.X, the.Y, out pos.X, out pos.Y);
                Body.SetTransform(pos, _playerAngle * _playerFace * _boltPlayer.LastDirectionX);
                Body.SetLinearVelocity(Vector2.Zero);
            }
            else
            {
                Destroy();
                return;
            }
        }

        if (FilterObjectId != -1)
        {
            Body.GetFixtureByIndex(0).GetFilterData(out var filter);
            filter.bodyIDToIgnore ??= new Dictionary<int, ushort>();

            filter.bodyIDToIgnore.Add(FilterObjectId, 1);
            Body.GetFixtureByIndex(0).SetFilterData(ref filter);

            var otherObject = GameWorld.GetObjectDataByID(FilterObjectId);
            if (otherObject != null)
            {
                otherObject.Body.GetFixtureByIndex(0).GetFilterData(out var filter1);
                filter1.bodyIDToIgnore ??= new Dictionary<int, ushort>();
                filter1.bodyIDToIgnore.Add(ObjectID, 1);
                otherObject.Body.GetFixtureByIndex(0).SetFilterData(ref filter1);
            }

            FilterObjectId = -1;
        }

        base.UpdateObject(ms);
    }

    internal void ApplyPlayerBolt(Player player)
    {
        _playerBolt = true;
        _boltPlayer = player;
        _playerOffset = GetWorldPosition() - player.Position;
        _playerAngle = GetAngle();
        _playerFace = player.LastDirectionX;
    }
}