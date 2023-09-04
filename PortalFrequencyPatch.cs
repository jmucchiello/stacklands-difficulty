using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Reflection.Emit;
using HarmonyLib.Tools;

namespace DifficultyModNS
{
    [HarmonyPatch]
    public class enabling
    {
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
                Type myClass = typeof(enabling);
                List<CodeInstruction> result = new CodeMatcher(instructions)
                    .MatchStartForward(new CodeMatch(OpCodes.Call, AccessTools.FirstMethod(typeof(EndOfMonthCutscenes), method => method.Name.Contains("get_CurrentMonth"))))
                    .ThrowIfNotMatch("Can't find Portal code")
                    .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.FirstMethod(myClass, t => t.Name.Contains("AllowPortals"))))
                    .RemoveInstructions(9)
                    .MatchStartForward(new CodeMatch(OpCodes.Ldarg_0),
                                       new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(WorldManager), "instance")))
                    .ThrowIfNotMatch("Can't find Pirate code")
                    .Advance(1)
                    .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.FirstMethod(myClass, t => t.Name.Contains("AllowPirates"))))
                    .RemoveInstructions(11)
                    .InstructionEnumeration()
                    .ToList();
                result.ForEach(instruction => DifficultyMod.Log($"{instruction}"));
                DifficultyMod.Log($"Exiting Instructions in {instructions.Count()}, instructions out {result.Count()}");
                return result;
            }
            catch (Exception e)
            {
                DifficultyMod.LogError("Failed to Transpile EndOfMonthCutscenes.SpecialEvents" + e.ToString());
                return instructions;
            }
        }

        static bool AllowTravelingCart()
        {
            int month = EndOfMonthCutscenes.CurrentMonth;
            bool spawnTravellingCart = (UnityEngine.Random.value <= 0.1f && month >= 8 && month % 2 == 1) || month == 19;
            return spawnTravellingCart;
        }

        static bool AllowPortals()
        {
            if (!DifficultyMod.AllowRarePortals) WorldManager.instance.CurrentRunVariables.StrangePortalSpawns = 1;
            bool b = DifficultyMod.AllowStrangePortals &&
                     EndOfMonthCutscenes.CurrentMonth > 8 &&
                     EndOfMonthCutscenes.CurrentMonth % DifficultyMod.PortalFrequncy == 0;
            DifficultyMod.Log("AllowPortals returning " + b.ToString());
            return b;
        }

        static bool AllowPirates()
        {
            bool b = DifficultyMod.AllowPirateShips &&
                     WorldManager.instance.CurrentBoard.BoardOptions.CanSpawnPirateBoat &&
                     WorldManager.instance.BoardMonths.IslandMonth % DifficultyMod.PirateFrequncy == 0;
            DifficultyMod.Log("AllowPirates returning " + b.ToString());
            return b;
        }

    }

    public static class CodeMatchExtensions
    {
        public static CodeMatcher GetPos(this CodeMatcher matcher, out Int32 position)
        {
            position = matcher.Pos;
            return matcher;
        }
    }

    public class test
    {
        public static string CutsceneTitle;
        public static string CutsceneText;

        public static int CurrentMonth => WorldManager.instance.CurrentMonth;

        public static IEnumerator SpecialEvents()
        {
            CutsceneTitle = "";
            CutsceneText = "";
            bool flag = CurrentMonth > 8 && CurrentMonth % 4 == 0;
            bool spawnTravellingCart = (UnityEngine.Random.value <= 0.1f && CurrentMonth >= 8 && CurrentMonth % 2 == 1) || CurrentMonth == 19;
            bool spawnPirateBoat = WorldManager.instance.BoardMonths.IslandMonth % 7 == 0 && WorldManager.instance.CurrentBoard.BoardOptions.CanSpawnPirateBoat;
            bool spawnShaman = (WorldManager.instance.CurrentRunVariables.FinishedDemon || QuestManager.instance.QuestIsComplete("kill_demon")) && WorldManager.instance.IsSpiritDlcActive() && !WorldManager.instance.CurrentRunVariables.ShamanVisited;
            bool spawnSadEvent = WorldManager.instance.CurrentBoard.Id == "happiness" && CurrentMonth > 4 && CurrentMonth % 4 == 0;
            yield return null;
        }
    }
}
