using System;
using SCommon.Orbwalking;

namespace SCommon.PluginBase
{
    interface IChampion
    {
        /// <summary>
        /// Creates config menu
        /// </summary>
        void CreateConfigMenu();

        /// <summary>
        /// Sets champion spells
        /// </summary>
        void SetSpells();

        /// <summary>
        /// OnUpdate event
        /// </summary>
        /// <param name="args"><see cref="EventArgs"/></param>
        void Game_OnUpdate(EventArgs args);

        /// <summary>
        /// OnDraw event
        /// </summary>
        /// <param name="args"><see cref="EventArgs"/></param>
        void Drawing_OnDraw(EventArgs args);
    }
}
