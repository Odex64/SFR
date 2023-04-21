using System;
using System.Collections.Generic;
using SFD;
using SFD.Sounds;
using SFDGameScriptInterface;
using SFR.Helper;
using SFR.Sync.Generic;

namespace SFR.Fighter;

/// <summary>
///     Since we need to save additional data into the player instance
///     we use this file to "extend" the player class.
/// </summary>
internal sealed class ExtendedPlayer : IEquatable<Player>
{
    internal static readonly List<ExtendedPlayer> ExtendedPlayers = new();
    private readonly Player _player;
    internal readonly TimeSequence Time = new();

    internal bool Stickied;

    internal ExtendedPlayer(Player player) => _player = player;

    internal bool RageBoost
    {
        get => Time.RageBoost > 0f;
        set => Time.RageBoost = value ? TimeSequence.RageBoostTime : 0f;
    }

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

    // TODO: Change other methods instead of using modifiers, like strength boost & speed boost do
    internal void ApplyRageBoost()
    {
        // var modifiers = _player.GetModifiers();
        var modifiers = new PlayerModifiers(true)
        {
            SprintSpeedModifier = 1.3f,
            RunSpeedModifier = 1.3f,
            SizeModifier = 1.05f,
            MeleeForceModifier = 1.2f
        };
        _player.SetModifiers(modifiers);
        RageBoost = true;
        GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, _player.ObjectID, GetStates()));
    }

    public bool[] GetStates()
    {
        bool[] states = new bool[1];
        states[0] = RageBoost;

        return states;
    }

    // TODO: Change other methods instead of using modifiers, like strength boost & speed boost do
    internal void DisableRageBoost()
    {
        SoundHandler.PlaySound("StrengthBoostStop", _player.Position, _player.GameWorld);
        // var modifiers = _player.GetModifiers();
        // modifiers.SprintSpeedModifier = 1f;
        // modifiers.RunSpeedModifier = 1f;
        // modifiers.SizeModifier = 1f;
        // modifiers.MeleeForceModifier = 1f;
        var modifiers = new PlayerModifiers(true);
        _player.SetModifiers(modifiers);
        GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, _player.ObjectID, GetStates()));
    }

    internal class TimeSequence
    {
        internal const float RageBoostTime = 16000f;
        internal float RageBoost;
    }
}