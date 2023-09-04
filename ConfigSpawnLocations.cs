using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyModNS
{
    public partial class DifficultyMod : Mod
    {
        public ConfigSpawnSites configSpawnSites;

        public float Xconfine => UnityEngine.Random.Range(lowX, highX);
        public float Zconfine => UnityEngine.Random.Range(lowZ, highZ);

        private float lowX, highX;
        private float lowZ, highZ;
        public void SetupSpawnSites()
        {
            switch (configSpawnSites.BoxedValue)
            {
                case SpawnSites.Anywhere: lowX = lowZ = 0f; highX = highZ = 1f; break;
                case SpawnSites.Center: lowX = lowZ = 0.4f; highX = highZ = 0.6f; break;
                case SpawnSites.UpperLeft: lowX = lowZ = 0f; highX = highZ = 0.2f; break;
                case SpawnSites.UpperRight: lowX = 0.8f; lowZ = 0f; highX = 1f;  highZ = 0.2f; break;
                case SpawnSites.LowerLeft: lowX = 0f; lowZ = 0.8f; highX = 0.2f;  highZ = 1f; break;
                case SpawnSites.LowerRight: lowX = lowZ = 0.8f; highX = highZ = 1f; break;
            }
            Log($"Spawn Location Ranges: X({lowX} to {highX}), Y({lowZ} to {highZ})");
        }

        public string YesNo(bool b) { return b ? "Yes" : "No"; }
    }

    public enum SpawnSites
    {
        Anywhere, Center, UpperLeft, UpperRight, LowerLeft, LowerRight
    };

    public class ConfigSpawnSites : ConfigEnum<SpawnSites>
    {
        public ConfigSpawnSites(string name, ConfigFile configFile, SpawnSites defaultValue, ConfigUI ui = null)
            : base(name, configFile, defaultValue, ui)
        {
            onDisplayText = delegate ()
            {
                return SokLoc.Translate("difficultymod_config_spawn") + " " + ColorText("blue", SokLoc.Translate($"difficultymod_config_spawn_{BoxedValue}"));
            };
            onDisplayEnum = delegate (SpawnSites s)
            {
                return SokLoc.Translate($"difficultymod_config_spawn_{s}");
            };
            onDisplayTooltip = delegate (SpawnSites s)
            {
                return SokLoc.Translate($"difficultymod_config_spawn_tooltip_{s}");
            };
            popupText = SokLoc.Translate("difficultymod_config_spawn_menu_text");
            popupTooltip = SokLoc.Translate("difficultymod_config_spawn_menu_tooltip");
        }
    }

#if false
    public class ConfigSpawnLocations : ConfigEntryHelper
    {
        public SpawnLocations allow = new();
        private string toolTip;
        private CustomButton setting;
        private CustomButton btnLeft, btnRight, btnTop, btnBottom;

        public ConfigSpawnLocations(string name, ConfigFile config)
        {
            Name = name;
            Config = config;
            ValueType = typeof(SpawnLocations);
            allow.left = Config.GetValue<bool>("difficultymod_allowLeft");
            allow.right = Config.GetValue<bool>("difficultymod_allowRight");
            allow.top = Config.GetValue<bool>("difficultymod_allowTop");
            allow.bottom = Config.GetValue<bool>("difficultymod_allowBottom");
            toolTip = SokLoc.Translate("difficultymod_config_location_tooltip");
            UI = new ConfigUI()
            {
                Hidden = true,
                OnUI = delegate (ConfigEntryBase c)
                {
                    setting = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab, ModOptionsScreen.instance.ButtonsParent);
                    setting.transform.localScale = Vector3.one;
                    setting.transform.localPosition = Vector3.zero;
                    setting.transform.localRotation = Quaternion.identity;
                    setting.TextMeshPro.text = SokLoc.Translate("difficultymod_config_location_header");
                    setting.TooltipText = toolTip;
                    setting.Clicked += delegate
                    {
                        OpenMenu();
                    };
                }
            };
            Config.Entries.Add(this);
        }

        public void OpenMenu()
        {
            if (GameCanvas.instance.ModalIsOpen) return;
            ModalScreen.instance.Clear();
            popup = ModalScreen.instance;
            popup.SetTexts(SokLoc.Translate("difficultymod_config_spawn_menu_text"),
                           SokLoc.Translate("difficultymod_config_spawn_menu_tooltip"));

            btnLeft = NewButton(ButtonAllowText(allow.left, "left"));
            btnLeft.Clicked += delegate
            {
                allow.left = !allow.left;
                btnLeft.TextMeshPro.text = ButtonAllowText(allow.left, "left");
                Config.Data["difficultymod_allowLeft"] = allow.left;
            };

            btnRight = NewButton(ButtonAllowText(allow.right, "right"));
            btnRight.Clicked += delegate
            {
                allow.right = !allow.right;
                btnRight.TextMeshPro.text = ButtonAllowText(allow.right, "right");
                Config.Data["difficultymod_allowRight"] = allow.right;
            };

            btnTop = NewButton(ButtonAllowText(allow.top, "top"));
            btnTop.Clicked += delegate
            {
                allow.top = !allow.top;
                btnTop.TextMeshPro.text = ButtonAllowText(allow.top, "top");
                Config.Data["difficultymod_allowTop"] = allow.top;
            };

            btnBottom = NewButton(ButtonAllowText(allow.bottom, "bottom"));
            btnBottom.Clicked += delegate
            {
                allow.bottom = !allow.bottom;
                btnBottom.TextMeshPro.text = ButtonAllowText(allow.bottom, "bottom");
                Config.Data["difficultymod_allowBottom"] = allow.bottom;
            };

            popup.AddOption(RightAlign(SokLoc.Translate("difficultymod_allon")), delegate () { UpdateAll(true); });
            popup.AddOption(RightAlign(SokLoc.Translate("difficultymod_alloff")), delegate () { UpdateAll(false); });
            popup.AddOption(RightAlign(SokLoc.Translate("difficultymod_closemenu")), CloseMenu);
            GameCanvas.instance.OpenModal();
        }

        private CustomButton NewButton(bool enabled, string text)
        {
            CustomButton btn = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab);
            btn.transform.SetParent(popup.ButtonParent);
            btn.transform.localScale = Vector3.one;
            btn.transform.localPosition = Vector3.zero;
            btn.transform.position = new Vector3 { x = 50, y = 0, z = 0 };
            btn.transform.localRotation = Quaternion.identity;
            btn.TextMeshPro.text = text;
            btn.TooltipText = SokLoc.Translate($"difficultymod_config_{(enabled ? "allow" : "disallow")}left");

            return btn;
        }

        private string TooltipText()
        {
            int num = (allow.left ? 8 : 0) + (allow.right ? 4 : 0) + (allow.top ? 2 : 0) + (allow.bottom ? 1 : 0);
            return SokLoc.Translate($"difficultymod_config_spawn_{num}");
        }

        private string ButtonAllowText(bool enabled, string place)
        {
            return SokLoc.Translate($"difficultymod_config_{(enabled?"allow":"disallow")}{place}");
        }

        public void UpdateAll(bool value)
        {
            allow.left = allow.right = allow.top = allow.bottom = value;
            btnLeft.TextMeshPro.text = ButtonAllowText(value, "left");
            btnRight.TextMeshPro.text = ButtonAllowText(value, "right");
            btnTop.TextMeshPro.text = ButtonAllowText(value, "top");
            btnBottom.TextMeshPro.text = ButtonAllowText(value, "bottom");
        }

        public override void SetDefaults()
        {
            allow = new();
            if (popup != null)
            {
                btnLeft.TextMeshPro.text = ButtonAllowText(allow.left, "left");
                btnRight.TextMeshPro.text = ButtonAllowText(allow.right, "right");
                btnTop.TextMeshPro.text = ButtonAllowText(allow.top, "top");
                btnBottom.TextMeshPro.text = ButtonAllowText(allow.bottom, "bottom");
            }
        }

        public override object BoxedValue
        {
            get => allow; set => allow = (SpawnLocations)value;
        }
    }
#endif

    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.GetRandomSpawnPosition))]
    internal class PortalControl
    {
        static bool Prefix(WorldManager __instance, ref Vector3 __result)
        {
            Bounds worldBounds = __instance.CurrentBoard.WorldBounds;
            DifficultyMod.Log($"GetRandomSpawnPosition() Min/Max X {worldBounds.min.x}/{worldBounds.max.x} Min/Max Z {worldBounds.min.z}/{worldBounds.max.z}");
            float x = Mathf.Lerp(worldBounds.min.x, worldBounds.max.x, DifficultyMod.instance.Xconfine);
            float z = Mathf.Lerp(worldBounds.min.z, worldBounds.max.z, DifficultyMod.instance.Zconfine);
            __result = new Vector3(x, 0f, z);
            DifficultyMod.Log($"GetRandomSpawnPosition() {__result}");
            return false;
        }
    }

}
