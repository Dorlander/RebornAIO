using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;
using Color = System.Drawing.Color;

namespace xSaliceResurrected.Mid
{
    class Ahri : Champion
    {
        public Ahri()
        {
            SetUpSpells();
            LoadMenu();
        }

        private void SetUpSpells()
        {
            //intalize spell
            SpellManager.Q = new Spell(SpellSlot.Q, 900);
            SpellManager.W = new Spell(SpellSlot.W, 650);
            SpellManager.E = new Spell(SpellSlot.E, 900);
            SpellManager.R = new Spell(SpellSlot.R, 850);

            SpellManager.Q.SetSkillshot(0.25f, 100, 1600, false, SkillshotType.SkillshotLine);
            SpellManager.E.SetSkillshot(0.25f, 60, 1550, true, SkillshotType.SkillshotLine);

            SpellManager.SpellList.Add(Q);
            SpellManager.SpellList.Add(W);
            SpellManager.SpellList.Add(E);
            SpellManager.SpellList.Add(R);
        }

        //Load Menu
        private void LoadMenu()
        {
            //key
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("charmCombo", "Q if Charmed in Combo", true).SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Toggle)));
                //add to menu
                menu.AddSubMenu(key);
            }

            //Combo menu:
            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "Use Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "Use W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R", true).SetValue(true));
                combo.AddItem(new MenuItem("rSpeed", "Use All R fast Duel", true).SetValue(true));
                //hitchance
                combo.AddSubMenu(HitChanceManager.AddHitChanceMenuCombo(true, false, true, false));
                //add to menu
                menu.AddSubMenu(combo);
            }
            //Harass menu:
            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                harass.AddItem(new MenuItem("longQ", "Cast Long range Q", true).SetValue(true));
                //hitchance
                harass.AddSubMenu(HitChanceManager.AddHitChanceMenuHarass(true, false, true, false));
                //mana
                ManaManager.AddManaManagertoMenu(harass, "Harass", 60);
                //add to menu
                menu.AddSubMenu(harass);
            }
            //Farming menu:
            var farm = new Menu("Farm", "Farm");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(false));
                farm.AddItem(new MenuItem("UseWFarm", "Use W", true).SetValue(false));
                //mana
                ManaManager.AddManaManagertoMenu(farm, "Farm", 50);
                //add to menu
                menu.AddSubMenu(farm);
            }

            //Misc Menu:
            var misc = new Menu("Misc", "Misc");
            {
                //aoe
                misc.AddSubMenu(AoeSpellManager.AddHitChanceMenuCombo(true, false, false, false));
                misc.AddItem(new MenuItem("UseInt", "Use E to Interrupt", true).SetValue(true));
                misc.AddItem(new MenuItem("UseGap", "Use E for GapCloser", true).SetValue(true));
                misc.AddItem(new MenuItem("EQ", "Use Q onTop of E", true).SetValue(true));
                misc.AddItem(new MenuItem("smartKS", "Smart KS", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(misc);
            }

            //Drawings menu:
            var drawing = new Menu("Drawings", "Drawings");
            {
                drawing.AddItem(
                        new MenuItem("QRange", "Q range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(
                        new MenuItem("WRange", "W range", true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(
                        new MenuItem("ERange", "E range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(
                        new MenuItem("RRange", "R range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(
                        new MenuItem("cursor", "Draw R Dash Range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));

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
                menu.AddSubMenu(drawing);
            }

            var customMenu = new Menu("Custom Perma Show", "Custom Perma Show");
            {
                var myCust = new CustomPermaMenu();
                customMenu.AddItem(new MenuItem("custMenu", "Move Menu", true).SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Press)));
                customMenu.AddItem(new MenuItem("enableCustMenu", "Enabled", true).SetValue(true));
                customMenu.AddItem(myCust.AddToMenu("Combo Active: ", "ComboActive"));
                customMenu.AddItem(myCust.AddToMenu("Harass Active: ", "HarassActive"));
                customMenu.AddItem(myCust.AddToMenu("Harass(T) Active: ", "HarassActiveT"));
                customMenu.AddItem(myCust.AddToMenu("Laneclear Active: ", "LaneClearActive"));
                customMenu.AddItem(myCust.AddToMenu("Require Charm: ", "charmCombo"));
                menu.AddSubMenu(customMenu);
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
            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (_rOn)
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) * RCount();
            else if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) * 3;

            damage = ItemManager.CalcDamage(enemy, damage);

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
            if (source == "Harass" && !ManaManager.HasMana(source))
                return;

            Obj_AI_Hero eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            Obj_AI_Hero rETarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            var dmg = GetComboDamage(eTarget);

            if (eTarget == null)
                return;

            if (source == "Combo")
            {
                //items-------
                ItemManager.Target = eTarget;

                //see if killable
                if (dmg > eTarget.Health - 50)
                    ItemManager.KillableTarget = true;

                //Items
                ItemManager.UseTargetted = true;
            }
            //end items-------

            //E
            if (useE && E.IsReady() && Player.Distance(eTarget.Position) < E.Range)
            {
                SpellCastManager.CastBasicSkillShot(E, E.Range, TargetSelector.DamageType.Magical, HitChanceManager.GetEHitChance(source));
                if (menu.Item("EQ", true).GetValue<bool>() && Q.IsReady() && !E.IsReady())
                {
                    SpellCastManager.CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Magical, HitChanceManager.GetQHitChance(source));
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
                    SpellCastManager.CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Magical, HitChanceManager.GetQHitChance(source));
                }
            }
            else if (useQ && Q.IsReady() && Player.Distance(eTarget.Position) <= Q.Range &&
                     ShouldQ(eTarget, source))
            {
                SpellCastManager.CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Magical, HitChanceManager.GetQHitChance(source));
            }

            //R
            if (useR && R.IsReady() && Player.Distance(eTarget.Position) < R.Range)
            {
                if (E.IsReady())
                {
                    if (CheckReq(rETarget))
                        SpellCastManager.CastBasicSkillShot(E, E.Range, TargetSelector.DamageType.Magical, HitChanceManager.GetEHitChance(source));
                }
                if (ShouldR(eTarget, dmg) && R.IsReady())
                {
                    R.Cast(Game.CursorPos);
                    _rTimer = Utils.TickCount - 250;
                }
                if (_rTimeLeft > 9500 && _rOn && R.IsReady())
                {
                    R.Cast(Game.CursorPos);
                    _rTimer = Utils.TickCount - 250;
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
                        Q.Cast(target);
                        return;
                    }

                    if (Player.Distance(target.ServerPosition) <= Q.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1)) >
                        target.Health && Q.IsReady())
                    {
                        Q.Cast(target);
                        return;
                    }

                    if (Player.Distance(target.ServerPosition) <= E.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.E)) > target.Health & E.IsReady())
                    {
                        E.Cast(target);
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
                        R.Cast(dashVector);
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
                return true;
            

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

                if (Player.Mana > ESpell.ManaCost + QSpell.ManaCost)
                    return true;

                if (!menu.Item("charmCombo", true).GetValue<KeyBind>().Active)
                    return true;

                if (target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Taunt))
                    return true;
            }
            if (source == "Harass")
                return true;
           
            return false;
        }

        private bool ShouldR(Obj_AI_Hero target, float dmg)
        {
            Vector3 dashVector = Player.ServerPosition + Vector3.Normalize(Game.CursorPos - Player.ServerPosition) * 425;

            if (Player.Distance(Game.CursorPos) < 475)
                dashVector = Game.CursorPos;

            if (target.Distance(dashVector) > 525)
                return false;

            if (menu.Item("rSpeed", true).GetValue<bool>() && Game.CursorPos.CountEnemiesInRange(1500) < 3 && dmg > target.Health - 100)
                return true;

            if (Player.GetSpellDamage(target, SpellSlot.R) * RCount() > target.Health)
                return true;

            if (_rOn && _rTimeLeft > 9500)
                return true;

            return false;
        }

        private bool CheckReq(Obj_AI_Hero target)
        {
            if (Player.Distance(Game.CursorPos) < 75)
                return false;

            if (GetComboDamage(target) > target.Health && !_rOn && Game.CursorPos.CountEnemiesInRange(1500) < 3)
            {
                if (target.Distance(Game.CursorPos) <= E.Range && E.IsReady())
                {
                    Vector3 dashVector = Player.Position + Vector3.Normalize(Game.CursorPos - Player.Position) * 425;
                    float addedDelay = Player.Distance(dashVector) / 2200;

                    //Game.PrintChat("added delay: " + addedDelay);

                    PredictionOutput pred = Util.GetP(Game.CursorPos, E, target, addedDelay, false);
                    if (pred.Hitchance >= HitChance.Medium && R.IsReady())
                    {
                        //Game.PrintChat("R-E Mode Intiate!");
                        R.Cast(Game.CursorPos);
                        _rTimer = Utils.TickCount - 250;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsRActive()
        {
            return Player.HasBuffIn("AhriTumble", 0.00f, true);
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
            if (!ManaManager.HasMana("Farm"))
                return;

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
                    Q.Cast(qPos.Position);
                }
            }

            if (useW && allMinionsW.Count > 0 && W.IsReady())
                W.Cast();
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            _rOn = IsRActive();

            if (_rOn)
                _rTimeLeft = Utils.TickCount - _rTimer;

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

            if (Player.Distance(unit.Position) < E.Range)
            {
                if (E.GetPrediction(unit).Hitchance >= HitChance.Medium && E.IsReady())
                    E.Cast(unit);
            }
        }
    }
}
