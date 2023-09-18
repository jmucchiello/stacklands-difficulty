using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using Newtonsoft.Json.Linq;

namespace DifficultyModNS
{
    public enum StorageCapacity
    {
        Small, Normal, Large, Enormous
    };

    public partial class DifficultyMod : Mod
    {
        public ConfigEnabling configEnabling;

        public static StorageCapacity storageCapacity => instance.configEnabling.storageCapacity;
        public static bool AllowSadEvents => true;
        public static bool AllowMommaCrab => true;
        public static bool AllowRarePortals => instance.configEnabling.enabling.rarePortals;
        public static bool AllowStrangePortals => instance.configEnabling.enabling.portals;
        public static bool AllowPirateShips => instance.configEnabling.enabling.pirates;
        public static bool AllowDangerousLocations => instance.configEnabling.enabling.locations;
        public static bool AllowAnimalsToRoam => instance.configEnabling.enabling.roamingAnimals;
        public static bool AllowCursesAtStart => instance.configEnabling.enabling.cursesAtStart;

        public static StorageCapacity GetStorageCapacity => instance.configEnabling.storageCapacity;
    }

    public class Enabling
    {
        public bool rarePortals = true, portals = true, pirates = true, locations = true, roamingAnimals = true, cursesAtStart = false;
    }

    public class ConfigEnabling : ConfigEntryModalHelper
    {
        public override object BoxedValue { get => enabling; set => enabling = (Enabling)value; }
        public Enabling enabling = new();
        public StorageCapacity storageCapacity = StorageCapacity.Normal;
        private string toolTip;

        private static CustomButton headerText;

        private T LoadConfigEntry<T>(string key, T defValue)
        {
            if (Config.Data.TryGetValue(key, out JToken value))
            {
                return value.Value<T>();
            }
            return defValue;
        }

        public ConfigEnabling(string name, ConfigFile config)
        {
            Name = name;
            Config = config;
            ValueType = typeof(ConfigEnabling);
            toolTip = SokLoc.Translate("difficultymod_config_enabling_tooltip");

            enabling.rarePortals = LoadConfigEntry<bool>("difficultymod_enableRarePortals", enabling.rarePortals);
            enabling.portals = LoadConfigEntry<bool>("difficultymod_enablePortals", enabling.portals);
            enabling.pirates = LoadConfigEntry<bool>("difficultymod_enablePirates", enabling.pirates);
            enabling.locations = LoadConfigEntry<bool>("difficultymod_enableLocationEnemies", enabling.locations);
            enabling.roamingAnimals = LoadConfigEntry<bool>("difficultymod_enableRangeFreeAnimals", enabling.roamingAnimals);
            enabling.cursesAtStart = LoadConfigEntry<bool>("difficultymod_enableCursesAtStart", enabling.cursesAtStart);
            storageCapacity = (StorageCapacity)LoadConfigEntry<int>("difficultymod_storage_capacity", (int)storageCapacity);

            UI = new ConfigUI()
            {
                Hidden = true,
                OnUI = delegate (ConfigEntryBase c)
                {
                    headerText = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab, ModOptionsScreen.instance.ButtonsParent);
                    headerText.transform.localScale = Vector3.one;
                    headerText.transform.localPosition = Vector3.zero;
                    headerText.transform.localRotation = Quaternion.identity;
                    headerText.TextMeshPro.text = SokLoc.Translate("difficultymod_config_enabling_header");
                    headerText.TooltipText = toolTip;
                    headerText.Clicked += delegate ()
                    {
                        DifficultyMod.Log("Calling OpenMenu");
                        OpenMenu();
                    };
                    _ = UnityEngine.Object.Instantiate(ModOptionsScreen.instance.SpacerPrefab, ModOptionsScreen.instance.ButtonsParent);
                }
            };
            Config.Entries.Add(this);
        }

        CustomButton btnRare, btnPortals, btnPirates, btnLocations, btnAnimals, btnCurses, btnStorage;

