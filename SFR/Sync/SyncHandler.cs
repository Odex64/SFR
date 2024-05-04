using System.Collections.Generic;
using HarmonyLib;
using Lidgren.Network;
using SFD;
using SFR.Fighter.Jetpacks;
using SFR.Helper;
using SFR.Objects;
using SFR.Sync.Generic;
using SFR.Weapons.Melee;

namespace SFR.Sync;

/// <summary>
/// This class handles the sync between clients &amp; server.
/// <list type="table">
/// <listheader>
/// <term>Sync</term>
/// <description>Data to sync</description>
/// </listheader>
/// <item>
/// <term>63</term>
/// <description>Generic Server Data.</description>
/// </item>
/// </list>
/// </summary>
[HarmonyPatch]
internal static class SyncHandler
{
    private const byte _maxAttempts = 18;

    internal static readonly Dictionary<int, byte> Attempts = [];

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(NetMessage), nameof(NetMessage.WriteDataType))]
    private static IEnumerable<CodeInstruction> ExtendWriteData(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.operand is null)
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
            if (instruction.operand is null)
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
    /// When spawning new objects clients will take a while to save them in their GameWorld.
    /// Therefore we check if given objects has been spawned yet with some attempts.
    /// TODO: Find a better way to sync newly spawned objects between clients / server.
    /// </summary>
    private static ObjectData[] SyncGameWorldObject(GameWorld gameWorld, GenericData genericData, params int[] objectsID)
    {
        List<ObjectData> receivedData = [];
        foreach (int obj in objectsID)
        {
            var data = gameWorld.GetObjectDataByID(obj);
            if (data is null)
            {
                if (!Attempts.ContainsKey(obj))
                {
                    Attempts.Add(obj, 0);
                }

                if (Attempts[obj]++ < _maxAttempts)
                {
                    GenericData.SendGenericDataToClients(genericData);
                }
                else
                {
                    Logger.LogError($"{genericData.Type} sync exceeded max attempts! Aborting.");
                    _ = Attempts.Remove(obj);
                }

                return null;
            }

            if (Attempts.ContainsKey(obj))
            {
                Logger.LogWarn($"Synced {genericData.Type} after {Attempts[obj]} attempts");
                _ = Attempts.Remove(obj);
            }

            receivedData.Add(data);
        }

        return [.. receivedData];
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Server), nameof(Server.updateRun))]
    private static void SendDataToClients(bool isLast, Server __instance)
    {
        if (__instance.m_server is null || __instance.CurrentState != ServerClientState.Game || __instance.WaitingForUsers || __instance.GameInfo.TotalGameUserCount == 0 || __instance.m_waitingForUserRestartInProgress)
        {
            return;
        }

        if (isLast)
        {
            foreach (var genericServerData in GenericData.ServerData)
            {
                __instance.m_server.SendToAll(genericServerData, null, GenericServerData.Delivery.Method, GenericServerData.Delivery.Channel);
            }

            GenericData.ServerData.Clear();
        }
    }

    private static void HandleGenericData(Client client, GenericData data)
    {
        switch (data.Type)
        {
            case DataType.SledgehammerBlink:
                var sledgehammerPlayer = client.GameWorld.GetPlayer((int)data.Args[0]);
                if (sledgehammerPlayer is null)
                {
                    return;
                }

                if (sledgehammerPlayer.GetCurrentWeapon() is Sledgehammer sledgehammer)
                {
                    float blinkTime = (float)data.Args[1];
                    sledgehammer.BlinkTimer = blinkTime;
                }

                break;

            case DataType.StickyGrenade:
                var stickyGrenadeData = SyncGameWorldObject(client.GameWorld, data, (int)data.Args[0]);
                var stickyGrenadePlayer = client.GameWorld.GetPlayer((int)data.Args[1]);
                if (stickyGrenadeData is null || stickyGrenadePlayer is null)
                {
                    return;
                }

                ((ObjectStickyBombThrown)stickyGrenadeData[0]).ApplyStickyPlayer(stickyGrenadePlayer, (float)data.Args[2], (float)data.Args[3], (float)data.Args[4]);
                break;

            case DataType.Head:
                var headData = SyncGameWorldObject(client.GameWorld, data, (int)data.Args[0]);
                if (headData is null)
                {
                    return;
                }

                ((ObjectHead)headData[0]).ReplaceTexture = ObjectHead.TextureFromString((string)data.Args[1]);
                break;

            case DataType.Crossbow:
                var crossbowData = SyncGameWorldObject(client.GameWorld, data, (int)data.Args[0]);
                var crossbowPlayer = client.GameWorld.GetPlayer((int)data.Args[1]);
                if (crossbowData is null || crossbowPlayer is null)
                {
                    return;
                }

                var crossbowBolt = (ObjectCrossbowBolt)crossbowData[0];
                crossbowBolt.Timer = (float)data.Args[2];
                crossbowBolt.ApplyPlayerBolt(crossbowPlayer);
                crossbowBolt.EnableUpdateObject();
                break;

            case DataType.ExtraClientStates:
                var player = client.GameWorld.GetPlayer((int)data.Args[0]);
                if (player is null)
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
                if (extendedPlayer.GenericJetpack is null || jetpackType != extendedPlayer.JetpackType)
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
                if (extendedPlayer.GenericJetpack is not null)
                {
                    extendedPlayer.GenericJetpack.Fuel.CurrentValue = jetPackFuel;
                }

                break;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Client), nameof(Client.HandleDataMessage))]
    private static void ReceiveAdditionalDataFromServer(NetMessage.MessageData messageData, NetIncomingMessage msg, Client __instance)
    {
        switch ((int)messageData.MessageType)
        {
            case 63:
                var data = GenericServerData.Read(msg);
                foreach (var syncFlags in data.Flags)
                {
                    switch (syncFlags)
                    {
                        case SyncFlag.NewObjects:
                            __instance.HandleNewObjects();
                            break;
                    }
                }

                HandleGenericData(__instance, data);
                break;
        }
    }
}