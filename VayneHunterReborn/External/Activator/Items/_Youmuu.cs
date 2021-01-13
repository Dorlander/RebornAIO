using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.External.Activator.Items
{
    class _Youmuu : IVHRItem
    {
        public void OnLoad()
        {
            Orbwalking.AfterAttack += AfterAttack;
        }

        public void BuildMenu(Menu RootMenu)
        {
            var itemMenu = new Menu("Youmuu's Ghostblade", "dz191.vhr.activator.offensive.youmuu");
            {
                itemMenu.AddItem(new MenuItem("dz191.vhr.activator.offensive.youmuu.combo", "Use In Combo")).SetValue(true);
                itemMenu.AddItem(new MenuItem("dz191.vhr.activator.offensive.youmuu.mixed", "Use In Harass")).SetValue(false);
                itemMenu.AddItem(new MenuItem("dz191.vhr.activator.offensive.youmuu.always", "Use Always")).SetValue(false);
                RootMenu.AddSubMenu(itemMenu);
            }
        }

        public IVHRItemType GetItemType()
        {
            return IVHRItemType.Offensive;
        }

        public bool ShouldRun()
        {
            return LeagueSharp.Common.Items.HasItem(3142) && LeagueSharp.Common.Items.CanUseItem(3142);
        }

        public void Run()
        {
            
        }

        private void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (ShouldRun())
            {
                if (!(target is Obj_AI_Hero))
                {
                    return;
                }

                var TargetHero = (Obj_AI_Hero) target;

                var currentMenuItem =
                    Variables.Menu.Item(
                        string.Format("dz191.vhr.activator.offensive.youmuu.{0}", Variables.Orbwalker.ActiveMode.ToString().ToLower()));
                var currentValue = currentMenuItem != null ? currentMenuItem.GetValue<bool>() : false;


                if (currentValue || MenuExtensions.GetItemValue<bool>("dz191.vhr.activator.offensive.youmuu.always"))
                {
                    if (TargetHero.IsValidTarget(ObjectManager.Player.AttackRange + 65f + 65f + 150f))
                    {
                        LeagueSharp.Common.Items.UseItem(3142);
                    }
                }
            }
        }
    }
}