        public void OpenMenu()
        {
            if (GameCanvas.instance.ModalIsOpen) return;
            ModalScreen.instance.Clear();
            popup = ModalScreen.instance;
            popup.SetTexts(SokLoc.Translate("difficultymod_config_enabling_menu_text"),
                                 SokLoc.Translate("difficultymod_config_enabling_menu_tooltip"));

            btnPortals = NewButton(enabling.portals, "strange");
            btnPortals.Clicked += delegate
            {
                enabling.portals = !enabling.portals;
                btnRare.enabled = enabling.portals;
                btnRare.TextMeshPro.faceColor = Color.white;
                btnRare.Update();
                btnPortals.TextMeshPro.text = ButtonAllowText(enabling.portals, "strange");
                Config.Data["difficultymod_enablePortals"] = enabling.portals;
            };

            btnRare = NewButton(enabling.rarePortals, "rare");
            btnRare.Clicked += delegate
            {
                enabling.rarePortals = !enabling.rarePortals;
                btnRare.TextMeshPro.text = ButtonAllowText(enabling.portals ? enabling.rarePortals : false, "rare");
                Config.Data["difficultymod_enableRarePortals"] = enabling.rarePortals;
            };
            btnRare.enabled = enabling.portals;
            btnRare.Update();

            btnPirates = NewButton(enabling.pirates, "pirate");
            btnPirates.Clicked += delegate
            {
                enabling.pirates = !enabling.pirates;
                btnPirates.TextMeshPro.text = ButtonAllowText(enabling.pirates, "pirate");
                Config.Data["difficultymod_enablePirates"] = enabling.pirates;
            };

            btnLocations = NewButton(enabling.locations, "location");
            btnLocations.Clicked += delegate
            {
                enabling.locations = !enabling.locations;
                btnLocations.TextMeshPro.text = ButtonAllowText(enabling.locations, "location");
                Config.Data["difficultymod_enableLocationEnemies"] = enabling.locations;
            };

            btnAnimals = NewButton(enabling.locations, "rangefree");
            btnAnimals.Clicked += delegate
            {
                enabling.roamingAnimals = !enabling.roamingAnimals;
                btnAnimals.TextMeshPro.text = ButtonAllowText(enabling.roamingAnimals, "rangefree");
                Config.Data["difficultymod_enableRangeFreeAnimals"] = enabling.roamingAnimals;
            };

            if (WorldManager.instance.IsSpiritDlcActive())
            {
                btnCurses = NewButton(enabling.cursesAtStart, "curses");
                btnCurses.Clicked += delegate
                {
                    enabling.cursesAtStart = !enabling.cursesAtStart;
                    btnCurses.TextMeshPro.text = ButtonAllowText(enabling.cursesAtStart, "curses");
                    Config.Data["difficultymod_enableCursesAtStart"] = enabling.cursesAtStart;
                };
            }

            btnStorage = NewButton(false, "storage");
            btnStorage.Clicked += delegate
            {
                if (storageCapacity == StorageCapacity.Enormous) storageCapacity = StorageCapacity.Small;
                else ++storageCapacity;
                btnStorage.TooltipText = SokLoc.Translate($"difficultymod_config_storage_{storageCapacity}_tooltip");
                btnStorage.TextMeshPro.text = ButtonAllowText(false, "storage");
                Config.Data["difficultymod_storage_capacity"] = (int)storageCapacity;
            };

            popup.AddOption(RightAlign(SokLoc.Translate("difficultymod_defaults")), SetDefaults);
            popup.AddOption(RightAlign(SokLoc.Translate("difficultymod_closemenu")), CloseMenu);
            GameCanvas.instance.OpenModal();
        }

        private string ButtonAllowText(bool enabled, string place)
        {
            if (place == "storage")
            {
                return SokLoc.Translate($"difficultymod_config_storage") +
                       ": " + ColorText(Color.blue, SokLoc.Translate($"difficultymod_config_storage_{storageCapacity}"));
            }
            else if (place == "rare" && enabling.portals)
            {
                return SokLoc.Translate($"difficultymod_config_enabling_{place}_{(enabled ? "enable" : "disable")}");
            }
            else if (place == "rare" && !enabling.portals)
            {
                return ColorText("#bfbfbf", "<s>" + SokLoc.Translate("difficultymod_config_enabling_rare_noportals") + "</s>");
            }
            else
            {
                if (place == "strange" && btnRare != null) btnRare.TextMeshPro.text = ButtonAllowText(enabling.rarePortals, "rare");
                return SokLoc.Translate($"difficultymod_config_enabling_{place}_{(enabled ? "enable" : "disable")}");
            }
        }

