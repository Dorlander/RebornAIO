using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;
using Color = System.Drawing.Color;

namespace xSaliceResurrected.Top
{
    class Irelia : Champion
    {
        public Irelia()
        {
            SetSpells();
            LoadMenu();
        }
        private void SetSpells()
        {
            SpellManager.Q = new Spell(SpellSlot.Q, 650);

            SpellManager.W = new Spell(SpellSlot.W);

            SpellManager.E = new Spell(SpellSlot.E, 425);

            SpellManager.R = new Spell(SpellSlot.R, 1000);
            R.SetSkillshot(0, 80f, 1400f, false, SkillshotType.SkillshotLine);
        }

        private void LoadMenu()
        {
            //key
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitKey", "Last Hit!", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("SpellMenu", "SpellMenu");
            {
                var qMenu = new Menu("QMenu", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("Q_Min_Distance", "Min range to Q", true).SetValue(new Slider(300, 0, 600)));
                    qMenu.AddItem(new MenuItem("Q_Gap_Close", "Q Minion to Gap Close", true).SetValue(true));
                    qMenu.AddItem(new MenuItem("Q_Under_Tower", "Q Enemy Under Tower", true).SetValue(false));
                    spellMenu.AddSubMenu(qMenu);
                }

                var eMenu = new Menu("EMenu", "EMenu");
                {
                    eMenu.AddItem(new MenuItem("E_Only_Stun", "Save E to Stun", true).SetValue(true));
                    eMenu.AddItem(new MenuItem("E_Running", "E On Running Enemy", true).SetValue(true));
                    spellMenu.AddSubMenu(eMenu);
                }

                var rMenu = new Menu("RMenu", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("R_If_HP", "R If HP <=", true).SetValue(new Slider(20)));
                    //rMenu.AddItem(new MenuItem("R_Wait_Sheen", "Wait for Sheen", true).SetValue(false));

                    spellMenu.AddSubMenu(rMenu);
                }

                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "Use Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "Use W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                ManaManager.AddManaManagertoMenu(harass, "Harass", 30);
                //add to menu
                menu.AddSubMenu(harass);
            }

            var lastHit = new Menu("Lasthit", "Lasthit");
            {
                lastHit.AddItem(new MenuItem("UseQLastHit", "Use Q", true).SetValue(true));
                ManaManager.AddManaManagertoMenu(lastHit, "Lasthit", 30);
                //add to menu
                menu.AddSubMenu(lastHit);
            }

            var farm = new Menu("LaneClear", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseQFarm_Tower", "Do not Q under Tower", true).SetValue(true));
                farm.AddItem(new MenuItem("UseWFarm", "Use W", true).SetValue(true));
                farm.AddItem(new MenuItem("UseRFarm", "Use R", true).SetValue(true));
                ManaManager.AddManaManagertoMenu(farm, "LaneClear", 0);
                //add to menu
                menu.AddSubMenu(farm);
            }

            var miscMenu = new Menu("Misc", "Misc");
            {
                //miscMenu.AddItem(new MenuItem("Cast_EQ", "Cast EQ nearest target", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                miscMenu.AddItem(new MenuItem("E_Gap_Closer", "Use E On Gap Closer", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("QE_Interrupt", "Use Q/E to interrupt", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("smartKS", "Smart KS", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(miscMenu);
            }

            var drawMenu = new Menu("Drawing", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "Disable All", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "Draw Q", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "Draw E", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "Draw R", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R_Killable", "Draw R Mark on Killable", true).SetValue(true));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "Draw Combo Damage", true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "Draw Combo Damage Fill", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
                drawMenu.AddItem(drawComboDamageMenu);
                drawMenu.AddItem(drawFill);
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
                menu.AddSubMenu(drawMenu);
            }

