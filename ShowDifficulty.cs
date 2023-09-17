using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyModNS
{
    public partial class DifficultyMod : Mod
    {
        private GameObject showDifficulty;
        private CustomButton DifficultyBtn;
        private ConfigEntry<bool> configShowDifficulty;
        private GameObject FoodBackground = null;
        private GameObject FoodText = null;

        public void SetupShowDifficulty()
        {
            Transform rb = GameScreen.instance.transform.Find("ResourcesBackground");
            showDifficulty = UnityEngine.Object.Instantiate(rb).gameObject;
            Log($"test {showDifficulty}");
            for (int i = 0; i < showDifficulty.transform.childCount; ++i)
            {
                GameObject go = showDifficulty.transform.GetChild(i).gameObject;
                if (go.name == "FoodBackground")
                {
                    FoodBackground = go;
                    FoodText = go.GetComponentInChildren<GameObject>();
                }
                else go.SetActive(false);
            }
            Log($"FoodBackground {FoodBackground} FoodText {FoodText}");
            showDifficulty.transform.SetParent(GameScreen.instance.transform);
            showDifficulty.transform.localPosition = new Vector3(0f, rb?.transform.localPosition.y ?? 500f, -0.0001f);
            showDifficulty.transform.localScale = Vector3.one;
            showDifficulty.transform.localRotation = Quaternion.identity;
        }

        public void ApplyShowDifficulty()
        {
            if (configShowDifficulty.Value)
            {
                string stripParens = SokLoc.Translate($"difficultymod_difficulty") + " " + SokLoc.Translate($"difficultymod_config_difficulty_{(int)(object)difficulty}");
                if (stripParens.Contains(" (")) stripParens = stripParens.Substring(0, stripParens.IndexOf(" ("));
                FoodBackground..transform.s.text = "<size=80%>" + stripParens + "</size>";
                DifficultyBtn.TooltipText = SokLoc.Translate($"difficultymod_config_difficulty_{(int)(object)difficulty}_tooltip");
            }
            showDifficulty.SetActive(configShowDifficulty.Value);
        }

    }
}
