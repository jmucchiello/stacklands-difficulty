using UnityEngine;
using HarmonyLib;
using TMPro;

namespace DifficultyModNS
{
    public enum InGameVisibility { Never, OnPause, Always };

    public partial class DifficultyMod : Mod
    {
        private GameObject ShowIngameSettingsBox;
        private ConfigEntry<bool> configPeacemodeOption;
        private ConfigToggledEnum<InGameVisibility> configShowDifficulty;
        private GameObject ShowDifficultyBackground = null;
        private GameObject ShowDifficultyText = null;
        private GameObject ShowPeacemodeBackground = null;
        private GameObject ShowPeacemodeText = null;
        private CustomButton ShowPeacemodeBtn = null;

        public static bool AllowPeacemodeToggle => instance.configPeacemodeOption.Value;

        private bool gameIsPaused = I.WM?.IsPlaying ?? true;

        public static void GameIsPaused(bool flag)
        {
            if (instance.gameIsPaused ^ flag)
            {
                instance.gameIsPaused = flag;
                instance.ApplyShowDifficulty();
            }
        }

        public static InGameVisibility InGameVisibility { get => instance?.configShowDifficulty.Value ?? InGameVisibility.Never; private set => instance.configShowDifficulty.Value = value; }

        private string ShowDifficultyVisibilityText()
        {
            return SokLoc.Translate("difficultymod_showdifficulty") + ": <color=blue>" +  SokLoc.Translate($"difficultymod_showdifficulty_{InGameVisibility}") + "</color>";
        }

        public void ConfigShowDifficulty()
        {
            configShowDifficulty = new ConfigToggledEnum<InGameVisibility>("difficultymod_showdifficulty", Config, InGameVisibility.Always, new ConfigUI()
            {
                NameTerm = "difficultymod_showdifficulty",
            });
            configShowDifficulty.currentValueColor = Color.blue;
            configShowDifficulty.onDisplayTooltip = delegate ()
            {
                string term = $"difficultymod_showdifficulty_{configShowDifficulty.Value}_tooltip";
                return SokLoc.Translate(term);
            };
            configShowDifficulty.onDisplayEnumText = delegate (InGameVisibility v)
            {
                string term = $"difficultymod_showdifficulty_{(InGameVisibility)v}";
                return SokLoc.Translate(term);
            };
            configShowDifficulty.onChange = delegate (InGameVisibility v)
            {
                InGameVisibility = configShowDifficulty.Value;
                return true;
            };

            configPeacemodeOption = new ConfigEntry<bool>("difficultymod_showpeacemode", Config, true, new ConfigUI()
            {
                NameTerm = "difficultymod_showpeacemode"
            });
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

                    ShowPeacemodeText = ShowPeacemodeBackground.transform.GetChild(0).gameObject;
                    ShowPeacemodeText.name = "ShowPeacemodeText";
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
            bool allowDifficulty = InGameVisibility == InGameVisibility.Always ||
                                   (InGameVisibility == InGameVisibility.OnPause && gameIsPaused);
            if (allowDifficulty)
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

            if (AllowPeacemodeToggle)
            {
                UpdatePeacemodeText();
                ShowInfoBox sib = ShowPeacemodeBackground.GetComponent<ShowInfoBox>();
                sib.InfoBoxText = SokLoc.Translate($"difficultymod_config_peacemode_tooltip").Replace("\n", ". ");
                sib.InfoBoxTitle = SokLoc.Translate("difficultymod_game_peacemode");
                ShowPeacemodeBtn.gameObject.SetActive(true);
            }
            else ShowPeacemodeBtn.gameObject.SetActive(false);

            ShowIngameSettingsBox.SetActive(AllowPeacemodeToggle || allowDifficulty);
//            Log($"ApplyShowDifficulty {InGameVisibility} {gameIsPaused} {ShowDifficultyBackground.activeSelf} {ShowPeacemodeBtn.gameObject.activeSelf} {ShowIngameSettingsBox.activeSelf}");}
        }
    }

    [HarmonyPatch(typeof(WorldManager)), HarmonyPatch("Update")]
    public class ShowDifficulty
    {
        private static int dragdelay = 0;
        static void Postfix()
        {
            if (I.WM.DraggingCard) dragdelay = 5;
            else if (dragdelay > 0) --dragdelay;
            if (DifficultyMod.InGameVisibility == InGameVisibility.OnPause && 
                I.WM.IsPlaying && 
                !I.WM.DraggingCard &&
                dragdelay == 0)
            {
                DifficultyMod.GameIsPaused(GameScreen.instance.PausedText.IsActive());
            }
        }
    }

    //[HarmonyPatch(typeof(GameScreen)), HarmonyPatch("Update")]
    public class NoPauseGame
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
