
using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;


namespace Evade
{
    public static class Config
    {
        public const bool PrintSpellData = false;
        public const bool TestOnAllies = false;
        public const int SkillShotsExtraRadius = 9;
        public const int SkillShotsExtraRange = 20;
        public const int GridSize = 10;
        public const int ExtraEvadeDistance = 15;
        public const int PathFindingDistance = 60;
        public const int PathFindingDistance2 = 35;

        public const int DiagonalEvadePointsCount = 7;
        public const int DiagonalEvadePointsStep = 20;

        public const int CrossingTimeOffset = 250;

        public const int EvadingFirstTimeOffset = 250;
        public const int EvadingSecondTimeOffset = 80;

        public const int EvadingRouteChangeTimeOffset = 250;

        public const int EvadePointChangeInterval = 300;
        public static int LastEvadePointChangeT = 0;

        public static Menu Menu;

        public static void CreateMenu()
        {
            Menu = new Menu("Badao Yauo WindWall", "Badao Yauo WindWall", true);
            Menu.SetFontStyle(System.Drawing.FontStyle.Bold, SharpDX.Color.YellowGreen);

            //Create the skillshots submenus.
            var skillShots = new Menu("Skillshots", "Skillshots");

            foreach (var hero in HeroManager.Enemies)
            {
                foreach (var spell in SpellDatabase.Spells.Where(x => x.MissileSpellName != ""))
                {
                    if (String.Equals(spell.ChampionName, hero.ChampionName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        skillShots.AddItem(new MenuItem(spell.MenuItemName, spell.MenuItemName).SetValue(false));
                    }
                }
            }

            Menu.AddSubMenu(skillShots);


            var collision = new Menu("Collision", "Collision");
            collision.AddItem(new MenuItem("MinionCollision", "Minion collision").SetValue(false));
            collision.AddItem(new MenuItem("HeroCollision", "Hero collision").SetValue(false));
            collision.AddItem(new MenuItem("YasuoCollision", "Yasuo wall collision").SetValue(true));
            collision.AddItem(new MenuItem("EnableCollision", "Enabled").SetValue(false));
            //TODO add mode.
            Menu.AddSubMenu(collision);


            var misc = new Menu("Misc", "Misc");
            misc.AddItem(new MenuItem("DisableFow", "Disable fog of war dodging").SetValue(false));


            Menu.AddSubMenu(misc);

            Menu.AddItem(
                new MenuItem("EnabledToggle", "Enabled Toggle").SetValue(new KeyBind("K".ToCharArray()[0], KeyBindType.Toggle, true)));

            Menu.AddItem(
                new MenuItem("EnablePress", "Enable Press").SetValue(new KeyBind(32, KeyBindType.Press)));

            Menu.AddToMainMenu();
            Game.OnUpdate += Game_OnUpdate;
            SkillshotDetector.SkillshotDetectors();
            TargetedSkillShots.EvadeTarget.Init();
        }
        // YasuoWall
        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Menu.Item("EnabledToggle").GetValue<KeyBind>().Active && !Menu.Item("EnablePress").GetValue<KeyBind>().Active)
                return;
            var skillshot = Skillshot.SkillshotAboutToHit(ObjectManager.Player, 250 + Game.Ping).Where
                (x => Menu.Item(x.SpellData.MenuItemName).GetValue<bool>()).FirstOrDefault();
            if (skillshot != null && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).IsReady())
            {
                var pos = ObjectManager.Player.Position.To2D().Extend(skillshot.Start, 250);
                ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W, pos.To3D());
            }
        }

    }
}
