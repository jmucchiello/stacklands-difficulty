﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DifficultyModNS
{
    public static class I
    {
        public static WorldManager WM => WorldManager.instance;
        public static WorldManager.GameState GameState => WM.CurrentGameState;
        public static RunVariables CRV => WM.CurrentRunVariables;
        public static GameDataLoader GDL => WM.GameDataLoader;
        public static PrefabManager PFM => PrefabManager.instance;
        public static GameScreen GS => GameScreen.instance;
        public static ModOptionsScreen MOS => ModOptionsScreen.instance;
        public static ModalScreen Modal => ModalScreen.instance;
        public static string Xlat(string termId, params LocParam[] terms)
        {
            return terms.Length > 0 ? SokLoc.Translate(termId, terms) : SokLoc.Translate(termId);
        }
    }
}
