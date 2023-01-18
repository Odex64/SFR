using System;
using System.Collections.Generic;
using SFD;
using SFD.Sounds;

namespace SFR.Fighter;

internal sealed class ExtendedPlayer : IEquatable<Player>
{
    internal static readonly List<ExtendedPlayer> ExtendedPlayers = new();
    private readonly Player _player;

    internal bool Stickied;

    internal ExtendedPlayer(Player player)
    {
        _player = player;
    }

    internal void ApplyStickiedBoost()
    {
        SoundHandler.PlaySound("StrengthBoostStart", _player.Position, _player.GameWorld);
        var modifiers = _player.GetModifiers();
        modifiers.SprintSpeedModifier = 1.6f;
        modifiers.RunSpeedModifier = 1.6f;
        _player.SetModifiers(modifiers);
        Stickied = true;
    }

    internal void DisableStickiedBoost()
    {
        SoundHandler.PlaySound("StrengthBoostStop", _player.Position, _player.GameWorld);
        var modifiers = _player.GetModifiers();
        modifiers.SprintSpeedModifier = 1f;
        modifiers.RunSpeedModifier = 1f;
        _player.SetModifiers(modifiers);
        Stickied = false;
    }

    public bool Equals(Player other) => other?.ObjectID == _player.ObjectID;
}