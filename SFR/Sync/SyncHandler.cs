using System.Collections.Generic;
using HarmonyLib;
using Lidgren.Network;
using SFD;
using SFR.Fighter.Jetpacks;
using SFR.Game;
using SFR.Helper;
using SFR.Objects;
using SFR.Sync.Generic;

namespace SFR.Sync;

/// <summary>
///     This class handles the sync between clients &amp; server.
///     <list type="table">
///         <listheader>
///             <term>Sync</term>
///             <description>Data to sync</description>
///         </listheader>
///         <item>
///             <term>63</term>
///             <description>Generic Server Data.</description>
///         </item>
///     </list>
/// </summary>
[HarmonyPatch]
internal static class SyncHandler
{
    private const byte MaxAttempts = 18;

    internal static readonly Dictionary<int, byte> Attempts = new();

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(NetMessage), nameof(NetMessage.WriteDataType))]
    private static IEnumerable<CodeInstruction> ExtendWriteData(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.operand == null)
            {
                continue;
            }

            if (instruction.operand.Equals((sbyte)62))
            {
                instruction.operand = 63;
            }
        }

        return instructions;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(NetMessage), nameof(NetMessage.ReadDataType))]
    private static IEnumerable<CodeInstruction> ExtendReadData(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.operand == null)
            {
                continue;
            }

            if (instruction.operand.Equals((sbyte)62))
            {
                instruction.operand = 63;
            }
        }

        return instructions;
    }

    /// <summary>
    ///     When spawning new objects clients will take a while to save them in their GameWorld.
    ///     Therefore we check if given objects has been spawned yet with some attempts.
    ///     TODO: Find a better way to sync newly spawned objects between clients / server.
    /// </summary>
    private static ObjectData[] SyncGameWorldObject(GameWorld gameWorld, GenericData genericData, params int[] objectsID)
    {
        List<ObjectData> receivedData = new();
        foreach (int obj in objectsID)
        {
            var data = gameWorld.GetObjectDataByID(obj);
            if (data == null)
            {
                if (!Attempts.ContainsKey(obj))
                {
                    Attempts.Add(obj, 0);
                }

                if (Attempts[obj]++ < MaxAttempts)
                {
                    GenericData.SendGenericDataToClients(genericData);
                }
                else
                {
                    Logger.LogError($"{genericData.Type} sync exceeded max attempts! Aborting.");
                    Attempts.Remove(obj);
                }

                return null;
            }

            if (Attempts.ContainsKey(obj))
            {
                Logger.LogWarn($"Synced {genericData.Type} after {Attempts[obj]} attempts");
                Attempts.Remove(obj);
            }

            receivedData.Add(data);
        }

        return receivedData.ToArray();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Server), nameof(Server.updateRun))]
    private static void SendDataToClients(bool isLast, bool isFirst, Server __instance)
    {
        if (__instance.m_server == null || __instance.CurrentState != ServerClientState.Game || __instance.WaitingForUsers || __instance.GameInfo.TotalGameUserCount == 0 || __instance.m_waitingForUserRestartInProgress)
        {
            return;
        }

        if (isLast)
        {
            // SendExtraPlayerStatesToClients(__instance);

            foreach (var genericServerData in GenericData.ServerData)
            {
                __instance.m_server.SendToAll(genericServerData, null, GenericServerData.Delivery.Method, GenericServerData.Delivery.Channel);
            }

            GenericData.ServerData.Clear();
        }
    }

    // private static void SendExtraPlayerStatesToClients(Server server)
    // {
    //     foreach (var player in server.GameWorld.Players)
    //     {
    //         var extendedPlayer = player.GetExtension();
    //         var data = new GenericData(DataType.ExtraClientStates, player.ObjectID, extendedPlayer.GetStates());
    //         var outgoingMessage = GenericServerData.Write(data, server.m_server.CreateMessage());
    //         server.m_server.SendToAll(outgoingMessage, null, GenericServerData.Delivery.Method, GenericServerData.Delivery.Channel);
    //     }
    // }

    #region Server

    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(Server), nameof(Server.HandleDataMessage))]
    // private static bool ReceiveFromClients(NetMessage.MessageData messageData, NetIncomingMessage msg, Server __instance)
    // {
    //     switch ((int)messageData.MessageType)
    //     {
    //     }
    //
    //     return true;
    // }

    #endregion


    #region Client

    private static void HandleGenericData(Client client, GenericData data)
    {
        switch (data.Type)
        {
            case DataType.Nuke:
                var nukeData = SyncGameWorldObject(client.GameWorld, data, (int)data.Args[0], (int)data.Args[1]);
                if (nukeData == null)
                {
                    return;
                }

                var nukes = new List<ObjectNuke>
                {
                    (ObjectNuke)nukeData[0],
                    (ObjectNuke)nukeData[1]
                };

                NukeHandler.Begin(nukes);
                break;

            case DataType.StickyGrenade:
                var stickyGrenadeData = SyncGameWorldObject(client.GameWorld, data, (int)data.Args[0], (int)data.Args[1]);
                if (stickyGrenadeData == null)
                {
                    return;
                }

                ((ObjectStickyBombThrown)stickyGrenadeData[0]).ApplyStickyPlayer(stickyGrenadeData[1].ObjectID, (float)data.Args[2], (float)data.Args[3], (float)data.Args[4]);
                break;

            case DataType.Head:
                var headData = SyncGameWorldObject(client.GameWorld, data, (int)data.Args[0]);
                if (headData == null)
                {
                    return;
                }

                ((ObjectHead)headData[0]).ReplaceTexture = ObjectHead.TextureFromString((string)data.Args[1]);
                break;

            case DataType.ExtraClientStates:
                var player = client.GameWorld.GetPlayer((int)data.Args[0]);
                if (player == null)
                {
                    Logger.LogError("Player is null while syncing states!");
                    return;
                }

                var extendedPlayer = player.GetExtension();
                bool adrenalineBoost = (bool)data.Args[1];
                if (!extendedPlayer.AdrenalineBoost && adrenalineBoost)
                {
                    extendedPlayer.AdrenalineBoost = true;
                }

                var jetpackType = (JetpackType)(int)data.Args[2];
                if (extendedPlayer.GenericJetpack == null || jetpackType != extendedPlayer.JetpackType)
                {
                    extendedPlayer.JetpackType = jetpackType;
                    extendedPlayer.GenericJetpack = extendedPlayer.JetpackType switch
                    {
                        JetpackType.None => null,
                        JetpackType.Jetpack => new Jetpack(),
                        JetpackType.JetpackEditor => new JetpackEditor(),
                        JetpackType.Gunpack => new Gunpack(),
                        _ => extendedPlayer.GenericJetpack
                    };
                }

                float jetPackFuel = (float)data.Args[3];
                if (extendedPlayer.GenericJetpack != null)
                {
                    extendedPlayer.GenericJetpack.Fuel.CurrentValue = jetPackFuel;
                }

                break;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Client), nameof(Client.HandleDataMessage))]
    private static void ReceiveDataFromServer(NetMessage.MessageData messageData, NetIncomingMessage msg, ref bool processGameWorldDependentData, Client __instance)
    {
        // if (processGameWorldDependentData)
        // {
        //     __instance.ProcessGameWorldDependentMessages();
        //     processGameWorldDependentData = false;
        // }

        switch ((int)messageData.MessageType)
        {
            case 63:
            {
                var data = GenericServerData.Read(msg);
                foreach (var syncFlags in data.Flags)
                {
                    switch (syncFlags)
                    {
                        case SyncFlag.MustSyncNewObjects:
                            __instance.HandleNewObjects();
                            break;
                    }
                }

                HandleGenericData(__instance, data);
                break;
            }
        }
    }

    #endregion
}