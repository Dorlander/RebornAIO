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
    public static class BadaoYasuoClear
    {
        public static void BadaoActivate()
        {
            Game.OnUpdate += Game_OnUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (BadaoMainVariables.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
                return;
            if (!unit.IsMe)
                return;
            if (BadaoChecker.BadaoHasTiamat())
                BadaoChecker.BadaoUseTiamat();
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (BadaoMainVariables.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
                return;
            //LaneClear
            if (BadaoYasuoVariables.LaneQ.GetValue<bool>() && BadaoMainVariables.Q.IsReady() && BadaoYasuoVariables.LaneE.GetValue<bool>() && BadaoMainVariables.E.IsReady())
            {
                var minions = MinionManager.GetMinions(BadaoMainVariables.E.Range).Where(x => !MinionManager.IsWard(x as Obj_AI_Minion));
                var minion = minions.Where(x => minions.Where(y => y.Distance(BadaoYasuoHelper.GetEDashEnd(x)) <= 250).Count() >= 2)
                    .MaxOrDefault(x => minions.Where(y => y.Distance(BadaoYasuoHelper.GetEDashEnd(x)) <= 250).Count());
                if (minion.IsValidTarget())
                {
                    BadaoMainVariables.E.Cast(minion);
                    Utility.DelayAction.Add(100, () => BadaoMainVariables.Q.Cast(ObjectManager.Player.Position));
                }
            }
            if (BadaoYasuoVariables.LaneQ.GetValue<bool>() && BadaoMainVariables.Q.IsReady() && !ObjectManager.Player.IsDashing())
            {
                var minions = MinionManager.GetMinions(BadaoYasuoHelper.GetQRange()).Where(x => !MinionManager.IsWard(x as Obj_AI_Minion));
                var minion = minions.Where(x => BadaoYasuoHelper.GetQDamage(x) + BadaoYasuoHelper.GetEDamage(x) >= x.Health).FirstOrDefault();
                if (minion.IsValidTarget())
                {
                    BadaoMainVariables.Q.Cast(minion);
                }
                var farm = MinionManager.GetBestLineFarmLocation(minions.Select(x => x.Position.To2D()).ToList(), 70, BadaoYasuoHelper.GetQRange());
                if (farm.MinionsHit >= 2)
                {
                    BadaoMainVariables.Q.Cast(farm.Position);
                }
            }
            if (BadaoYasuoVariables.LaneE.GetValue<bool>() && BadaoMainVariables.E.IsReady())
            {
                var minion = MinionManager.GetMinions(BadaoMainVariables.E.Range).Where(x => !MinionManager.IsWard(x as Obj_AI_Minion)
                    && BadaoYasuoHelper.GetEDamage(x) >= x.Health).FirstOrDefault();
                if (minion.IsValidTarget())
                {
                    BadaoMainVariables.E.Cast(minion);
                }
            }

            // JungleClear
            if (BadaoYasuoVariables.JungQ.GetValue<bool>() && BadaoMainVariables.Q.IsReady() && BadaoYasuoVariables.JungE.GetValue<bool>() && BadaoMainVariables.E.IsReady())
            {
                var minions = MinionManager.GetMinions(BadaoMainVariables.E.Range,MinionTypes.All,MinionTeam.Neutral).Where(x => !MinionManager.IsWard(x as Obj_AI_Minion));
                var minion = minions.Where(x => minions.Where(y => y.Distance(BadaoYasuoHelper.GetEDashEnd(x)) <= 250).Count() >= 2)
                    .MaxOrDefault(x => minions.Where(y => y.Distance(BadaoYasuoHelper.GetEDashEnd(x)) <= 250).Count());
                if (minion.IsValidTarget())
                {
                    BadaoMainVariables.Q.Cast(minion);
                }
            }
            if (BadaoYasuoVariables.JungQ.GetValue<bool>() && BadaoMainVariables.Q.IsReady() && !ObjectManager.Player.IsDashing())
            {
                var minions = MinionManager.GetMinions(BadaoYasuoHelper.GetQRange(), MinionTypes.All, MinionTeam.Neutral).OrderByDescending(x => x.MaxHealth)
                    .Where(x => !MinionManager.IsWard(x as Obj_AI_Minion));
                var minion = minions.FirstOrDefault();
                if (minion.IsValidTarget())
                {
                    BadaoMainVariables.Q.Cast(minion);
                }
                var farm = MinionManager.GetBestLineFarmLocation(minions.Select(x => x.Position.To2D()).ToList(), 70, BadaoYasuoHelper.GetQRange());
                if (farm.MinionsHit >= 2)
                {
                    BadaoMainVariables.Q.Cast(farm.Position);
                }
            }
            if (BadaoYasuoVariables.JungE.GetValue<bool>() && BadaoMainVariables.E.IsReady())
            {
                var minion = MinionManager.GetMinions(BadaoMainVariables.E.Range,MinionTypes.All, MinionTeam.Neutral).Where(x => !MinionManager.IsWard(x as Obj_AI_Minion)
                    && BadaoYasuoHelper.GetEDamage(x) >= x.Health).FirstOrDefault();
                if (minion.IsValidTarget())
                {
                    BadaoMainVariables.E.Cast(minion);
                    Utility.DelayAction.Add(100, () => BadaoMainVariables.Q.Cast(ObjectManager.Player.Position));
                }
            }
        }
    }
}
