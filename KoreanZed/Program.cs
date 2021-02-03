namespace KoreanZed
{
    using System;
    using LeagueSharp;
    using LeagueSharp.Common;

    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName.ToLowerInvariant() == "zed")
            {
                var ZedZeppelin = new Zed();
            }
        }
    }
}
