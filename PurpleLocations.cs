using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace DifficultyModNS
{
    [HarmonyPatch(typeof(Harvestable),nameof(Harvestable.GetCardToGive))]
    public class PurpleLocations
    {
        public static bool bCalledByLocation = false;

        static void Prefix(Harvestable __instance)
        {
            bCalledByLocation = DifficultyMod.AllowDangerousLocations && __instance.MyCardType == CardType.Locations;
        }

        static void Postfix(Harvestable __instance, ICardId __result)
        {
            bCalledByLocation = false;
        }
    }

    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.GetRandomCard))]
    public class PurpleLocations2
    {
        static bool Prefix(WorldManager __instance, ICardId __result, ref List<CardChance> chances, bool removeCard)
        {
            if (PurpleLocations.bCalledByLocation)
            {
                PurpleLocations.bCalledByLocation = false;
                chances = chances.FindAll(x => !isEnemy(x.Id));
            }
            return true;
        }

        static bool isEnemy(string cardId)
        {
            CardData cd = WorldManager.instance.GameDataLoader.GetCardFromId(cardId);
            return cd != null && cd is Enemy;
        }
    }
}

