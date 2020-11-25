namespace KoreanLucian
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using KoreanCommon;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal class KillSteal
    {
        private readonly Func<Obj_AI_Hero, float> aaDamage;

        private readonly float aaRange;

        private readonly Spell e;

        private readonly Orbwalking.Orbwalker orbwalker;

        private readonly Obj_AI_Hero player;

        private readonly Spell w;

        public KillSteal(CommonChampion champion)
        {
            orbwalker = champion.Orbwalker;
            player = champion.Player;
            e = champion.Spells.E;
            w = champion.Spells.W;
            aaRange = Orbwalking.GetRealAutoAttackRange(champion.Player);
            aaDamage = target => (float)champion.Player.GetAutoAttackDamage(target);

            if (KoreanUtils.GetParamBool(champion.MainMenu, "killsteal"))
            {
                Game.OnUpdate += KS;
            }
        }

        public void KS(EventArgs args)
        {
            if (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
            {
                return;
            }

            List<Obj_AI_Hero> targetList =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(target => target.HealthPercent < 20f && target.Distance(player) < w.Range && !target.IsDead)
                    .ToList();

            if (targetList.Count == 0)
            {
                return;
            }

            targetList =
                targetList.Where(
                    target =>
                    ((e.IsReady() && !target.IsDead && target.Distance(player) > aaRange
                      && target.Distance(player) < e.Range + aaRange && aaDamage(target) * 1.4f > target.Health)
                     || (w.IsReady() && w.CanCast(target) && w.IsKillable(target)))).ToList();

            if (targetList.Count == 0)
            {
                return;
            }

            foreach (Obj_AI_Hero target in targetList)
            {
                if (w.IsReady() && w.CanCast(target) && w.IsKillable(target))
                {
                    w.Cast(target);
                }
                else
                {
                    e.Cast(target.Position);
                    orbwalker.ForceTarget(target);
                }
            }
        }
    }
}