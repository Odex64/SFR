using SFD;

namespace SFR.Sync.Generic;

/// <summary>
///     Use this class to send any kind of data to clients or server.
///     Only primitive variables allowed.
/// </summary>
internal sealed class GenericData
{
    internal readonly object[] Args;

    internal readonly DataType Type;

    internal GenericData(DataType type, params object[] args)
    {
        Type = type;
        Args = args;
    }

    internal static void SendGenericDataToClients(GenericData data)
    {
        if (GameSFD.Handle.Server is { NetServer: { } })
        {
            var netOutgoingMessage = GenericServerData.Write(data, GameSFD.Handle.Server.NetServer.CreateMessage());
            GameSFD.Handle.Server.NetServer.SendToAll(netOutgoingMessage, null, GenericServerData.Delivery.Method, GenericServerData.Delivery.Channel);
        }
    }
}