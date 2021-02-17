using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Support.Util;
using ActiveGapcloser = Support.Util.ActiveGapcloser;

namespace Support.Plugins
{
    public class Nunu : PluginBase
    {
        private int last = 0;

        public Nunu()
        {
            Q = new Spell(SpellSlot.Q, 125);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R, 650);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.IsReady() && ConfigValue<bool>("Combo.Q") &&
                    Player.HealthPercent < ConfigValue<Slider>("Combo.Q.Health").Value)
                {
                    var minion = MinionManager.GetMinions(Player.Position, Q.Range).FirstOrDefault();
                    if (minion.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(minion);
                    }
                }

                var allys = Helpers.AllyInRange(W.Range).OrderByDescending(h => h.FlatPhysicalDamageMod).ToList();
                if (W.IsReady() && allys.Count > 0 && ConfigValue<bool>("Combo.W"))
                {
                    W.CastOnUnit(allys.FirstOrDefault());
                }

                if (W.IsReady() && Target.IsValidTarget(AttackRange) && ConfigValue<bool>("Combo.W"))
                {
                    W.CastOnUnit(Player);
                }

                if (E.IsReady() && Target.IsValidTarget(E.Range) && ConfigValue<bool>("Combo.E"))
                {
                    E.CastOnUnit(Target);
                }
            }

            if (HarassMode)
            {
                if (Q.IsReady() && ConfigValue<bool>("Harass.Q") &&
                    Player.HealthPercent < ConfigValue<Slider>("Harass.Q.Health").Value)
                {
                    var minion = MinionManager.GetMinions(Player.Position, Q.Range).FirstOrDefault();
                    if (minion.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(minion);
                    }
                }

                var allys = Helpers.AllyInRange(W.Range).OrderByDescending(h => h.FlatPhysicalDamageMod).ToList();
                if (W.IsReady() && allys.Count > 0 && ConfigValue<bool>("Harass.W"))
                {
                    W.CastOnUnit(allys.FirstOrDefault());
                }

                if (W.IsReady() && Target.IsValidTarget(AttackRange) && ConfigValue<bool>("Harass.W"))
                {
                    W.CastOnUnit(Player);
                }

                if (E.IsReady() && Target.IsValidTarget(E.Range) && ConfigValue<bool>("Harass.E"))
                {
                    E.CastOnUnit(Target);
                }
            }

            if (ConfigValue<StringList>("Misc.Laugh").SelectedValue == "ON" && Player.CountEnemiesInRange(2000) > 0 && last + 4200 < Environment.TickCount)
            {
                Game.PrintChat("Laugh");
                last = Environment.TickCount;
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (E.CastCheck(gapcloser.Sender, "Gapcloser.E"))
            {
                E.CastOnUnit(gapcloser.Sender);

                if (W.IsReady())
                {
                    W.CastOnUnit(Player);
                }
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "Use Q", true);
            config.AddBool("Combo.W", "Use W", true);
            config.AddBool("Combo.E", "Use E", true);
            config.AddSlider("Combo.Q.Health", "Consume below %HP", 50, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.Q", "Use Q", true);
            config.AddBool("Harass.W", "Use W", false);
            config.AddBool("Harass.E", "Use E", true);
            config.AddSlider("Harass.Q.Health", "Consume below %HP", 50, 1, 100);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddList("Misc.Laugh", "Laugh Emote", new[] {"OFF", "ON"});
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Gapcloser.E", "Use E to Interrupt Gapcloser", true);
        }
    }
}