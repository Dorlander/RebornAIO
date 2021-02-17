using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SCommon;
using SCommon.Database;
//typedefs
using TargetSelector = SCommon.TS.TargetSelector;

namespace SCommon.Orbwalking
{
    public class Orbwalker
    {
        public enum Mode
        {
            None,
            Combo,
            Mixed,
            LaneClear,
            LastHit,
        }

        private Random m_rnd;
        private int m_lastAATick;
        private int m_lastWindUpTick;
        private int m_lastWindUpTime;
        private int m_lastAttackCooldown;
        private int m_lastAttackCompletesAt;
        private int m_lastMoveTick;
        private int m_lastAttackTick;
        private float m_baseAttackSpeed;
        private float m_baseWindUp;
        private bool m_attackInProgress;
        private bool m_Attack;
        private bool m_Move;
        private Vector2 m_lastAttackPos;
        private Vector3 m_orbwalkingPoint;
        private ConfigMenu m_Configuration;
        private bool m_orbwalkEnabled;
        private AttackableUnit m_forcedTarget;
        private bool m_attackReset;
        private AttackableUnit m_lastTarget;
        private AttackableUnit m_towerTarget;
        private Func<bool> m_fnCanAttack;
        private Func<bool> m_fnCanMove;
        private Func<AttackableUnit, bool> m_fnCanOrbwalkTarget;
        private Func<bool> m_fnShouldWait;

        public Orbwalker(Menu menuToAttach)
        {
            m_rnd = new Random();
            m_lastAATick = 0;
            m_lastWindUpTick = 0;
            m_lastMoveTick = 0;
            m_Attack = true;
            m_Move = true;
            m_baseWindUp = 1f / (ObjectManager.Player.AttackCastDelay * ObjectManager.Player.GetAttackSpeed());
            m_baseAttackSpeed = 1f / (ObjectManager.Player.AttackDelay * ObjectManager.Player.GetAttackSpeed());
            m_orbwalkingPoint = Vector3.Zero;
            m_Configuration = new ConfigMenu(this, menuToAttach);
            m_orbwalkEnabled = true;
            m_forcedTarget = null;
            m_lastTarget = null;
            m_fnCanAttack = null;
            m_fnCanMove = null;
            m_fnCanOrbwalkTarget = null;
            m_fnShouldWait = null;

            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Obj_AI_Base.OnDoCast += Obj_AI_Base_OnDoCast;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnPlayAnimation;
            Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
            Obj_AI_Base.OnBuffAdd += Obj_AI_Base_OnBuffAdd;
            Obj_AI_Base.OnBuffRemove += Obj_AI_Base_OnBuffRemove;
            new Drawings(this);
        }

        /// <summary>
        /// Gets Orbwalker's active mode
        /// </summary>
        public Mode ActiveMode
        {
            get
            {
                if (m_Configuration.Combo)
                    return Mode.Combo;

                if (m_Configuration.Harass)
                    return Mode.Mixed;

                if (m_Configuration.LaneClear)
                    return Mode.LaneClear;

                if (m_Configuration.LastHit)
                    return Mode.LastHit;

                return Mode.None;
            }
        }

        /// <summary>
        /// Gets Last Auto Attack Tick
        /// </summary>
        public int LastAATick
        {
            get { return m_lastAATick; }
        }

        /// <summary>
        /// Gets Last WindUp tick
        /// </summary>
        public int LastWindUpTick
        {
            get { return m_lastWindUpTick; }
        }

        /// <summary>
        /// Gets Last Movement tick
        /// </summary>
        public int LastMoveTick
        {
            get { return m_lastMoveTick; }
        }

        /// <summary>
        /// Gets Configuration menu;
        /// </summary>
        public ConfigMenu Configuration
        {
            get { return m_Configuration; }
        }

