using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;

namespace SFR.Fighter.Jetpacks;

[HarmonyPatch]
internal static class JetpackHandler
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    private static void RunJetpackUpdates(float ms, float realMs, Player __instance)
    {
        if (__instance.GameOwner != GameOwnerEnum.Client)
        {
            var extendedPlayer = Helper.Fighter.GetExtension(__instance);
            extendedPlayer.GenericJetpack?.Update(ms, extendedPlayer);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.Draw))]
    private static bool PreDraw(SpriteBatch spriteBatch, float ms, Player __instance)
    {
        var extendedPlayer = Helper.Fighter.GetExtension(__instance);
        if (extendedPlayer.GenericJetpack == null)
        {
            return true;
        }

        if (!__instance.Climbing)
        {
            //Set angle and position according to current state
            var gamePos = __instance.Position + new Vector2(-4 * __instance.LastDirectionX, 12);
            var effect = __instance.LastDirectionX == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            var texture = extendedPlayer.GenericJetpack.GetJetpackTexture(string.Empty);
            float angle = 0;
            if (__instance.Diving)
            {
                var aim = __instance.AimVector();
                angle = __instance.DiveRotation;
                gamePos = __instance.Position + new Vector2(aim.X, aim.Y) * 2 + new Vector2(0, 4);
                Vector2 extra = new(aim.X, aim.Y);
                if (__instance.LastDirectionX == 1)
                {
                    SFDMath.RotateVector90CW(ref extra, out extra);
                }
                else
                {
                    SFDMath.RotateVector90CCW(ref extra, out extra);
                }

                gamePos += extra * 4;
                texture = extendedPlayer.GenericJetpack.GetJetpackTexture("Diving");
            }

            if (__instance.Falling)
            {
                angle = (float)(__instance.LastFallingRotation + Math.PI / 2f * __instance.LastDirectionX);
                gamePos = __instance.Position;
            }

            if (__instance.Crouching)
            {
                gamePos = __instance.Position + new Vector2(-4 * __instance.LastDirectionX, 8);
            }

            if (__instance.TakingCover)
            {
                gamePos = __instance.Position + new Vector2(4 * __instance.LastDirectionX, 8);
                effect = __instance.LastDirectionX == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }

            if (__instance.LedgeGrabbing)
            {
                gamePos -= new Vector2(0, 8);
            }

            if (__instance.Rolling || __instance.LayingOnGround || __instance.RocketRideProjectile != null)
            {
                return true;
            }

            // Draw
            if (extendedPlayer.GenericJetpack.Shake)
            {
                Vector2 extra = new(0, 0.75f);
                SFDMath.RotatePosition(ref extra, Constants.random.NextFloat(0, (float)Math.PI * 2), out extra);
                gamePos += extra;
            }

            gamePos = new Vector2(Converter.WorldToBox2D(gamePos.X), Converter.WorldToBox2D(gamePos.Y));
            Camera.ConvertBox2DToScreen(ref gamePos, out gamePos);
            spriteBatch.Draw(texture, gamePos, null, new Color(0.5f, 0.5f, 0.5f, 1f), angle, new Vector2(texture.Width / 2, texture.Height / 2), Camera.ZoomUpscaled, effect, 0f);
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.Draw))]
    private static void PostDraw(SpriteBatch spriteBatch, float ms, Player __instance)
    {
        var extendedPlayer = Helper.Fighter.GetExtension(__instance);
        if (extendedPlayer.GenericJetpack == null)
        {
            return;
        }

        if (__instance.Climbing) //TODO why jetpackActive == false?
        {
            var gamePos = __instance.Position + new Vector2(0, 12);
            gamePos = new Vector2(Converter.WorldToBox2D(gamePos.X), Converter.WorldToBox2D(gamePos.Y));
            Camera.ConvertBox2DToScreen(ref gamePos, out gamePos);
            var texture = extendedPlayer.GenericJetpack.GetJetpackTexture("Back");
            spriteBatch.Draw(texture, gamePos, null, new Color(0.5f, 0.5f, 0.5f, 1f), 0, new Vector2(texture.Width / 2, texture.Height / 2), Camera.ZoomUpscaled, SpriteEffects.None, 0f);
        }
    }
}