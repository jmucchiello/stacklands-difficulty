using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyModNS
{
    public abstract class ConfigEntryHelper : ConfigEntryBase
    {
        public virtual void SetDefaults() { }

        public static string CenterAlign(string txt)
        {
            return "<align=center>" + txt + "</align>";
        }

        public static string RightAlign(string txt)
        {
            return "<align=right>" + txt + "</align>";
        }

        public static string ColorText(string color, string txt)
        {
            return $"<color={color}>" + txt + "</color>";
        }

        public static string ColorText(Color color, string txt)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>" + txt + "</color>";
        }

        public CustomButton DefaultButton(RectTransform parent, string text, string tooltip = null)
        {
            CustomButton btn = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab);
            btn.transform.SetParent(parent);
            btn.transform.localPosition = Vector3.zero;
            btn.transform.localScale = Vector3.one;
            btn.transform.localRotation = Quaternion.identity;
            btn.TextMeshPro.text = text;
            btn.TooltipText = tooltip;
            return btn;
        }
    }

    public class ConfigEmtySpace : ConfigEntryBase
    {
        private RectTransform spacer1, spacer2;
        public override object BoxedValue { get => new object(); set => _ = value; }

        public ConfigEmtySpace(ConfigFile Config)
        {
            Name = "none";
            ValueType = typeof(object);
            Config.Entries.Add(this);
            UI = new ConfigUI()
            {
                Hidden = true,
                OnUI = delegate {
                    spacer1 = UnityEngine.Object.Instantiate(ModOptionsScreen.instance.SpacerPrefab, ModOptionsScreen.instance.ButtonsParent);
                    spacer2 = UnityEngine.Object.Instantiate(ModOptionsScreen.instance.SpacerPrefab, ModOptionsScreen.instance.ButtonsParent);
                }
            };
        }

    }
}
