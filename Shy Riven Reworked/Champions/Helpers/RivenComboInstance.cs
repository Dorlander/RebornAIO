using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SAutoCarry.Champions;
using SharpDX;

namespace SAutoCarry.Champions.Helpers
{
    internal class ComboInstance
    {
        private static Riven s_Champion;

        public static Action<Obj_AI_Hero>[] MethodsOnUpdate = new Action<Obj_AI_Hero>[3];
        public static Action<Obj_AI_Hero, string>[] MethodsOnAnimation = new Action<Obj_AI_Hero, string>[3];
        public static Action<Obj_AI_Hero>[] GapCloseMethods = new Action<Obj_AI_Hero>[3];

        private const int Q = 0, W = 1, E = 2, R = 3, Q2 = 4, W2 = 5, E2 = 6, R2 = 7;

        public static void Initialize(Riven Me)
        {
            s_Champion = Me;
            #region Gapclosers
            GapCloseMethods[0] = new Action<Obj_AI_Hero>((t) =>
            {
                if (t.Distance(ObjectManager.Player.ServerPosition) > Me.ConfigMenu.Item("MMINDIST").GetValue<Slider>().Value)
                {
                    if (Me.Spells[E].IsReady())
                    {
                        int eMode = 3;
                        if (Me.Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Combo)
                            eMode = Me.ConfigMenu.Item("CEMODE").GetValue<StringList>().SelectedIndex;
                        else if (Me.Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Mixed)
                            eMode = Me.ConfigMenu.Item("HEMODE").GetValue<StringList>().SelectedIndex;

                        if (eMode == 0)
                            Me.Spells[E].Cast(t.ServerPosition);
                        else if (eMode == 1)
                            Me.Spells[E].Cast(Game.CursorPos);                            
                    }
                }
            });

            GapCloseMethods[1] = new Action<Obj_AI_Hero>((t) =>
            {
                if (t.Distance(ObjectManager.Player.ServerPosition) > Me.ConfigMenu.Item("MMINDIST").GetValue<Slider>().Value)
                {
                    if (!Me.Spells[E].IsReady())
                    {
                        if (Me.Spells[Q].IsReady())
                            Me.Spells[Q].Cast(t.ServerPosition);
                    }
                }
            });

            GapCloseMethods[2] = new Action<Obj_AI_Hero>((t) =>
            {
                if (Target.IsTargetFlashed() && Me.ConfigMenu.Item("CUSEF").GetValue<KeyBind>().Active)
                {
                    if (t.Distance(ObjectManager.Player.ServerPosition) > 300 && t.Distance(ObjectManager.Player.ServerPosition) < 500 && Me.Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Combo)
                    {
                        int steps = (int)(t.Distance(ObjectManager.Player.ServerPosition) / 10);
                        Vector3 direction = (t.ServerPosition - ObjectManager.Player.ServerPosition).Normalized();
                        for (int i = 0; i < steps - 1; i++)
                        {
                            if (NavMesh.GetCollisionFlags(ObjectManager.Player.ServerPosition + direction * 10 * i).HasFlag(CollisionFlags.Wall))
                                return;
                        }
                        ObjectManager.Player.Spellbook.CastSpell(Me.SummonerFlash, t.ServerPosition);
                    }
                    Target.SetFlashed(false);
                }
            });
            #endregion

            #region Normal Combo
            MethodsOnUpdate[0] = (t) =>
            {
                if (t != null)
                {
                    //gapclose
                    for (int i = 0; i < GapCloseMethods.Length; i++)
                        GapCloseMethods[i](t);

                    if (Me.CheckR1(t))
                    {
                        if (Me.Spells[E].IsReady())
                            Me.Spells[E].Cast(t.ServerPosition);
                        Me.Spells[R].Cast();
                    }

                    if (Me.CheckR2(t))
                        Me.Spells[R].Cast(t.ServerPosition);

                    if (Me.Spells[W].IsReady() && t.Distance(ObjectManager.Player.ServerPosition) < Me.Spells[W].Range && !Me.IsDoingFastQ)
                    {
                        Me.CastCrescent();
                        Me.Spells[W].Cast(true);
                    }
                }

                if (!Animation.CanAttack() && Animation.CanCastAnimation && !Me.Spells[W].IsReady() && !Me.CheckR1(t))
                    Me.FastQCombo();
            };

            MethodsOnAnimation[0] = (t, animname) =>
            {
                if (Me.Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Combo || Me.Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Mixed)
                {
                    t = Target.Get(600, true);
                    if (t != null)
                    {
                        if (animname == "Spell3") //e w & e q etc
                        {
                            if (Me.CheckR1(t))
                            {
                                Me.Spells[R].Cast();
                                return;
                            }

                            if (Me.Spells[W].IsReady() && t.Distance(ObjectManager.Player.ServerPosition) < Me.Spells[W].Range && !Me.IsDoingFastQ)
                            {
                                Me.Spells[W].Cast();
                                return;
                            }

                            if (Me.Spells[Q].IsReady() && !Me.IsDoingFastQ && !Me.CheckR1(t) && t.Distance(ObjectManager.Player.ServerPosition) < Me.Spells[Q].Range)
                            {
                                Me.Spells[Q].Cast(t.ServerPosition + (t.ServerPosition - ObjectManager.Player.ServerPosition).Normalized() * 40);
                                Me.FastQCombo();
                                return;
                            }
                        }
                        else if (animname == "Spell4a")
                        {
                            if (Me.Spells[W].IsReady() && t.Distance(ObjectManager.Player.ServerPosition) < Me.Spells[W].Range)
                            {
                                Me.Spells[W].Cast();
                                return;
                            }
                        }
                    }

                    //r2 target
                    t = Target.Get(900);
                    if (t != null && Me.CheckR2(t))
                    {
                        if (animname == "Spell3") //q3 r2
                            Utility.DelayAction.Add(393 - Game.Ping, () => Me.Spells[R].Cast(t.ServerPosition));
                    }
                }
            };
            #endregion

            #region Shy Burst (E-R-Flash-W-AA-R2-Hydra-Q)
            MethodsOnUpdate[1] = (t) =>
            {
                if (!ObjectManager.Player.Spellbook.GetSpell(Me.SummonerFlash).IsReady() && !ObjectManager.Player.HasBuff("RivenFengShuiEngine"))
                {
                    MethodsOnUpdate[0](t);
                    return;
                }

                t = Target.Get(900, true);
                if (t != null)
                {
                    if (Me.Spells[E].IsReady() && ObjectManager.Player.ServerPosition.Distance(t.ServerPosition) <= Me.Spells[E].Range + 400 + Me.Spells[W].Range / 2f && !ObjectManager.Player.HasBuff("RivenFengShuiEngine"))
                    {
                        Me.Spells[E].Cast(t.ServerPosition);
                        if (!Me.ConfigMenu.Item("CDISABLER").GetValue<bool>() && Me.Spells[R].IsReady())
                            Me.Spells[R].Cast();
                        return;
                    }

                    if (Me.Spells[W].IsReady() && t.IsValidTarget(Me.Spells[W].Range))
                    {
                        Me.Spells[W].Cast();
                        return;
                    }

                    if (ObjectManager.Player.HasBuff("RivenFengShuiEngine"))
                    {
                        if ((t.Health - Me.CalculateDamageR2(t) <= 0) && !Me.ConfigMenu.Item("CDISABLER").GetValue<bool>())
                        {
                            if (Me.Spells[R].IsReady()) //r2
                                Me.Spells[R].Cast(t.ServerPosition);
                        }
                        else
                        {
                            if (!Me.Spells[W].IsReady())
                            {
                                Me.FastQCombo();
                            }
                        }
                    }
                }
            };

            MethodsOnAnimation[1] = (t, animname) =>
            {
                if (!ObjectManager.Player.Spellbook.GetSpell(Me.SummonerFlash).IsReady() && !ObjectManager.Player.HasBuff("RivenFengShuiEngine"))
                {
                    MethodsOnAnimation[0](t, animname);
                    return;
                }

                switch (animname)
                {
                    case "Spell2": //w aa
                        Me.Orbwalker.ResetAATimer();
                        break;
                    case "Spell3": //e r1
                        {
                            if (!ObjectManager.Player.HasBuff("RivenFengShuiEngine") && !Me.ConfigMenu.Item("CDISABLER").GetValue<bool>() && Me.Spells[R].IsReady())
                                Me.Spells[R].Cast();
                        }
                        break;
                    case "Spell4a": //r flash
                        {
                            if (t.Distance(ObjectManager.Player.ServerPosition) > 300)
                            {
                                ObjectManager.Player.Spellbook.CastSpell(Me.SummonerFlash, t.ServerPosition);
                            }
                        }
                        break;
                }
            };
            #endregion

            #region Flash Combo (Q1-Q2-E-R1-Flash-Q3-Hydra-W-R2)
            MethodsOnUpdate[2] = (t) =>
            {
                if (!ObjectManager.Player.Spellbook.GetSpell(Me.SummonerFlash).IsReady() && !ObjectManager.Player.HasBuff("RivenFengShuiEngine"))
                {
                    MethodsOnUpdate[0](t);
                    return;
                }

                t = Target.Get(1000);
                if (Animation.QStacks == 2)
                {
                    if (!Me.Spells[E].IsReady() && !ObjectManager.Player.HasBuff("RivenFengShuiEngine"))
                        return;

                    if (t != null)
                    {
                        if (Me.Spells[E].IsReady())
                        {
                            Me.Spells[E].Cast(t.ServerPosition);
                            return;
                        }

                        if (t.IsValidTarget(600))
                        {
                            Me.CastCrescent();
                            if (Me.Spells[W].IsReady())
                            {
                                if (t.IsValidTarget(Me.Spells[W].Range))
                                    Me.Spells[W].Cast();
                            }
                            else
                                if (ObjectManager.Player.HasBuff("RivenFengShuiEngine") && !Me.ConfigMenu.Item("CDISABLER").GetValue<bool>() && Me.Spells[R].IsReady())
                                    Me.Spells[R].Cast(t.ServerPosition);
                        }
                    }
                }
                else
                {
                    if (Me.Spells[Q].IsReady())
                    {
                        if (Utils.TickCount - Animation.LastQTick >= 1000)
                            Me.Spells[Q].Cast(Game.CursorPos);
                    }
                }
            };

            MethodsOnAnimation[2] = (t, animname) =>
            {
                {
                    switch (animname)
                    {
                        case "Spell3": //e r1
                            {
                                if (!ObjectManager.Player.HasBuff("RivenFengShuiEngine") && !Me.ConfigMenu.Item("CDISABLER").GetValue<bool>() && Me.Spells[R].IsReady())
                                    Me.Spells[R].Cast();
                            }
                            break;
                        case "Spell4a": //r1 flash
                            {
                                if (t.Distance(ObjectManager.Player.ServerPosition) > 300 && Me.Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Combo)
                                {
                                    ObjectManager.Player.Spellbook.CastSpell(Me.SummonerFlash, t.ServerPosition);
                                    Me.Spells[Q].Cast(t.ServerPosition);
                                }
                            }
                            break;
                        case "Spell2": //w r2
                            {
                                if (ObjectManager.Player.HasBuff("RivenFengShuiEngine") && !Me.ConfigMenu.Item("CDISABLER").GetValue<bool>() && Me.Spells[R].IsReady())
                                    Me.Spells[R].Cast(t.ServerPosition);
                            }
                            break;
                    }
                }
            };
            #endregion
        }
    }
}
