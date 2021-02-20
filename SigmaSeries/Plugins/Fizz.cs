using System;
using LeagueSharp;
using LeagueSharp.Common;
using Version = System.Version;


namespace SigmaSeries.Plugins
{
    public class Fizz : PluginBase
    {
        public Fizz()
            : base(new Version(0, 1, 1))
        {
            Q = new Spell(SpellSlot.Q, 560);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 370);
            E2 = new Spell(SpellSlot.E, 370);
            R = new Spell(SpellSlot.R, 1275);

            E.SetSkillshot(0.5f, 120, 1300, false, SkillshotType.SkillshotCircle);
            E2.SetSkillshot(0.5f, 400, 1300, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 250f, 1200f, false, SkillshotType.SkillshotLine);
            UseEAgain = true;
        }

        public static Spell E2;
        public static bool packetCast;

        public static Spellbook spellBook = ObjectManager.Player.Spellbook;
        public static SpellDataInst eSpell = spellBook.GetSpell(SpellSlot.E);
        public static bool UseEAgain;


        public override void ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
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
            config.AddItem(new MenuItem("UseEWC", "Use E WC").SetValue(true));
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
                var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if ((Config.Item("forceR").GetValue<KeyBind>().Active) && target != null)
            {
                R.Cast(target, true);
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
            var target = TargetSelector.GetTarget(800, TargetSelector.DamageType.Magical);
            if (target != null)
            {
                if (target.IsValidTarget(Q.Range) && useQ && Q.IsReady())
                {
                    Q.CastOnUnit(target, packetCast);
                    return;
                }
                //castItems(target);
                if (target.IsValidTarget(R.Range) && useR && R.IsReady())
                {
                    R.Cast(target, true);
                }
                if (target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && useW && W.IsReady())
                {
                    W.CastOnUnit(Player, packetCast);
                    return;
                }
                if (target.IsValidTarget(800) && useE && E.IsReady() && UseEAgain)
                {
                    if (target.IsValidTarget(370 + 250) && eSpell.Name == "FizzJump")
                    {
                        E.Cast(target, true);
                        UseEAgain = false;
                        Utility.DelayAction.Add(250, () => UseEAgain = true);
                    }
                    if (target.IsValidTarget(370 + 150) && target.IsValidTarget(330) == false && eSpell.Name == "fizzjumptwo")
                    {
                        E.Cast(target, true);
                    }
                }
                
            }
        }
        private void Harass()
        {
            var useQ = Config.Item("UseQHarass").GetValue<bool>();
            var useW = Config.Item("UseWHarass").GetValue<bool>();
            var useE = Config.Item("UseEHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(800, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                if (target.IsValidTarget(Q.Range) && useQ && Q.IsReady())
                {
                    Q.CastOnUnit(target, packetCast);
                    return;
                }
                if (target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && useW && W.IsReady())
                {
                    W.CastOnUnit(Player, packetCast);
                    return;
                }
                if (target.IsValidTarget(800) && useE && E.IsReady() && UseEAgain)
                {
                    if (target.IsValidTarget(370 + 330) && eSpell.Name == "FizzJump")
                    {
                        E.Cast(target, true);
                        UseEAgain = false;
                        Utility.DelayAction.Add(250, () => UseEAgain = true);
                    }
                    if (target.IsValidTarget(370 + 270) && target.IsValidTarget(330) == false && eSpell.Name == "fizzjumptwo")
                    {
                        E.Cast(target, true);
                    }
                }
            }
        }

        private void WaveClear()
        {
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2;
            var useE = Config.Item("UseEWC").GetValue<bool>();
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range);
            
            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (minion.IsValidTarget(E.Range) && E.IsReady() && useE && eSpell.Name == "FizzJump")
                    {
                        var ePoint = E.GetCircularFarmLocation(jungleMinions);
                        E.Cast(ePoint.Position, true);
                    }
                    if (minion.IsValidTarget(Q.Range) && useQ && Q.IsReady() && Q.GetDamage(minion) > minion.Health)
                    {
                        Q.CastOnUnit(minion, packetCast);
                    }
                    if (minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && useW && W.IsReady())
                    {
                        W.CastOnUnit(Player, packetCast);
                    }
                }

            }
        }
        private void Freeze()
        {            
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2; 
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range);

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
                        W.CastOnUnit(Player, packetCast);
                    }
                }
            }
        }
        private void Jungle()
        {
            var useQ = Config.Item("UseQJung").GetValue<bool>();
            var useW = Config.Item("UseWJung").GetValue<bool>();
            var useE = Config.Item("UseEJung").GetValue<bool>();
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (minion.IsValidTarget(E.Range) && E.IsReady() && eSpell.Name == "FizzJump" && useE)
                    {
                        E.Cast(minion);
                    }
                    if (minion.IsValidTarget(Q.Range) && useQ && Q.IsReady())
                    {
                        Q.CastOnUnit(minion, packetCast);
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
