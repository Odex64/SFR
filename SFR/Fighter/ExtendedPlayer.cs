using System;
using System.Collections.Generic;
using SFD;
using SFD.Sounds;

namespace SFR.Fighter;

/// <summary>
///     Since we need to save additional data into the player instance
///     we use this file to "extend" the player class.
/// </summary>
internal sealed class ExtendedPlayer : IEquatable<Player>
{
    internal static readonly List<ExtendedPlayer> ExtendedPlayers = new();
    private readonly Player _player;

    internal bool Stickied;

    internal ExtendedPlayer(Player player) => _player = player;

    public bool Equals(Player other) => other?.ObjectID == _player.ObjectID;

    internal void ApplyStickiedBoost()
    {
        SoundHandler.PlaySound("StrengthBoostStart", _player.Position, _player.GameWorld);
        var modifiers = _player.GetModifiers();
        modifiers.SprintSpeedModifier = 1.6f;
        modifiers.RunSpeedModifier = 1.6f;
        _player.SetModifiers(modifiers);

        // avoid the server from reposition the player due to excessive speed.
        // TODO: Implement a proper mechanics in Player.GetTopSpeed()
        // _player.SpeedBoostActive = true;
        Stickied = true;
    }

    internal void DisableStickiedBoost()
    {
        SoundHandler.PlaySound("StrengthBoostStop", _player.Position, _player.GameWorld);
        var modifiers = _player.GetModifiers();
        modifiers.SprintSpeedModifier = 1f;
        modifiers.RunSpeedModifier = 1f;
        _player.SetModifiers(modifiers);

        // _player.SpeedBoostActive = false; // temp
        Stickied = false;
    }
}