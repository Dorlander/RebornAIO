using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp.Common;
using VayneHunter_Reborn.External.Activator.ActivatorSpells;
using VayneHunter_Reborn.External.Activator.Items;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.External.Activator
{
    class Activator
    {
        public static List<IVHRItem> ActivatorItems = new List<IVHRItem> 
        {   
            new _BOTRK(), 
            new _Cutlass(), 
            new _Youmuu() 
        };

        public static List<IVHRSpell> ActivatorSpells = new List<IVHRSpell> 
        {   
            new Heal(),
            new Barrier(),
            new Ignite()
        };

        private static float _lastCycle;

        public static void OnLoad()
        {
            foreach (var item in ActivatorItems)
            {
                item.OnLoad();
            }

            foreach (var spell in ActivatorSpells)
            {
                spell.OnLoad();
            }
        }

        public static void LoadMenu()
        {
            var RootMenu = Variables.Menu;
            var ActivatorMenu = new Menu("[VHR] Activator","dz191.vhr.activator");
            {
                var OffensiveMenu = new Menu("Offensive","dz191.vhr.activator.offensive");
                {
                    foreach (var item in ActivatorItems.Where(h => h.GetItemType() == IVHRItemType.Offensive))
                    {
                        item.BuildMenu(OffensiveMenu);
                    }

                    ActivatorMenu.AddSubMenu(OffensiveMenu);
                }

                var DefensiveMenu = new Menu("Defensive", "dz191.vhr.activator.defensive");
                {
                    foreach (var item in ActivatorItems.Where(h => h.GetItemType() == IVHRItemType.Defensive))
                    {
                        item.BuildMenu(DefensiveMenu);
                    }

                    ActivatorMenu.AddSubMenu(DefensiveMenu);
                }

                var SpellsMenu = new Menu("Spells", "dz191.vhr.activator.spells");
                {
                    foreach (var spell in ActivatorSpells)
                    {
                        spell.BuildMenu(SpellsMenu);
                    }

                    ActivatorMenu.AddSubMenu(SpellsMenu);
                }

                ActivatorMenu.AddKeybind("dz191.vhr.activator.onkey","Activator Key", new Tuple<uint, KeyBindType>(32, KeyBindType.Press));
                ActivatorMenu.AddBool("dz191.vhr.activator.always", "Always Enabled", true);

                RootMenu.AddSubMenu(ActivatorMenu);
            }
        }

        public static void OnUpdate()
        {

            if (!MenuExtensions.GetItemValue<KeyBind>("dz191.vhr.activator.onkey").Active &&
                !MenuExtensions.GetItemValue<bool>("dz191.vhr.activator.always"))
            {
                return;
            }

            foreach (var item in ActivatorItems.Where(item => item.ShouldRun()))
            {
                item.Run();
            }

            foreach (var spell in ActivatorSpells.Where(item => item.ShouldRun()))
            {
                spell.Run();
            }
        }
    }
}
