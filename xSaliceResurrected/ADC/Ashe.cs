using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;
using Color = System.Drawing.Color;

namespace xSaliceResurrected.ADC
{
    class Ashe : Champion
    {
        public Ashe()
        {
            SetSpells();
            LoadMenu();
        }

        private void SetSpells()
        {
            SpellManager.Q = new Spell(SpellSlot.Q);

            SpellManager.W = new Spell(SpellSlot.W, 1200);
            SpellManager.W.SetSkillshot(0.25f, 70f, 1500f, true, SkillshotType.SkillshotCone);

            SpellManager.E = new Spell(SpellSlot.E);

            SpellManager.R = new Spell(SpellSlot.R, 20000);
            SpellManager.R.SetSkillshot(250f, 130f, 1600f, false, SkillshotType.SkillshotLine);

        }

        private void LoadMenu()
        {
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("Force_R", "Force R Lowest", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("SpellMenu", "SpellMenu");
            {
                var qMenu = new Menu("QMenu", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("Q_Min_Stack", "Require Q Min Stacks", true).SetValue(new Slider(5, 0, 5)));
                    spellMenu.AddSubMenu(qMenu);
                }

                var rMenu = new Menu("RMenu", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("R_Min_Range", "R Min Range Sliders", true).SetValue(new Slider(300, 0, 1000)));
                    rMenu.AddItem(new MenuItem("R_Max_Range", "R Max Range Sliders", true).SetValue(new Slider(2000, 0, 4000)));

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
                combo.AddItem(new MenuItem("UseWCombo", "Use W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R", true).SetValue(true));
                combo.AddSubMenu(HitChanceManager.AddHitChanceMenuCombo(false, true, false, true));
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(true));
                harass.AddSubMenu(HitChanceManager.AddHitChanceMenuHarass(false, true, false, true));
                ManaManager.AddManaManagertoMenu(harass, "Harass", 30);
                //add to menu
                menu.AddSubMenu(harass);
            }

            var farm = new Menu("LaneClear", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseWFarm", "Use W", true).SetValue(true));
                ManaManager.AddManaManagertoMenu(farm, "LaneClear", 30);
                //add to menu
                menu.AddSubMenu(farm);
            }

            var miscMenu = new Menu("Misc", "Misc");
            {
                //aoe
                miscMenu.AddSubMenu(AoeSpellManager.AddHitChanceMenuCombo(false, true, false, true));
                miscMenu.AddItem(new MenuItem("smartKS", "Smart KS", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("ksR", "KS with R", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("UseInt", "Use R to Interrupt", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(miscMenu);
            }

            var drawMenu = new Menu("Drawing", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "Disable All", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_W", "Draw W", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "Draw R", true).SetValue(true));

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
                customMenu.AddItem(myCust.AddToMenu("Harass(T) Active: ", "HarassActiveT"));
                customMenu.AddItem(myCust.AddToMenu("Laneclear Active: ", "LaneClearActive"));
                customMenu.AddItem(myCust.AddToMenu("Force R: ", "Force_R"));
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

            if (R.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R);

            comboDamage = ItemManager.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target) * 2);
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                false, menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(),
                false, false, "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, string source)
        {
            if (source == "Harass" && !ManaManager.HasMana("Harass"))
                return;

            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            if (target.IsValidTarget(R.Range))
            {
                var dmg = GetComboDamage(target);

                if (useR && dmg > target.Health && Player.ServerPosition.Distance(target.ServerPosition) > menu.Item("R_Min_Range", true).GetValue<Slider>().Value)
                    SpellCastManager.CastBasicSkillShot(R, R.Range, TargetSelector.DamageType.Physical, HitChanceManager.GetRHitChance(source));

                if (Q.IsReady() && Player.ServerPosition.Distance(target.ServerPosition) < 550)
                {
                    var qMin = menu.Item("Q_Min_Stack", true).GetValue<Slider>().Value;

                    if (qMin <= QStacks)
                        Q.Cast();
                }
            }

            if (useW && W.IsReady())
                SpellCastManager.CastBasicSkillShot(W, W.Range, TargetSelector.DamageType.Magical, HitChanceManager.GetWHitChance(source));

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
            
        }

        protected override void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if ((menu.Item("UseQCombo", true).GetValue<bool>() && menu.Item("ComboActive", true).GetValue<KeyBind>().Active) || 
                (menu.Item("HarassActive", true).GetValue<KeyBind>().Active && menu.Item("UseQHarass", true).GetValue<bool>()))
            {
                if (Q.IsReady())
                {
                    var qMin = menu.Item("Q_Min_Stack", true).GetValue<Slider>().Value;

                    if (qMin <= QStacks)
                        Q.Cast();
                }
            }
        }

        private int QStacks
        {
            get
            {
                return
                    (from buff in Player.Buffs
                        where buff.Name == "asheqcastready" || buff.Name == "AsheQ"
                        select buff.Count).FirstOrDefault();
            }
        }

        private void Farm()
        {
            if (!ManaManager.HasMana("LaneClear"))
                return;

            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range,
                MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();

            if (useQ && allMinionsQ.Count > 0)
            {
                var qMin = menu.Item("Q_Min_Stack", true).GetValue<Slider>().Value;

                if (qMin <= QStacks)
                    Q.Cast();
            }

            if(useW)
                SpellCastManager.CastBasicFarm(W);
        }

        private void ForceR()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (target != null)
                R.Cast(target);
        }

        private void CheckKs()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(R.Range)).OrderByDescending(GetComboDamage))
            {
                //W
                if (Player.ServerPosition.Distance(target.ServerPosition) <= W.Range && Player.GetSpellDamage(target, SpellSlot.W) > target.Health && W.IsReady())
                {
                    W.Cast(target);
                    return;
                }

                //R
                if (Player.ServerPosition.Distance(target.ServerPosition) <= R.Range && Player.GetSpellDamage(target, SpellSlot.R) > target.Health && R.IsReady() && menu.Item("ksR", true).GetValue<bool>())
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

            //adjust range
            if(R.IsReady())
                R.Range = menu.Item("R_Max_Range", true).GetValue<Slider>().Value;

            if (menu.Item("smartKS", true).GetValue<bool>())
                CheckKs();

            if (menu.Item("Force_R", true).GetValue<KeyBind>().Active)
            {
                OrbwalkManager.Orbwalk(null, Game.CursorPos);
                ForceR();
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
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            if (menu.Item("Draw_W", true).GetValue<bool>())
                if (W.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!menu.Item("UseInt", true).GetValue<bool>()) return;

            if (Player.Distance(unit.Position) < R.Range && spell.DangerLevel >= Interrupter2.DangerLevel.Medium)
            {
                if (R.GetPrediction(unit).Hitchance >= HitChance.Medium && R.IsReady())
                    R.Cast(unit);
            }
        }
    }
    
}
