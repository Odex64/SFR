using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Sounds;
using SFD.Tiles;

namespace SFR.Fighter.Jetpacks;

internal sealed class Jetpack : GenericJetpack
{
    internal Jetpack() : base(100, 0.4f) { }

    protected override void PlayEffect(Player player)
    {
        EffectHandler.PlayEffect("FNDTRA", player.Position + new Vector2(-4 * player.LastDirectionX, -4), player.GameWorld);
        EffectTimer = 30f;
    }

    protected override void PlaySound(Player player)
    {
        SoundHandler.PlaySound("Flamethrower", player.GameWorld);
        SoundTimer = 60f;
    }

    // internal override void Update(float ms, ExtendedPlayer extendedPlayer)
    // {
    //     var player = extendedPlayer.Player;
    //     if (player.InAir && !player.LedgeGrabbing && !player.Climbing)
    //     {
    //         AirTime += ms;
    //     }
    //     else
    //     {
    //         AirTime = 0;
    //         Landed = true;
    //     }
    //
    //     if (!player.Crouching && !player.Diving && !player.Climbing && !player.Staggering && !player.LayingOnGround && !player.Falling)
    //     {
    //         if (AirTime > 250 && (player.VirtualKeyboard.PressingKey(0) || player.VirtualKeyboard.PressingKey(19)))
    //         {
    //             if (Landed)
    //             {
    //                 Landed = false;
    //                 SoundHandler.PlaySound("Bazooka", player.GameWorld);
    //             }
    //
    //             var velocity = player.CurrentVelocity;
    //             velocity.X *= player.SlowmotionFactor * 0.6f;
    //
    //             if (velocity.Y <= MaxSpeed)
    //             {
    //                 velocity.Y = (velocity.Y > 1.96f ? velocity.Y : 1.96f) * player.SlowmotionFactor * 1.17f;
    //             }
    //             else
    //             {
    //                 velocity.Y = MaxSpeed;
    //             }
    //
    //             player.SetNewLinearVelocity(velocity);
    //             // player.ImportantUpdate = true;
    //
    //             if (!player.InfiniteAmmo && !player.InfiniteAmmo)
    //             {
    //                 Fuel.CurrentValue -= 0.8f * player.SlowmotionFactor;
    //                 GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, player.ObjectID, extendedPlayer.GetStates()));
    //             }
    //
    //             EffectTimer -= ms;
    //             if (EffectTimer <= 0f)
    //             {
    //                 PlayEffect(player);
    //             }
    //
    //             SoundTimer -= ms;
    //             if (SoundTimer < 0f)
    //             {
    //                 PlaySound(player);
    //             }
    //
    //             Shake = true;
    //         }
    //         else
    //         {
    //             Shake = false;
    //         }
    //
    //         if (Fuel.CurrentValue <= 0 && extendedPlayer.JetpackType != JetpackType.None)
    //         {
    //             Discard(extendedPlayer);
    //         }
    //     }
    // }

    internal override Texture2D GetJetpackTexture(string postFix)
    {
        Jetpack ??= Textures.GetTexture("JetpackNormal");
        JetpackBack ??= Textures.GetTexture("JetpackNormalBack");
        JetpackDiving ??= Textures.GetTexture("JetpackNormalDiving");

        var texture = postFix switch
        {
            "" => Jetpack,
            "Back" => JetpackBack,
            "Diving" => JetpackDiving,
            _ => null
        };

        return texture;
    }

    protected override void Discard(ExtendedPlayer extendedPlayer)
    {
        base.Discard(extendedPlayer);
        var player = extendedPlayer.Player;

        // if (player.GameOwner != GameOwnerEnum.Client)
        // {
        player.GameWorld.CreateTile("JetpackDebris1", player.Position, 0);
        // }
    }
}