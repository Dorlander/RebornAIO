using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using Version = System.Version;

namespace SigmaSeries.Plugins
{
    public class FiddleSticks : PluginBase
    {
        public FiddleSticks()
            : base(new Version(0, 1, 1))
        {
            Q = new Spell(SpellSlot.Q, 575);
            W = new Spell(SpellSlot.W, 575);
            E = new Spell(SpellSlot.E, 750);
            R = new Spell(SpellSlot.R, 0);

            LeagueSharp.Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnGameSendPacket += Game_OnGameSendPacket;
        }
        public static int count;
        public static float newTime;
        public static bool packetCast;
        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.Name == Player.Name && args.SData.Name == "Drain")
            {
                newTime = Game.Time + 1f;
                Orbwalker.SetMovement(false);
                Orbwalker.SetAttack(false);
            }
        }

        private void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == 0x72 && Player.HasBuff("Drain") && count < 3)
            {
                count = count + 1;
                args.Process = false;
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            config.AddItem(new MenuItem("sep", "When Using W, it will block"));
            config.AddItem(new MenuItem("sep2", "packets click 3 times to free it"));
        }

        public override void HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(false));
            config.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(false));
            config.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
        }

        public override void FarmMenu(Menu config)
        {
            config.AddItem(new MenuItem("useWFarm", "W").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 3)));
            config.AddItem(new MenuItem("useEFarm", "E").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 1)));
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

            if (Player.HasBuff("Drain"))
            {
                Orbwalker.SetMovement(false);
                Orbwalker.SetAttack(false);
            }
            if (!Player.HasBuff("Drain") && newTime < Game.Time)
            {
                count = 0;
                Orbwalker.SetMovement(true);
                Orbwalker.SetAttack(true);
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
        }

        private void combo()
        {
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var Target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (Target != null)
            {
                if (!Player.HasBuff("Drain") && newTime < Game.Time)
                {
                    if (Player.Distance(Target) < Q.Range && useQ && Q.IsReady())
                    {
                        Q.CastOnUnit(Target, packetCast);
                        return;
                    }
                    if (Player.Distance(Target) < 575 && useW && W.IsReady())
                    {
                        W.CastOnUnit(Target, packetCast);
                        return;
                    }
                    if (Player.Distance(Target) < E.Range && useE && E.IsReady())
                    {
                        E.CastOnUnit(Target, packetCast);
                        return;
                    }
                }
            }
        }
        private void harass()
        {
            var useQ = Config.Item("UseQHarass").GetValue<bool>();
            var useW = Config.Item("UseWHarass").GetValue<bool>();
            var useE = Config.Item("UseEHarass").GetValue<bool>();
            var Target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (Target != null)
            {
                if (!Player.HasBuff("Drain") && newTime < Game.Time)
                {
                    if (Player.Distance(Target) < Q.Range && useQ && Q.IsReady())
                    {
                        Q.CastOnUnit(Target, packetCast);
                        return;
                    }
                    if (Player.Distance(Target) < W.Range && useW && W.IsReady())
                    {
                        W.CastOnUnit(Target, packetCast);
                        return;
                    }
                    if (Player.Distance(Target) < E.Range && useE && E.IsReady())
                    {
                        E.CastOnUnit(Target, packetCast);
                        return;
                    }
                }
            }
        }
        private void waveClear()
        {
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2;
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);
            if (!Player.HasBuff("Drain") && newTime < Game.Time)
            {
                if (jungleMinions.Count > 0)
                {
                    foreach (var minion in jungleMinions)
                    {
                        if (E.IsReady() && useE)
                        {
                            E.CastOnUnit(minion, packetCast);
                            return;
                        }
                        if (W.IsReady() && useW)
                        {
                            W.CastOnUnit(minion, packetCast);
                            return;
                        }
                    }
                }
            }
        }
        private void freeze()
        {
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2;
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);
            if (!Player.HasBuff("Drain") && newTime < Game.Time)
            {
                if (jungleMinions.Count > 0)
                {
                    foreach (var minion in jungleMinions)
                    {
                        if (E.IsReady() && useE)
                        {
                            E.CastOnUnit(minion, packetCast);
                            return;
                        }
                        if (W.IsReady() && useW)
                        {
                            W.CastOnUnit(minion, packetCast);
                            return;
                        }
                    }
                }
            }
        }
        private void jungle()
        {
            var useQ = Config.Item("UseQJung").GetValue<bool>();
            var useW = Config.Item("UseWJung").GetValue<bool>();
            var useE = Config.Item("UseEJung").GetValue<bool>();
            if (!Player.HasBuff("Drain") && newTime < Game.Time)
            {
                if (JungleMinions.Count > 0)
                {
                    foreach (var minion in JungleMinions)
                    {
                        if (Q.IsReady() && useQ)
                        {
                            Q.CastOnUnit(minion, packetCast);
                            return;
                        }
                        if (E.IsReady() && useE)
                        {
                            E.CastOnUnit(minion, packetCast);
                            return;
                        }
                        if (W.IsReady() && useW)
                        {
                            W.CastOnUnit(minion, packetCast);
                            return;
                        }

                    }
                }
            }
        }

        
    }
}
