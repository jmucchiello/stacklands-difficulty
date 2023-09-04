using HarmonyLib;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DifficultyModNS
{
    public partial class DifficultyMod : Mod
    {
        public static DifficultyMod instance;
        public static Harmony myHarmonyInstance;

        private void Awake()
        {
            instance = this;
        }
        public override void Ready()
        {
            myHarmonyInstance = Harmony;
            Harmony.PatchAll();
            SetupConfig();
            Logger.Log("Ready!");
        }

        public static void Log(string msg) => instance.Logger.Log(msg);
        public static void LogError(string msg) => instance.Logger.LogError(msg);

        public float foodBlueprintModifier = 1f;

        public void SetupConfig()
        {
            configDifficulty = new ConfigDifficulty("difficultymod_difficulty", Config);
            configEnabling = new ConfigEnabling("difficultymod_enabling", Config);
            configSpawnSites = new ConfigSpawnSites("difficultymod_spawning", Config, SpawnSites.Anywhere);
            ConfigEntry<bool> configDefaults = new ConfigEntry<bool>("none", Config, false, new ConfigUI()
            {
                Hidden = true,
                OnUI = delegate (ConfigEntryBase c)
                {
                    CustomButton btn = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab, ModOptionsScreen.instance.ButtonsParent);
                    btn.transform.localScale = Vector3.one;
                    btn.transform.localPosition = Vector3.zero;
                    btn.transform.localRotation = Quaternion.identity;
                    btn.TextMeshPro.text = ConfigEntryHelper.RightAlign(SokLoc.Translate("difficultymod_defaults"));
                    btn.Clicked += delegate
                    {
                        configDifficulty.SetDefaults();
                        configEnabling.SetDefaults();
                        configSpawnSites.SetDefaults();
                    };
                }
            });
            Config.OnSave = () => {
                ApplySettings();
            };
        }

        public void ApplySettings()
        {
            Log($"Overall Difficulty set to {difficulty}");
            SetupSpawnSites();
            SetupSummoningFrequency();
            SetupStrengthMultiplier();
            SetupFoodMultiplier();
            SetupNewVillagerChecks();
            SetupStorageCapacity();
            SetupDLC();
        }

        public void SetupFoodMultiplier()
        {
            foodBlueprintModifier = Difficulty switch
            {
                <= DifficultyType.VeryEasy => 0.8f,
                >= DifficultyType.VeryHard => 1.2f,
                _ => 1f
            };
            Log($"Food multiplier: {foodBlueprintModifier}");
            if (foodBlueprintModifier != 1f)
            {
                new BlueprintModifier() { blueprintId = "blueprint_growth", subprintindex = -1, multiplier = foodBlueprintModifier }.AddToList();
            }
        }

        public void SetupStorageCapacity()
        {
            switch (storageCapacity)
            {
                case StorageCapacity.Small:
                    CardCapIncrease.ShedIncrease = 3;
                    CardCapIncrease.WarehouseIncrase = 12;
                    break;
                case StorageCapacity.Normal:
                    CardCapIncrease.ShedIncrease = 4;
                    CardCapIncrease.WarehouseIncrase = 14;
                    break;
                case StorageCapacity.Large:
                    CardCapIncrease.ShedIncrease = 5;
                    CardCapIncrease.WarehouseIncrase = 18;
                    break;
                case StorageCapacity.Enormous:
                    CardCapIncrease.ShedIncrease = 6;
                    CardCapIncrease.WarehouseIncrase = 25;
                    break;
            }
        }

        public void SetupSummoningFrequency()
        {
            switch (difficulty)
            {
                case DifficultyType.Impossible:
                    PortalFrequncy = 2;
                    PirateFrequncy = 4;
                    MommaFrequncy = 1;
                    break;
                case <= DifficultyType.VeryEasy:
                    PortalFrequncy = 6;
                    PirateFrequncy = 10;
                    MommaFrequncy = 4;
                    break;
                case >= DifficultyType.Hard:
                    PortalFrequncy = 3;
                    PirateFrequncy = 6;
                    MommaFrequncy = 2;
                    break;
                default:
                    PortalFrequncy = 4;
                    PirateFrequncy = 7;
                    MommaFrequncy = 3;
                    break;
            }
            DifficultyMod.Log($"Portal Checks? {AllowStrangePortals} Frequency {PortalFrequncy} Rare Portals? {AllowRarePortals}");
            DifficultyMod.Log($"Pirate Ships? {AllowPirateShips} Frequency {PirateFrequncy} Momma Crab Freequncy: {MommaFrequncy}");
            DifficultyMod.Log($"Roaming Animals? {AllowAnimalsToRoam} New Game Curses? {AllowCursesAtStart} Safe Location? {AllowDangerousLocations}");
            DifficultyMod.Log($"Storage Capacity: {storageCapacity}");
        }
    }


}