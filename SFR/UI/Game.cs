using System.Collections.Generic;
using HarmonyLib;
using SFD;
using Constants = SFR.Misc.Constants;

namespace SFR.UI;

[HarmonyPatch]
internal static class Game
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.DrawInner))]
    private static IEnumerable<CodeInstruction> DrawInner(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.operand == null)
            {
                continue;
            }

            if (instruction.operand.Equals("v.1.3.7x"))
            {
                instruction.operand = $"SFR {Constants.SFRVersion}";
                break;
            }
        }

        return instructions;
    }
}