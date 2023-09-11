using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DifficultyModNS
{
    public class ConfigFreeText : ConfigEntryBase
    {
        private string Text;
        public override object BoxedValue { get => new object(); set => _ = value; }

        public Action<ConfigEntryBase> Clicked;

        /**
         *  Create a header line in the config screen. Also useful for stuff like "Close" in ModalScreen
         **/
        public ConfigFreeText(string name, ConfigFile config, string text)
        {
            Name = name;
            Config = config;
            ValueType = typeof(object);
            SokTerm term = SokLoc.instance.CurrentLocSet.GetTerm(text) ?? SokLoc.FallbackSet.GetTerm(text);
            Text = term?.GetText() ?? text;
            UI = new ConfigUI()
            {
                Hidden = true,
                OnUI = delegate (ConfigEntryBase c)
                {
                    CustomButton btn = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab, ModOptionsScreen.instance.ButtonsParent);
                    btn.transform.localScale = Vector3.one;
                    btn.transform.localPosition = Vector3.zero;
                    btn.transform.localRotation = Quaternion.identity;
                    btn.TextMeshPro.text = Text;
                    btn.Clicked += delegate ()
                    {
                        Clicked?.Invoke(this);
                    };
                }
            };
            config.Entries.Add(this);
        }
    }

    public class ConfigEntryEnum<T> : ConfigEntryModalHelper where T : Enum
    {
        private int content; // access via BoxedValue
        private int defaultValue; // access via BoxedValue
        private CustomButton anchor;  // this holds the ModOptionsScreen text that is clicked to open the menu

        public delegate string OnDisplayAnchorText();       // the text seen in the main option screen
        public delegate string OnDisplayEnumText(T t);
        public delegate string OnDisplayEnumTooltip(T t);
        public OnDisplayAnchorText onDisplayAnchorText; 
        public OnDisplayEnumText onDisplayEnumText;
        public OnDisplayEnumTooltip onDisplayEnumTooltip;

        public delegate bool OnChange(T newValue); // return false to prevent acceptance of newValue
        public OnChange onChange;

        public string popupMenuTitleText; // the title bar text of the popup screen
        public string popupMenuHelpText; // the help text that appears below the title bar text

        public string CloseButtonText = null; // if null, no close button is created
        public Color currentValueColor = Color.black;

        public virtual T DefaultValue { get => (T)(object)defaultValue; set => defaultValue = (int)(object)value; }
        public virtual T Value { get => (T)(object)content; set => content = (int)(object)value; }

        public override object BoxedValue {
            get => content;
            set => content = (int)value;
        }

        public ConfigEntryEnum(string name, ConfigFile configFile, T defaultValue, ConfigUI ui = null)
        {
            Name = name;
            ValueType = typeof(System.Object); // to avoid shenanigans from ModOptionScreen's default processing of string/int/bool
            DefaultValue = defaultValue;
            Config = configFile;
            if (Config.Data.TryGetValue(name, out _))
            {
                BoxedValue = Config.GetValue<int>(name); // store as int to make it easier to reload.
            }
            else
            {
                BoxedValue = defaultValue;
            }
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
                    anchor = DefaultButton(ModOptionsScreen.instance.ButtonsParent,
                                           onDisplayAnchorText != null ? onDisplayAnchorText() : c.UI.GetName(),
                                           c.UI.GetTooltip());
                    anchor.Clicked += delegate
                    {
                        OpenMenu();
                    };
                }
            };
            configFile.Entries.Add(this);
        }

        private string EntryText(T entry)
        {
            string text = onDisplayEnumText != null ? onDisplayEnumText(entry) : Enum.GetName(typeof(T), entry);
            if (currentValueColor != null && EqualityComparer<T>.Default.Equals(entry, (T)BoxedValue))
            {
                text = ColorText(currentValueColor, text);
            }
            return text;
        }

        private void OpenMenu()
        {
            if (GameCanvas.instance.ModalIsOpen) return;
            ModalScreen.instance.Clear();
            popup = ModalScreen.instance;
            popup.SetTexts(popupMenuTitleText, popupMenuHelpText);
            foreach (T t in Enum.GetValues(typeof(T)))
            {
                T thisEntry = t; // so the delegate grabs the correct value, not the loop variable
                CustomButton btn = DefaultButton(popup.ButtonParent,
                                                 EntryText(thisEntry),
                                                 onDisplayEnumTooltip != null ? onDisplayEnumTooltip(thisEntry) : null);
                btn.Clicked += delegate ()
                {
                    if (onChange == null || onChange(thisEntry))
                    {
                        Config.Data[Name] = (int)(object)thisEntry;
                        content = (int)(object)thisEntry;
                        anchor.TextMeshPro.text = onDisplayAnchorText != null ? onDisplayAnchorText() : UI.GetName();
                        CloseMenu();
                    }
                };
            }
            if (CloseButtonText != null)
            {
                CustomButton btnClose = DefaultButton(ModalScreen.instance.ButtonParent, RightAlign(CloseButtonText));
                btnClose.Clicked += CloseMenu;
            }
            GameCanvas.instance.OpenModal();
        }

        public override void SetDefaults()
        {
            BoxedValue = DefaultValue;
            if (popup != null)
            {
                anchor.TextMeshPro.text = onDisplayAnchorText != null ? onDisplayAnchorText() : UI.GetName();
            }
        }
    }


    public class ConfigEntryStringList : ConfigEntryModalHelper 
    {
        private string content; // access via BoxedValue
        private CustomButton anchor;  // this holds the ModOptionsScreen text that is clicked to open the menu
        private List<string> values;

        public delegate string OnDisplayText();
        public delegate string OnDisplayTooltip(string entry);
        public OnDisplayText onDisplayAnchorText;  // the text seen in the main option screen
        public OnDisplayTooltip onDisplayTooltip;  // the text seen in the main option screen

        public delegate bool OnChange(string newValue); // return false to prevent acceptance of newValue
        public OnChange onChange;

        public string popupMenuTitleText; // the title bar text of the popup screen
        public string popupMenuHelpText; // the help text that appears below the title bar text

        public string CloseButtonText = null; // if null, no close button is created
        public Color currentValueColor = Color.black;

        public virtual string DefaultValue { get; private set; }

        public override object BoxedValue
        {
            get => content; set => content = value.ToString();
        }

        public ConfigEntryStringList(string name, ConfigFile configFile, List<string> list, string defaultValue, ConfigUI ui = null)
        {
            Name = name;
            Config = configFile;
            DefaultValue = defaultValue;
            values = list;
            ValueType = typeof(string);
            if (Config.Data.TryGetValue(name, out _))
            {
                BoxedValue = Config.GetValue<string>(name);
            }
            else
            {
                BoxedValue = defaultValue;
            }
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
                    anchor = DefaultButton(ModOptionsScreen.instance.ButtonsParent,
                                           onDisplayAnchorText != null ? onDisplayAnchorText() : c.UI.GetName(),
                                           c.UI.GetTooltip());
                    anchor.Clicked += delegate
                    {
                        OpenMenu();
                    };
                }
            };
            configFile.Entries.Add(this);
        }

        public void AddString(string value, bool resort = false)
        {
            if (!values.Contains(value)) values.Add(value);
            if (resort) values.Sort();
        }

        private string EntryText(string entry)
        {
            if (currentValueColor != null && entry == content)
            {
                entry = ColorText(currentValueColor, entry);
            }
            return entry;
        }

        private void OpenMenu()
        {
            if (GameCanvas.instance.ModalIsOpen) return;
            ModalScreen.instance.Clear();
            popup = ModalScreen.instance;
            popup.SetTexts(popupMenuTitleText, popupMenuHelpText);
            foreach (string t in values)
            {
                string thisEntry = t; // so the delegate grabs the correct value, not the loop variable
                CustomButton btn = DefaultButton(popup.ButtonParent,
                                                 EntryText(thisEntry),
                                                 onDisplayTooltip != null ? onDisplayTooltip(thisEntry) : null);
                btn.Clicked += delegate ()
                {
                    if (onChange == null || onChange(thisEntry))
                    {
                        Config.Data[Name] = (int)(object)thisEntry;
                        content = thisEntry;
                        anchor.TextMeshPro.text = onDisplayAnchorText != null ? onDisplayAnchorText() : UI.GetName();
                        CloseMenu();
                    }
                };
            }
            if (CloseButtonText != null)
            {
                CustomButton btnClose = DefaultButton(ModalScreen.instance.ButtonParent, RightAlign(CloseButtonText));
                btnClose.Clicked += delegate ()
                {
                    CloseMenu();
                };
            }
            GameCanvas.instance.OpenModal();
        }

        public override void SetDefaults()
        {
            BoxedValue = DefaultValue;
            if (popup != null)
            {
                anchor.TextMeshPro.text = onDisplayAnchorText != null ? onDisplayAnchorText() : UI.GetName();
            }
        }
    }

}
