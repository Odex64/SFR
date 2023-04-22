using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Sounds;
using SFR.Sync.Generic;

namespace SFR.Fighter.Jetpacks;

internal abstract class GenericJetpack
{
    // private readonly float _jetpackFuel;
    private readonly float _maxSpeed;
    internal readonly BarMeter Fuel;
    private float _airTime;
    private bool _landed;
    protected float EffectTimer;

    protected Texture2D Jetpack;
    protected Texture2D JetpackBack;
    protected Texture2D JetpackDiving;
    internal bool Shake;
    protected float SoundTimer;

    protected GenericJetpack(float fuel = 100f, float maxSpeed = 7f)
    {
        _maxSpeed = maxSpeed;
        // _jetpackFuel = fuel;
        Fuel = new BarMeter(fuel, fuel);

        // if (player.GameOwner == GameOwnerEnum.Server && sync)
        // {
        //     Logger.LogInfo("jetpack 3 " + player.GameOwner);
        //     player.ObjectData.SyncedMethod(
        //         new ObjectDataSyncedMethod(ObjectDataSyncedMethod.Methods.AnimationSetFrame,
        //             player.GameWorld.ElapsedTotalGameTime,
        //             jetpackFuel,
        //             (int)jetpackType)
        //     );
        // }
    }

    internal virtual void Update(float ms, ExtendedPlayer extendedPlayer)
    {
        var player = extendedPlayer.Player;
        if (player.InAir && !player.LedgeGrabbing && !player.Climbing)
        {
            _airTime += ms;
        }
        else
        {
            _airTime = 0;
            _landed = true;
        }

        if (!player.Crouching && !player.Diving && !player.Climbing && !player.Staggering && !player.LayingOnGround)
        {
            if (_airTime > 250 && (player.VirtualKeyboard.PressingKey(0) || player.VirtualKeyboard.PressingKey(19)))
            {
                if (_landed)
                {
                    _landed = false;
                    SoundHandler.PlaySound("Bazooka", player.GameWorld);
                }

                var velocity = player.CurrentVelocity;
                velocity.X *= player.SlowmotionFactor * 0.6f;

                if (velocity.Y <= _maxSpeed)
                {
                    velocity.Y = (velocity.Y > 1.96f ? velocity.Y : 1.96f) * player.SlowmotionFactor * 1.17f;
                }
                else
                {
                    velocity.Y = _maxSpeed;
                }

                player.SetNewLinearVelocity(velocity);
                // player.ImportantUpdate = true;

                if (!player.InfiniteAmmo && !player.InfiniteAmmo)
                {
                    Fuel.CurrentValue -= 0.8f;
                    GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, player.ObjectID, extendedPlayer.GetStates()));
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
            GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, extendedPlayer.Player.ObjectID, extendedPlayer.GetStates()));
        }
    }

    internal abstract Texture2D GetJetpackTexture(string postFix);
}