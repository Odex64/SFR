using System;
using System.Collections.Generic;
using HarmonyLib;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using SFD;
using SFD.MenuControls;
using SFD.SFDOnlineServices;
using Constants = SFR.Misc.Constants;

namespace SFR.OnlineServices;

/// <summary>
///     Handles in-game browser and server join requests.
/// </summary>
[HarmonyPatch]
internal static class Browser
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameBrowserMenuItem), nameof(GameBrowserMenuItem.Game), MethodType.Setter)]
    private static bool PatchBrowser(SFDGameServerInstance value, GameBrowserMenuItem __instance)
    {
        if (__instance.m_game != value)
        {
            __instance.m_game = value;
            if (__instance.labels != null)
            {
                var color = Color.Red;
                if (__instance.m_game is { SFDGameServer: { } })
                {
                    if (__instance.m_game.SFDGameServer.Version == Constants.ServerVersion)
                    {
                        color = Color.White;
                    }
                    else if (__instance.m_game.SFDGameServer.Version == "v.1.3.7d")
                    {
                        color = Color.Yellow;
                    }
                }

                foreach (var label in __instance.labels)
                {
                    label.Color = color;
                }
            }
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFDGameServer), nameof(SFDGameServer.Version), MethodType.Setter)]
    private static bool GameServerVersion(ref string value)
    {
        if (value == "v.1.3.7x")
        {
            value = Constants.ServerVersion;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFD.Constants), nameof(SFD.Constants.VersionCheckDifference), typeof(string))]
    private static bool VersionCheckPatch(string versionToCheck, ref VersionDifference __result)
    {
        __result = versionToCheck == Constants.ServerVersion ? VersionDifference.Same : VersionDifference.Older;

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NetMessage.Connection.DiscoveryResponse.Data), MethodType.Constructor, typeof(ServerResponse), typeof(string), typeof(string), typeof(string), typeof(Guid))]
    private static void PatchServerVersionResponse(ref NetMessage.Connection.DiscoveryResponse.Data __instance)
    {
        if (__instance.Version == "v.1.3.7x")
        {
            __instance.Version = Constants.ServerVersion;
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Server), nameof(Server.DoReadRun))]
    private static IEnumerable<CodeInstruction> ServerReadRun(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.operand == null)
            {
                continue;
            }

            if (instruction.operand.Equals("v.1.3.7x"))
            {
                instruction.operand = Constants.ServerVersion;
            }
        }

        return instructions;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NetMessage.Connection.DiscoveryRequest.Data), MethodType.Constructor, typeof(Guid), typeof(int), typeof(bool), typeof(string), typeof(string))]
    private static void PatchServerVersionRequest(ref NetMessage.Connection.DiscoveryRequest.Data __instance)
    {
        if (__instance.Version == "v.1.3.7x")
        {
            __instance.Version = Constants.ServerVersion;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NetMessage.Connection.DiscoveryResponse), nameof(NetMessage.Connection.DiscoveryResponse.Read))]
    private static bool PatchServerVersion(NetIncomingMessage netIncomingMessage, ref NetMessage.Connection.DiscoveryResponse.Data __result)
    {
        NetMessage.Connection.DiscoveryResponse.Data result = default;
        try
        {
            result.Version = netIncomingMessage.ReadString();
            result.Response = (ServerResponse)netIncomingMessage.ReadInt32();

            if (netIncomingMessage.Position < netIncomingMessage.LengthBits)
            {
                result.CryptPhraseA = netIncomingMessage.ReadString();
            }

            if (netIncomingMessage.Position < netIncomingMessage.LengthBits)
            {
                result.CryptPhraseB = netIncomingMessage.ReadString();
            }

            result.ServerPInstance = result.Version == Constants.ServerVersion ? new Guid(netIncomingMessage.ReadBytes(16)) : Guid.Empty;
        }
        catch (Exception)
        {
            result.Version = "Unknown";
            result.Response = ServerResponse.RefuseConnectVersionDiffer;
            result.ServerPInstance = Guid.Empty;
        }

        __result = result;

        return false;
    }
}