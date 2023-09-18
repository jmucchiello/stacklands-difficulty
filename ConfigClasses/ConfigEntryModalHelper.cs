using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyModNS
{
    // Base class for Configuration Entries that open a modal dialog.
    public abstract class ConfigEntryModalHelper : ConfigEntryHelper
    {
        protected static ModalScreen popup;

        public void CloseMenu()
        {
            GameCanvas.instance.CloseModal();
            popup = null;
        }
    }
}
