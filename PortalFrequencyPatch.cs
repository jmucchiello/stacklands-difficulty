using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace DifficultyModNS
{
    [HarmonyPatch]
    public class SpecialEvents_Patch
    {
        public static int PortalMinMonth = 8;
        public static int PortalDivisor = 4;
        public static float FrequencyOfTravellingCart = 0.1f;
        public static int PirateDivisor = 4;
        public static int SadEventMinMonth = 4;
        public static int SadEventDivisor = 4;

        private static Type innerClass;
        public static MethodBase TargetMethod()
        {
            innerClass = AccessTools.FirstInner(typeof(EndOfMonthCutscenes), t => t.Name.Contains("SpecialEvents"));
            DifficultyMod.Log(innerClass?.ToString() ?? "null");
            MethodBase method = AccessTools.FirstMethod(innerClass, method => method.Name.Contains("MoveNext"));
            DifficultyMod.Log(method?.ToString() ?? "null");
            return method;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                Type myClass = typeof(SpecialEvents_Patch);
                List<CodeInstruction> result = new CodeMatcher(instructions)
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_8)
                    )
                    .ThrowIfNotMatch("Can't find portal min month")
                    .Set(OpCodes.Ldsfld, AccessTools.Field(myClass, "PortalMinMonth"))
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_4)
                    )
                    .ThrowIfNotMatch("Can't find portal min month")
                    .Set(OpCodes.Ldsfld, AccessTools.Field(myClass, "PortalDivisor"))
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_R4, 0.1f)
                    )
                    .ThrowIfNotMatch("Can't find portal divisor")
                    .Set(OpCodes.Ldsfld, AccessTools.Field(myClass, "FrequencyOfTravellingCart"))
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_7)
                    )
                    .ThrowIfNotMatch("Can't find travelling cart frequency")
                    .Set(OpCodes.Ldsfld, AccessTools.Field(myClass, "PirateDivisor"))
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldstr, "happiness")
                    )
                    .ThrowIfNotMatch("Can't find happiness")
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_4)
                    )
                    .ThrowIfNotMatch("Can't find happiness min month")
                    .Set(OpCodes.Ldsfld, AccessTools.Field(myClass, "SadEventMinMonth"))
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_4)
                    )
                    .ThrowIfNotMatch("Can't find happiness min month")
                    .Set(OpCodes.Ldsfld, AccessTools.Field(myClass, "SadEventDivisor"))
                    .InstructionEnumeration()
                    .ToList();
                //result.ForEach(instruction => DifficultyMod.Log($"{instruction}"));
                DifficultyMod.Log($"Exiting Instructions in {instructions.Count()}, instructions out {result.Count()}");
                return result;
            }
            catch (Exception e)
            {
                DifficultyMod.LogError("Failed to Transpile EndOfMonthCutscenes.SpecialEvents" + e.ToString());
                return instructions;
            }
        }
    }
}
