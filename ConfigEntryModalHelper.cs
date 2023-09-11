using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyModNS
{
    public abstract class ConfigEntryModalHelper : ConfigEntryBase
    {
        public virtual void SetDefaults() { }

        protected static ModalScreen popup;

        public void CloseMenu()
        {
            GameCanvas.instance.CloseModal();
            popup = null;
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
}