        /// <summary>
        /// Gets or sets orbwalking point
        /// </summary>
        public Vector3 OrbwalkingPoint
        {
            get { return m_orbwalkingPoint == Vector3.Zero ? Game.CursorPos : m_orbwalkingPoint; }
            set { m_orbwalkingPoint = value; }
        }

        /// <summary>
        /// Gets or sets orbwalking is enabled
        /// </summary>
        public bool Enabled
        {
            get { return m_orbwalkEnabled; }
            set { m_orbwalkEnabled = value; }
        }

        /// <summary>
        /// Gets or sets forced orbwalk target
        /// </summary>
        public AttackableUnit ForcedTarget
        {
            get { return m_forcedTarget; }
            set { m_forcedTarget = value; }
        }

        /// <summary>
        /// Gets base attack speed value
        /// </summary>
        public float BaseAttackSpeed
        {
            get { return m_baseAttackSpeed; }
        }

        /// <summary>
        /// Gets base windup value
        /// </summary>
        public float BaseWindup
        {
            get { return m_baseWindUp; }
        }

        /// <summary>
        /// Resets auto attack timer
        /// </summary>
        public void ResetAATimer()
        {
            if (m_baseAttackSpeed != 0.5f)
            {
                m_lastAATick = Utils.TickCount - Game.Ping / 2 - m_lastAttackCooldown;
                m_lastAttackTick = 0;
                m_attackReset = true;
                m_attackInProgress = false;
            }
        }

        /// <summary>
        /// Resets orbwalk values
        /// </summary>
        public void ResetOrbwalkValues()
        {
            m_baseAttackSpeed = 0.5f;
        }

        /// <summary>
        /// Checks if player can attack
        /// </summary>
        /// <returns>true if can attack</returns>
        public bool CanAttack(int t = 0)
        {
            if (ObjectManager.Player.CharData.BaseSkinName == "Graves" && !ObjectManager.Player.HasBuff("GravesBasicAttackAmmo1") && !ObjectManager.Player.HasBuff("GravesBasicAttackAmmo2"))
                return false;

            if (!m_Attack)
                return false;

            if (m_fnCanAttack != null)
                return m_fnCanAttack();

            if (m_attackReset)
                return true;

            return Utils.TickCount + t + Game.Ping / 2 - m_lastAATick - m_Configuration.ExtraWindup - (m_Configuration.LegitMode && !ObjectManager.Player.IsMelee ? Math.Max(100, ObjectManager.Player.AttackDelay * 1000) : 0) * m_Configuration.LegitPercent / 100f >= 1000 / (ObjectManager.Player.GetAttackSpeed() * m_baseAttackSpeed);
        }

        /// <summary>
        /// Checks if player can move
        /// </summary>
        /// <returns>true if can move</returns>
        public bool CanMove(int t = 0)
        {
            if (!m_Move)
                return false;

            if (m_fnCanMove != null)
                return m_fnCanMove();

            if (Utility.IsNonCancelChamp(ObjectManager.Player.CharData.BaseSkinName))
                return Utils.TickCount - m_lastMoveTick >= 150 + m_rnd.Next(0, Game.Ping);

            return Utils.TickCount + t - 20 - m_lastAATick - m_Configuration.ExtraWindup - m_Configuration.MovementDelay >= 1000 / (ObjectManager.Player.GetAttackSpeed() * m_baseWindUp);
        }

        /// <summary>
        /// Checks if player can orbwalk given target
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>true if can orbwalk target</returns>
        public bool CanOrbwalkTarget(AttackableUnit target)
        {
            if (target == null)
                return false;

            if (m_fnCanOrbwalkTarget != null)
                return m_fnCanOrbwalkTarget(target);

            if (target.IsValidTarget())
            {
                if (target.Type == GameObjectType.obj_AI_Hero)
                {
                    Obj_AI_Hero hero = target as Obj_AI_Hero;
                    return ObjectManager.Player.Distance(hero.ServerPosition) - hero.BoundingRadius - hero.GetScalingRange() + 10 < Utility.GetAARange();
                }
                else
                    return (target.Type != GameObjectType.obj_AI_Turret || m_Configuration.AttackStructures) && ObjectManager.Player.Distance(target.Position) - target.BoundingRadius + 20 < Utility.GetAARange();
            }
            return false;
        }

