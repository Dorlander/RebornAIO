using System;
using System.Reflection;
using LeagueSharp.Common;

namespace KurisuRiven
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            new KurisuRiven();
        }
    }
}