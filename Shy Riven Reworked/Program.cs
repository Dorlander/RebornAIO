using System;
using SAutoCarry.Champions;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAutoCarry
{
    class Program
    {
        public static SCommon.PluginBase.Champion Champion; 
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.CharData.BaseSkinName != "Riven")
                return;

            Champion = new Riven();
        }
    }
}
