using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Tiles;
using SFR.Game;
using SFR.Helper;

namespace SFR.Objects;

internal sealed class ObjectNuke : ObjectData
{
    private const float CreateBoomInterval = 5;
    private readonly List<Boom> _booms = [];
    private float _createBoomTimer;
    internal bool IsActive;
    internal float Progress = 0f;

    internal ObjectNuke(ObjectDataStartParams startParams) : base(startParams) { }

    public override void Draw(SpriteBatch spriteBatch, float ms)
    {
        if (IsActive)
        {
            int width = (int)Camera.ConvertWorldToScreenX(Camera.WorldRight - Camera.WorldLeft);
            int height = (int)Camera.ConvertWorldToScreenX(Camera.WorldTop - Camera.WorldBottom);

            var color = Color.White;
            var texture = Textures.GetTexture("NukeGradient");

            if (Tile.Name == "BgNuke")
            {
                color = Color.Yellow;
            }

            if (Progress > NukeHandler.FadeTime)
            {
                float value = ExtendedMath.InverseLerp(NukeHandler.FadeTime, 1, Progress);
                float fade = ExtendedMath.Lerp(0, 255, ExtendedMath.Clamp(1 - value));
                color.A = (byte)fade;
            }

            Rectangle rect = new((int)Camera.WorldLeft - (int)(width * (1 - Progress * 1.5)) + width / 2, (int)Camera.WorldBottom - 1, width / 2, height);
            Rectangle rect2 = new(rect.X - width, rect.Y, width, height);

            spriteBatch.Draw(texture, rect, color);
            spriteBatch.Draw(Constants.WhitePixel, rect2, color);
            if (Tile.Name == "FgNuke" && Constants.EFFECT_LEVEL_FULL)
            {
                foreach (var boom in _booms.Where(boom => boom.IsActive))
                {
                    boom.Draw(spriteBatch, ms);
                }

                _createBoomTimer += ms;
                if (_createBoomTimer > CreateBoomInterval)
                {
                    _createBoomTimer = 0;
                    Vector2 pos = new(rect.X + width / 4, Misc.Constants.Random.Next(0, height));
                    _booms.Add(new Boom(pos));
                }
            }
        }
    }

    internal void Delete()
    {
        IsActive = false;
    }
}

internal sealed class Boom
{
    private const float FrameTime = 20;
    private static Texture2D _sheet;
    private static int _frameCount;
    private readonly Vector2 _position;
    private float _timer;
    internal bool IsActive;

    internal Boom(Vector2 position)
    {
        if (_sheet == null)
        {
            _sheet = Textures.GetTexture("NukeBooms");
            _frameCount = _sheet.Width / _sheet.Height;
        }

        _position = position;
        IsActive = true;
    }

    internal void Draw(SpriteBatch spriteBatch, float ms)
    {
        _timer += ms;
        if (_timer > _frameCount * FrameTime)
        {
            IsActive = false;
            return;
        }

        int frame = (int)(_timer / FrameTime);
        spriteBatch.Draw(_sheet, _position, new Rectangle(frame * _sheet.Height, 0, _sheet.Height, _sheet.Height), new Color(0.5f, 0.5f, 0.5f, 1f), 0, new Vector2(0, 0), Camera.Zoom, SpriteEffects.None, 0f);
    }
}