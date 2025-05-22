using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;

namespace SFR.Objects.Animal;

internal sealed class ObjectFroggy : ObjectAnimal
{
    private const float _animSpeed1 = 0.1f;
    private const float _animSpeed2 = 1f;

    internal ObjectFroggy(ObjectDataStartParams startParams) : base(startParams)
    {
    }

    public override void Initialize()
    {
        JumpSound = "FootstepConcrete";
        MaxCheckInterval = 10000;
        MinCheckInterval = 500;
        MaxJumpForce = 5;
        MinJumpForce = 1;
        base.Initialize();

        // idk why i cant give it color :((
        // var colors = Colors;
        // colors[0] = Tile.ColorPalette.GetRandomColorFromLevel(0);
        // colors[1] = Tile.ColorPalette.GetRandomColorFromLevel(1);
        // ApplyColors(colors, true);
    }

    public override void Draw(SpriteBatch spriteBatch, float ms)
    {
        float len = GetLinearVelocity().Length();
        switch (len)
        {
            case < _animSpeed1:
                {
                    float num = GameWorld.ElapsedTotalGameTime % 4000;
                    switch (num)
                    {
                        case < 100:
                            CurrentAnimation.SetFrame(0);
                            break;
                        case < 200:
                            CurrentAnimation.SetFrame(1);
                            break;
                        case < 300:
                            CurrentAnimation.SetFrame(2);
                            break;
                        case < 400:
                            CurrentAnimation.SetFrame(1);
                            break;
                        default:
                            CurrentAnimation.SetFrame(3);
                            break;
                    }

                    break;
                }
            case < _animSpeed2:
                CurrentAnimation.SetFrame(4);
                break;
            default:
                CurrentAnimation.SetFrame(5);
                break;
        }

        DrawBase(spriteBatch, ms, new Color(0.5f, 0.5f, 0.5f, 1));
    }
}