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
    public static class BadaoYasuoHarass
    {
        public static void BadaoActivate()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (BadaoMainVariables.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed)
                return;
            // Q
            if (BadaoYasuoVariables.HarassQ.GetValue<bool>() && BadaoMainVariables.Q.IsReady() && !BadaoYasuoHelper.IsDashing())
            {
                var target = TargetSelector.GetTarget(BadaoYasuoHelper.GetQRange(), TargetSelector.DamageType.Physical);
                if (target.IsValidTarget() && (BadaoYasuoHelper.Qstate() != 3 || (BadaoYasuoHelper.Qstate() == 3 && BadaoYasuoVariables.HarassQ3.GetValue<bool>())))
                    BadaoYasuoHelper.CastQ(target);
            }
        }
    }
}
