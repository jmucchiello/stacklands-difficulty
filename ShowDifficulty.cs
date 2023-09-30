using UnityEngine;
using HarmonyLib;
using TMPro;

namespace DifficultyModNS
{
    public enum InGameVisibility { Never, OnPause, Always };

    public partial class DifficultyMod : Mod
    {
        private GameObject ShowIngameSettingsBox;
        private ConfigToggledEnum<InGameVisibility> configShowDifficulty;
        private ConfigToggledEnum<InGameVisibility> configShowPeacemode;
        private GameObject ShowDifficultyBackground = null;
        private GameObject ShowDifficultyText = null;
        private GameObject ShowPeacemodeBackground = null;
        private CustomButton ShowPeacemodeBtn = null;

        public static bool ShowDifficultyBox => DifficultyBoxVisibility == InGameVisibility.Always ||
                                               (DifficultyBoxVisibility == InGameVisibility.OnPause && gameIsPaused);
        public static bool ShowPeacemodeBox => PeacemodeBoxVisibility == InGameVisibility.Always ||
                                               (PeacemodeBoxVisibility == InGameVisibility.OnPause && gameIsPaused);

        private static bool gameIsPaused = I.WM?.IsPlaying ?? true;

        public static void GameIsPaused(bool flag)
        {
            if (gameIsPaused ^ flag)
            {
                gameIsPaused = flag;
                instance.ApplyShowDifficulty();
            }
        }

        public static InGameVisibility DifficultyBoxVisibility { get => instance?.configShowDifficulty.Value ?? InGameVisibility.Never; private set => instance.configShowDifficulty.Value = value; }
        public static InGameVisibility PeacemodeBoxVisibility { get => instance?.configShowPeacemode.Value ?? InGameVisibility.Never; private set => instance.configShowPeacemode.Value = value; }

        private string ShowDifficultyVisibilityText()
        {
            return SokLoc.Translate("difficultymod_showdifficulty") + ": <color=blue>" +  SokLoc.Translate($"difficultymod_showdifficulty_{DifficultyBoxVisibility}") + "</color>";
        }

        public class SmallerToggledEnum : ConfigToggledEnum<InGameVisibility>
        {
            public SmallerToggledEnum(string name, ConfigFile configFile, InGameVisibility defaultValue, ConfigUI ui = null, bool parentIsPopup = false) 
                : base(name, configFile, defaultValue, ui, parentIsPopup)
            {
                currentValueColor = Color.blue;
            }
        }

        public void ConfigShowDifficulty()
        {
            configShowDifficulty = new ConfigToggledEnum<InGameVisibility>("difficultymod_showdifficulty", Config, InGameVisibility.Always, new ConfigUI()
            {
                NameTerm = "difficultymod_showdifficulty",
            });
            configShowDifficulty.FontSize = 25;
            configShowDifficulty.currentValueColor = Color.blue;
            configShowDifficulty.onDisplayTooltip = delegate ()
            {
                string term = $"difficultymod_showdifficulty_{configShowDifficulty.Value}_tooltip";
                return SokLoc.Translate(term);
            };
            configShowDifficulty.onDisplayEnumText = delegate (InGameVisibility v)
            {
                string term = $"difficultymod_visibility_{(InGameVisibility)v}";
                return SokLoc.Translate(term);
            };
            configShowDifficulty.onChange = delegate (InGameVisibility v)
            {
                DifficultyBoxVisibility = configShowDifficulty.Value;
                return true;
            };

            configShowPeacemode = new ConfigToggledEnum<InGameVisibility>("difficultymod_showpeacemode", Config, InGameVisibility.Never, new ConfigUI()
            {
                NameTerm = "difficultymod_showpeacemode"
            });
            configShowPeacemode.FontSize = 25;
            configShowPeacemode.currentValueColor = Color.blue;
            configShowPeacemode.onDisplayTooltip = delegate ()
            {
                string term = $"difficultymod_showpeacemode_{configShowPeacemode.Value}_tooltip";
                return SokLoc.Translate(term);
            };
            configShowPeacemode.onDisplayEnumText = delegate (InGameVisibility v)
            {
                string term = $"difficultymod_visibility_{(InGameVisibility)v}";
                return SokLoc.Translate(term);
            };
            configShowPeacemode.onChange = delegate (InGameVisibility v)
            {
                PeacemodeBoxVisibility = configShowPeacemode.Value;
                return true;
            };
        }

