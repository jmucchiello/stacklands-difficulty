﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyModNS
{
    public class ConfigToggledEnum<T> : ConfigEntryHelper where T : Enum
    {
        private int content; // access via BoxedValue
        private int defaultValue; // access via BoxedValue
        private string[] EnumNames = new string[0];
        private CustomButton anchor;  // this holds the ModOptionsScreen text that is clicked to open the menu

        public delegate string OnDisplayText();
        public delegate string OnDisplayEnumText(T t);
        public OnDisplayText onDisplayText;
        public OnDisplayText onDisplayTooltip;
        public OnDisplayEnumText onDisplayEnumText;

        public delegate bool OnChange(T newValue); // return false to prevent acceptance of newValue
        public OnChange onChange;

        public string CloseButtonText = null; // if null, no close button is created
        public Color currentValueColor = Color.black;

        private int fontSize = 0;
        public int FontSize
        {
            get => fontSize;
            set {
                fontSize = value;
//                anchor.TextMeshPro.text = SizeText(fontSize, GetDisplayText());
            }
        }

        public virtual T DefaultValue { get => (T)(object)defaultValue; set => defaultValue = (int)(object)value; }
        public virtual T Value { get => (T)(object)content; set => content = (int)(object)value; }

        public override object BoxedValue
        {
            get => content;
            set => content = (int)value;
        }

        public ConfigToggledEnum(string name, ConfigFile configFile, T defaultValue, ConfigUI ui = null, bool parentIsPopup = false)
        {
            Name = name;
            EnumNames = EnumHelper.GetNames<T>().ToArray();
            ValueType = typeof(object); // to avoid shenanigans from ModOptionScreen's default processing of string/int/bool
            DefaultValue = defaultValue;
            Config = configFile;
            BoxedValue = LoadConfigEntry<int>(name, (int)(object)defaultValue);

            UI = new ConfigUI()
            {
                Hidden = true,
                Name = ui?.Name,
                NameTerm = ui?.NameTerm ?? name,
                Tooltip = ui?.Tooltip,
                TooltipTerm = ui?.TooltipTerm,
                PlaceholderText = ui?.PlaceholderText,
                RestartAfterChange = ui?.RestartAfterChange ?? false,
                ExtraData = ui?.ExtraData,
                OnUI = delegate (ConfigEntryBase c)
                {
                    anchor = DefaultButton(parentIsPopup ? I.Modal.ButtonParent : I.MOS.ButtonsParent,
                                           SizeText(FontSize, GetDisplayText()),
                                           GetDisplayTooltip());
                    anchor.Clicked += delegate
                    {
                        if (++content >= EnumNames.Length) content = 0;
                        anchor.TextMeshPro.text = SizeText(FontSize, GetDisplayText());
                        anchor.TooltipText = GetDisplayTooltip();
                        Config.Data[Name] = content;
                    };

                }
            };
            configFile.Entries.Add(this);
        }

        public virtual string GetDisplayText()
        {
            return onDisplayText != null ? onDisplayText() : UI.GetName()
                 + ": "
                 + ColorText(currentValueColor, onDisplayEnumText != null ? onDisplayEnumText((T)BoxedValue) : EnumNames[content]);
        }
        public virtual string GetDisplayTooltip()
        {
            return onDisplayTooltip != null ? onDisplayTooltip() : UI.GetTooltip();
        }
        public override void SetDefaults()
        {
            Value = DefaultValue;
            anchor.TextMeshPro.text = GetDisplayText();
            anchor.TooltipText = GetDisplayTooltip();
        }
    }
}
