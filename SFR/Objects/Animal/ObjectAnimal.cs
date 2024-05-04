using Microsoft.Xna.Framework;
using SFD;
using SFD.Sounds;
using SFR.Helper;
using SFR.Misc;
using Math = System.Math;

namespace SFR.Objects.Animal;

/// <summary>
/// Every animal must inherits from this class.
/// </summary>
internal abstract class ObjectAnimal(ObjectDataStartParams startParams) : ObjectData(startParams)
{
    private float _jumpTimer;
    private float _lastX;
    private float _timeSinceJump;
    protected string JumpSound;
    protected float MaxCheckInterval;
    protected float MaxJumpForce;
    protected float MinCheckInterval;
    protected float MinJumpForce;

    public override void Initialize()
    {
        _jumpTimer = Globals.Random.NextFloat(MinCheckInterval, MaxCheckInterval);
        Body.SetFixedRotation(true);
        GameWorld.PortalsObjectsToKeepTrackOf.Add(this);
        GameWorld.TriggerMineObjectsToKeepTrackOf.Add(this);
        EnableUpdateObject();
    }

    public override void UpdateObject(float ms)
    {
        if (GameOwner != GameOwnerEnum.Client && Body is not null && GameWorld is not null)
        {
            if (GetLinearVelocity().Y is < (float)0.01 and > (float)-0.01)
            {
                _timeSinceJump += ms;
                foreach (var player in GameWorld.Players)
                {
                    if (Vector2.Distance(player.Position, GetWorldPosition()) < 32)
                    {
                        short num = -1;
                        if (player.Position.X < GetWorldPosition().X)
                        {
                            num = 1;
                        }

                        Jump(num);
                        break;
                    }
                }
            }

            if (!Body.IsFixedRotation() && _timeSinceJump > 100)
            {
                Body.SetAngularVelocity(0);
                Body.Rotation = 0;
                Body.SetFixedRotation(true);
            }

            if (_timeSinceJump > _jumpTimer)
            {
                Jump();
            }
        }
    }

    public override void OnRemoveObject()
    {
        _ = GameWorld.TriggerMineObjectsToKeepTrackOf.Remove(this);
        _ = GameWorld.PortalsObjectsToKeepTrackOf.Remove(this);
    }

    public override void Dispose()
    {
        DisableUpdateObject();
        base.Dispose();
    }

    private void Jump(short faceDirection = 0)
    {
        _timeSinceJump = 0;
        _jumpTimer = Globals.Random.NextFloat(MinCheckInterval, MaxCheckInterval);

        if (Globals.Random.Next(4) == 0 || Math.Abs(Math.Abs(_lastX) - Math.Abs(GetWorldPosition().X)) < 8)
        {
            FaceDirection *= -1;
        }

        if (Globals.Random.Next(10) == 0)
        {
            Body.SetFixedRotation(false);
            Body.SetAngularVelocity(-12 * FaceDirection);
        }
        else
        {
            Body.SetAngularVelocity(0);
            Body.Rotation = 0;
            Body.SetFixedRotation(true);
        }

        if (faceDirection != 0)
        {
            FaceDirection = faceDirection;
        }

        _lastX = GetWorldPosition().X;
        Vector2 vec = new(Globals.Random.NextFloat(MinJumpForce, MaxJumpForce) * FaceDirection, Globals.Random.NextFloat(MinJumpForce, MaxJumpForce));
        Body.SetLinearVelocity(vec);
        SyncTransform();
        SoundHandler.PlaySound(JumpSound, GameWorld);
    }
}