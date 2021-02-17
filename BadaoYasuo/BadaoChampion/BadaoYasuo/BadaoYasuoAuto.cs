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
    public static class BadaoYasuoAuto
    {
        public static void BadaoActivate()
        {
            Game.OnUpdate += Game_OnUpdate;   
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (BadaoMainVariables.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None || ObjectManager.Player.IsRecalling())
                return;
            //AutoQ
            if (!ObjectManager.Player.UnderTurret(true) && BadaoYasuoVariables.AutoQ.GetValue<KeyBind>().Active && BadaoMainVariables.Q.IsReady() && !BadaoYasuoHelper.IsDashing())
            {
                var target = TargetSelector.GetTarget(BadaoYasuoHelper.GetQRange(), TargetSelector.DamageType.Physical);
                if (target.IsValidTarget() && (BadaoYasuoHelper.Qstate() != 3 || (BadaoYasuoHelper.Qstate() == 3 && BadaoYasuoVariables.AlsoAutoQ3.GetValue<bool>())))
                    BadaoYasuoHelper.CastQ(target);
            }
            
            //AutoKS
            if(BadaoYasuoVariables.AutoKSQ.GetValue<bool>() && BadaoMainVariables.Q.IsReady() && !BadaoYasuoHelper.IsDashing())
            {
                foreach (var hero in HeroManager.Enemies.Where(x => x.IsValidTarget(BadaoYasuoHelper.GetQRange()) && x.Health < BadaoYasuoHelper.GetQDamage(x)))
                {
                    BadaoYasuoHelper.CastQ(hero);
                }
            }
            if (BadaoYasuoVariables.AutoKSE.GetValue<bool>() && BadaoMainVariables.E.IsReady())
            {
                foreach (var hero in HeroManager.Enemies.Where(x => x.IsValidTarget(BadaoMainVariables.E.Range) && x.Health < BadaoYasuoHelper.GetEDamage(x)))
                {
                    BadaoMainVariables.E.Cast(hero);
                }
            }

            //AutoSTack Q
            if (BadaoYasuoVariables.AutoStackQ3.GetValue<bool>() && BadaoMainVariables.Q.IsReady() && BadaoYasuoHelper.Qstate() != 3
                && !HeroManager.Enemies.Any(x => x.IsValidTarget() && BadaoMainVariables.E.IsInRange(x)))
            {
                var target = BadaoYasuoHelper.GetETargets().FirstOrDefault();
                if (target.IsValidTarget())
                {
                    BadaoMainVariables.Q.Cast(target);
                }
            }
        }
    }
}
