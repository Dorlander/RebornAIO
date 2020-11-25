namespace KoreanLucian
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal class Program
    {
        public static Lucian ChampionLucian;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName.ToLowerInvariant() == "lucian")
            {
                ChampionLucian = new Lucian();
            }
        }
    }
}