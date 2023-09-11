using DifficultyModNS;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtensionMethods
{
    public static class CodeMatchExtensions
    {
        public static CodeMatcher GetPos(this CodeMatcher matcher, out Int32 position)
        {
            position = matcher.Pos;
            DifficultyMod.Log($"GetPos returning {position}");
            return matcher;
        }

        public static CodeMatcher Comment(this CodeMatcher matcher, string comment)
        {
            DifficultyMod.Log(comment);
            return matcher;
        }
    }
}