        private CustomButton NewButton(bool state, string place)
        {
            CustomButton btn = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab, ModOptionsScreen.instance.ButtonsParent);
            btn.transform.SetParent(popup.ButtonParent);
            btn.transform.localScale = Vector3.one;
            btn.transform.localPosition = Vector3.zero;
            btn.transform.position = new Vector3 { x = 50, y = 0, z = 0 };
            btn.transform.localRotation = Quaternion.identity;
            btn.TextMeshPro.text = ButtonAllowText(state, place);
            if (place != "storage")
            {
                SokTerm term = SokLoc.instance.CurrentLocSet.GetTerm($"difficultymod_config_enabling_{place}_tooltip");
                if (term != null)
                {
                    btn.TooltipText = SokLoc.Translate($"difficultymod_config_enabling_{place}_tooltip");
                }
            }
            else
            {
                SokTerm term = SokLoc.instance.CurrentLocSet.GetTerm($"difficultymod_config_storage_{storageCapacity}_tooltip");
                if (term != null)
                {
                    btn.TooltipText = SokLoc.Translate($"difficultymod_config_storage_{storageCapacity}_tooltip");
                }
            }
            return btn;
        }

        public override void SetDefaults()
        {
            enabling = new();
            storageCapacity = StorageCapacity.Normal;
            if (popup != null)
            {
                btnAnimals.TextMeshPro.text = ButtonAllowText(enabling.roamingAnimals, "rangefree");
                btnLocations.TextMeshPro.text = ButtonAllowText(enabling.locations, "location");
                btnPirates.TextMeshPro.text = ButtonAllowText(enabling.pirates, "pirate");
                btnPortals.TextMeshPro.text = ButtonAllowText(enabling.portals, "strange");
                btnRare.TextMeshPro.text = ButtonAllowText(enabling.rarePortals, "rare");
                btnRare.enabled = enabling.portals;
                if (WorldManager.instance.IsSpiritDlcActive())
                {
                    btnCurses.TextMeshPro.text = ButtonAllowText(enabling.cursesAtStart, "curses");
                }
                btnStorage.TextMeshPro.text = ButtonAllowText(false, "storage");
            }
            Config.Data["difficultymod_enableRarePortals"] = enabling.rarePortals;
            Config.Data["difficultymod_enablePortals"] = enabling.portals;
            Config.Data["difficultymod_enablePirates"] = enabling.pirates;
            Config.Data["difficultymod_enableLocationEnemies"] = enabling.locations;
            Config.Data["difficultymod_enableRangeFreeAnimals"] = enabling.roamingAnimals;
            Config.Data["difficultymod_enableCursesAtStart"] = enabling.cursesAtStart;
            Config.Data["difficultymod_storage_capacity"] = (int)storageCapacity;
        }
    }

    [HarmonyPatch(typeof(Animal))]
    public class RangeFreeAnimals
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Animal.CanMove), MethodType.Getter)]
        static void Postfix(Animal __instance, ref bool __result)
        {
            if (!DifficultyMod.AllowAnimalsToRoam)
            {
                __result = false;
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Animal.UpdateCard))]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /***
             *      Changed Animal.UpdateCard:
             *      		if (CreateTimer >= CreateTime && (moveFlag || InAnimalPen))
             *      to
             *              if (CreateTimer >= CreateTime && (CanMove || InAnimalPen))
             *
             ***/
            try
            {
                Type myClass = typeof(SpecialEvents_Patch);
                List<CodeInstruction> result = new CodeMatcher(instructions)
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Mob),"moveFlag"))
                    )
                    .Set(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Animal), "CanMove"))
                    .InstructionEnumeration()
                    .ToList();
//                result.ForEach(instruction => DifficultyMod.Log($"{instruction}"));
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

    [HarmonyPatch(typeof(RunOptionsScreen), "SetCurseButton")]
    public class CursesAtStart
    {
        static void Prefix(RunOptionsScreen __instance, CustomButton but, ref bool curseUnlocked, bool curseEnabled, string mainTerm)
        {
            if (DifficultyMod.AllowCursesAtStart)
            {
                curseUnlocked = true;
            }
        }
    }

    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.CardCapIncrease))]
    public class CardCapIncrease
    {
        public static int ShedIncrease = 4;
        public static int WarehouseIncrease = 14;

        static bool Prefix(WorldManager __instance, ref int __result, GameBoard board)
        {
            __result = __instance.GetCardCount("shed", board) * ShedIncrease
                     + __instance.GetCardCount("warehouse", board) * WarehouseIncrease
                     + __instance.GetCardCount("lighthouse", board) * WarehouseIncrease;
            return false;
        }
    }

}
