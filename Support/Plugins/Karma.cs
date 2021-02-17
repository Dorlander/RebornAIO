using System;
using LeagueSharp;
using LeagueSharp.Common;
using Support.Util;
using ActiveGapcloser = Support.Util.ActiveGapcloser;

namespace Support.Plugins
{
    public class Karma : PluginBase
    {
        public Karma()
        {
            Q = new Spell(SpellSlot.Q, 1050);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 0);

            Q.SetSkillshot(0.25f, 60f, 1700f, true, SkillshotType.SkillshotLine);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.CastCheck(Target, "Combo.Q") && R.IsReady() && Q.GetPrediction(Target).Hitchance >= HitChance.High &&
                    Q.GetPrediction(Target).CollisionObjects.Count == 0 &&
                    Q.GetPrediction(Target).UnitPosition.CountEnemiesInRange(250) >=
                    ConfigValue<Slider>("Misc.Q.Count").Value)
                {
                    R.CastOnUnit(Player);
                    Utility.DelayAction.Add(200, () => Q.Cast(Target));
                }
                if (Q.CastCheck(Target, "Combo.Q"))
                {
                    Q.Cast(Target);
                }

                if (W.CastCheck(Target, "Combo.W") && R.IsReady() &&
                    Player.HealthPercent <= ConfigValue<Slider>("Misc.W.Hp").Value)
                {
                    R.CastOnUnit(Player);
                    Utility.DelayAction.Add(200, () => W.CastOnUnit(Target));
                }
                if (W.CastCheck(Target, "Combo.W"))
                {
                    W.CastOnUnit(Target);
                }

                if (E.IsReady() && R.IsReady() &&
                    Helpers.AllyInRange(600).Count >= ConfigValue<Slider>("Misc.E.Count").Value)
                {
                    R.CastOnUnit(Player);
                    Utility.DelayAction.Add(200, () => E.CastOnUnit(Player));
                }
            }

            if (HarassMode)
            {
                if (Q.CastCheck(Target, "Harass.Q") && R.IsReady() &&
                    Q.GetPrediction(Target).Hitchance >= HitChance.High &&
                    Q.GetPrediction(Target).CollisionObjects.Count == 0 &&
                    Q.GetPrediction(Target).UnitPosition.CountEnemiesInRange(250) >=
                    ConfigValue<Slider>("Misc.Q.Count").Value)
                {
                    R.CastOnUnit(Player);
                    Utility.DelayAction.Add(200, () => Q.Cast(Target));
                }
                if (Q.CastCheck(Target, "Harass.Q"))
                {
                    Q.Cast(Target);
                }

                if (E.IsReady() && R.IsReady() &&
                    Helpers.AllyInRange(600).Count >= ConfigValue<Slider>("Misc.E.Count").Value)
                {
                    R.CastOnUnit(Player);
                    Utility.DelayAction.Add(200, () => E.CastOnUnit(Player));
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

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "Use Q", true);
            config.AddBool("Combo.W", "Use W", true);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddSlider("Misc.Q.Count", "R/Q Enemy in Range", 2, 0, 4);
            config.AddSlider("Misc.W.Hp", "R/W HP", 40, 1, 100);
            config.AddSlider("Misc.E.Count", "R/E Ally in Range", 3, 0, 4);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.Q", "Use Q", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Gapcloser.W", "Use W to Interrupt Gapcloser", true);
        }
    }
}