namespace KoreanZed
{
    using LeagueSharp;
    using LeagueSharp.Common;

    class ZedUnderTurretFarm
    {
        private readonly Orbwalking.Orbwalker zedOrbwalker;

        private readonly ZedSpell q;

        private readonly ZedSpell e;

        private Obj_AI_Minion targetUnderTurret;

        private Obj_AI_Base turrent;

        public ZedUnderTurretFarm(ZedSpells zedSpells, Orbwalking.Orbwalker zedOrbwalker)
        {
            q = zedSpells.Q;
            e = zedSpells.E;

            this.zedOrbwalker = zedOrbwalker;

           // Obj_AI_Base.OnTarget += Obj_AI_Base_OnTarget;
            Game.OnUpdate += Game_OnUpdate;
        }

       /* private void Obj_AI_Base_OnTarget(Obj_AI_Base sender, Obj_AI_BaseTargetEventArgs args)
        {
            if (sender == null || args.Target == null || !sender.IsAlly || !args.Target.IsEnemy
                || !sender.SkinName.ToLowerInvariant().Contains("turret")
                || !args.Target.Name.ToLowerInvariant().Contains("minion"))
            {
                return;
            }

            if (ObjectManager.Player.Distance(args.Target) <= q.Range)
            {
                turrent = sender;
                targetUnderTurret = new Obj_AI_Minion((ushort)args.Target.Index, (uint)args.Target.NetworkId); ;
            }
            else
            {
                turrent = null;
                targetUnderTurret = null;
            }
        }*/

        private void Game_OnUpdate(System.EventArgs args)
        {
            if (targetUnderTurret != null && targetUnderTurret.IsDead)
            {
                targetUnderTurret = null;
                turrent = null;
            }

            if (zedOrbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo
                && zedOrbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None && turrent != null
                && targetUnderTurret != null && !targetUnderTurret.IsDead && targetUnderTurret.IsValid)
            {
                if (targetUnderTurret.IsValid)
                {
                    if (ObjectManager.Player.Distance(targetUnderTurret)
                        < Orbwalking.GetRealAutoAttackRange(targetUnderTurret) + 20F
                        && (targetUnderTurret.Health
                            < (ObjectManager.Player.GetAutoAttackDamage(targetUnderTurret) * 2)
                            + turrent.GetAutoAttackDamage(targetUnderTurret)
                            && targetUnderTurret.Health
                            > turrent.GetAutoAttackDamage(targetUnderTurret)
                            + ObjectManager.Player.GetAutoAttackDamage(targetUnderTurret)))
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, targetUnderTurret);
                    }

                    if (q.IsReady() && q.CanCast(targetUnderTurret)
                        && ObjectManager.Player.Distance(targetUnderTurret)
                        < Orbwalking.GetRealAutoAttackRange(targetUnderTurret) + 20F
                        && targetUnderTurret.Health
                        < q.GetDamage(targetUnderTurret)
                        + ObjectManager.Player.GetAutoAttackDamage(targetUnderTurret, true))
                    {
                        q.Cast(targetUnderTurret);
                        return;
                    }

                    if (e.IsReady() && e.CanCast(targetUnderTurret) && !q.IsReady()
                        && ObjectManager.Player.Distance(targetUnderTurret)
                        < Orbwalking.GetRealAutoAttackRange(targetUnderTurret) + 20F
                        && targetUnderTurret.Health
                        < e.GetDamage(targetUnderTurret)
                        + ObjectManager.Player.GetAutoAttackDamage(targetUnderTurret, true))
                    {
                        e.Cast(targetUnderTurret);
                    }
                }
            }
        }
    }
}
