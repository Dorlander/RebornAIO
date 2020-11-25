using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;

namespace xSaliceResurrected.Mid
{
    class Katarina : Champion
    {
        public Katarina()
        {
            SetUpSpells();
            LoadMenu();
        }

        private void SetUpSpells()
        {
            //intalize spell
            SpellManager.Q = new Spell(SpellSlot.Q, 675);
            SpellManager.W = new Spell(SpellSlot.W, 375);
            SpellManager.E = new Spell(SpellSlot.E, 700);
            SpellManager.R = new Spell(SpellSlot.R, 550);

            SpellManager.Q.SetTargetted(400, 1400);
            SpellManager.R.SetCharged("KatarinaR", "KatarinaR", 550, 550, 1.0f);

            SpellManager.SpellList.Add(Q);
            SpellManager.SpellList.Add(W);
            SpellManager.SpellList.Add(E);
            SpellManager.SpellList.Add(R);
        }

        private void LoadMenu()
        {
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("jFarm", "Jungle Farm", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("lastHit", "Lasthit!", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("Wardjump", "Escape/Ward jump", true).SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            //Combo menu:
            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "Use Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "Use W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E", true).SetValue(true));
                combo.AddItem(new MenuItem("eDis", "E only if >", true).SetValue(new Slider(0, 0, 700)));
                combo.AddItem(new MenuItem("smartE", "Smart E with R CD ", true).SetValue(false));
                combo.AddItem(new MenuItem("UseRCombo", "Use R", true).SetValue(true));
                combo.AddItem(new MenuItem("comboMode", "Mode", true).SetValue(new StringList(new[] { "QEW", "EQW" })));
                combo.AddItem(new MenuItem("disableaa", "Disable AA").SetValue(true));
                //add to menu
                menu.AddSubMenu(combo);
            }
            //Harass menu:
            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                harass.AddItem(new MenuItem("harassMode", "Mode", true).SetValue(new StringList(new[] { "QEW", "EQW", "QW" }, 2)));
                //add to menu
                menu.AddSubMenu(harass);
            }
            //Farming menu:
            var farm = new Menu("Farm", "Farm");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q Farm", true).SetValue(false));
                farm.AddItem(new MenuItem("UseWFarm", "Use W Farm", true).SetValue(false));
                farm.AddItem(new MenuItem("UseEFarm", "Use E Farm", true).SetValue(false));
                farm.AddItem(new MenuItem("UseQHit", "Use Q Last Hit", true).SetValue(false));
                farm.AddItem(new MenuItem("UseWHit", "Use W Last Hit", true).SetValue(false));
                //add to menu
                menu.AddSubMenu(farm);
            }
            //killsteal
            var killSteal = new Menu("KillSteal", "KillSteal");
            {
                killSteal.AddItem(new MenuItem("smartKS", "Use Smart KS System", true).SetValue(true));
                killSteal.AddItem(new MenuItem("wardKs", "Use Jump KS", true).SetValue(true));
                killSteal.AddItem(new MenuItem("rKS", "Use R for KS", true).SetValue(true));
                killSteal.AddItem(new MenuItem("rCancel", "NO R Cancel for KS", true).SetValue(true));
                killSteal.AddItem(new MenuItem("KS_With_E", "Don't KS with E Toggle!", true).SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
                //add to menu
                menu.AddSubMenu(killSteal);
            }
            //Misc Menu:
            var misc = new Menu("Misc", "Misc");
            {
                misc.AddItem(new MenuItem("waitQ", "Wait For Q Mark to W", true).SetValue(true));
                misc.AddItem(new MenuItem("autoWz", "Auto W Enemy", true).SetValue(true));
                misc.AddItem(new MenuItem("E_Delay_Slider", "Delay Between E(ms)", true).SetValue(new Slider(0, 0, 1000)));
                //add to menu
                menu.AddSubMenu(misc);
            }

            //Drawings menu:
            var drawing = new Menu("Drawings", "Drawings");
            {
                drawing.AddItem(new MenuItem("QRange", "Q range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(new MenuItem("WRange", "W range", true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(new MenuItem("ERange", "E range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(new MenuItem("RRange", "R range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(new MenuItem("Draw_Mode", "Draw E Mode", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "Draw Combo Damage", true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "Draw Combo Damage Fill", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
                drawing.AddItem(drawComboDamageMenu);
                drawing.AddItem(drawFill);
                DamageIndicator.DamageToUnit = GetComboDamage;
                DamageIndicator.Enabled = drawComboDamageMenu.GetValue<bool>();
                DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
                DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;
                drawComboDamageMenu.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                    };
                drawFill.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                        DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
                    };

                //add to menu
                menu.AddSubMenu(drawing);
            }

            var customMenu = new Menu("Custom Perma Show", "Custom Perma Show");
            {
                var myCust = new CustomPermaMenu();
                customMenu.AddItem(new MenuItem("custMenu", "Move Menu", true).SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Press)));
                customMenu.AddItem(new MenuItem("enableCustMenu", "Enabled", true).SetValue(true));
                customMenu.AddItem(myCust.AddToMenu("Combo Active: ", "ComboActive"));
                customMenu.AddItem(myCust.AddToMenu("Harass Active: ", "HarassActive"));
                customMenu.AddItem(myCust.AddToMenu("Harass(T) Active: ", "HarassActiveT"));
                customMenu.AddItem(myCust.AddToMenu("Laneclear Active: ", "LaneClearActive"));
                customMenu.AddItem(myCust.AddToMenu("Jungle Active: ", "LaneClearActive"));
                customMenu.AddItem(myCust.AddToMenu("LastHit Active: ", "lastHit"));
                customMenu.AddItem(myCust.AddToMenu("WardJump Active: ", "jFarm"));
                menu.AddSubMenu(customMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            double damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            damage += MarkDmg(enemy);

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady() || (RSpell.State == SpellState.Surpressed && R.Level > 0))
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) * 8;

            damage = ItemManager.CalcDamage(enemy, damage);

            return (float)damage;
        }

