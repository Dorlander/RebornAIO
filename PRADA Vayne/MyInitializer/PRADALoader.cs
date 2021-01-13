using System;
using LeagueSharp.Common;

namespace PRADA_Vayne.MyInitializer
{
    public static partial class PRADALoader
    {
        public static void Init()
        {
            CustomEvents.Game.OnGameLoad += args =>
            {
                MyUtils.Cache.Load();
                LoadMenu();
                LoadSpells();
                LoadLogic();
                ShowNotifications();
            };
        }
    }
}
