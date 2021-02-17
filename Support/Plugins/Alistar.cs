using System;
using LeagueSharp;
using LeagueSharp.Common;
using Support.Util;
using ActiveGapcloser = Support.Util.ActiveGapcloser;

namespace Support.Plugins
{
    public class Alistar : PluginBase
    {
        public Alistar()
        {
            Q = new Spell(SpellSlot.Q, 365);
            W = new Spell(SpellSlot.W, 650);
            E = new Spell(SpellSlot.E, 575);
            R = new Spell(SpellSlot.R, 0);

            W.SetTargetted(0.5f, float.MaxValue);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.CastCheck(Target, "Combo.Q"))
                {
                    Q.Cast();
                }

                if (Q.IsReady() && W.CastCheck(Target, "Combo.W"))
                {
                    W.CastOnUnit(Target);
                    var jumpTime = Math.Max(0, Player.Distance(Target) - 500)*10/25 + 25;
                    Utility.DelayAction.Add((int) jumpTime, () => Q.Cast());
                }

                var ally = Helpers.AllyBelowHp(ConfigValue<Slider>("Combo.E.Health").Value, E.Range);
                if (E.CastCheck(ally, "Combo.E", true, false))
                {
                    E.Cast();
                }
            }

            if (HarassMode)
            {
                if (Q.CastCheck(Target, "Harass.Q"))
                {
                    Q.Cast();
                }

                var ally = Helpers.AllyBelowHp(ConfigValue<Slider>("Harass.E.Health").Value, E.Range);
                if (E.CastCheck(ally, "Harass.E", true, false))
                {
                    E.Cast();
                }
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (W.CastCheck(gapcloser.Sender, "Gapcloser.W"))
            {
                W.CastOnUnit(gapcloser.Sender);
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }

            if (Q.CastCheck(target, "Interrupt.Q"))
            {
                Q.Cast();
            }

            if (W.CastCheck(target, "Interrupt.W"))
            {
                W.CastOnUnit(target);
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "Use Q", true);
            config.AddBool("Combo.W", "Use WQ", true);
            config.AddBool("Combo.E", "Use E", true);
            config.AddSlider("Combo.E.Health", "Health to Heal", 20, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.Q", "Use Q", true);
            config.AddBool("Harass.E", "Use E", true);
            config.AddSlider("Harass.E.Health", "Health to Heal", 20, 1, 100);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Gapcloser.W", "Use W to Interrupt Gapcloser", true);

            config.AddBool("Interrupt.Q", "Use Q to Interrupt Spells", true);
            config.AddBool("Interrupt.W", "Use W to Interrupt Spells", true);
        }
    }
}