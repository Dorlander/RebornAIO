namespace KoreanZed
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.Remoting.Channels;

    using LeagueSharp;
    using LeagueSharp.Common;

    using KoreanZed.Enumerators;

    class ZedMenu
    {
        public Menu MainMenu { get; set; }

        private readonly ZedSpells zedSpells;

        public ZedMenu(ZedSpells zedSpells, out Orbwalking.Orbwalker orbwalker)
        {
            MainMenu = new Menu("Korean Zed", "mainmenu", true);
            this.zedSpells = zedSpells;

            AddTargetSelector();
            AddOrbwalker(out orbwalker);
            ComboMenu();
            HarassMenu();
            LaneClearMenu();
            LastHitMenu();
            MiscMenu();
            DrawingMenu();

            MainMenu.AddToMainMenu();

            GetInitialSpellValues();
        }

        private void AddTargetSelector()
        {
            Menu targetSelectorMenu = new Menu("Target Selector", "zedtargetselector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            MainMenu.AddSubMenu(targetSelectorMenu);
        }

        private void AddOrbwalker(out Orbwalking.Orbwalker orbwalker)
        {
            Menu orbwalkerMenu = new Menu("Orbwalker", "zedorbwalker");
            orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            MainMenu.AddSubMenu(orbwalkerMenu);
        }

        private void ComboMenu()
        {
            string prefix = "koreanzed.combomenu";
            Menu comboMenu = new Menu("Combo", prefix);

            comboMenu.AddItem(new MenuItem(prefix + ".useq", "Use Q").SetValue(true)).ValueChanged += (sender, args) =>
                { zedSpells.Q.UseOnCombo = args.GetNewValue<bool>(); };

            comboMenu.AddItem(new MenuItem(prefix + ".usew", "Use W").SetValue(true)).ValueChanged += (sender, args) =>
                { zedSpells.W.UseOnCombo = args.GetNewValue<bool>(); };

            comboMenu.AddItem(new MenuItem(prefix + ".usee", "Use E").SetValue(true)).ValueChanged += (sender, args) =>
                { zedSpells.E.UseOnCombo = args.GetNewValue<bool>(); };

            comboMenu.AddItem(new MenuItem(prefix + ".user", "Use R").SetValue(true)).ValueChanged += (sender, args) =>
                { zedSpells.R.UseOnCombo = args.GetNewValue<bool>(); };

            string useItemsPrefix = prefix + ".items";
            Menu useItems = new Menu("Items", useItemsPrefix);

            useItems.AddItem(new MenuItem(useItemsPrefix + ".bilgewater", "Use Bilgewater Cutlass").SetValue(true));
            useItems.AddItem(new MenuItem(useItemsPrefix + ".botrk", "Use BotRK").SetValue(true));
            useItems.AddItem(new MenuItem(useItemsPrefix + ".yomuus", "Use Youmuu's Ghostblade").SetValue(true));
            useItems.AddItem(new MenuItem(useItemsPrefix + ".hydra", "Use Hydra").SetValue(true));
            useItems.AddItem(new MenuItem(useItemsPrefix + ".tiamat", "Use Tiamat").SetValue(true));

            Menu rBlockSettings = new Menu("Use R Against", prefix + ".neverultmenu");
            string blockUltPrefix = prefix + ".blockult";
            foreach (var objAiHero in HeroManager.Enemies)
            {
                rBlockSettings.AddItem(
                    new MenuItem(
                        string.Format("{0}.{1}", blockUltPrefix, objAiHero.SkinName.ToLowerInvariant()),
                        objAiHero.SkinName).SetValue(true));
            }

            comboMenu.AddItem(new MenuItem("koreanzed.combo.ronselected", "Use R ONLY on Selected Target").SetValue(false));

            comboMenu.AddItem(new MenuItem(prefix + ".labelcombo1", "To switch the combo style:"));
            comboMenu.AddItem(new MenuItem(prefix + ".labelcombo2", "1 - Hold SHIFT; 2 - LEFT Click on Zed;"));
            comboMenu.AddItem(
                new MenuItem(prefix + ".combostyle", "Combo Style").SetValue(
                    new StringList(new string[] { "All Star", "The Line" })));

            comboMenu.AddSubMenu(useItems);
            comboMenu.AddSubMenu(rBlockSettings);
            MainMenu.AddSubMenu(comboMenu);
        }

        private void HarassMenu()
        {
            string prefix = "koreanzed.harasmenu";
            Menu harasMenu = new Menu("Harass", prefix);

            harasMenu.AddItem(new MenuItem(prefix + ".useq", "Use Q").SetValue(true)).ValueChanged += (sender, args) =>
                { zedSpells.Q.UseOnHarass = args.GetNewValue<bool>(); };

            harasMenu.AddItem(
                new MenuItem(prefix + ".checkcollisiononq", "Check Collision Before Using Q").SetValue(false));

            harasMenu.AddItem(new MenuItem(prefix + ".usee", "Use E").SetValue(true)).ValueChanged += (sender, args) =>
                { zedSpells.E.UseOnHarass = args.GetNewValue<bool>(); };

            string eUsagePrefix = prefix + ".wusage";
            Menu eHarasUsage = new Menu("W Settings", eUsagePrefix);

            eHarasUsage.AddItem(new MenuItem(prefix + ".usew", "Use W").SetValue(true)).ValueChanged +=
                (sender, args) =>
                    {
                        zedSpells.W.UseOnHarass = args.GetNewValue<bool>();
                    };
            eHarasUsage.AddItem(
                new MenuItem(eUsagePrefix + ".trigger", "Trigger").SetValue(
                    new StringList(new string[] { "Max Range", "Max Damage" })));
            eHarasUsage.AddItem(
                new MenuItem(eUsagePrefix + ".dontuseagainst", "Don't Use if Laning Against X Enemies").SetValue(
                    new Slider(6, 2, 6)));
            eHarasUsage.AddItem(
                new MenuItem(eUsagePrefix + ".dontuselowlife", "Don't Use if % HP Below").SetValue(
                    new Slider(0, 0, 100)));

            string blackListPrefix = prefix + ".blacklist";
            Menu blackListHaras = new Menu("Harass Target(s)", blackListPrefix + "");
            foreach (var objAiHero in HeroManager.Enemies)
            {
                blackListHaras.AddItem(
                    new MenuItem(
                        string.Format("{0}.{1}", blackListPrefix, objAiHero.SkinName.ToLowerInvariant()),
                        objAiHero.SkinName).SetValue(true));
            }

            string useItemsPrefix = prefix + ".items";
            Menu useItems = new Menu("Use Items", useItemsPrefix);
            useItems.AddItem(new MenuItem(useItemsPrefix + ".hydra", "Use Hydra").SetValue(true));
            useItems.AddItem(new MenuItem(useItemsPrefix + ".tiamat", "Use Tiamat").SetValue(true));

            harasMenu.AddItem(new MenuItem(prefix + ".saveenergy", "Save Energy (%)").SetValue(new Slider(50, 0, 100)));

            harasMenu.AddSubMenu(blackListHaras);
            harasMenu.AddSubMenu(useItems);
            harasMenu.AddSubMenu(eHarasUsage);
            MainMenu.AddSubMenu(harasMenu);
        }

        private void LaneClearMenu()
        {
            string prefix = "koreanzed.laneclearmenu";
            Menu laneClearMenu = new Menu("Lane / Jungle Clear", prefix);

            laneClearMenu.AddItem(new MenuItem(prefix + ".useq", "Use Q").SetValue(true)).ValueChanged +=
                (sender, args) =>
                    {
                        zedSpells.Q.UseOnLaneClear = args.GetNewValue<bool>();
                    };

            laneClearMenu.AddItem(new MenuItem(prefix + ".useqif", "Min. Minions to Use Q").SetValue(new Slider(3, 1, 6)));

            laneClearMenu.AddItem(new MenuItem(prefix + ".usew", "Use W").SetValue(true)).ValueChanged +=
                (sender, args) =>
                    {
                        zedSpells.W.UseOnLaneClear = args.GetNewValue<bool>();
                    };
            laneClearMenu.AddItem(new MenuItem(prefix + ".dontuseeif", "Don't Use if Laning Against X Enemies").SetValue(new Slider(6, 1, 6)));

            laneClearMenu.AddItem(new MenuItem(prefix + ".usee", "Use E").SetValue(true)).ValueChanged +=
                (sender, args) =>
                    {
                        zedSpells.E.UseOnLaneClear = args.GetNewValue<bool>();
                    };

            laneClearMenu.AddItem(new MenuItem(prefix + ".useeif", "Min. Minions to Use E").SetValue(new Slider(3, 1, 6)));

            laneClearMenu.AddItem(new MenuItem(prefix + ".saveenergy", "Save Energy (%)").SetValue(new Slider(40, 0, 100)));

            string useItemsPrefix = prefix + ".items";
            Menu useItems = new Menu("Use Items", useItemsPrefix);

            useItems.AddItem(new MenuItem(useItemsPrefix + ".hydra", "Use Hydra").SetValue(true));
            useItems.AddItem(new MenuItem(useItemsPrefix + ".tiamat", "Use Tiamat").SetValue(true));
            useItems.AddItem(new MenuItem(useItemsPrefix + ".when", "When will hit X minions").SetValue(new Slider(3, 1, 10)));

            laneClearMenu.AddSubMenu(useItems);

            MainMenu.AddSubMenu(laneClearMenu);
        }

        private void LastHitMenu()
        {
            string prefix = "koreanzed.lasthitmenu";
            Menu lastHitMenu = new Menu("Last Hit", prefix);

            lastHitMenu.AddItem(new MenuItem(prefix + ".useq", "Use Q").SetValue(true)).ValueChanged +=
                (sender, args) =>
                    {
                        zedSpells.Q.UseOnLastHit = args.GetNewValue<bool>();
                    };

            lastHitMenu.AddItem(new MenuItem(prefix + ".usee", "Use E").SetValue(true)).ValueChanged +=
                (sender, args) =>
                    {
                        zedSpells.E.UseOnLastHit = args.GetNewValue<bool>();
                    };

            lastHitMenu.AddItem(new MenuItem(prefix + ".useeif", "Min. Minions to Use E").SetValue(new Slider(3, 1, 6)));

            lastHitMenu.AddItem(new MenuItem(prefix + ".saveenergy", "Save Energy (%)").SetValue(new Slider(0)));

            MainMenu.AddSubMenu(lastHitMenu);
        }

        private void GetInitialSpellValues()
        {
            zedSpells.Q.UseOnCombo = GetParamBool("koreanzed.combomenu.useq");
            zedSpells.W.UseOnCombo = GetParamBool("koreanzed.combomenu.usew");
            zedSpells.E.UseOnCombo = GetParamBool("koreanzed.combomenu.usee");
            zedSpells.R.UseOnCombo = GetParamBool("koreanzed.combomenu.user");

            zedSpells.Q.UseOnHarass = GetParamBool("koreanzed.harasmenu.useq");
            zedSpells.W.UseOnHarass = GetParamBool("koreanzed.harasmenu.usew");
            zedSpells.E.UseOnHarass = GetParamBool("koreanzed.harasmenu.usee");

            zedSpells.Q.UseOnLastHit = GetParamBool("koreanzed.lasthitmenu.useq");
            zedSpells.E.UseOnLastHit = GetParamBool("koreanzed.lasthitmenu.usee");

            zedSpells.Q.UseOnLaneClear = GetParamBool("koreanzed.laneclearmenu.useq");
            zedSpells.W.UseOnLaneClear = GetParamBool("koreanzed.laneclearmenu.usew");
            zedSpells.E.UseOnLaneClear = GetParamBool("koreanzed.laneclearmenu.usee");
        }

        private void MiscMenu()
        {
            string prefix = "koreanzed.miscmenu";
            Menu miscMenu = new Menu("Miscellaneous", prefix);

            string gcPrefix = prefix + ".gc";
            Menu antiGapCloserMenu = new Menu("Gapcloser Options", gcPrefix);
            antiGapCloserMenu.AddItem(new MenuItem(prefix + ".usewantigc", "Use W to Escape").SetValue(true));
            antiGapCloserMenu.AddItem(new MenuItem(prefix + ".useeantigc", "Use E to Slow").SetValue(true));

            string rDodgePrefix = prefix + ".rdodge";
            Menu rDodgeMenu = new Menu("Use R to Dodge", rDodgePrefix);
            rDodgeMenu.AddItem(new MenuItem(rDodgePrefix + ".user", "Active").SetValue(true));
            rDodgeMenu.AddItem(new MenuItem(rDodgePrefix + ".dodgeifhealf", "Only if % HP Below").SetValue(new Slider(90, 0, 100)));
            rDodgeMenu.AddItem(new MenuItem(rDodgePrefix + ".label", "Dangerous Spells to Dodge:"));

            string[] neverDodge =
                {
                    "shen", "karma", "poppy", "soraka", "janna", "nidalee", "zilean", "yorick",
                    "mordekaiser", "vayne", "tryndamere", "trundle", "nasus", "lulu", "masteryi",
                    "kennen", "anivia", "heimerdinger", "drmundo", "elise", "fiora", "jax", "kassadin",
                    "khazix", "maokai", "fiddlesticks", "poppy", "shaco", "olaf", "alistar", "aatrox",
                    "taric", "nunu", "katarina", "rammus", "singed", "twistedfate", "teemo", "sivir",
                    "udyr"
                };

            foreach (
                Obj_AI_Hero objAiHero in
                    HeroManager.Enemies.Where(hero => !neverDodge.Contains(hero.SkinName.ToLowerInvariant()))
                        .OrderBy(hero => hero.SkinName))
            {
                foreach (
                    SpellDataInst spellDataInst in objAiHero.Spellbook.Spells.Where(spell => spell.Slot == SpellSlot.R))
                {
                    rDodgeMenu.AddItem(
                        new MenuItem(
                            rDodgePrefix + "." + spellDataInst.Name.ToLowerInvariant(),
                            objAiHero.SkinName + " - " + spellDataInst.Name.Replace(objAiHero.SkinName, "")).SetValue(
                                true));
                }
            }

            string potPrefix = prefix + ".pot";
            Menu usePotionMenu = new Menu("Use Health Potion", potPrefix);
            usePotionMenu.AddItem(new MenuItem(potPrefix + ".active", "Active").SetValue(true));
            usePotionMenu.AddItem(new MenuItem(potPrefix + ".when", "Use Potion at % HP").SetValue(new Slider(65)));

            miscMenu.AddItem(new MenuItem(prefix + ".flee", "Flee").SetValue(new KeyBind('G', KeyBindType.Press)));

            miscMenu.AddItem(new MenuItem(prefix + ".autoe", "Auto E if any enemy is on range").SetValue(true));

            miscMenu.AddItem(new MenuItem(prefix + ".forceultimate", "Force R Using Mouse Buttons (Cursor Sprite)").SetValue(true));

            miscMenu.AddSubMenu(antiGapCloserMenu);
            miscMenu.AddSubMenu(rDodgeMenu);
            miscMenu.AddSubMenu(usePotionMenu);
            MainMenu.AddSubMenu(miscMenu);
        }

        private void DrawingMenu()
        {
            string prefix = "koreanzed.drawing";
            Menu drawingMenu = new Menu("Drawings", prefix);

            drawingMenu.AddItem(new MenuItem(prefix + ".damageindicator", "Damage Indicator").SetValue(true));
            drawingMenu.AddItem(
                new MenuItem(prefix + ".damageindicatorcolor", "Color Scheme").SetValue(
                    new StringList(new string[] { "Normal", "Colorblind", "Sexy (Beta)" })));
            drawingMenu.AddItem(new MenuItem(prefix + ".killableindicator", "Killable Indicator").SetValue(true));
            drawingMenu.AddItem(new MenuItem(prefix + ".skillranges", "Skill Ranges").SetValue(true));

            MainMenu.AddSubMenu(drawingMenu);
        }

        public List<Obj_AI_Hero> GetBlockList(BlockListType blockListType)
        {
            List<Obj_AI_Hero> blackList = new List<Obj_AI_Hero>();

            switch (blockListType)
            {
                    case BlockListType.Harass:
                    foreach (Obj_AI_Hero objAiHero in HeroManager.Enemies)
                    {
                        if (!GetParamBool("koreanzed.harasmenu.blacklist." + objAiHero.SkinName.ToLowerInvariant()))
                        {
                            blackList.Add(objAiHero);
                        }
                    }
                    break;

                    case BlockListType.Ultimate:
                    foreach (Obj_AI_Hero objAiHero in HeroManager.Enemies)
                    {
                        if (!GetParamBool("koreanzed.combomenu.blockult." + objAiHero.SkinName.ToLowerInvariant()))
                        {
                            blackList.Add(objAiHero);
                        }
                    }
                    break;
            }

            return blackList;
        }

        public bool GetParamKeyBind(string paramName)
        {
            return MainMenu.Item(paramName).GetValue<KeyBind>().Active;
        }

        public int GetParamSlider(string paramName)
        {
            return MainMenu.Item(paramName).GetValue<Slider>().Value;
        }

        public bool GetParamBool(string paramName)
        {
            return MainMenu.Item(paramName).GetValue<bool>();
        }

        public int GetParamStringList(string paramName)
        {
            return MainMenu.Item(paramName).GetValue<StringList>().SelectedIndex;
        }

        public ComboType GetCombo()
        {
            return (ComboType)GetParamStringList("koreanzed.combomenu.combostyle");
        }

        public void SetCombo(ComboType comboStyle)
        {
            var teste =
                MainMenu.Item("koreanzed.combomenu.combostyle")
                    .SetValue<StringList>(
                        new StringList(new string[] { "All Star", "The Line" }) { SelectedIndex = (int)comboStyle });
        }

        public Color GetParamColor(string paramName)
        {
            return MainMenu.Item(paramName).GetValue<Circle>().Color;
        }

        public bool CheckMenuItem(string paramName)
        {
            return (MainMenu.Item(paramName) != null);
        }
    }
}
