using System;
using System.Collections.Generic;
using HarmonyLib;
using ScriptEngine;

namespace SFR.API;

/// <summary>
/// For unknown reasons scripts won't work in SFR. This class fixes them.
/// Most likely it's due to the strong assembly signing of Superfighters Deluxe.exe
/// which SFR breaks, thus the script engine won't recognize it at all.
/// </summary>
[HarmonyPatch(typeof(Sandbox))]
internal static class Engine
{
    private static readonly Dictionary<Sandbox, AppDomain> _appDomains = [];

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Sandbox.Create))]
    private static bool Create(ref Sandbox __result)
    {
        string applicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        AppDomainSetup info = new()
        {
            ApplicationBase = applicationBase,
            ApplicationName = "Sandbox",
            DisallowBindingRedirects = false,
            DisallowCodeDownload = true,
            DisallowPublisherPolicy = true
        };


        AppDomain appDomain = AppDomain.CreateDomain("Sandbox");
        Sandbox sandbox = (Sandbox)Activator.CreateInstanceFrom(appDomain, typeof(Sandbox).Assembly.ManifestModule.FullyQualifiedName, typeof(Sandbox).FullName).Unwrap();

        sandbox.Setup();

        _appDomains.Add(sandbox, appDomain);

        __result = sandbox;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Sandbox.Unload))]
    private static bool Unload(Sandbox sandbox)
    {
        if (_appDomains.ContainsKey(sandbox))
        {
            AppDomain domain = _appDomains[sandbox];
            sandbox.Dispose();
            _ = _appDomains.Remove(sandbox);
            AppDomain.Unload(domain);
        }

        return false;
    }
}