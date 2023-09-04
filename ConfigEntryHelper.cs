using System;
using System.Collections.Generic;
using System.Text;

namespace DifficultyModNS
{
    public abstract class ConfigEntryHelper : ConfigEntryBase
    {
        public abstract void SetDefaults();

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
    }
}
