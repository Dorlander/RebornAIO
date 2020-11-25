using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;
using Color = System.Drawing.Color;
using Geometry = LeagueSharp.Common.Geometry;

namespace xSaliceResurrected.Mid
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
            SpellManager.Q = new Spell(SpellSlot.Q, 800);
            SpellManager.Q.SetSkillshot(.5f, 130f, 2000f, false, SkillshotType.SkillshotCircle);

            SpellManager.W = new Spell(SpellSlot.W, 950);
            SpellManager.W.SetSkillshot(.25f, 140f, 1600f, false, SkillshotType.SkillshotCircle);

            SpellManager.E = new Spell(SpellSlot.E, 700);
            SpellManager.E.SetSkillshot(.25f, (float)(45 * 0.5), 2500f, false, SkillshotType.SkillshotCircle);

            SpellManager.R = new Spell(SpellSlot.R, 750);

            _qe = new Spell(SpellSlot.Q, 1250);
            _qe.SetSkillshot(.900f, 70f, 2100f, false, SkillshotType.SkillshotCircle);

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
                            .AddItem(new MenuItem("Dont_R" + enemy.CharData.BaseSkinName, enemy.CharData.BaseSkinName, true).SetValue(false));

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
                combo.AddSubMenu(HitChanceManager.AddHitChanceMenuCombo(true, true, true, false, true));
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseQEHarass", "Use QE", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(true));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                harass.AddSubMenu(HitChanceManager.AddHitChanceMenuHarass(true, true, true, false, true));
                ManaManager.AddManaManagertoMenu(harass, "Harass", 30);
                //add to menu
                menu.AddSubMenu(harass);
            }

            var farm = new Menu("LaneClear", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseWFarm", "Use W", true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "Use E", true).SetValue(true));
                ManaManager.AddManaManagertoMenu(farm, "LaneClear", 30);
                //add to menu
                menu.AddSubMenu(farm);
            }

            var miscMenu = new Menu("Misc", "Misc");
            {
                miscMenu.AddSubMenu(AoeSpellManager.AddHitChanceMenuCombo(true, false, true, false));
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


            var customMenu = new Menu("Custom Perma Show", "Custom Perma Show");
            {
                var myCust = new CustomPermaMenu();
                customMenu.AddItem(new MenuItem("custMenu", "Move Menu", true).SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Press)));
                customMenu.AddItem(new MenuItem("enableCustMenu", "Enabled", true).SetValue(true));
                customMenu.AddItem(myCust.AddToMenu("Combo Active: ", "ComboActive"));
                customMenu.AddItem(myCust.AddToMenu("Harass Active: ", "HarassActive"));
                customMenu.AddItem(myCust.AddToMenu("Laneclear Active: ", "LaneClearActive"));
                customMenu.AddItem(myCust.AddToMenu("QE Nearest Active: ", "Misc_QE_Mouse"));
                customMenu.AddItem(myCust.AddToMenu("QE Mouse Active: ", "Misc_QE_Mouse2"));
                customMenu.AddItem(myCust.AddToMenu("Force R Active: ", "forceR"));
                menu.AddSubMenu(customMenu);
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
                comboDamage += (3 + GetOrbCount()) * Player.GetSpellDamage(target, SpellSlot.R, 1) - 20;

            comboDamage = ItemManager.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target));
        }

        private float Get_Ult_Dmg(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (R.IsReady())
                damage += (3 + GetOrbCount()) * Player.GetSpellDamage(enemy, SpellSlot.R, 1) - 20;

            return (float)damage;
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
            if (source == "Harass" && !ManaManager.HasMana("Harass"))
                return;

            var qTarget = TargetSelector.GetTarget(650, TargetSelector.DamageType.Magical);
            float dmg = 0;
            if (qTarget != null)
                dmg += GetComboDamage(qTarget);

            if (useR)
                Cast_R();

            if (useQe)
                Cast_QE(source);

            if (useQ)
                SpellCastManager.CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Magical, HitChanceManager.GetQHitChance(source));

            if (useE)
                Cast_E(source);

            if (useW)
                Cast_W(true, source);

            //items
            if (source == "Combo")
            {
                var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                if (itemTarget != null)
                {
                    ItemManager.Target = itemTarget;

                    //see if killable
                    if (dmg > itemTarget.Health - 50)
                        ItemManager.KillableTarget = true;

                    ItemManager.UseTargetted = true;
                }
            }
        }

        private void Farm()
        {
            if (!ManaManager.HasMana("LaneClear"))
                return;

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ)
                SpellCastManager.CastBasicFarm(Q);

            if (useW)
                Cast_W(false, "Null");

            if (useE)
                SpellCastManager.CastBasicFarm(E);
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
                    Cast_QE("Null", target);
                }
            }
        }

        private void Cast_W(bool mode, string source)
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
                    if (Utils.TickCount - W.LastCastAttemptT > Game.Ping && W.IsReady())
                    {
                        if (grabbableObj.Distance(Player.Position) < W.Range)
                        {
                            W.Cast(grabbableObj.ServerPosition);
                            W.LastCastAttemptT = Utils.TickCount + 500;
                            return;
                        }
                    }
                }

                if (wToggleState != 1 && Get_Current_Orb() != null)
                {
                    W.From = Get_Current_Orb().ServerPosition;
                    var pred = W.GetPrediction(wTarget);

                    if (pred.Hitchance < HitChanceManager.GetWHitChance(source))
                        return;

                    if (Player.Distance(wTarget.Position) < E.Range - 100)
                    {
                        if (W.IsReady() && Utils.TickCount - W.LastCastAttemptT > -300 + Game.Ping)
                        {
                            var vector = pred.CastPosition.Shorten(Player.ServerPosition, 100);
                            W.Cast(vector);
                            Console.WriteLine("Shooting to vector");
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
                var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.NotAlly);

                if (grabbableObj == null || allMinionsW.Count < 1)
                    return;

                if (wToggleState == 1 && Utils.TickCount - W.LastCastAttemptT > Game.Ping && W.IsReady())
                {
                    W.Cast(grabbableObj.ServerPosition);
                    W.LastCastAttemptT = Utils.TickCount + 1000;
                    return;
                }

                if (Get_Current_Orb() == null)
                    return;

                W.From = Get_Current_Orb().ServerPosition;

                var farmLocation = Q.GetCircularFarmLocation(allMinionsW, W.Width);

                if (farmLocation.MinionsHit > 0)
                    W.Cast(farmLocation.Position);
                else
                {
                    W.Cast();
                }
            }
        }

        private void Cast_E(string source)
        {
            if (GetOrbCount() <= 0)
                return;

            var target = TargetSelector.GetTarget(_qe.Range + 100, TargetSelector.DamageType.Magical);
            if (target == null || Utils.TickCount - W.LastCastAttemptT < Game.Ping)
                return;

            foreach (var orb in _orbs.Where(x => Player.Distance(x.Position) < E.Range))
            {
                double rangeLeft = 100 + (-0.6 * Player.Distance(orb.ServerPosition) + 950);
                var startPos = orb.ServerPosition - Vector3.Normalize(orb.ServerPosition - Player.ServerPosition) * 100;
                var endPos = startPos + Vector3.Normalize(startPos - Player.ServerPosition) * (float)rangeLeft;

                _qe.Delay = E.Delay + Player.Distance(orb.Position) / E.Speed + target.Distance(orb.Position) / _qe.Speed;
                _qe.From = startPos;

                var targetPos = _qe.GetPrediction(target);

                var projection = targetPos.UnitPosition.To2D().ProjectOn(startPos.To2D(), endPos.To2D());

                if (!projection.IsOnSegment || targetPos.Hitchance < HitChance.Medium ||
                    !(projection.LinePoint.Distance(targetPos.UnitPosition.To2D()) < _qe.Width))
                    continue;

                if (targetPos.Hitchance >= HitChanceManager.GetEHitChance(source))
                {
                    E.Cast(startPos);
                    W.LastCastAttemptT = Utils.TickCount + 500;
                    return;
                }
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
                R.Cast(rTarget);
            }
        }

        private void Cast_QE(string source, Obj_AI_Base target = null)
        {
            var qeTarget = TargetSelector.GetTarget(_qe.Range, TargetSelector.DamageType.Magical);
            if (qeTarget == null || !Q.IsReady() || !E.IsReady())
                return;

            var qTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (qTarget.IsValidTarget(E.Range))
            {
                var pred = Q.GetPrediction(qTarget);

                if (pred.Hitchance >= HitChanceManager.GetQEHitChance(source))
                {
                    Q.Cast(pred.CastPosition);
                    W.LastCastAttemptT = Utils.TickCount + 500;
                    _qe.LastCastAttemptT = Utils.TickCount;
                }
            }
            else
            {
                var startPos = Player.ServerPosition.To2D().Extend(qeTarget.ServerPosition.To2D(), Q.Range).To3D();
                double rangeLeft = 100 + (-0.6*Player.Distance(startPos) + 950);
                var endPos = startPos + Vector3.Normalize(startPos - Player.ServerPosition)*(float) rangeLeft;

                _qe.From = startPos;
                _qe.Delay = E.Delay + Q.Range / E.Speed;

                var qePred = _qe.GetPrediction(qeTarget);

                var poly = new Geometry.Polygon.Rectangle(startPos, endPos, _qe.Width);

                if (!poly.IsInside(qePred.UnitPosition))
                    return;

                poly.Draw(Color.LawnGreen);

                if (qePred.Hitchance >= HitChanceManager.GetQEHitChance(source))
                {
                    Q.Cast(startPos);
                    W.LastCastAttemptT = Utils.TickCount + 500;
                    _qe.LastCastAttemptT = Utils.TickCount;
                }
            }
        }

        private void CastQeMouse()
        {
            Cast_QE("Null");
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

            if (R.IsReady())
                R.Range = R.Level == 3 ? 750f : 675f;
            if (E.IsReady())
                E.Width = E.Level == 5 ? 45f : (float)(45 * 0.5);

            if (menu.Item("Misc_QE_Mouse", true).GetValue<KeyBind>().Active)
            {
                CastQeMouse();
            }
            if (menu.Item("Misc_QE_Mouse2", true).GetValue<KeyBind>().Active)
            {
                var startPos = Player.ServerPosition + Vector3.Normalize(Game.CursorPos - Player.ServerPosition) * (E.Range - 100);
                Q.Cast(startPos);
                W.LastCastAttemptT = Utils.TickCount + 500;
                _qe.LastCastAttemptT = Utils.TickCount;
            }

            SmartKs();

            if (menu.Item("forceR", true).GetValue<KeyBind>().Active)
            {
                var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

                if (target != null)
                    R.Cast(target);
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

                var qTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                if (qTarget.IsValidTarget(E.Range))
                {
                    var pred = Q.GetPrediction(qTarget);

                    if (pred.Hitchance >= HitChanceManager.GetQEHitChance("Combo"))
                    {
                        var poly = new Geometry.Polygon.Rectangle(pred.CastPosition, Player.ServerPosition.Extend(pred.CastPosition, _qe.Range), _qe.Width);
                        poly.Draw(Color.LawnGreen);
                        var line = new Geometry.Polygon.Line(Player.Position, Player.ServerPosition.Extend(pred.CastPosition, _qe.Range));
                        line.Draw(Color.LawnGreen);
                        Render.Circle.DrawCircle(pred.CastPosition, Q.Width / 2, Color.Aquamarine);
                        Render.Circle.DrawCircle(Player.ServerPosition.Extend(pred.CastPosition, _qe.Range), Q.Width / 2, Color.SpringGreen);
                    }
                }
                else
                {
                    var startPos = Player.ServerPosition.To2D().Extend(qeTarget.ServerPosition.To2D(), Q.Range).To3D();
                    double rangeLeft = 100 + (-0.6 * Player.Distance(startPos) + 950);
                    var endPos = startPos + Vector3.Normalize(startPos - Player.ServerPosition) * (float)rangeLeft;

                    _qe.From = startPos;
                    _qe.Delay = E.Delay + Q.Range / E.Speed;

                    var qePred = _qe.GetPrediction(qeTarget);

                    var poly = new Geometry.Polygon.Rectangle(startPos, endPos, _qe.Width);

                    if (!poly.IsInside(qePred.UnitPosition))
                        return;

                    if (qePred.Hitchance >= HitChanceManager.GetQEHitChance("Combo"))
                    {
                        poly.Draw(Color.LawnGreen);
                        var line = new Geometry.Polygon.Line(Player.Position, endPos);
                        line.Draw(Color.LawnGreen);
                        Render.Circle.DrawCircle(startPos, Q.Width/2, Color.Aquamarine);
                        Render.Circle.DrawCircle(endPos, Q.Width/2, Color.SpringGreen);
                    }
                }
            }

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

        private readonly List<Obj_AI_Minion> _orbs = new List<Obj_AI_Minion>();

        private int GetOrbCount()
        {
            return _orbs.Count;
        }

        private Obj_AI_Minion Get_Nearest_orb()
        {
            if (!menu.Item("W_Only_Orb", true).GetValue<bool>())
            {
                var minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.IsValidTarget(W.Range));

                if (minion != null)
                    return minion;
            }

            return _orbs.FirstOrDefault();
        }

        private Obj_AI_Minion Get_Current_Orb()
        {
            var orb = _orbs.FirstOrDefault(x => x.Team == Player.Team && x.Name == "Seed" && !x.IsTargetable);

            if (orb != null)
                return orb;

            var minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.IsInvulnerable && x.Name != "Seed" && x.Name != "k");

            return minion;
        }

        protected override void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Minion))
                return;

            
            if (sender.Name == "Seed" && sender.IsAlly)
            {
                Console.WriteLine(_orbs.Count);

                Obj_AI_Minion orb = (Obj_AI_Minion)sender;
                _orbs.Add(orb);

                Console.WriteLine("Added orb");
            }
        }

        protected override void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Minion))
                return;

            if (_orbs.RemoveAll(s => s.NetworkId == sender.NetworkId) > 0)
            {
                Console.WriteLine("Removed Orb");
            }
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("E_Gap_Closer", true).GetValue<bool>())
                return;

            if (!E.IsReady() || !gapcloser.Sender.IsValidTarget(E.Range))
                return;
            E.Cast(gapcloser.Sender);
            W.LastCastAttemptT = Utils.TickCount + 500;
        }


        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (!unit.IsMe || !E.IsReady() || (spell.SData.Name != "SyndraQ") ||
                Utils.TickCount - _qe.LastCastAttemptT >= 300)
                return;
            E.Cast(spell.End);
            W.LastCastAttemptT = Utils.TickCount + 500;
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (spell.DangerLevel < Interrupter2.DangerLevel.Medium || unit.IsAlly)
                return;

            if (menu.Item("QE_Interrupt", true).GetValue<bool>() && unit.IsValidTarget(_qe.Range))
                Cast_QE("Null", unit);
        }

    }
}
