using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Version = System.Version;


namespace SigmaSeries.Plugins
{
    public class Mordekaiser : PluginBase
    {
        public Mordekaiser()
            : base(new Version(0, 1, 1))
        {
            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 670);
            R = new Spell(SpellSlot.R, 850);
        }

        public static bool packetCast;


        public override void ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(false));
            config.AddItem(new MenuItem("controlMinion", "Control the Minion").SetValue(true));
            config.AddItem(new MenuItem("forceR", "Force R Cast").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
        }

        public override void HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(false));
            config.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(false));
            config.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
        }

        public override void FarmMenu(Menu config)
        {
            config.AddItem(new MenuItem("useQFarm", "Q").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 1)));
            config.AddItem(new MenuItem("useWFarm", "W").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 3)));
            config.AddItem(new MenuItem("useEFarm", "E").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 3)));
            config.AddItem(new MenuItem("JungleActive", "Jungle Clear!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            config.AddItem(new MenuItem("UseQJung", "Use Q").SetValue(false));
            config.AddItem(new MenuItem("UseWJung", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEJung", "Use E").SetValue(true));
        }

        public override void BonusMenu(Menu config)
        {
            config.AddItem(new MenuItem("packetCast", "Packet Cast").SetValue(true));
        }

        public override void OnUpdate(EventArgs args)
        {
            packetCast = Config.Item("packetCast").GetValue<bool>();
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if ((Config.Item("forceR").GetValue<KeyBind>().Active) && target != null)
            {
                R.CastOnUnit(target, true);
            }
            if (ComboActive)
            {
                Combo();
            }
            if (HarassActive)
            {
                Harass();
            }
            if (WaveClearActive)
            {
                WaveClear();
            }
            if (FreezeActive)
            {
                Freeze();
            }
            if (Config.Item("JungleActive").GetValue<KeyBind>().Active)
            {
                Jungle();
            }
        }

        private void Combo()
        {
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();
            var useRCon = Config.Item("controlMinion").GetValue<bool>();
            var Target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);
            if (Target != null)
            {
                if (Player.HasBuff("MordekaiserCOTGSelf") && useRCon)
                {
                    var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where Player.Position.Distance(champ.ServerPosition) < 2500 && champ.IsEnemy select champ).ToList();
                    nearChamps.OrderBy(x => Player.Position.Distance(x.ServerPosition));
                    if (nearChamps.Count > 0)
                    {
                        R.Cast(nearChamps.First().ServerPosition, packetCast);
                    }
                    else
                    {
                        R.Cast(Game.CursorPos, packetCast);
                    }
                }
                if (R.GetDamage(Target) > Target.Health && useR)
                {
                    R.CastOnUnit(Target);
                }
                if (Orbwalking.InAutoAttackRange(Target) && useQ && Q.IsReady())
                {
                    Q.CastOnUnit(Player, packetCast);;
                    return;
                }
                if (W.IsReady() && useW && wCast(Target))
                {
                    return;
                }
                if (Player.Distance(Target) < E.Range && useE && E.IsReady())
                {
                    E.Cast(Target.Position, packetCast);
                }
            }
        }

        private void Harass()
        {
            var useQ = Config.Item("UseQHarass").GetValue<bool>();
            var useW = Config.Item("UseWHarass").GetValue<bool>();
            var useE = Config.Item("UseEHarass").GetValue<bool>();
            var Target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (Target != null)
            {
                if (Orbwalking.InAutoAttackRange(Target) && useQ && Q.IsReady())
                {
                    Q.CastOnUnit(Player, packetCast);;
                    return;
                }
                if (W.IsReady() && useW && wCast(Target))
                {
                    return;
                }
                if (Player.Distance(Target) < E.Range && useE && E.IsReady())
                {
                    E.Cast(Target.Position, packetCast);
                }
            }
        }

        private void WaveClear()
        {
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2;
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);
            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (Q.IsReady() && useQ)
                    {
                        Q.Cast(Player.Position, packetCast);
                        return;
                    }
                    if (E.IsReady() && useE)
                    {
                        E.Cast(minion.Position, packetCast);
                        return;
                    }
                    if (W.IsReady() && useW)
                    {
                        W.CastOnUnit(Player);
                        return;
                    }
                }
            }
        }
        private void Freeze()
        {

            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2;
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);

            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (Q.IsReady() && useQ)
                    {
                        Q.Cast(Player.Position, packetCast);
                        return;
                    }
                    if (E.IsReady() && useE)
                    {
                        E.Cast(minion.Position, packetCast);
                        return;
                    }
                    if (W.IsReady() && useW)
                    {
                        W.CastOnUnit(Player);
                        return;
                    }
                }
            }
        }
        private void Jungle()
        {
            var useQ = Config.Item("UseQJung").GetValue<bool>();
            var useW = Config.Item("UseWJung").GetValue<bool>();
            var useE = Config.Item("UseEJung").GetValue<bool>();

            if (JungleMinions.Count > 0)
            {
                foreach (var minion in JungleMinions)
                {
                    if (Q.IsReady() && useQ)
                    {
                        Q.Cast(Player.Position, packetCast);
                        return;
                    }
                    if (E.IsReady() && useE)
                    {
                        E.Cast(minion.Position, packetCast);
                        return;
                    }
                    if (W.IsReady() && useW)
                    {
                        W.CastOnUnit(Player);
                        return;
                    }
                }
            }
        }

        public bool wCast(Obj_AI_Base wTarget)
        {
            if (Player.Distance(wTarget) < Orbwalking.GetRealAutoAttackRange(Player))
            {
                W.Cast(Player);
                return true;
            }
            var allies = (from champs in ObjectManager.Get<Obj_AI_Hero>() where Player.Distance(champs) < W.Range && champs.IsAlly select champs).ToList();
            foreach (var ally in allies)
            {
                if (ally.Distance(wTarget) < Orbwalking.GetRealAutoAttackRange(Player))
                {
                    W.Cast(ally, packetCast);
                    return true;
                }
            }
            return false;
        }
    }
}
