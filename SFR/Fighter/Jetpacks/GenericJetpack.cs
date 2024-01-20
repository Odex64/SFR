using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Sounds;
using SFR.Sync.Generic;

namespace SFR.Fighter.Jetpacks;

/// <summary>
///     All the jetpacks derive from this.
///     This class will handle basic tasks, such as setting basic speed, playing effects / sounds etc.
/// </summary>
internal abstract class GenericJetpack(float fuel = 100f, float maxSpeed = 7f)
{
    protected const float FlyThreshold = 250f;
    protected internal readonly BarMeter Fuel = new(fuel, fuel);
    protected internal readonly float MaxSpeed = maxSpeed;

    protected float AirTime;
    protected float EffectTimer;

    protected Texture2D Jetpack;
    protected Texture2D JetpackBack;
    protected Texture2D JetpackDiving;
    internal bool Shake;
    protected float SoundTimer;

    protected internal JetpackState State;

    internal virtual void Update(float ms, ExtendedPlayer extendedPlayer)
    {
        var player = extendedPlayer.Player;
        if (player.RocketRideProjectileWorldID != 0)
        {
            Discard(extendedPlayer);
        }
        else if (player.InAir && !(player.Diving || player.LedgeGrabbing || player.Climbing || player.Crouching || player.Staggering || player.LayingOnGround || player.Falling || player.IsCaughtByPlayer))
        {
            AirTime += ms;
        }
        else
        {
            AirTime = 0f;
            State = JetpackState.Idling;
        }

        if (AirTime > FlyThreshold && player.VirtualKeyboard.PressingKey(19))
        {
            if (State is JetpackState.Idling or JetpackState.Falling)
            {
                State = JetpackState.Flying;

                if (player.GameOwner != GameOwnerEnum.Client)
                {
                    SoundHandler.PlaySound("Bazooka", player.GameWorld);
                }
            }

            var velocity = player.CurrentVelocity;
            velocity.X *= player.SlowmotionFactor * 0.6f;

            if (velocity.Y <= MaxSpeed)
            {
                velocity.Y = (velocity.Y > 1.94f ? velocity.Y : MaxSpeed > 1 ? 1.94f : 1.94f * MaxSpeed) * player.SlowmotionFactor * 1.17f;
            }
            else
            {
                velocity.Y = MaxSpeed;
            }

            // player.SetNewLinearVelocity(velocity);
            player.WorldBody.SetLinearVelocity(velocity);
            player.m_preBox2DLinearVelocity = velocity;
            player.AirControlBaseVelocity = velocity;
            player.ForceServerPositionState();
            player.ImportantUpdate = true;

            if (!player.InfiniteAmmo && player.GameOwner != GameOwnerEnum.Client)
            {
                Fuel.CurrentValue -= 0.03f * player.SlowmotionFactor * ms;

                if (player.GameOwner == GameOwnerEnum.Server)
                {
                    GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, [], player.ObjectID, extendedPlayer.GetStates()));
                }
            }

            if (player.GameOwner != GameOwnerEnum.Client)
            {
                EffectTimer -= ms;
                if (EffectTimer <= 0f)
                {
                    PlayEffect(player);
                }

                SoundTimer -= ms;
                if (SoundTimer < 0f)
                {
                    PlaySound(player);
                }
            }

            Shake = true;
        }
        else
        {
            if (State == JetpackState.Flying)
            {
                State = JetpackState.Falling;
            }

            Shake = false;
        }

        if (Fuel.CurrentValue <= 0 && extendedPlayer.JetpackType != JetpackType.None)
        {
            Discard(extendedPlayer);
        }
    }

    protected abstract void PlayEffect(Player player);

    protected abstract void PlaySound(Player player);

    protected internal virtual void Discard(ExtendedPlayer extendedPlayer)
    {
        extendedPlayer.JetpackType = JetpackType.None;
        extendedPlayer.GenericJetpack = null;
        if (extendedPlayer.Player.GameOwner == GameOwnerEnum.Server)
        {
            GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, [], extendedPlayer.Player.ObjectID, extendedPlayer.GetStates()));
        }
    }

    internal abstract Texture2D GetJetpackTexture(string postFix);
}