        /// <summary>
        /// Checks if player can orbwalk given target in custom range
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="range">Custom range</param>
        /// <returns>true if can orbwalk target</returns>
        public bool CanOrbwalkTarget(AttackableUnit target, float range)
        {
            if (target.IsValidTarget())
            {
                if (target.Type == GameObjectType.obj_AI_Hero)
                {
                    Obj_AI_Hero hero = target as Obj_AI_Hero;
                    return ObjectManager.Player.Distance(hero.ServerPosition) - hero.BoundingRadius - hero.GetScalingRange() + 10 < range + ObjectManager.Player.BoundingRadius + ObjectManager.Player.GetScalingRange();
                }
                else
                    return ObjectManager.Player.Distance(target.Position) - target.BoundingRadius + 20 < range + ObjectManager.Player.BoundingRadius + ObjectManager.Player.GetScalingRange();
            }
            return false;
        }

        /// <summary>
        /// Checks if player can orbwalk given target from custom position
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="position">Custom position</param>
        /// <returns>true if can orbwalk target</returns>
        public bool CanOrbwalkTarget(AttackableUnit target, Vector3 position)
        {
            if (target.IsValidTarget())
            {
                if (target.Type == GameObjectType.obj_AI_Hero)
                {
                    Obj_AI_Hero hero = target as Obj_AI_Hero;
                    return position.Distance(hero.ServerPosition) - hero.BoundingRadius - hero.GetScalingRange() < Utility.GetAARange();
                }
                else
                    return position.Distance(target.Position) - target.BoundingRadius < Utility.GetAARange();
            }
            return false;
        }

        /// <summary>
        /// Orbwalk itself
        /// </summary>
        /// <param name="target">Target</param>
        public void Orbwalk(AttackableUnit target)
        {
            Orbwalk(target, OrbwalkingPoint);
        }

        /// <summary>
        /// Orbwalk itself
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="point">Orbwalk point</param>
        public void Orbwalk(AttackableUnit target, Vector3 point)
        {
            if (!m_attackInProgress)
            {
                if (CanOrbwalkTarget(target))
                {
                    if (CanAttack())
                    {
                        BeforeAttackArgs args = Events.FireBeforeAttack(this, target);
                        if (args.Process)
                            Attack(target);
                        else
                        {
                            if(CanMove() && ObjectManager.Player.GetAttackSpeed() < 2.51f)
                            {
                                if (m_Configuration.DontMoveInRange && target.Type == GameObjectType.obj_AI_Hero)
                                    return;

                                if ((m_Configuration.LegitMode && !ObjectManager.Player.IsMelee) || !m_Configuration.LegitMode)
                                    Move(point);
                            }
                        }
                    }
                    else if (CanMove() && ObjectManager.Player.GetAttackSpeed() < 2.51f)
                    {
                        if (m_Configuration.DontMoveInRange && target.Type == GameObjectType.obj_AI_Hero)
                            return;

                        if ((m_Configuration.LegitMode && !ObjectManager.Player.IsMelee) || !m_Configuration.LegitMode)
                            Move(point);
                    }
                }
                else
                {
                    Move(point);
                }
            }
        }

        public void RegisterCanAttack(Func<bool> fn)
        {
            m_fnCanAttack = fn;
        }

        public void RegisterCanMove(Func<bool> fn)
        {
            m_fnCanMove = fn;
        }

        public void RegisterCanOrbwalkTarget(Func<AttackableUnit, bool> fn)
        {
            m_fnCanOrbwalkTarget = fn;
        }

