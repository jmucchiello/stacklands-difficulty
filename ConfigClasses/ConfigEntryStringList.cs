using UnityEngine;

namespace DifficultyModNS
{
    public class ConfigEntryStringList : ConfigEntryModalHelper 
    {
        private string content; // access via BoxedValue
        private CustomButton anchor;  // this holds the ModOptionsScreen text that is clicked to open the menu
        private List<string> values;

        public delegate string OnDisplayText();
        public delegate string OnDisplayTooltip(string entry);
        public OnDisplayText onDisplayAnchorText;  // the text seen in the main option screen
        public OnDisplayTooltip onDisplayTooltip;  // the text seen in the main option screen

        public delegate bool OnChange(string newValue); // return false to prevent acceptance of newValue
        public OnChange onChange;

        public string popupMenuTitleText; // the title bar text of the popup screen
        public string popupMenuHelpText; // the help text that appears below the title bar text

        public string CloseButtonText = null; // if null, no close button is created
        public Color currentValueColor = Color.black;

        public virtual string DefaultValue { get; private set; }

        public override object BoxedValue
        {
            get => content; set => content = value.ToString();
        }

        public ConfigEntryStringList(string name, ConfigFile configFile, List<string> list, string defaultValue, ConfigUI ui = null)
        {
            Name = name;
            Config = configFile;
            DefaultValue = defaultValue;
            values = list;
            ValueType = typeof(string);
            if (Config.Data.TryGetValue(name, out _))
            {
                BoxedValue = Config.GetValue<string>(name);
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
                    anchor = DefaultButton(ModOptionsScreen.instance.ButtonsParent,
                                           onDisplayAnchorText != null ? onDisplayAnchorText() : c.UI.GetName(),
                                           c.UI.GetTooltip());
                    anchor.Clicked += delegate
                    {
                        OpenMenu();
                    };
                }
            };
            configFile.Entries.Add(this);
        }

        public void AddString(string value, bool resort = false)
        {
            if (!values.Contains(value)) values.Add(value);
            if (resort) values.Sort();
        }

        private string EntryText(string entry)
        {
            if (currentValueColor != null && entry == content)
            {
                entry = ColorText(currentValueColor, entry);
            }
            return entry;
        }

        private void OpenMenu()
        {
            if (GameCanvas.instance.ModalIsOpen) return;
            ModalScreen.instance.Clear();
            popup = ModalScreen.instance;
            popup.SetTexts(popupMenuTitleText, popupMenuHelpText);
            foreach (string t in values)
            {
                string thisEntry = t; // so the delegate grabs the correct value, not the loop variable
                CustomButton btn = DefaultButton(popup.ButtonParent,
                                                 EntryText(thisEntry),
                                                 onDisplayTooltip != null ? onDisplayTooltip(thisEntry) : null);
                btn.Clicked += delegate ()
                {
                    if (onChange == null || onChange(thisEntry))
                    {
                        Config.Data[Name] = (int)(object)thisEntry;
                        content = thisEntry;
                        anchor.TextMeshPro.text = onDisplayAnchorText != null ? onDisplayAnchorText() : UI.GetName();
                        CloseMenu();
                    }
                };
            }
            if (CloseButtonText != null)
            {
                CustomButton btnClose = DefaultButton(ModalScreen.instance.ButtonParent, RightAlign(CloseButtonText));
                btnClose.Clicked += delegate ()
                {
                    CloseMenu();
                };
            }
            GameCanvas.instance.OpenModal();
        }

        public override void SetDefaults()
        {
            BoxedValue = DefaultValue;
            if (popup != null)
            {
                anchor.TextMeshPro.text = onDisplayAnchorText != null ? onDisplayAnchorText() : UI.GetName();
            }
        }
    }

}
