using System;
using System.Collections.Generic;
using System.Text;

namespace DifficultyModNS
{
    public static class I
    {
        public static WorldManager WM => WorldManager.instance;
        public static GameDataLoader GDL => WM.GameDataLoader;
        public static PrefabManager PF => PrefabManager.instance;
    }
}
