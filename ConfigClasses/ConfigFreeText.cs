using UnityEngine;

namespace DifficultyModNS
{
    public class ConfigFreeText : ConfigEntryHelper
    {
        public string Text;
        public override object BoxedValue { get => new object(); set => _ = value; }

        public Action<ConfigEntryBase, CustomButton> Clicked;

        /**
         *  Create a header line in the config screen. Also useful for stuff like "Close" in ModalScreen
         **/
        public ConfigFreeText(string name, ConfigFile config, string text, string tooltip = null)
        {
            Name = name;
            Config = config;
            ValueType = typeof(object);
            SokTerm term = SokLoc.instance.CurrentLocSet.GetTerm(text ?? name) ?? SokLoc.FallbackSet.GetTerm(text ?? name);
            Text = term?.GetText() ?? text;
            UI = new ConfigUI()
            {
                Hidden = true,
                OnUI = delegate (ConfigEntryBase c)
                {
                    CustomButton btn = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab, ModOptionsScreen.instance.ButtonsParent);
                    btn.transform.localScale = Vector3.one;
                    btn.transform.localPosition = Vector3.zero;
                    btn.transform.localRotation = Quaternion.identity;
                    btn.TextMeshPro.text = CenterAlign(Text);
                    btn.TooltipText = tooltip;
                    btn.EnableUnderline = false;
                    btn.Clicked += delegate ()
                    {
                        Clicked?.Invoke(this, btn);
                    };
                }
            };
            config.Entries.Add(this);
        }
    }
}