        public void RegisterShouldWait(Func<bool> fn)
        {
            m_fnShouldWait = fn;
        }

        public void UnRegisterCanAttack()
        {
            m_fnCanAttack = null;
        }

        public void UnRegisterCanMove()
        {
            m_fnCanMove = null;
        }

        public void UnRegisterCanOrbwalkTarget()
        {
            m_fnCanOrbwalkTarget = null;
        }

        public void UnRegisterShouldWait()
        {
            m_fnShouldWait = null;
        }

        private float GetAnimationTime()
        {
            return 1 / (ObjectManager.Player.GetAttackSpeed() * m_baseAttackSpeed);
        }

        private float GetWindupTime()
        {
            return 1 / (ObjectManager.Player.GetAttackSpeed() * m_baseWindUp) + m_Configuration.ExtraWindup;
        }

        private void Move(Vector3 pos)
        {
            if (!m_attackInProgress && (!CanAttack(60) || CanAttack()))
            {
                if (!m_Configuration.DontMoveMouseOver || ObjectManager.Player.Distance(Game.CursorPos, true) > ObjectManager.Player.BoundingRadius * ObjectManager.Player.BoundingRadius * 4)
                {
                    Vector3 playerPos = ObjectManager.Player.ServerPosition;
                    if (playerPos.Distance(pos, true) < m_Configuration.HoldAreaRadius * m_Configuration.HoldAreaRadius)
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.Stop, playerPos);
                        m_lastMoveTick = Utils.TickCount + m_rnd.Next(1, 70);
                        return;
                    }

                    if (ObjectManager.Player.Distance(pos, true) < 22500)
                        pos = playerPos.Extend(pos, (m_rnd.NextFloat(0.6f, 1) + 0.2f) * 400);


                    if (m_lastMoveTick + Math.Min(10, Game.Ping) < Utils.TickCount)
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, pos);
                        m_lastMoveTick = Utils.TickCount + m_rnd.Next(1, 70);
                    }
                }
            }
        }

        private void Attack(AttackableUnit target)
        {
            if (m_lastAttackTick + Math.Min(10, Game.Ping) < Utils.TickCount && !m_attackInProgress)
            {
                m_attackInProgress = true;
                ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                m_lastAttackTick = Utils.TickCount + m_rnd.Next(1, 150 + Game.Ping);
                m_lastAATick = Utils.TickCount + Game.Ping / 2;
            }
        }

        private void Magnet(AttackableUnit target)
        {
            if (!m_attackInProgress)
            {
                if (ObjectManager.Player.AttackRange <= m_Configuration.StickRange)
                {
                    if (!CanOrbwalkTarget(target) && target.IsValidTarget(m_Configuration.StickRange))
                    {
                        OrbwalkingPoint = target.Position;
                    }
                    else
                        OrbwalkingPoint = Vector3.Zero;
                }
                else
                    OrbwalkingPoint = Vector3.Zero;
            }
            else
                OrbwalkingPoint = Vector3.Zero;
        }

        private Obj_AI_Base GetLaneClearTarget()
        {
            Obj_AI_Base unkillableMinion = null;
            foreach (var minion in MinionManager.GetMinions(ObjectManager.Player.AttackRange + 250f).OrderByDescending(p => Damage.AutoAttack.GetDamage(p)))
            {
                float t = GetAnimationTime() + minion.Distance(ObjectManager.Player.ServerPosition) / Utility.GetProjectileSpeed() - 0.07f;
                if (CanOrbwalkTarget(minion))
                {
                    if (minion.Health - Damage.Prediction.GetPrediction(minion, t) > 2 * Damage.AutoAttack.GetDamage(minion) || Damage.Prediction.IsLastHitable(minion))
                        return minion;
                    else
                    {
                        if (Damage.Prediction.GetPrediction(minion, t) == 0 && Damage.Prediction.AggroCount(minion) == 0)
                            unkillableMinion = minion;
                    }
                }
            }
            var mob = GetJungleClearTarget();
            if (mob != null)
                return mob;

            return unkillableMinion;
        }

        private Obj_AI_Base GetJungleClearTarget()
        {
            Obj_AI_Base mob = null;
            if (Game.MapId == GameMapId.SummonersRift || Game.MapId == GameMapId.TwistedTreeline)
            {
                int mobPrio = 0;
                foreach (var minion in MinionManager.GetMinions(2000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).OrderBy(p => Damage.Prediction.GetPrediction(p, GetAnimationTime())))
                {
                    if (CanOrbwalkTarget(minion))
                    {
                        int prio = minion.GetJunglePriority();
                        if (minion.Health < Damage.AutoAttack.GetDamage(minion))
                            return minion;
                        else
                        {
                            if (mob == null)
                            {
                                mob = minion;
                                mobPrio = prio;
                            }
                            else if (prio < mobPrio)
                            {
                                mob = minion;
                                mobPrio = prio;
                            }
                        }
                    }
                }
            }
            return mob;
        }

        private int GetTowerMinion()
        {
            if (CanOrbwalkTarget(m_towerTarget))
            {
                var attack = Damage.Prediction.ContainsTowerAttack(m_towerTarget as Obj_AI_Base);
                if (attack == null)
                    return -2;

                float pred = HealthPrediction.GetHealthPrediction(attack.Target, (int)(attack.Delay + attack.Target.Distance(attack.Source.ServerPosition) / attack.ProjectileSpeed * 1000f), 0);
                float dmg = (float)ObjectManager.Player.GetAutoAttackDamage(attack.Target);
                float health = pred - attack.Damage;

                if (health < 0)
                    return 2;

                if (health > 0 && dmg > health)
                    return -1;

                float t = (GetWindupTime() + attack.Target.Distance(ObjectManager.Player.ServerPosition) / Utility.GetProjectileSpeed() - 0.07f) * 1000f;
                if (attack.Delay + attack.Target.Distance(attack.Source.ServerPosition) / attack.ProjectileSpeed * 1000f > t)
                {
                    pred = HealthPrediction.GetHealthPrediction(attack.Target, (int)(attack.Delay + attack.Target.Distance(attack.Source.ServerPosition) / attack.ProjectileSpeed * 1000f), 0);
                    health = pred - attack.Damage;
                    if (health > 0 && health - dmg > 0 && dmg > health - dmg)
                        return 0;
                }
                if (health >= 0 && health > dmg * 2 + attack.Damage)
                    return 2;
            }
            return -1;
        }

        private Obj_AI_Base FindKillableMinion()
        {
            if (m_Configuration.SupportMode)
                return null;

            foreach (var minion in MinionManager.GetMinions(ObjectManager.Player.AttackRange + 250f).OrderBy(p => Damage.AutoAttack.GetDamage(p, true)).ThenByDescending(q => Damage.Prediction.AggroCount(q)))
            {
                if (CanOrbwalkTarget(minion) && Damage.Prediction.IsLastHitable(minion))
                    return minion;
            }
            return null;
        }

        public bool ShouldWait()
        {
            if (m_fnShouldWait != null)
                return m_fnShouldWait();

            if (m_towerTarget != null && m_towerTarget.IsValidTarget() && CanOrbwalkTarget(m_towerTarget) && GetTowerMinion() != -1)
                return true;
            return
                ObjectManager.Get<Obj_AI_Minion>()
                    .Any(
                        minion =>
                            (minion.IsValidTarget() && minion.Team != GameObjectTeam.Neutral &&
                            Utility.InAARange(minion) && MinionManager.IsMinion(minion, false) &&
                             (minion.Health - Damage.Prediction.GetPrediction(minion, ObjectManager.Player.AttackDelay * 1000f * 2f, true) <= Damage.AutoAttack.GetDamage(minion, true) * (int)(Math.Ceiling(Damage.Prediction.AggroCount(minion) / 2f)))));
        }


        private AttackableUnit GetTarget()
        {
            bool wait = ShouldWait();
            if (ActiveMode == Mode.LaneClear || ActiveMode == Mode.LastHit || ActiveMode == Mode.Mixed)
            {
                if (m_towerTarget != null && m_towerTarget.IsValidTarget() && CanOrbwalkTarget(m_towerTarget, ObjectManager.Player.AttackRange + 150f))
                {
                    int x = GetTowerMinion();
                    if (x == -1)
                        return FindKillableMinion();
                    if (x == 0)
                        return m_towerTarget;
                }
                var killableMinion = FindKillableMinion();
                if (killableMinion != null)
                    return killableMinion;
            }

            if (m_forcedTarget != null && m_forcedTarget.IsValidTarget() && Utility.InAARange(m_forcedTarget))
                return m_forcedTarget;

            //buildings
            if (ActiveMode == Mode.LaneClear && m_Configuration.AttackStructures && !wait)
            {
                /* turrets */
                foreach (var turret in
                    ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsValidTarget() && Utility.InAARange(t)))
                {
                    return turret;
                }

                /* inhibitor */
                foreach (var turret in
                    ObjectManager.Get<Obj_BarracksDampener>().Where(t => t.IsValidTarget() && Utility.InAARange(t)))
                {
                    return turret;
                }

                /* nexus */
                foreach (var nexus in
                    ObjectManager.Get<Obj_HQ>().Where(t => t.IsValidTarget() && Utility.InAARange(t)))
                {
                    return nexus;
                }
            }

            //champions
            if (ActiveMode != Mode.LastHit)
            {
                if (ActiveMode == Mode.LaneClear && wait)
                    return null;

                if ((ActiveMode == Mode.LaneClear && !m_Configuration.DontAttackChampWhileLaneClear) || ActiveMode == Mode.Combo || ActiveMode == Mode.Mixed)
                {
                    float range = -1;
                    range = (ObjectManager.Player.IsMelee && m_Configuration.MagnetMelee && m_Configuration.StickRange > ObjectManager.Player.AttackRange) ? m_Configuration.StickRange : -1;
                    if (ObjectManager.Player.CharData.BaseSkinName == "Azir")
                        range = 950f;
                    var target = LeagueSharp.Common.TargetSelector.GetTarget(range, LeagueSharp.Common.TargetSelector.DamageType.Physical);
                    if (target.IsValidTarget() && (Utility.InAARange(target) || (ActiveMode != Mode.LaneClear && ObjectManager.Player.IsMelee && m_Configuration.MagnetMelee && target.IsValidTarget(m_Configuration.StickRange))))
                        return target;
                }
            }

            if (!wait)
            {
                if (ActiveMode == Mode.LaneClear)
                {
                    var minion = GetLaneClearTarget();
                    if (minion != null)
                        return minion;
                }
            }
            return null;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (ActiveMode == Mode.None || ObjectManager.Player.IsCastingInterruptableSpell(true) || ObjectManager.Player.IsDead)
                return;

            if (Utils.TickCount - m_lastAttackTick < 70 + Math.Min(60, Game.Ping))
                return;

            if (CanMove() && m_attackInProgress)
                LeagueSharp.Common.Utility.DelayAction.Add(Game.Ping, () => m_attackInProgress = false);

            var t = GetTarget();
            m_lastTarget = t;

            if (ObjectManager.Player.IsMelee && m_Configuration.MagnetMelee && t is Obj_AI_Hero)
                Magnet(t);
            else
                OrbwalkingPoint = Vector3.Zero;

            Orbwalk(t);
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (Utility.IsAutoAttack(args.SData.Name))
                {
                    OnAttackArgs onAttackArgs = Events.FireOnAttack(this, args.Target as AttackableUnit);
                    if (!onAttackArgs.Cancel)
                    {
                        m_lastAATick = Utils.TickCount - Game.Ping / 2;
                        m_lastWindUpTime = (int)(sender.AttackCastDelay * 1000) + 1;
                        m_lastAttackCooldown = (int)(sender.AttackDelay * 1000) + 1;
                        m_lastAttackCompletesAt = m_lastAATick + m_lastWindUpTime;
                        m_lastAttackPos = ObjectManager.Player.ServerPosition.To2D();
                        m_attackInProgress = true;
                    }
                    if (m_baseAttackSpeed == 0.5f)
                    {
                        m_baseWindUp = 1f / (sender.AttackCastDelay * ObjectManager.Player.GetAttackSpeed());
                        m_baseAttackSpeed = 1f / (sender.AttackDelay * ObjectManager.Player.GetAttackSpeed());
                    }
                }
                else if (Utility.IsAutoAttackReset(args.SData.Name))
                {
                    ResetAATimer();
                }
                else if (!Utility.IsAutoAttackReset(args.SData.Name))
                {
                    if (m_attackInProgress)
                        ResetAATimer();
                }
                else if (args.SData.Name == "AspectOfTheCougar")
                {
                    ResetOrbwalkValues();
                }
            }
            else
            {
                if (sender.Type == GameObjectType.obj_AI_Turret && args.Target.Type == GameObjectType.obj_AI_Minion && sender.Team == ObjectManager.Player.Team && args.Target.Position.Distance(ObjectManager.Player.ServerPosition) <= 2000)
                    m_towerTarget = args.Target as AttackableUnit;
            }
        }

        private void Obj_AI_Base_OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (Utility.IsAutoAttack(args.SData.Name))
                {
                    m_lastWindUpTick = Utils.TickCount;
                    m_attackInProgress = false;
                    m_attackReset = false;
                    Events.FireAfterAttack(this, args.Target as AttackableUnit);
                }
            }
        }

        private void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Order == GameObjectOrder.MoveTo || args.Order == GameObjectOrder.Stop)
                    m_attackInProgress = false;
            }
        }

        private void Obj_AI_Base_OnPlayAnimation(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && sender.CharData.BaseSkinName == "Rengar")
            {
                Events.FireOnAttack(this, m_lastTarget);
                m_lastAATick = Utils.TickCount - Game.Ping / 2;
                m_lastWindUpTime = (int)(sender.AttackCastDelay * 1000) + 1;
                m_lastAttackCooldown = (int)(sender.AttackDelay * 1000) + 1;
                m_lastAttackCompletesAt = m_lastAATick + m_lastWindUpTime;
                m_lastAttackPos = ObjectManager.Player.ServerPosition.To2D();
                m_attackInProgress = true;

                LeagueSharp.Common.Utility.DelayAction.Add((int)(sender.AttackCastDelay * 1000 + m_Configuration.ExtraWindup + 100), () =>
                {
                    m_lastWindUpTick = Utils.TickCount;
                    m_attackInProgress = false;
                    Events.FireAfterAttack(this, m_lastTarget);
                });
            }
        }

        private void Obj_AI_Base_OnBuffRemove(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args)
        {
            if (sender.IsMe)
            {
                string buffname = args.Buff.Name.ToLower();
                if (buffname == "swainmetamorphism" || buffname == "gnartransform" || buffname == "rengarqbase" || buffname == "rengarqemp")
                    ResetOrbwalkValues();
            }
        }

        private void Obj_AI_Base_OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if (sender.IsMe)
            {
                string buffname = args.Buff.Name.ToLower();
                if (buffname == "jaycestancegun" || buffname == "jaycestancehammer" || buffname == "swainmetamorphism" || buffname == "gnartransform" || buffname == "rengarqbase" || buffname == "rengarqemp")
                    ResetOrbwalkValues();
            }
        }
    }
}
