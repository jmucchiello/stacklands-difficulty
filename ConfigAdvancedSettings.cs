using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyModNS
{
    public partial class DifficultyMod : Mod
    {
        public ConfigAdvancedSettings AdvancedSettings;
        public static int portalFrequency => instance.AdvancedSettings.portalFrequency.Value;
    }

    public class ConfigAdvancedSettings : ConfigEntryModalHelper
    {
        public override object BoxedValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ConfigSlider portalFrequency;

        public ConfigAdvancedSettings(string name, ConfigFile config)
        {
            Name = name;
            Config = config;
            ValueType = typeof(object);
            UI = new ConfigUI()
            {
                Hidden = true,
                OnUI = delegate (ConfigEntryBase c) {
                    AnchorButton = DefaultButton(I.MOS.ButtonsParent, GetAnchorText(), GetAnchorTooltip());
                    AnchorButton.Clicked += delegate ()
                    {
                        OpenMenu();
                    };
                }
            };
            Config.Entries.Add(this);
        }

        public void OpenMenu()
        {
            if (GameCanvas.instance.ModalIsOpen) return;
            ModalScreen.instance.Clear();
            popup = ModalScreen.instance;
            popup.SetTexts(I.Xlat("difficultymod_advanced_menu_text"),
                           I.Xlat("difficultymod_advanced_menu_tooltip"));
            GameCanvas.instance.OpenModal();

            popup.AddOption(RightAlign(SokLoc.Translate("difficultymod_closemenu")), CloseMenu);
            popup.AddOption(RightAlign(SokLoc.Translate("difficultymod_reset_defaults")), SetDefaults);
            DifficultyMod.Log("1");

            portalFrequency = new ConfigSlider("difficultymod_advanced_portalfrequency", Config, GetPortalFrequencyText(), null, 1, 8, 1, 4, true);
            DifficultyMod.Log($"2 {portalFrequency}");
            DifficultyMod.Log($"2 {portalFrequency.Slider}");
            portalFrequency.Slider.transform.SetParentClean(popup.ButtonParent);
            DifficultyMod.Log("3");

        }

        public string GetPortalFrequencyText()
        {
            return I.Xlat("difficultymod_advanced_portalfrequency");
        }

        public string GetAnchorText()
        {
            return SizeText(25, I.Xlat("difficultymod_advanced_anchor"));
        }
        public string GetAnchorTooltip()
        {
            return SizeText(25, I.Xlat("difficultymod_advanced_anchor_tooltip"));
        }

        public override void SetDefaults()
        {
            portalFrequency.SetDefaults();
        }
    }
}
