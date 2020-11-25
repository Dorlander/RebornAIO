namespace KoreanCommon
{
    using LeagueSharp.Common;

    public class CommonMenu
    {
        private Menu _comboMenu;

        private Menu _drwaingsMenu;

        private Menu _harasMenu;

        private Menu _laneClearMenu;

        private Menu _mainMenu;

        private string _menuName;

        private Menu _miscMenu;

        private Orbwalking.Orbwalker _orbwalker;

        public CommonMenu(string displayName, bool misc)
        {
            _menuName = displayName.Replace(" ", "_").ToLowerInvariant();

            _mainMenu = new Menu(displayName, _menuName, true);

            addOrbwalker(_mainMenu);
            addTargetSelector(_mainMenu);

            Menu modes = new Menu("Modes", string.Format("{0}.modes", MenuName));
            _mainMenu.AddSubMenu(modes);

            _harasMenu = addHarasMenu(modes);
            _laneClearMenu = addLaneClearMenu(modes);
            _comboMenu = addComboMenu(modes);

            if (misc)
            {
                _miscMenu = addMiscMenu(_mainMenu);
            }

            _drwaingsMenu = addDrawingMenu(_mainMenu);

            _mainMenu.AddToMainMenu();
        }

        public Menu MainMenu
        {
            get
            {
                return _mainMenu;
            }
        }

        public Menu HarasMenu
        {
            get
            {
                return _harasMenu;
            }
        }

        public Menu LaneClearMenu
        {
            get
            {
                return _laneClearMenu;
            }
        }

        public Menu ComboMenu
        {
            get
            {
                return _comboMenu;
            }
        }

        public Menu MiscMenu
        {
            get
            {
                return _miscMenu;
            }
        }

        public Menu DrawingMenu
        {
            get
            {
                return _drwaingsMenu;
            }
        }

        public string MenuName
        {
            get
            {
                return _menuName;
            }
        }

        public Orbwalking.Orbwalker Orbwalker
        {
            get
            {
                return _orbwalker;
            }
            set
            {
                _orbwalker = value;
            }
        }

        private void addOrbwalker(Menu RootMenu)
        {
            Menu orbwalkerMenu = new Menu("Orbwalker", string.Format("{0}.orbwalker", MenuName));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            RootMenu.AddSubMenu(orbwalkerMenu);
        }

        private void addTargetSelector(Menu RootMenu)
        {
            Menu targetselectorMenu = new Menu("Target Selector", string.Format("{0}.targetselector", MenuName));
            TargetSelector.AddToMenu(targetselectorMenu);
            RootMenu.AddSubMenu(targetselectorMenu);
        }

        private Menu addHarasMenu(Menu RootMenu)
        {
            Menu newMenu = new Menu("Haras", string.Format("{0}.haras", MenuName));
            RootMenu.AddSubMenu(newMenu);

            newMenu.AddItem(new MenuItem(string.Format("{0}.useqtoharas", MenuName), "Use Q").SetValue(true));
            newMenu.AddItem(new MenuItem(string.Format("{0}.usewtoharas", MenuName), "Use W").SetValue(true));
            newMenu.AddItem(new MenuItem(string.Format("{0}.useetoharas", MenuName), "Use E").SetValue(true));

            MenuItem ManaLimitToHaras =
                new MenuItem(string.Format("{0}.manalimittoharas", MenuName), "Mana limit").SetValue(
                    new Slider(0, 0, 100));
            newMenu.AddItem(ManaLimitToHaras);

            return newMenu;
        }

        private Menu addLaneClearMenu(Menu RootMenu)
        {
            Menu newMenu = new Menu("Lane Clear", string.Format("{0}.laneclear", MenuName));
            RootMenu.AddSubMenu(newMenu);

            newMenu.AddItem(new MenuItem(string.Format("{0}.useqtolaneclear", MenuName), "Use Q").SetValue(true));
            newMenu.AddItem(new MenuItem(string.Format("{0}.usewtolaneclear", MenuName), "Use W").SetValue(true));
            newMenu.AddItem(new MenuItem(string.Format("{0}.useetolaneclear", MenuName), "Use E").SetValue(true));
            newMenu.AddItem(
                new MenuItem(string.Format("{0}.manalimittolaneclear", MenuName), "Mana limit").SetValue(
                    new Slider(50, 0, 100)));
            newMenu.AddItem(
                new MenuItem(string.Format("{0}.harasonlaneclear", MenuName), "Haras enemies").SetValue(true));

            return newMenu;
        }

        private Menu addComboMenu(Menu RootMenu)
        {
            Menu newMenu = new Menu("Combo", string.Format("{0}.combo", MenuName));
            RootMenu.AddSubMenu(newMenu);

            newMenu.AddItem(new MenuItem(string.Format("{0}.useqtocombo", MenuName), "Use Q").SetValue(true));
            newMenu.AddItem(new MenuItem(string.Format("{0}.usewtocombo", MenuName), "Use W").SetValue(true));
            newMenu.AddItem(new MenuItem(string.Format("{0}.useetocombo", MenuName), "Use E").SetValue(true));
            newMenu.AddItem(new MenuItem(string.Format("{0}.usertocombo", MenuName), "Use R").SetValue(true));
            newMenu.AddItem(
                new MenuItem(string.Format("{0}.minenemiestor", MenuName), "Only R if will hit at least").SetValue(
                    new Slider(3, 1, 5)));
            newMenu.AddItem(
                new MenuItem(
                    string.Format("{0}.forceultusingmouse", MenuName),
                    "Force ultimate using mouse buttons (cursor sprite)").SetValue(true));

            return newMenu;
        }

        private Menu addMiscMenu(Menu RootMenu)
        {
            Menu newMenu = new Menu("Options", string.Format("{0}.misc", MenuName));
            RootMenu.AddSubMenu(newMenu);

            return newMenu;
        }

        private Menu addDrawingMenu(Menu RootMenu)
        {
            Menu newMenu = new Menu("Drawings", string.Format("{0}.drawings", MenuName));
            RootMenu.AddSubMenu(newMenu);

            newMenu.AddItem(new MenuItem(string.Format("{0}.drawskillranges", MenuName), "Skill ranges").SetValue(true));
            newMenu.AddItem(
                new MenuItem(string.Format("{0}.damageindicator", MenuName), "Damage indicator").SetValue(true));
            newMenu.AddItem(
                new MenuItem(string.Format("{0}.killableindicator", MenuName), "Killable indicator").SetValue(true));

            return newMenu;
        }
    }
}