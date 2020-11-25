using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;

namespace xSaliceResurrected.ADC
{
    class Urgot : Champion
    {
        public Urgot ()
        {
            SetSpells();
            LoadMenu();
        }

        private void SetSpells()
        {
            SpellManager.Q = new Spell(SpellSlot.Q, 1000);
            SpellManager.Q.SetSkillshot(0.2667f, 60f, 1600f, true, SkillshotType.SkillshotLine);

            SpellManager.Q2 = new Spell(SpellSlot.Q, 1300);
            SpellManager.Q2.SetSkillshot(0.3f, 60f, 1800f, false, SkillshotType.SkillshotLine);

            SpellManager.W = new Spell(SpellSlot.W);

            SpellManager.E = new Spell(SpellSlot.E, 850);
            SpellManager.E.SetSkillshot(0.2658f, 120f, 1500f, false, SkillshotType.SkillshotCircle);

            SpellManager.R = new Spell(SpellSlot.R, 550);
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

            var spellMenu = new Menu("SpellMenu", "SpellMenu");
            {
                var qMenu = new Menu("QMenu", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("Q_Poison", "Auto Q Poison Target", true).SetValue(true));
                    qMenu.AddItem(new MenuItem("ForceE", "Require to use E first if Enemy is in E range", true).SetValue(false));
                    spellMenu.AddSubMenu(qMenu);
                }

                var wMenu = new Menu("WMenu", "WMenu");
                {
                    wMenu.AddItem(new MenuItem("W_If_HP", "W If HP <= ", true).SetValue(new Slider(50)));
                    wMenu.AddItem(new MenuItem("W_Always", "Always W At start Of Combo", true).SetValue(false));
                    spellMenu.AddSubMenu(wMenu);
                }

                var rMenu = new Menu("RMenu", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("R_Safe_Net", "Do not ult into >= enemies after swap", true).SetValue(new Slider(2, 0, 5)));
                    rMenu.AddItem(new MenuItem("R_If_UnderTurret", "Ult Enemy If they are under ally Turret", true).SetValue(true));
                    rMenu.AddItem(new MenuItem("R_On_Killable", "Ult Enemy If they are Killable", true).SetValue(true));
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
                combo.AddItem(new MenuItem("UseECombo", "Use E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R", true).SetValue(true));
                combo.AddSubMenu(HitChanceManager.AddHitChanceMenuCombo(true, false, true, false));
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                harass.AddSubMenu(HitChanceManager.AddHitChanceMenuHarass(true, false, true, false));
                ManaManager.AddManaManagertoMenu(harass, "Harass", 50);
                menu.AddSubMenu(harass);
            }

