using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Syndra : Champion
    {
        public Syndra()
        {
            SetSpells();
            LoadMenu();
        }

        private Spell _qe;

        private void SetSpells()
        {
            Q = new Spell(SpellSlot.Q, 800);
            Q.SetSkillshot(.6f, 130f, 2000f, false, SkillshotType.SkillshotCircle);

            W = new Spell(SpellSlot.W, 950);
            W.SetSkillshot(.25f, 140f, 1600f, false, SkillshotType.SkillshotCircle);

            E = new Spell(SpellSlot.E, 700);
            E.SetSkillshot(.25f, (float)(45 * 0.5), 2500f, false, SkillshotType.SkillshotCircle);

            R = new Spell(SpellSlot.R, 750);

            _qe = new Spell(SpellSlot.Q, 1250);
            _qe.SetSkillshot(.900f, 70f, 2100f, false, SkillshotType.SkillshotLine);

        }

        private void LoadMenu()
        {
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("Misc_QE_Mouse", "QE to Nearest Target To Mouse", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("Misc_QE_Mouse2", "QE to Mouse", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("forceR", "Force R to best Target", true).SetValue(new KeyBind("R".ToCharArray()[0], KeyBindType.Press)));
                //key.AddItem(new MenuItem("qAA", "Auto Q AAing target", true).SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Toggle)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("SpellMenu", "SpellMenu");
            {
                var qMenu = new Menu("QMenu", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("Q_Auto_Immobile", "Auto Q on Immobile", true).SetValue(true));
                    spellMenu.AddSubMenu(qMenu);
                }

                var wMenu = new Menu("WMenu", "WMenu");
                {
                    wMenu.AddItem(new MenuItem("W_Only_Orb", "Only Pick Up Orb", true).SetValue(false));
                    spellMenu.AddSubMenu(wMenu);
                }
                var rMenu = new Menu("RMenu", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("R_Overkill_Check", "Overkill Check", true).SetValue(true));

                    rMenu.AddSubMenu(new Menu("Don't use R on", "Dont_R"));
                    foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team)
                    )
                        rMenu.SubMenu("Dont_R")
                            .AddItem(new MenuItem("Dont_R" + enemy.BaseSkinName, enemy.BaseSkinName, true).SetValue(false));

                    spellMenu.AddSubMenu(rMenu);
                }

                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "Use Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseQECombo", "Use QE", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "Use W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R", true).SetValue(true));
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseQEHarass", "Use QE", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(true));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                AddManaManagertoMenu(harass, "Harass", 30);
                //add to menu
                menu.AddSubMenu(harass);
            }

            var farm = new Menu("LaneClear", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseWFarm", "Use W", true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "Use E", true).SetValue(true));
                AddManaManagertoMenu(farm, "LaneClear", 30);
                //add to menu
                menu.AddSubMenu(farm);
            }

            var miscMenu = new Menu("Misc", "Misc");
            {
                miscMenu.AddItem(new MenuItem("QE_Interrupt", "Use QE to Interrupt", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("E_Gap_Closer", "Use E On Gap Closer", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("smartKS", "Use Smart KS System", true).SetValue(true));
                menu.AddSubMenu(miscMenu);
            }

            var drawMenu = new Menu("Drawing", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "Disable All", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "Draw Q", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_QE", "Draw QE", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_W", "Draw W", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "Draw E", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "Draw R", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_QE_Line", "Draw QE Line", true).SetValue(true));
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

                menu.AddSubMenu(drawMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q);

            if (W.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.W);

            if (E.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.E);

            if (R.IsReady())
                comboDamage += (3 + getOrbCount()) * Player.GetSpellDamage(target, SpellSlot.R, 1) - 20;

            comboDamage = ActiveItems.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target));
        }

        private float Get_Ult_Dmg(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (R.IsReady())
                damage += (3 + getOrbCount()) * Player.GetSpellDamage(enemy, SpellSlot.R, 1) - 20;

            return (float) damage;
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>(), menu.Item("UseQECombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>(), false, menu.Item("UseQEHarass", true).GetValue<bool>(), "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, bool useQe, string source)
        {
            if (source == "Harass" && !HasMana("Harass"))
                return;

            var qTarget = TargetSelector.GetTarget(650, TargetSelector.DamageType.Magical);
            float dmg = 0;
            if (qTarget != null)
                dmg += GetComboDamage(qTarget);

            if (useR)
                Cast_R();

            if(useQ)
                Cast_Q();

            if (useE)
                Cast_E();

            if (useW)
                Cast_W(true);

            //items
            if (source == "Combo")
            {
                var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                if (itemTarget != null)
                {
                    ActiveItems.Target = itemTarget;

                    //see if killable
                    if (dmg > itemTarget.Health - 50)
                        ActiveItems.KillableTarget = true;

                    ActiveItems.UseTargetted = true;
                }
            }

            if (useQe)
                Cast_QE();

            
        }

        private void Farm()
        {
            if (!HasMana("LaneClear"))
                return;

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ)
                CastBasicFarm(Q);

            if(useW)
                Cast_W(false);

            if(useE)
                CastBasicFarm(E);
        }

        private void SmartKs()
        {
            if (!menu.Item("smartKS", true).GetValue<bool>())
                return;

             foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(_qe.Range) && x.IsEnemy && !x.IsDead).OrderByDescending(GetComboDamage))
            {
                //Q
                if (Q.IsKillable(target) && Player.Distance(target.Position) < Q.Range)
                {
                    Q.Cast(target);
                }
                //E
                if (E.IsKillable(target) && Player.Distance(target.Position) < E.Range)
                {
                    E.Cast(target);
                }
                //QE
                if (E.IsKillable(target) && Player.Distance(target.Position) < _qe.Range)
                {
                    Cast_QE(false, target);
                }
            }
        }

        private void Cast_Q()
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (qTarget == null)
                return;

            if (Q.IsReady())
            {
                Q.Cast(qTarget, packets());
            }
        }

        private void Cast_W(bool mode)
        {
            if (mode)
            {
                var wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);

                var grabbableObj = Get_Nearest_orb();
                var wToggleState = Player.Spellbook.GetSpell(SpellSlot.W).ToggleState;

                if (wTarget == null)
                    return;

                if (grabbableObj != null && wToggleState == 1)
                {
                    if (Environment.TickCount - W.LastCastAttemptT > Game.Ping && W.IsReady())
                    {
                        if (grabbableObj.Distance(Player.Position) < W.Range)
                        {
                            W.Cast(grabbableObj.ServerPosition);
                            W.LastCastAttemptT = Environment.TickCount + 500;
                            return;
                        }
                    }
                }

                if (wToggleState != 1 && Get_Current_Orb() != null)
                {
                    //W.UpdateSourcePosition(Get_Current_Orb().ServerPosition, Get_Current_Orb().ServerPosition);
                    W.From = Get_Current_Orb().ServerPosition;

                    if (Player.Distance(wTarget.Position) < E.Range - 100)
                    {
                        if (wToggleState != 1 && W.IsReady() &&
                            Environment.TickCount - W.LastCastAttemptT > -300 + Game.Ping)
                        {
                            W.Cast(wTarget);
                            return;
                        }
                    }

                    if (W.IsReady())
                    {
                        W.Cast(wTarget);
                    }
                }
            }
            else
            {
                var grabbableObj = Get_Nearest_orb();
                var wToggleState = Player.Spellbook.GetSpell(SpellSlot.W).ToggleState;

                if (grabbableObj == null)
                    return;

                if (wToggleState == 1 && Environment.TickCount - W.LastCastAttemptT > Game.Ping && W.IsReady())
                {
                    W.Cast(grabbableObj.ServerPosition);
                    W.LastCastAttemptT = Environment.TickCount + 1000;
                    return;
                }

                if (Get_Current_Orb() == null)
                    return;

                W.From = Get_Current_Orb().ServerPosition;

                var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.NotAlly);
                var farmLocation = Q.GetCircularFarmLocation(allMinionsW, W.Width);

                if (farmLocation.MinionsHit > 0)
                    W.Cast(farmLocation.Position);
                else
                {
                    W.Cast(packets());
                }
            }
        }

        private void Cast_E()
        {
            if (getOrbCount() <= 0)
                return;

            var target = TargetSelector.GetTarget(_qe.Range + 100, TargetSelector.DamageType.Magical);
            if (target == null || Environment.TickCount - W.LastCastAttemptT < Game.Ping)
                return;

            foreach (var orb in getOrb().Where(x => Player.Distance(x.Position) < E.Range && x != null))
            {
                double rangeLeft = 100 + (-0.6 * Player.Distance(orb.ServerPosition) + 950);
                var startPos = orb.ServerPosition - Vector3.Normalize(orb.ServerPosition - Player.ServerPosition) * 100;
                var endPos = startPos + Vector3.Normalize(startPos - Player.ServerPosition) * (float)rangeLeft;

                _qe.Delay = E.Delay + Player.Distance(orb.Position) / E.Speed + target.Distance(orb.Position) / _qe.Speed;
                _qe.From = startPos;

                var targetPos = _qe.GetPrediction(target);

                var projection = targetPos.UnitPosition.To2D().ProjectOn(startPos.To2D(), endPos.To2D());

                if (!projection.IsOnSegment || targetPos.Hitchance < HitChance.Medium || !(projection.LinePoint.Distance(targetPos.UnitPosition.To2D()) < _qe.Width))
                    return;

                E.Cast(startPos, packets());
                W.LastCastAttemptT = Environment.TickCount + 500;
                return;
            }
        }

        private void Cast_R()
        {
            var rTarget = TargetSelector.GetTarget(R.Level > 2 ? R.Range : 675, TargetSelector.DamageType.Magical);

            if (rTarget == null)
                return;
            if (rTarget.HasBuffOfType(BuffType.Invulnerability))
                return;

            if (menu.Item("Dont_R" + rTarget.ChampionName, true) == null)
                return;
            if (menu.Item("Dont_R" + rTarget.ChampionName, true).GetValue<bool>())
                return;
            if (menu.Item("R_Overkill_Check", true).GetValue<bool>())
            {
                if (Player.GetSpellDamage(rTarget, SpellSlot.Q) - 25 > rTarget.Health)
                {
                    return;
                }
            }

            if (Get_Ult_Dmg(rTarget) > rTarget.Health - 20 && rTarget.Distance(Player.Position) < R.Range)
            {
                R.CastOnUnit(rTarget);
            }
        }

        private void Cast_QE(bool usePred = true , Obj_AI_Base target = null)
        {
            var qeTarget = TargetSelector.GetTarget(_qe.Range, TargetSelector.DamageType.Magical);
            if (qeTarget == null || !Q.IsReady() || !E.IsReady())
                return;

            var from = Prediction.GetPrediction(qeTarget, Q.Delay + E.Delay).UnitPosition;
            var startPos = Player.ServerPosition + Vector3.Normalize(from - Player.ServerPosition) * (E.Range - 100);
            double rangeLeft = 100 + (-0.6 * Player.Distance(startPos) + 950);
            var endPos = startPos + Vector3.Normalize(startPos - Player.ServerPosition) * (float)rangeLeft;

            _qe.From = startPos;
            _qe.Delay = Q.Delay + E.Delay + Player.Distance(from) / E.Speed;

            var qePred = _qe.GetPrediction(qeTarget);
            var projection = qePred.UnitPosition.To2D().ProjectOn(startPos.To2D(), endPos.To2D());

            if (!projection.IsOnSegment || !(projection.LinePoint.Distance(qePred.UnitPosition.To2D()) < _qe.Width))
                return;

            if (qePred.Hitchance >= HitChance.Medium || !usePred)
            {
                Q.Cast(startPos, packets());
                W.LastCastAttemptT = Environment.TickCount + 500;
                _qe.LastCastAttemptT = Environment.TickCount;
            }
        }

        private void CastQeMouse()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(_qe.Range)))
                if (Game.CursorPos.Distance(enemy.ServerPosition) < 300)
                    Cast_QE(false, enemy);
        }

        private void QImmobile()
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (!menu.Item("Q_Auto_Immobile", true).GetValue<bool>() || qTarget == null)
                return;
            if (Q.GetPrediction(qTarget).Hitchance == HitChance.Immobile)
                Q.Cast(qTarget);
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {

            if(R.IsReady())
                R.Range = R.Level == 3 ? 750f : 675f;
            if(E.IsReady())
                E.Width = E.Level == 5 ? 45f : (float)(45 * 0.5);

            if (menu.Item("Misc_QE_Mouse", true).GetValue<KeyBind>().Active)
            {
                CastQeMouse();
            }
            if (menu.Item("Misc_QE_Mouse2", true).GetValue<KeyBind>().Active)
            {
                var startPos = Player.ServerPosition + Vector3.Normalize(Game.CursorPos - Player.ServerPosition) * (E.Range - 100);
                Q.Cast(startPos, packets());
                W.LastCastAttemptT = Environment.TickCount + 500;
                _qe.LastCastAttemptT = Environment.TickCount;
            }

            SmartKs();

            if (menu.Item("forceR", true).GetValue<KeyBind>().Active)
            {
                var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

                if(target != null)
                    R.CastOnUnit(target, packets());
            }
            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
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

            QImmobile();
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            xSLxOrbwalker.EnableDrawing();
            if (menu.Item("Draw_Q", true).GetValue<bool>())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);
            if (menu.Item("Draw_QE", true).GetValue<bool>())
                if (Q.Level > 0 && E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, _qe.Range, Q.IsReady() && E.IsReady() ? Color.Green : Color.Red);
            if (menu.Item("Draw_W", true).GetValue<bool>())
                if (W.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

            if (Get_Current_Orb() != null)
                Render.Circle.DrawCircle(Get_Current_Orb().Position, W.Width, Color.Green);

            //draw EQ
            if (menu.Item("Draw_QE_Line", true).GetValue<bool>())
            {
                var qeTarget = TargetSelector.GetTarget(_qe.Range, TargetSelector.DamageType.Magical);
                if (qeTarget == null || !Q.IsReady() || !E.IsReady())
                    return;

                var from = Prediction.GetPrediction(qeTarget, Q.Delay + E.Delay).UnitPosition;
                var startPos = Player.ServerPosition + Vector3.Normalize(from - Player.ServerPosition) * (E.Range - 100);
                double rangeLeft = 100 + (-0.6 * Player.Distance(startPos) + 950);
                var endPos = startPos + Vector3.Normalize(startPos - Player.ServerPosition) * (float)rangeLeft;

                _qe.From = startPos;
                _qe.Delay = Q.Delay + E.Delay + Player.Distance(from) / E.Speed;

                var qePred = _qe.GetPrediction(qeTarget);
                var projection = qePred.UnitPosition.To2D().ProjectOn(startPos.To2D(), endPos.To2D());

                if (!projection.IsOnSegment || !(projection.LinePoint.Distance(qePred.UnitPosition.To2D()) < _qe.Width))
                    return;

                if (qePred.Hitchance >= HitChance.Medium)
                {
                    Vector2 wtsPlayer = Drawing.WorldToScreen(Player.Position);
                    Vector2 wtsPred = Drawing.WorldToScreen(endPos);
                    Render.Circle.DrawCircle(startPos, Q.Width/2, Color.Aquamarine);
                    Render.Circle.DrawCircle(endPos, Q.Width/2, Color.SpringGreen);
                    Drawing.DrawLine(wtsPlayer, wtsPred, 1, Color.LawnGreen);
                }

            }
            //end draw EQ--------------------------
            /*
            if (getOrbCount() <= 0)
                return;
            var target = TargetSelector.GetTarget(_qe.Range + 100, TargetSelector.DamageType.Magical);
            if (target == null)
                return;

            foreach (var orb in getOrb().Where(x => Player.Distance(x.Position) < E.Range && x != null))
            {
                double rangeLeft = 100 + (-0.6 * Player.Distance(orb.ServerPosition) + 950);
                var startPos = orb.ServerPosition - Vector3.Normalize(orb.ServerPosition - Player.ServerPosition) * 100;
                var endPos = startPos + Vector3.Normalize(startPos - Player.ServerPosition) * (float)rangeLeft;

                _qe.Delay = E.Delay + Player.Distance(orb)/E.Speed + target.Distance(orb)/_qe.Speed;
                _qe.From = startPos;

                var targetPos = _qe.GetPrediction(target);

                var projection = targetPos.UnitPosition.To2D().ProjectOn(startPos.To2D(), endPos.To2D());

                if (!projection.IsOnSegment || targetPos.Hitchance < HitChance.Medium || !(projection.LinePoint.Distance(targetPos.UnitPosition.To2D()) < _qe.Width + target.BoundingRadius))
                    return;
               
                Vector2 wtsPlayer = Drawing.WorldToScreen(Player.Position);
                Vector2 wtsPred = Drawing.WorldToScreen(endPos);
                Render.Circle.DrawCircle(startPos, Q.Width / 2, Color.Aquamarine);
                Render.Circle.DrawCircle(endPos, Q.Width / 2, Color.SpringGreen);
                Drawing.DrawLine(wtsPlayer, wtsPred, 1, Color.LawnGreen);
            }*/

            if (menu.Item("Draw_R_Killable", true).GetValue<bool>() && R.IsReady())
            {
                foreach (var wts in from unit in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(2000) && !x.IsDead && x.IsEnemy).OrderByDescending(GetComboDamage)
                        let health = unit.Health + unit.HPRegenRate + 10
                        where Get_Ult_Dmg(unit) > health
                        select Drawing.WorldToScreen(unit.Position))
                {
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "KILL!!!");
                }
            }
        }

        private int getOrbCount()
        {
            return
                ObjectManager.Get<Obj_AI_Minion>().Count(obj => obj.IsValid && obj.Team == ObjectManager.Player.Team && obj.Name == "Seed");
        }

        private IEnumerable<Obj_AI_Minion> getOrb()
        {
            return ObjectManager.Get<Obj_AI_Minion>().Where(obj => obj.IsValid && obj.Team == ObjectManager.Player.Team && obj.Name == "Seed").ToList();
        }

        private Obj_AI_Minion Get_Nearest_orb()
        {
            if (!menu.Item("W_Only_Orb", true).GetValue<bool>())
            {
                var minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.IsValidTarget(W.Range));

                if (minion != null)
                    return minion;
            }

            var orb =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(obj => obj.IsValid && obj.Team == ObjectManager.Player.Team && obj.Name == "Seed")
                    .ToList()
                    .OrderBy(x => Player.Distance(x.Position))
                    .FirstOrDefault();
            if (orb != null)
                return orb;

            return null;
        }

        private Obj_AI_Base Get_Current_Orb()
        {
            var orb = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.Team == Player.Team && x.Name == "Seed" && !x.IsTargetable);

            if (orb != null)
                return orb;

            var minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.IsInvulnerable && x.Name != "Seed" && x.Name != "k");

            if(minion != null)
                return minion;

            return null;
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("E_Gap_Closer", true).GetValue<bool>())
                return;

            if (!E.IsReady() || !gapcloser.Sender.IsValidTarget(E.Range))
                return;
            E.Cast(gapcloser.Sender, packets());
            W.LastCastAttemptT = Environment.TickCount + 500;
        }


        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (!unit.IsMe || !E.IsReady() || (spell.SData.Name != "SyndraQ") ||
                Environment.TickCount - _qe.LastCastAttemptT >= 300)
                return;
            E.Cast(spell.End, packets());
            W.LastCastAttemptT = Environment.TickCount + 500;
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (spell.DangerLevel < Interrupter2.DangerLevel.Medium || unit.IsAlly)
                return;

            if (menu.Item("QE_Interrupt", true).GetValue<bool>() && unit.IsValidTarget(_qe.Range))
                Cast_QE(false, unit);
        }

        /*public override void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            GamePacket g = new GamePacket(args.PacketData);
            if (g.Header != 0xFE)
                return;

            if (menu.Item("qAA", true).GetValue<KeyBind>().Active)
            {
                if (Packet.MultiPacket.OnAttack.Decoded(args.PacketData).Type == Packet.AttackTypePacket.TargetedAA)
                {
                    g.Position = 1;
                    var k = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(g.ReadInteger());
                    if (k is Obj_AI_Hero && k.IsEnemy)
                    {
                        if (Vector3.Distance(k.Position, Player.Position) <= Q.Range)
                        {
                            Q.Cast(k.Position, packets());
                        }
                    }
                }
            }
        }*/
    }
}
