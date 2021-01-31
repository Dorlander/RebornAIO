using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
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
            Q = new Spell(SpellSlot.Q, 675);
            W = new Spell(SpellSlot.W, 375);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 550);

            Q.SetTargetted(400, 1400);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private void LoadMenu()
        {
            var key = new Menu("Key", "Key");{
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
                combo.AddItem(new MenuItem("comboMode", "Mode", true).SetValue(new StringList(new[] {"QEW", "EQW"})));
                //add to menu
                menu.AddSubMenu(combo);
            }
            //Harass menu:
            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                harass.AddItem(new MenuItem("harassMode", "Mode", true).SetValue(new StringList(new[] {"QEW", "EQW", "QW"}, 2)));
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
                killSteal.AddItem(new MenuItem("dfgKS", "Use DFG for KS", true).SetValue(true));
                killSteal.AddItem(new MenuItem("rCancel", "NO R Cancel for KS", true).SetValue(false));
                killSteal.AddItem(new MenuItem("KS_With_E", "Don't KS with E Toggle!", true).SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
                //add to menu
                menu.AddSubMenu(killSteal);
            }
            //Misc Menu:
            var misc = new Menu("Misc", "Misc");
            {
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
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            double damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q) + Player.GetSpellDamage(enemy, SpellSlot.Q, 1);

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady() || (RSpell.State == SpellState.Surpressed && R.Level > 0))
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) * 8;

            damage = ActiveItems.CalcDamage(enemy, damage);

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
                        ActiveItems.Target = itemTarget;

                        //see if killable
                        if (dmg > itemTarget.Health - 50)
                            ActiveItems.KillableTarget = true;

                        ActiveItems.UseTargetted = true;
                    }
                    

                    if (useQ && Q.IsReady() && Player.Distance(target.Position) <= Q.Range)
                    {
                        Q.Cast(target, packets());
                    }

                    if (useE && E.IsReady() && Player.Distance(target.Position) < E.Range && Environment.TickCount - E.LastCastAttemptT > 0 && 
                        Player.Distance(target.Position) > eDis)
                    {
                        if (menu.Item("smartE", true).GetValue<bool>() &&
                            countEnemiesNearPosition(target.ServerPosition, 500) > 2 &&
                            (!R.IsReady() || !(RSpell.State == SpellState.Surpressed && R.Level > 0)))
                            return;

                        var delay = menu.Item("E_Delay_Slider", true).GetValue<Slider>().Value;
                        E.Cast(target, packets());
                        E.LastCastAttemptT = Environment.TickCount + delay;
                    }
                }
                else if (mode == 1) //eqw
                {
                    //items
                    var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                    if (itemTarget != null && E.IsReady())
                    {
                        var dmg = GetComboDamage(itemTarget);
                        ActiveItems.Target = itemTarget;

                        //see if killable
                        if (dmg > itemTarget.Health - 50)
                            ActiveItems.KillableTarget = true;

                        ActiveItems.UseTargetted = true;
                    }

                    if (useE && E.IsReady() && Player.Distance(target.Position) < E.Range && Environment.TickCount - E.LastCastAttemptT > 0 &&
                        Player.Distance(target.Position) > eDis)
                    {
                        if (menu.Item("smartE", true).GetValue<bool>() &&
                            countEnemiesNearPosition(target.ServerPosition, 500) > 2 &&
                            (!R.IsReady() || !(RSpell.State == SpellState.Surpressed && R.Level > 0)))
                            return;

                        var delay = menu.Item("E_Delay_Slider", true).GetValue<Slider>().Value;
                        E.Cast(target, packets());
                        E.LastCastAttemptT = Environment.TickCount + delay;
                    }

                    if (useQ && Q.IsReady() && Player.Distance(target.Position) <= Q.Range)
                    {
                        Q.Cast(target, packets());
                    }
                }

                if (useW && W.IsReady() && Player.Distance(target.Position) <= W.Range)
                {
                    W.Cast();
                }

                if (useR && R.IsReady() &&
                    countEnemiesNearPosition(Player.ServerPosition, R.Range) > 0)
                {
                    if (!Q.IsReady() && !E.IsReady() && !W.IsReady())
                        R.Cast();
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
                        Q.Cast(qTarget, packets());
                }

                if (useE && eTarget != null && E.IsReady())
                {
                    if (Player.Distance(eTarget.Position) < E.Range)
                        E.Cast(eTarget, packets());
                }
            }
            else if (mode == 1) //eqw
            {
                if (useE && eTarget != null && E.IsReady())
                {
                    if (Player.Distance(eTarget.Position) < E.Range)
                        E.Cast(eTarget, packets());
                }

                if (useQ && Q.IsReady() && qTarget != null)
                {
                    if (Player.Distance(qTarget.Position) <= Q.Range)
                        Q.Cast(qTarget, packets());
                }
            }
            else if (mode == 2)
            {
                if (useQ && Q.IsReady() && qTarget != null)
                {
                    if (Player.Distance(qTarget.Position) <= Q.Range)
                        Q.Cast(qTarget, packets());
                }
            }

            if (useW && wTarget != null && W.IsReady())
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
                        HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion.Position) * 1000 / 1400)) <
                        Player.GetSpellDamage(minion, SpellSlot.Q) - 35)
                    {
                        Q.CastOnUnit(minion, packets());
                        return;
                    }
                }
            }

            if (W.IsReady() && useW)
            {
                if (allMinions.Where(minion => minion.IsValidTarget(W.Range) && minion.Health < Player.GetSpellDamage(minion, SpellSlot.W) - 35).Any(minion => Player.Distance(minion.ServerPosition) < W.Range))
                {
                    W.Cast();
                }
            }
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
                Q.Cast(allMinionsQ[0], packets());
            }

            if (useE && allMinionsQ.Count > 0 && E.IsReady() && allMinionsQ[0].IsValidTarget(E.Range))
            {
                E.Cast(allMinionsE[0], packets());
            }

            if (useW && W.IsReady())
            {
                if (allMinionsW.Count > 0)
                    W.Cast();
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
                Q.Cast(allMinionsQ[0], packets());
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

            if (menu.Item("rCancel", true).GetValue<bool>() && countEnemiesNearPosition(Player.ServerPosition, 570) > 1)
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(1375) && !x.HasBuffOfType(BuffType.Invulnerability)).OrderByDescending(GetComboDamage))
            {
                if (target != null)
                {
                    var delay = menu.Item("E_Delay_Slider", true).GetValue<Slider>().Value;
                    bool shouldE = !menu.Item("KS_With_E", true).GetValue<KeyBind>().Active && Environment.TickCount - E.LastCastAttemptT > 0;
                    //QEW
                    if (Player.Distance(target.ServerPosition) <= E.Range && shouldE &&
                        (Player.GetSpellDamage(target, SpellSlot.E) + Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1) + 
                         Player.GetSpellDamage(target, SpellSlot.W)) > target.Health + 20)
                    {
                        if (E.IsReady() && Q.IsReady() && W.IsReady())
                        {
                            CancelUlt(target);
                            Q.Cast(target, packets());
                            E.Cast(target, packets());
                            E.LastCastAttemptT = Environment.TickCount + delay;
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
                            E.Cast(target, packets());
                            E.LastCastAttemptT = Environment.TickCount + delay;
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
                            E.Cast(target, packets());
                            E.LastCastAttemptT = Environment.TickCount + delay;
                            Q.Cast(target, packets());
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
                            Q.Cast(target, packets());
                            //Game.PrintChat("ks 7");
                            return;
                        }
                        if (Q.IsReady() && E.IsReady() && Player.Distance(target.ServerPosition) <= 1375 &&
                            menu.Item("wardKs", true).GetValue<bool>() &&
                            countEnemiesNearPosition(target.ServerPosition, 500) < 3)
                        {
                            CancelUlt(target);
                            JumpKs(target);
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
                            E.Cast(target, packets());
                            E.LastCastAttemptT = Environment.TickCount + delay;
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
                            R.Cast();
                            //Game.PrintChat("ks 8");
                            return;
                        }
                    }
                    if (menu.Item("dfgKS", true).GetValue<bool>())
                    {
                        //dfg
                        if (Dfg.IsReady() && Player.GetItemDamage(target, Damage.DamageItems.Dfg) > target.Health + 20 &&
                            Player.Distance(target.ServerPosition) <= 750)
                        {
                            Items.UseItem(Dfg.Id, target);
                            //Game.PrintChat("ks 1");
                            return;
                        }

                        //dfg + q
                        if (Player.Distance(target.ServerPosition) <= Q.Range &&
                            (Player.GetItemDamage(target, Damage.DamageItems.Dfg) +
                             (Player.GetSpellDamage(target, SpellSlot.Q))*1.2) > target.Health + 20)
                        {
                            if (Dfg.IsReady() && Q.IsReady())
                            {
                                Items.UseItem(Dfg.Id, target);
                                CancelUlt(target);
                                Q.Cast(target, packets());
                                //Game.PrintChat("ks 2");
                                return;
                            }
                        }

                        //dfg + e
                        if (Player.Distance(target.ServerPosition) <= E.Range &&
                            (Player.GetItemDamage(target, Damage.DamageItems.Dfg) +
                             (Player.GetSpellDamage(target, SpellSlot.E))*1.2) > target.Health + 20)
                        {
                            if (Dfg.IsReady() && E.IsReady())
                            {
                                Items.UseItem(Dfg.Id, target);
                                CancelUlt(target);
                                E.Cast(target, packets());
                                E.LastCastAttemptT = Environment.TickCount + delay;
                                //Game.PrintChat("ks 3");
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void CancelUlt(Obj_AI_Hero target)
        {
            if (Player.IsChannelingImportantSpell() || Player.HasBuffIn("katarinarsound",0f, true))
            {
                //xSLxOrbwalker.Orbwalk(target.ServerPosition, null);
                Player.IssueOrder(GameObjectOrder.MoveTo, target.ServerPosition);
                xSLxOrbwalker.R.LastCastAttemptT = 0;
                R.LastCastAttemptT = 0;
            }
        }

        private void ShouldCancel()
        {
            if (countEnemiesNearPosition(Player.ServerPosition, 600) < 1)
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

                if (target == null)
                    return;

                R.LastCastAttemptT = 0;
                Player.IssueOrder(GameObjectOrder.MoveTo, target);
                xSLxOrbwalker.R.LastCastAttemptT = 0;
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

        //wardjump
        //-------------------------------------------------

        private void JumpKs(Obj_AI_Hero target)
        {
            foreach (Obj_AI_Minion ward in ObjectManager.Get<Obj_AI_Minion>().Where(ward =>
                E.IsReady() && Q.IsReady() && ward.Name.ToLower().Contains("ward") &&
                ward.Distance(target.ServerPosition) < Q.Range && ward.Distance(Player.Position) < E.Range))
            {
                E.Cast(ward);
                return;
            }

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero =>
                E.IsReady() && Q.IsReady() && hero.Distance(target.ServerPosition) < Q.Range &&
                hero.Distance(Player.Position) < E.Range && hero.IsValidTarget(E.Range)))
            {
                E.Cast(hero);
                return;
            }

            foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion =>
                E.IsReady() && Q.IsReady() && minion.Distance(target.ServerPosition) < Q.Range &&
                minion.Distance(Player.Position) < E.Range && minion.IsValidTarget(E.Range)))
            {
                E.Cast(minion);
                return;
            }

            if (Player.Distance(target.Position) < Q.Range)
            {
                Q.Cast(target, packets());
                return;
            }

            if (E.IsReady() && Q.IsReady())
            {
                Vector3 position = Player.ServerPosition +
                                   Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * 590;

                if (target.Distance(position) < Q.Range)
                {
                    InventorySlot invSlot = FindBestWardItem();
                    if (invSlot == null) return;

                    Player.Spellbook.CastSpell(invSlot.SpellSlot, position);
                    LastWardPos = position;
                    LastPlaced = Environment.TickCount;
                }
            }

            if (Player.Distance(target.Position) < Q.Range)
            {
                Q.Cast(target, packets());
            }
        }

        private void WardJump()
        {
            //wardWalk(Game.CursorPos);

            foreach (Obj_AI_Minion ward in ObjectManager.Get<Obj_AI_Minion>().Where(ward =>
                ward.Name.ToLower().Contains("ward") && ward.Distance(Game.CursorPos) < 250))
            {
                if (E.IsReady())
                {
                    E.CastOnUnit(ward);
                    return;
                }
            }

            foreach (
                Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.Distance(Game.CursorPos) < 250 && !hero.IsDead))
            {
                if (E.IsReady())
                {
                    E.CastOnUnit(hero);
                    return;
                }
            }

            foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion =>
                minion.Distance(Game.CursorPos) < 250))
            {
                if (E.IsReady())
                {
                    E.CastOnUnit(minion);
                    return;
                }
            }

            if (Environment.TickCount <= LastPlaced + 3000 || !E.IsReady()) return;

            Vector3 cursorPos = Game.CursorPos;
            Vector3 myPos = Player.ServerPosition;

            Vector3 delta = cursorPos - myPos;
            delta.Normalize();

            Vector3 wardPosition = myPos + delta * (600 - 5);

            InventorySlot invSlot = FindBestWardItem();
            if (invSlot == null) return;

            Items.UseItem((int)invSlot.Id, wardPosition);
            LastWardPos = wardPosition;
            LastPlaced = Environment.TickCount;
        }

        private static InventorySlot FindBestWardItem()
        {
            InventorySlot slot = Items.GetWardSlot();
            if (slot == default(InventorySlot)) return null;
            return slot;
        }

        //end wardjump
        //-------------------------------------------------
        //-------------------------------------------------

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (!unit.IsMe) return;

            SpellSlot castedSlot = ObjectManager.Player.GetSpellSlot(args.SData.Name);

            if (castedSlot == SpellSlot.R)
            {
                //Game.PrintChat("RAWR 2");
                R.LastCastAttemptT = Environment.TickCount;
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            SmartKs();

            if (Player.IsChannelingImportantSpell() || Player.HasBuffIn("katarinarsound",0f, true) || Player.HasBuff("KatarinaR"))
            {
                if (menu.Item("Orbwalker_Mode", true).GetValue<bool>())
                {
                    Orbwalker.SetAttack(false);
                    Orbwalker.SetMovement(false);
                }

                ShouldCancel();
                return;
            }

            if (menu.Item("Orbwalker_Mode", true).GetValue<bool>())
            {
                Orbwalker.SetAttack(true);
                Orbwalker.SetMovement(true);
            }

            if (menu.Item("Wardjump", true).GetValue<KeyBind>().Active)
            {
                WardJump();
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

            if (Environment.TickCount < LastPlaced + 300)
            {
                var ward = (Obj_AI_Minion)sender;
                if (ward.Name.ToLower().Contains("ward") && ward.Distance(LastWardPos) < 500 && E.IsReady())
                {
                    E.Cast(ward);
                }
            }
        }
    }
}
