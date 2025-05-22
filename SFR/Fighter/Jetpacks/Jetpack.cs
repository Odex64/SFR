﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Sounds;
using SFD.Tiles;

namespace SFR.Fighter.Jetpacks;

internal sealed class Jetpack : GenericJetpack
{
    internal Jetpack() : base(100, 0.4f)
    {
    }

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

    internal override Texture2D GetJetpackTexture(string postFix)
    {
        Jetpack ??= Textures.GetTexture("Jetpack");
        JetpackBack ??= Textures.GetTexture("JetpackBack");
        JetpackDiving ??= Textures.GetTexture("JetpackDiving");

        Texture2D texture = postFix switch
        {
            "" => Jetpack,
            "Back" => JetpackBack,
            "Diving" => JetpackDiving,
            _ => null
        };

        return texture;
    }

    protected internal override void Discard(ExtendedPlayer extendedPlayer)
    {
        base.Discard(extendedPlayer);
        Player player = extendedPlayer.Player;

        _ = player.GameWorld.CreateTile("JetpackDebris", player.Position, 0);
    }
}