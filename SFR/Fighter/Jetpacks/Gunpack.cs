using System;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Sounds;
using SFD.Tiles;
using SFDGameScriptInterface;
using SFR.Helper;
using Constants = SFR.Misc.Constants;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SFR.Fighter.Jetpacks;

internal sealed class Gunpack : GenericJetpack
{
    private ushort _fireProjectiles;
    private float _fireRate;
    private float _projectileTimer;

    internal Gunpack() : base(140, 1.2f) { }

    protected override void PlayEffect(Player player)
    {
        EffectHandler.PlayEffect("FNDTRA", player.Position + new Vector2(-4 * player.LastDirectionX, -4), player.GameWorld);
        EffectTimer = 20f;
    }

    internal void ApplyFireUpgrade(ushort amount = 80)
    {
        _fireProjectiles = amount;
    }

    protected override void PlaySound(Player player)
    {
        SoundHandler.PlaySound("Flamethrower", player.GameWorld);
        SoundTimer = 50f;
    }

    internal override void Update(float ms, ExtendedPlayer extendedPlayer)
    {
        base.Update(ms, extendedPlayer);

        _projectileTimer -= ms;

        if (!extendedPlayer.Player.Falling && extendedPlayer.Player.VirtualKeyboard.PressingKey(19) && AirTime > 300f)
        {
            if (_projectileTimer - _fireRate <= 0f)
            {
                var gameWorld = extendedPlayer.Player.GameWorld;
                var position = extendedPlayer.Player.Position - new Vector2(0, 8);

                gameWorld.SpawnProjectile(40, position, new Vector2(Constants.Random.NextFloat(-0.1f, 0.1f), -1), extendedPlayer.Player.ObjectID, _fireProjectiles > 0 ? ProjectilePowerup.Fire : ProjectilePowerup.None);
                if (_fireProjectiles > 0)
                {
                    _fireProjectiles--;
                }

                _projectileTimer = 240f;

                if (_fireRate + ms > 200f)
                {
                    _fireRate = 200f;
                }
                else
                {
                    _fireRate += ms;
                }

                if (_fireRate >= _projectileTimer)
                {
                    throw new Exception("Wrong gunpack fire rate");
                }
            }
        }
        else
        {
            _fireRate -= ms;

            if (_fireRate - ms < 0f)
            {
                _fireRate = 0f;
            }
        }
    }

    internal override Texture2D GetJetpackTexture(string postFix)
    {
        Jetpack ??= Textures.GetTexture("Gunpack");
        JetpackBack ??= Textures.GetTexture("GunpackBack");
        JetpackDiving ??= Textures.GetTexture("GunpackDiving");

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

        player.GameWorld.CreateTile("GunpackDebris", player.Position, 0);
    }
}