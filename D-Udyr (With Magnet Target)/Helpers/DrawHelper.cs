using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace D_Udyr.Helpers
{
    public class DrawHelper
    {
        public static Obj_AI_Hero player = ObjectManager.Player;
        public static string[] HeroesWithPet = new string[] { "Shaco", "Mordekaiser", "Ivern" };
        public static Menu CommonMenu;

        public static void DrawCircle(Circle circle, float spellRange)
        {
            if (circle.Active)
            {
                Render.Circle.DrawCircle(player.Position, spellRange, circle.Color);
            }
        }

        public static void popUp(string text, int time, Color fontColor, Color boxColor, Color borderColor)
        {
            var popUp = new Notification(text).SetTextColor(fontColor);
            popUp.SetBoxColor(boxColor);
            popUp.SetBorderColor(borderColor);
            Notifications.AddNotification(popUp);
            Utility.DelayAction.Add(time, () => popUp.Dispose());
        }

        public static Menu AddMisc(Menu main)
        {
            main = Events(main);
            return main;
        }

        public static Menu Events(Menu menu)
        {
            Menu menuEvents = new Menu("Events", "Eventsettings");

            Menu menuSpell = new Menu("On Damage", "SpellCastAllsettings");

            foreach (var h in HeroManager.AllHeroes)
            {
                Menu menuH = new Menu(h.ChampionName, "SpellCast" + h.ChampionName + "settings");
                menuH.AddItem(new MenuItem("DamPred0" + h.ChampionName, "Q")).SetValue(true);
                menuH.AddItem(new MenuItem("DamPred1" + h.ChampionName, "W")).SetValue(true);
                menuH.AddItem(new MenuItem("DamPred2" + h.ChampionName, "E")).SetValue(true);
                menuH.AddItem(new MenuItem("DamPred3" + h.ChampionName, "R")).SetValue(true);
                menuSpell.AddSubMenu(menuH);
            }
            menuEvents.AddSubMenu(menuSpell);

            Menu menuDash = new Menu("On Dash", "Dashsettings");
            foreach (var h in HeroManager.AllHeroes)
            {
                var data =
                    CombatHelper.DashDatas.Where(d => d.ChampionName == h.ChampionName && d.Slot != SpellSlot.Unknown);
                if (data.Any())
                {
                    menuDash.AddItem(
                        new MenuItem(
                            "DashEnabled" + h.ChampionName,
                            h.ChampionName + " " + (data.Count() == 1 ? data.First().Slot.ToString() : "Combo")))
                        .SetValue(true);
                }
            }
            menuEvents.AddSubMenu(menuDash);

            Menu menuInt = new Menu("On Interrupt", "OnIntsettings");
            foreach (var h in HeroManager.Enemies)
            {
                menuInt.AddItem(new MenuItem("IntEnabled" + h.ChampionName, h.ChampionName)).SetValue(true);
            }
            menuEvents.AddSubMenu(menuInt);

            menu.AddSubMenu(menuEvents);
            CommonMenu = menu;
            return menu;
        }

        public static Menu Tselector(Menu menu)
        {
            Menu rMenu = new Menu("TargetSelector+", "TS");
            rMenu.AddItem(new MenuItem("TargetSelectorSpellOnlyFocused", "Cast spells only selected target"))
                .SetValue(true);
            rMenu.AddItem(new MenuItem("TargetSelectorSpellOnlyRange", "Max range of selected target"))
                .SetValue(new Slider(10000, 1, 10000));
            menu.AddSubMenu(rMenu);
            CommonMenu = menu;
            return menu;
        }

        public static Obj_AI_Hero GetBetterTarget(float range,
            TargetSelector.DamageType damageType,
            bool ignoreShield = true,
            IEnumerable<Obj_AI_Hero> ignoredChamps = null,
            Vector3? rangeCheckFrom = null,
            TargetSelector.TargetSelectionConditionDelegate conditions = null)
        {
            var target = TargetSelector.GetTarget(
                range, damageType, ignoreShield, ignoredChamps, rangeCheckFrom, conditions);
            var selected = TargetSelector.GetSelectedTarget();
            if (CommonMenu.Item("TargetSelectorSpellOnlyFocused").GetValue<bool>() && selected != null &&
                player.ChampionName != "Gangplank" &&
                selected.Distance(player) < CommonMenu.Item("TargetSelectorSpellOnlyRange").GetValue<Slider>().Value)
            {
                if (selected.Distance(player) < range && selected.IsValidTarget())
                {
                    return selected;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return target;
            }
        }

        public static bool dashEnabled(string championName)
        {
            try
            {
                return CommonMenu.Item("DashEnabled" + championName).GetValue<bool>();
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static bool IntEnabled(string championName)
        {
            try
            {
                return CommonMenu.Item("IntEnabled" + championName).GetValue<bool>();
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static bool damagePredEnabled(string championName, SpellSlot slot)
        {
            try
            {
                return CommonMenu.Item("DamPred" + (int)slot + championName).GetValue<bool>();
            }
            catch (Exception e)
            {
                return true;
            }
        }
    }
}