        private void Combo()
        {
            Combo(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>());
        }

        private void Harass()
        {
            Harass(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>());
        }

        private void Combo(bool useQ, bool useW, bool useE, bool useR)
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            int mode = menu.Item("comboMode", true).GetValue<StringList>().SelectedIndex;

            int eDis = menu.Item("eDis", true).GetValue<Slider>().Value;

            if (!target.IsValidTarget(E.Range))
                return;

            if (!target.HasBuffOfType(BuffType.Invulnerability) && !target.IsZombie)
            {
                if (mode == 0) //qwe
                {
                    //items

                    var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                    if (itemTarget != null && E.IsReady())
                    {
                        var dmg = GetComboDamage(itemTarget);
                        ItemManager.Target = itemTarget;

                        //see if killable
                        if (dmg > itemTarget.Health - 50)
                            ItemManager.KillableTarget = true;

                        ItemManager.UseTargetted = true;
                    }


                    if (useQ && Q.IsReady() && Player.Distance(target.Position) <= Q.Range)
                    {
                        Q.Cast(target);
                    }

                    if (useE && E.IsReady() && Player.Distance(target.Position) < E.Range && Utils.TickCount - E.LastCastAttemptT > 0 &&
                        Player.Distance(target.Position) > eDis && !Q.IsReady())
                    {
                        if (menu.Item("smartE", true).GetValue<bool>() &&
                            Player.CountEnemiesInRange(500) > 2 &&
                            (!R.IsReady() || !(RSpell.State == SpellState.Surpressed && R.Level > 0)))
                            return;

                        var delay = menu.Item("E_Delay_Slider", true).GetValue<Slider>().Value;
                        OrbwalkManager.SetAttack(false);
                        OrbwalkManager.SetMovement(false);
                        E.Cast(target);
                        E.LastCastAttemptT = Utils.TickCount + delay;
                    }
                }
                else if (mode == 1) //eqw
                {
                    //items
                    var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                    if (itemTarget != null && E.IsReady())
                    {
                        var dmg = GetComboDamage(itemTarget);
                        ItemManager.Target = itemTarget;

                        //see if killable
                        if (dmg > itemTarget.Health - 50)
                            ItemManager.KillableTarget = true;

                        ItemManager.UseTargetted = true;
                    }

                    if (useE && E.IsReady() && Player.Distance(target.Position) < E.Range && Utils.TickCount - E.LastCastAttemptT > 0 &&
                        Player.Distance(target.Position) > eDis)
                    {
                        if (menu.Item("smartE", true).GetValue<bool>() &&
                            Player.CountEnemiesInRange(500) > 2 &&
                            (!R.IsReady() || !(RSpell.State == SpellState.Surpressed && R.Level > 0)))
                            return;

                        var delay = menu.Item("E_Delay_Slider", true).GetValue<Slider>().Value;
                        OrbwalkManager.SetAttack(false);
                        OrbwalkManager.SetMovement(false);
                        E.Cast(target);
                        E.LastCastAttemptT = Utils.TickCount + delay;
                    }

                    if (useQ && Q.IsReady() && Player.Distance(target.Position) <= Q.Range)
                    {
                        Q.Cast(target);
                    }
                }

                if (useW && W.IsReady() && Player.Distance(target.Position) <= W.Range && QSuccessfullyCasted())
                {
                    W.Cast();
                }

                if (useR && R.IsReady() &&
                    Player.CountEnemiesInRange(R.Range) > 0)
                {
                    if (!Q.IsReady() && !E.IsReady() && !W.IsReady())
                    {
                        OrbwalkManager.SetAttack(false);
                        OrbwalkManager.SetMovement(false);
                        R.Cast();
                    }
                }
            }
        }

