using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Sounds;
using SFD.Tiles;

namespace SFR.Fighter.Jetpacks;

internal sealed class Jetpack : GenericJetpack
{
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