using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using SFD.MenuControls;

namespace SFR.UI;

[HarmonyPatch]
internal static class MainMenu
{
    private const string _website = "redux.odex64.dev";
    private static Action<Panel> _openSubPanel;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuPanel), MethodType.Constructor)]
    private static void MainMenuPanel(MainMenuPanel __instance)
    {
        _openSubPanel = __instance.OpenSubPanel;

        __instance.menu.Height += 1;
        __instance.menu.Add(new MainMenuItem("CREDITS ⛭", Credits), 6);
    }

    private static void Credits(object sender) => _openSubPanel(new CreditsPanel());

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainMenuPanel), nameof(SFD.MenuControls.MainMenuPanel.KeyPress))]
    private static IEnumerable<CodeInstruction> ExitButton(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(10).opcode = OpCodes.Ldc_I4_8;
        return instructions;
    }
}