using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using System.Threading.Tasks;
using LX_Orbwalker;
using Version = System.Version;


namespace SigmaSeries.Plugins
{
    public class Cassiopeia : PluginBase
    {
        public Cassiopeia()
            : base(new Version(0, 1, 1))
        {
            Q = new Spell(SpellSlot.Q, 925);
            W = new Spell(SpellSlot.W, 925);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 875);

            Q.SetSkillshot(0.75f, 130, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.5f, 212, 2500, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 210, float.MaxValue, false, SkillshotType.SkillshotCone);
        }

        public bool delayed;

        public override void ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
        }

        public override void HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
        }

        public override void FarmMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQWC", "Use Q WC").SetValue(true));
            config.AddItem(new MenuItem("UseWWC", "Use W WC").SetValue(true));
            config.AddItem(new MenuItem("useEFarm", "E").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 2)));
            config.AddItem(new MenuItem("JungleActive", "JungleActive!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            config.AddItem(new MenuItem("UseQJung", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWJung", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEJung", "Use E").SetValue(true));
        }

        public override void BonusMenu(Menu config)
        {
            config.AddItem(new MenuItem("packetCast", "Packet Cast").SetValue(true));
        }


        public override void OnUpdate(EventArgs args)
        {
            var pCast = Config.Item("packetCast").GetValue<bool>();
            if (ComboActive)
            {
                var useQCombo = Config.Item("UseQCombo").GetValue<bool>();
                var useWCombo = Config.Item("UseWCombo").GetValue<bool>();
                var useECombo = Config.Item("UseECombo").GetValue<bool>();
                var eTarget = SimpleTs.GetTarget(1000f, SimpleTs.DamageType.Magical);
                if (eTarget != null)
                {
                    if (Player.Distance(eTarget) < E.Range && E.IsReady() && useECombo)
                    {
                        if (eTarget.HasBuffOfType(BuffType.Poison) || E.GetDamage(eTarget) > eTarget.Health)
                        {
                            E.CastOnUnit(eTarget, pCast);
                            return;
                        }
                    }
                    if (Player.Distance(eTarget) < Q.Range && Q.IsReady() && useQCombo)
                    {
                        Q.Cast(eTarget, pCast);
                        return;
                    }
                    if (Player.Distance(eTarget) < W.Range && W.IsReady() && useWCombo)
                    {
                        W.Cast(eTarget, pCast);
                        return;
                    }
                }
            }
          

            if (HarassActive)
            {
                var useQ = Config.Item("UseQHarass").GetValue<bool>();
                var useW = Config.Item("UseWHarass").GetValue<bool>();
                var useE = Config.Item("UseEHarass").GetValue<bool>();
                var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
                if (eTarget.IsValidTarget(E.Range) && E.IsReady() && useE)
                {
                    if (eTarget.HasBuffOfType(BuffType.Poison) || E.GetDamage(eTarget) > eTarget.Health)
                    {
                        E.CastOnUnit(eTarget, true);
                        return;
                    }
                }
                if (eTarget.IsValidTarget(Q.Range) && Q.IsReady() && useQ)
                {
                    Q.Cast(eTarget, true);
                    return;
                }
                if (eTarget.IsValidTarget(W.Range) && W.IsReady() && useW)
                {
                    W.Cast(eTarget, true);
                    return;
                }
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

        public override void OnDraw(EventArgs args)
        {
        }
        private void Freeze()
        {
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);
            if (minions.Count > 1)
            {
                foreach (var minion in minions)
                {
                    var predHP = HealthPrediction.GetHealthPrediction(minion, (int)E.Delay);

                    if (E.GetDamage(minion) > minion.Health && predHP > 0 && minion.IsValidTarget(E.Range) && useE)
                    {
                        E.CastOnUnit(minion, true);
                    }
                }
            }
        }

        private void Jungle()
        {
            var useQ = Config.Item("UseQJung").GetValue<bool>();
            var useW = Config.Item("UseWJung").GetValue<bool>();
            var useE = Config.Item("UseEJung").GetValue<bool>();

            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (JungleMinions.Count > 1)
            {
                foreach (var minion in JungleMinions)
                {
                    if (minion.HasBuffOfType(BuffType.Poison) && minion.IsValidTarget(E.Range) && useE)
                    {
                        E.CastOnUnit(minion, true);
                    }

                    if (minion.IsValidTarget(Q.Range) && useQ)
                    {
                        Q.Cast(minion, true);
                    }

                    if (minion.IsValidTarget(W.Range) && useW)
                    {
                        W.Cast(minion, true);
                    }
                }
            }
        }
        private void WaveClear()
        {
            var useW = Config.Item("UseQWC").GetValue<bool>();
            var useQ = Config.Item("UseWWC").GetValue<bool>();
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);
            if (minions.Count > 1)
            {
                foreach (var minion in minions)
                {
                    var predHP = HealthPrediction.GetHealthPrediction(minion, (int)E.Delay);

                    if (minion.HasBuffOfType(BuffType.Poison) && E.GetDamage(minion) > minion.Health && predHP > 0 && minion.IsValidTarget(E.Range) && useE)
                    {
                        E.CastOnUnit(minion, true);
                    }

                    if (minion.IsValidTarget(Q.Range) && useQ)
                    {
                        Q.Cast(Q.GetCircularFarmLocation(minions).Position, true);
                    }

                    if (minion.IsValidTarget(Q.Range) && useW)
                    {
                        W.Cast(W.GetCircularFarmLocation(minions).Position, true);
                    }
                }
            }
        }
    }
}
