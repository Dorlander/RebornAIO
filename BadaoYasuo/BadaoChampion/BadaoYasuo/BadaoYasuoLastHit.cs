using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace BadaoKingdom.BadaoChampion.BadaoYasuo
{
    public static class BadaoYasuoLastHit
    {
        public static void BadaoActivate()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (BadaoMainVariables.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LastHit)
                return;
            //LastHit
            if (BadaoYasuoVariables.LastHitQ.GetValue<bool>() && BadaoMainVariables.Q.IsReady() && !ObjectManager.Player.IsDashing())
            {
                var minion = MinionManager.GetMinions(BadaoYasuoHelper.GetQRange()).Where(x => !MinionManager.IsWard(x as Obj_AI_Minion)
                    && BadaoYasuoHelper.GetQDamage(x) >= x.Health).FirstOrDefault();
                if (minion.IsValidTarget())
                {
                    BadaoMainVariables.Q.Cast(minion);
                }
            }
            if (BadaoYasuoVariables.LastHitE.GetValue<bool>() && BadaoMainVariables.E.IsReady())
            {
                var minion = MinionManager.GetMinions(BadaoMainVariables.E.Range).Where(x => !MinionManager.IsWard(x as Obj_AI_Minion)
                    && BadaoYasuoHelper.GetEDamage(x) >= x.Health).FirstOrDefault();
                if (minion.IsValidTarget())
                {
                    BadaoMainVariables.E.Cast(minion);
                }
            }
        }
    }
}
