using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace DifficultyModNS
{
    public enum DifficultyType { VeryEasy, Easy, Normal, Challenging, Hard, VeryHard, Brutal }; // order matters

    public partial class DifficultyMod : Mod
    {
        public DifficultyType difficulty;
        public ConfigDifficulty configDifficulty;

        public static DifficultyType Difficulty { get => DifficultyMod.instance.difficulty; }
    }

    public class ConfigDifficulty : ConfigEntryEnum<DifficultyType>
    {
        public ConfigDifficulty(string name, ConfigFile configFile, DifficultyType defaultValue, ConfigUI UI = null)
            : base(name, configFile, defaultValue, UI)
        {
            onDisplayAnchorText = delegate ()
            {
                return SokLoc.Translate("difficultymod_config_difficulty") + " " + ColorText("blue", SokLoc.Translate($"difficultymod_config_difficulty_{(int)BoxedValue}"));
            };
            onDisplayAnchorTooltip = delegate ()
            {
                return SokLoc.Translate("difficultymod_config_difficulty_tooltip");
            };
            onDisplayEnumText = delegate (DifficultyType s)
            {
                return SokLoc.Translate($"difficultymod_config_difficulty_{(int)(object)s}");
            };
            onDisplayEnumTooltip = delegate (DifficultyType s)
            {
                return SokLoc.Translate($"difficultymod_config_difficulty_{(int)(object)s}_tooltip");
            };
            popupMenuTitleText = SokLoc.Translate("difficultymod_config_difficulty_menu_text");
            popupMenuHelpText = SokLoc.Translate("difficultymod_config_difficulty_menu_tooltip");

            onChange += delegate (DifficultyType newValue) {
                DifficultyMod.Log($"onChange({newValue}) called");
                DifficultyMod.instance.difficulty = newValue;
                return true;
            };

            CloseButtonText = SokLoc.Translate("difficultymod_closemenu");
            currentValueColor = Color.blue;
        }

        public override void SetDefaults()
        {
            DifficultyMod.Log($"ConfigDifficulty SetDefaults called");
            base.SetDefaults();
        }
    }
}
