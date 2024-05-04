using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Tiles;

namespace SFR.Objects;

internal class ObjectPirateShip : ObjectData
{
    private const int _damageFrames = 5;
    private Texture2D[] _textures;

    internal ObjectPirateShip(ObjectDataStartParams startParams) : base(startParams) { }

    public override void Initialize()
    {
        _textures = new Texture2D[_damageFrames];
        base.Initialize();
        for (int i = 0; i < _damageFrames; i++)
        {
            string str = "PirateShipSmall00";
            if (i != 0)
            {
                str += "_" + i;
            }

            _textures[i] = Textures.GetTexture(str);
        }
    }

    public override void DealScriptDamage(float damage, int sourceID = 0)
    {
        base.DealScriptDamage(damage, sourceID);
        if (!Health.IsEmpty)
        {
            int frame = (int)(_damageFrames - Health.Fullness * _damageFrames);
            ClearDecals();
            AddDecal(new(_textures[frame]));
        }
    }

    public override void Draw(SpriteBatch spriteBatch, float ms)
    {
        if (!Health.IsEmpty)
        {
            int frame = (int)(_damageFrames - Health.Fullness * _damageFrames);
            ClearDecals();
            AddDecal(new(_textures[frame]));
            base.Draw(spriteBatch, ms);
        }
    }
}