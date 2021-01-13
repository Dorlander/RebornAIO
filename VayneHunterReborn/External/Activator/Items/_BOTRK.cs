using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;
using Color = SharpDX.Color;

namespace VayneHunter_Reborn.External.Activator.Items
{
    class _BOTRK : IVHRItem
    {
        public void OnLoad()
        {
            
        }

        public void BuildMenu(Menu RootMenu)
        {
            var itemMenu = new Menu("Blade of the Ruined King","dz191.vhr.activator.offensive.botrk");
            {
                itemMenu.AddItem(new MenuItem("dz191.vhr.activator.offensive.botrk.enemy", "On TARGET health % >")).SetFontStyle(FontStyle.Bold, Color.Red).SetValue(new Slider(50));
                itemMenu.AddItem(new MenuItem("dz191.vhr.activator.offensive.botrk.my", "On MY health % <")).SetFontStyle(FontStyle.Bold, Color.Green).SetValue(new Slider(20));
                itemMenu.AddItem(new MenuItem("dz191.vhr.activator.offensive.botrk.combo", "Use In Combo")).SetValue(true);
                itemMenu.AddItem(new MenuItem("dz191.vhr.activator.offensive.botrk.mixed", "Use In Harass")).SetValue(false);
                itemMenu.AddItem(new MenuItem("dz191.vhr.activator.offensive.botrk.always", "Use Always")).SetValue(false);
                RootMenu.AddSubMenu(itemMenu);
            }
        }

        public IVHRItemType GetItemType()
        {
            return IVHRItemType.Offensive;
        }

        public bool ShouldRun()
        {
            return LeagueSharp.Common.Items.HasItem(3153) && LeagueSharp.Common.Items.CanUseItem(3153);
        }

        public void Run()
        {
            var currentMenuItem =
                Variables.Menu.Item(
                    string.Format("dz191.vhr.activator.offensive.botrk.{0}", Variables.Orbwalker.ActiveMode.ToString().ToLower()));
            var currentValue = currentMenuItem != null ? currentMenuItem.GetValue<bool>() : false;

            if (currentValue || MenuExtensions.GetItemValue<bool>("dz191.vhr.activator.offensive.botrk.always"))
            {
                var target = Variables.Orbwalker.GetTarget();

                if (target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null)) && (target is Obj_AI_Hero))
                {
                    var tg = target as Obj_AI_Hero;
                    if (ObjectManager.Player.HealthPercent <=
                        MenuExtensions.GetItemValue<Slider>("dz191.vhr.activator.offensive.botrk.my").Value &&
                        (tg.Health / tg.MaxHealth) * 100 >=
                        MenuExtensions.GetItemValue<Slider>("dz191.vhr.activator.offensive.botrk.enemy").Value)
                    {
                        LeagueSharp.Common.Items.UseItem(3153, tg);
                    }
                }
            }
        }
    }
}
