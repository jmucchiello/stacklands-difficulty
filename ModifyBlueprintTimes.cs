using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace DifficultyModNS
{
    public class BlueprintModifier : Object
    {
        public string blueprintId;
        public int subprintindex; // use -1 for all subprints in the blueprint
        public float multiplier; // if >0 time is multiplied by this amount
        public float newTime; // if multiplier is 0 and this >0, time is replaced with this value

        public override bool Equals(Object other)
        {
            if (other == null || other is not BlueprintModifier) return false;
            return blueprintId == ((BlueprintModifier)other).blueprintId && subprintindex == ((BlueprintModifier)other).subprintindex;
        }

        public override int GetHashCode()
        {
            return blueprintId.GetHashCode() ^ subprintindex.GetHashCode(); 
        }

        public void AddToList()
        {
            ChangeBlueprintTimer.modifiers.Remove(this);
            ChangeBlueprintTimer.modifiers.Add(this);
        }
    }

    [HarmonyPatch(typeof(GameCard), nameof(GameCard.StartBlueprintTimer))]
    public class ChangeBlueprintTimer
    {
        public static float timeModifier = 1f;
        public static HashSet<BlueprintModifier> modifiers = new();

        static void Prefix(GameCard __instance, float time, TimerAction a, string status, string actionId, string blueprintId, int subprintIndex)
        {
            BlueprintModifier m = modifiers.FirstOrDefault(x => x.blueprintId == blueprintId && (x.subprintindex == subprintIndex || x.subprintindex == -1));
            if (m != null)
            {
                if (m.multiplier > 0f) { time *= timeModifier; }
                else if (m.newTime > 0f) { time = m.newTime; }
            }
        }
    }

}
