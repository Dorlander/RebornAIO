using LeagueSharp;
using VayneHunter_Reborn.External.Activator;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Utility
{
    class VHRBootstrap
    {
        public static void OnLoad()
        {
            Variables.Menu = new LeagueSharp.Common.Menu("VayneHunter Reborn","dz191.vhr", true);

            MenuGenerator.OnLoad();
            Activator.OnLoad();
            VHR.OnLoad();
            DrawManager.OnLoad();
        }
    }
}
