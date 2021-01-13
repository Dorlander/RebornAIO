using LeagueSharp.Common;

namespace VayneHunter_Reborn.External.Activator.ActivatorSpells
{
    interface IVHRSpell
    {
        void OnLoad();

        void BuildMenu(Menu RootMenu);
        
        bool ShouldRun();

        void Run();
    }
}
