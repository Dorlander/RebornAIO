using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Azir : Champion
    {
        public Azir()
        {
            LoadSpells();
            LoadMenu();
        }

        private static Obj_AI_Base _insecTarget;
        private Vector3 _rVec;

        private void LoadSpells()
        {
            //intalize spell
            Q = new Spell(SpellSlot.Q, 875);
            QExtend = new Spell(SpellSlot.Q, 1150);
            Q2 = new Spell(SpellSlot.Q, 2000);
            W = new Spell(SpellSlot.W, 350);
            E = new Spell(SpellSlot.E, 2000);
            R = new Spell(SpellSlot.R, 450);

            Q.SetSkillshot(0.1f, 100, 1700, false, SkillshotType.SkillshotLine);
            QExtend.SetSkillshot(0.1f, 100, 1700, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 100, 1200, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.5f, 700, 1400, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private void LoadMenu()
        {

            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!",true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!",true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!",true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!",true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("escape", "Escape", true).SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("insec", "Insec Selected target",true).SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("qeCombo", "Q->E stun Nearest target",true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("qMulti", "Q if 2+ Soilder",true).SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("point", "Escape Point Tag(Delay Escape Only)", true).SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            //Spell Menu
            var spell = new Menu("Spell", "Spell");
            {

                var qMenu = new Menu("QSpell", "QSpell"); { 
                    qMenu.AddItem(new MenuItem("qOutRange", "Only Use When target out of range",true).SetValue(false));
                    qMenu.AddItem(new MenuItem("qExtend", "Use Extended Q Range",true).SetValue(true));
                    qMenu.AddItem(new MenuItem("qHit", "Q HitChance",true).SetValue(new Slider(3, 1, 3)));
                    spell.AddSubMenu(qMenu);
                }
                //W Menu
                var wMenu = new Menu("WSpell", "WSpell"); {
                    wMenu.AddItem(new MenuItem("wAtk", "Always Atk Enemy",true).SetValue(true));
                    wMenu.AddItem(new MenuItem("wQ", "Use WQ Poke",true).SetValue(true));
                    spell.AddSubMenu(wMenu);
                }
                //E Menu
                var eMenu =  new Menu("ESpell", "ESpell");
                {
                    eMenu.AddItem(new MenuItem("eKill", "If Killable Combo",true).SetValue(false));
                    eMenu.AddItem(new MenuItem("eKnock", "Always Knockup/DMG",true).SetValue(false));
                    eMenu.AddItem(new MenuItem("eHP", "if HP >",true).SetValue(new Slider(100)));
                    spell.AddSubMenu(eMenu);
                }
                //R Menu
                var rMenu = new Menu("RSpell", "RSpell");{
                    rMenu.AddItem(new MenuItem("rHP", "if HP <",true).SetValue(new Slider(20)));
                    rMenu.AddItem(new MenuItem("rHit", "If Hit >= Target",true).SetValue(new Slider(3, 0, 5)));
                    rMenu.AddItem(new MenuItem("rWall", "R Enemy Into Wall",true).SetValue(true));
                    spell.AddSubMenu(rMenu);
                }
                menu.AddSubMenu(spell);
            }

            //Combo menu:
            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "Use Q",true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "Use W",true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E",true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R",true).SetValue(true));
                menu.AddSubMenu(combo);
            }

            //Harass menu:
            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q",true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W",true).SetValue(true));
                harass.AddItem(new MenuItem("UseEHarass", "Use E",true).SetValue(false));
                menu.AddSubMenu(harass);
            }

            //killsteal
            var killSteal = new Menu("KillSteal", "KillSteal");
            {
                killSteal.AddItem(new MenuItem("smartKS", "Use Smart KS System",true).SetValue(true));
                killSteal.AddItem(new MenuItem("eKS", "Use E KS",true).SetValue(false));
                killSteal.AddItem(new MenuItem("wqKS", "Use WQ KS",true).SetValue(true));
                killSteal.AddItem(new MenuItem("qeKS", "Use WQE KS",true).SetValue(false));
                killSteal.AddItem(new MenuItem("rKS", "Use R KS",true).SetValue(true));
                menu.AddSubMenu(killSteal);
            }

            //farm menu
            var farm = new Menu("Farm", "Farm");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q",true).SetValue(false));
                farm.AddItem(new MenuItem("qFarm", "Only Q if > minion",true).SetValue(new Slider(3, 0, 5)));
                menu.AddSubMenu(farm);
            }

            //Misc Menu:
            var misc = new Menu("Misc", "Misc");
            {
                misc.AddItem(new MenuItem("UseInt", "Use E to Interrupt",true).SetValue(true));
                misc.AddItem(new MenuItem("UseGap", "Use R for GapCloser",true).SetValue(true));
                misc.AddItem(new MenuItem("fastEscape", "Fast Escape = on, Delay EQ Escape = off",true).SetValue(true));
                misc.AddItem(new MenuItem("escapeDelay", "Escape Delay Decrease", true).SetValue(new Slider(0, 0, 300)));
                menu.AddSubMenu(misc);
            }

            //Drawings menu:
            var draw = new Menu("Drawings", "Drawings"); { 
                draw.AddItem(new MenuItem("QRange", "Q range",true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                draw.AddItem(new MenuItem("QExtendRange", "Q Extended range",true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                draw.AddItem(new MenuItem("WRange", "W range",true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                draw.AddItem(new MenuItem("ERange", "E range",true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                draw.AddItem(new MenuItem("RRange", "R range",true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                draw.AddItem(new MenuItem("slaveDmg", "Draw Slave AA Needed",true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "Draw Combo Damage",true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "Draw Combo Damage Fill",true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
                draw.AddItem(drawComboDamageMenu);
                draw.AddItem(drawFill);
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

                menu.AddSubMenu(draw);
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            if (enemy == null)
                return 0;

            var damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (soilderCount() > 0 || W.IsReady())
            {
                damage += 2*xSLxOrbwalker.GetAzirAaSandwarriorDamage(enemy);
            }

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);

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
            var qTarget = TargetSelector.GetTarget(QExtend.Range, TargetSelector.DamageType.Magical);
            var soilderTarget = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Magical);
            var dmg = GetComboDamage(soilderTarget);

            if (soilderTarget == null || qTarget == null)
                return;

            //R
            if (useR && R.IsReady() && ShouldR(qTarget) && Player.Distance(qTarget.Position) < R.Range)
                R.Cast(qTarget);

            //WQ
            if (soilderCount() == 0 && useQ && useW && W.IsReady() && (Q.IsReady() || QSpell.State == SpellState.Surpressed) && menu.Item("wQ", true).GetValue<bool>())
            {
                CastWq(qTarget);
            }

            //W
            if (useW && W.IsReady())
            {
                CastW(qTarget);
            }

            //Q
            if (useQ && Q.IsReady())
            {
                CastQ(qTarget);
                return;
            }

            //items
            if (source == "Combo")
            {
                ActiveItems.Target = soilderTarget;

                //see if killable
                if (dmg > soilderTarget.Health - 50)
                    ActiveItems.KillableTarget = true;

                ActiveItems.UseTargetted = true;
                
            }

            //E
            if (useE && (E.IsReady() || ESpell.State == SpellState.Surpressed))
            {
                CastE(soilderTarget);
                //Game.PrintChat("RAWR");
            }

            //Console.Clear();
            //AutoAtk
            //attackTarget(soilderTarget);
        }

        private bool WallStun(Obj_AI_Hero target)
        {
            var pushedPos = R.GetPrediction(target).UnitPosition;

            if (IsPassWall(Player.ServerPosition, pushedPos))
                return true;

            return false;
        }

        private void SmartKs()
        {
            if (!menu.Item("smartKS", true).GetValue<bool>())
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(1200) && !x.HasBuffOfType(BuffType.Invulnerability)).OrderByDescending(GetComboDamage))
            {
                if (target != null)
                {
                    //R
                    if ((Player.GetSpellDamage(target, SpellSlot.R)) > target.Health + 20 && Player.Distance(target.Position) < R.Range && menu.Item("rKS", true).GetValue<bool>())
                    {
                        R.Cast(target);
                    }

                    if (soilderCount() < 1 && !W.IsReady())
                        return;

                    //WQ
                    if ((Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20 && menu.Item("wqKS", true).GetValue<bool>())
                    {
                        CastWq(target);
                    }

                    //qe
                    if ((Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 20 && Player.Distance(target.Position) < Q.Range && menu.Item("qeKS", true).GetValue<bool>())
                    {
                        CastQe(target);
                    }

                }
            }
        }

        private void Escape()
        {
            Vector3 wVec = Player.ServerPosition + Vector3.Normalize(Game.CursorPos - Player.ServerPosition) * W.Range;

            if (menu.Item("fastEscape", true).GetValue<bool>())
            {
                if (W.IsReady() || soilderCount() > 0)
                {
                    if ((E.IsReady() || ESpell.State == SpellState.Surpressed))
                    {
                        W.Cast(wVec);
                        W.LastCastAttemptT = Environment.TickCount;
                    }

                    if ((QExtend.IsReady() || QSpell.State == SpellState.Surpressed) &&
                        ((Environment.TickCount - E.LastCastAttemptT < Game.Ping + 500 &&
                          Environment.TickCount - E.LastCastAttemptT > Game.Ping + 50) || E.IsReady()))
                    {
                        if (Environment.TickCount - W.LastCastAttemptT > Game.Ping + 300 || ESpell.State == SpellState.Cooldown || !W.IsReady())
                        {
                            Vector3 qVec = Player.ServerPosition +
                                           Vector3.Normalize(Game.CursorPos - Player.ServerPosition) * 800;

                            var lastAttempt = (int)qVec.Distance(GetNearestSoilderToMouse().Position) / 1000;

                            Q.Cast(qVec, packets());
                            Q.LastCastAttemptT = Environment.TickCount + lastAttempt;
                            return;
                        }
                    }

                    if ((E.IsReady() || ESpell.State == SpellState.Surpressed))
                    {
                        if (Player.Distance(Game.CursorPos) > GetNearestSoilderToMouse().Position.Distance(Game.CursorPos) && Environment.TickCount - Q.LastCastAttemptT > Game.Ping)
                        {
                            E.Cast(GetNearestSoilderToMouse().Position, packets());
                            E.LastCastAttemptT = Environment.TickCount - 250;
                            //Game.PrintChat("Rawr2");
                            return;
                        }
                        if (Environment.TickCount - W.LastCastAttemptT < Game.Ping + 300 && (Q.IsReady() || QSpell.State == SpellState.Surpressed))
                        {
                            E.Cast(wVec, packets());
                            E.LastCastAttemptT = Environment.TickCount - 250;
                            //Game.PrintChat("Rawr1");
                        }
                    }
                }
            }
            else
            {
                if (Environment.TickCount - E.LastCastAttemptT < 250 + Game.Ping / 2 && Environment.TickCount - E.LastCastAttemptT > _delay && ESpell.State == SpellState.Cooldown && _point != Vector3.Zero)
                {
                    Q2.Cast(_vecPoint, packets());
                }

                if ((E.IsReady() || ESpell.State == SpellState.Surpressed) && _point != Vector3.Zero)
                {
                    if(soilderCount() < 1 && W.IsReady())
                        W.Cast(wVec);
                    else if (soilderCount() < 1 && !W.IsReady())
                        return;

                    if (GetNearestSoilderToMouse() == null)
                        return;

                    var nearSlave = GetNearestSoilderToMouse();

                    if (E.IsReady() || ESpell.State == SpellState.Surpressed)
                        E.Cast(nearSlave.Position, packets());

                    if (QExtend.IsReady() || QSpell.State == SpellState.Surpressed)
                    {
                        _vecPoint = nearSlave.Position + Vector3.Normalize(_point - nearSlave.Position) * 700;
                        _delay = (int)(100*(Player.Distance(nearSlave.Position)/800)+ menu.Item("escapeDelay", true).GetValue<Slider>().Value);
                        
                        //Game.PrintChat("Delay" + _delay);
                    }
                }
            }
        }

        private int _delay;
        private Vector3 _vecPoint;

        private GameObject GetNearestSoilderToMouse()
        {
            var soilder = xSLxOrbwalker.Soilders.ToList().OrderBy(x => Game.CursorPos.Distance(x.Position));

            if (soilder.FirstOrDefault() != null)
                return soilder.FirstOrDefault();

            return null;
        }

        private void CastQe(Obj_AI_Hero target)
        {
            if (soilderCount() > 0)
            {
                if ((Q.IsReady() || QSpell.State == SpellState.Surpressed) && E.IsReady())
                {
                    var slaves = xSLxOrbwalker.Soilders.ToList();

                    foreach (var slave in slaves)
                    {
                        if (target != null && Player.Distance(target.Position) < 800)
                        {
                            var qPred = GetP(slave.Position, QExtend, target, true);

                            if (Q.IsReady() && Player.Distance(target.Position) < 800 && qPred.Hitchance >= getQHitchance())
                            {
                                var vec = target.ServerPosition - Player.ServerPosition;
                                var castBehind = qPred.CastPosition + Vector3.Normalize(vec) * 75;

                                Q.Cast(castBehind, packets());
                                E.Cast(slave.Position, packets());
                                return;

                            }
                        }
                    }
                }
            }
            else if (W.IsReady())
            {
                Vector3 wVec = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * W.Range;

                var qPred = GetP(wVec, QExtend, target, true);

                if ((Q.IsReady() || QSpell.State == SpellState.Surpressed) && (E.IsReady() || ESpell.State == SpellState.Surpressed) && Player.Distance(target.Position) < 800 && qPred.Hitchance >= getQHitchance())
                {
                    var vec = target.ServerPosition - Player.ServerPosition;
                    var castBehind = qPred.CastPosition + Vector3.Normalize(vec) * 75;

                    W.Cast(wVec);
                    QExtend.Cast(castBehind, packets());
                    Utility.DelayAction.Add(1, () => E.Cast(getNearestSoilderToEnemy(target).Position, packets()));
                }
            }
        }

        private void Insec()
        {
            var target = (Obj_AI_Hero)_insecTarget;

            if (target == null)
                return;

            if (soilderCount() > 0)
            {
                if ((Q.IsReady() || QSpell.State == SpellState.Surpressed) && E.IsReady())
                {
                    var slaves = xSLxOrbwalker.Soilders.ToList();

                    foreach (var slave in slaves)
                    {
                        if (Player.Distance(target.Position) < 800)
                        {
                            var qPred = GetP(slave.Position, QExtend, target, true);
                            var vec = target.ServerPosition - Player.ServerPosition;
                            var castBehind = qPred.CastPosition + Vector3.Normalize(vec) * 75;
                            _rVec = qPred.CastPosition - Vector3.Normalize(vec) * 300;

                            if (Q.IsReady() && (E.IsReady() || ESpell.State == SpellState.Surpressed) && R.IsReady() && qPred.Hitchance >= getQHitchance())
                            {

                                Q.Cast(castBehind, packets());
                                E.Cast(slave.Position, packets());
                                E.LastCastAttemptT = Environment.TickCount;
                            }
                        }
                    }
                }
                if (R.IsReady())
                {
                    if (Player.Distance(target.Position) < 200 && Environment.TickCount - E.LastCastAttemptT > Game.Ping + 150)
                    {
                        //Game.PrintChat("rawr");
                        R.Cast(_rVec);
                    }
                }
            }
            else if (W.IsReady())
            {
                Vector3 wVec = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * W.Range;

                var qPred = GetP(wVec, QExtend, target, true);

                if ((Q.IsReady() || QSpell.State == SpellState.Surpressed) && (E.IsReady() || ESpell.State == SpellState.Surpressed)
                    && R.IsReady() && Player.Distance(target.Position) < 800 && qPred.Hitchance >= getQHitchance())
                {
                    var vec = target.ServerPosition - Player.ServerPosition;
                    var castBehind = qPred.CastPosition + Vector3.Normalize(vec) * 75;
                    _rVec = Player.Position;

                    W.Cast(wVec);
                    QExtend.Cast(castBehind, packets());
                    E.Cast(getNearestSoilderToEnemy(target).Position, packets());
                }
                if (R.IsReady())
                {
                    if (Player.Distance(target.Position) < 200 && Environment.TickCount - E.LastCastAttemptT > Game.Ping + 150)
                    {
                        //Game.PrintChat("rawr2");
                        R.Cast(_rVec);
                    }
                }
            }
        }

        private void CastWq(Obj_AI_Hero target)
        {
            if (soilderCount() < 1 && menu.Item("qMulti", true).GetValue<KeyBind>().Active)
                return;

            if (Player.Distance(target.Position) < 1150 && Player.Distance(target.Position) > W.Range)
            {
                if (W.IsReady() && (Q.IsReady() || QSpell.State == SpellState.Surpressed))
                {
                    Vector3 wVec = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * W.Range;

                    var qPred = GetP(wVec, QExtend, target, true);

                    if (qPred.Hitchance >= getQHitchance())
                    {
                        W.Cast(wVec);
                        QExtend.Cast(qPred.CastPosition, packets());
                    }
                }
            }
        }

        private void CastW(Obj_AI_Hero target)
        {
            if (Player.Distance(target.Position) < 1200)
            {
                if (Player.Distance(target.Position) < 600)
                {
                    Vector3 wVec = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * W.Range;

                    //Game.PrintChat("W Cast2");
                    if (W.IsReady())
                    {
                        W.Cast(wVec);
                        if (canAttack())
                            Utility.DelayAction.Add(200, () => Player.IssueOrder(GameObjectOrder.AttackUnit, target));
                    }
                }
                else if (Player.Distance(target.Position) < 950)
                {
                    Vector3 wVec = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * W.Range;
                    if (Player.Level > 10)
                    {
                        if (W.IsReady())
                        {
                            W.Cast(wVec);
                            if (canAttack())
                                Utility.DelayAction.Add(200, () => Player.IssueOrder(GameObjectOrder.AttackUnit, target));
                        }
                    }

                    //Game.PrintChat("W Cast2");
                    if (W.IsReady() && (Q.IsReady() || QSpell.State == SpellState.Surpressed))
                    {
                        var qPred = GetP(wVec, QExtend, target, true);

                        if (qPred.Hitchance >= getQHitchance())
                        {
                            W.Cast(wVec);
                        }
                    }
                }
            }
        }

        private void CastQ(Obj_AI_Hero target)
        {
            if (soilderCount() < 1)
                return;

            var slaves = xSLxOrbwalker.Soilders.ToList();

            foreach (var slave in slaves)
            {
                if (target != null && Player.Distance(target.Position) < QExtend.Range && ShouldQ(target, slave))
                {

                    var qPred = GetP(slave.Position, QExtend, target, true);

                    if (Q.IsReady() && Player.Distance(target.Position) < 800 && qPred.Hitchance >= getQHitchance())
                    {
                        Q.Cast(qPred.CastPosition, packets());
                        return;
                    }
                    if (Q.IsReady() && Player.Distance(target.Position) > 800 && qPred.Hitchance >= getQHitchance() && menu.Item("qExtend", true).GetValue<bool>())
                    {
                        var qVector = Player.ServerPosition + Vector3.Normalize(qPred.CastPosition - Player.ServerPosition) * 800;

                        //Game.PrintChat("QHarass");
                        QExtend.Cast(qVector, packets());
                        return;
                    }
                }
            }
        }

        private void CastE(Obj_AI_Hero target)
        {
            if (soilderCount() < 1)
                return;

            var slaves = xSLxOrbwalker.Soilders.ToList();

            foreach (var slave in slaves)
            {
                if (target != null && Player.Distance(slave.Position) < E.Range)
                {
                    var ePred = E.GetPrediction(target);
                    Object[] obj = VectorPointProjectionOnLineSegment(Player.ServerPosition.To2D(), slave.Position.To2D(), ePred.UnitPosition.To2D());
                    var isOnseg = (bool)obj[2];
                    var pointLine = (Vector2)obj[1];

                    if (E.IsReady() && isOnseg && pointLine.Distance(ePred.UnitPosition.To2D()) < E.Width && ShouldE(target))
                    {
                        E.Cast(slave.Position, packets());
                        return;
                    }
                }
            }

            //Game.PrintChat("RAWR");
        }

        private bool ShouldQ(Obj_AI_Hero target, GameObject slave)
        {

            if (soilderCount() < 2 && menu.Item("qMulti", true).GetValue<KeyBind>().Active)
                return false;

            if (!menu.Item("qOutRange", true).GetValue<bool>())
                return true;

            if (target.Distance(slave.Position) > 390)
                return true;

            if (Player.GetSpellDamage(target, SpellSlot.Q) > target.Health + 10)
                return true;


            return false;
        }
        private bool ShouldE(Obj_AI_Hero target)
        {
            if (menu.Item("eKnock", true).GetValue<bool>())
                return true;

            if (menu.Item("eKill", true).GetValue<bool>() && GetComboDamage(target) > target.Health + 15)
                return true;

            if (menu.Item("eKS", true).GetValue<bool>() && Player.GetSpellDamage(target, SpellSlot.E) > target.Health + 10)
                return true;

            //hp 
            var hp = menu.Item("eHP", true).GetValue<Slider>().Value;
            var hpPercent = Player.Health / Player.MaxHealth * 100;

            if (hpPercent > hp)
                return true;

            return false;
        }

        private bool ShouldR(Obj_AI_Hero target)
        {
            if (Player.GetSpellDamage(target, SpellSlot.R) > target.Health - 150)
                return true;

            var hp = menu.Item("rHP", true).GetValue<Slider>().Value;
            var hpPercent = Player.Health / Player.MaxHealth * 100;
            if (hpPercent < hp)
                return true;

            var rHit = menu.Item("rHit", true).GetValue<Slider>().Value;
            var pred = R.GetPrediction(target);
            if (pred.AoeTargetsHitCount >= rHit)
                return true;

            if (WallStun(target) && GetComboDamage(target) > target.Health / 2 && menu.Item("rWall", true).GetValue<bool>())
            {
                //Game.PrintChat("Walled");
                return true;
            }

            return false;
        }

        private void AutoAtk()
        {
            if (soilderCount() < 1)
                return;

            var soilderTarget = TargetSelector.GetTarget(800, TargetSelector.DamageType.Magical);

            //Game.PrintChat("YEhhhhh");
            if (soilderTarget == null)
                return;

            AttackTarget(soilderTarget);
        }

        private HitChance getQHitchance()
        {
            var hitC = HitChance.High;
            var qHit = menu.Item("qHit", true).GetValue<Slider>().Value;

            // HitChance.Low = 3, Medium , High .... etc..
            switch (qHit)
            {
                case 1:
                    hitC = HitChance.Low;
                    break;
                case 2:
                    hitC = HitChance.Medium;
                    break;
                case 3:
                    hitC = HitChance.High;
                    break;
                case 4:
                    hitC = HitChance.VeryHigh;
                    break;
            }

            return hitC;
        }

        private int soilderCount()
        {
            return xSLxOrbwalker.Soilders.Count();
        }

        private bool canAttack()
        {
            return xSLxOrbwalker.CanAttack();
        }

        private void AttackTarget(Obj_AI_Hero target)
        {
            if (soilderCount() < 1)
                return;

            var tar = getNearestSoilderToEnemy(target);
            if (tar != null && Player.Distance(tar.Position) < 800)
            {
                if (target != null && target.Distance(tar.Position) <= 390 && canAttack())
                {
                    xSLxOrbwalker.Orbwalk(Game.CursorPos, target);
                }
            }

        }

        private GameObject getNearestSoilderToEnemy(Obj_AI_Base target)
        {
            var soilder = xSLxOrbwalker.Soilders.ToList().OrderBy(x => target.Distance(x.Position));

            if (soilder.FirstOrDefault() != null)
                return soilder.FirstOrDefault();

            return null;
        }

        private void Farm()
        {
            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All, MinionTeam.NotAlly);
            var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var min = menu.Item("qFarm", true).GetValue<Slider>().Value;


            if (useQ && (Q.IsReady() || QSpell.State == SpellState.Surpressed))
            {
                int hit;
                if (soilderCount() > 0)
                {
                    var slaves = xSLxOrbwalker.Soilders.ToList();
                    foreach (var slave in slaves)
                    {
                        foreach (var enemy in allMinionsQ)
                        {
                            hit = 0;
                            var prediction = GetP(slave.Position, Q, enemy, true);

                            if (Q.IsReady() && Player.Distance(enemy.Position) <= Q.Range)
                            {
                                hit += allMinionsQ.Count(enemy2 => enemy2.Distance(prediction.CastPosition) < 200 && Q.IsReady());
                                if (hit >= min)
                                {
                                    if (Q.IsReady())
                                    {
                                        Q.Cast(prediction.CastPosition, packets());
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                if (W.IsReady())
                {
                    var wpred = W.GetCircularFarmLocation(allMinionsW);
                    if(wpred.MinionsHit > 0)
                        W.Cast(wpred.Position);

                    foreach (var enemy in allMinionsQ)
                    {
                        hit = 0;
                        var prediction = GetP(Player.ServerPosition, Q, enemy, true);

                        if (Q.IsReady() && Player.Distance(enemy.Position) <= Q.Range)
                        {
                            hit += allMinionsQ.Count(enemy2 => enemy2.Distance(prediction.CastPosition) < 200 && Q.IsReady());
                            if (hit >= min)
                            {
                                if (Q.IsReady())
                                {
                                    Q.Cast(prediction.CastPosition, packets());
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private Vector3 _point;

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            SmartKs();

            if (menu.Item("point", true).GetValue<KeyBind>().Active)
            {
                _point = Game.CursorPos;
            }

            if (menu.Item("escape", true).GetValue<KeyBind>().Active)
            {
                Escape();
            }
            else if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else if (menu.Item("insec", true).GetValue<KeyBind>().Active)
            {
                xSLxOrbwalker.Orbwalk(Game.CursorPos, null);

                _insecTarget = TargetSelector.GetSelectedTarget();
                if (_insecTarget != null)
                    Insec();
            }
            else if (menu.Item("qeCombo", true).GetValue<KeyBind>().Active)
            {
                var soilderTarget = TargetSelector.GetTarget(900, TargetSelector.DamageType.Magical);

                xSLxOrbwalker.Orbwalk(Game.CursorPos, null);
                CastQe(soilderTarget);
            }
            else
            {
                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                {
                    Farm();
                }

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("wAtk", true).GetValue<bool>())
                    AutoAtk();
            }

            if (Player.Distance(_point) > QExtend.Range + W.Range)
            {
                _point = Vector3.Zero;
            }
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range", true).GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
            if (menu.Item("QExtendRange", true).GetValue<Circle>().Active)
                Render.Circle.DrawCircle(Player.Position, QExtend.Range, Color.LightBlue);

            if (menu.Item("slaveDmg", true).GetValue<Circle>().Active)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team && enemy.IsValid && !enemy.IsDead))
                {
                    var wtsz = Drawing.WorldToScreen(enemy.Position);
                    Drawing.DrawText(wtsz[0], wtsz[1], Color.White, "AA To Kill: " + Math.Ceiling((enemy.Health / xSLxOrbwalker.GetAzirAaSandwarriorDamage(enemy))));
                }
            }

            Vector2 wts = Drawing.WorldToScreen(Player.Position);
            if (menu.Item("qMulti", true).GetValue<KeyBind>().Active)
                Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "Q If 2+ Soldier");
            else
                Drawing.DrawText(wts[0] - 20, wts[1], Color.Red, "Q If 2+ Soldier");

            if (_point != Vector3.Zero && !menu.Item("fastEscape", true).GetValue<bool>())
            {
                var vec = Player.ServerPosition + Vector3.Normalize(Game.CursorPos - Player.ServerPosition) * W.Range;
                var vecPoint = vec + Vector3.Normalize(_point - vec) * Q.Range;
                if (soilderCount() > 0 && GetNearestSoilderToMouse() != null)
                {
                    vec = GetNearestSoilderToMouse().Position;
                    vecPoint = GetNearestSoilderToMouse().Position + Vector3.Normalize(_point - GetNearestSoilderToMouse().Position) * Q.Range;
                }

                var wts1 = Drawing.WorldToScreen(vec);
                var wts2 = Drawing.WorldToScreen(vecPoint);
                var wts3 = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawLine(wts3, wts1, 1, Color.Green);
                Drawing.DrawLine(wts1, wts2, 1, Color.Green);
                Render.Circle.DrawCircle(_point, 50, Color.Yellow);
            }
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap", true).GetValue<bool>()) return;

            if (R.IsReady() && gapcloser.Sender.IsValidTarget(R.Range))
                R.Cast(gapcloser.Sender);
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!menu.Item("UseInt", true).GetValue<bool>()) return;

            if (Player.Distance(unit.Position) < R.Range && unit != null && R.IsReady())
            {
                R.CastOnUnit(unit);
            }
        }
    }
}
