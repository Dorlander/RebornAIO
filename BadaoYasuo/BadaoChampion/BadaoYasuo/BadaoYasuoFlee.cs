using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


namespace BadaoKingdom.BadaoChampion.BadaoYasuo
{
    public static class BadaoYasuoFlee
    {
        public static void BadaoActivate()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!BadaoYasuoVariables.FleeKey.GetValue<KeyBind>().Active)
                return;
            //Flee
            Orbwalking.Orbwalk( null, Game.CursorPos, 90, 50, false, false);

            //gap close E
            if (BadaoMainVariables.E.IsReady())
            {
                var Etargets = BadaoYasuoHelper.GetETargets();
                var Etarget = Etargets.Where(x => x != null && BadaoYasuoHelper.GetEDashEnd(x).Distance(Game.CursorPos.To2D()) + 150 < ObjectManager.Player.Distance(Game.CursorPos))
                    .MinOrDefault(x => BadaoYasuoHelper.GetEDashEnd(x).Distance(Game.CursorPos.To2D()));
                if (Etarget != null)
                {
                    BadaoMainVariables.E.Cast(Etarget);
                }
            }

        }
    }
}
