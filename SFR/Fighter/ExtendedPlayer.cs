using System;
using System.Runtime.CompilerServices;
using SFD;
using SFD.Sounds;
using SFR.Fighter.Jetpacks;
using SFR.Sync.Generic;

namespace SFR.Fighter;

/// <summary>
///     Since we need to save additional data into the player instance
///     we use this file to "extend" the player class.
/// </summary>
internal sealed class ExtendedPlayer : IEquatable<Player>
{
    internal static readonly ConditionalWeakTable<Player, ExtendedPlayer> ExtendedPlayersTable = new();
    internal readonly Player Player;
    internal readonly TimeSequence Time = new();
    internal GenericJetpack GenericJetpack;
    internal JetpackType JetpackType = JetpackType.None;

    internal ExtendedPlayer(Player player) => Player = player;

    internal bool AdrenalineBoost
    {
        get => Time.AdrenalineBoost > 0f;
        set => Time.AdrenalineBoost = value ? TimeSequence.AdrenalineBoostTime : 0f;
    }

    public bool Equals(Player other) => other?.ObjectID == Player.ObjectID;

    internal void ApplyAdrenalineBoost()
    {
        AdrenalineBoost = true;
        GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, [], Player.ObjectID, GetStates()));
    }

    internal object[] GetStates()
    {
        object[] states = new object[3];
        states[0] = AdrenalineBoost;
        states[1] = (int)JetpackType;
        states[2] = GenericJetpack?.Fuel?.CurrentValue ?? 0f;

        return states;
    }

    internal void DisableAdrenalineBoost()
    {
        AdrenalineBoost = false;
        GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, [], Player.ObjectID, GetStates()));
        SoundHandler.PlaySound("StrengthBoostStop", Player.Position, Player.GameWorld);
    }

    internal class TimeSequence
    {
        internal const float AdrenalineBoostTime = 20000f;
        internal float AdrenalineBoost;
    }
}