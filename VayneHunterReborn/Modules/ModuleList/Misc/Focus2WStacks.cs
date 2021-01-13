using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Modules.ModuleHelpers;
using VayneHunter_Reborn.Utility.Helpers;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Modules.ModuleList.Misc
{
    class Focus2WStacks : IModule
    {
        public void OnLoad()
        {

        }

        public bool ShouldGetExecuted()
        {
            return MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.general.specialfocus");
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.OnUpdate;
        }

        public void OnExecute()
        {
            var target = HeroManager.Enemies.Find(en => en.IsValidTarget(ObjectManager.Player.AttackRange + 65f + 65f) && en.Has2WStacks());
            if (target != null)
            {
                TargetSelector.SetTarget(target);
            }
        }
    }
}
