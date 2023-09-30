using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using UnityEngine;
using UnityEngine.UI;

namespace DifficultyModNS
{

    public class ConfigSlider : ConfigEntryHelper
    {
        public delegate string SetSliderText(int value);
        public SetSliderText setSliderText;
        public SetSliderText setSliderTooltip;
        public string Text;
        public override object BoxedValue { get => Value; set => Value = (int)value; }

        public int Value { get; set; }
        private int DefaultValue;

        public Action<int> onChange;

        public GameObject Slider = null;
        GameObject SliderFill = null;
        GameObject SliderText = null;
        Image SliderImage = null;
        CustomButton SliderBtn = null;
        RectTransform SliderRectTransform = null;
        Vector2 SliderSize = Vector2.zero;
        bool ParentIsPopup = false;

        int low = 0, high = 1, step = 1;
        int span = 0;

        public static (T,T) Swap<T>(T a, T b) { return (b, a); }
        public static void Swap<T>(ref T a, ref T b) { T c = a; a = b; b = c; }
        /**
         *  Create a header line in the config screen. Also useful for stuff like "Close" in ModalScreen
         **/
        public ConfigSlider(string name, ConfigFile config, string text, Action<int> OnChange, int low, int high, int step = 1, int defValue = 0, bool parentIsPopup = false)
        {
            Name = name;
            Config = config;
            ValueType = typeof(int);
            onChange = OnChange;
            DifficultyMod.Log($"{name} {parentIsPopup}");
            ParentIsPopup = parentIsPopup;
            if (low > high) (low, high) = Swap(low, high);
            this.low = low;
            this.high = high;
            this.step = step;
            span = high - low + step;

            DefaultValue = defValue;
            Value = LoadConfigEntry<int>(name, defValue);

            SokTerm term = SokLoc.instance.CurrentLocSet.GetTerm(text ?? name) ?? SokLoc.FallbackSet.GetTerm(text ?? name);
            Text = term?.GetText() ?? text ?? name;
            UI = new ConfigUI()
            {
                Hidden = true,
                OnUI = delegate (ConfigEntryBase c)
                {
                    try
                    {
                        Transform tb = GameScreen.instance.transform.Find("TimeBackground");
                        DifficultyMod.Log($"{Name} {ParentIsPopup} {tb}");
                        Transform x = UnityEngine.Object.Instantiate(tb);
                        DifficultyMod.Log($"Slider {x == null}");
                        Slider = x?.gameObject;
                        DifficultyMod.Log($"Slider {Slider == null}");
                        x.SetParentClean(parentIsPopup ? I.Modal.ButtonParent : I.MOS.ButtonsParent);
                        Slider.name = "SliderBackground" + Name;
                        DifficultyMod.Log($"Slider {Slider}");
                        for (int i = 0; i < Slider.transform.childCount; ++i)
                        {
                            GameObject goChild = Slider.transform.GetChild(i).gameObject;
                            if (goChild.name == "SpeedIcon") goChild.SetActive(false);
                            if (goChild.name == "TimeFill") SliderFill = goChild;
                            if (goChild.name == "TimeText") SliderText = goChild;
                        }
                    }
                    catch (Exception ex)
                    {
                        DifficultyMod.Log(ex.ToString());
                    }

                    LayoutElement layout = Slider.GetComponent<LayoutElement>();
                    layout.preferredHeight = -1; // original component has a fixed height

                    SliderRectTransform = Slider.GetComponent<RectTransform>();
                    SliderRectTransform.localScale = Vector3.one;
                    SliderRectTransform.localPosition = Vector3.zero;
                    SliderSize = SliderRectTransform.offsetMax - SliderRectTransform.offsetMin;
#pragma warning disable 8602
                    SliderText.name = "SliderText" + Name;
                    SliderFill.name = "SliderFill" + Name;
#pragma warning restore 8602
                    SliderFill.transform.localScale = Vector3.one;
                    SliderImage = SliderFill.GetComponent<Image>();
                    SliderBtn = Slider.GetComponent<CustomButton>();
                    SliderBtn.name = "SliderButton" + Name;
                    SliderBtn.transform.localScale = Vector3.one;
                    SliderBtn.Clicked += () =>
                    {
                        SliderRectTransform = Slider.GetComponent<RectTransform>();
                        SliderRectTransform.localScale = Vector3.one;
                        SliderRectTransform.localPosition = Vector3.zero;
                        SliderSize = SliderRectTransform.offsetMax - SliderRectTransform.offsetMin;
                        Vector2 pos = InputController.instance.ClampedMousePosition();
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(SliderRectTransform, pos, null, out Vector2 newpos);
                        Vector2 tmp = newpos + SliderSize;
                        float value = tmp.x / SliderSize.x;
                        int OldValue = Value;
                        Value = Math.Clamp((int)(span * value + step / 2) / step * step + low, low, high);
                        SetSlider();
                        if (OldValue != Value) onChange?.Invoke(Value);
                        DifficultyMod.Log($"test.clicked called {SliderSize} {pos} {newpos} {value} {Value}");
                    };
                    SetSlider();
                }
            };
            config.Entries.Add(this);
        }

        public void SetSlider()
        {
            SliderImage.fillAmount = (float)(Value - low) / (float)(high - low);
            string btnText = setSliderText?.Invoke(Value) ?? Text + " <color=blue>" + Value.ToString() + "</color>";
            SliderBtn.TextMeshPro.text = btnText;
            SliderBtn.TooltipText = setSliderTooltip?.Invoke(Value);
            DifficultyMod.Log($"Fill Amount {low} {high} {Value} {SliderImage.fillAmount}");
            Config.Data[Name] = Value;
        }

        public override void SetDefaults()
        {
            bool change = Value != DefaultValue;
            Value = DefaultValue;
            SetSlider();
            if (change) onChange?.Invoke(Value);
        }
        
    }
}
