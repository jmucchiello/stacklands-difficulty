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

        private float lowX = 0f, highX = 1f;
        private float lowZ = 0f, highZ = 1f;
        public void ApplySpawnSites()
        {
            switch (configSpawnSites.Value)
            {
                case SpawnSites.Anywhere:   lowX = lowZ = 0.1f; highX = highZ = 0.9f; break;
                case SpawnSites.Center:     lowX = lowZ = 0.4f; highX = highZ = 0.6f; break;
                case SpawnSites.LowerLeft:  lowX = lowZ = 0.1f; highX = highZ = 0.3f; break;
                case SpawnSites.UpperRight: lowX = lowZ = 0.7f; highX = highZ = 0.9f; break;
                case SpawnSites.LowerRight: lowX = 0.7f; lowZ = 0.1f; highX = 0.9f; highZ = 0.3f; break;
                case SpawnSites.UpperLeft:  lowX = 0.1f; lowZ = 0.7f; highX = 0.3f; highZ = 0.9f; break;
            }
            Log($"Spawn Location Ranges: X({lowX:F1} to {highX:F1}), Y({lowZ:F1} to {highZ:F1})");
        }

        public string YesNo(bool b) { return b ? "Yes" : "No"; }
    }

    public enum SpawnSites
    {
        Anywhere, Center, UpperLeft, UpperRight, LowerLeft, LowerRight
    };

    public class ConfigSpawnSites : ConfigEntryEnum<SpawnSites>
    {
        public ConfigSpawnSites(string name, ConfigFile configFile, SpawnSites defaultValue, ConfigUI ui = null)
            : base(name, configFile, defaultValue, ui)
        {
            currentValueColor = Color.blue;
            onDisplayAnchorText = delegate ()
            {
                return SizeText(25, SokLoc.Translate("difficultymod_config_spawn_anchor") + " " + ColorText(currentValueColor, SokLoc.Translate($"difficultymod_config_spawn_{(SpawnSites)Value}")));
            };
            onDisplayEnumText = delegate (SpawnSites s)
            {
                return SizeText(25, SokLoc.Translate($"difficultymod_config_spawn_{s}"));
            };
            onDisplayEnumTooltip = delegate (SpawnSites s)
            {
                SokTerm term = SokLoc.instance.CurrentLocSet.GetTerm($"difficultymod_config_spawn_tooltip_{s}");
                if (term == null) return null;
                return SokLoc.Translate($"difficultymod_config_spawn_tooltip_{s}");
            };
            popupMenuTitleText = SokLoc.Translate("difficultymod_config_spawn_menu_text");
            popupMenuHelpText = SokLoc.Translate("difficultymod_config_spawn_menu_tooltip");

            CloseButtonText = SokLoc.Translate("difficultymod_closemenu");
        }
    }

    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.GetRandomSpawnPosition))]
    internal class SpawnPosition_Patch
    {
        static bool Prefix(WorldManager __instance, ref Vector3 __result)
        {
            Bounds worldBounds = __instance.CurrentBoard.WorldBounds;
//            DifficultyMod.Log($"GetRandomSpawnPosition() Min/Max X {worldBounds.min.x}/{worldBounds.max.x} Min/Max Z {worldBounds.min.z}/{worldBounds.max.z}");
            float x = Mathf.Lerp(worldBounds.min.x, worldBounds.max.x, DifficultyMod.instance.Xconfine);
            float z = Mathf.Lerp(worldBounds.min.z, worldBounds.max.z, DifficultyMod.instance.Zconfine);
            __result = new Vector3(x, 0f, z);
//            DifficultyMod.Log($"GetRandomSpawnPosition() {__result}");
            return false;
        }
    }

}
