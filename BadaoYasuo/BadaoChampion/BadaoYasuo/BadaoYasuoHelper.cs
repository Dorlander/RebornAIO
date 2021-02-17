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
    public static class BadaoYasuoHelper
    {
        public static int LastETick;
        public static int LastQTick;
        public static void BadaoActivate()
        {
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnPlayAnimation;
            //Game.OnUpdate += Game_OnUpdate;
            //CustomEvents.Unit.OnDash += Unit_OnDash;
        }

        private static void Obj_AI_Base_OnPlayAnimation(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
                return;
            if (args.Slot == SpellSlot.E != BadaoMainVariables.E.IsReady())
            {
                Orbwalking.Attack = false;
                Utility.DelayAction.Add(500, () => Orbwalking.Attack = true);
            }
        }

        private static void Unit_OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            if (!sender.IsMe)
                return;
            Game.PrintChat(args.StartPos.Distance(args.EndPos).ToString());
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            //Game.PrintChat(ObjectManager.Player.Distance(Game.CursorPos).ToString());
            //Game.PrintChat(BadaoMainVariables.Q.Instance.Name);
        }
        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!sender.Owner.IsMe)
                return;
            if (args.Slot == SpellSlot.E)
                LastETick = Environment.TickCount;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
                return;
            if (args.Slot == SpellSlot.E)
                LastETick = Environment.TickCount;
        }

        public static bool CanCastSpell(Obj_AI_Hero target)
        {
            return
                !(target.IsValidTarget() && Orbwalking.InAutoAttackRange(target) && (Orbwalking.CanAttack() || BadaoChecker.BadaoHasTiamat()))
                && Orbwalking.CanMove(0);
        }
        public static bool CanCastSpell()
        {
            return
                !(HeroManager.Enemies.Any(x => x.IsValidTarget() && Orbwalking.InAutoAttackRange(x)) && (Orbwalking.CanAttack() || BadaoChecker.BadaoHasTiamat()))
                && Orbwalking.CanMove(0);
        }
        // figure out dash duration
        public static bool IsDashing()
        {
            return Environment.TickCount - LastETick <= 500 - Game.Ping;
        }
        public static int Qstate()
        {
            var name = BadaoMainVariables.Q.Instance.Name;
            return
                name == "YasuoQW" ? 1 :
                name == "YasuoQ2W" ? 2 : 3;
        }
        public static void CastQ(Obj_AI_Base target)
        {
            if (Qstate() == 3)
            {
                var pred = BadaoMainVariables.Q2.GetPrediction(target, false, -1, new CollisionableObjects[] { CollisionableObjects.YasuoWall }).UnitPosition;
                if (ObjectManager.Player.Distance(pred) <= BadaoMainVariables.Q2.Range)
                    BadaoMainVariables.Q2.Cast(pred);
            }
            else
            {
                BadaoMainVariables.Q.Cast(target);
            }
        }
        public static void CastQ()
        {
            var enemies = HeroManager.Enemies.Where(x => x.IsValidTarget()).OrderBy(x => x.Health);
            foreach (var target in enemies)
            {
                if (Qstate() == 3)
                {
                    var pred = BadaoMainVariables.Q2.GetPrediction(target, false, -1,new CollisionableObjects[]{CollisionableObjects.YasuoWall}).UnitPosition;
                    if (ObjectManager.Player.Distance(pred) <= BadaoMainVariables.Q2.Range)
                        BadaoMainVariables.Q2.Cast(pred);
                }
                else
                {
                    BadaoMainVariables.Q.Cast(target);
                }
            }
        }
        public static void CastQCone(Obj_AI_Base target)
        {
            if (!ObjectManager.Player.IsDashing())
                return;
            var data = Dash.GetDashInfo(ObjectManager.Player);
            if (data == null)
                return;
            //if (ObjectManager.Player.ServerPosition.To2D().Distance(data.EndPos) >= Game.Ping + 150)
            //    return;
            var pred = Prediction.GetPrediction(target, (data.EndTick - Utils.TickCount) /*(475 + LastETick - Environment.TickCount) / 1000f*/);
            if (pred.UnitPosition.To2D().Distance(data.EndPos) <= 270 || target.Distance(data.EndPos) <= 270)
                ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Q,ObjectManager.Player.Position);
        }
        public static float GetQRange()
        {
            return
                Qstate() == 3 ?
                BadaoMainVariables.Q2.Range : BadaoMainVariables.Q.Range;
        }
        //E buff : YasuoDashWrapper
        public static List<Obj_AI_Base> GetETargets()
        {
            var targets = new List<Obj_AI_Base>();
            targets.AddRange(HeroManager.Enemies.Where(x => x.IsValidTarget()
                && !x.HasBuff("YasuoDashWrapper") && BadaoMainVariables.E.IsInRange(x)).ToList());
            targets.AddRange(ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget(BadaoMainVariables.E.Range) && !MinionManager.IsWard(x as Obj_AI_Minion)
                && x.Team != GameObjectTeam.Unknown)
                .Where(x => MinionManager.IsMinion(x) || x.Team == GameObjectTeam.Neutral).Where(x => !x.HasBuff("YasuoDashWrapper")));
            return targets.ToList();
        }
        public static Vector2 GetEDashEnd(Obj_AI_Base target)
        {
            var pred = Prediction.GetPrediction(target, Game.Ping / 2000f);
            return
                ObjectManager.Player.Position.To2D().Extend(pred.UnitPosition.To2D(), 475);
        }
        public static void CastE(bool underturret = false)
        {
            underturret = BadaoYasuoVariables.DiveTurretKey.GetValue<KeyBind>().Active;
            var Etargets = GetETargets().Where(x => !underturret ? !GetEDashEnd(x).To3D().UnderTurret(true) : x != null);
            var Eheroes = HeroManager.Enemies.Where(x => x.IsValidTarget());
            var ValidETarget = Etargets.Where(x => Eheroes.Any(y => Prediction.GetPrediction(y, 0.45f).UnitPosition.To2D().Distance(GetEDashEnd(x)) 
                <= ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + y.BoundingRadius
                || y.Position.To2D().Distance(GetEDashEnd(x))
                <= ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + y.BoundingRadius
                ))
                .FirstOrDefault();
            if (ValidETarget != null)
                BadaoMainVariables.E.Cast(ValidETarget);
        }
        public static void CastE(Obj_AI_Base target, bool underturret = false)
        {
            underturret = BadaoYasuoVariables.DiveTurretKey.GetValue<KeyBind>().Active;
            if (!target.IsValidTarget())
                return;
            var Etargets = GetETargets().Where(x => !underturret ? !GetEDashEnd(x).To3D().UnderTurret(true) : x != null);
            var ValidETarget = Etargets.Where(x => Prediction.GetPrediction(target, 0.45f).UnitPosition.To2D().Distance(GetEDashEnd(x))
                <= ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + target.BoundingRadius
                || target.Position.To2D().Distance(GetEDashEnd(x))
                <= ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + target.BoundingRadius
                )
                .FirstOrDefault();
            if (ValidETarget != null)
                BadaoMainVariables.E.Cast(ValidETarget);
        }
        public static Obj_AI_Hero GetESelector(bool underturret = false)
        {
            //underturret = BadaoYasuoVariables.DiveTurretKey.GetValue<KeyBind>().Active;
            //var Etargets = GetETargets().Where(x => !underturret ? !GetEDashEnd(x).To3D().UnderTurret(true) : x != null);
            //var target = HeroManager.Enemies.Where(y => y.IsValidTarget() && Etargets.Any(x =>
            //    Prediction.GetPrediction(y, 0.45f).UnitPosition.To2D().Distance(GetEDashEnd(x))
            //    <= ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + y.BoundingRadius
            //    || y.Position.To2D().Distance(GetEDashEnd(x))
            //    <= ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + y.BoundingRadius))
            //    .OrderBy(x => x.Health / HeroManager.Player.CalcDamage(x, Damage.DamageType.Physical, 100)).FirstOrDefault();
            //return target;
            return TargetSelector.GetTarget(BadaoMainVariables.E.Range + 100, TargetSelector.DamageType.Physical);
        }
        public static bool IsOnAir(Obj_AI_Hero target)
        {
            return target.HasBuffOfType(BuffType.Knockback) || target.HasBuffOfType(BuffType.Knockup);
        }
        public static int GetRCount(Obj_AI_Hero target)
        {
            return
                HeroManager.Enemies.Where(x => x.IsValidTarget() && x.Distance(target) <= 400 && IsOnAir(x)).Count();
        }
        public static double GetEDamage(Obj_AI_Base target)
        {
            return BadaoMainVariables.E.GetDamage(target) * (1 + 0.25 * ObjectManager.Player.GetBuffCount("YasuoDashScalar"));
        }
        public static double GetQDamage(Obj_AI_Base target)
        {
            var multiply = ItemData.Infinity_Edge.GetItem().IsOwned() ? 2.5 : 2;
            return
                ObjectManager.Player.Crit > 0.99 ?
                BadaoMainVariables.Q.GetDamage(target) * multiply * 0.9 :
                BadaoMainVariables.Q.GetDamage(target);
        }
    }
}
