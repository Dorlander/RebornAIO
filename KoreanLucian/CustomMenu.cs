namespace KoreanLucian
{
    using KoreanCommon;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal static class CustomMenu
    {
        public static void Load(CommonChampion champion)
        {
            LoadLaneClearMenu(champion);
            LoadMiscMenu(champion);
            LoadComboMenu(champion);
            LoadHarasMenu(champion);
        }

        private static void LoadHarasMenu(CommonChampion champion)
        {
            Menu menu = champion.MainMenu.HarasMenu;

            Menu subMenuChampions =
                menu.AddSubMenu(new Menu("Targets", KoreanUtils.ParamName(champion.MainMenu, "harastargets")));

            foreach (Obj_AI_Hero enemy in HeroManager.Enemies)
            {
                subMenuChampions.AddItem(
                    new MenuItem(
                        KoreanUtils.ParamName(champion.MainMenu, enemy.ChampionName.ToLowerInvariant()),
                        enemy.ChampionName).SetValue(true));
            }
        }

        private static void LoadComboMenu(CommonChampion champion)
        {
            Menu menu = champion.MainMenu.ComboMenu;

            MenuItem useRToCombo = menu.Item(KoreanUtils.ParamName(champion.MainMenu, "usertocombo"));
            useRToCombo.SetValue(false);
            menu.Items.Remove(useRToCombo);

            menu.Items.Remove(menu.Item(KoreanUtils.ParamName(champion.MainMenu, "minenemiestor")));
        }

        private static void LoadMiscMenu(CommonChampion champion)
        {
            Menu menu = champion.MainMenu.MiscMenu;

            Menu SemiAutoE = menu.AddSubMenu(new Menu("Semi-Automatic E", "semiautoe"));

            MenuItem semiAutomaticE =
                new MenuItem(KoreanUtils.ParamName(champion.MainMenu, "dashmode"), "Active").SetValue(false);

            SemiAutoE.AddItem(semiAutomaticE);
            semiAutomaticE.ValueChanged += delegate(object sender, OnValueChangeEventArgs e)
                {
                    if (e.GetNewValue<bool>())
                    {
                        Game.OnWndProc += ((Lucian)champion).semiAutomaticE.Game_OnWndProc;
                    }
                    else
                    {
                        Game.OnWndProc -= ((Lucian)champion).semiAutomaticE.Game_OnWndProc;
                    }
                };

            SemiAutoE.AddItem(
                new MenuItem(KoreanUtils.ParamName(champion.MainMenu, "drawingetext"), "Drawing text").SetValue(true));

            Menu extendedQMenu =
                menu.AddSubMenu(new Menu("Extended Q", KoreanUtils.ParamName(champion.MainMenu, "extendedqmenu")));
            extendedQMenu.AddItem(
                new MenuItem(KoreanUtils.ParamName(champion.MainMenu, "extendedq"), "Active").SetValue(true));

            MenuItem extendedQ =
                extendedQMenu.AddItem(
                    new MenuItem(KoreanUtils.ParamName(champion.MainMenu, "toggleextendedq"), "Auto Haras").SetValue(
                        new KeyBind('T', KeyBindType.Toggle)));

            extendedQ.ValueChanged += delegate(object sender, OnValueChangeEventArgs e)
                {
                    if (e.GetNewValue<KeyBind>().Active)
                    {
                        Game.OnUpdate += ExtendedQ.AutoExtendedQ;
                    }
                    else
                    {
                        Game.OnUpdate -= ExtendedQ.AutoExtendedQ;
                    }
                };

            menu.AddItem(new MenuItem(KoreanUtils.ParamName(champion.MainMenu, "lockr"), "Lock R").SetValue(true));
            menu.AddItem(
                new MenuItem(KoreanUtils.ParamName(champion.MainMenu, "useyoumuu"), "Use Youmuu before R").SetValue(
                    true));

            MenuItem ksOption =
                menu.AddItem(
                    new MenuItem(KoreanUtils.ParamName(champion.MainMenu, "killsteal"), "Smart KillSteal").SetValue(
                        true));

            ksOption.ValueChanged += delegate(object sender, OnValueChangeEventArgs e)
                {
                    if (e.GetNewValue<bool>())
                    {
                        Game.OnUpdate += ((Lucian)champion).killSteal.KS;
                    }
                    else
                    {
                        Game.OnUpdate -= ((Lucian)champion).killSteal.KS;
                    }
                };
        }

        private static void LoadLaneClearMenu(CommonChampion champion)
        {
            Menu menu = champion.MainMenu.LaneClearMenu;

            menu.AddItem(
                new MenuItem(KoreanUtils.ParamName(champion.MainMenu, "qcounthit"), "Q must hit").SetValue(
                    new Slider(3, 0, 6)));

            menu.AddItem(
                new MenuItem(KoreanUtils.ParamName(champion.MainMenu, "wcounthit"), "W must hit").SetValue(
                    new Slider(2, 0, 6)));

            MenuItem harasOnLaneClear = menu.Item(KoreanUtils.ParamName(champion.MainMenu, "harasonlaneclear"));
            menu.Items.Remove(harasOnLaneClear);
            menu.Items.Add(harasOnLaneClear);
        }
    }
}