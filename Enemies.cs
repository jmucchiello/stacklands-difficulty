using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace DifficultyModNS
{
    public partial class DifficultyMod : Mod
    {
        public float SpawnMultiplier = 1.0f;

        public void SetupStrengthMultiplier()
        {
            switch (difficulty)
            {
                case DifficultyType.VeryEasy: SpawnMultiplier = 0.6f; break;
                case DifficultyType.Easy: SpawnMultiplier = 0.8f; break;
                case DifficultyType.Normal: SpawnMultiplier = 1.0f; break;
                case DifficultyType.Challenging: SpawnMultiplier = 1.25f; break;
                case DifficultyType.Hard: SpawnMultiplier = 1.5f; break;
                case DifficultyType.VeryHard: SpawnMultiplier = 1.75f; break;
                case DifficultyType.Impossible: SpawnMultiplier = 2.0f; break;
            };
            DifficultyMod.Log($"Spawned Enemies Strength Multiplies: {SpawnMultiplier}");
        }
    }

    [HarmonyPatch(typeof(SpawnHelper))]
    [HarmonyPatch(nameof(SpawnHelper.GetEnemiesToSpawn))]
    [HarmonyPatch(new Type[] { typeof(List<SetCardBagType>), typeof(float), typeof(bool) })]
    internal class EmemySpawning
    {
        static bool Prefix(List<CardIdWithEquipment> __result, List<SetCardBagType> cardbags, ref float strength, bool canHaveInventory)
        {
            string s = String.Join(",", cardbags.ToArray());
            DifficultyMod.Log($"SpawnHelper.GetEnemiesToSpawn - list of cardbags {s} strength {strength:F02}");
            strength *= DifficultyMod.instance.SpawnMultiplier;
            DifficultyMod.Log($"SpawnHelper.GetEnemiesToSpawn - modified strength {strength:F02}");
            return true;
        }
    }
}
