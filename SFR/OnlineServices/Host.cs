using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using Constants = SFR.Misc.Constants;

namespace SFR.OnlineServices;

[HarmonyPatch]
internal class Host
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameInfo), MethodType.Constructor, typeof(GameOwnerEnum))]
    private static IEnumerable<CodeInstruction> ExpandGameSlots(IEnumerable<CodeInstruction> instructions)
    {
        if (Constants.Slots != null)
        {
            instructions.ElementAt(94).opcode = OpCodes.Ldc_I4_S;
            instructions.ElementAt(94).operand = Constants.Slots;
        }

        return instructions;
    }
    
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Server), nameof(Server.Start))]
    private static IEnumerable<CodeInstruction> AllowMoreConnections(IEnumerable<CodeInstruction> instructions)
    {
        if (Constants.Slots != null)
        {
            instructions.ElementAt(25).operand = Constants.Slots + 4;
        }

        return instructions;
    }
}