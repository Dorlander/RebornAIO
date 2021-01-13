using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Modules.ModuleHelpers;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Modules.ModuleList.Condemn
{
    class EAfterAA : IModule
    {
        public void OnLoad()
        {
        }

        public bool ShouldGetExecuted()
        {
            return MenuExtensions.GetItemValue<KeyBind>("dz191.vhr.misc.condemn.enextauto").Active &&
                   Variables.spells[SpellSlot.E].IsReady();
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.OnAfterAA;
        }

        public void OnExecute()
        {
            var target = Variables.Orbwalker.GetTarget();
            if (target.IsValidTarget(Variables.spells[SpellSlot.E].Range) && (target is Obj_AI_Hero))
            {
                var menuKey = Variables.Menu.Item("dz191.vhr.misc.condemn.enextauto");
                var key = menuKey.GetValue<KeyBind>().Key;
                Variables.spells[SpellSlot.E].Cast(target as Obj_AI_Hero);
                menuKey.SetValue(new KeyBind(key, KeyBindType.Toggle));
            }
        }
    }
}
