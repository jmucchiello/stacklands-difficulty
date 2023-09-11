using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;

namespace DifficultyModNS
{
    public partial class DifficultyMod : Mod
    {
        public void SetupNewVillagerChecks()
        {
            switch (difficulty)
            {
                case >= DifficultyType.Brutal:
                    OneVillagerChecks.frequency = 8;
                    OneVillagerChecks.startChecking = 16;
                    break;
                case >= DifficultyType.Hard:
                    OneVillagerChecks.frequency = 6;
                    OneVillagerChecks.startChecking = 12;
                    break;
                case DifficultyType.VeryEasy:
                    OneVillagerChecks.frequency = 4;
                    OneVillagerChecks.startChecking = 8;
                    break;
                default:
                    OneVillagerChecks.frequency = 5;
                    OneVillagerChecks.startChecking = 10;
                    break;
            }
            DifficultyMod.Log($"VillagerChecks: Frequency {OneVillagerChecks.frequency}, Start Checking {OneVillagerChecks.startChecking}");
        }
    }

    [HarmonyPatch(typeof(Boosterpack),nameof(Boosterpack.Clicked))]
    public class OneVillagerChecks
    {
        public static Int32 frequency = 5;
        public static Int32 startChecking = 10;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4_5)
                {
                    DifficultyMod.Log("Transpiling const 5 to variable");
                    FieldInfo fi = typeof(OneVillagerChecks).GetField("frequency");
                    yield return new CodeInstruction(OpCodes.Ldsfld, fi);
                }
                else if (instruction.opcode == OpCodes.Ldc_I4_S && Convert.ToInt32(instruction.operand) == 10)
                {
                    DifficultyMod.Log("Transpiling const 10 to variable");
                    FieldInfo fi = typeof(OneVillagerChecks).GetField("startChecking");
                    yield return new CodeInstruction(OpCodes.Ldsfld, fi);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}
