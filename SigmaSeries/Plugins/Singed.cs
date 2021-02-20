using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using System.Threading.Tasks;
using Version = System.Version;


namespace SigmaSeries.Plugins
{
    public class Singed : PluginBase
    {
        public Singed()
            : base(new Version(0, 1, 1))
        {

            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 1175);
            E = new Spell(SpellSlot.E, 125);
            R = new Spell(SpellSlot.R, 0);

            useQAgain = true;
           
            W.SetSkillshot(0.5f, 350, 700, false, SkillshotType.SkillshotCircle);
        }

        public bool useQAgain;

        public override void ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            config.AddItem(new MenuItem("delayms", "Delay (MS)").SetValue<Slider>(new Slider(150, 0, 1000)));
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
            config.AddItem(new MenuItem("useEFarm", "E").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 2)));
            config.AddItem(new MenuItem("JungleActive", "JungleActive!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            config.AddItem(new MenuItem("UseQJung", "Use Q").SetValue(true));
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
                var useQ = Config.Item("UseQCombo").GetValue<bool>();
                var useW = Config.Item("UseWCombo").GetValue<bool>();
                var useE = Config.Item("UseECombo").GetValue<bool>();
                var delay = Config.Item("delayms").GetValue<Slider>().Value;
                var eTarget = SimpleTs.GetTarget(500f, SimpleTs.DamageType.Magical);
                if (eTarget != null)
                {
                    if (Q.IsReady() && useQ)
                    {
                        if (Player.HasBuff("Poison Trail"))
                        {
                            Q.CastOnUnit(Player, pCast);
                        }
                        if (Player.HasBuff("Poison Trail") == false && useQAgain)
                        {
                            Q.CastOnUnit(Player, pCast);
                            useQAgain = false;
                            Utility.DelayAction.Add(delay, () => useQAgain = true);
                        }
                    }
                    if (Player.HasBuff("Poison Trail") || Q.IsReady() == false)
                    {
                        if (eTarget.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) == false && useW)
                        {
                            W.Cast(eTarget, pCast);
                        }
                        if (eTarget.IsValidTarget(E.Range + 100) && useE)
                        {
                            E.CastOnUnit(eTarget, false);
                        }
                    }
                }
            }

            if (HarassActive)
            {
                var useQ = Config.Item("UseQHarass").GetValue<bool>();
                var useW = Config.Item("UseWHarass").GetValue<bool>();
                var useE = Config.Item("UseEHarass").GetValue<bool>();
                var delay = Config.Item("delayms").GetValue<Slider>().Value;
                var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
                if (eTarget != null)
                {
                    if (Q.IsReady() && useQ)
                    {
                        if (Player.HasBuff("Poison Trail"))
                        {
                            Q.CastOnUnit(Player, pCast);
                        }
                        if (Player.HasBuff("Poison Trail") == false && useQAgain)
                        {
                            Q.CastOnUnit(Player, pCast);
                            useQAgain = false;
                            Utility.DelayAction.Add(delay, () => useQAgain = true);
                        }
                    }
                    if (Player.HasBuff("Poison Trail"))
                    {
                        if (eTarget.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) == false && useW)
                        {
                            W.Cast(eTarget, pCast);
                        }
                        if (eTarget.IsValidTarget(E.Range) && useE)
                        {
                            E.CastOnUnit(eTarget, pCast);
                        }
                    } 
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

        private void Freeze()
        {
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, 400, MinionTypes.All);
            if (minions.Count > 1)
            {
                foreach (var minion in minions)
                {
                    var predHP = HealthPrediction.GetHealthPrediction(minion, (int)E.Delay);

                    if (E.GetDamage(minion) > minion.Health && predHP > 0 && minion.IsValidTarget(E.Range))
                    {
                        E.CastOnUnit(minion, true);
                    }
                }
            }
        }

        private void Jungle()
        {
            var useQ = Config.Item("UseQJung").GetValue<bool>();

            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, 400, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (JungleMinions.Count > 0)
            {
                foreach (var minion in JungleMinions)
                {
                    if (Q.IsReady() && useQ)
                    {
                        if (Player.HasBuff("Poison Trail") == false)
                        {
                            Q.Cast(Game.CursorPos, true);
                        }
                    }
                }
            }
            else
            {
                if (Q.IsReady() && useQ)
                {
                    if (Player.HasBuff("Poison Trail"))
                    {
                        Q.Cast(Game.CursorPos, true);
                    }
                }
            }
        }
        private void WaveClear()
        {
            var useQ = Config.Item("UseQWC").GetValue<bool>();
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, 400, MinionTypes.All);
            if (minions.Count > 0)
            {
                if (Q.IsReady() && useQ)
                {
                    if (Player.HasBuff("Poison Trail") == false)
                    {
                        Q.Cast(Game.CursorPos, true);
                    }
                }

            }
            else
            {
                if (Q.IsReady() && useQ)
                {
                    if (Player.HasBuff("Poison Trail"))
                    {
                        Q.Cast(Game.CursorPos, true);
                    }
                }
            }
        }
    }
}
