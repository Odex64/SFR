using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;
using SFR.Helper;
using SFR.Weapons.Makeshift;
using Player = SFD.Player;

namespace SFR.Objects;

internal sealed class ObjectCannon : ObjectActivateTrigger
{
    private bool _isLoaded;

    internal ObjectCannon(ObjectDataStartParams startParams) : base(startParams)
    {
    }

    public override void Initialize()
    {
        base.Initialize();
        DoDraw = true;
        m_highlightObject = this;
    }

    public override void Activate(ObjectData sender)
    {
        if (GameOwner != GameOwnerEnum.Client)
        {
            if (_isLoaded)
            {
                _isLoaded = false;
                FireCannon();
            }
            else if (sender is { IsPlayer: true })
            {
                Player player = GameWorld.GetPlayer(sender.BodyID);
                if (player is { IsRemoved: false })
                {
                    if (player.CurrentMeleeMakeshiftWeapon is CannonBall)
                    {
                        player.RemoveWeaponItem(WeaponItemType.Melee);
                        _isLoaded = true;
                        SoundHandler.PlaySound("CoinSlot", GameWorld);
                        EffectHandler.PlayEffect("GLM", GetMuzzle(), GameWorld);
                        Logger.LogDebug("Reloaded");
                    }
                    else
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            EffectHandler.PlayEffect("TR_S", GetMuzzle(), GameWorld);
                        }

                        SoundHandler.PlaySound("DestroySmall", GameWorld);
                    }
                }
            }
        }

        base.Activate(sender);
    }

    public override ObjectData GetActivateableHighlightObject(Player player) => this;

    public override void Draw(SpriteBatch spriteBatch, float ms)
    {
        DrawBase(spriteBatch, ms, new Color(0.5f, 0.5f, 0.5f, 1f));
        if (GameWorld.EditTestMode)
        {
            string text = _isLoaded ? "LOADED" : "EMPTY";
            Vector2 vector = Camera.ConvertWorldToScreen(GetWorldPosition() + new Vector2(-4f, 8.5f));
            float scale = Camera.Zoom * 0.3f;
            _ = Constants.DrawString(spriteBatch, Constants.FontSimple, text, vector + new Vector2(1f), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
            _ = Constants.DrawString(spriteBatch, Constants.FontSimple, text, vector - new Vector2(1f), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
            _ = Constants.DrawString(spriteBatch, Constants.FontSimple, text, vector + new Vector2(1f, -1f), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
            _ = Constants.DrawString(spriteBatch, Constants.FontSimple, text, vector + new Vector2(-1f, 1f), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
            _ = Constants.DrawString(spriteBatch, Constants.FontSimple, text, vector, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
    }

    private Vector2 GetMuzzle()
    {
        Vector2 pos = new(FaceDirection == 1 ? 16 : -16, 0);
        SFDMath.RotatePosition(ref pos, GetAngle(), out pos);
        return GetWorldPosition() + pos;
    }

    private void FireCannon()
    {
        for (int i = 0; i < 8; i++)
        {
            EffectHandler.PlayEffect("TR_S", GetMuzzle(), GameWorld);
        }

        EffectHandler.PlayEffect("EXP", GetMuzzle(), GameWorld);
        SoundHandler.PlaySound("BarrelExplode", GameWorld);
        SoundHandler.PlaySound("Explosion", GameWorld);
        EffectHandler.PlayEffect("CAM_S", Vector2.Zero, GameWorld, 1f, 250f, false);
        Vector2 angle = new(FaceDirection * 16, 0);
        SFDMath.RotatePosition(ref angle, GetAngle(), out angle);
        _ = GameWorld.SpawnProjectile(93, GetMuzzle(), angle, ObjectID);
    }
}