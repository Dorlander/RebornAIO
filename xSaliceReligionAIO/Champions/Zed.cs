using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Zed : Champion
    {
        public Zed()
        {
            LoadSpells();
            LoadMenu();
        }

        private Vector3 _currentWShadow = Vector3.Zero;
        private Vector3 _currentRShadow = Vector3.Zero;

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 900f);
            W = new Spell(SpellSlot.W, 550f);
            E = new Spell(SpellSlot.E, 220f);
            R = new Spell(SpellSlot.R, 650f);

            Q.SetSkillshot(.25f, 60f, 1700f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(.25f, 270f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0f, 220f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        private void LoadMenu()
        {
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitQ", "Last hit with Q!", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("Switch_1", "Line Mode", true).SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("Switch_2", "Coax Mode", true).SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("Escape", "W To Mouse Escape", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("SpellMenu", "SpellMenu");
            {
                var wMenu = new Menu("WMenu", "WMenu");
                {
                    wMenu.AddItem(new MenuItem("W_Require_QE", "Require both Q/E to hit on W Harass", true).SetValue(true));
                    wMenu.AddItem(new MenuItem("W_Follow_Combo", "Follow W in Line Combo", true).SetValue(false));
                    wMenu.AddItem(new MenuItem("useW_Health", "Use W swap if health below", true).SetValue(new Slider(25)));
                    spellMenu.AddSubMenu(wMenu);
                }

                var rMenu = new Menu("RMenu", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("R_Place_line", "R Range behind target in Line", true).SetValue(new Slider(400, 250, 550)));
                    rMenu.AddItem(new MenuItem("R_Back", "R Swap if Enemy Is dead", true).SetValue(true));
                    rMenu.AddItem(new MenuItem("useR_Health", "Use R swap if health below", true).SetValue(new Slider(10)));

                    //evading spells
                    var dangerous = new Menu("Dodge Dangerous", "Dodge Dangerous");
                    {
                        foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
                        {
                            dangerous.AddSubMenu(new Menu(hero.ChampionName, hero.ChampionName));
                            dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.Q).Name + "R_Dodge", hero.Spellbook.GetSpell(SpellSlot.Q).Name, true).SetValue(false));
                            dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.W).Name + "R_Dodge", hero.Spellbook.GetSpell(SpellSlot.W).Name, true).SetValue(false));
                            dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.E).Name + "R_Dodge", hero.Spellbook.GetSpell(SpellSlot.E).Name, true).SetValue(false));
                            dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.R).Name + "R_Dodge", hero.Spellbook.GetSpell(SpellSlot.R).Name, true).SetValue(false));
                        }
                        rMenu.AddSubMenu(dangerous);
                    }
                    spellMenu.AddSubMenu(rMenu);
                }
                //add to menu
                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("selected", "Focus Selected Target", true).SetValue(true));
                combo.AddItem(new MenuItem("Combo_mode", "Combo Mode", true).SetValue(new StringList(new[] { "Normal", "Line Combo", "Coax", "Ult no W", "Normal With Ult" })));
                combo.AddItem(new MenuItem("Combo_Switch", "Switch mode Key", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                combo.AddItem(new MenuItem("UseQCombo", "Use Q", true).SetValue(true));
                combo.AddItem(new MenuItem("Prioritize_Q", "Prioritize Q over W->Q", true).SetValue(true));
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
                //add to menu
                menu.AddSubMenu(harass);
            }
            var farm = new Menu("LaneClear", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "Use E", true).SetValue(true));
                farm.AddItem(new MenuItem("LaneClear_useE_minHit", "Use E if min. hit", true).SetValue(new Slider(2, 1, 6)));
                //add to menu
                menu.AddSubMenu(farm);
            }
            var misc = new Menu("Misc", "Misc");
            {
                misc.AddItem(new MenuItem("smartKS", "Use Smart KS System", true).SetValue(true));
                misc.AddItem(new MenuItem("Use_W_KS", "Use W for KS", true).SetValue(true));
                misc.AddItem(new MenuItem("AutoE", "Auto E in range", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(misc);
            }
            var drawMenu = new Menu("Drawing", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "Disable All", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "Draw Q", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_W", "Draw W", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "Draw E", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "Draw R", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Current_Mode", "Draw current Mode", true).SetValue(true));

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
        }

        private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q) * 2;

            if (W.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.W);

            if (E.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.E) * 2;

            if ((target.Health / target.MaxHealth * 100) <= 50)
                comboDamage += CalcPassive(target);

            if (HasBuff(target, "zedulttargetmark"))
            {
                if (R.Level == 1)
                    comboDamage += comboDamage * 1.2;
                else if(R.Level == 2)
                    comboDamage += comboDamage * 1.35;
                else if(R.Level == 3)
                    comboDamage += comboDamage * 1.5;
            }
            if (R.IsReady())
            {
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R);

                if (R.Level == 1)
                    comboDamage += comboDamage * 1.2;
                else if (R.Level == 2)
                    comboDamage += comboDamage * 1.35;
                else if (R.Level == 3)
                    comboDamage += comboDamage * 1.5;
            }

            comboDamage = ActiveItems.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target) * 2);
        }

        private double CalcPassive(Obj_AI_Base target)
        {
            double dmg = 0;
            
            if (Player.Level > 16)
            {
                double hp = target.MaxHealth * .1;
                dmg += Player.CalcDamage(target, Damage.DamageType.Magical, hp);
            }
            else if (Player.Level > 6)
            {
                double hp = target.MaxHealth * .08;
                dmg += Player.CalcDamage(target, Damage.DamageType.Magical, hp);
            }
            else
            {
                double hp = target.MaxHealth * .06;
                dmg += Player.CalcDamage(target, Damage.DamageType.Magical, hp);
            }

            return dmg;
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
            int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var target = TargetSelector.GetTarget(Q.Range + W.Range, TargetSelector.DamageType.Physical);
            
            switch (mode)
            {
                case 0:
                    if (qTarget != null)
                    {
                        //items
                        var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);

                        if (GetMarked() != null)
                            itemTarget = GetMarked();

                        if (itemTarget != null)
                        {
                            var dmg = GetComboDamage(itemTarget);
                            ActiveItems.Target = itemTarget;

                            //see if killable
                            if (dmg > itemTarget.Health - 50)
                                ActiveItems.KillableTarget = true;

                            ActiveItems.UseTargetted = true;
                        }
                        
                    }

                    if (menu.Item("Prioritize_Q", true).GetValue<bool>())
                    {
                        if (useQ)
                            Cast_Q();

                        if (HasEnergy(false, W.IsReady() && useW, E.IsReady() && useE))
                        {
                            if (useW)
                                Cast_W("Combo", false, useE);
                        }
                    }
                    else
                    {
                        if (HasEnergy(Q.IsReady() && useQ, W.IsReady() && useW, E.IsReady() && useE))
                        {
                            if (useW)
                                Cast_W("Combo", useQ, useE);
                        }
                        if (useQ && (!W.IsReady() || WSpell.ToggleState == 2))
                        {
                            Cast_Q();
                        }
                    }

                    if (useE)
                        Cast_E();

                    if (WShadow == null)
                        return;

                    if(target == null)
                        return;

                    if (menu.Item("W_Follow_Combo", true).GetValue<bool>() && WSpell.ToggleState == 2 && Player.Distance(target.Position) > WShadow.Distance(target.Position) && HasBuff(target, "zedulttargetmark"))
                        W.Cast(packets());

                    break;
                //line
                case 1:
                    if(useR)
                        LineCombo(useQ, useE);
                    else
                        menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "Normal", "Line Combo", "Coax", "Ult no W", "Normal With Ult" }));
                break;
                //Coax
                case 2:
                    CoaxCombo(useQ, useE);
                break;
                //ham
                case 3:
                    if (qTarget != null)
                    {
                        var dmg = GetComboDamage(qTarget);

                        float range = Q.Range;
                        if (GetTargetFocus(range) != null)
                            qTarget = GetTargetFocus(range);

                        if (dmg > qTarget.Health + 50 && qTarget.IsValidTarget(R.Range) && HasEnergy(true, true, false))
                            R.CastOnUnit(qTarget, packets());

                        if (GetMarked() != null)
                            qTarget = GetMarked();

                        //items
                        var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);

                        if (GetMarked() != null)
                            itemTarget = GetMarked();

                        if (itemTarget != null)
                        {
                            ActiveItems.Target = itemTarget;

                            //see if killable
                            if (dmg > itemTarget.Health - 50)
                                ActiveItems.KillableTarget = true;

                            ActiveItems.UseTargetted = true;
                        }
                    }

                    if (useQ)
                    {
                        Cast_Q();
                    }
                    
                    if (useE)
                        Cast_E();
                    break;
                //Normal /w Ult
                case 4:
                    if (qTarget != null)
                    {
                        var dmg2 = GetComboDamage(qTarget);

                        float range = Q.Range;
                        if (GetTargetFocus(range) != null)
                            qTarget = GetTargetFocus(range);

                        if (dmg2 > qTarget.Health + 50 && qTarget.IsValidTarget(R.Range) && HasEnergy(true, true, false))
                            R.CastOnUnit(qTarget, packets());

                        //items
                        var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);

                        if (GetMarked() != null)
                            itemTarget = GetMarked();

                        if (itemTarget != null)
                        {
                            var dmg = GetComboDamage(itemTarget);
                            ActiveItems.Target = itemTarget;

                            //see if killable
                            if (dmg > itemTarget.Health - 50)
                                ActiveItems.KillableTarget = true;

                            ActiveItems.UseTargetted = true;
                        }
                    }

                    if (menu.Item("Prioritize_Q", true).GetValue<bool>())
                    {
                        if (useQ)
                            Cast_Q();

                        if (HasEnergy(false, W.IsReady() && useW, E.IsReady() && useE))
                        {
                            if (useW)
                                Cast_W("Combo", false, useE);
                        }
                    }
                    else
                    {
                        if (HasEnergy(Q.IsReady() && useQ, W.IsReady() && useW, E.IsReady() && useE))
                        {
                            if (useW)
                                Cast_W("Combo", useQ, useE);
                        }
                        if (useQ && (!W.IsReady() || WSpell.ToggleState == 2))
                        {
                            Cast_Q();
                        }
                    }

                    if (useE)
                        Cast_E();

                    if (WShadow == null)
                        return;

                    if(target == null)
                        return;

                    if (menu.Item("W_Follow_Combo", true).GetValue<bool>() && WSpell.ToggleState == 2 && Player.Distance(target.Position) > WShadow.Distance(target.Position) && HasBuff(target, "zedulttargetmark"))
                        W.Cast(packets());
                    break;

            }
        }

        private int _coaxDelay;

        private void CoaxCombo(bool useQ, bool useE)
        {
            var target = TargetSelector.GetTarget(W.Range + Q.Range, TargetSelector.DamageType.Physical);

            if (target == null)
                return;

            float range = W.Range + Q.Range;
            if (GetTargetFocus(range) != null)
                target = GetTargetFocus(range);

            if (GetMarked() != null)
                target = GetMarked();

            if (W.IsReady() && WSpell.ToggleState == 0)
            {
                Cast_W("Coax", useQ, useE);
                _coaxDelay = Environment.TickCount + 500;
                return;
            }

            if (WShadow == null)
                return;

            if (WShadow.Distance(target.Position) > R.Range - 100)
            {
            }
            else
            {
                if (useQ && (_qCooldown - Game.Time) > (QSpell.Cooldown / 3))
                    return;
                if (useE && !E.IsReady())
                    return;
            }

            if (WShadow != null && HasEnergy(Q.IsReady() && useQ, false, E.IsReady() && useE) && Environment.TickCount - _coaxDelay > 0)
            {
                if (WSpell.ToggleState == 2 && WShadow.Distance(target.Position) < R.Range)
                {
                    W.Cast(packets());
                    Utility.DelayAction.Add(50, () => R.Cast(target, packets()));
                    Utility.DelayAction.Add(300, () => menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "Normal", "Line Combo", "Coax", "Ult no W", "Normal With Ult" })));
                }
            }
        }

        private void LineCombo(bool useQ, bool useE)
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (target == null)
                return;

            float range = R.Range;
            if (GetTargetFocus(range) != null)
                target = GetTargetFocus(range);

            if (GetMarked() != null)
                target = GetMarked();

            if (HasEnergy(Q.IsReady() && useQ, W.IsReady(), E.IsReady() && useE))
            {
                var pred = Prediction.GetPrediction(target, 250f);

                if (Environment.TickCount - R.LastCastAttemptT > Game.Ping && RSpell.ToggleState == 0 && W.IsReady())
                {
                    R.Cast(target, packets());
                    R.LastCastAttemptT = Environment.TickCount + 300;
                    return;
                }

                if (HasBuff(target, "zedulttargetmark"))
                {

                    if (WSpell.ToggleState == 0 && W.IsReady() && Environment.TickCount - R.LastCastAttemptT > 0 && Environment.TickCount - W.LastCastAttemptT > Game.Ping)
                    {
                        var dist = menu.Item("R_Place_line", true).GetValue<Slider>().Value;
                        var behindVector = Player.ServerPosition - Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * dist;
                        //Game.PrintChat("dist: " + dist);

                        if ((useE && pred.Hitchance >= HitChance.Medium) ||
                            Q.GetPrediction(target).Hitchance >= HitChance.Medium)
                        {
                            W.Cast(behindVector);
                            W.LastCastAttemptT = Environment.TickCount + 300;

                            _predWq = useQ ? Q.GetPrediction(target).UnitPosition : Vector3.Zero;

                            _willEHit = useE;

                            Utility.DelayAction.Add(400, () => menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "Normal", "Line Combo", "Coax", "Ult no W", "Normal With Ult" })));
                        }
                    }
                }
            }
        }

        private void CheckShouldSwap()
        {
            var wHp = menu.Item("useW_Health", true).GetValue<Slider>().Value;
            var rHp = menu.Item("useR_Health", true).GetValue<Slider>().Value;

            if (RShadow != null)
            {
                if (GetHealthPercent() < rHp && RSpell.ToggleState == 2 && countEnemiesNearPosition(RShadow.ServerPosition, 400) < 1)
                {
                    R.Cast(packets());
                    return;
                }
            }

            if (WShadow != null)
            {
                if (GetHealthPercent() < wHp && WSpell.ToggleState == 2 && countEnemiesNearPosition(WShadow.ServerPosition, 400) < 1)
                    W.Cast(packets());
            }
        }
        private void Harass(bool useQ, bool useW, bool useE)
        {
            //energy check
            if (HasEnergy(Q.IsReady() && useQ, W.IsReady() && useW, E.IsReady() && useE))
            {
                if (useW)
                {
                    Cast_W("Harass", useQ, useE);

                    if (useQ && (!W.IsReady() || WSpell.ToggleState == 2))
                    {
                        //Game.PrintChat("RAWR");
                        Cast_Q();
                    }
                }
                else
                {
                    if(useQ)
                        Cast_Q();
                }

                if (useE)
                    Cast_E();
            }
        }

        private Obj_AI_Hero GetMarked()
        {
            return ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(x => x.IsValidTarget(W.Range + Q.Range) && HasBuff(x, "zedulttargetmark") && x.IsVisible);
        }

        private void SmartKs()
        {
            if (!menu.Item("smartKS", true).GetValue<bool>())
                return;
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(W.Range + Q.Range) && !x.IsDead && !x.HasBuffOfType(BuffType.Invulnerability)))
            {
                //WQE
                if ((Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 20 && W.IsReady() && Q.IsReady() && E.IsReady())
                {
                    if (menu.Item("Use_W_KS", true).GetValue<bool>())
                        Cast_W("Combo", true, true);
                    else
                    {
                        Cast_Q(target);
                        Cast_E(target);
                    }
                }

                //WQ
                if (Q.IsKillable(target) && Player.Distance(target.Position) > Q.Range && Q.IsReady() && W.IsReady()){
                    if (menu.Item("Use_W_KS", true).GetValue<bool>())
                        Cast_W("Combo", true, true);
                    else
                    {
                        Cast_Q(target);
                    }
                }
                //WE
                if (E.IsKillable(target) && Player.Distance(target.Position) > E.Range && E.IsReady() && W.IsReady())
                {
                    if (menu.Item("Use_W_KS", true).GetValue<bool>())
                        Cast_W("Combo", true, true);
                    else
                    {
                        Cast_E(target);
                    }
                }
                //Q
                if (Q.IsKillable(target) && Player.Distance(target.Position) < Q.Range && Q.IsReady())
                {
                    Cast_Q(target);
                }
                //E
                if (E.IsKillable(target) && Player.Distance(target.Position) < E.Range && E.IsReady())
                {
                    Cast_E(target);
                }
            }
        }

        private void Cast_Q(Obj_AI_Hero forceTarget = null)
        {
            var target = TargetSelector.GetTarget(Q.Range + W.Range, TargetSelector.DamageType.Physical);
            var qTarget = TargetSelector.GetTarget(Q.Range - 50, TargetSelector.DamageType.Physical);

            float range = W.Range + Q.Range;
            if (GetTargetFocus(range) != null)
                target = GetTargetFocus(range);

            if (GetMarked() != null)
            {
                target = GetMarked();
                qTarget = GetMarked();
            }
            if (forceTarget != null)
            {
                target = forceTarget;
                qTarget = forceTarget;
            }

            if (target == null || !Q.IsReady())
                return;

            if (qTarget != null)
            {
                Q.UpdateSourcePosition(Player.ServerPosition, Player.ServerPosition);
                Q.Cast(qTarget, packets());
                Q.LastCastAttemptT = Environment.TickCount + 300;
                return;
            }

            if (WShadow != null &&  _currentWShadow != Vector3.Zero)
            {
                Q.UpdateSourcePosition(WShadow.ServerPosition, WShadow.ServerPosition);
                Q.Cast(target, packets());
                return;
            }
            if (RShadow != null && _currentRShadow != Vector3.Zero)
            {
                Q.UpdateSourcePosition(RShadow.ServerPosition, RShadow.ServerPosition);
                Q.Cast(target, packets());
            }
        }

        private void Cast_E(Obj_AI_Hero forceTarget = null)
        {
            var target = TargetSelector.GetTarget(E.Range + W.Range, TargetSelector.DamageType.Physical);

            float range = E.Range + W.Range;
            if (GetTargetFocus(range) != null)
                target = GetTargetFocus(range);

            if (GetMarked() != null)
            {
                target = GetMarked();
            }
            if (forceTarget != null)
            {
                target = forceTarget;
            }
            if (target == null || !E.IsReady())
                return;

            if (WShadow != null && _currentWShadow != Vector3.Zero)
            {
                E.UpdateSourcePosition(WShadow.ServerPosition, WShadow.ServerPosition);
                E.Cast(target, packets());
            }
            if (RShadow != null && _currentRShadow != Vector3.Zero)
            {
                E.UpdateSourcePosition(RShadow.ServerPosition, RShadow.ServerPosition);
                E.Cast(target, packets());
            }

            E.UpdateSourcePosition(Player.ServerPosition, Player.ServerPosition);
            E.Cast(target, packets());
            E.LastCastAttemptT = Environment.TickCount + 300;
        }

        private Vector3 _predWq;
        private bool _willEHit;

        private void Cast_W(string source, bool useQ, bool useE)
        {
            var target = TargetSelector.GetTarget(Q.Range + W.Range - 100, TargetSelector.DamageType.Physical);

            float range = Q.Range + W.Range - 100;
            if (GetTargetFocus(range) != null)
                target = GetTargetFocus(range);

            if (target == null)
                return;

            if (GetMarked() != null)
                target = GetMarked();

            if (E.Level < 1)
                useE = false;
            if (Q.Level < 1)
                useQ = false;

            if (WSpell.ToggleState == 0 && W.IsReady() && Environment.TickCount - W.LastCastAttemptT > 0)
            {
                if (Player.Distance(target.Position) < W.Range + target.BoundingRadius)
                {
                    if ((!useQ || Q.IsReady()) && (!useE || E.IsReady()) && Player.Distance(target.Position) < W.Range)
                    {
                        if (IsPassWall(Player.ServerPosition, target.Position))
                            return;

                        W.Cast(target);
                        W.LastCastAttemptT = Environment.TickCount + 500;

                        _predWq = useQ ? target.Position : Vector3.Zero;
                        _willEHit = useE;
                    }
                }
                else
                {
                    var predE = Prediction.GetPrediction(target, .1f);
                    var vec = Player.ServerPosition + Vector3.Normalize(predE.CastPosition - Player.ServerPosition) * W.Range;

                    if (IsPassWall(Player.ServerPosition, vec))
                        return;

                    if ((!useQ || Q.IsReady()) && (!useE || E.IsReady()) && Player.Distance(vec) < W.Range)
                    {
                        if (useQ && useE)
                        {
                            if ((menu.Item("W_Require_QE", true).GetValue<bool>() && source == "Harass") || source == "Coax")
                            {
                                if (vec.Distance(target.ServerPosition) < E.Range)
                                {
                                    W.Cast(vec);
                                    W.LastCastAttemptT = Environment.TickCount + 500;
                                }
                            }
                            else
                            {
                                W.Cast(vec);
                                W.LastCastAttemptT = Environment.TickCount + 500;
                            }
                        }
                        else if (useE && vec.Distance(target.ServerPosition) < E.Range + target.BoundingRadius)
                        {
                            W.Cast(vec);
                            W.LastCastAttemptT = Environment.TickCount + 500;
                        }
                        else if (useQ)
                        {
                            W.Cast(vec);
                            W.LastCastAttemptT = Environment.TickCount + 500;
                        }

                        _predWq = useQ ? target.Position : Vector3.Zero;
                        _willEHit = useE && vec.Distance(target.ServerPosition) < E.Range;
                        
                    }
                }
            }
        }

        private void LastHitQ()
        {
            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            
            Q.UpdateSourcePosition(Player.ServerPosition, Player.ServerPosition);

            foreach(var minion in allMinionsQ){
                var predHealth = HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion.Position) * 1000 / Q.Speed));

                if (Player.GetSpellDamage(allMinionsQ[0], SpellSlot.Q) * .6 > predHealth + 5)
                    Q.Cast(minion);
            }
        }

        private void Farm()
        {
            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();
             
            if (useQ && Q.IsReady())
            {
                Q.UpdateSourcePosition(Player.ServerPosition, Player.ServerPosition);
                var pred = Q.GetLineFarmLocation(allMinionsQ);

                if (pred.MinionsHit > 2)
                    Q.Cast(pred.Position);
            }

            if (useE && E.IsReady())
            {
                E.UpdateSourcePosition(Player.ServerPosition, Player.ServerPosition);
                var pred = E.GetCircularFarmLocation(allMinionsE);

                if (pred.MinionsHit > menu.Item("LaneClear_useE_minHit", true).GetValue<Slider>().Value)
                    E.Cast(packets());
            }
        }

        private bool HasEnergy(bool q, bool w, bool e)
        {
            float energy = Player.Mana;
            float totalEnergy = 0;

            if (q)
                totalEnergy += Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            if (w)
                totalEnergy += Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;
            if (e)
                totalEnergy += Player.Spellbook.GetSpell(SpellSlot.E).ManaCost;

            if (energy >= totalEnergy)
                return true;

            return false;
        }

        private int _lasttick;
        private void ModeSwitch()
        {
            int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
            int lasttime = Environment.TickCount - _lasttick;

            if (menu.Item("Switch_1", true).GetValue<KeyBind>().Active && lasttime > Game.Ping)
            {
                menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "Normal", "Line Combo", "Coax", "Ult no W", "Normal With Ult" }, 1));
                _lasttick = Environment.TickCount + 300;
                return;
            }

            if (menu.Item("Switch_2", true).GetValue<KeyBind>().Active && lasttime > Game.Ping)
            {
                menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "Normal", "Line Combo", "Coax", "Ult no W", "Normal With Ult" }, 2));
                _lasttick = Environment.TickCount + 300;
                return;
            }

            if (menu.Item("Combo_Switch", true).GetValue<KeyBind>().Active && lasttime > Game.Ping)
            {
                if (mode == 0)
                {
                    menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "Normal", "Line Combo", "Coax", "Ult no W", "Normal With Ult" }, 1));
                    _lasttick = Environment.TickCount + 300;
                }
                else if (mode == 1)
                {
                    menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "Normal", "Line Combo", "Coax", "Ult no W", "Normal With Ult" }, 2));
                    _lasttick = Environment.TickCount + 300;
                }
                else if (mode == 2)
                {
                    menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "Normal", "Line Combo", "Coax", "Ult no W", "Normal With Ult" }, 3));
                    _lasttick = Environment.TickCount + 300;
                }
                else if (mode == 3)
                {
                    menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "Normal", "Line Combo", "Coax", "Ult no W", "Normal With Ult" }, 4));
                    _lasttick = Environment.TickCount + 300;
                }
                else
                {
                    menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "Normal", "Line Combo", "Coax", "Ult no W", "Normal With Ult" }));
                    _lasttick = Environment.TickCount + 300;
                }
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;

            ModeSwitch();
            SmartKs();
            CheckShouldSwap();

            if (menu.Item("Escape", true).GetValue<KeyBind>().Active && W.IsReady())
            {
                var vec = Player.ServerPosition + (Game.CursorPos - Player.ServerPosition)*W.Range;

                if (WSpell.ToggleState == 0)
                    W.Cast(vec);
                else if(WSpell.ToggleState == 2)
                    W.Cast(packets());
            }
            else if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("LastHitQ", true).GetValue<KeyBind>().Active)
                    LastHitQ();

                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();
            }

            if(E.IsReady() && menu.Item("AutoE", true).GetValue<bool>())
                Cast_E();
        }

        private float _qCooldown;

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (unit.IsEnemy && (unit is Obj_AI_Hero))
            {
                if (Player.Distance(args.End) > R.Range || !R.IsReady() || RSpell.ToggleState == 2)
                    return;

                if (menu.Item(args.SData.Name + "R_Dodge", true).GetValue<bool>() && args.SData.Name == "SyndraR")
                {
                    Utility.DelayAction.Add(150, () => R.Cast(unit, packets()));
                    return;
                }

                if (menu.Item(args.SData.Name + "R_Dodge", true).GetValue<bool>())
                    R.Cast(unit, packets());
            }

            if (!unit.IsMe)
                return;

            if (args.SData.Name == "ZedShuriken")
                _qCooldown = Game.Time + QSpell.Cooldown;

            if (args.SData.Name == "ZedShadowDash")
            {
                if (W.LastCastAttemptT - Environment.TickCount > 0)
                {
                    if (_predWq != Vector3.Zero)
                    {
                        Q.Cast(_predWq, packets());
                        Q.LastCastAttemptT = Environment.TickCount + 300;
                        _predWq = Vector3.Zero;
                    }

                    if (_willEHit)
                        E.Cast(packets());
                }
            }
            if (args.SData.Name == "zedw2")
            {
                _currentWShadow = Player.ServerPosition;
            }

            if (args.SData.Name == "zedult")
            {
                _currentRShadow = Player.ServerPosition;
            }

            if (args.SData.Name == "ZedR2")
            {
                _currentRShadow = Player.ServerPosition;
            }
        }

        private Obj_AI_Minion WShadow
        {
            get
            {
                if (_currentWShadow == Vector3.Zero)
                    return null;
                if(RShadow != null)
                    return ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(minion => minion.IsVisible && minion.IsAlly && minion.Name == "Shadow" && minion != RShadow && minion.ServerPosition != RShadow.ServerPosition);

                return ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(minion => minion.IsVisible && minion.IsAlly && minion.Name == "Shadow");
            }
        }

        private Obj_AI_Minion RShadow
        {
            get
            {
                if (_currentRShadow == Vector3.Zero)
                    return null;
                if(_currentRShadow == Vector3.Zero)
                    return ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(minion => minion.IsVisible && minion.IsAlly && minion.Name == "Shadow");

                return ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(minion => minion.IsVisible && minion.IsAlly && minion.Name == "Shadow" && minion.Distance(_currentRShadow) < 200);
            }
        }

        protected override void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_GeneralParticleEmitter))
                return;

            if (sender.Name == "Zed_Base_W_cloneswap_buf.troy")
            {
                //Game.PrintChat("W shadow created " + sender.Type);
                _currentWShadow = sender.Position;
            }

            if (sender.Name == "ZedUltMissile")
            {
                //CurrentRShadow = Player.ServerPosition;
            }

            if (sender.Name == "Zed_Base_R_buf_tell.troy")
            {
                if (RSpell.ToggleState == 2 && RShadow != null && menu.Item("R_Back", true).GetValue<bool>())
                    Utility.DelayAction.Add(500, () => R.Cast(packets()));
            }

        }

        protected override void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_GeneralParticleEmitter))
                return;
            
            if (sender.Name == "Zed_Clone_idle.troy" && _currentWShadow != Vector3.Zero && WShadow.Distance(sender.Position) < 100)
            {
                _currentWShadow = Vector3.Zero;
            }
            

            if (RShadow != null)
            {
                if (sender.Name == "Zed_Clone_idle.troy" && _currentRShadow != Vector3.Zero && RShadow.Distance(sender.Position) < 100)
                {
                    _currentRShadow = Vector3.Zero;
                    //Game.PrintChat("R Deleted");
                }
            }
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {

            if (menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            if (menu.Item("Draw_Q", true).GetValue<bool>())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_W", true).GetValue<bool>())
                if (W.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, W.Range - 2, W.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

            if (WShadow != null)
            {
                Render.Circle.DrawCircle(WShadow.Position, E.Range, Color.Aqua);
            }

            if (RShadow != null)
            {
                Render.Circle.DrawCircle(RShadow.Position, E.Range, Color.Yellow);
            }

            if (menu.Item("Current_Mode", true).GetValue<bool>())
            {
                Vector2 wts = Drawing.WorldToScreen(Player.Position);
                int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
                if (mode == 0)
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "Normal ");
                else if (mode == 1)
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "Line Combo");
                else if (mode == 2)
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "Coax");
                else if (mode == 3)
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "Ult no W");
                else if (mode == 4)
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "Normal With ult");
            }
        }
    }
}
