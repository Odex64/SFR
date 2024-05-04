using System.Collections.Generic;
using HarmonyLib;
using SFD;
using SFR.Misc;

namespace SFR.UI;

[HarmonyPatch]
internal static class GameMenu
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.DrawInner))]
    private static IEnumerable<CodeInstruction> DrawInner(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.operand is null)
            {
                continue;
            }

            if (instruction.operand.Equals("v.1.3.7x"))
            {
                instruction.operand = $"SFR {Globals.SFRVersion}";
                break;
            }
        }

        return instructions;
    }
}