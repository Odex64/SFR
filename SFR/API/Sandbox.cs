using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using HarmonyLib;

namespace SFR.API;

/// <summary>
///     For unknown reasons scripts won't work in SFR. This class fixes them.
///     Most likely it's due to the strong signing of Superfighters Deluxe.exe
///     which SFR breaks, thus the script engine won't recognize it at all.
/// </summary>
[HarmonyPatch(typeof(ScriptEngine.Sandbox))]
internal static class Sandbox
{
    private static readonly Dictionary<ScriptEngine.Sandbox, AppDomain> _appDomains = new();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ScriptEngine.Sandbox.Create))]
    private static bool Create(ref ScriptEngine.Sandbox __result)
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

        PermissionSet permissionSet = new(PermissionState.Unrestricted);
        var appDomain = AppDomain.CreateDomain("Sandbox", null, info, permissionSet, typeof(ScriptEngine.Sandbox).Assembly.Evidence.GetHostEvidence<StrongName>());
        var sandbox = (ScriptEngine.Sandbox)Activator.CreateInstanceFrom(appDomain, typeof(ScriptEngine.Sandbox).Assembly.ManifestModule.FullyQualifiedName, typeof(ScriptEngine.Sandbox).FullName!).Unwrap();
        sandbox.Setup();
        _appDomains.Add(sandbox, appDomain);

        __result = sandbox;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ScriptEngine.Sandbox.Unload))]
    private static bool Unload(ScriptEngine.Sandbox sandbox)
    {
        if (_appDomains.ContainsKey(sandbox))
        {
            var domain = _appDomains[sandbox];
            sandbox.Dispose();
            _appDomains.Remove(sandbox);
            AppDomain.Unload(domain);
        }

        return false;
    }
}