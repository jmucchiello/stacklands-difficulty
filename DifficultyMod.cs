using HarmonyLib;
using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
            WMCreateCard_Patch.WM_IsLoadingSaveRound = new Traverse(I.WM).Field<bool>("IsLoadingSaveRound");
            Harmony.PatchAll();
        }
        public override void Ready()
        {
            SetupConfig();
            SetupShowDifficulty();
            ApplySettings();
            Logger.Log("Ready!");
        }

        public void SetupConfig()
        {
            new ConfigFreeText("difficultymod_label_gameplay", Config, null);
            configDifficulty = new ConfigDifficulty("difficultymod_difficulty", Config, DifficultyType.Normal);
            configEnabling = new ConfigEnabling("difficultymod_enabling", Config);
            configSpawnSites = new ConfigSpawnSites("difficultymod_spawning", Config, SpawnSites.Anywhere);
            new ConfigEmtySpace(Config);
            new ConfigFreeText("difficultymod_label_modsettings", Config, null);
            ConfigShowDifficulty();
            ConfigFreeText configResetDefaults = new("none", Config, ConfigEntryModalHelper.RightAlign(SokLoc.Translate("difficultymod_reset_defaults")));
            configResetDefaults.Clicked += delegate (ConfigEntryBase c, CustomButton _)
            {
                DifficultyMod.Log($"configDefaults.clicked called");
                configDifficulty.SetDefaults();
                configEnabling.SetDefaults();
                configSpawnSites.SetDefaults();
                configShowDifficulty.SetDefaults();
            };

            Config.OnSave = ApplySettings;
        }

        public void SetupNoPause()
        {
            InputController.instance.PlayerInput.actions["time_pause"].Disable();
            InputController.instance.PlayerInput.actions["time_3"].Disable();
            NoPauseGame.Setup();
        }

        public void ApplySettings()
        {
            GameCardStartTimer_Patch.modifiers.Clear();
            BlueprintStartTimer_Patch.modifiers.Clear();
            difficulty = configDifficulty.Value;
            Log($"Overall Difficulty set to {difficulty}");
            ApplySpawnSites();
            ApplySummoningFrequency();
            ApplyStrengthMultiplier();
            ApplyFoodMultiplier();
            ApplyNewVillagerChecks();
            ApplyStorageCapacity();
            ApplyDLC();
            ApplyShowDifficulty();
        }

        public static bool AnyCardInCardBagIsOfType(CardBag cardBag, Type type)
        {
            List<string> cards = cardBag.GetCardsInBag(I.WM.GameDataLoader);
            foreach (string card in cards)
            {
                if (type.IsAssignableFrom(I.WM.GameDataLoader.GetCardFromId(card).GetType())) return true;
            }
            return false;
        }

        public static bool BlueprintCreatesCardsOfType(Blueprint bp, Type type)
        {
            foreach (Subprint sp in bp.Subprints)
            {
                if (!String.IsNullOrEmpty(sp.ResultCard) && I.WM.GameDataLoader.GetCardFromId(sp.ResultCard).GetType().Equals(type)) return true;
                foreach (string s in sp.ExtraResultCards)
                {
                    if (!String.IsNullOrEmpty(s) && I.WM.GameDataLoader.GetCardFromId(s).GetType().Equals(type)) return true;
                }
            }
            return false;
        }

        public void ApplyFoodMultiplier()
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
                foreach (Blueprint bp in I.WM.GameDataLoader.BlueprintPrefabs)
                {
                    if (BlueprintCreatesCardsOfType(bp, typeof(Food)))
                    {
//                        Log($"SetupFoodMultiplier adding {bp.Id}");
                        new BlueprintTimerModifier() { blueprintId = bp.Id, subprintindex = -1, multiplier = foodBlueprintModifier }.AddToList();
                    }
                }
                new GameCardTimerModifier() { actionId = "complete_harvest", myCardDataType = typeof(Harvestable), multiplier = foodBlueprintModifier, hasCardBag = "MyCardBag", cardBagProduces = typeof(Food) }.AddToList();
            }
        }

        public void ApplyStorageCapacity()
        {
            switch (GetStorageCapacity)
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

        public void ApplySummoningFrequency()
        {
            switch (difficulty)
            {
                case DifficultyType.Brutal:
                    SpecialEvents_Patch.PortalDivisor = 2;
                    SpecialEvents_Patch.PirateDivisor = 4;
                    SpecialEvents_Patch.FrequencyOfTravellingCart = 0.02f;
                    MommaCrab_Patch.MommaCrabFrequency = 1;
                    break;
                case >= DifficultyType.Hard:
                    SpecialEvents_Patch.PortalDivisor = 3;
                    SpecialEvents_Patch.PirateDivisor = 6;
                    SpecialEvents_Patch.FrequencyOfTravellingCart = 0.05f;
                    MommaCrab_Patch.MommaCrabFrequency = 2;
                    break;
                case <= DifficultyType.VeryEasy:
                    SpecialEvents_Patch.PortalDivisor = 6;
                    SpecialEvents_Patch.PirateDivisor = 10;
                    SpecialEvents_Patch.FrequencyOfTravellingCart = 0.15f;
                    MommaCrab_Patch.MommaCrabFrequency = 4;
                    break;
                default:
                    SpecialEvents_Patch.PortalDivisor = 4;
                    SpecialEvents_Patch.PirateDivisor = 7;
                    SpecialEvents_Patch.FrequencyOfTravellingCart = 0.1f;
                    MommaCrab_Patch.MommaCrabFrequency = 3;
                    break;
            }
            if (!AllowStrangePortals) SpecialEvents_Patch.PortalDivisor = 1000000;
            if (!AllowRarePortals) I.CRV.StrangePortalSpawns = 1000000;
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
