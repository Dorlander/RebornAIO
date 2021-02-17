using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;
using SCommon.PluginBase;

namespace SAutoCarry.Champions.Helpers
{
    public static class Animation
    {
        private static int s_LastAATick;
        private static bool s_CheckAA;
        private static bool s_DoAttack;

        public static int QStacks = 0;
        public static int LastQTick = 0;
        public static bool CanCastAnimation = true;
        public static bool UltActive;
        public static long blResetQueued;

        public delegate void dOnAnimationCastable(string animname);
        public static event dOnAnimationCastable OnAnimationCastable;

       /* public static void OnPlay(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                int t = 0;
                switch (args.CastDelay)
                {
                    case "Spell1a":
                        QStacks = 1;
                        CanCastAnimation = false;
                        LastQTick = Utils.TickCount;
                        t = 291;
                        break;
                    case "Spell1b":
                        QStacks = 2;
                        CanCastAnimation = false;
                        LastQTick = Utils.TickCount;
                        t = 291;
                        break;
                    case "Spell1c":
                        QStacks = 0;
                        SetAttack(false);
                        CanCastAnimation = false;
                        LastQTick = Utils.TickCount;
                        t = 393;
                        break;
                    case "Spell2":
                        CanCastAnimation = false;
                        t = 10;
                        break;
                    case "Spell3":
                        CanCastAnimation = true;
                        break;
                    case "Spell4a":
                        t = 0;
                        CanCastAnimation = false;
                        UltActive = true;
                        break;
                    case "Spell4b":
                        t = 0;
                        CanCastAnimation = false;
                        UltActive = false;
                        break;
                }
                if (t != 0)
                {
                    if (Program.Champion.Orbwalker.ActiveMode != SCommon.Orbwalking.Orbwalker.Mode.None)
                    {
                        LeagueSharp.Common.Utility.DelayAction.Add(Math.Max(1, t - Game.Ping), () => CancelAnimation());
                        LeagueSharp.Common.Utility.DelayAction.Add(Math.Max(1, t - Game.Ping), () => OnAnimationCastable(args.CastDelay));
                    }
                }
                else
                    LeagueSharp.Common.Utility.DelayAction.Add(1, () => OnAnimationCastable(args.CastDelay));
            }
        }*/

        public static void AfterAttack(SCommon.Orbwalking.AfterAttackArgs args)
        {
            if (s_CheckAA)
            {
                s_CheckAA = false;
                CanCastAnimation = true;
                 
                if(args.Target.IsValidTarget() && !Program.Champion.Spells[0].IsReady() && Program.Champion.Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Mixed)
                {
                    if(Program.Champion.Spells[2].IsReady() && !Program.Champion.Spells[0].IsReady(1000) && Program.Champion.ConfigMenu.Item("HEMODE").GetValue<StringList>().SelectedIndex == 2)
                        Program.Champion.Spells[2].Cast(ObjectManager.Player.ServerPosition + (args.Target.Position - ObjectManager.Player.ServerPosition).Normalized() * -Program.Champion.Spells[2].Range);
                }

                var t = Target.Get(Program.Champion.Spells[0].Range + 50, true);
                if (s_DoAttack && Program.Champion.Spells[0].IsReady())
                {
                    if (t != null && (Program.Champion.Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Combo || Program.Champion.Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Mixed || Program.Champion.ConfigMenu.Item("CSHYKEY").GetValue<KeyBind>().Active))
                    {
                        Program.Champion.Spells[0].Cast(t.ServerPosition + (t.ServerPosition - ObjectManager.Player.ServerPosition).Normalized() * 40);
                        Program.Champion.Orbwalker.ResetAATimer();
                        Program.Champion.Orbwalker.ForcedTarget = t;
                    }
                    else if (Program.Champion.Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.LaneClear)
                    {
                        var minion = MinionManager.GetMinions(400, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).OrderBy(p => p.ServerPosition.Distance(ObjectManager.Player.ServerPosition)).FirstOrDefault();
                        if (minion != null)
                        {
                            if (minion.Health <= ObjectManager.Player.GetAutoAttackDamage(minion) * 2 && minion.IsJungleMinion())
                                SetAttack(false);
                            else
                            {
                                Program.Champion.Spells[0].Cast(minion.ServerPosition);
                                Program.Champion.Orbwalker.ResetAATimer();
                                Program.Champion.Orbwalker.ForcedTarget = t;
                            }
                        }
                    }
                }
                else
                    SetAttack(false);
            }
        }

        public static void CancelAnimation()
        {
            if (s_DoAttack)
            {
                if (Program.Champion.Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.LaneClear)
                {
                    var minion = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 400, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).OrderBy(p => p.Distance(ObjectManager.Player.ServerPosition)).FirstOrDefault();
                    if (minion != null)
                        Program.Champion.Orbwalker.ForcedTarget = minion;
                }
                else if (Program.Champion.Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Mixed || Program.Champion.Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Combo)
                {
                    var t = Target.Get(1000f);
                    if (t != null)
                        Program.Champion.Orbwalker.ForcedTarget = t;
                }
                Program.Champion.Orbwalker.ResetAATimer();
            }
            Game.PrintChat("/d");
            CanCastAnimation = true;
        }

        public static void SetLastAATick(int tick)
        {
            s_LastAATick = Utils.TickCount;
            s_CheckAA = true;
        }

        public static void SetAttack(bool b)
        {
            s_DoAttack = b;
        }

        public static bool CanAttack()
        {
            return s_DoAttack;
        }
    }
}
