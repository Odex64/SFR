using Microsoft.Xna.Framework;
using SFD;
using SFD.Sounds;
using SFR.Helper;
using Constants = SFR.Misc.Constants;
using Math = System.Math;

namespace SFR.Objects.Animal;

/// <summary>
///     Every animal must inherit from this class.
/// </summary>
internal abstract class ObjectAnimal : ObjectData
{
    private float _jumpTimer;
    private float _lastX;
    private float _timeSinceJump;
    protected string JumpSound;
    protected float MaxCheckInterval;
    protected float MaxJumpForce;
    protected float MinCheckInterval;
    protected float MinJumpForce;

    protected ObjectAnimal(ObjectDataStartParams startParams) : base(startParams) { }

    public override void Initialize()
    {
        _jumpTimer = Constants.Random.NextFloat(MinCheckInterval, MaxCheckInterval);
        Body.SetFixedRotation(true);
        GameWorld.PortalsObjectsToKeepTrackOf.Add(this);
        GameWorld.TriggerMineObjectsToKeepTrackOf.Add(this);
        EnableUpdateObject();
    }

    public override void UpdateObject(float ms)
    {
        if (GameOwner != GameOwnerEnum.Client && Body != null && GameWorld != null)
        {
            if (GetLinearVelocity().Y < 0.01 && GetLinearVelocity().Y > -0.01)
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
        GameWorld.TriggerMineObjectsToKeepTrackOf.Remove(this);
        GameWorld.PortalsObjectsToKeepTrackOf.Remove(this);
    }

    public override void Dispose()
    {
        DisableUpdateObject();
        base.Dispose();
    }

    private void Jump(short faceDirection = 0)
    {
        _timeSinceJump = 0;
        _jumpTimer = Constants.Random.NextFloat(MinCheckInterval, MaxCheckInterval);

        if (Constants.Random.Next(4) == 0 || Math.Abs(Math.Abs(_lastX) - Math.Abs(GetWorldPosition().X)) < 8)
        {
            FaceDirection *= -1;
        }

        if (Constants.Random.Next(10) == 0)
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
        Vector2 vec = new(Constants.Random.NextFloat(MinJumpForce, MaxJumpForce) * FaceDirection, Constants.Random.NextFloat(MinJumpForce, MaxJumpForce));
        Body.SetLinearVelocity(vec);
        SyncTransform();
        SoundHandler.PlaySound(JumpSound, GameWorld);
    }
}