using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Support.Util;
using ActiveGapcloser = Support.Util.ActiveGapcloser;

namespace Support.Plugins
{
    public class Blitzcrank : PluginBase
    {
        public Blitzcrank()
        {
            Q = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, AttackRange);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);
        }

        private bool BlockQ
        {
            get
            {
                if (!Q.IsReady())
                {
                    return true;
                }

                if (!ConfigValue<bool>("Misc.Q.Block"))
                {
                    return false;
                }

                if (!Target.IsValidTarget())
                {
                    return true;
                }

                if (Target.HasBuff("BlackShield"))
                {
                    return true;
                }

                if (Helpers.AllyInRange(1200)
                    .Any(ally => ally.Distance(Target) < ally.AttackRange + ally.BoundingRadius))
                {
                    return true;
                }

                return Player.Distance(Target) < ConfigValue<Slider>("Misc.Q.Block.Distance").Value;
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            try
            {
                if (ComboMode)
                {
                    if (Q.CastCheck(Target, "ComboQ") && !BlockQ)
                    {
                        Q.Cast(Target);
                    }

                    if (E.CastCheck(Target))
                    {
                        if (E.Cast())
                        {
                            Orbwalking.ResetAutoAttackTimer();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                        }
                    }

                    if (E.IsReady() && Target.IsValidTarget() && Target.HasBuff("RocketGrab"))
                    {
                        if (E.Cast())
                        {
                            Orbwalking.ResetAutoAttackTimer();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                        }
                    }

                    if (W.IsReady() && ConfigValue<bool>("ComboW") && Player.CountEnemiesInRange(1500) > 0)
                    {
                        W.Cast();
                    }

                    if (R.CastCheck(Target, "ComboR"))
                    {
                        if (Helpers.EnemyInRange(ConfigValue<Slider>("ComboCountR").Value, R.Range))
                        {
                            R.Cast();
                        }
                    }
                }

                if (HarassMode)
                {
                    if (Q.CastCheck(Target, "HarassQ") && !BlockQ)
                    {
                        Q.Cast(Target);
                    }

                    if (E.CastCheck(Target))
                    {
                        if (E.Cast())
                        {
                            Orbwalking.ResetAutoAttackTimer();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                        }
                    }

                    if (E.IsReady() && Target.IsValidTarget() && Target.HasBuff("RocketGrab"))
                    {
                        if (E.Cast())
                        {
                            Orbwalking.ResetAutoAttackTimer();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
            {
                return;
            }

            if (!target.IsValid<Obj_AI_Hero>() && !target.Name.ToLower().Contains("ward"))
            {
                return;
            }

            if (!E.IsReady())
            {
                return;
            }

            if (E.Cast())
            {
                Orbwalking.ResetAutoAttackTimer();
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (E.CastCheck(gapcloser.Sender, "GapcloserE"))
            {
                if (E.Cast())
                {
                    Orbwalking.ResetAutoAttackTimer();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, gapcloser.Sender);
                }
            }

            if (R.CastCheck(gapcloser.Sender, "GapcloserR"))
            {
                R.Cast();
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }

            if (E.CastCheck(target, "InterruptE"))
            {
                if (E.Cast())
                {
                    Orbwalking.ResetAutoAttackTimer();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }
            }

            if (Q.CastCheck(Target, "InterruptQ"))
            {
                Q.Cast(target);
            }

            if (R.CastCheck(target, "InterruptR"))
            {
                R.Cast();
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
            config.AddBool("ComboR", "Use R", true);
            config.AddSlider("ComboCountR", "Targets in range to Ult", 2, 1, 5);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddBool("Misc.Q.Block", "Block Q on close Targets", true);
            config.AddSlider("Misc.Q.Block.Distance", "Q Block Distance", 400, 0, 800);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "Use Q", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserE", "Use E to Interrupt Gapcloser", true);
            config.AddBool("GapcloserR", "Use R to Interrupt Gapcloser", true);
            config.AddBool("InterruptQ", "Use Q to Interrupt Spells", true);
            config.AddBool("InterruptE", "Use E to Interrupt Spells", true);
            config.AddBool("InterruptR", "Use R to Interrupt Spells", true);
        }
    }
}