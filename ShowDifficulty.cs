using UnityEngine;
using HarmonyLib;
using TMPro;

namespace DifficultyModNS
{
    public enum InGameVisibility { Never, OnPause, Always };

    public partial class DifficultyMod : Mod
    {
        private InGameVisibility InGameVisibility = InGameVisibility.Always;
        private GameObject showDifficulty;
        private ConfigEntry<bool> configPeacemodeOption;
        private ConfigToggledEnum<InGameVisibility> configShowDifficulty;
        private GameObject ShowDifficultyBackground = null;
        private GameObject ShowDifficultyText = null;
        private GameObject ShowPeacemodeBackground = null;
        private GameObject ShowPeacemodeText = null;
        private CustomButton ShowPeacemodeBtn = null;
        private string peacemodeText;

        private bool gameIsPaused = WorldManager.instance?.IsPlaying ?? true;

        public static void GameIsPaused(bool flag)
        {
            if (instance.gameIsPaused ^ flag)
            {
                instance.gameIsPaused = flag;
                instance.ApplyShowDifficulty();
            }
        }

        public static InGameVisibility inGameVisibility { get => instance?.InGameVisibility ?? InGameVisibility.Never; }

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
            showDifficulty = UnityEngine.Object.Instantiate(rb).gameObject;
            showDifficulty.name = "ShowDifficultyBox";
            for (int i = 0; i < showDifficulty.transform.childCount; ++i)
            {
                GameObject go = showDifficulty.transform.GetChild(i).gameObject;
                if (go.name == "FoodBackground")
                {
                    ShowDifficultyBackground = go;
                    ShowDifficultyBackground.name = "ShowDifficultyBackground";
                    ShowDifficultyText = ShowDifficultyBackground.transform.GetChild(0).gameObject;
                    ShowDifficultyText.name = "ShowDifficultyText";
                }
                else if (go.name == "MoneyBackground")
                { // rip this out. Replace with TimeBackground.TimeText (Child 1)
                    ShowPeacemodeBackground = go;
                    ShowPeacemodeBackground.name = "ShowPeacemodeBackground";
                    ShowPeacemodeBtn = ShowPeacemodeBackground.AddComponent<CustomButton>();
                    ShowPeacemodeBtn.transform.localPosition = Vector3.zero;
                    ShowPeacemodeBtn.name = "ShowPeacemodeBtn";
                    ShowPeacemodeBtn.Clicked += this.PeacemodeToggle;
                    ShowPeacemodeBtn.enabled = true;
                    peacemodeText = PeacemodeText();

                    ShowPeacemodeText = ShowPeacemodeBackground.transform.GetChild(0).gameObject;
                    ShowPeacemodeText.name = "ShowPeacemodeText";
//                    ShowPeacemodeText.SetActive(false);
                }
                else go.SetActive(false);
            }
            Log($"FoodBackground {ShowDifficultyBackground} FoodText {ShowDifficultyText}");
            showDifficulty.transform.SetParent(GameScreen.instance.transform);
            showDifficulty.transform.localPosition = new Vector3(0f, rb.transform.localPosition.y, rb.transform.position.z);
            showDifficulty.transform.localScale = Vector3.one;
            showDifficulty.transform.localRotation = Quaternion.identity;

//            UnityEngine.UI.Image.Instantiate()
        }

        public void PeacemodeToggle()
        {
            Log("PeacemodeToggle clicked");
            WorldManager.instance.CurrentRunOptions.IsPeacefulMode = !WorldManager.instance.CurrentRunOptions.IsPeacefulMode;
            peacemodeText = PeacemodeText();
            ShowPeacemodeBtn.TextMeshPro.text = peacemodeText;
        }

        public string PeacemodeText()
        {
            return SokLoc.Translate("label_debug_toggle_peaceful_mode", LocParam.Create("on_off", YesNo(WorldManager.instance.CurrentRunOptions.IsPeacefulMode)));
//            return WorldManager.instance.CurrentRunOptions.IsPeacefulMode ? "Peace Mode: On" : "Peace Mode: Off";
        }

        public void ApplyShowDifficulty()
        {
            if (inGameVisibility == InGameVisibility.Always || 
                inGameVisibility == InGameVisibility.OnPause && gameIsPaused)
            {
                string stripParens = SokLoc.Translate($"difficultymod_difficulty") + " " + SokLoc.Translate($"difficultymod_config_difficulty_{(int)(object)difficulty}");
                if (stripParens.Contains(" (")) stripParens = stripParens.Substring(0, stripParens.IndexOf(" ("));
                TextMeshProUGUI mesh = ShowDifficultyText.GetComponent<TextMeshProUGUI>();
                mesh.text = stripParens;

                ShowInfoBox sib = ShowDifficultyBackground.GetComponent<ShowInfoBox>();
                sib.InfoBoxText = SokLoc.Translate($"difficultymod_config_difficulty_{(int)(object)difficulty}_tooltip").Replace("\n",". ");
                sib.InfoBoxTitle = SokLoc.Translate("difficultymod_game_difficulty");

                ShowPeacemodeBtn.TextMeshPro.text = peacemodeText;
                sib = ShowPeacemodeBackground.GetComponent<ShowInfoBox>();
                sib.InfoBoxText = SokLoc.Translate($"difficultymod_config_peacemode_tooltip").Replace("\n", ". ");
                sib.InfoBoxTitle = SokLoc.Translate("difficultymod_game_peacemode");

                showDifficulty.SetActive(true);
            }
            else showDifficulty.SetActive(false);
        }

    }

    [HarmonyPatch(typeof(GameScreen)), HarmonyPatch("Update")]
    public class NoPauseGame
    {
        private static Traverse<bool> gameSpeedButtonClicked;
        public static void Setup()
        {
            gameSpeedButtonClicked = new Traverse(GameScreen.instance).Field<bool>("gameSpeedButtonClicked");
        }

        static void Prefix()
        {
            if ((WorldManager.instance.InAnimation || GameCanvas.instance.ModalIsOpen)
                && gameSpeedButtonClicked.Value)
            {
                gameSpeedButtonClicked.Value = false;
                if (WorldManager.instance.SpeedUp == 0f)
                {
                    WorldManager.instance.SpeedUp = 1f;
                }
                else if (WorldManager.instance.SpeedUp == 1f)
                {
                    WorldManager.instance.SpeedUp = 5f;
                }
                else if (WorldManager.instance.SpeedUp == 5f)
                {
                    WorldManager.instance.SpeedUp = 1f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(WorldManager)), HarmonyPatch("Update")]
    public class ShowDifficulty
    {
        private static int dragdelay = 0;
        static void Postfix()
        {
            if (WorldManager.instance.DraggingCard) dragdelay = 5;
            else if (dragdelay > 0) --dragdelay;
            if (DifficultyMod.inGameVisibility == InGameVisibility.OnPause && 
                WorldManager.instance.IsPlaying && 
                !WorldManager.instance.DraggingCard &&
                dragdelay == 0)
            {
                DifficultyMod.GameIsPaused(GameScreen.instance.PausedText.IsActive());
            }
        }
    }
}