        private void Harass(bool useQ, bool useW, bool useE)
        {
            Obj_AI_Hero qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            Obj_AI_Hero wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            Obj_AI_Hero eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            int mode = menu.Item("harassMode", true).GetValue<StringList>().SelectedIndex;

            if (mode == 0) //qwe
            {
                if (useQ && Q.IsReady() && qTarget != null)
                {
                    if (Player.Distance(qTarget.Position) <= Q.Range)
                        Q.Cast(qTarget);
                }

                if (useE && eTarget != null && E.IsReady() && !Q.IsReady())
                {
                    if (Player.Distance(eTarget.Position) < E.Range)
                        E.Cast(eTarget);
                }
            }
            else if (mode == 1) //eqw
            {
                if (useE && eTarget != null && E.IsReady())
                {
                    if (Player.Distance(eTarget.Position) < E.Range)
                        E.Cast(eTarget);
                }

                if (useQ && Q.IsReady() && qTarget != null)
                {
                    if (Player.Distance(qTarget.Position) <= Q.Range)
                        Q.Cast(qTarget);
                }
            }
            else if (mode == 2)
            {
                if (useQ && Q.IsReady() && qTarget != null)
                {
                    if (Player.Distance(qTarget.Position) <= Q.Range)
                        Q.Cast(qTarget);
                }
            }

            if (useW && wTarget != null && W.IsReady() && QSuccessfullyCasted())
            {
                if (Player.Distance(wTarget.Position) <= W.Range)
                    W.Cast();
            }
        }

        private void LastHit()
        {
            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All,
                MinionTeam.NotAlly);
            MinionManager.GetMinions(Player.ServerPosition, W.Range);

            var useQ = menu.Item("UseQHit", true).GetValue<bool>();
            var useW = menu.Item("UseWHit", true).GetValue<bool>();