        public void SetupShowDifficulty()
        {
            Transform rb = GameScreen.instance.transform.Find("ResourcesBackground");
            ShowIngameSettingsBox = UnityEngine.Object.Instantiate(rb).gameObject;
            ShowIngameSettingsBox.name = "ShowIngameSettingsBox";
            for (int i = 0; i < ShowIngameSettingsBox.transform.childCount; ++i)
            {
                GameObject go = ShowIngameSettingsBox.transform.GetChild(i).gameObject;
                if (go.name == "FoodBackground")
                {
                    ShowDifficultyBackground = go;
                    ShowDifficultyBackground.name = "ShowDifficultyBackground";
                    ShowDifficultyText = ShowDifficultyBackground.transform.GetChild(0).gameObject;
                    ShowDifficultyText.name = "ShowDifficultyText";
                }
                else if (go.name == "MoneyBackground")
                {
                    ShowPeacemodeBackground = go;
                    ShowPeacemodeBackground.name = "ShowPeacemodeBackground";
                    ShowPeacemodeBtn = ShowPeacemodeBackground.AddComponent<CustomButton>();
                    ShowPeacemodeBtn.transform.localPosition = Vector3.zero;
                    ShowPeacemodeBtn.name = "ShowPeacemodeBtn";
                    ShowPeacemodeBtn.Clicked += PeacemodeToggleClicked;
                    ShowPeacemodeBtn.enabled = true;
                    UpdatePeacemodeText();
                }
                else go.SetActive(false);
            }
            ShowIngameSettingsBox.transform.SetParent(GameScreen.instance.transform);
            ShowIngameSettingsBox.transform.localPosition = new Vector3(0f, rb.transform.localPosition.y, rb.transform.position.z);
            ShowIngameSettingsBox.transform.localScale = Vector3.one;
            ShowIngameSettingsBox.transform.localRotation = Quaternion.identity;
        }

        public void PeacemodeToggleClicked()
        {
            Log("PeacemodeToggle clicked");
            I.WM.CurrentRunOptions.IsPeacefulMode = !I.WM.CurrentRunOptions.IsPeacefulMode;
            UpdatePeacemodeText();
        }

        public void UpdatePeacemodeText()
        {
            string peacemodeText = SokLoc.Translate("label_debug_toggle_peaceful_mode", LocParam.Create("on_off", YesNo(I.WM.CurrentRunOptions.IsPeacefulMode)));
            ShowPeacemodeBtn.TextMeshPro.text = peacemodeText;
        }

        public void ApplyShowDifficulty()
        {
            if (ShowDifficultyBox)
            {
                string stripParens = SokLoc.Translate($"difficultymod_difficulty") + " " + SokLoc.Translate($"difficultymod_config_difficulty_{(int)(object)difficulty}");
                if (stripParens.Contains(" (")) stripParens = stripParens.Substring(0, stripParens.IndexOf(" ("));
                TextMeshProUGUI mesh = ShowDifficultyText.GetComponent<TextMeshProUGUI>();
                if (mesh != null) mesh.text = stripParens;

                ShowInfoBox sib = ShowDifficultyBackground.GetComponent<ShowInfoBox>();
                sib.InfoBoxText = SokLoc.Translate($"difficultymod_config_difficulty_{(int)(object)difficulty}_tooltip").Replace("\n", ". ");
                sib.InfoBoxTitle = SokLoc.Translate("difficultymod_game_difficulty");

                ShowDifficultyBackground.SetActive(true);
            }
            else ShowDifficultyBackground.SetActive(false);

            if (ShowPeacemodeBox)
            {
                UpdatePeacemodeText();
                ShowInfoBox sib = ShowPeacemodeBackground.GetComponent<ShowInfoBox>();
                sib.InfoBoxText = SokLoc.Translate($"difficultymod_config_peacemode_tooltip").Replace("\n", ". ");
                sib.InfoBoxTitle = SokLoc.Translate("difficultymod_game_peacemode");
                ShowPeacemodeBtn.gameObject.SetActive(true);
            }
            else ShowPeacemodeBtn.gameObject.SetActive(false);

            ShowIngameSettingsBox.SetActive(ShowDifficultyBox || ShowPeacemodeBox);
        }
    }

    [HarmonyPatch(typeof(WorldManager)), HarmonyPatch("Update")]
    public class ShowDifficulty_Patch
    {
        private static int dragdelay = 0;
        static void Postfix()
        {
            if (I.WM.DraggingCard) dragdelay = 5;
            else if (dragdelay > 0) --dragdelay;
            if (DifficultyMod.DifficultyBoxVisibility == InGameVisibility.OnPause && 
                I.WM.IsPlaying && 
                !I.WM.DraggingCard &&
                dragdelay == 0)
            {
                DifficultyMod.GameIsPaused(GameScreen.instance.PausedText.IsActive());
            }
        }
    }

    //[HarmonyPatch(typeof(GameScreen)), HarmonyPatch("Update")]
    public class NoPauseGame_Patch
    {
        private static Traverse<bool> gameSpeedButtonClicked;
        public static void Setup()
        {
            gameSpeedButtonClicked = new Traverse(GameScreen.instance).Field<bool>("gameSpeedButtonClicked");
        }

        static void Prefix()
        {
            if ((I.WM.InAnimation || GameCanvas.instance.ModalIsOpen)
                && gameSpeedButtonClicked.Value)
            {
                gameSpeedButtonClicked.Value = false;
                if (I.WM.SpeedUp == 0f)
                {
                    I.WM.SpeedUp = 1f;
                }
                else if (I.WM.SpeedUp == 1f)
                {
                    I.WM.SpeedUp = 5f;
                }
                else if (I.WM.SpeedUp == 5f)
                {
                    I.WM.SpeedUp = 1f;
                }
            }
        }
    }
}
