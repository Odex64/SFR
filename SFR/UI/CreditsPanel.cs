using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SFD.MenuControls;

namespace SFR.UI;

internal sealed class CreditsPanel : Panel
{
    internal CreditsPanel() : base("CREDITS", 360, 420)
    {
        List<MenuItem> items =
        [
            new MenuItemSeparator("MISC"),
            new MenuItemLabel("Project Repo", Align.Center, Color.BlueViolet, _ => Process.Start("https://github.com/Odex64/SFR")),
            new MenuItemLabel("Discord", Align.Center, Color.BlueViolet, _ => Process.Start("https://discord.gg/CqYZfazH6M")),

            new MenuItemSeparator("PROGRAMMERS"),
            new MenuItemLabel("Odex64", Align.Center, Color.Orange),
            new MenuItemLabel("Argón", Align.Center, Color.Orange),

            new MenuItemSeparator("ARTISTS"),
            new MenuItemLabel("Shock", Align.Center, Color.Lime),
            new MenuItemLabel("Dxse", Align.Center, Color.Lime),
            new MenuItemLabel("KLI", Align.Center, Color.Lime),
            new MenuItemLabel("Danila015", Align.Center, Color.Lime),
            new MenuItemLabel("Eiga", Align.Center, Color.Lime),

            new MenuItemSeparator("COMPOSERS"),
            new MenuItemLabel("Samwow", Align.Center, Color.Red),

            new MenuItemSeparator("SPECIAL THANKS"),
            new MenuItemLabel("Odex64 - Founder & Project Leader", Align.Center, Color.Gold),
            new MenuItemLabel("Eiga, Heapons, Olv - Moderation", Align.Center, Color.Gold),
            new MenuItemLabel("Mimyuu - For the special font ", Align.Center, Color.Gold),
            new MenuItemLabel("Motto73 - For his extensive work on the mod", Align.Center, Color.Gold),
            new MenuItemLabel("NearHuscarl - For his amazing items editor", Align.Center, Color.Gold)
        ];

        Menu menu = new(new Vector2(0f, 40f), Width, Height - 40, this, [.. items]);
        members.Add(menu);
    }

    private void Close() => ParentPanel.CloseSubPanel();

    public override void KeyPress(Keys key)
    {
        if (subPanel is null && key == Keys.Escape)
        {
            Close();
            return;
        }

        base.KeyPress(key);
    }
}