            if (Q.IsReady() && useQ)
            {
                foreach (Obj_AI_Base minion in allMinions)
                {
                    if (minion.IsValidTarget(Q.Range) &&
                        HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion.Position) * 1000 / 1400), 200) <
                        Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        Q.Cast(minion);
                        return;
                    }
                }
            }

            if (W.IsReady() && useW)
            {
                if (allMinions.Where(minion => minion.IsValidTarget(W.Range) && minion.Health < Player.GetSpellDamage(minion, SpellSlot.W) + MarkDmg(minion) - 35).Any(minion => Player.Distance(minion.ServerPosition) < W.Range))
                {
                    W.Cast();
                }
            }
        }

        private double MarkDmg(Obj_AI_Base target)
        {
            return target.HasBuff("katarinaqmark") ? Player.GetSpellDamage(target, SpellSlot.Q, 1) : 0;
        }

        private void Farm()
        {
            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range,
                MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ && allMinionsQ.Count > 0 && Q.IsReady() && allMinionsQ[0].IsValidTarget(Q.Range))
            {
                Q.Cast(allMinionsQ[0]);
            }

            if (useE && allMinionsQ.Count > 0 && E.IsReady() && allMinionsQ[0].IsValidTarget(E.Range))
            {
                E.Cast(allMinionsE[0]);
            }

            if (useW && W.IsReady())
            {
                if (allMinionsW.Count > 0 && QSuccessfullyCasted())
                {
                    foreach (var minion in allMinionsW)
                    {
                        if (!Q.IsReady() || minion.HasBuff("katarinaqmark"))
                            W.Cast();
                    }
                }
                    
            }
        }
        private void JungleFarm()
        {
            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.Neutral);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range,
                MinionTypes.All, MinionTeam.Neutral);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();

            if (useQ && allMinionsQ.Count > 0 && Q.IsReady() && allMinionsQ[0].IsValidTarget(Q.Range))
            {
                Q.Cast(allMinionsQ[0]);
            }

            if (useW && W.IsReady())
            {
                if (allMinionsW.Count > 0)
                    W.Cast();
            }
        }

        private void SmartKs()
        {
            if (!menu.Item("smartKS", true).GetValue<bool>())
                return;

            if (menu.Item("rCancel", true).GetValue<bool>() && Player.CountEnemiesInRange(570) > 1)
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(1375) && !x.HasBuffOfType(BuffType.Invulnerability)).OrderByDescending(GetComboDamage))
            {
                if (target != null)
                {
                    var delay = menu.Item("E_Delay_Slider", true).GetValue<Slider>().Value;
                    bool shouldE = !menu.Item("KS_With_E", true).GetValue<KeyBind>().Active && Utils.TickCount - E.LastCastAttemptT > 0;
                    //QEW
                    if (Player.Distance(target.ServerPosition) <= E.Range && shouldE &&
                        (Player.GetSpellDamage(target, SpellSlot.E) + Player.GetSpellDamage(target, SpellSlot.Q) + MarkDmg(target) +
                         Player.GetSpellDamage(target, SpellSlot.W)) > target.Health + 20)
                    {
                        if (E.IsReady() && Q.IsReady() && W.IsReady())
                        {
                            CancelUlt(target);
                            Q.Cast(target);
                            E.Cast(target);
                            E.LastCastAttemptT = Utils.TickCount + delay;
                            if (Player.Distance(target.ServerPosition) < W.Range)
                                W.Cast();
                            return;
                        }
                    }

                    //E + W
                    if (Player.Distance(target.ServerPosition) <= E.Range && shouldE &&
                        (Player.GetSpellDamage(target, SpellSlot.E) + Player.GetSpellDamage(target, SpellSlot.W)) >
                        target.Health + 20)
                    {
                        if (E.IsReady() && W.IsReady())
                        {
                            CancelUlt(target);
                            E.Cast(target);
                            E.LastCastAttemptT = Utils.TickCount + delay;
                            if (Player.Distance(target.ServerPosition) < W.Range)
                                W.Cast();
                            //Game.PrintChat("ks 5");
                            return;
                        }
                    }

                    //E + Q
                    if (Player.Distance(target.ServerPosition) <= E.Range && shouldE &&
                        (Player.GetSpellDamage(target, SpellSlot.E) + Player.GetSpellDamage(target, SpellSlot.Q)) >
                        target.Health + 20)
                    {
                        if (E.IsReady() && Q.IsReady())
                        {
                            CancelUlt(target);
                            E.Cast(target);
                            E.LastCastAttemptT = Utils.TickCount + delay;
                            Q.Cast(target);
                            //Game.PrintChat("ks 6");
                            return;
                        }
                    }

                    //Q
                    if ((Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20)
                    {
                        if (Q.IsReady() && Player.Distance(target.ServerPosition) <= Q.Range)
                        {
                            CancelUlt(target);
                            Q.Cast(target);
                            //Game.PrintChat("ks 7");
                            return;
                        }
                        if (Q.IsReady() && E.IsReady() && Player.Distance(target.ServerPosition) <= 1375 &&
                            menu.Item("wardKs", true).GetValue<bool>() &&
                            target.CountEnemiesInRange(500) < 3)
                        {
                            CancelUlt(target);
                            WardJumper.JumpKs(target);
                            //Game.PrintChat("wardKS!!!!!");
                            return;
                        }
                    }

                    //E
                    if (Player.Distance(target.ServerPosition) <= E.Range && shouldE &&
                        (Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 20)
                    {
                        if (E.IsReady())
                        {
                            CancelUlt(target);
                            E.Cast(target);
                            E.LastCastAttemptT = Utils.TickCount + delay;
                            //Game.PrintChat("ks 8");
                            return;
                        }
                    }

                    //R
                    if (Player.Distance(target.ServerPosition) <= E.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.R) * 5) > target.Health + 20 &&
                        menu.Item("rKS", true).GetValue<bool>())
                    {
                        if (R.IsReady())
                        {
                            OrbwalkManager.SetAttack(false);
                            OrbwalkManager.SetMovement(false);
                            R.Cast();
                            //Game.PrintChat("ks 8");
                            return;
                        }
                    }
                }
            }
        }

        private void CancelUlt(Obj_AI_Hero target)
        {
            if (Player.IsChannelingImportantSpell() || Player.HasBuff("katarinarsound"))
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, target.ServerPosition);
                R.LastCastAttemptT = 0;
            }
        }

        private void ShouldCancel()
        {
            if (Player.CountEnemiesInRange(500) < 1)
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

                if (target == null)
                    return;

                R.LastCastAttemptT = 0;
                Player.IssueOrder(GameObjectOrder.MoveTo, target);
            }

        }

        private void AutoW()
        {
            if (!W.IsReady())
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (target != null && !target.IsDead && target.IsEnemy &&
                    Player.Distance(target.ServerPosition) <= W.Range && target.IsValidTarget(W.Range))
                {
                    if (Player.Distance(target.ServerPosition) < W.Range)
                        W.Cast();
                }
            }
        }

        private bool QSuccessfullyCasted()
        {
            return Utils.TickCount - Q.LastCastAttemptT > 350 || !menu.Item("waitQ", true).GetValue<bool>();
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (!unit.IsMe) return;

            if (args.SData.Name == "KatarinaR")
            {
                OrbwalkManager.SetAttack(false);
                OrbwalkManager.SetMovement(false);
            }

            SpellSlot castedSlot = ObjectManager.Player.GetSpellSlot(args.SData.Name);

            if (castedSlot == SpellSlot.Q)
            {
                Q.LastCastAttemptT = Utils.TickCount;
            }

            if (castedSlot == SpellSlot.R)
            {
                R.LastCastAttemptT = Utils.TickCount;
            }
        }

        protected override void ObjAiHeroOnOnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            /*if (ObjectManager.Player.IsCastingInterruptableSpell())
            {
                if (ObjectManager.Get<Obj_AI_Hero>().Any(h => h.IsEnemy && h.Health > 1 && R.IsInRange(h)))
                {
                    args.Process = false;
                }
                else
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, Player.ServerPosition.Randomize(0, 800));
                }
            }*/
            if (args.Order == GameObjectOrder.AttackUnit && menu.Item("disableaa").GetValue<bool>() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                args.Process = false;
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            SmartKs();

            if (Player.IsChannelingImportantSpell() || Player.HasBuff("KatarinaR"))
            {
                OrbwalkManager.SetAttack(false);
                OrbwalkManager.SetMovement(false);
                ShouldCancel();
                return;
            }

            OrbwalkManager.SetAttack(true);
            OrbwalkManager.SetMovement(true);
            
            if (menu.Item("Wardjump", true).GetValue<KeyBind>().Active)
            {
                OrbwalkManager.Orbwalk(null, Game.CursorPos);
                WardJumper.WardJump();
            }
            else if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("lastHit", true).GetValue<KeyBind>().Active)
                    LastHit();

                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("jFarm", true).GetValue<KeyBind>().Active)
                    JungleFarm();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();
            }

            if (menu.Item("autoWz", true).GetValue<bool>())
                AutoW();
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            foreach (Spell spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range", true).GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, (spell.IsReady()) ? Color.Cyan : Color.DarkRed);
            }

            if (menu.Item("Draw_Mode", true).GetValue<Circle>().Active)
            {
                var wts = Drawing.WorldToScreen(Player.Position);

                Drawing.DrawText(wts[0], wts[1], Color.White,
                    menu.Item("KS_With_E", true).GetValue<KeyBind>().Active ? "Ks E Active" : "Ks E Off");
            }
        }

        protected override void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Minion))
                return;

            if (Utils.TickCount < WardJumper.LastPlaced + 300)
            {
                var ward = (Obj_AI_Minion)sender;
                if (ward.Name.ToLower().Contains("ward") && ward.Distance(WardJumper.LastWardPos) < 500 && E.IsReady())
                {
                    E.Cast(ward);
                }
            }
        }
    }
}
