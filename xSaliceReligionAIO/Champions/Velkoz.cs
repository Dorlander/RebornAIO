using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Velkoz : Champion
    {
        private Spell _qSplit;
        private Spell _qDummy;
        private MissileClient _qMissle;

        public Velkoz()
        {
            QMana = new[] {40, 40, 45, 50, 55, 60};
            WMana = new[] {50, 50, 55, 60, 65, 70};
            EMana = new[] {50, 50, 55, 60, 65, 70};
            RMana = new[] {100, 100, 100, 100};

            LoadSpell();
            LoadMenu();
        }

        private void LoadSpell()
        {
            //intalize spell
            Q = new Spell(SpellSlot.Q, 1000);
            _qSplit = new Spell(SpellSlot.Q, 800);
            _qDummy = new Spell(SpellSlot.Q, (float)Math.Sqrt(Math.Pow(Q.Range, 2) + Math.Pow(_qSplit.Range, 2)));
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 850);
            R = new Spell(SpellSlot.R, 1500);

            Q.SetSkillshot(0.25f, 60f, 1300f, true, SkillshotType.SkillshotLine);
            _qDummy.SetSkillshot(0.25f, 65f, float.MaxValue, false, SkillshotType.SkillshotLine);
            _qSplit.SetSkillshot(0.25f, 55f, 2100, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 10f, 1700f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.5f, 80f, 1500f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.3f, 1f, float.MaxValue, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private void LoadMenu()
        {
            //key
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("R_Mouse", "R To Mouse", true).SetValue(new KeyBind("R".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            //Spell Menu
            var spell = new Menu("Spell", "Spell");
            {
                //Q Menu
                var qMenu = new Menu("QSpell", "QSpell"); {
                    qMenu.AddItem(new MenuItem("qSplit", "Auto Split Q", true).SetValue(true));
                    qMenu.AddItem(new MenuItem("qAngle", "Shoot Q At Angle", true).SetValue(true));
                    spell.AddSubMenu(qMenu);
                }   
                //R
                var rMenu = new Menu("RSpell", "RSpell");
                {
                    rMenu.AddItem(new MenuItem("rAimer", "R Aim", true).SetValue(new StringList(new[] { "Auto", "To Mouse" })));
                    rMenu.AddSubMenu(new Menu("Dont use R on", "DontUlt"));
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                        rMenu.SubMenu("DontUlt").AddItem(new MenuItem("DontUlt" + enemy.BaseSkinName, enemy.BaseSkinName, true).SetValue(false));
                    rMenu.AddItem(new MenuItem("R_Max_Dist", "R Max Distance", true).SetValue(new Slider(1000, 200, 1300)));
                    spell.AddSubMenu(rMenu);
                }
                menu.AddSubMenu(spell);
            }

            //Combo menu:
            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "Use Q", true).SetValue(true));
                combo.AddItem(new MenuItem("qHit", "Q HitChance", true).SetValue(new Slider(3, 1, 4)));
                combo.AddItem(new MenuItem("UseWCombo", "Use W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R", true).SetValue(true));
                menu.AddSubMenu(combo);
            }

            //Harass menu:
            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("qHit2", "Q HitChance", true).SetValue(new Slider(3, 1, 4)));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                menu.AddSubMenu(harass);
            }

            //Farming menu:
            var farm = new Menu("Farm", "Farm");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(false));
                farm.AddItem(new MenuItem("UseWFarm", "Use W", true).SetValue(false));
                farm.AddItem(new MenuItem("UseEFarm", "Use E", true).SetValue(false));
                menu.AddSubMenu(farm);
            }
            //Misc Menu:
            var misc = new Menu("Misc", "Misc");
            {
                misc.AddItem(new MenuItem("UseInt", "Use E to Interrupt", true).SetValue(true));
                misc.AddItem(new MenuItem("UseGap", "Use E for GapCloser", true).SetValue(true));
                misc.AddItem(new MenuItem("smartKS", "Use Smart KS System", true).SetValue(true));
                menu.AddSubMenu(misc);
            }

            //Drawings menu:
            var draw = new Menu("Drawings", "Drawings"); { 
                draw.AddItem(new MenuItem("QRange", "Q range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                draw.AddItem(new MenuItem("WRange", "W range", true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                draw.AddItem(new MenuItem("ERange", "E range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                draw.AddItem(new MenuItem("RRange", "R range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                draw.AddItem(new MenuItem("drawUlt", "Killable With ult", true).SetValue(true));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "Draw Combo Damage", true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "Draw Combo Damage Fill", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
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
            double damage = 0d;

            int collisionCount = Q.GetPrediction(enemy).CollisionObjects.Count;
            if (Q.IsReady() && collisionCount < 1)
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (W.IsReady())
                damage += W.Instance.Ammo * Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += GetUltDmg((Obj_AI_Hero)enemy);

            damage = ActiveItems.CalcDamage(enemy, damage);

            damage += GetPassiveDmg();

            return (float)damage;
        }

        private float GetPassiveDmg()
        {
            double stack = 0;
            double dmg = 25 + (10 * Player.Level);

            if (Q.IsReady())
                stack++;

            if (W.IsReady())
                stack += 2;

            if (E.IsReady())
                stack++;

            stack = stack / 3;

            stack = Math.Floor(stack);

            dmg = dmg * stack;

            //Game.PrintChat("Stacks: " + stack);

            return (float)dmg;
        }
        private float GetUltDmg(Obj_AI_Hero target)
        {
            double dmg = 0;

            float dist = (Player.ServerPosition.To2D().Distance(target.ServerPosition.To2D()) - 600) / 100;
            double div = Math.Ceiling(10 - dist);

            //Game.PrintChat("ult dmg" + target.BaseSkinName + " " + div);

            if (Player.Distance(target.Position) < 600)
                div = 10;

            if (Player.Distance(target.Position) < 1550)
                if (R.IsReady())
                {
                    double ultDmg = Player.GetSpellDamage(target, SpellSlot.R) / 10;

                    dmg += ultDmg * div;
                }

            if (div >= 3)
                dmg += 25 + (10 * Player.Level);

            if (menu.Item("drawUlt", true).GetValue<bool>())
            {
                if (R.IsReady() && dmg > target.Health + 20)
                {
                    Vector2 wts = Drawing.WorldToScreen(target.Position);
                    Drawing.DrawText(wts[0], wts[1], Color.White, "Killable with Ult");
                }
                else
                {
                    Vector2 wts = Drawing.WorldToScreen(target.Position);
                    Drawing.DrawText(wts[0], wts[1], Color.Red, "No Ult Kill");
                }
            }

            return (float)dmg;
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
            var range = R.IsReady() ? R.Range : Q.Range;

            Obj_AI_Hero target = TargetSelector.GetTarget(range, TargetSelector.DamageType.Magical);

            Obj_AI_Hero qDummyTarget = TargetSelector.GetTarget(_qDummy.Range, TargetSelector.DamageType.Magical);

            if (target == null)
                return;

            float dmg = GetComboDamage(target);

            useR = (menu.Item("DontUlt" + target.BaseSkinName, true) != null && menu.Item("DontUlt" + target.BaseSkinName, true).GetValue<bool>() == false) && useR;

            if (useW && W.IsReady() && Player.Distance(target.Position) <= W.Range &&
                W.GetPrediction(target).Hitchance >= HitChance.High)
            {
                W.Cast(target);
                return;
            }

            if (useE && E.IsReady() && Player.Distance(target.Position) < E.Range &&
                E.GetPrediction(target).Hitchance >= HitChance.High)
            {
                E.Cast(target, packets());
                return;
            }

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

            if (useQ && Q.IsReady())
            {
                CastQ(target, qDummyTarget, source);
                return;
            }

            if (useR && R.IsReady() && Player.Distance(target.Position) < R.Range)
            {
                if (GetUltDmg(target) >= target.Health)
                {
                    R.Cast(target.ServerPosition);
                }

            }
        }

        private void Farm()
        {
            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                Q.Range + Q.Width, MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range,
                MinionTypes.Ranged, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range,
                MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useW && W.IsReady() && allMinionsW.Count > 0)
            {
                MinionManager.FarmLocation wPos = W.GetLineFarmLocation(allMinionsW);

                if (wPos.MinionsHit > 2)
                    W.Cast(wPos.Position, packets());
            }

            if (useE && allMinionsE.Count > 0 && E.IsReady())
            {
                MinionManager.FarmLocation ePos = E.GetCircularFarmLocation(allMinionsE);

                if (ePos.MinionsHit > 2)
                    E.Cast(ePos.Position, packets());
            }

            if (useQ && Q.IsReady() && allMinionsQ.Count > 0)
            {
                MinionManager.FarmLocation qPos = Q.GetLineFarmLocation(allMinionsQ);

                Q.Cast(qPos.Position, packets());
            }
        }

        private void CastQ(Obj_AI_Hero target, Obj_AI_Hero targetExtend, string source)
        {
            PredictionOutput pred = Q.GetPrediction(target);
            int collision = pred.CollisionObjects.Count;

            //cast Q with no collision
            if (Player.Distance(target.Position) < 1050 && Q.Instance.Name == "VelkozQ")
            {
                if (collision == 0)
                {
                    if (pred.Hitchance >= GetHitchance(source))
                    {
                        Q.Cast(pred.CastPosition, packets());
                    }

                    return;
                }
            }

            if (!menu.Item("qAngle", true).GetValue<bool>())
                return;

            if (Q.Instance.Name == "VelkozQ" && targetExtend != null)
            {
                _qDummy.Delay = Q.Delay + Q.Range / Q.Speed * 1000 + _qSplit.Range / _qSplit.Speed * 1000;
                pred = _qDummy.GetPrediction(targetExtend);

                if (pred.Hitchance >= GetHitchance(source))
                {
                    //math by esk0r <3
                    for (int i = -1; i < 1; i = i + 2)
                    {
                        const float alpha = 28 * (float)Math.PI / 180;
                        Vector2 cp = Player.ServerPosition.To2D() +
                                     (pred.CastPosition.To2D() - Player.ServerPosition.To2D()).Rotated(i * alpha);

                        //Render.Circle.DrawCircle(cp.To3D(), 100, Color.Blue, 1, 1);

                        if (Q.GetCollision(Player.ServerPosition.To2D(), new List<Vector2> { cp }).Count == 0 &&
                            _qSplit.GetCollision(cp, new List<Vector2> { pred.CastPosition.To2D() }).Count == 0)
                        {
                            if (Player.Distance(cp) <= R.Range)
                            {
                                Q.Cast(cp, packets());
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void SplitMissle()
        {
            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(1500)))
            {
                if (_qMissle != null)
                {
                    _qSplit.UpdateSourcePosition(_qMissle.Position, _qMissle.Position);
                    PredictionOutput pred = _qSplit.GetPrediction(target);

                    Vector2 perpendicular = (_qMissle.EndPosition - _qMissle.StartPosition).To2D().Normalized().Perpendicular();

                    Vector2 lineSegment1End = _qMissle.Position.To2D() + perpendicular * _qSplit.Range;
                    Vector2 lineSegment2End = _qMissle.Position.To2D() - perpendicular * _qSplit.Range;

                    float d1 = pred.UnitPosition.To2D().Distance(_qMissle.Position.To2D(), lineSegment1End, true);
                    //Render.Circle.DrawCircle(lineSegment1End.To3D(), 50, Color.Blue);
                    float d2 = pred.UnitPosition.To2D().Distance(_qMissle.Position.To2D(), lineSegment2End, true);
                    //Render.Circle.DrawCircle(lineSegment2End.To3D(), 50, Color.Red);

                    //cast split
                    if ((d1 < _qSplit.Width || d2 < _qSplit.Width) && pred.Hitchance >= HitChance.Medium)
                    {
                        Q.Cast();
                        _qMissle = null;
                        //Game.PrintChat("splitted");
                    }
                }
            }

        }

        private void SmartKs()
        {
            if (!menu.Item("smartKS", true).GetValue<bool>())
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.IsValidTarget(Q.Range) && x.IsEnemy && !x.IsDead).OrderByDescending(GetComboDamage))
            {
                //Q
                if (Player.Distance(target.ServerPosition) <= Q.Range &&
                    (Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 30)
                {
                    if (Q.IsReady())
                    {
                        CastQ(target, target, "Combo");
                        return;
                    }
                }

                //EW
                if (Player.Distance(target.ServerPosition) <= E.Range && (Player.GetSpellDamage(target, SpellSlot.E) + Player.GetSpellDamage(target, SpellSlot.W)) > target.Health + 30)
                {
                    if (W.IsReady() && E.IsReady())
                    {
                        E.Cast(target);
                        W.Cast(target, packets());
                        return;
                    }
                }

                //E
                if (Player.Distance(target.ServerPosition) <= E.Range &&
                    (Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 30)
                {
                    if (E.IsReady())
                    {
                        E.CastOnUnit(target, packets());
                        return;
                    }
                }

                //W
                if (Player.Distance(target.ServerPosition) <= W.Range &&
                    (Player.GetSpellDamage(target, SpellSlot.W)) > target.Health + 50)
                {
                    if (W.IsReady())
                    {
                        W.Cast(target, packets());
                        return;
                    }
                }

            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //Console.Clear();
            //check if player is dead
            if (Player.IsDead) return;

            if (Player.IsChannelingImportantSpell())
            {
                var range = R.IsReady() ? R.Range : Q.Range;
                Obj_AI_Hero target = TargetSelector.GetTarget(range, TargetSelector.DamageType.Magical);

                if (target != null)
                {
                    int aimMode = menu.Item("rAimer", true).GetValue<StringList>().SelectedIndex;

                    Player.Spellbook.UpdateChargedSpell(SpellSlot.R,
                        aimMode == 0 ? target.ServerPosition : Game.CursorPos, false, false);
                }
                Player.Spellbook.UpdateChargedSpell(SpellSlot.R, Game.CursorPos, false, false);
                return;
            }

            if (_qMissle != null && _qMissle.IsValid && menu.Item("qSplit", true).GetValue<bool>())
                SplitMissle();

            SmartKs();

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();
            }

            if (menu.Item("R_Mouse", true).GetValue<KeyBind>().Active)
                R.Cast(Game.CursorPos);

            R.Range = menu.Item("R_Max_Dist", true).GetValue<Slider>().Value;
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            foreach (Spell spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range", true).GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
        }


        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap", true).GetValue<bool>()) return;

            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
                E.Cast(gapcloser.Sender, packets());
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!menu.Item("UseInt", true).GetValue<bool>()) return;

            if (Player.Distance(unit.Position) < E.Range && unit != null && E.IsReady())
            {
                E.Cast(unit, packets());
            }
        }

        protected override void Spellbook_OnUpdateChargedSpell(Spellbook sender, SpellbookUpdateChargedSpellEventArgs args)
        {
            if (!sender.Owner.IsMe)
                return;

            args.Process = !(menu.Item("ComboActive", true).GetValue<KeyBind>().Active && menu.Item("UseRCombo", true).GetValue<bool>() && menu.Item("smartKS", true).GetValue<bool>()
                        && menu.Item("HarassActive", true).GetValue<KeyBind>().Active && menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active && menu.Item("R_Mouse", true).GetValue<KeyBind>().Active);
        }
        

        protected override void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            // return if its not a missle
            if (!(obj is MissileClient))
                return;

            var spell = (MissileClient)obj;

            if (Player.Distance(obj.Position) < 1500)
            {
                //Q
                if (spell.IsValid && spell.SData.Name == "VelkozQMissile")
                {
                    //Game.PrintChat("Woot");
                    _qMissle = spell;
                }
            }
        }
    }
}
