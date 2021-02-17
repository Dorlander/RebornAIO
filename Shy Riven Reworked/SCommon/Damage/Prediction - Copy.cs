using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SCommon.Damage
{
    public static class Prediction
    {
        private static readonly Dictionary<int, PredictedDamage> ActiveAttacks;
        private static Random s_Rnd;
        static Prediction()
        {
            ActiveAttacks = new Dictionary<int, PredictedDamage>();
            s_Rnd = new Random();
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Obj_AI_Base.OnDoCast += Obj_AI_Base_OnDoCast;
            Obj_AI_Base.OnDamage += Obj_AI_Base_OnDamage;
            Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;
            Game.OnUpdate += Game_OnUpdate;
        }
       
        /// <summary>
        /// Checks if given unit is last hitable
        /// </summary>
        /// <param name="unit">Unit</param>
        /// <returns>true if last hitable</returns>
        public static bool IsLastHitable(Obj_AI_Base unit, float extraWindup = 0)
        {
            float health = unit.Health - GetPrediction(unit, (unit.Distance(ObjectManager.Player.ServerPosition) / Orbwalking.Utility.GetProjectileSpeed() + ObjectManager.Player.AttackCastDelay) * 1000f);
            return health <= AutoAttack.GetDamage(unit);
        }

        /// <summary>
        /// Checks if given unit is two hitable
        /// </summary>
        /// <param name="unit">Unit</param>
        /// <returns>true if two hitable</returns>
        public static bool IsTwoHitable(Obj_AI_Base unit)
        {
            float health = unit.Health - GetPrediction(unit, (unit.Distance(ObjectManager.Player.ServerPosition) / Orbwalking.Utility.GetProjectileSpeed() + ObjectManager.Player.AttackCastDelay - 0.07f) * 2 * 1000f);
            return health <= AutoAttack.GetDamage(unit);
        }

        /// <summary>
        /// Checks if given unit is unkillable
        /// </summary>
        /// <param name="unit">Unit</param>
        /// <returns>true if unkillable</returns>
        public static bool IsUnkillable(Obj_AI_Base unit, int t = 0)
        {
            float health = unit.Health - GetPrediction(unit, ((unit.Distance(ObjectManager.Player.ServerPosition) / Orbwalking.Utility.GetProjectileSpeed() + ObjectManager.Player.AttackCastDelay - 0.07f) * 2f + t / 1000f) * 1000f);
            return health <= AutoAttack.GetDamage(unit);
        }

        /// <summary>
        /// Gets damage prediction to given unit
        /// </summary>
        /// <param name="unit">Unit</param>
        /// <param name="t">t in seconds</param>
        /// <returns></returns>
        public static float GetPrediction(Obj_AI_Base unit, float t)
        {
            float dmg = 0.0f;
            foreach (var attack in ActiveAttacks.Values)
            {
                if (attack.Source.IsValidTarget(float.MaxValue, false) && attack.Target.IsValidTarget(float.MaxValue, false))
                {
                    if (attack.Target.NetworkId == unit.NetworkId)
                    {
                        float d = attack.Target.Distance(attack.Source.ServerPosition);
                        float maxTravelTime = d / attack.ProjectileSpeed * 1000f;
                        float arriveTime = float.MaxValue;
                        if (!attack.Damaged)
                            arriveTime = Utils.TickCount - (attack.StartTick + attack.Delay + maxTravelTime);
                        else
                            arriveTime = Utils.TickCount - (attack.StartTick + attack.AnimationTime + attack.Delay + maxTravelTime);

                        if (arriveTime <= t) //if minion's missile arrives earlier than me
                        {
                            dmg += attack.Damage; //add minion's dmg
                            float totalAttackNumCanFired = t / (attack.AnimationTime) - 1;
                            Game.PrintChat("total attacks can be fired {0} in {1}ms", totalAttackNumCanFired, t);
                            while (t > 0 && totalAttackNumCanFired >= 1)
                            {
                                dmg += attack.Damage;
                                t -= attack.AnimationTime + maxTravelTime;
                                totalAttackNumCanFired--;
                            }
                            //if (totalAttackNumCanFired >= attack.Delay + maxTravelTime)
                            //    dmg += attack.Damage;
                        }
                    }
                }
            }
            if (dmg < 0)
                dmg = 0;
            return dmg;
        }

        public static int AggroCount(Obj_AI_Base unit)
        {
            return ActiveAttacks.Values.Count(p => p != null && p.Target != null && p.Target.NetworkId == unit.NetworkId);
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsValidTarget(3000, false) || sender.Team != ObjectManager.Player.Team || sender is Obj_AI_Hero || !Orbwalking.Utility.IsAutoAttack(args.SData.Name) || !(args.Target is Obj_AI_Base))
                return;

            var target = (Obj_AI_Base)args.Target;
            ActiveAttacks.Remove(sender.NetworkId);

            var attackData = new PredictedDamage(
                sender,
                target,
                Utils.TickCount,
                sender.AttackCastDelay * 1000f,
                sender.AttackDelay * 1000f,
                sender.IsMelee() ? int.MaxValue : (int)args.SData.MissileSpeed,
                (float)sender.GetAutoAttackDamage(target, true));
            ActiveAttacks.Add(sender.NetworkId, attackData);
        }

        private static void Obj_AI_Base_OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (ActiveAttacks.ContainsKey(sender.NetworkId))
                ActiveAttacks[sender.NetworkId].Processed = true;
        }

        private static void Obj_AI_Base_OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            if (ActiveAttacks.ContainsKey(args.SourceNetworkId))
                ActiveAttacks[args.SourceNetworkId].Damaged = true;
        }

        private static void Obj_AI_Base_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (args.Animation == "Death" && ActiveAttacks.ContainsKey(sender.NetworkId))
                ActiveAttacks.Remove(sender.NetworkId);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            ActiveAttacks.ToList()
                .Where(pair => pair.Value.StartTick < Utils.GameTimeTickCount - 3000)
                .ToList()
                .ForEach(pair => ActiveAttacks.Remove(pair.Key));
        }

        #region predicted damage class
        /// <summary>
        /// Represetns predicted damage.
        /// </summary>
        private class PredictedDamage
        {
            /// <summary>
            /// The animation time
            /// </summary>
            public readonly float AnimationTime;

            /// <summary>
            /// Gets or sets the damage.
            /// </summary>
            /// <value>
            /// The damage.
            /// </value>
            public float Damage { get; private set; }

            /// <summary>
            /// Gets or sets the delay.
            /// </summary>
            /// <value>
            /// The delay.
            /// </value>
            public float Delay { get; private set; }

            /// <summary>
            /// Gets or sets the projectile speed.
            /// </summary>
            /// <value>
            /// The projectile speed.
            /// </value>
            public int ProjectileSpeed { get; private set; }

            /// <summary>
            /// Gets or sets the source.
            /// </summary>
            /// <value>
            /// The source.
            /// </value>
            public Obj_AI_Base Source { get; private set; }

            /// <summary>
            /// Gets or sets the start tick.
            /// </summary>
            /// <value>
            /// The start tick.
            /// </value>
            public int StartTick { get; internal set; }

            /// <summary>
            /// Gets or sets the target.
            /// </summary>
            /// <value>
            /// The target.
            /// </value>
            public Obj_AI_Base Target { get; private set; }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="PredictedDamage"/> is processed.
            /// </summary>
            /// <value>
            ///   <c>true</c> if processed; otherwise, <c>false</c>.
            /// </value>
            public bool Processed { get; internal set; }

            public bool Damaged { get; internal set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="PredictedDamage"/> class.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <param name="target">The target.</param>
            /// <param name="startTick">The start tick.</param>
            /// <param name="delay">The delay.</param>
            /// <param name="animationTime">The animation time.</param>
            /// <param name="projectileSpeed">The projectile speed.</param>
            /// <param name="damage">The damage.</param>
            public PredictedDamage(Obj_AI_Base source,
                Obj_AI_Base target,
                int startTick,
                float delay,
                float animationTime,
                int projectileSpeed,
                float damage)
            {
                Source = source;
                Target = target;
                StartTick = startTick;
                Delay = delay;
                ProjectileSpeed = projectileSpeed;
                Damage = damage;
                AnimationTime = animationTime;
                Damaged = false;
            }
        }
        #endregion
    }
}
