using System;
using Microsoft.Xna.Framework;
using SFD;

namespace SFR.Fighter;

internal static class StatusBarHandler
{
    private static void DrawHealth(Player player, Rectangle rec, BarMeter bm, Color bc)
    {
        if (bm.CheckRecentlyModified(50f))
        {
            bc = Color.White;
        }

        player.m_spriteBatch.Draw(Constants.WhitePixel, rec, new Color(64, 64, 64));
        rec.Width = (int)(rec.Width * bm.Fullness);

        player.m_spriteBatch.Draw(Constants.WhitePixel, rec, ColorCorrection.FromXNAToCustom(bc));
    }

    private static void DrawFuel(Player player, Rectangle rec)
    {
        var extendedPlayer = Helper.Fighter.GetExtension(player);
        if (extendedPlayer.GenericJetpack == null)
        {
            return;
        }

        player.m_spriteBatch.Draw(Constants.WhitePixel, rec, new Color(64, 64, 64));
        rec.Width = (int)(rec.Width * extendedPlayer.GenericJetpack.Fuel.Fullness);

        player.m_spriteBatch.Draw(Constants.WhitePixel, rec, ColorCorrection.FromXNAToCustom(Constants.COLORS.PING_YELLOW));
    }

    private static void DrawEnergy(Player player, Rectangle rec)
    {
        player.m_spriteBatch.Draw(Constants.WhitePixel, rec, new Color(64, 64, 64));
        rec.Width = (int)(rec.Width * player.Energy.Fullness);

        player.m_spriteBatch.Draw(Constants.WhitePixel, rec, ColorCorrection.FromXNAToCustom(Constants.COLORS.ENERGY_BAR));
    }

    internal static void Draw(Player player, Vector2 vec, float num)
    {
        if ((player.DrawStatusInfo & Player.DrawStatusInfoFlags.StatusBars) != Player.DrawStatusInfoFlags.StatusBars)
        {
            return;
        }

        if (player is not { IsDead: false, IsRemoved: false })
        {
            return;
        }

        // 11f to 10f
        vec.Y -= 10f * num;

        var barColor = Constants.COLORS.LIFE_BAR;
        var barMeter = player.Health;

        var healthMode = player.GetCurrentHealthMode();
        bool flag = barMeter.CheckRecentlyModified(2000f);

        if (healthMode is Player.HealthMode.StrengthBoostOverHealth or Player.HealthMode.RocketRideOverHealth)
        {
            barMeter = player.OverHealth;

            barColor = (int)(GameSFD.LastUpdateNetTimeMS / 200f) % 2 == 0 ? Constants.COLORS.LIFE_BAR_OVERHEALTH_A : Constants.COLORS.LIFE_BAR_OVERHEALTH_B;

            flag = true;
        }

        if (!(flag | player.Energy.CheckRecentlyModified(2000f)))
        {
            return;
        }

        float f = 32f * num;

        Rectangle destinationRectangle = new((int)(vec.X - f / 2f), (int)vec.Y, (int)f, (int)(2f * num));
        float n4 = Math.Max(1f, Camera.Zoom * 0.5f);

        // What's doing this?
        for (float num5 = -n4; num5 <= n4; num5 += n4 * 2f)
        {
            for (float num6 = -n4; num6 <= n4; num6 += n4 * 2f)
            {
                player.m_spriteBatch.Draw(
                    Constants.WhitePixel,
                    new Rectangle(destinationRectangle.X + (int)num5, destinationRectangle.Y + (int)num6, destinationRectangle.Width, (int)(destinationRectangle.Height * 2f)),
                    Color.Black
                );
            }
        }

        destinationRectangle.Y -= destinationRectangle.Height;
        DrawFuel(player, destinationRectangle);

        destinationRectangle.Y += destinationRectangle.Height;
        DrawHealth(player, destinationRectangle, barMeter, barColor);

        destinationRectangle.Y += destinationRectangle.Height;
        DrawEnergy(player, destinationRectangle);
    }
}