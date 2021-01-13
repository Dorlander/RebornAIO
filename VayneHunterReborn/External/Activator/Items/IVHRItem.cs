using LeagueSharp.Common;

namespace VayneHunter_Reborn.External.Activator.Items
{
    interface IVHRItem
    {
        void OnLoad();

        void BuildMenu(Menu RootMenu);

        IVHRItemType GetItemType();

        bool ShouldRun();

        void Run();
    }
}
