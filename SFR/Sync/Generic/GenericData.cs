using System.Collections.Generic;
using Lidgren.Network;
using SFD;

namespace SFR.Sync.Generic;

/// <summary>
///     Use this class to send any kind of data to clients or server.
///     Only primitive variables allowed.
/// </summary>
internal sealed class GenericData
{
    internal static readonly List<NetOutgoingMessage> ServerData = new();

    internal readonly object[] Args;

    internal readonly SyncFlag[] Flags;

    internal readonly DataType Type;

    internal GenericData(DataType type, SyncFlag[] flags, params object[] args)
    {
        Type = type;
        Flags = flags;
        Args = args;
    }

    internal static void SendGenericDataToClients(GenericData data)
    {
        if (GameSFD.Handle.Server is { NetServer: not null })
        {
            ServerData.Add(GenericServerData.Write(data, GameSFD.Handle.Server.NetServer.CreateMessage()));
        }
    }
}