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
    public static class BadaoYasuoAssasinate
    {
        public static void BadaoActivate()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!BadaoYasuoVariables.AssassinateKey.GetValue<KeyBind>().Active)
                return;

            var target = TargetSelector.GetSelectedTarget();
            // orbwalk
            Orbwalking.Orbwalk((target.BadaoIsValidTarget() && Orbwalking.InAutoAttackRange(target))
                    ? target : null
                    , Game.CursorPos, 90, 50, false, false);

            if (!target.BadaoIsValidTarget())
                return;

            //QE
            if (BadaoYasuoHelper.CanCastSpell(target))
            {
                if (BadaoMainVariables.Q.IsReady() && !BadaoYasuoHelper.IsDashing()
                    && target.IsValidTarget(BadaoYasuoHelper.GetQRange()))
                {
                    BadaoYasuoHelper.CastQ(target);
                }
                if (BadaoMainVariables.E.IsReady())
                {
                    BadaoYasuoHelper.CastE(target);
                }
                if (BadaoMainVariables.Q.IsReady() && ObjectManager.Player.IsDashing())
                {
                    var data = ObjectManager.Player.GetDashInfo();
                    if (BadaoYasuoHelper.Qstate() == 3 || (Utility.CountEnemiesInRange(data.EndPos.To3D(), 270) >= 2))
                    {
                        BadaoYasuoHelper.CastQCone(target);
                    }
                }
            }

            // R always
            if (BadaoMainVariables.R.IsReady() && target.BadaoIsValidTarget(BadaoMainVariables.R.Range) && BadaoYasuoHelper.IsOnAir(target))
            {
                if (target.HasBuffOfType(BuffType.Knockback))
                    BadaoMainVariables.R.Cast(target);
                var buff = target.Buffs.Where(x => x.Type == BuffType.Knockup).MaxOrDefault(x => x.EndTime);
                if (buff != null && (buff.EndTime - Game.Time) * 1000 <= 200 + Game.Ping)
                {
                    BadaoMainVariables.R.Cast(target);
                }
            }

            //stack Q
            if (BadaoYasuoVariables.ComboStackQ.GetValue<bool>() && BadaoMainVariables.Q.IsReady() && BadaoYasuoHelper.Qstate() != 3
                && !BadaoMainVariables.E.IsInRange(target))
            {
                var Qtarget = BadaoYasuoHelper.GetETargets().FirstOrDefault();
                if (Qtarget.IsValidTarget())
                {
                    BadaoMainVariables.Q.Cast(target);
                }
            }

            //gap close E
            if (BadaoMainVariables.E.IsReady() && target.Distance(ObjectManager.Player) > 475)
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
