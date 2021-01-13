using VayneHunter_Reborn.Modules.ModuleHelpers;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Modules.ModuleList.Misc
{
    class DisableMovement : IModule
    {
        public void OnLoad()
        {

        }

        public bool ShouldGetExecuted()
        {
            return MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.general.disablemovement") 
                || MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.general.disableattk");
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.OnUpdate;
        }

        public void OnExecute()
        {
            Variables.Orbwalker.SetMovement(!MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.general.disablemovement"));
            Variables.Orbwalker.SetAttack(!MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.general.disableattk"));
        }
    }
}
