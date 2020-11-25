using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;

namespace xSaliceResurrected.ADC
{
    class KogMaw : Champion
    {
        public KogMaw()
        {
            SetSpells();
            LoadMenu();
        }

        private void SetSpells()
        {
            SpellManager.Q = new Spell(SpellSlot.Q, 1000);
            SpellManager.Q.SetSkillshot(.25f, 70, 1650, true, SkillshotType.SkillshotLine);

            SpellManager.W = new Spell(SpellSlot.W);

            SpellManager.E = new Spell(SpellSlot.E, 1280);
            SpellManager.E.SetSkillshot(.25f, 70, 1650, false, SkillshotType.SkillshotLine);

            SpellManager.R = new Spell(SpellSlot.R, 1800);
            SpellManager.R.SetSkillshot(.9f, 150, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        private void LoadMenu()
        {
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "Use Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "Use W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R", true).SetValue(true));
                combo.AddItem(new MenuItem("NoMovementDuringW", "Dont Move During W").SetValue(true));
                combo.AddItem(new MenuItem("ComboR_Limit", "Limit R Stack", true).SetValue(new Slider(7, 0, 7)));
                combo.AddSubMenu(HitChanceManager.AddHitChanceMenuCombo(true, false, true, true));
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(false));
                harass.AddItem(new MenuItem("UseRHarass", "Use R", true).SetValue(true));
                harass.AddItem(new MenuItem("HarassR_Limit", "Limit R Stack", true).SetValue(new Slider(3, 0, 7)));
                harass.AddSubMenu(HitChanceManager.AddHitChanceMenuHarass(true, false, true, true));
                ManaManager.AddManaManagertoMenu(harass, "Harass", 50);
                menu.AddSubMenu(harass);
            }

            var farm = new Menu("LaneClear", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "Use E", true).SetValue(false));
                farm.AddItem(new MenuItem("UseRFarm", "Use R", true).SetValue(true));
                farm.AddItem(new MenuItem("LaneClearR_Limit", "Limit R Stack", true).SetValue(new Slider(2, 0, 7)));
                ManaManager.AddManaManagertoMenu(farm, "LaneClear", 50);
                menu.AddSubMenu(farm);
            }

            var miscMenu = new Menu("Misc", "Misc");
            {
                //aoe
                miscMenu.AddSubMenu(AoeSpellManager.AddHitChanceMenuCombo(false, false, true, true));
                miscMenu.AddItem(new MenuItem("smartKS", "Smart KS", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("UseGap", "Use E for GapCloser", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(miscMenu);
            }

            var drawMenu = new Menu("Drawing", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "Disable All", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "Draw Q", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_W", "Draw W", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "Draw E", true).SetValue(true));
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
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R);

            comboDamage = ItemManager.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target) * 2);
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(), menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(), menu.Item("UseEHarass", true).GetValue<bool>(), menu.Item("UseRHarass", true).GetValue<bool>(), "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, string source)
        {
            if (source == "Harass" && !ManaManager.HasMana("Harass"))
                return;

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

            if (useR && R.IsReady())
                Cast_R(source);
            if (useQ && Q.IsReady())
                SpellCastManager.CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Magical, HitChanceManager.GetQHitChance(source));
            if (useE && E.IsReady())
                SpellCastManager.CastBasicSkillShot(E, E.Range, TargetSelector.DamageType.Physical, HitChanceManager.GetEHitChance(source));
            if (useW && W.IsReady())
            {
                var target = TargetSelector.GetTarget(Player.AttackRange + new[] { 130 , 150 , 170 , 190 , 210 }[W.Level - 1], TargetSelector.DamageType.Magical);
                if (target.IsValidTarget(Player.AttackRange + new[] { 90, 120, 150, 180, 210 }[W.Level - 1]))
                    W.Cast();
            }
        }

        private void Farm()
        {
            if (!ManaManager.HasMana("LaneClear"))
                return;

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();
            var useR = menu.Item("UseRFarm", true).GetValue<bool>();

            if (useQ)
            {
                var minion = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All,
                    MinionTeam.NotAlly);

                if (minion.Count > 0)
                    Q.Cast(minion[0]);
            }
            if (useR)
                Cast_R("Farm");
            if (useE)
            {
                SpellCastManager.CastBasicFarm(E);
            }
        }

        private void Cast_R(string mode)
        {
            if (mode == "Combo" && RStacks <= menu.Item("ComboR_Limit", true).GetValue<Slider>().Value)
                SpellCastManager.CastBasicSkillShot(R, new[] { 1200f, 1500f, 1800f }[R.Level - 1], TargetSelector.DamageType.Magical, HitChanceManager.GetRHitChance(mode));
            else if (mode == "Harass" && RStacks <= menu.Item("HarassR_Limit", true).GetValue<Slider>().Value)
                SpellCastManager.CastBasicSkillShot(R, new[] { 1200f, 1500f, 1800f }[R.Level - 1], TargetSelector.DamageType.Magical, HitChanceManager.GetRHitChance(mode));
            else if (mode == "Farm" && RStacks <= menu.Item("LaneClearR_Limit", true).GetValue<Slider>().Value)
                SpellCastManager.CastBasicFarm(R);
        }

        private int RStacks
        {
            get { return RSpell.Ammo/40; }
        }

        private void CheckKs()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(R.Range)).OrderByDescending(GetComboDamage))
            {
                //Q
                if (Player.ServerPosition.Distance(target.ServerPosition) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && Q.IsReady())
                {
                    Q.Cast(target);
                    return;
                }

                //E
                if (Player.ServerPosition.Distance(target.ServerPosition) <= E.Range && Player.GetSpellDamage(target, SpellSlot.E) > target.Health && E.IsReady())
                {
                    E.Cast(target);
                    return;
                }

                //R
                if (Player.ServerPosition.Distance(target.ServerPosition) <= R.Range && Player.GetSpellDamage(target, SpellSlot.R) > target.Health && R.IsReady())
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

            //no aa canceling option
            Orbwalker.SetMovement(menu.Item("NoMovementDuringW").GetValue<bool>() && Player.HasBuff("KogMawBioArcaneBarrage") && ObjectManager.Get<Obj_AI_Hero>().Count(h=>h.IsEnemy && !h.IsDead && h.IsVisible && h.CanAttack && h.Distance(Player) < h.AttackRange) == 0);

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

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap", true).GetValue<bool>()) return;

            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
                E.Cast(gapcloser.Sender);
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
                    Render.Circle.DrawCircle(Player.Position, Player.AttackRange + new[] { 130, 150, 170, 190, 210 }[W.Level - 1], W.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, new[] { 1200f, 1500f, 1800f }[R.Level - 1], R.IsReady() ? Color.Green : Color.Red);
        }

    }
}
