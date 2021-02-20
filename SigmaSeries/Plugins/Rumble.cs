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
    public class Rumble : PluginBase
    {
        public Rumble()
            : base(new Version(0, 1, 1))
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 850);
            R = new Spell(SpellSlot.R, 1700);

            E.SetSkillshot(0.5f, 90, 1200, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(1700, 120, 1400, false, SkillshotType.SkillshotLine);
            checkHeat = true;
        }
        public static bool packetCast;

        public static float maxRangeR = 2400f;
        public static List<Obj_AI_Base> minions;
        public static Obj_AI_Hero bestChamp;
        public static int maxCount;
        public static float sleepTime;
        public static bool checkHeat;
        public override void ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            config.AddItem(new MenuItem("castR", "Cast R!").SetValue(new KeyBind("R".ToCharArray()[0], KeyBindType.Press)));
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
            config.AddItem(new MenuItem("useEFarm", "E").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 0)));
            config.AddItem(new MenuItem("JungleActive", "Jungle Clear!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            config.AddItem(new MenuItem("UseQJung", "Use Q").SetValue(false));
            config.AddItem(new MenuItem("UseWJung", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEJung", "Use E").SetValue(true));
        }

        public override void BonusMenu(Menu config)
        {
            config.AddItem(new MenuItem("packetCast", "Packet Cast").SetValue(true));
            config.AddItem(new MenuItem("keepHeat", "Maintain Heat").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle)));
            config.AddItem(new MenuItem("keepHeatQ", "Use Q to maintain heat").SetValue(false));
            config.AddItem(new MenuItem("keepHeatW", "Use W to maintain heat").SetValue(true));
            config.AddItem(new MenuItem("KSE", "KillSteal With E").SetValue(true));
        }

        public override void OnUpdate(EventArgs args)
        {
            if (Config.Item("castR").GetValue<KeyBind>().Active && R.IsReady())
            {
                var rTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
                CastR(rTarget);
            }
            packetCast = Config.Item("packetCast").GetValue<bool>();
            if ( Config.Item("KSE").GetValue<bool>())
            {
                ks();
            }
            
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

            if (Config.Item("keepHeat").GetValue<KeyBind>().Active)
            {
                keepHeat();
            }
        }

        private void ks()
        {
            foreach (var player in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (player.IsEnemy && player.IsValidTarget(E.Range) && player.IsDead == false && E.GetDamage(player) > player.Health)
                {
                    CastE(player, true);
                }
            }
        }

        private void keepHeat()
        {
            var useQ = Config.Item("keepHeatQ").GetValue<bool>();
            var useW = Config.Item("keepHeatW").GetValue<bool>();
            if (checkHeat)
            {
                if (Player.Mana < 50)
                {
                    if (Q.IsReady() && useQ)
                    {
                        Q.Cast(Game.CursorPos, true);
                        checkHeat = false;
                        Utility.DelayAction.Add(200, (() => checkHeat = true));
                        return;
                    }
                    if (W.IsReady() && useW)
                    {
                        W.CastOnUnit(Player, packetCast);;
                        checkHeat = false;
                        Utility.DelayAction.Add(200, (() => checkHeat = true));
                    }
                }
            }
        }

        private void combo()
        {
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var rTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (rTarget != null)
            {
                if (Player.Distance(rTarget) < Orbwalking.GetRealAutoAttackRange(Player) && W.IsReady() && willOverLoad(false) == false && useQ)
                {
                    CastW();
                    return;
                }
                if (Player.Distance(rTarget) < Q.Range && Q.IsReady() && willOverLoad(false) == false && useW)
                {
                    CastQ(rTarget, false);
                    return;
                }
                if (Player.Distance(rTarget) < E.Range && E.IsReady() && willOverLoad(false) == false && useE)
                {
                    CastE(rTarget, false);
                    return;
                }
            }
        }
        private void harass()
        {
            var useQ = Config.Item("UseQHarass").GetValue<bool>();
            var useW = Config.Item("UseWHarass").GetValue<bool>();
            var useE = Config.Item("UseEHarass").GetValue<bool>();
            var rTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (rTarget != null)
            {
                if (Player.Distance(rTarget) < Orbwalking.GetRealAutoAttackRange(Player) && W.IsReady() && willOverLoad(false) == false && useQ)
                {
                    CastW();
                    return;
                }
                if (Player.Distance(rTarget) < Q.Range && Q.IsReady() && willOverLoad(false) == false && useW)
                {
                    CastQ(rTarget, false);
                    return;
                }
                if (Player.Distance(rTarget) < E.Range && E.IsReady() && willOverLoad(false) == false && useE)
                {
                    CastE(rTarget, false);
                    return;
                }
            }
        }
        private void waveClear()
        {
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;

            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);

            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (Q.IsReady() && useQ)
                    {
                        CastQ(minion, false);
                        return;
                    }
                    if (E.IsReady() && useE && E.GetDamage(minion) > minion.Health)
                    {
                        CastE(minion, false);
                        return;
                    }

                }
            }
            
        }
        private void freeze()
        {
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);

            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (Q.IsReady() && useQ)
                    {
                        CastQ(minion, false);
                        return;
                    }
                    if (E.IsReady() && useE && E.GetDamage(minion) > minion.Health)
                    {
                        CastE(minion, false);
                        return;
                    }
                }
            }
            
        }
        private void jungle()
        {
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;

            if (JungleMinions.Count > 0)
            {
                foreach (var minion in JungleMinions)
                {
                    if (Q.IsReady() && useQ)
                    {
                        CastQ(minion, false);
                        return;
                    }
                    if (E.IsReady() && useE && E.GetDamage(minion) > minion.Health)
                    {
                        CastE(minion, false);
                        return;
                    }
                }
            }
            
        }
        private void CastQ(Obj_AI_Base qTarget, bool  ks)
        {
            if (ks)
            {
                if (Player.Distance(qTarget) < Q.Range)
                {
                    Q.Cast(qTarget, packetCast);
                }
            }
            else if (!ks && willOverLoad(false) == false)
            {
                if (Player.Distance(qTarget) < Q.Range)
                {
                    Q.Cast(qTarget, packetCast);
                }
            }
        }

        private void CastW()
        {
            if (!willOverLoad(false))
            {
                W.CastOnUnit(Player, packetCast);
            }
        }

        private void CastE(Obj_AI_Base eTarget, bool ks)
        {
            if (ks)
            {
                E.Cast(eTarget, packetCast);
            }

            if (!willOverLoad(true) && ks == false)
            {
                E.Cast(eTarget, packetCast);
            }
        }
        private void CastR(Obj_AI_Base rTarget)
        {
            var getChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where champ.IsValidTarget(maxRangeR) && rTarget.Name != champ.Name select champ).ToList();

            maxCount = -1;
            bestChamp = null;

            foreach (var enemy in getChamps)
            {
                var getMoarChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where champ.IsValidTarget(maxRangeR) select champ).ToList();
                var count = 0;
                foreach (var champs in getMoarChamps)
                {
                    count = (int)SimpleTs.GetPriority(enemy);
                }
                if (maxCount < count || maxCount == -1)
                {
                    maxCount = count;
                    bestChamp = enemy;
                }
            }

            if (bestChamp == null)
            {
                castR2(rTarget.Position, Prediction.GetPrediction(rTarget, 2f).CastPosition);
            }

            if (bestChamp != null)
            {
                castR2(R.GetPrediction(rTarget).CastPosition, R.GetPrediction(bestChamp).CastPosition);
            }
            
        }

        private void castR2(Vector3 point1, Vector3 point2)
        {
            var p1 = point1.To2D();
            var p2 = point2.To2D();

            Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(0, R.Slot, -1, p1.X, p1.Y, p2.X, p2.Y)).Send();
        }
        private bool willOverLoad(bool isE = false)
        {
            if (isE && Player.HasBuff("RumbleGrenade"))
            {
                return false;
            }
            if (isE == false || isE && Player.HasBuff("RumbleGrenade") == false)
            {
                if ((Player.Mana + 20) < 100)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
