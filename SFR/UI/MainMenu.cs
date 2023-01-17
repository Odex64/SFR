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
    private const string Website = "superfightersredux.tk";
    private static Action<Panel> OpenSubPanel;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuPanel), MethodType.Constructor)]
    private static void MainMenuPanel(MainMenuPanel __instance)
    {
        OpenSubPanel = __instance.OpenSubPanel;

        __instance.menu.Height += 1;
        __instance.menu.Add(new MainMenuItem("CREDITS ⛭", Credits), 6);
    }

    private static void Credits(object sender)
    {
        OpenSubPanel(new CreditsPanel());
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainMenuPanel), nameof(SFD.MenuControls.MainMenuPanel.KeyPress))]
    private static IEnumerable<CodeInstruction> ExitButton(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(10).opcode = OpCodes.Ldc_I4_8;
        return instructions;
    }
}