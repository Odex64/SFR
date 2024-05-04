using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;

namespace SFR.Fighter;

internal static class NameIconHandler
{
    private static Vector2 CalculatePosition(Player player, Vector2 vec, Texture2D icon, float num) => new(vec.X - player.m_nameTextSize.X * 0.25f * num - icon.Width * num - 4f, vec.Y - player.m_nameTextSize.Y * num);

    private static void DrawDeveloperIcon(Player player, Vector2 vec, float num)
    {
        var user = player.GetGameUser();
        if (user is not null && DevHandler.GetDeveloperIcon(user.Account) is { } icon)
        {
            player.m_spriteBatch.Draw(
                icon,
                CalculatePosition(player, vec, icon, num),
                null,
                Color.Gray,
                0f,
                Vector2.Zero,
                num,
                SpriteEffects.None,
                1f
            );
        }
    }

    private static void DrawMemberIcon(Player player, Vector2 vec, float num)
    {
        var icon = Constants.GetTeamIcon(player.m_currentTeam);

        player.m_spriteBatch.Draw(
            icon,
            new(vec.X - player.m_nameTextSize.X * 0.25f * num - icon.Width * num, vec.Y - player.m_nameTextSize.Y * num),
            null,
            Color.Gray,
            0f,
            Vector2.Zero,
            num,
            SpriteEffects.None,
            1f
        );
    }

    private static void DrawName(Player player, Vector2 vec, float num) => Constants.DrawString(
            player.m_spriteBatch,
            Constants.Font1Outline,
            player.Name, new(vec.X, vec.Y - 0.75f * player.m_nameTextSize.Y * num),
            player.GetTeamTextColor(),
            0f,
            player.m_nameTextSize * 0.5f,
            num * 0.5f,
            SpriteEffects.None,
            0
        );

    internal static void Draw(Player player, Vector2 vec, float num)
    {
        if (player.IsDead || player.IsRemoved)
        {
            return;
        }

        if ((player.DrawStatusInfo & Player.DrawStatusInfoFlags.Name) != Player.DrawStatusInfoFlags.Name)
        {
            return;
        }

        DrawName(player, vec, num);

        if (!player.IsBot)
        {
            var user = player.GetGameUser();
            if (user is not null && DevHandler.IsDeveloper(user.Account))
            {
                DrawDeveloperIcon(player, vec, num);
            }
        }
        else
        {
            DrawMemberIcon(player, vec, num);
        }
    }
}