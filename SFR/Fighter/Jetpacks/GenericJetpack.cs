using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.Effects;
using SFD.Sounds;
using SFR.Sync.Generic;
using Keyboard = SFD.Input.Keyboard;

namespace SFR.Fighter.Jetpacks;

internal abstract class GenericJetpack
{
    internal readonly BarMeter Fuel;
    protected readonly float MaxSpeed;

    protected float AirTime;
    protected float EffectTimer;

    protected Texture2D Jetpack;
    protected Texture2D JetpackBack;
    protected Texture2D JetpackDiving;
    protected bool Landed;
    internal bool Shake;
    protected float SoundTimer;

    protected GenericJetpack(float fuel = 100f, float maxSpeed = 7f)
    {
        MaxSpeed = maxSpeed;
        Fuel = new BarMeter(fuel, fuel);
    }

    internal virtual void Update(float ms, ExtendedPlayer extendedPlayer)
    {
        var player = extendedPlayer.Player;
        if (player.InAir && !player.LedgeGrabbing && !player.Climbing)
        {
            AirTime += ms;
        }
        else
        {
            AirTime = 0;
            Landed = true;

            if (Keyboard.IsKeyDown(Keys.Space))
            {
                SoundHandler.PlaySound("PistolDraw", player.GameWorld);
                EffectHandler.PlayEffect("CFTXT", player.Position, player.GameWorld, "DISCARD");
                Discard(extendedPlayer);
            }
        }

        if (!player.Crouching && !player.Climbing && !player.Staggering && !player.LayingOnGround && !player.Falling)
        {
            if (AirTime > 250f && (player.VirtualKeyboard.PressingKey(0) || player.VirtualKeyboard.PressingKey(19)))
            {
                if (Landed)
                {
                    Landed = false;
                    SoundHandler.PlaySound("Bazooka", player.GameWorld);
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

                player.SetNewLinearVelocity(velocity);
                // player.ImportantUpdate = true;

                if (!player.InfiniteAmmo && !player.InfiniteAmmo)
                {
                    Fuel.CurrentValue -= 0.03f * player.SlowmotionFactor * ms;
                    GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, new SyncFlag[] { }, player.ObjectID, extendedPlayer.GetStates()));
                }

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

                Shake = true;
            }
            else
            {
                Shake = false;
            }

            if (Fuel.CurrentValue <= 0 && extendedPlayer.JetpackType != JetpackType.None)
            {
                Discard(extendedPlayer);
            }
        }
    }

    protected abstract void PlayEffect(Player player);

    protected abstract void PlaySound(Player player);

    protected virtual void Discard(ExtendedPlayer extendedPlayer)
    {
        extendedPlayer.JetpackType = JetpackType.None;
        extendedPlayer.GenericJetpack = null;
        if (extendedPlayer.Player.GameOwner == GameOwnerEnum.Server)
        {
            GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, new SyncFlag[] { }, extendedPlayer.Player.ObjectID, extendedPlayer.GetStates()));
        }
    }

    internal abstract Texture2D GetJetpackTexture(string postFix);
}