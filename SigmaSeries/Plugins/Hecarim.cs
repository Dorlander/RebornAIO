using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Version = System.Version;


namespace SigmaSeries.Plugins
{
    public class Hecarim : PluginBase
    {
        public Hecarim()
            : base(new Version(0, 1, 1))
        {
            Q = new Spell(SpellSlot.Q, 350);
            W = new Spell(SpellSlot.W, 525);
            E = new Spell(SpellSlot.E, 0);
            R = new Spell(SpellSlot.R, 1350);

            R.SetSkillshot(0.5f, 200f, 1200f, false, SkillshotType.SkillshotLine);

            Obj_AI_Base.OnProcessSpellCast +=Obj_AI_Base_OnProcessSpellCast;

        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsAlly || Player.Distance(sender) > 600 || sender.IsMinion) return;
            foreach (var spell in Interrupter.Spells)
            {
                if (args.SData.Name == spell.SpellName)
                {
                    if (Config.Item(args.SData.Name).GetValue<bool>())
                    {
                        E.CastOnUnit(Player, packetCast);;
                        Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
                        break;
                    }

                }
            }
        }

        public static bool packetCast;


        public override void ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            config.AddItem(new MenuItem("wHP", "W HP").SetValue(new Slider(40, 1)));
            config.AddItem(new MenuItem("UseRCombo", "Use R!").SetValue(false));
        }

        public override void HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(false));
            config.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(false));
        }

        public override void FarmMenu(Menu config)
        {
            config.AddItem(
                new MenuItem("useQFarm", "Q").SetValue(new StringList(new[] {"Freeze", "WaveClear", "Both", "None"}, 1)));
            config.AddItem(
                new MenuItem("useWFarm", "W").SetValue(new StringList(new[] {"Freeze", "WaveClear", "Both", "None"}, 3)));
            config.AddItem(
                new MenuItem("JungleActive", "Jungle Clear!").SetValue(new KeyBind("C".ToCharArray()[0],
                    KeyBindType.Press)));
            config.AddItem(new MenuItem("UseQJung", "Use Q").SetValue(false));
            config.AddItem(new MenuItem("UseWJung", "Use W").SetValue(true));
        }

        public override void BonusMenu(Menu config)
        {
            config.AddItem(new MenuItem("packetCast", "Packet Cast").SetValue(true));
            Utility.DelayAction.Add(1000, () => actiontobedelayed(config));
        }

        private void actiontobedelayed(Menu config)
        {
            config.AddItem(new MenuItem("--", "--"));
            config.AddItem(new MenuItem("Interrupter", "Interrupter"));
            foreach (var interrupter in Interrupter.Spells)
            {
                config.AddItem(
                    new MenuItem(interrupter.SpellName, interrupter.ChampionName + ": " + interrupter.SpellName)
                        .SetValue(true));
            }
        }

        public override void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            Game.PrintChat(unit.Name);
        }

        

        public override void OnUpdate(EventArgs args)
        {
            
            if (ComboActive)
            {
                combo();
            }
            if (HarassActive)
            {
                harass();
            }
            if (WaveClearActive)
            {
                waveClear();
            }
            if (FreezeActive)
            {
                freeze();
            }
            if (Config.Item("JungleActive").GetValue<KeyBind>().Active)
            {
                jungle();
            }
            if (FleeActive && E.IsReady())
            {
                E.CastOnUnit(Player, true);
            }
        }

        private void combo()
        {
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();
            var Target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            if (Target != null)
            {
                if (Target.IsValidTarget(Q.Range) && useQ && Q.IsReady())
                {
                    Q.CastOnUnit(Player, packetCast);
                    return;
                }
                //castItems(Target);
                if (Target.IsValidTarget(R.Range) && useR && R.IsReady())
                {
                    R.Cast(Target, packetCast);
                }
                if (Target.IsValidTarget(W.Range) && useW && W.IsReady())
                {
                    W.CastOnUnit(Player, packetCast);
                }
            }
        }
        private void harass()
        {
            var useQ = Config.Item("UseQHarass").GetValue<bool>();
            var useW = Config.Item("UseWHarass").GetValue<bool>();
            var Target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);

            if (Target != null)
            {
                if (Target != null)
                {
                    if (Target.IsValidTarget(Q.Range) && useQ && Q.IsReady())
                    {
                        Q.CastOnUnit(Player, packetCast);
                        return;
                    }
                    if (Target.IsValidTarget(W.Range) && useW && W.IsReady())
                    {
                        W.CastOnUnit(Player, packetCast);
                        return;
                    }
                }
            }
        }

        private void waveClear()
        {
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, 800, MinionTypes.All);

            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (minion.IsValidTarget(Q.Range) && useQ && Q.IsReady() && Q.GetDamage(minion) > minion.Health)
                    {
                        Q.CastOnUnit(minion, packetCast);
                    }
                    if (minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && useW && W.IsReady())
                    {
                        W.Cast(Game.CursorPos, packetCast);
                    }
                }

            }
        }
        private void freeze()
        {
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, 800, MinionTypes.All);

            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (minion.IsValidTarget(Q.Range) && useQ && Q.IsReady() && Q.GetDamage(minion) > minion.Health)
                    {
                        Q.CastOnUnit(Player, packetCast);
                    }
                    if (minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && useW && W.IsReady())
                    {
                        W.CastOnUnit(Player, packetCast);
                    }
                }
            }
        }
        private void jungle()
        {
            var useQ = Config.Item("UseQJung").GetValue<bool>();
            var useW = Config.Item("UseWJung").GetValue<bool>();

            if (JungleMinions.Count > 0)
            {
                foreach (var minion in JungleMinions)
                {
                    if (minion.IsValidTarget(Q.Range) && useQ && Q.IsReady())
                    {
                        Q.CastOnUnit(Player, packetCast);
                    }
                    if (minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && useW && W.IsReady())
                    {
                        W.CastOnUnit(Player, packetCast);
                    }
                }
            }
        }
    }
}
