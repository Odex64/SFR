using System.Collections.Generic;
using HarmonyLib;
using Lidgren.Network;
using SFD;
using SFR.Game;
using SFR.Helper;
using SFR.Objects;
using SFR.Sync.Generic;
using SFR.Weapons.Rifles;

namespace SFR.Sync;

/// <summary>
///     This class handles the sync between clients & server.
///     <para>
///         Here's the list of custom message types.
///         <list type="bullet">
///             <item>
///                 <description>63: Generic Server Data.</description>
///             </item>
///         </list>
///     </para>
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

            case DataType.Minigun:
                var minigunData = SyncGameWorldObject(client.GameWorld, data, (int)data.Args[0]);
                if (minigunData == null)
                {
                    return;
                }

                if (minigunData[0].IsPlayer)
                {
                    var player = (Player)minigunData[0].InternalData;
                    if (player is { CurrentRifleWeapon: Minigun minigun })
                    {
                        minigun.ClientSyncRev(!((string)data.Args[1]).EndsWith("UNREV"));
                    }
                }

                break;

            case DataType.Head:
                var headData = SyncGameWorldObject(client.GameWorld, data, (int)data.Args[0]);
                if (headData == null)
                {
                    return;
                }

                ((ObjectHead)headData[0]).ReplaceTexture = ObjectHead.TextureFromString((string)data.Args[1]);
                break;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Client), nameof(Client.HandleDataMessage))]
    private static bool ReceiveFromServer(NetMessage.MessageData messageData, NetIncomingMessage msg, ref bool processGameWorldDependentData, Client __instance)
    {
        if (processGameWorldDependentData)
        {
            __instance.ProcessGameWorldDependentMessages();
            processGameWorldDependentData = false;
        }

        switch ((int)messageData.MessageType)
        {
            case 63:
            {
                HandleGenericData(__instance, GenericServerData.Read(msg));
                return false;
            }
        }

        return true;
    }

    #endregion
}