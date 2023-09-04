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

    internal class ConfigFreeText : ConfigEntryBase
    {
        private string Text;
        public override object BoxedValue { get => new object(); set => _ = value; }

        public ConfigFreeText(string name, ConfigFile config, string text)
        {
            Name = name;
            Config = config;
            ValueType = typeof(object);
            SokTerm term = SokLoc.instance.CurrentLocSet.GetTerm(text);
            if (term == null) term = SokLoc.FallbackSet.GetTerm(text);
            Text = term == null ? text : term.GetText();
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
                }
            };
            config.Entries.Add(this);
        }
    }


    public class ConfigEnum<T> : ConfigEntryHelper where T : Enum
    {
        private T content;
        private CustomButton anchor;

        public delegate string OnDisplayText();
        public delegate string OnDisplayEnum(T t);
        public delegate string OnDisplayTooltip(T t);
        public OnDisplayText onDisplayText;
        public OnDisplayEnum onDisplayEnum;
        public OnDisplayTooltip onDisplayTooltip;

        public delegate bool OnChange(T c);
        public OnChange onChange;

        public string popupText;
        public string popupTooltip;

        public T DefaultValue { get { T[] t = (T[])Enum.GetValues(typeof(T)); return t[0]; } }

        public override object BoxedValue { get => content;
            set
            {
                if (value is int)
                {
                    T[] a = (T[])Enum.GetValues(typeof(T));
                    content = a[(int)value];
                }
                else if (value is T)
                {
                    content = (T)value;
                }
            }
        }

        public ConfigEnum(string name, ConfigFile configFile, T defaultValue, ConfigUI ui = null)
        {
            Name = name;
            ValueType = typeof(System.Object);
            Config = configFile;
            if (Config.Data.TryGetValue(name, out _))
            {
                BoxedValue = Config.GetEntry<int>(name);
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
                    DifficultyMod.Log($"UI.Hidden = {UI.Hidden}");
                    anchor = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab, ModOptionsScreen.instance.ButtonsParent);
                    anchor.transform.localScale = Vector3.one;
                    anchor.transform.localPosition = Vector3.zero;
                    anchor.transform.localRotation = Quaternion.identity;
                    anchor.TextMeshPro.text = onDisplayText != null? onDisplayText() : c.UI.GetName();
                    anchor.TooltipText = c.UI.GetTooltip();
                    anchor.Clicked += delegate
                    {
                        OpenMenu();
                    };
                }
            };
            configFile.Entries.Add(this);
        }

        private void OpenMenu()
        {
            if (GameCanvas.instance.ModalIsOpen) return;
            ModalScreen.instance.Clear();
            popup = ModalScreen.instance;
            popup.SetTexts(popupText, popupTooltip);
            foreach (T t in Enum.GetValues(typeof(T)))
            {
                T thisEntry = t;
                string text = onDisplayText != null ? onDisplayEnum(thisEntry) : Enum.GetName(typeof(T), thisEntry);
                if (thisEntry.Equals(content)) text = ColorText("blue", text);
                CustomButton btn = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab);
                btn.transform.SetParent(ModalScreen.instance.ButtonParent);
                btn.transform.localPosition = Vector3.zero;
                btn.transform.localScale = Vector3.one;
                btn.transform.localRotation = Quaternion.identity;
                btn.TextMeshPro.text = text;
                btn.TooltipText = onDisplayTooltip != null ? onDisplayTooltip(thisEntry) : null;
                btn.Clicked += delegate ()
                {
                    if (onChange != null && onChange(thisEntry) || onChange == null)
                    {
                        Config.Data[Name] = (int)(object)thisEntry;
                        content = thisEntry;
                        anchor.TextMeshPro.text = onDisplayText != null ? onDisplayText() : UI.GetName();
                        CloseMenu();
                    }
                };
            }
            GameCanvas.instance.OpenModal();
        }

        public override void SetDefaults()
        {
            BoxedValue = DefaultValue;
            if (popup != null)
            {

            }
        }
    }




#if false

    [HarmonyPatch(typeof(ModOptionsScreen), "OnEnable")]
    internal class MOSpatch
    {
        static void Postfix(ModOptionsScreen __instance)
        {

        }

        static int dayToCheck = 3;

        void test(CardBag currentCardBag)
        {
            Location BoosterLocation = Location.Mainland;
            bool IsIntroPack = false;
            int timesBoosterWasBoughtOnLocation = 4;
            CardId cardId;
            GameBoard MyBoard = WorldManager.instance.CurrentBoard;

            if (WorldManager.instance.GetBoardWithLocation(BoosterLocation).BoardOptions.NewVillagerSpawnsFromPack)
            {
                int cardCount = WorldManager.instance.GetCardCount((BaseVillager x) => x.CanBreed);
                cardCount += WorldManager.instance.GetCardCount<TeenageVillager>();
                if (!IsIntroPack && (timesBoosterWasBoughtOnLocation == 7 || (timesBoosterWasBoughtOnLocation > 7 && timesBoosterWasBoughtOnLocation % 5 == 0)) && currentCardBag.CardsInPack == 0 && cardCount <= 1)
                {
                    cardId = (CardId)"villager";
                }
                if (MyBoard.BoardOptions.CanSpawnCombatIntro && timesBoosterWasBoughtOnLocation >= 10 && !WorldManager.instance.CurrentSave.FoundBoosterIds.Contains("combat_intro") && currentCardBag.CardsInPack == 0)
                {
                    WorldManager.instance.CreateBoosterpack(Vector3.zero, "combat_intro").SendIt();
                }
            }
        }

        void test2(CardBag currentCardBag)
        {
            Location BoosterLocation = Location.Mainland;
            bool IsIntroPack = false;
            int timesBoosterWasBoughtOnLocation = 4;
            CardId cardId;
            GameBoard MyBoard = WorldManager.instance.CurrentBoard;

            if (WorldManager.instance.GetBoardWithLocation(BoosterLocation).BoardOptions.NewVillagerSpawnsFromPack)
            {
                int cardCount = WorldManager.instance.GetCardCount((BaseVillager x) => x.CanBreed);
                cardCount += WorldManager.instance.GetCardCount<TeenageVillager>();
                if (!IsIntroPack && (timesBoosterWasBoughtOnLocation == 7 || (timesBoosterWasBoughtOnLocation > 7 && timesBoosterWasBoughtOnLocation % dayToCheck == 0)) && currentCardBag.CardsInPack == 0 && cardCount <= 1)
                {
                    cardId = (CardId)"villager";
                }
                if (MyBoard.BoardOptions.CanSpawnCombatIntro && timesBoosterWasBoughtOnLocation >= 10 && !WorldManager.instance.CurrentSave.FoundBoosterIds.Contains("combat_intro") && currentCardBag.CardsInPack == 0)
                {
                    WorldManager.instance.CreateBoosterpack(Vector3.zero, "combat_intro").SendIt();
                }
            }
        }
    }
#endif
}
