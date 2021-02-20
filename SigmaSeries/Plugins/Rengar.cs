using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Version = System.Version;


namespace SigmaSeries.Plugins
{
    public class Rengar : PluginBase
    {

        public Rengar()
            : base(new Version(0, 1, 1))
        {

            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 500);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R, 0);
            Q.SetTargetted(0.5f, 10000);
            W.SetTargetted(0.5f, 10000);
            E.SetSkillshot(0.5f, 70, 1500, true, SkillshotType.SkillshotLine);
        }

        public override void ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseItems", "Use Items").SetValue(true));
            config.AddItem(new MenuItem("sep", "-- Smart Stack Settings"));
            config.AddItem(new MenuItem("smart5Stack", "Use Stacks Smartly").SetValue(false));
            config.AddItem(new MenuItem("q1", "Q if AA Able.").SetValue(false));
            config.AddItem(new MenuItem("w1", "W Below % Hp").SetValue(false));
            config.AddItem(new MenuItem("wSlide", "% HP").SetValue(new Slider(15, 0, 100)));
            config.AddItem(new MenuItem("e1", "E if Distance > AA Range").SetValue(false));
            config.AddItem(new MenuItem("sep2", "-- No Smart Stack Settings"));
            config.AddItem(new MenuItem("stackPriority", "5 Stack Priority").SetValue(new StringList(new[] { "Q", "W", "E" }, 0)));
            config.AddItem(new MenuItem("tripleQ", "Triple Q!").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Press)));
        }

        public override void HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
        }

        public override void FarmMenu(Menu config)
        {
            config.AddItem(new MenuItem("sep22", "-- Freeze Settings"));
            config.AddItem(new MenuItem("qf", "Q").SetValue(false));
            config.AddItem(new MenuItem("wf", "W").SetValue(false));
            config.AddItem(new MenuItem("ef", "E").SetValue(false));
            config.AddItem(new MenuItem("sep33", "-- Lane Clear Settings"));
            config.AddItem(new MenuItem("51", "Use 5th Passive Stack").SetValue(false));
            config.AddItem(new MenuItem("waveClearPriority", "5 Stack Priority").SetValue(new StringList(new[] { "Q", "W" }, 0)));
            config.AddItem(new MenuItem("ql", "Q").SetValue(false));
            config.AddItem(new MenuItem("wl", "W").SetValue(false));
            config.AddItem(new MenuItem("el", "E").SetValue(false));
            config.AddItem(new MenuItem("sep", "-- Jungle Settings"));
            config.AddItem(new MenuItem("JungleActive", "Jungle Clear!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            config.AddItem(new MenuItem("52", "Use 5th Passive Stack").SetValue(false));
            config.AddItem(new MenuItem("jungPriority", "5 Stack Priority").SetValue(new StringList(new[] { "Q", "W" }, 0)));
            config.AddItem(new MenuItem("qj", "Q").SetValue(false));
            config.AddItem(new MenuItem("wj", "W").SetValue(false));
            config.AddItem(new MenuItem("ej", "E").SetValue(false));
        }


        public override void BonusMenu(Menu config)
        {
            config.AddItem(new MenuItem("packetCast", "Packet Cast").SetValue(true));
        }


        public override void OnUpdate(EventArgs args)
        {
            if (Config.Item("tripleQ").GetValue<KeyBind>().Active)
            {
                combo(true);
            }

            if (ComboActive)
            {
                combo(false);
            }

            if (HarassActive)
            {
                harass();
            }

            if (Config.Item("JungleActive").GetValue<KeyBind>().Active)
            {
                Jungle();
            }

            if (WaveClearActive)
            {
                WaveClear();
            }
            if (FreezeActive)
            {
                Freeze();
            }
        }

        public void harass()
        {
            var pCast = Config.Item("packetCast").GetValue<bool>();
            var useW = Config.Item("UseWHarass").GetValue<bool>();
            var useE = Config.Item("UseEHarass").GetValue<bool>();
            var stackPrior = Config.Item("stackPriority").GetValue<StringList>().SelectedIndex;
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (eTarget != null)
            {
                if (E.IsReady())
                {
                    if (eTarget.IsValidTarget(E.Range) && E.IsReady() && useE && Player.Mana > 5 || eTarget.IsValidTarget(E.Range) && E.IsReady() && useE && stackPrior == 2 || Vector3.Distance(eTarget.Position, Player.Position) > Orbwalking.GetRealAutoAttackRange(Player) + 100 && eTarget.IsValidTarget(E.Range) && E.IsReady())
                    {
                        E.Cast(eTarget, true);
                        return;
                    }
                }
                if (W.IsReady())
                {
                    if (eTarget.IsValidTarget(W.Range) && W.IsReady() && useW && Player.Mana > 5 || eTarget.IsValidTarget(W.Range) && W.IsReady() && useW && stackPrior == 1)
                    {
                        W.CastOnUnit(Player, pCast);;
                        return;
                    }
                }
                //castItems(eTarget);
            }
        }

        public void combo(bool is3Q)
        {
            var pCast = Config.Item("packetCast").GetValue<bool>();
            var smartMode = Config.Item("smart5Stack").GetValue<bool>();
            var smartHP = Config.Item("wSlide").GetValue<Slider>().Value;
            var smartQ = Config.Item("q1").GetValue<bool>();
            var smartW = Config.Item("w1").GetValue<bool>();
            var smartE = Config.Item("e1").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var stackPrior = Config.Item("stackPriority").GetValue<StringList>().SelectedIndex;
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            var aaTarget = TargetSelector.GetTarget(Orbwalking.GetRealAutoAttackRange(Player) + 200, TargetSelector.DamageType.Physical);

            
            if (eTarget != null)
            {
                if (is3Q)
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, aaTarget);
                }
                if (smartMode && Player.Mana == 5 && !is3Q)
                {
                    if (smartW && Player.Health / Player.MaxHealth * 100 < smartHP)
                    {
                        W.CastOnUnit(Player, pCast);;
                    }
                    else if (smartQ && Player.Distance(eTarget) <= 300)
                    {
                        Q.CastOnUnit(Player, pCast);;
                    }
                    else if (smartE && Player.Distance(eTarget) > Orbwalking.GetRealAutoAttackRange(Player))
                    {
                        E.Cast(eTarget, true);
                    }
                }
                var useQ = Config.Item("UseQCombo").GetValue<bool>();
                if (Q.IsReady())
                {
                    if (eTarget.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
                    {
                        if (Q.IsReady() && Player.Mana < 5 && useQ || Q.IsReady() && stackPrior == 0 && useQ && !smartMode || Q.IsReady() && is3Q)
                        {
                            Q.CastOnUnit(Player, pCast);;
                        }
                    }
                }
                if (E.IsReady())
                {
                    if (eTarget.IsValidTarget(E.Range) && E.IsReady() && useE && Player.Mana < 5 || eTarget.IsValidTarget(E.Range) && E.IsReady() && useE && stackPrior == 2 && !smartMode && !is3Q)
                    {
                        E.Cast(eTarget, true);
                    }
                }
                if (W.IsReady())
                {
                    if (eTarget.IsValidTarget(W.Range) && W.IsReady() && useW && Player.Mana < 5 || Player.Distance(eTarget) < W.Range && W.IsReady() && useW && stackPrior == 1 && !smartMode && !is3Q)
                    {
                        W.CastOnUnit(Player, pCast);
                    }
                }
            }

        }

        private void Freeze()
        {
            var pCast = Config.Item("packetCast").GetValue<bool>();
            var useQ = Config.Item("qf").GetValue<bool>();
            var useW = Config.Item("wf").GetValue<bool>();
            var useE = Config.Item("ef").GetValue<bool>();
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All, MinionTeam.Enemy);

            foreach (var minion in minions)
            {
                if (Q.IsReady())
                {
                    if (Player.Mana < 5 && Q.GetDamage(minion) > minion.Health && minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && useQ)
                    {
                        Q.CastOnUnit(Player, pCast);;
                    }
                }
                if (E.IsReady())
                {
                    if (Player.Mana < 5 && E.GetDamage(minion) > minion.Health && minion.IsValidTarget(E.Range) && useE)
                    {
                        E.Cast(minion, pCast);
                    }
                }
                if (W.IsReady())
                {
                    if (Player.Mana < 5 && W.GetDamage(minion) > minion.Health && useW && minion.IsValidTarget(W.Range))
                    {
                        W.CastOnUnit(Player, pCast);;
                    }
                }
            }
        }

        private void Jungle()
        {
            var pCast = Config.Item("packetCast").GetValue<bool>();
            var useQ = Config.Item("qj").GetValue<bool>();
            var useW = Config.Item("wj").GetValue<bool>();
            var useE = Config.Item("ej").GetValue<bool>();
            var stack5 = Config.Item("52").GetValue<bool>();

            foreach (var minion in JungleMinions)
            {
                var stackPrior = Config.Item("jungPriority").GetValue<StringList>().SelectedIndex;
                if (E.IsReady())
                {
                    if (Player.Mana < 5 && minion.IsValidTarget(E.Range) && useE)
                    {
                        E.Cast(minion, pCast);
                    }
                }
                if (W.IsReady())
                {
                    if (W.IsReady() && minion.IsValidTarget(W.Range) && useW)
                    {
                        if (Player.Mana < 5)
                        {
                            W.CastOnUnit(Player, pCast);;
                        }
                        else if (Player.Mana == 5 && stackPrior == 1 && stack5)
                        {
                            W.CastOnUnit(Player, pCast);;
                        }
                    }
                }
                if (Q.IsReady())
                {
                    if (Vector3.Distance(minion.Position, Player.Position) < Orbwalking.GetRealAutoAttackRange(Player) && Q.IsReady() && useQ)
                    {
                        if (Player.Mana < 5)
                        {
                            Q.CastOnUnit(Player, pCast);;
                        }
                        else if (Player.Mana == 5 && stackPrior == 0 && stack5)
                        {
                            Q.CastOnUnit(Player, pCast);;
                        }
                    }
                }
                //castItems(minion);
            }
        }
        private void WaveClear()
        {
            var pCast = Config.Item("packetCast").GetValue<bool>();
            var useQ = Config.Item("ql").GetValue<bool>();
            var useW = Config.Item("wl").GetValue<bool>();
            var useE = Config.Item("el").GetValue<bool>();
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All, MinionTeam.Enemy);
            var stackPrior = Config.Item("waveClearPriority").GetValue<StringList>().SelectedIndex;
            var stack5 = Config.Item("51").GetValue<bool>();

            foreach (var minion in minions)
            {
                if (E.IsReady())
                {
                    if (Player.Mana < 5 && E.GetDamage(minion) > minion.Health && minion.IsValidTarget(E.Range) && useE)
                    {
                        E.Cast(minion, pCast);
                    }
                }
                if (W.IsReady())
                {
                    if (minion.IsValidTarget(W.Range) && useW)
                    {
                        if (Player.Mana < 5 && W.GetDamage(minion) > minion.Health)
                        {
                            W.CastOnUnit(Player, pCast);;
                        }
                        else if (Player.Mana == 5 && stackPrior == 1 && stack5)
                        {
                            W.CastOnUnit(Player, pCast);;
                        }
                    }
                }
                if (Q.IsReady())
                {
                    if (Q.GetDamage(minion) > minion.Health && Vector3.Distance(minion.Position, Player.Position) < Orbwalking.GetRealAutoAttackRange(Player) && useQ)
                    {
                        if (Player.Mana < 5)
                        {
                            Q.CastOnUnit(Player, pCast);;
                        }
                        else if (Player.Mana == 5 && stackPrior == 0 && stack5)
                        {
                            Q.CastOnUnit(Player, pCast);;
                        }
                    }
                }
                //castItems(minion);
            }
        }
    }
}
