using HarmonyLib;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DifficultyModNS
{
    public partial class DifficultyMod : Mod
    {
        /**
         *      Features in this file:
         *          Configuration and Settings handling
         **/

        public static DifficultyMod instance;
        public static void Log(string msg) => instance.Logger.Log(msg);
        public static void LogError(string msg) => instance.Logger.LogError(msg);

        private void Awake()
        {
            instance = this;
            Harmony.PatchAll();
        }
        public override void Ready()
        {
            SetupConfig();
            ApplySettings();
            Logger.Log("Ready!");
        }

        public void SetupConfig()
        {
            configDifficulty = new ConfigDifficulty("difficultymod_difficulty", Config, DifficultyType.Normal);
            configEnabling = new ConfigEnabling("difficultymod_enabling", Config);
            configSpawnSites = new ConfigSpawnSites("difficultymod_spawning", Config, SpawnSites.Anywhere);
            ConfigFreeText configDefaults = new("none", Config, ConfigEntryModalHelper.RightAlign(SokLoc.Translate("difficultymod_defaults")));
            configDefaults.Clicked += delegate (ConfigEntryBase c)
            {
                configDifficulty.SetDefaults();
                configEnabling.SetDefaults();
                configSpawnSites.SetDefaults();
            };

            Config.OnSave = ApplySettings;
        }

        public void ApplySettings()
        {
            GameCardStartTimer_Patch.modifiers.Clear();
            BlueprintStartTimer_Patch.modifiers.Clear();
            difficulty = configDifficulty.Value;
            Log($"Overall Difficulty set to {difficulty}");
            SetupSpawnSites();
            SetupSummoningFrequency();
            SetupStrengthMultiplier();
            SetupFoodMultiplier();
            SetupNewVillagerChecks();
            SetupStorageCapacity();
            SetupDLC();
        }

        public static bool AnyCardInCardBagIsOfType(CardBag cardBag, Type type)
        {
            List<string> cards = cardBag.GetCardsInBag(WorldManager.instance.GameDataLoader);
            foreach (string card in cards)
            {
                if (type.IsAssignableFrom(WorldManager.instance.GameDataLoader.GetCardFromId(card).GetType())) return true;
            }
            return false;
        }

        public static bool BlueprintCreatesCardsOfType(Blueprint bp, Type type)
        {
            foreach (Subprint sp in bp.Subprints)
            {
                if (!String.IsNullOrEmpty(sp.ResultCard) && WorldManager.instance.GameDataLoader.GetCardFromId(sp.ResultCard).GetType().Equals(type)) return true;
                foreach (string s in sp.ExtraResultCards)
                {
                    if (!String.IsNullOrEmpty(s) && WorldManager.instance.GameDataLoader.GetCardFromId(s).GetType().Equals(type)) return true;
                }
            }
            return false;
        }

        public void SetupFoodMultiplier()
        {
            float foodBlueprintModifier = Difficulty switch
            {
                <= DifficultyType.VeryEasy => 0.8f,
                >= DifficultyType.VeryHard => 1.2f,
                _ => 1f
            };
            Log($"Food multiplier: {foodBlueprintModifier}");
            if (foodBlueprintModifier != 1f)
            {
                new BlueprintTimerModifier() { blueprintId = "blueprint_growth", subprintindex = -1, multiplier = foodBlueprintModifier }.AddToList();
                foreach (Blueprint bp in WorldManager.instance.GameDataLoader.BlueprintPrefabs)
                {
                    if (BlueprintCreatesCardsOfType(bp, typeof(Food)))
                    {
                        Log($"SetupFoodMultiplier adding {bp.Id}");
                        new BlueprintTimerModifier() { blueprintId = bp.Id, subprintindex = -1, multiplier = foodBlueprintModifier }.AddToList();
                    }
                }
                new GameCardTimerModifier() { actionId = "complete_harvest", myCardDataType = typeof(Harvestable), multiplier = foodBlueprintModifier, hasCardBag = "MyCardBag", cardBagProduces = typeof(Food) }.AddToList();
            }
        }

        public void SetupStorageCapacity()
        {
            switch (storageCapacity)
            {
                case StorageCapacity.Small:
                    CardCapIncrease.ShedIncrease = 3;
                    CardCapIncrease.WarehouseIncrease = 12;
                    break;
                case StorageCapacity.Normal:
                    CardCapIncrease.ShedIncrease = 4;
                    CardCapIncrease.WarehouseIncrease = 14;
                    break;
                case StorageCapacity.Large:
                    CardCapIncrease.ShedIncrease = 5;
                    CardCapIncrease.WarehouseIncrease = 18;
                    break;
                case StorageCapacity.Enormous:
                    CardCapIncrease.ShedIncrease = 6;
                    CardCapIncrease.WarehouseIncrease = 25;
                    break;
            }
            Log($"Storage Capacity Shed {CardCapIncrease.ShedIncrease} Warehouse {CardCapIncrease.WarehouseIncrease}");
        }

        public void SetupSummoningFrequency()
        {
            switch (difficulty)
            {
                case DifficultyType.Brutal:
                    SpecialEvents_Patch.PortalDivisor = 2;
                    SpecialEvents_Patch.PirateDivisor = 4;
                    SpecialEvents_Patch.FrequencyOfTravellingCart = 0.02f;
                    MommaCrab_Patch.MommaCrabFrequency = 1;
                    break;
                case <= DifficultyType.VeryEasy:
                    SpecialEvents_Patch.PortalDivisor = 6;
                    SpecialEvents_Patch.PirateDivisor = 10;
                    SpecialEvents_Patch.FrequencyOfTravellingCart = 0.15f;
                    MommaCrab_Patch.MommaCrabFrequency = 4;
                    break;
                case >= DifficultyType.Hard:
                    SpecialEvents_Patch.PortalDivisor = 3;
                    SpecialEvents_Patch.PirateDivisor = 6;
                    SpecialEvents_Patch.FrequencyOfTravellingCart = 0.05f;
                    MommaCrab_Patch.MommaCrabFrequency = 2;
                    break;
                default:
                    SpecialEvents_Patch.PortalDivisor = 4;
                    SpecialEvents_Patch.PirateDivisor = 7;
                    SpecialEvents_Patch.FrequencyOfTravellingCart = 0.1f;
                    MommaCrab_Patch.MommaCrabFrequency = 3;
                    break;
            }
            if (!AllowStrangePortals) SpecialEvents_Patch.PortalDivisor = 1000000;
            if (!AllowPirateShips) SpecialEvents_Patch.PirateDivisor = 1000000;
            DifficultyMod.Log($"Portal Checks? {AllowStrangePortals} Frequency {SpecialEvents_Patch.PortalDivisor} Rare Portals? {AllowRarePortals}");
            DifficultyMod.Log($"Pirate Ships? {AllowPirateShips} Frequency {SpecialEvents_Patch.PirateDivisor} Momma Crab Freequncy: {MommaCrab_Patch.MommaCrabFrequency}");
            DifficultyMod.Log($"Roaming Animals? {AllowAnimalsToRoam} New Game Curses? {AllowCursesAtStart} Safe Location? {AllowDangerousLocations}");
        }
    }

    [HarmonyPatch(typeof(WorldManager),nameof(WorldManager.LoadSaveRound))]
    internal class LoadSaveRound_Patch
    {
        static void Postfix(WorldManager __instance, SaveRound saveRound)
        {
            DifficultyMod.instance.ApplySettings();
        }
    }
}
