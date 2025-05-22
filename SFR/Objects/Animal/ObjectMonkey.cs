using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;

namespace SFR.Objects.Animal;

internal sealed class ObjectMonkey : ObjectAnimal
{
    private const float _animSpeed1 = 0.2f;
    private const float _animSpeed2 = 2f;

    internal ObjectMonkey(ObjectDataStartParams startParams) : base(startParams)
    {
    }

    public override void Initialize()
    {
        JumpSound = "PlayerJump";
        MaxCheckInterval = 5000;
        MinCheckInterval = 500;
        MaxJumpForce = 8;
        MinJumpForce = 2;
        base.Initialize();
    }

    public override void Draw(SpriteBatch spriteBatch, float ms)
    {
        float len = GetLinearVelocity().Length();
        switch (len)
        {
            case < _animSpeed1:
                CurrentAnimation.SetFrame(0);
                break;
            case < _animSpeed2:
                CurrentAnimation.SetFrame(1);
                break;
            default:
                CurrentAnimation.SetFrame(2);
                break;
        }

        DrawBase(spriteBatch, ms, new Color(0.5f, 0.5f, 0.5f, 1));
    }
}