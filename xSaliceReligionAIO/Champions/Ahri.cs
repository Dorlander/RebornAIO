using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Ahri : Champion
    {
        public Ahri()
        {
            //Set up mana
            //Q
            QMana = new[] {55, 55, 60, 65, 70, 75};
            //W
            WMana = new[] { 50, 50, 50, 50, 50, 50 };
            //E
            EMana = new[] { 85, 85, 85, 85, 85, 85 };
            //R
            RMana = new[] { 100, 100, 100, 100 };
   
            //set up spells
            SetUpSpells();

            //Set up Menu
            LoadMenu();
            
        }

        private void SetUpSpells()
        {
            //intalize spell
            Q = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W, 650);
            E = new Spell(SpellSlot.E, 875);
            R = new Spell(SpellSlot.R, 850);

            Q.SetSkillshot(0.25f, 100, 1600, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 60, 1200, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        //Load Menu
        private void LoadMenu()
        {
            //key
            var key = new Menu("Key", "Key");{
                key.AddItem(new MenuItem("ComboActive", "Combo!",true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!",true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!",true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!",true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("charmCombo", "Q if Charmed in Combo",true).SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Toggle)));
                //add to menu
                menu.AddSubMenu(key);
            }

            //Combo menu:
            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("selected", "Focus Selected Target",true).SetValue(true));
                combo.AddItem(new MenuItem("UseQCombo", "Use Q",true).SetValue(true));
                combo.AddItem(new MenuItem("qHit", "Q/E HitChance",true).SetValue(new Slider(3, 1, 4)));
                combo.AddItem(new MenuItem("UseWCombo", "Use W",true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E",true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R",true).SetValue(true));
                combo.AddItem(new MenuItem("rSpeed", "Use All R fast Duel",true).SetValue(true));
                //add to menu
                menu.AddSubMenu(combo);
            }
            //Harass menu:
            var harass = new Menu("Harass", "Harass");
            {
               harass.AddItem(new MenuItem("UseQHarass", "Use Q",true).SetValue(true));
               harass.AddItem(new MenuItem("qHit2", "Q/E HitChance",true).SetValue(new Slider(3, 1, 4)));
               harass.AddItem(new MenuItem("UseWHarass", "Use W",true).SetValue(false));
               harass.AddItem(new MenuItem("UseEHarass", "Use E",true).SetValue(true));
               harass.AddItem(new MenuItem("longQ", "Cast Long range Q",true).SetValue(true));
               harass.AddItem(new MenuItem("charmHarass", "Only Q if Charmed",true).SetValue(true)); 
               //add to menu
                menu.AddSubMenu(harass);
            }
            //Farming menu:
            var farm = new Menu("Farm", "Farm");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q",true).SetValue(false));
                farm.AddItem(new MenuItem("UseWFarm", "Use W",true).SetValue(false));
                //add to menu
                menu.AddSubMenu(farm);
            }

            //Misc Menu:
            var misc = new Menu("Misc", "Misc");
            {
                misc.AddItem(new MenuItem("UseInt", "Use E to Interrupt",true).SetValue(true));
                misc.AddItem(new MenuItem("UseGap", "Use E for GapCloser",true).SetValue(true));
                misc.AddItem(new MenuItem("mana", "Mana check before use R",true).SetValue(true));
                misc.AddItem(new MenuItem("dfgCharm", "Require Charmed to DFG",true).SetValue(true));
                misc.AddItem(new MenuItem("EQ", "Use Q onTop of E",true).SetValue(true));
                misc.AddItem(new MenuItem("smartKS", "Smart KS",true).SetValue(true));
                misc.AddItem(new MenuItem("Prediction_Check_Off", "Use Prediciton Mode 2",true).SetValue(false));
                //add to menu
                menu.AddSubMenu(misc);
            }

            //Drawings menu:
            var drawing = new Menu("Drawings", "Drawings");
            {
                drawing.AddItem(
                        new MenuItem("QRange", "Q range",true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(
                        new MenuItem("WRange", "W range",true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(
                        new MenuItem("ERange", "E range",true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(
                        new MenuItem("RRange", "R range",true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(
                        new MenuItem("cursor", "Draw R Dash Range",true).SetValue(new Circle(false,Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(
                        new MenuItem("Draw_Mode", "Draw E Mode",true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "Draw Combo Damage",true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "Draw Combo Damage Fill",true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
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
                menu.AddSubMenu(drawing);
            }
        }
        
        //settings
        private static bool _rOn;
        private static int _rTimer;
        private static int _rTimeLeft;

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            if (enemy == null)
                return 0;

            double damage = 0d;

            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q, 1);
            }

            if (Dfg.IsReady())
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (_rOn)
                damage += Player.GetSpellDamage(enemy, SpellSlot.R)*RCount();
            else if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) * 3;

            damage = ActiveItems.CalcDamage(enemy, damage);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

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
            var range = Q.Range;
            Obj_AI_Hero eTarget = TargetSelector.GetTarget(range, TargetSelector.DamageType.Magical);


            if (GetTargetFocus(range) != null)
                eTarget = GetTargetFocus(range);

            Obj_AI_Hero rETarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            var hitC = GetHitchance(source);
            var dmg = GetComboDamage(eTarget);
            var predOff = menu.Item("Prediction_Check_Off", true).GetValue<bool>();

            if (eTarget == null)
                return;

            if (source == "Combo")
            {
                //items-------
                ActiveItems.Target = eTarget;

                //see if killable
                if (dmg > eTarget.Health - 50)
                    ActiveItems.KillableTarget = true;

                //Items
                if (eTarget.HasBuffOfType(BuffType.Charm) || eTarget.HasBuffOfType(BuffType.Taunt) || !menu.Item("dfgCharm", true).GetValue<bool>())
                    ActiveItems.UseTargetted = true;
            }
            //end items-------

            //E
            if (useE && E.IsReady() && Player.Distance(eTarget.Position) < E.Range)
            {
                if (E.GetPrediction(eTarget).Hitchance >= hitC || predOff)
                {
                    E.Cast(eTarget, packets());
                    if (menu.Item("EQ", true).GetValue<bool>() && Q.IsReady())
                    {
                        Q.Cast(eTarget, packets());
                    }
                    return;
                }
            }

            //W
            if (useW && W.IsReady() && Player.Distance(eTarget.Position) <= W.Range - 100 &&
                ShouldW(eTarget, source))
            {
                W.Cast();
            }

            if (source == "Harass" && menu.Item("longQ", true).GetValue<bool>())
            {
                if (useQ && Q.IsReady() && Player.Distance(eTarget.Position) <= Q.Range &&
                    ShouldQ(eTarget, source) && Player.Distance(eTarget.Position) > 600)
                {
                    if (Q.GetPrediction(eTarget).Hitchance >= hitC || predOff)
                    {
                        Q.Cast(eTarget, packets(), true);
                        return;
                    }
                }
            }
            else if (useQ && Q.IsReady() && Player.Distance(eTarget.Position) <= Q.Range &&
                     ShouldQ(eTarget, source))
            {
                if (Q.GetPrediction(eTarget).Hitchance >= hitC || predOff)
                {
                    Q.Cast(eTarget, packets(), true);
                    return;
                }
            }

            //R
            if (useR && R.IsReady() && Player.Distance(eTarget.Position) < R.Range)
            {
                if (E.IsReady())
                {
                    if (CheckReq(rETarget))
                        E.Cast(rETarget, packets());
                }
                if (ShouldR(eTarget) && R.IsReady())
                {
                    R.Cast(Game.CursorPos, packets());
                    _rTimer = Environment.TickCount - 250;
                }
                if (_rTimeLeft > 9500 && _rOn && R.IsReady())
                {
                    R.Cast(Game.CursorPos, packets());
                    _rTimer = Environment.TickCount - 250;
                }
            }
        }

        private void CheckKs()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(1300) && x.IsEnemy && !x.IsDead).OrderByDescending(GetComboDamage))
            {
                if (target != null)
                {
                    if (Player.Distance(target.ServerPosition) <= W.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1) +
                         Player.GetSpellDamage(target, SpellSlot.W)) > target.Health && Q.IsReady() && Q.IsReady())
                    {
                        Q.Cast(target, packets());
                        return;
                    }

                    if (Player.Distance(target.ServerPosition) <= Q.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1)) >
                        target.Health && Q.IsReady())
                    {
                        Q.Cast(target, packets());
                        return;
                    }

                    if (Player.Distance(target.ServerPosition) <= E.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.E)) > target.Health & E.IsReady())
                    {
                        E.Cast(target, packets());
                        return;
                    }

                    if (Player.Distance(target.ServerPosition) <= W.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.W)) > target.Health && W.IsReady())
                    {
                        W.Cast();
                        return;
                    }

                    Vector3 dashVector = Player.Position +
                                         Vector3.Normalize(target.ServerPosition - Player.Position) * 425;
                    if (Player.Distance(target.ServerPosition) <= R.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.R)) > target.Health && R.IsReady() && _rOn &&
                        target.Distance(dashVector) < 425 && R.IsReady())
                    {
                        R.Cast(dashVector, packets());
                    }
                }
            }
        }

        private bool ShouldQ(Obj_AI_Hero target, string source)
        {
            if (source == "Combo")
            {
                if ((Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1)) >
                    target.Health)
                    return true;

                if (_rOn)
                    return true;

                if (!menu.Item("charmCombo", true).GetValue<KeyBind>().Active)
                    return true;

                if (target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Taunt))
                    return true;
            }

            if (source == "Harass")
            {
                if ((Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1)) >
                    target.Health)
                    return true;

                if (_rOn)
                    return true;

                if (!menu.Item("charmHarass", true).GetValue<bool>())
                    return true;

                if (target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Taunt))
                    return true;
            }

            return false;
        }

        private bool ShouldW(Obj_AI_Hero target, string source)
        {
            if (source == "Combo")
            {
                if (Player.GetSpellDamage(target, SpellSlot.W) > target.Health)
                    return true;

                if (_rOn)
                    return true;

                if (!menu.Item("charmCombo", true).GetValue<KeyBind>().Active)
                    return true;

                if (target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Taunt))
                    return true;
            }
            if (source == "Harass")
            {
                if (Player.GetSpellDamage(target, SpellSlot.W) > target.Health)
                    return true;

                if (_rOn)
                    return true;

                if (!menu.Item("charmHarass", true).GetValue<bool>())
                    return true;

                if (target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Taunt))
                    return true;
            }

            return false;
        }

        private bool ShouldR(Obj_AI_Hero target)
        {
            if (!ManaCheck())
                return false;

            Vector3 dashVector = Player.Position + Vector3.Normalize(Game.CursorPos - Player.Position) * 425;
            if (Player.Distance(Game.CursorPos) < 75 && target.Distance(dashVector) > 525)
                return false;

            if (menu.Item("rSpeed", true).GetValue<bool>() && countEnemiesNearPosition(Game.CursorPos, 1500) < 2 && GetComboDamage(target) > target.Health - 100)
                return true;

            if (GetComboDamage(target) > target.Health && !_rOn)
            {
                if (target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Taunt))
                    return true;
            }

            if (target.HasBuffOfType(BuffType.Charm) && _rOn || target.HasBuffOfType(BuffType.Taunt))
                return true;

            if (countAlliesNearPosition(Game.CursorPos, 1000) > 2 && _rTimeLeft > 3500)
                return true;

            if (Player.GetSpellDamage(target, SpellSlot.R) * 2 > target.Health)
                return true;

            if (_rOn && _rTimeLeft > 9500)
                return true;

            return false;
        }

        private bool CheckReq(Obj_AI_Hero target)
        {
            if (Player.Distance(Game.CursorPos) < 75)
                return false;

            if (GetComboDamage(target) > target.Health && !_rOn && countEnemiesNearPosition(Game.CursorPos, 1500) < 3)
            {
                if (target.Distance(Game.CursorPos) <= E.Range && E.IsReady())
                {
                    Vector3 dashVector = Player.Position + Vector3.Normalize(Game.CursorPos - Player.Position) * 425;
                    float addedDelay = Player.Distance(dashVector) / 2200;

                    //Game.PrintChat("added delay: " + addedDelay);

                    PredictionOutput pred = GetP(Game.CursorPos, E, target, addedDelay, false);
                    if (pred.Hitchance >= HitChance.Medium && R.IsReady())
                    {
                        //Game.PrintChat("R-E Mode Intiate!");
                        R.Cast(Game.CursorPos, packets());
                        _rTimer = Environment.TickCount - 250;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsRActive()
        {
            return Player.HasBuffIn("AhriTumble", 0f, true);
        }

        private int RCount()
        {
            var buff = Player.Buffs.FirstOrDefault(x => x.Name == "AhriTumble");
            if (buff != null)
                return buff.Count;
            return 0;
        }

        private void Farm()
        {
            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range,
                MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();

            if (useQ && Q.IsReady())
            {
                MinionManager.FarmLocation qPos = Q.GetLineFarmLocation(allMinionsQ);
                if (qPos.MinionsHit >= 3)
                {
                    Q.Cast(qPos.Position, packets());
                }
            }

            if (useW && allMinionsW.Count > 0 && W.IsReady())
                W.Cast();
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            _rOn = IsRActive();

            if (_rOn)
                _rTimeLeft = Environment.TickCount - _rTimer;

            //ks check
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

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();
            }
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            foreach (Spell spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range", true).GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }

            if (menu.Item("cursor", true).GetValue<Circle>().Active)
                Render.Circle.DrawCircle(Player.Position, 475, Color.Aquamarine);
            if (menu.Item("Draw_Mode", true).GetValue<Circle>().Active)
            {
                var wts = Drawing.WorldToScreen(Player.Position);

                Drawing.DrawText(wts[0], wts[1], Color.White,
                    menu.Item("charmCombo", true).GetValue<KeyBind>().Active ? "Require E: On" : "Require E: Off");
            }
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap", true).GetValue<bool>()) return;

            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
                E.Cast(gapcloser.Sender);
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!menu.Item("UseInt", true).GetValue<bool>()) return;

            if (Player.Distance(unit.Position) < E.Range && unit != null)
            {
                if (E.GetPrediction(unit).Hitchance >= HitChance.Medium && E.IsReady())
                    E.Cast(unit, packets());
            }
        }
    }
}
