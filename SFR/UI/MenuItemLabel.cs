using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD.MenuControls;

namespace SFR.UI;

internal class MenuItemLabel : MenuItem
{
    private readonly Label _label;

    internal MenuItemLabel(string text, Align alignment, Color color, OnKeyPress chooseEvent = null)
    {
        _label = new Label(text, color)
        {
            TextAlign = alignment
        };
        chooseEvent ??= delegate { };
        ChooseEvent = (ControlEvents.ChooseEvent)Delegate.Combine(ChooseEvent, new ControlEvents.ChooseEvent(chooseEvent));
    }

    public override void Initialize(Menu parentMenu)
    {
        base.Initialize(parentMenu);
        _label.Width = parentMenu.Width;
    }

    public override void Select()
    {
        base.Select();
        _label.SetScrollingText(true);
    }

    public override void Deselect()
    {
        base.Deselect();
        _label.SetScrollingText(false);
    }

    public override void Draw(SpriteBatch batch, float elapsed)
    {
        if (ParentMenu is { Focus: Focus.None })
        {
            _label.SetScrollingText(false);
        }

        base.Draw(batch, elapsed);
        _label.Draw(batch, elapsed, new Vector2(Position.X + ParentMenu.Width / 2, Position.Y));
    }

    internal delegate void OnKeyPress(object sender);
}