            var customMenu = new Menu("Custom Perma Show", "Custom Perma Show");
            {
                var myCust = new CustomPermaMenu();
                customMenu.AddItem(new MenuItem("custMenu", "Move Menu", true).SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Press)));
                customMenu.AddItem(new MenuItem("enableCustMenu", "Enabled", true).SetValue(true));
                customMenu.AddItem(myCust.AddToMenu("Combo Active: ", "ComboActive"));
                customMenu.AddItem(myCust.AddToMenu("Harass Active: ", "HarassActive"));
                customMenu.AddItem(myCust.AddToMenu("Laneclear Active: ", "LaneClearActive"));
                customMenu.AddItem(myCust.AddToMenu("LastHit Active: ", "LastHitKey"));
                menu.AddSubMenu(customMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q);

            if (W.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.W) * 4;

            if (E.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.E);

            if (R.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R) * 4;

            comboDamage = ItemManager.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target) * 4);
        }

        private float GetComboDmgPercent(Obj_AI_Hero target)
        {
            double comboDamage = GetComboDamage(target);

            var predHp = target.Health - comboDamage;
            var predHpPercent = predHp / target.MaxHealth * 100;

            return (float)predHpPercent;
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, string source)
        {
            if (source == "Harass" && !ManaManager.HasMana("Harass"))
                return;

            if (useQ)
                Cast_Q();
            if (useW)
                Cast_W();
            //items
            if (source == "Combo")
            {
                var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                if (itemTarget != null)
                {
                    var dmg = GetComboDamage(itemTarget);
                    ItemManager.Target = itemTarget;

                    //see if killable
                    if (dmg > itemTarget.Health - 50)
                        ItemManager.KillableTarget = true;

                    ItemManager.UseTargetted = true;
                }
            }
            if (useE)
                Cast_E();
            if (useR)
                Cast_R();
        }

        private void Lasthit()
        {
            if (menu.Item("UseQLastHit", true).GetValue<bool>() && ManaManager.HasMana("Lasthit"))
                Cast_Q_Last_Hit();
        }

        private void Farm()
        {
            if (!ManaManager.HasMana("LaneClear"))
                return;

            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 250,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionR = MinionManager.GetMinions(Player.ServerPosition, R.Range, MinionTypes.All,
                        MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();
            var useR = menu.Item("UseRFarm", true).GetValue<bool>();

            if (useQ)
                Cast_Q_Last_Hit();

            if (useW && allMinionsW.Count > 0 && W.IsReady())
                W.Cast();

            var rPred = R.GetLineFarmLocation(allMinionR);
            if (useR && rPred.MinionsHit > 0 && R.IsReady())
                R.Cast(rPred.Position);
        }

        private void Cast_Q()
        {
            var target = TargetSelector.GetTarget(Q.Range * 2, TargetSelector.DamageType.Physical);

            if (Q.IsReady() && target != null)
            {
                if (Q.IsKillable(target))
                    Q.Cast(target);

                if (Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E) > target.Health)
                    Q.Cast(target);

                var minDistance = menu.Item("Q_Min_Distance", true).GetValue<Slider>().Value;

                if (!menu.Item("Q_Under_Tower", true).GetValue<bool>())
                    if (target.UnderTurret(true))
                        return;

                if (Player.Distance(target.Position, true) > Q.RangeSqr / 2 && menu.Item("Q_Gap_Close", true).GetValue<bool>())
                {
                    var allMinionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);

                    Obj_AI_Base bestMinion = allMinionQ[0];

                    foreach (var minion in allMinionQ)
                    {
                        double dmg = 0;

                        dmg += Player.GetSpellDamage(minion, SpellSlot.Q);
                        if (W.IsReady() || Player.HasBuffIn("ireliahitenstylecharged", 0.00f, true))
                            dmg += Player.GetSpellDamage(minion, SpellSlot.W);

                        if (target.Distance(minion.Position) < Q.Range && Player.Distance(minion.Position) < Q.Range && target.Distance(minion.Position) < target.Distance(Player.Position) && dmg > minion.Health + 40)
                            if (target.Distance(minion.Position) < target.Distance(bestMinion.Position))
                                bestMinion = minion;
                    }

                    //check if can Q without activating
                    if (bestMinion != null)
                    {
                        if (target.Distance(bestMinion.Position, true) < Q.RangeSqr && Player.Distance(bestMinion.Position, true) < Q.RangeSqr)
                        {
                            var dmg2 = Player.GetSpellDamage(bestMinion, SpellSlot.Q);

                            if (dmg2 > bestMinion.Health + 40)
                            {
                                Q.Cast(bestMinion);
                                return;
                            }

                            if (W.IsReady() || Player.HasBuffIn("ireliahitenstylecharged", 0.00f, true))
                                dmg2 += Player.GetSpellDamage(bestMinion, SpellSlot.W);

                            if (dmg2 > bestMinion.Health)
                            {
                                W.Cast();
                                Q.Cast(bestMinion);
                                return;
                            }
                        }
                    }
                }

                if (Player.Distance(target.Position) > minDistance && Player.Distance(target.Position, true) < Q.RangeSqr)
                {
                    Q.Cast(target);
                }
            }
        }

        private void Cast_Q_Last_Hit()
        {
            var allMinionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Player.BoundingRadius, MinionTypes.All, MinionTeam.NotAlly);

            if (allMinionQ.Count > 0 && Q.IsReady())
            {

                foreach (var minion in allMinionQ)
                {
                    double dmg = Player.GetSpellDamage(minion, SpellSlot.Q);

                    if (Player.HasBuffIn("ireliahitenstylecharged", 0.00f, true))
                        dmg += Player.GetSpellDamage(minion, SpellSlot.W);


                    if (dmg > minion.Health + 35)
                    {
                        if (menu.Item("UseQFarm_Tower", true).GetValue<bool>())
                        {
                            if (!minion.UnderTurret(true))
                            {
                                Q.Cast(minion);
                                return;
                            }
                        }
                        else
                            Q.Cast(minion);
                    }
                }
            }
        }

        private void Cast_W()
        {
            var target = TargetSelector.GetTarget(200, TargetSelector.DamageType.Physical);

            if (target != null && W.IsReady())
            {
                W.Cast();
            }
        }

        private void Cast_E()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (target != null && E.IsReady())
            {
                if (E.IsKillable(target))
                    E.Cast(target);

                if (menu.Item("E_Only_Stun", true).GetValue<bool>())
                {
                    var targetHealthPercent = target.Health / target.MaxHealth * 100;

                    if (Player.HealthPercent < targetHealthPercent)
                    {
                        E.Cast(target);
                        return;
                    }
                }

                if (menu.Item("E_Running", true).GetValue<bool>())
                {
                    var pred = Prediction.GetPrediction(target, 1f);

                    if (Player.Distance(target.Position) < Player.Distance(pred.UnitPosition) && Player.Distance(target.Position) > 200)
                        E.Cast(target);
                }
            }
        }

        private void Cast_R()
        {
            var target = TargetSelector.GetTarget(Player.Spellbook.GetSpell(SpellSlot.R).ToggleState == 1 ? Q.Range : R.Range,
                TargetSelector.DamageType.Physical);

            if (target != null && R.IsReady())
            {
                if (!Player.HasBuff("IreliaTranscendentBlades"))
                {
                    if (GetComboDmgPercent(target) < 25)
                        R.Cast(target);

                    var rHpValue = menu.Item("R_If_HP", true).GetValue<Slider>().Value;
                    if (Player.HealthPercent <= rHpValue)
                        R.Cast(target);
                }
                else if (Player.HasBuff("IreliaTranscendentBlades"))
                {
                    R.Cast(target);
                }
            }
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("E_Gap_Closer", true).GetValue<bool>()) return;

            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
                E.Cast(gapcloser.Sender);
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (spell.DangerLevel < Interrupter2.DangerLevel.Medium || unit.IsAlly)
                return;

            if (menu.Item("QE_Interrupt", true).GetValue<bool>())
            {
                var enemyHp = unit.Health / unit.MaxHealth * 100;
                if (Player.HealthPercent > enemyHp)
                    return;

                if (unit.IsValidTarget(E.Range))
                    E.Cast(unit);

                if (unit.IsValidTarget(Q.Range))
                {
                    Q.Cast(unit);
                    E.Cast(unit);
                }
            }
        }

        private void CheckKs()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(R.Range)).OrderByDescending(GetComboDamage))
            {
                //Q
                if (Player.ServerPosition.Distance(target.ServerPosition) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && Q.IsReady())
                {
                    Q.Cast(target);
                    return;
                }

                //E
                if (Player.ServerPosition.Distance(target.ServerPosition) <= E.Range && Player.GetSpellDamage(target, SpellSlot.E) > target.Health && E.IsReady())
                {
                    E.Cast(target);
                    return;
                }

                //R
                if (Player.ServerPosition.Distance(target.ServerPosition) <= R.Range && Player.GetSpellDamage(target, SpellSlot.R) > target.Health && R.IsReady())
                {
                    R.Cast(target);
                    return;
                }
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            if (menu.Item("smartKS", true).GetValue<bool>())
                CheckKs();

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("LastHitKey", true).GetValue<KeyBind>().Active)
                    Lasthit();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();
            }
        }

        private int _lastNotification;

        protected override void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            if (menu.Item("Draw_Q", true).GetValue<bool>())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R_Killable", true).GetValue<bool>())
            {
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(5000) && !x.IsDead && x.IsEnemy).OrderBy(x => x.Health))
                {
                    Vector2 wts = Drawing.WorldToScreen(target.Position);
                    if (GetComboDmgPercent(target) < 30 && R.IsReady())
                    {
                        if (Utils.TickCount - _lastNotification > 0)
                        {
                            Notifications.AddNotification(target.CharData.BaseSkinName + " Is Killable!", 500);
                            _lastNotification = Utils.TickCount + 5000;
                        }
                    }

                    var enemyhp = target.Health / target.MaxHealth * 100;
                    if (Player.HealthPercent < enemyhp && E.IsReady())
                        Drawing.DrawText(wts[0] - 20, wts[1] - 30, Color.White, "Stunnable");
                }
            }
        }
    }
}
