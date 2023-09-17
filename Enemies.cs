using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;

namespace DifficultyModNS
{
    public partial class DifficultyMod : Mod
    {
        public void SetupStrengthMultiplier()
        {
            float value = 1.0f;
            switch (difficulty)
            {
                case DifficultyType.VeryEasy: value = 0.6f; break;
                case DifficultyType.Easy: value = 0.8f; break;
                case DifficultyType.Normal: value = 1.0f; break;
                case DifficultyType.Challenging: value = 1.25f; break;
                case DifficultyType.Hard: value = 1.5f; break;
                case DifficultyType.VeryHard: value = 1.75f; break;
                case DifficultyType.Brutal: value = 2.0f; break;
            };
            EmemySpawning_Patch.SpawnMultiplier = value;
            DifficultyMod.Log($"Spawned Enemies Strength Multiplies: {value}");
        }
    }

    [HarmonyPatch(typeof(SpawnHelper))]
    [HarmonyPatch(nameof(SpawnHelper.GetEnemiesToSpawn))]
    [HarmonyPatch(new Type[] { typeof(List<SetCardBagType>), typeof(float), typeof(bool) })]
    internal class EmemySpawning_Patch
    {
        public static float SpawnMultiplier = 1.0f;

        static void Prefix(List<CardIdWithEquipment> __result, List<SetCardBagType> cardbags, ref float strength, bool canHaveInventory)
        {
            string s = String.Join(",", cardbags.ToArray());
            DifficultyMod.Log($"SpawnHelper.GetEnemiesToSpawn - list of cardbags {s} strength {strength:F02}");
            strength *= SpawnMultiplier;
            DifficultyMod.Log($"SpawnHelper.GetEnemiesToSpawn - modified strength {strength:F02}");
        }
    }

    [HarmonyPatch(typeof(Crab),nameof(Crab.Die))]
    internal class MommaCrab_Patch
    {
        public static int MommaCrabFrequency = 3;
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> result = new CodeMatcher(instructions)
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldc_I4_3)
                )
                .Set(OpCodes.Ldsfld, AccessTools.Field(typeof(MommaCrab_Patch),"MommaCrabFrequency"))
                .InstructionEnumeration()
                .ToList();
            return result;
        }
    }

    [HarmonyPatch(typeof(WorldManager),nameof(WorldManager.CreateCard))]
    [HarmonyPatch(new Type[] { typeof(Vector3), typeof(CardData), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
    public class WMCreateCard_Patch
    {
        public static Traverse<bool> WM_IsLoadingSaveRound;
        static void Postfix(WorldManager __instance, CardData __result, ref Vector3 position)
        {
            bool loading = WM_IsLoadingSaveRound.Value;
            if (!loading && __result is Combatable c && __result is not BaseVillager)
            {
                c.BaseCombatStats.MaxHealth = (int)(c.BaseCombatStats.MaxHealth * EmemySpawning_Patch.SpawnMultiplier);
                c.HealthPoints = c.ProcessedCombatStats.MaxHealth;
                position = __instance.GetRandomSpawnPosition();
            }
        }
    }
}
