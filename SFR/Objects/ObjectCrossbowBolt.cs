﻿using System.Linq;
using Box2D.XNA;
using SFD;
using SFDGameScriptInterface;
using BodyType = Box2D.XNA.BodyType;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SFR.Objects;

internal sealed class ObjectCrossbowBolt : ObjectData
{
    private Player _boltPlayer;
    private int _playerFace;
    private Vector2 _playerOffset;
    internal int FilterObjectId = -1;
    internal float Timer;

    internal ObjectCrossbowBolt(ObjectDataStartParams startParams) : base(startParams)
    {
    }

    public override void Initialize()
    {
    }

    public override void UpdateObject(float ms)
    {
        if (Timer <= GameWorld.ElapsedTotalGameTime)
        {
            Destroy();
            return;
        }

        if (FilterObjectId != -1)
        {
            Body.GetFixtureByIndex(0).GetFilterData(out Filter filter);
            filter.bodyIDToIgnore ??= [];

            filter.bodyIDToIgnore.Add(FilterObjectId, 1);
            Body.GetFixtureByIndex(0).SetFilterData(ref filter);

            ObjectData otherObject = GameWorld.GetObjectDataByID(FilterObjectId);
            if (otherObject is not null)
            {
                otherObject.Body.GetFixtureByIndex(0).GetFilterData(out Filter filter1);
                filter1.bodyIDToIgnore ??= [];
                filter1.bodyIDToIgnore.Add(ObjectID, 1);
                otherObject.Body.GetFixtureByIndex(0).SetFilterData(ref filter1);
            }

            FilterObjectId = -1;
        }

        if (_boltPlayer is not null)
        {
            if (_boltPlayer is { IsRemoved: false, IsDead: false, Rolling: false, Falling: false, Diving: false })
            {
                Vector2 pos = Vector2.Zero;
                Vector2 the = _boltPlayer.Position + new Vector2(_boltPlayer.LastDirectionX * _playerFace * _playerOffset.X, _playerOffset.Y);
                FaceDirection = (short)(_boltPlayer.LastDirectionX * _playerFace);
                if (_boltPlayer.Crouching)
                {
                    pos += new Vector2(0, -10);
                }

                Converter.ConvertWorldToBox2D(the.X, the.Y, out pos.X, out pos.Y);
                Body.Position = pos;
            }
            else
            {
                Destroy();
            }
        }
        else if (IsDynamic)
        {
            Vector2 pos = GetWorldPosition();
            pos.Y -= 4;
            AABB.Create(out AABB aabb, pos, 4);
            if (GetLinearVelocity() == Vector2.Zero && GameWorld.GetObjectDataByArea(aabb, true, PhysicsLayer.All).Any(o => o.IsStatic && !o.Tile.Name.StartsWith("Bg")))
            {
                Body.SetType(BodyType.Static);
            }
        }
    }

    internal void ApplyPlayerBolt(Player player)
    {
        _boltPlayer = player;
        _playerOffset = GetWorldPosition() - player.Position;
        _playerFace = player.LastDirectionX;
    }
}