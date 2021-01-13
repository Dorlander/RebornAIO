using System;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Utility;

namespace VayneHunter_Reborn
{
    class Program
    {
        private static string ChampionName = "Vayne";
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != ChampionName)
            {
                return;
            }
            VHRBootstrap.OnLoad();

            Game.PrintChat("<font color='#FF0000'><b>[VHR - Rewrite!]</b></font> By Asuna Loaded!");
            Game.PrintChat("Also try <font color='#66FF33'><b>DZAwareness</b></font> for a gamebreaking experience!");
        }
    }
}
