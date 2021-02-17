#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace AutoLantern
{
    internal class Program
    {
        private const String LanternName = "ThreshLantern";
        private static Menu Menu;

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {

            if (!ThreshInGame())
            {
                return;
            }

            Menu = new Menu("AutoLantern", "AutoLantern", true);
            Menu.AddItem(new MenuItem("Auto", "Auto-Lantern at Low HP").SetValue(true));
            Menu.AddItem(new MenuItem("Low", "Low HP Percent").SetValue(new Slider(20, 30, 5)));
            Menu.AddItem(new MenuItem("Hotkey", "Hotkey").SetValue(new KeyBind(32, KeyBindType.Press)));
            Menu.AddToMainMenu();

            Game.OnUpdate += OnGameUpdate;

            Game.PrintChat("AutoLantern by Trees loaded.");
            Game.PrintChat("AutoLantern: You may have to click the lantern once manually.");
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (!(IsLow() && Menu.Item("Auto").IsActive()) && !Menu.Item("Hotkey").IsActive())
            {
                return;
            }

            var lantern =
                ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(o => o.IsValid && o.IsAlly && o.Name.Equals(LanternName));
            
            if (lantern != null && Player.Distance(lantern) <= 500 && Player.Spellbook.GetSpell((SpellSlot)  62).Name.Equals("LanternWAlly"))
            {
                Player.Spellbook.CastSpell((SpellSlot) 62, lantern);
            }
        }

        private static bool IsLow()
        {
            return Player.HealthPercent <= Menu.Item("Low").GetValue<Slider>().Value;
        }

        private static bool ThreshInGame()
        {
            return ObjectManager.Get<Obj_AI_Hero>().Any(h => h.IsAlly && !h.IsMe && h.ChampionName == "Thresh");
        }
    }
}