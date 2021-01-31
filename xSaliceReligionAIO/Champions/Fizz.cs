using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Fizz : Champion
    {
        public Fizz()
        {
            LoadSpells();
            LoadMenu();
            Q.LastCastAttemptT = Environment.TickCount;
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 725);
            R = new Spell(SpellSlot.R, 1300);

            E.SetSkillshot(0.5f, 270f, 1300, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 120f, 1350f, false, SkillshotType.SkillshotLine);
        }

        private void LoadMenu()
        {
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("Flee", "Escape with E", true).SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("SpellMenu", "SpellMenu");
            {
                var qMenu = new Menu("QMenu", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("Q_Min_Dist", "Min Distance to use E", true).SetValue(new Slider(100, 1, 475)));
                    spellMenu.AddSubMenu(qMenu);
                }

                var eMenu = new Menu("EMenu", "EMenu");
                {
                    eMenu.AddItem(new MenuItem("E_Min_Dist", "Min Distance to use E", true).SetValue(new Slider(100, 1, 400)));
                    spellMenu.AddSubMenu(eMenu);
                }

                var rMenu = new Menu("RMenu", "RMenu");
                {   
                    rMenu.AddItem(new MenuItem("rBestTarget", "Shoot R to Best Target", true).SetValue(new KeyBind("R".ToCharArray()[0], KeyBindType.Press)));
                    rMenu.AddItem(new MenuItem("R_Delay", "Delay to shoot R During Q(ms)", true).SetValue(new Slider(100, 1, 500)));
                    rMenu.AddItem(new MenuItem("R_Max_Dist", "R Max Distance", true).SetValue(new Slider(1000, 200, 1300)));
                    rMenu.AddItem(new MenuItem("ROverkill", "R OverKill Check", true).SetValue(false));
                    rMenu.AddItem(new MenuItem("AlwaysR", "Always Use R in Combo", true).SetValue(true));
                    spellMenu.AddSubMenu(rMenu);
                }
                //add to menu
                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("Combo_mode", "Combo Mode", true).SetValue(new StringList(new[] { "R-W-Q-E (Normal)", "W-Q-R-E(R During Q)", "R-E-W-Q (R->E gap)" }, 1)));
                combo.AddItem(new MenuItem("Combo_Switch", "Switch mode Key", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
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
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(true));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                harass.AddItem(new MenuItem("turretCheck", "Do not use under enemy Turret", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(harass);
            }
            var farm = new Menu("Farming", "Farming");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q Farm", true).SetValue(true));
                farm.AddItem(new MenuItem("UseWFarm", "Use W Farm", true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "Use E Farm", true).SetValue(true));
                farm.AddItem(new MenuItem("LaneClear_useE_minHit", "Use E if min. hit", true).SetValue(new Slider(2, 1, 6)));
                //add to menu
                menu.AddSubMenu(farm);
            }

            var misc = new Menu("Misc", "Misc");
            {
                misc.AddItem(new MenuItem("smartKS", "Use Smart KS System", true).SetValue(true));
                menu.AddSubMenu(misc);
            }

            var drawMenu = new Menu("Drawing", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "Disable All", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "Draw Q", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "Draw E", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "Draw R", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_Mode", "Draw Modes", true).SetValue(true));

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
            }
            //add to menu
            menu.AddSubMenu(drawMenu);
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            if (enemy == null)
                return 0;

            double damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W)*3;

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
            {
                damage += damage*1.2;
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            damage += Player.GetAutoAttackDamage(enemy)*3;

            damage = ActiveItems.CalcDamage(enemy, damage);

            return (float)damage;
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
            int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
            var range = R.IsReady() || E.IsReady() ? R.Range : Q.Range;

            if (source == "Harass")
                range = E.Range;

            var target = TargetSelector.GetTarget(range, TargetSelector.DamageType.Magical);

            if (target == null)
                return;

            if (source == "Harass" && menu.Item("turretCheck", true).GetValue<bool>())
            {
                if (target.UnderTurret(true))
                    return;
            }

            var dmg = GetComboDamage(target);

            if (source == "Combo" && Q.IsInRange(target))
            {
                ActiveItems.Target = target;

                //see if killable
                if (dmg > target.Health - 50)
                    ActiveItems.KillableTarget = true;

                ActiveItems.UseTargetted = true;
            }

            switch (mode)
            {
                case 0://R-W-Q-E
                    if (useR && R.IsReady())
                    {
                        if (ShouldCastR(target, dmg))
                        {
                            if (R.GetPrediction(target).Hitchance >= HitChance.High)
                            {
                                CastR(R.GetPrediction(target).CastPosition);
                            }
                        }
                    }

                    if (!R.IsReady() || dmg < target.Health - 100 || !useR)
                    {
                        if (useW && W.IsReady())
                        {
                            if(ShouldCastW(target))
                                W.Cast(packets());
                        }

                        if (useQ && Q.IsReady())
                        {
                            if(ShouldCastQ(target))
                                Q.CastOnUnit(target, packets());
                        }

                        if (useE && E.IsReady())
                        {
                            if (ShouldCastE(target))
                            {
                                CastE(target);
                            }
                        }
                    }
                    break;

                case 1://W-Q-R-E
                    if (useW && W.IsReady())
                    {
                        if (ShouldCastW(target))
                            W.Cast(packets());
                    }

                    if (useQ && Q.IsReady())
                    {
                        if (ShouldCastQ(target))
                        {
                            Q.CastOnUnit(target, packets());

                            if (R.IsReady() && ShouldCastR(target, dmg) && useR)
                            { 
                                _qDelay = (int)(Player.Distance(target.Position) / 2.2);
                                _qVec = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * Q.Range;
                                _qLast = Environment.TickCount;
                            }
                        }
                    }
                    if (useE && E.IsReady() && !Q.IsReady())
                    {
                        if (ShouldCastE(target, true))
                        {
                            CastE(target);
                        }
                    }
                    break;
                case 2://R-E-W-Q (Gap)
                    if (useR && R.IsReady())
                    {
                        if (ShouldCastR(target, dmg))
                        {
                            if (R.GetPrediction(target).Hitchance >= HitChance.High)
                            {
                                CastR(R.GetPrediction(target).CastPosition);
                            }
                        }
                    }

                    if (!R.IsReady() || dmg < target.Health - 100 || !useR)
                    {
                        if (useE && E.IsReady())
                        {
                            if (ShouldCastE(target, true))
                            {
                                CastE(target);
                                return;
                            }
                        }

                        if (useW && W.IsReady())
                        {
                            if(ShouldCastW(target))
                                W.Cast(packets());
                        }

                        if (useQ && Q.IsReady())
                        {
                            if(ShouldCastQ(target))
                                Q.CastOnUnit(target, packets());
                        }
                    }
                    break;
            }
        }

        private void CheckKs()
        {
            var range = E.IsReady() ? (E.Range * 2 - 50 + Q.Range) : Q.Range;
            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(range)).OrderByDescending(GetComboDamage))
            {
                if (target != null) {
                    if ((Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.W) + Player.GetAutoAttackDamage(target) + 
                        (Items.HasItem(3100) ? Player.CalcDamage(target, Damage.DamageType.Magical, ActiveItems.LichDamage()) : 0)) >
                        target.Health && Q.IsReady() && W.IsReady())
                    {
                        if (Player.Distance(target.Position) < Q.Range)
                        {
                            W.Cast(packets());
                            Q.CastOnUnit(target, packets());
                            return;
                        }
                        if(Player.Distance(target.Position) < range && !target.UnderTurret() && target.CountEnemiesInRange(600) < 2 && E.IsReady())
                        {
                            E.Cast(target);
                            Obj_AI_Hero target1 = target;
                            Utility.DelayAction.Add(200, () =>  E.Cast(target1));
                            return;
                        }
                    }


                    if (Player.Distance(target.Position) < Q.Range && Q.IsReady() && (Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.W) + Player.GetAutoAttackDamage(target) +
                        (Items.HasItem(3100) ? Player.CalcDamage(target, Damage.DamageType.Magical, ActiveItems.LichDamage()) : 0)) >
                        target.Health)
                    {
                        Q.Cast(target, packets());
                        return;
                    }

                    if (Player.Distance(target.Position) < E.Range * 2 - 50 && E.IsReady() && E.IsKillable(target) && !target.UnderTurret() && target.CountEnemiesInRange(600) < 2)
                    {
                        CastE(target);
                        return;
                    }
                }
            }
        }

        private void CastR(Vector3 pos)
        {
            var vec = Player.ServerPosition + Vector3.Normalize(pos - Player.ServerPosition)*1200;

            R.Cast(vec, packets());

        }
        private void CastE(Obj_AI_Hero target)
        {
            if (Player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzJump")
                E.Cast(target, packets());

            if (Player.Spellbook.GetSpell(SpellSlot.E).Name == "fizzjumptwo" && Environment.TickCount - E.LastCastAttemptT > 50)
                E.Cast(target, packets());
        }

        private void CastE(Vector2 vec)
        {
            if (Player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzJump")
                E.Cast(vec, packets());

            if (Player.Spellbook.GetSpell(SpellSlot.E).Name == "fizzjumptwo" && Environment.TickCount - E.LastCastAttemptT > 50)
                E.Cast(vec, packets());
        }

        private bool ShouldCastQ(Obj_AI_Hero target)
        {
            if (Player.Distance(target.Position) > menu.Item("Q_Min_Dist", true).GetValue<Slider>().Value && Player.Distance(target.Position) < Q.Range)
                return true;

            return false;
        }

        private bool ShouldCastW(Obj_AI_Hero target)
        {
            if (Player.Distance(target.Position) < Q.Range + 100 && Q.IsReady())
                return true;

            if (Player.Distance(target.Position) < 250)
                return true;

            return false;
        }

        private bool ShouldCastE(Obj_AI_Hero target, bool gap = false)
        {
            if (Player.Spellbook.GetSpell(SpellSlot.E).Name == "fizzjumptwo")
                return true;

            if (Player.Distance(target.Position) > menu.Item("E_Min_Dist", true).GetValue<Slider>().Value && Player.Distance(target.Position) < E.Range*2 - 50)
                return true;

            if (gap && Player.Distance(target.Position) < 1000)
                return true;

            return false;
        }

        private bool ShouldCastR(Obj_AI_Hero target, float dmg)
        {
            if (menu.Item("AlwaysR", true).GetValue<bool>())
                return true;

            if (menu.Item("ROverkill", true).GetValue<bool>())
            {
                if (dmg - Player.GetSpellDamage(target, SpellSlot.R) > target.Health + 75)
                    return false;
            }

            if (dmg > target.Health - 100)
                return true;

            return false;
        }

        private void Farm()
        {
            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 300,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range,
                MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useW && W.IsReady() && allMinionsW.Count > 0)
                W.Cast();

            if (useQ && Q.IsReady())
                Q.Cast(allMinionsQ[0]);

            if (useE && E.IsReady())
            {
                var pred = E.GetCircularFarmLocation(allMinionsE);

                if (pred.MinionsHit >= menu.Item("LaneClear_useE_minHit", true).GetValue<Slider>().Value)
                    CastE(pred.Position);
            }
        }

        private void Flee()
        {
            if (E.IsReady())
            {
                var eVec = Player.ServerPosition + Vector3.Normalize(Game.CursorPos - Player.ServerPosition)*400;
                E.Cast(eVec);
            }

            if (!Q.IsReady()) return;
            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Distance(Player.Position) < Q.Range && !x.IsAlly).OrderBy(x => x.Distance(Game.CursorPos)))
            {
                Q.Cast(minion);
                return;
            }
        }

        private void CastBestR()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (target == null)
                return;
            
            R.Cast(target);
        }

        private int _qLast;
        private Vector3 _qVec;
        private int _qDelay;

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (unit.IsMe)
            {
                SpellSlot castedSlot = ObjectManager.Player.GetSpellSlot(args.SData.Name);

                if (castedSlot == SpellSlot.Q)
                {
                    if (R.IsReady() && Environment.TickCount - _qLast < 250)
                    {
                        var vec = _qVec + Vector3.Normalize(Prediction.GetPrediction((Obj_AI_Hero)args.Target, _qDelay).CastPosition - _qVec) * 600;

                        var delay = menu.Item("R_Delay", true).GetValue<Slider>().Value;
                        Utility.DelayAction.Add(delay, () => R.Cast(vec, packets()));
                    }
                }
                if (castedSlot == SpellSlot.E)
                {
                    E.LastCastAttemptT = Environment.TickCount;
                }
            }
        }

        private int _lasttick;

        private void ModeSwitch()
        {
            int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
            int lasttime = Environment.TickCount - _lasttick;

            if (menu.Item("Combo_Switch", true).GetValue<KeyBind>().Active && lasttime > Game.Ping)
            {
                if (mode == 0)
                {
                    menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "R-W-Q-E (Normal)", "W-Q-R-E(R During Q)", "R-E-W-Q (R->E gap)" }, 1));
                    _lasttick = Environment.TickCount + 300;
                }
                else if (mode == 1)
                {
                    menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "R-W-Q-E (Normal)", "W-Q-R-E(R During Q)", "R-E-W-Q (R->E gap)" }, 2));
                    _lasttick = Environment.TickCount + 300;
                }
                else
                {
                    menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "R-W-Q-E (Normal)", "W-Q-R-E(R During Q)", "R-E-W-Q (R->E gap)" }));
                    _lasttick = Environment.TickCount + 300;
                }
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            ModeSwitch();

            //ks check
            if (menu.Item("smartKS", true).GetValue<bool>())
                CheckKs();

            if (menu.Item("rBestTarget", true).GetValue<KeyBind>().Active)
            {
                CastBestR();
            }

            if (menu.Item("Flee", true).GetValue<KeyBind>().Active)
            {
                Flee();
            }
            else if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();
            }

            R.Range = menu.Item("R_Max_Dist", true).GetValue<Slider>().Value;
        }

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

            if (menu.Item("Draw_Mode", true).GetValue<bool>())
            {
                Vector2 wts = Drawing.WorldToScreen(Player.Position);
                int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
                if (mode == 0)
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "R-W-Q-E (Normal)");
                else if (mode == 1)
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "W-Q-R-E(R During Q)");
                else if (mode == 2)
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "R-E-W-Q (R->E gap)");
            }
        }
    }
}