            var farm = new Menu("LaneClear", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "Use E", true).SetValue(false));
                ManaManager.AddManaManagertoMenu(farm, "LaneClear", 50);
                menu.AddSubMenu(farm);
            }

            var miscMenu = new Menu("Misc", "Misc");
            {
                //aoe
                miscMenu.AddSubMenu(AoeSpellManager.AddHitChanceMenuCombo(false, false, true, false));
                miscMenu.AddItem(new MenuItem("smartKS", "Smart KS", true).SetValue(true));

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
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q) * 2;

            if (E.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.E);

            comboDamage = ItemManager.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target) * 3);
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
            if (source == "Harass" && !ManaManager.HasMana("Harass"))
                return;

            var target = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);
            if (!target.IsValidTarget(Q2.Range))
                return;

            //items
            if (source == "Combo")
            {
                var dmg = GetComboDamage(target);
                ItemManager.Target = target;

                //see if killable
                if (dmg > target.Health - 50)
                    ItemManager.KillableTarget = true;

                ItemManager.UseTargetted = true;
            }

            if (useR && R.IsReady())
                Cast_R();
            if (useW && W.IsReady())
                Cast_W(target);
            if (useE && E.IsReady())
                SpellCastManager.CastBasicSkillShot(E, E.Range, TargetSelector.DamageType.Physical, HitChanceManager.GetEHitChance(source));
            if (useQ && Q.IsReady())
            {
                if (Player.ServerPosition.Distance(target.ServerPosition) > E.Range || !E.IsReady() || !menu.Item("ForceE", true).GetValue<bool>())
                    Cast_Q(target, source);
            }
        }

        private readonly List<Obj_AI_Hero> _poisonTargets = new List<Obj_AI_Hero>();

        protected override void ObjAiBaseOnOnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if (sender.IsMe || !(sender is Obj_AI_Hero) || !sender.IsEnemy)
                return;

            if (args.Buff.Name == "urgotcorrosivedebuff")
            {
                _poisonTargets.Add((Obj_AI_Hero) sender);
                Console.WriteLine("Added: " + _poisonTargets.Count);
            }
        }

        protected override void ObjAiBaseOnOnBuffRemove(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args)
        {
            if (sender.IsMe || !(sender is Obj_AI_Hero) || !sender.IsEnemy)
                return;
            if (args.Buff.Name == "urgotcorrosivedebuff")
            {
                _poisonTargets.RemoveAll(x => x.NetworkId == sender.NetworkId);
                Console.WriteLine("Added: " + _poisonTargets.Count);
            }
        }

        protected override void AfterAttack(AttackableUnit unit, AttackableUnit mytarget)
        {
            var target = (Obj_AI_Base)mytarget;

            if (!menu.Item("ComboActive", true).GetValue<KeyBind>().Active || !unit.IsMe || !(target is Obj_AI_Hero))
                return;

            if (menu.Item("UseWCombo", true).GetValue<bool>())
                W.Cast();
            if (menu.Item("UseECombo", true).GetValue<bool>() && E.IsReady())
                E.Cast(target);
            if (menu.Item("UseQCombo", true).GetValue<bool>() && Q.IsReady())
                    Q.Cast(target);
        }

        private void Farm()
        {
            if (!ManaManager.HasMana("LaneClear"))
                return;

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            var minion = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (useQ && minion.Count > 0)
                Q.Cast(minion[0]);
            if (useE)
            {
                var allMinionECount = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);
                var pred = E.GetCircularFarmLocation(allMinionECount);
                if (pred.MinionsHit > 1)
                    E.Cast(pred.Position);
            }
        }

        private void Cast_R()
        {
            if (R.Instance.Level == 1)
                R.Range = 550;
            else if (R.Instance.Level == 2)
                R.Range = 700;
            else if (R.Instance.Level == 3)
                R.Range = 850;

            var safeNet = menu.Item("R_Safe_Net", true).GetValue<Slider>().Value;

            foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range)).OrderByDescending(GetComboDamage))
            {
                if (menu.Item("Dont_R" + target.CharData.BaseSkinName, true) != null)
                {
                    if (!menu.Item("Dont_R" + target.CharData.BaseSkinName, true).GetValue<bool>())
                    {
                        if (!(target.CountEnemiesInRange(1000) >= safeNet))
                        {
                            //if killable
                            if (menu.Item("R_On_Killable", true).GetValue<bool>())
                            {
                                if (GetComboDamage(target) > target.Health && Player.Distance(target.Position) < R.Range)
                                {
                                    R.Cast(target);
                                    return;
                                }
                            }

                            //if player is under turret
                            if (menu.Item("R_If_UnderTurret", true).GetValue<bool>())
                            {
                                if (Util.UnderAllyTurret() && Player.ServerPosition.Distance(target.ServerPosition) > 300f)
                                {
                                    R.Cast(target);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Cast_W(Obj_AI_Hero target)
        {
            if (menu.Item("W_Always", true).GetValue<bool>() && Player.ServerPosition.Distance(target.ServerPosition) < Q.Range)
                W.Cast();

            if (target.HasBuffIn("urgotcorrosivedebuff", 0.00f, true))
                W.Cast();

            var hp = menu.Item("W_If_HP", true).GetValue<Slider>().Value;

            if (Player.HealthPercent <= hp)
                W.Cast();
        }

        private void Cast_Q(Obj_AI_Hero target, string source)
        {
            if (target.HasBuffIn("urgotcorrosivedebuff", 0.00f, true))
                Q2.Cast(target);
            else 
                SpellCastManager.CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Physical, HitChanceManager.GetQHitChance(source));
        }

        private void CheckKs()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Q2.Range)).OrderByDescending(GetComboDamage))
            {
                //Q
                if (Player.ServerPosition.Distance(target.ServerPosition) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && Q.IsReady())
                {
                    Q.Cast(target);
                    return;
                }

                //R
                if (Player.ServerPosition.Distance(target.ServerPosition) <= E.Range && Player.GetSpellDamage(target, SpellSlot.E) > target.Health && E.IsReady())
                {
                    E.Cast(target);
                    return;
                }
            }
        }


        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            if (Player.IsChannelingImportantSpell())
                return;

            if (menu.Item("Q_Poison", true).GetValue<bool>() && _poisonTargets.Count > 0)
            {
                var target = _poisonTargets.OrderByDescending(GetComboDamage).FirstOrDefault();

                if (target.IsValidTarget(Q2.Range))
                    Q2.Cast(target);
            }

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
            if (menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            if (menu.Item("Draw_Q", true).GetValue<bool>())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_W", true).GetValue<bool>())
                if (W.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position,R.Range, R.IsReady() ? Color.Green : Color.Red);
        }
    }
}
