using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Sounds;
using SFD.Weapons;
using SFR.Helper;
using SFR.Weapons;

namespace SFR.Fighter.Jetpacks;

[HarmonyPatch]
internal static class JetpackHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.ActivateObject))]
    private static void CheckDisposeKey(Player __instance)
    {
        var closedObject = __instance.GetClosestActivateableObject(true, false, 0f, 0f);
        if (closedObject is null)
        {
            var extendedPlayer = __instance.GetExtension();
            if (extendedPlayer.GenericJetpack is { State: JetpackState.Idling } jetpack && __instance.Crouching)
            {
                SoundHandler.PlaySound("PistolDraw", __instance.GameWorld);
                EffectHandler.PlayEffect("CFTXT", __instance.Position, __instance.GameWorld, "DISCARD");
                jetpack.Discard(extendedPlayer);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WpnFireAmmo), nameof(WpnFireAmmo.OnPickup))]
    private static void ApplyFireUpgrade(Player player)
    {
        var extendedPlayer = player.GetExtension();

        if (extendedPlayer.GenericJetpack is Gunpack gunpack)
        {
            gunpack.ApplyFireUpgrade();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.Draw))]
    private static bool PreDraw(SpriteBatch spriteBatch, Player __instance)
    {
        var extendedPlayer = __instance.GetExtension();
        if (extendedPlayer.GenericJetpack is null)
        {
            return true;
        }

        if (!__instance.Climbing)
        {
            if (__instance.Rolling || __instance.LayingOnGround || __instance.RocketRideProjectile is not null)
            {
                return true;
            }

            // Set angle and position according to current state
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

            if (__instance.GrabbedByPlayer is not null)
            {
                gamePos -= new Vector2(0, 8);
            }

            // Draw
            if (extendedPlayer.GenericJetpack.Shake)
            {
                Vector2 extra = new(0, 0.75f);
                SFDMath.RotatePosition(ref extra, Constants.random.NextFloat(0, (float)Math.PI * 2), out extra);
                gamePos += extra;
            }

            gamePos = new(Converter.WorldToBox2D(gamePos.X), Converter.WorldToBox2D(gamePos.Y));
            Camera.ConvertBox2DToScreen(ref gamePos, out gamePos);
            spriteBatch.Draw(texture, gamePos, null, new(0.5f, 0.5f, 0.5f, 1f), angle, new(texture.Width / 2, texture.Height / 2), Camera.ZoomUpscaled, effect, 0f);
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.Draw))]
    private static void PostDraw(SpriteBatch spriteBatch, float ms, Player __instance)
    {
        object weapon = __instance.GetCurrentWeapon();
        if (weapon is IExtendedWeapon wep)
        {
            wep.DrawExtra(spriteBatch, __instance, ms);
        }

        var extendedPlayer = __instance.GetExtension();
        if (extendedPlayer.GenericJetpack is not null)
        {
            if (__instance.Climbing)
            {
                var gamePos = __instance.Position + new Vector2(0, 12);
                gamePos = new(Converter.WorldToBox2D(gamePos.X), Converter.WorldToBox2D(gamePos.Y));
                Camera.ConvertBox2DToScreen(ref gamePos, out gamePos);
                var texture = extendedPlayer.GenericJetpack.GetJetpackTexture("Back");
                spriteBatch.Draw(texture, gamePos, null, new(0.5f, 0.5f, 0.5f, 1f), 0, new(texture.Width / 2, texture.Height / 2), Camera.ZoomUpscaled, SpriteEffects.None, 0f);
            }
        }
    }
}