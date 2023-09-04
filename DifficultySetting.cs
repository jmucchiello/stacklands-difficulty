using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace DifficultyModNS
{
    public enum DifficultyType { VeryEasy, Easy, Normal, Challenging, Hard, VeryHard, Impossible }; // order matters

    public partial class DifficultyMod : Mod
    {
        public DifficultyType difficulty;
        public ConfigDifficulty configDifficulty;

        public static DifficultyType Difficulty { get => DifficultyMod.instance.difficulty; }
    }

    public class ConfigDifficulty : ConfigEntryHelper
    {
        public static ModalScreen DifficultyMenu;
        public string[] DifficultyNames = new string[Enum.GetValues(typeof(DifficultyType)).Length];
        CustomButton setting;

        public ConfigDifficulty(string name, ConfigFile configFile)
        {
            for (int i = 0; i < DifficultyNames.Length; ++i)
            {
                DifficultyNames[i] = SokLoc.Translate($"difficultymod_config_difficulty_{i}");
            }
            Name = name;
            Config = configFile;
            ValueType = typeof(int);
            List<string> toolTips = new List<string>();
            BoxedValue = Config.GetValue(Name, typeof(int)) ?? DifficultyType.Normal;

            UI = new ConfigUI()
            {
                Hidden = true,
                OnUI = delegate (ConfigEntryBase c)
                {
                    setting = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab, ModOptionsScreen.instance.ButtonsParent);
                    setting.transform.localScale = Vector3.one;
                    setting.transform.localPosition = Vector3.zero;
                    setting.transform.localRotation = Quaternion.identity;
                    SetDifficultyText((int)DifficultyMod.Difficulty);
                    setting.Clicked += delegate
                    {
                        OpenMenu();
                    };
                    _ = UnityEngine.Object.Instantiate(ModOptionsScreen.instance.SpacerPrefab, ModOptionsScreen.instance.ButtonsParent);
                }
            };
            Config.Entries.Add(this);
        }

        public void OpenMenu()
        {
            if (GameCanvas.instance.ModalIsOpen) return;
            ModalScreen.instance.Clear();
            DifficultyMenu = ModalScreen.instance;
            DifficultyMenu.SetTexts(SokLoc.Translate("difficultymod_config_difficulty_menu_text"),
                                    SokLoc.Translate("difficultymod_config_difficulty_menu_tooltip"));
            for (int i = 0; i < DifficultyNames.Length; i++)
            {
                int j = i;
                CustomButton btn = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab);
                btn.transform.SetParent(DifficultyMenu.ButtonParent);
                btn.transform.localPosition = Vector3.zero;
                btn.transform.localScale = Vector3.one;
                btn.transform.localRotation = Quaternion.identity;
                btn.TextMeshPro.text = DifficultyNames[j];
                SokTerm term = SokLoc.instance.CurrentLocSet.GetTerm($"difficultymod_config_difficulty_{i}_tooltip");
                btn.TooltipText = term?.GetText() ?? ""; // don't display ---MISSING---
                btn.Clicked += delegate ()
                {
                    SetDifficultyText(j);
                    GameCanvas.instance.CloseModal();
                };
            }
            GameCanvas.instance.OpenModal();
        }

        private void SetDifficultyText(int i)
        {
            setting.TextMeshPro.text = SokLoc.Translate("difficultymod_config_difficulty") + ": " + SokLoc.Translate($"difficultymod_config_difficulty_{i}");
            BoxedValue = (DifficultyType)i;
            Config.Data[Name] = i;
        }

        public override void SetDefaults()
        {
            SetDifficultyText((int)DifficultyType.Normal);
        }

        public override object BoxedValue { get => DifficultyMod.Difficulty; set => DifficultyMod.instance.difficulty = (DifficultyType)value; }
    }
}
