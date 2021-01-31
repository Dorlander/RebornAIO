using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Jayce : Champion
    {
        public Jayce()
        {
            SetSpells();
            LoadMenu();
        }

        private void SetSpells()
        {
            Q = new Spell(SpellSlot.Q, 1050);
            _qCharge = new Spell(SpellSlot.Q, 1650);
            Q2 = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W);
            W2 = new Spell(SpellSlot.W, 350);
            E = new Spell(SpellSlot.E, 650);
            E2 = new Spell(SpellSlot.E, 240);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.15f, 60, 1200, true, SkillshotType.SkillshotLine);
            _qCharge.SetSkillshot(0.25f, 60, 1600, true, SkillshotType.SkillshotLine);
            Q2.SetTargetted(0.25f, float.MaxValue);
            E.SetSkillshot(0.1f, 120, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E2.SetTargetted(.25f, float.MaxValue);
        }

        private Spell _qCharge;

        private void LoadMenu()
        {
            //Keys
            var key = new Menu("Keys", "Keys"); { 
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!", true).SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("shootMouse", "Shoot QE Mouse", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                menu.AddSubMenu(key);
            }

            //Combo menu:
            var combo = new Menu("Combo", "Combo"); { 
                combo.AddItem(new MenuItem("UseQCombo", "Use Cannon Q", true).SetValue(true));
                combo.AddItem(new MenuItem("qSpeed", "QE Speed, Higher = Faster, Lower = Accurate", true).SetValue(new Slider(1600, 400, 2500)));
                combo.AddItem(new MenuItem("UseWCombo", "Use Cannon W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use Cannon E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseQComboHam", "Use Hammer Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWComboHam", "Use Hammer W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseEComboHam", "Use Hammer E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R to Switch", true).SetValue(true));
                menu.AddSubMenu(combo);
            }

            //Harass menu:
            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(true));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                harass.AddItem(new MenuItem("UseQHarassHam", "Use Q Hammer", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarassHam", "Use W Hammer", true).SetValue(true));
                harass.AddItem(new MenuItem("UseEHarassHam", "Use E Hammer", true).SetValue(true));
                harass.AddItem(new MenuItem("UseRHarass", "Use R to switch", true).SetValue(true));
                harass.AddItem(new MenuItem("manaH", "Mana > %", true).SetValue(new Slider(40)));
                menu.AddSubMenu(harass);
            }

            //Misc Menu:
            var misc = new Menu("Misc", "Misc"); { 
                misc.AddItem(new MenuItem("UseInt", "Use E to Interrupt", true).SetValue(true));
                misc.AddItem(new MenuItem("UseGap", "Use E for GapCloser", true).SetValue(true));
                misc.AddItem(new MenuItem("forceGate", "Force Gate After Q", true).SetValue(false));
                misc.AddItem(new MenuItem("gatePlace", "Gate Distance", true).SetValue(new Slider(300, 50, 600)));
                misc.AddItem(new MenuItem("UseQAlways", "Use Q When E onCD", true).SetValue(true));
                misc.AddItem(new MenuItem("autoE", "EPushInCombo HP < %", true).SetValue(new Slider(20)));
                misc.AddItem(new MenuItem("smartKS", "Smart KS", true).SetValue(true));
                menu.AddSubMenu(misc);
            }

            //Drawings menu:
            var drawMenu = new Menu("Drawings", "Drawings"); {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "Disable All", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "Draw Q Cannon", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_QExtend", "Draw Q Cannon Extended", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "Draw E Cannon", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_Q2", "Draw Q Hammer", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E2", "Draw E Hammer", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("drawcds", "Draw Cooldowns", true).SetValue(true));

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

        //status
        private bool _hammerTime;
        private readonly SpellDataInst _qdata = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q);

        //CoolDowns
        private readonly float[] _cannonQcd = { 8, 8, 8, 8, 8 };
        private readonly float[] _cannonWcd = { 14, 12, 10, 8, 6 };
        private readonly float[] _cannonEcd = { 16, 16, 16, 16, 16 };

        private readonly float[] _hammerQcd = { 16, 14, 12, 10, 8 };
        private readonly float[] _hammerWcd = { 10, 10, 10, 10, 10 };
        private readonly float[] _hammerEcd = { 14, 13, 12, 11, 10 };

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            if (enemy == null)
                return 0;

            var damage = 0d;

            if (_canQcd == 0 && _canEcd == 0 && Q.Level > 0 && E.Level > 0)
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q) * 1.4;
            else if (_canQcd == 0 && Q.Level > 0)
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (_hamQcd == 0 && Q.Level > 0)
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q, 1);

            if (_hamWcd == 0 && W.Level > 0)
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (_hamEcd == 0 && E.Level > 0)
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            damage = ActiveItems.CalcDamage(enemy, damage);

            damage += Player.GetAutoAttackDamage(enemy)*3;
            return (float)damage;
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseQComboHam", true).GetValue<bool>(), menu.Item("UseWComboHam", true).GetValue<bool>(),
                menu.Item("UseEComboHam", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");

        }
        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>(), menu.Item("UseQHarassHam", true).GetValue<bool>(), menu.Item("UseWHarassHam", true).GetValue<bool>(),
                menu.Item("UseEHarassHam", true).GetValue<bool>(), menu.Item("UseRHarass", true).GetValue<bool>(), "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useQ2, bool useW2, bool useE2, bool useR, String source)
        {
            var qTarget = TargetSelector.GetTarget(_qCharge.Range, TargetSelector.DamageType.Physical);
            var q2Target = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);
            var e2Target = TargetSelector.GetTarget(E2.Range, TargetSelector.DamageType.Physical);

            //mana manager for harass
            var mana = menu.Item("manaH", true).GetValue<Slider>().Value;
            var manaPercent = Player.Mana / Player.MaxMana * 100;

            //Main Combo
            if (source == "Combo")
            {

                if (qTarget != null)
                {
                    if (useQ && _canQcd == 0 && Player.Distance(qTarget.Position) <= _qCharge.Range && !_hammerTime)
                    {
                        CastQCannon(qTarget, useE);
                        return;
                    }
                }

                if (_hammerTime)
                {
                    if (q2Target != null)
                    {
                        if (useW2 && Player.Distance(q2Target.Position) <= 300 && W.IsReady())
                            W.Cast();

                        if (useQ2 && Player.Distance(q2Target.Position) <= Q2.Range + q2Target.BoundingRadius && Q2.IsReady())
                            Q2.Cast(q2Target, packets());
                    }
                    if (e2Target != null) { 
                        if (useE2 && ECheck(e2Target, useQ, useW) && Player.Distance(e2Target.Position) <= E2.Range + e2Target.BoundingRadius && E2.IsReady())
                                E2.Cast(q2Target, packets());
                    }
                }

                var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                if (itemTarget != null)
                {
                    var dmg = GetComboDamage(itemTarget);
                    ActiveItems.Target = itemTarget;

                    //see if killable
                    if (dmg > itemTarget.Health - 50)
                        ActiveItems.KillableTarget = true;

                    ActiveItems.UseTargetted = true;
                }

                //form switch check
                if (useR)
                    SwitchFormCheck(q2Target, useQ, useW, useQ2, useW2, useE2);
            }
            else if (source == "Harass" && manaPercent > mana)
            {
                if (qTarget != null)
                {
                    if (useQ && _canQcd == 0 && Player.Distance(qTarget.Position) <= _qCharge.Range && !_hammerTime)
                    {
                        CastQCannon(qTarget, useE);
                        return;
                    }
                }
                if (_hammerTime)
                {
                    if (q2Target != null)
                    {
                        if (useW2 && Player.Distance(q2Target.Position) <= 300 && W.IsReady())
                            W.Cast();

                        if (useQ2 && Player.Distance(q2Target.Position) <= Q2.Range + q2Target.BoundingRadius && Q2.IsReady())
                            Q2.Cast(q2Target, packets());
                    }

                    if (e2Target != null)
                    {
                        if (useE2 && Player.Distance(q2Target.Position) <= E2.Range + e2Target.BoundingRadius && E2.IsReady())
                            E2.Cast(q2Target, packets());
                    }
                }

                //form switch check
                if (useR && q2Target != null)
                    SwitchFormCheck(q2Target, useQ, useW, useQ2, useW2, useE2);
            }

        }

        private bool ECheck(Obj_AI_Hero target, bool useQ, bool useW)
        {
            if (Player.GetSpellDamage(target, SpellSlot.E) >= target.Health)
            {
                return true;
            }
            if (((_canQcd == 0 && useQ) || (_canWcd == 0 && useW)) && _hamQcd != 0 && _hamWcd != 0)
            {
                return true;
            }
            if (WallStun(target))
            {
                //Game.PrintChat("Walled");
                return true;
            }

            var hp = menu.Item("autoE", true).GetValue<Slider>().Value;
            var hpPercent = Player.Health / Player.MaxHealth * 100;

            if (hpPercent <= hp)
            {
                return true;
            }

            return false;
        }

        private bool WallStun(Obj_AI_Hero target)
        {
            var pred = E2.GetPrediction(target);

            var pushedPos = pred.CastPosition + Vector3.Normalize(pred.CastPosition - Player.ServerPosition) * 350;

            if (IsPassWall(target.ServerPosition, pushedPos))
                return true;

            return false;
        }

        private void KsCheck()
        {
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(_qCharge.Range) && x.IsEnemy && !x.IsDead).OrderByDescending(GetComboDamage))
            {
                //Q
                if ((Player.GetSpellDamage(enemy, SpellSlot.Q) - 20) > enemy.Health && _canQcd == 0 && Q.GetPrediction(enemy).Hitchance >= HitChance.High && Player.Distance(enemy.ServerPosition) <= Q.Range)
                {
                    if (_hammerTime && R.IsReady())
                        R.Cast();

                    if (!_hammerTime && Q.IsReady())
                        Q.Cast(enemy, packets());
                }

                //QE
                if ((Player.GetSpellDamage(enemy, SpellSlot.Q) * 1.4 - 20) > enemy.Health && _canQcd == 0 && _canEcd == 0 && Player.Distance(enemy.ServerPosition) <= _qCharge.Range)
                {
                    if (_hammerTime && R.IsReady())
                        R.Cast();

                    if (!_hammerTime)
                        CastQCannon(enemy, true);
                }

                //Hammer QE
                if ((Player.GetSpellDamage(enemy, SpellSlot.E) + Player.GetSpellDamage(enemy, SpellSlot.Q, 1) - 20) > enemy.Health
                    && _hamEcd == 0 && _hamQcd == 0 && Player.Distance(enemy.ServerPosition) <= Q2.Range + enemy.BoundingRadius)
                {
                    if (!_hammerTime && R.IsReady())
                        R.Cast();

                    if (_hammerTime && Q2.IsReady() && E2.IsReady())
                    {
                        Q2.Cast(enemy, packets());
                        E2.Cast(enemy, packets());
                        return;
                    }
                }

                //Hammer Q
                if ((Player.GetSpellDamage(enemy, SpellSlot.Q, 1) - 20) > enemy.Health && _hamQcd == 0 && Player.Distance(enemy.ServerPosition) <= Q2.Range + enemy.BoundingRadius)
                {
                    if (!_hammerTime && R.IsReady())
                        R.Cast();

                    if (_hammerTime && Q2.IsReady())
                    {
                        Q2.Cast(enemy, packets());
                        return;
                    }
                }

                //Hammer E
                if ((Player.GetSpellDamage(enemy, SpellSlot.E) - 20) > enemy.Health && _hamEcd == 0 && Player.Distance(enemy.ServerPosition) <= E2.Range + enemy.BoundingRadius)
                {
                    if (!_hammerTime && R.IsReady() && enemy.Health > 80)
                        R.Cast();

                    if (_hammerTime && E2.IsReady())
                    {
                        E2.Cast(enemy, packets());
                        return;
                    }
                }
            }
        }

        private void SwitchFormCheck(Obj_AI_Hero target, bool useQ, bool useW, bool useQ2, bool useW2, bool useE2)
        {
            if (target == null)
                return;

            if (target.Health > 80)
            {
                //switch to hammer
                if ((_canQcd != 0 || !useQ) &&
                    (_canWcd != 0 && !HyperCharged() || !useW) && R.IsReady() &&
                     HammerAllReady() && !_hammerTime && Player.Distance(target.ServerPosition) < 650 &&
                     (useQ2 || useW2 || useE2))
                {
                    //Game.PrintChat("Hammer Time");
                    R.Cast();
                    return;
                }
            }

            //switch to cannon
            if (((_canQcd == 0 && useQ) || (_canWcd == 0 && useW) && R.IsReady())
                && _hammerTime)
            {
                //Game.PrintChat("Cannon Time");
                R.Cast();
                return;
            }

            if (_hamQcd != 0 && _hamWcd != 0 && _hamEcd != 0 && _hammerTime && R.IsReady())
            {
                R.Cast();
            }
        }

        private bool HyperCharged()
        {
            foreach (var buffs in Player.Buffs)
            {
                if (buffs.Name == "jaycehypercharge")
                    return true;
            }
            return false;
        }

        private bool HammerAllReady()
        {
            if (_hamQcd == 0 && _hamWcd == 0 && _hamEcd == 0)
            {
                return true;
            }
            return false;
        }

        private void CastQCannon(Obj_AI_Hero target, bool useE)
        {
            var gateDis = menu.Item("gatePlace", true).GetValue<Slider>().Value;
            var qSpeed = menu.Item("qSpeed", true).GetValue<Slider>().Value;

            _qCharge.Speed = qSpeed;

            var tarPred = _qCharge.GetPrediction(target);

            if (tarPred.Hitchance >= HitChance.High && _canQcd == 0 && _canEcd == 0 && useE)
            {
                var gateVector = Player.Position + Vector3.Normalize(target.ServerPosition - Player.Position) * gateDis;

                if (Player.Distance(tarPred.CastPosition) < _qCharge.Range + 100)
                {
                    if (E.IsReady() && _qCharge.IsReady())
                    {
                        E.Cast(gateVector, packets());
                        _qCharge.Cast(tarPred.CastPosition, packets());
                        return;
                    }
                }
            }

            if ((menu.Item("UseQAlways", true).GetValue<bool>() || !useE) && _canQcd == 0 && Q.GetPrediction(target).Hitchance >= HitChance.High && Player.Distance(target.ServerPosition) <= Q.Range && Q.IsReady())
            {
                Q.Cast(target, packets());
            }

        }

        private void CastQCannonMouse()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (_hammerTime && !R.IsReady())
                return;

            if (_hammerTime && R.IsReady())
            {
                R.Cast();
                return;
            }

            if (_canEcd == 0 && _canQcd == 0 && !_hammerTime)
            {
                var gateDis = menu.Item("gatePlace", true).GetValue<Slider>().Value;
                var gateVector = Player.ServerPosition + Vector3.Normalize(Game.CursorPos - Player.ServerPosition) * gateDis;

                if (E.IsReady() && Q.IsReady())
                {
                    E.Cast(gateVector, packets());
                    Q.Cast(Game.CursorPos, packets());
                }
            }
        }

        private float _canQcd, _canWcd, _canEcd;
        private float _hamQcd, _hamWcd, _hamEcd;
        private float _canQcdRem, _canWcdRem, _canEcdRem;
        private float _hamQcdRem, _hamWcdRem, _hamEcdRem;

        private void ProcessCooldowns()
        {
            _canQcd = ((_canQcdRem - Game.Time) > 0) ? (_canQcdRem - Game.Time) : 0;
            _canWcd = ((_canWcdRem - Game.Time) > 0) ? (_canWcdRem - Game.Time) : 0;
            _canEcd = ((_canEcdRem - Game.Time) > 0) ? (_canEcdRem - Game.Time) : 0;
            _hamQcd = ((_hamQcdRem - Game.Time) > 0) ? (_hamQcdRem - Game.Time) : 0;
            _hamWcd = ((_hamWcdRem - Game.Time) > 0) ? (_hamWcdRem - Game.Time) : 0;
            _hamEcd = ((_hamEcdRem - Game.Time) > 0) ? (_hamEcdRem - Game.Time) : 0;
        }

        private float CalculateCd(float time)
        {
            return time + (time * Player.PercentCooldownMod);
        }

        private void GetCooldowns(GameObjectProcessSpellCastEventArgs spell)
        {
            if (_hammerTime)
            {
                if (spell.SData.Name == "JayceToTheSkies")
                    _hamQcdRem = Game.Time + CalculateCd(_hammerQcd[Q.Level - 1]);
                if (spell.SData.Name == "JayceStaticField")
                    _hamWcdRem = Game.Time + CalculateCd(_hammerWcd[W.Level - 1]);
                if (spell.SData.Name == "JayceThunderingBlow")
                    _hamEcdRem = Game.Time + CalculateCd(_hammerEcd[E.Level - 1]);
            }
            else
            {
                if (spell.SData.Name == "jayceshockblast")
                    _canQcdRem = Game.Time + CalculateCd(_cannonQcd[Q.Level - 1]);
                if (spell.SData.Name == "jaycehypercharge")
                    _canWcdRem = Game.Time + CalculateCd(_cannonWcd[W.Level - 1]);
                if (spell.SData.Name == "jayceaccelerationgate")
                    _canEcdRem = Game.Time + CalculateCd(_cannonEcd[E.Level - 1]);
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //Console.Clear();
            //check if player is dead
            if (Player.IsDead) return;

            //cd check
            ProcessCooldowns();

            //Check form
            _hammerTime = !_qdata.Name.Contains("jayceshockblast");

            //ks check
            if (menu.Item("smartKS", true).GetValue<bool>())
                KsCheck();

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("shootMouse", true).GetValue<KeyBind>().Active)
                    CastQCannonMouse();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();
            }
        }

        protected override void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var useWCombo = menu.Item("UseWCombo", true).GetValue<bool>();
            var useWHarass = menu.Item("UseWHarass", true).GetValue<bool>();

            if (unit.IsMe && !_hammerTime)
            {
                if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
                {
                    if (_canWcd == 0 && Player.Distance(target.Position) < 600 && !_hammerTime && W.Level > 0 && W.IsReady())
                        if (useWCombo)
                        {
                            Orbwalking.ResetAutoAttackTimer();
                            xSLxOrbwalker.ResetAutoAttackTimer();
                            W.Cast();
                        }
                }

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active || menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                {
                    if (_canWcd == 0 && Player.Distance(target.Position) < 600 && !_hammerTime && W.Level > 0 && W.IsReady() && target is Obj_AI_Hero)
                        if (useWHarass)
                        {
                            Orbwalking.ResetAutoAttackTimer();
                            xSLxOrbwalker.ResetAutoAttackTimer();
                            W.Cast();
                        }
                }
            }
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            if (menu.Item("Draw_Q", true).GetValue<bool>() && !_hammerTime)
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_QExtend", true).GetValue<bool>() && !_hammerTime)
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, _qCharge.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_Q2", true).GetValue<bool>() && _hammerTime)
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q2.Range, W.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>() && !_hammerTime)
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E2", true).GetValue<bool>() && _hammerTime)
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E2.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("drawcds", true).GetValue<bool>())
            {
                var wts = Drawing.WorldToScreen(Player.Position);
                if (_hammerTime)
                {

                    if (_canQcd == 0)
                        Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q Ready");
                    else
                        Drawing.DrawText(wts[0] - 80, wts[1], Color.Orange, "Q: " + _canQcd.ToString("0.0"));
                    if (_canWcd == 0)
                        Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W Ready");
                    else
                        Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.Orange, "W: " + _canWcd.ToString("0.0"));
                    if (_canEcd == 0)
                        Drawing.DrawText(wts[0], wts[1], Color.White, "E Ready");
                    else
                        Drawing.DrawText(wts[0], wts[1], Color.Orange, "E: " + _canEcd.ToString("0.0"));

                }
                else
                {
                    if (_hamQcd == 0)
                        Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q Ready");
                    else
                        Drawing.DrawText(wts[0] - 80, wts[1], Color.Orange, "Q: " + _hamQcd.ToString("0.0"));
                    if (_hamWcd == 0)
                        Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W Ready");
                    else
                        Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.Orange, "W: " + _hamWcd.ToString("0.0"));
                    if (_hamEcd == 0)
                        Drawing.DrawText(wts[0], wts[1], Color.White, "E Ready");
                    else
                        Drawing.DrawText(wts[0], wts[1], Color.Orange, "E: " + _hamEcd.ToString("0.0"));
                }
            }
        }

        protected override void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!(sender is MissileClient))
                return;

            var spell = (MissileClient)sender;
            var unit = spell.SpellCaster.Name;
            var name = spell.SData.Name;

            if (unit == null)
                return;

            if (unit == Player.Name && name == "JayceShockBlastMis")
            {
                if (menu.Item("forceGate", true).GetValue<bool>() && _canEcd == 0 && E.IsReady())
                {
                    var vec = spell.Position - Vector3.Normalize(Player.ServerPosition - spell.Position) * 100;
                    E.Cast(vec, packets());
                }
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs attack)
        {
            if (unit.IsMe)
                GetCooldowns(attack);
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap", true).GetValue<bool>()) return;

            if (_hamEcd == 0 && gapcloser.Sender.IsValidTarget(E2.Range + gapcloser.Sender.BoundingRadius))
            {
                if (!_hammerTime && R.IsReady())
                    R.Cast();

                if (E2.IsReady())
                    E2.Cast(gapcloser.Sender, packets());
            }
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!menu.Item("UseInt", true).GetValue<bool>()) return;

            if (unit != null && Player.Distance(unit.Position) < Q2.Range + unit.BoundingRadius && _hamQcd == 0 && _hamEcd == 0)
            {
                if (!_hammerTime && R.IsReady())
                    R.Cast();

                if (Q2.IsReady())
                    Q2.Cast(unit, packets());
            }

            if (unit != null && (Player.Distance(unit.Position) < E2.Range + unit.BoundingRadius && _hamEcd == 0))
            {
                if (!_hammerTime && R.IsReady())
                    R.Cast();

                if (E2.IsReady())
                    E2.Cast(unit, packets());
            }
        }

    }
}
