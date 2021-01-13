using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;
using Color = SharpDX.Color;

namespace VayneHunter_Reborn.External.Activator.Items
{
    class _Cutlass : IVHRItem
    {
        public void OnLoad()
        {

        }

        public void BuildMenu(Menu RootMenu)
        {
            var itemMenu = new Menu("Bilgewater Cutlass", "dz191.vhr.activator.offensive.cutlass");
            {
                itemMenu.AddItem(new MenuItem("dz191.vhr.activator.offensive.cutlass.enemy", "On TARGET health % >")).SetFontStyle(FontStyle.Bold, Color.Red).SetValue(new Slider(50));
                itemMenu.AddItem(new MenuItem("dz191.vhr.activator.offensive.cutlass.my", "On MY health % <")).SetFontStyle(FontStyle.Bold, Color.Green).SetValue(new Slider(20));
                itemMenu.AddItem(new MenuItem("dz191.vhr.activator.offensive.cutlass.combo", "Use In Combo")).SetValue(true);
                itemMenu.AddItem(new MenuItem("dz191.vhr.activator.offensive.cutlass.mixed", "Use In Harass")).SetValue(false);
                itemMenu.AddItem(new MenuItem("dz191.vhr.activator.offensive.cutlass.always", "Use Always")).SetValue(false);
                RootMenu.AddSubMenu(itemMenu);
            }
        }

        public IVHRItemType GetItemType()
        {
            return IVHRItemType.Offensive;
        }

        public bool ShouldRun()
        {
            return LeagueSharp.Common.Items.HasItem(3144) && LeagueSharp.Common.Items.CanUseItem(3144);
        }

        public void Run()
        {
            var currentMenuItem =
                Variables.Menu.Item(
                    string.Format("dz191.vhr.activator.offensive.cutlass.{0}", Variables.Orbwalker.ActiveMode.ToString().ToLower()));
            var currentValue = currentMenuItem != null ? currentMenuItem.GetValue<bool>() : false;


            if (currentValue || MenuExtensions.GetItemValue<bool>("dz191.vhr.activator.offensive.cutlass.always"))
            {
                var target = TargetSelector.GetTarget(450f, TargetSelector.DamageType.True);
                if (target.IsValidTarget())
                {
                    if (ObjectManager.Player.HealthPercent <=
                        MenuExtensions.GetItemValue<Slider>("dz191.vhr.activator.offensive.cutlass.my").Value &&
                        target.HealthPercent >=
                        MenuExtensions.GetItemValue<Slider>("dz191.vhr.activator.offensive.cutlass.enemy").Value)
                    {
                        LeagueSharp.Common.Items.UseItem(3144, target);
                    }
                }
            }
        }
    }
}
