using System;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;
using RandomUlt.Helpers;

namespace RandomUlt
{
    internal class Program
    {
        public static Menu config;
        public static Orbwalking.Orbwalker orbwalker;
        public static LastPositions positions;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;

        private static void Main(string[] args)
        {
                CustomEvents.Game.OnGameLoad += Game_OnGameLoad;         
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            config = new Menu("RandomUlt Beta", "RandomUlt Beta", true);
            Menu RandomUltM = new Menu("Options", "Options");
            positions = new LastPositions(RandomUltM);
            config.AddSubMenu(RandomUltM);
            config.AddItem(new MenuItem("RandomUlt ", "by Soresu"));
            config.AddToMainMenu();
            Notifications.AddNotification(new Notification("RandomUlt by Soresu", 3000, true).SetTextColor(Color.Peru));
        }
    }
}