using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace BadaoKingdom.BadaoChampion.BadaoYasuo
{
    public static class BadaoYasuoConfig
    {
        public static Menu config;
        public static void BadaoActivate()
        {
            // spells init
            BadaoMainVariables.Q = new Spell(SpellSlot.Q, 475);
            BadaoMainVariables.Q.SetSkillshot(0.3f, 10f, float.MaxValue, false, SkillshotType.SkillshotLine);
            BadaoMainVariables.Q.MinHitChance = HitChance.Low;
            BadaoMainVariables.Q2 = new Spell(SpellSlot.Q, 1150);
            BadaoMainVariables.Q2.SetSkillshot(0.5f, 10f, 1200, false, SkillshotType.SkillshotLine);
            BadaoMainVariables.Q.MinHitChance = HitChance.Low;
            BadaoMainVariables.W = new Spell(SpellSlot.W, 900);// 112.5 radius , 1.25 sec delay
            BadaoMainVariables.W.SetSkillshot(1.5f, 112.5f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            BadaoMainVariables.W.MinHitChance = HitChance.Medium;
            BadaoMainVariables.E = new Spell(SpellSlot.E, 475); // 375 radius, 500ms delay
            BadaoMainVariables.R = new Spell(SpellSlot.R, 1200);


            // main menu
            config = new Menu("BadaoKingdom " + ObjectManager.Player.ChampionName, ObjectManager.Player.ChampionName, true);
            config.SetFontStyle(System.Drawing.FontStyle.Bold, SharpDX.Color.YellowGreen);

            // orbwalker menu
            Menu orbwalkerMenu = new Menu("Orbwalker", "Orbwalker");
            BadaoMainVariables.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            config.AddSubMenu(orbwalkerMenu);

            // TS
            Menu ts = config.AddSubMenu(new Menu("Target Selector", "Target Selector")); ;
            TargetSelector.AddToMenu(ts);

            // Combo
            Menu Combo = config.AddSubMenu(new Menu("Combo", "Combo"));
            BadaoYasuoVariables.DiveTurretKey = Combo.AddItem(new MenuItem("DiveTurretKey", "Dive Turret").SetValue(new KeyBind('T', KeyBindType.Toggle,true)));
            BadaoYasuoVariables.TargetMode = Combo.AddItem(new MenuItem("Target Mode", "Target Mode").SetValue(new StringList(new string[] {"FreeStyle", "T.Selector" },0)));
            BadaoYasuoVariables.TargetModeKey = Combo.AddItem(new MenuItem("TargetModeKey", "Switch Mode Key").SetValue(new KeyBind('Y', KeyBindType.Press)));
            BadaoYasuoVariables.ComboIgnite = Combo.AddItem(new MenuItem("ComboIngite", "Ignite").SetValue(true));
            BadaoYasuoVariables.ComboStackQ = Combo.AddItem(new MenuItem("Combo Stack Q", "Stack Q").SetValue(true));
            BadaoYasuoVariables.ComboRHits = Combo.AddItem(new MenuItem("Combo R Hits", "R if will hit > or =").SetValue(new Slider(2,2,3)));
            foreach (var hero in HeroManager.Enemies)
            {
                Combo.AddItem(new MenuItem("ComboRAlways" + hero.NetworkId, "Always R " + hero.ChampionName + "(" + hero.Name + ")").SetValue(true));
                Combo.AddItem(new MenuItem("ComboRAlwaysHp" + hero.NetworkId, "if %HP <=").SetValue(new Slider(100, 0, 100)));
            }

            //Harass
            Menu Harass = config.AddSubMenu(new Menu("Harass", "Harass"));
            BadaoYasuoVariables.HarassQ = Harass.AddItem(new MenuItem("HarassQ", "Q").SetValue(true));
            BadaoYasuoVariables.HarassQ3 = Harass.AddItem(new MenuItem("HarassQ3", "Also Q3").SetValue(true));

            // LaneClear
            Menu LaneClear = config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            BadaoYasuoVariables.LaneQ = LaneClear.AddItem(new MenuItem("LaneQ", "Q").SetValue(true));
            BadaoYasuoVariables.LaneE = LaneClear.AddItem(new MenuItem("LaneE", "E").SetValue(true));

            // JungleClear
            Menu JungleClear = config.AddSubMenu(new Menu("JungleClear", "JungleClear"));
            BadaoYasuoVariables.JungQ = JungleClear.AddItem(new MenuItem("JungQ", "Q").SetValue(true));
            BadaoYasuoVariables.JungE = JungleClear.AddItem(new MenuItem("JungE", "E").SetValue(true));

            // LastHit
            Menu LastHit = config.AddSubMenu(new Menu("LastHit", "LastHit"));
            BadaoYasuoVariables.LastHitQ = LastHit.AddItem(new MenuItem("LastHitQ", "Q").SetValue(true));
            BadaoYasuoVariables.LastHitE = LastHit.AddItem(new MenuItem("LastHitE", "E").SetValue(true));

            // Auto
            Menu Auto = config.AddSubMenu(new Menu("Auto", "Auto"));
            BadaoYasuoVariables.AutoQ = Auto.AddItem(new MenuItem("AutoQ", "Auto Q").SetValue(new KeyBind('H',KeyBindType.Toggle,true)));
            BadaoYasuoVariables.AlsoAutoQ3 = Auto.AddItem(new MenuItem("AlsoAutoQ3", "Also Auto Q3").SetValue(true));
            BadaoYasuoVariables.AutoStackQ3 = Auto.AddItem(new MenuItem("AutoStackQ3", "Stack Q3").SetValue(true));
            BadaoYasuoVariables.AutoKSQ = Auto.AddItem(new MenuItem("AutoKSQ", "KS Q").SetValue(true));
            BadaoYasuoVariables.AutoKSE = Auto.AddItem(new MenuItem("AutoKSE", "KS E").SetValue(true));

            //Flee
            Menu Flee = config.AddSubMenu(new Menu("Flee", "Flee"));
            BadaoYasuoVariables.FleeKey = Flee.AddItem(new MenuItem("FleeKey", "Flee Key").SetValue(new KeyBind('Y', KeyBindType.Press)));

            //Flee
            Menu Assassinate = config.AddSubMenu(new Menu("Assassinate", "Assassinate"));
            BadaoYasuoVariables.AssassinateKey = Assassinate.AddItem(new MenuItem("AssassinateKey", "Assassinate Key").SetValue(new KeyBind('G', KeyBindType.Press)));

            //Draw
            Menu Draw = config.AddSubMenu(new Menu("Draw", "Draw"));
            BadaoYasuoVariables.DrawDiveTurret = Draw.AddItem(new MenuItem("DrawDiveTurret", "Dive Turret status").SetValue(true));
            BadaoYasuoVariables.DrawComboMode = Draw.AddItem(new MenuItem("DrawComboMode", "Combo Mode").SetValue(true));
            BadaoYasuoVariables.DrawAutoQ = Draw.AddItem(new MenuItem("DrawAutoQ", "Auto Q status").SetValue(true));
            BadaoYasuoVariables.DrawAssasinate = Draw.AddItem(new MenuItem("DrawAssasinate", "Draw Assasinate").SetValue(true));
            BadaoYasuoVariables.DrawE = Draw.AddItem(new MenuItem("DrawE", "E Range").SetValue(true));
            BadaoYasuoVariables.DrawR = Draw.AddItem(new MenuItem("DrawR", "R Range").SetValue(true));

            // attach to mainmenu
            config.AddToMainMenu();
        }
    }
}
