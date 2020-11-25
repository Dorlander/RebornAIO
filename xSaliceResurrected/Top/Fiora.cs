using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;
using Color = System.Drawing.Color;

namespace xSaliceResurrected.Top
{
    class Fiora : Champion
    {
        public Fiora()
        {
            SetSpells();
            LoadMenu();
        }

        private void SetSpells()
        {
            SpellManager.Q = new Spell(SpellSlot.Q, 600);
            SpellManager.Q.SetTargetted(.25f, float.MaxValue);

            SpellManager.W = new Spell(SpellSlot.W);

            SpellManager.E = new Spell(SpellSlot.E);

            SpellManager.R = new Spell(SpellSlot.R, 400);
        }

        private void LoadMenu()
        {
            //key
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitKey", "Last Hit!", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("Combo_Switch", "Switch mode Key", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("SpellMenu", "SpellMenu");
            {
                var qMenu = new Menu("QMenu", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("Q_Min_Distance", "Min range to Q", true).SetValue(new Slider(300, 0, 600)));
                    qMenu.AddItem(new MenuItem("Q_Gap_Close", "Q Minion to Gap Close", true).SetValue(true));
                    spellMenu.AddSubMenu(qMenu);
                }
                var wMenu = new Menu("WMenu", "WMenu");
                {
                    wMenu.AddItem(new MenuItem("W_Incoming", "W Block incoming Atk Always", true).SetValue(true));
                    wMenu.AddItem(new MenuItem("W_Tower", "Don't W block under Tower", true).SetValue(true));
                    wMenu.AddItem(new MenuItem("W_minion", "W Block Minion", true).SetValue(false));
                    spellMenu.AddSubMenu(wMenu);
                }
                var eMenu = new Menu("EMenu", "EMenu");
                {
                    eMenu.AddItem(new MenuItem("E_Reset", "E Auto-Attack Reset", true).SetValue(true));
                    spellMenu.AddSubMenu(eMenu);
                }

                var rMenu = new Menu("RMenu", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("R_If_HP", "R If HP <=", true).SetValue(new Slider(20)));

                    //evading spells
                    var dangerous = new Menu("Dodge Dangerous", "Dodge Dangerous");
                    {
                        foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
                        {
                            dangerous.AddSubMenu(new Menu(hero.ChampionName, hero.ChampionName));
                            dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.Q).Name + "R_Dodge", hero.Spellbook.GetSpell(SpellSlot.Q).Name, true).SetValue(false));
                            dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.W).Name + "R_Dodge", hero.Spellbook.GetSpell(SpellSlot.W).Name, true).SetValue(false));
                            dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.E).Name + "R_Dodge", hero.Spellbook.GetSpell(SpellSlot.E).Name, true).SetValue(false));
                            dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.R).Name + "R_Dodge", hero.Spellbook.GetSpell(SpellSlot.R).Name, true).SetValue(false));
                        }
                        rMenu.AddSubMenu(dangerous);
                    }

                    spellMenu.AddSubMenu(rMenu);
                }

                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("Combo_mode", "Combo Mode", true).SetValue(new StringList(new[] { "Normal", "Q-AA-Q-AA-Ult" })));
                combo.AddItem(new MenuItem("UseQCombo", "Use Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "Use W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                ManaManager.AddManaManagertoMenu(harass, "Harass", 30);
                //add to menu
                menu.AddSubMenu(harass);
            }

            var lastHit = new Menu("Lasthit", "Lasthit");
            {
                lastHit.AddItem(new MenuItem("UseQLastHit", "Use Q", true).SetValue(true));
                ManaManager.AddManaManagertoMenu(lastHit, "Lasthit", 30);
                //add to menu
                menu.AddSubMenu(lastHit);
            }

            var farm = new Menu("LaneClear", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseQFarm_Tower", "Do not Q under Tower", true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "Use E", true).SetValue(true));
                ManaManager.AddManaManagertoMenu(farm, "LaneClear", 30);
                //add to menu
                menu.AddSubMenu(farm);
            }
            var misc = new Menu("Misc", "Misc");
            {
                misc.AddItem(new MenuItem("smartKS", "Use Smart KS System", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(misc);
            }
            var drawMenu = new Menu("Drawing", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "Disable All", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "Draw Q", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "Draw E", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "Draw R", true).SetValue(true));
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
                //add to menu
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
                customMenu.AddItem(myCust.AddToMenu("LastHit Active: ", "LastHitKey"));
                menu.AddSubMenu(customMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q) * 2;

            if (W.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.W);

            if (R.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R) / target.CountEnemiesInRange(R.Range);

            comboDamage = ItemManager.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target) * 3);
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useE, bool useR, string source)
        {
            if (source == "Harass" && !ManaManager.HasMana("Harass"))
                return;

            if (useQ)
                Cast_Q();

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

            if (useE && !menu.Item("E_Reset", true).GetValue<bool>())
                E.Cast();

            if (useR)
                Cast_R();

        }

        private void Lasthit()
        {
            if (menu.Item("UseQLastHit", true).GetValue<bool>() && ManaManager.HasMana("Lasthit"))
                Cast_Q_Last_Hit();
        }

        private void Farm()
        {
            if (!ManaManager.HasMana("LaneClear"))
                return;

            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(Player.ServerPosition, Player.AttackRange + Player.BoundingRadius,
                MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ)
                Cast_Q_Last_Hit();

            if (useE && allMinionsE.Count > 0 && E.IsReady())
                E.Cast();

        }

        private void SmartKs()
        {
            if (!menu.Item("smartKS", true).GetValue<bool>())
                return;
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Q.Range) && !x.IsDead && !x.HasBuffOfType(BuffType.Invulnerability)))
            {
                //Q *2
                if (Player.GetSpellDamage(target, SpellSlot.Q) * 2 > target.Health && Player.Distance(target.Position) < Q.Range && Q.IsReady())
                {
                    Q.Cast(target);
                    return;
                }
                //Q
                if (Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && Player.Distance(target.Position) < Q.Range && Q.IsReady())
                {
                    Q.Cast(target);
                    return;
                }
            }
        }

        private void Cast_Q()
        {
            var target = TargetSelector.GetTarget(Q.Range * 2, TargetSelector.DamageType.Physical);

            int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
            if (mode == 0)
            {
                if (Q.IsReady() && target != null)
                {
                    if (Q.IsKillable(target))
                        Q.Cast(target);

                    if (Player.GetSpellDamage(target, SpellSlot.Q) * 2 > target.Health)
                        Q.Cast(target);

                    if (Utils.TickCount - Q.LastCastAttemptT > 3800 && Utils.TickCount - Q.LastCastAttemptT < 4000)
                        Q.Cast(target);

                    var minDistance = menu.Item("Q_Min_Distance", true).GetValue<Slider>().Value;

                    if (Player.Distance(target.Position, true) > Q.RangeSqr && menu.Item("Q_Gap_Close", true).GetValue<bool>())
                    {
                        var allMinionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);

                        if (allMinionQ.Count < 1)
                            return;

                        Obj_AI_Base bestMinion = allMinionQ[0];

                        foreach (var minion in allMinionQ)
                        {
                            if (target.Distance(minion.Position, true) < Q.RangeSqr && Player.Distance(minion.Position, true) < Q.RangeSqr &&
                                target.Distance(minion.Position, true) < target.Distance(Player.Position, true))
                                if (target.Distance(minion.Position, true) < target.Distance(bestMinion.Position, true))
                                    bestMinion = minion;
                        }

                        //check if can Q without activating
                        if (bestMinion != null && Utils.TickCount - Q.LastCastAttemptT > 4000)
                        {
                            if (target.Distance(bestMinion.Position, true) < Q.RangeSqr && Player.Distance(bestMinion.Position, true) < Q.RangeSqr)
                            {
                                Q.Cast(bestMinion);
                                return;
                            }
                        }
                    }

                    if (Player.Distance(target.Position) > minDistance &&
                        Player.Distance(target.Position, true) < Q.RangeSqr)
                    {
                        Q.Cast(target);
                    }
                }
            }
            else if (mode == 1)//Ham mode
            {
                if (target == null)
                    return;

                if (Q.IsReady() && Utils.TickCount - Q.LastCastAttemptT > 4000 && Player.Distance(target.Position) < Q.Range && Player.Distance(target.Position) > Player.AttackRange)
                    Q.Cast(target);
            }
        }

        private void Cast_Q_Last_Hit()
        {
            var allMinionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Player.BoundingRadius, MinionTypes.All, MinionTeam.NotAlly);

            if (allMinionQ.Count > 0 && Q.IsReady())
            {

                foreach (var minion in allMinionQ)
                {
                    double dmg = Player.GetSpellDamage(minion, SpellSlot.Q);

                    if (dmg > minion.Health + 35)
                    {
                        if (menu.Item("UseQFarm_Tower", true).GetValue<bool>())
                        {
                            if (!minion.UnderTurret(true))
                            {
                                Q.Cast(minion);
                                return;
                            }
                        }
                        else
                            Q.Cast(minion);
                    }
                }
            }
        }

        private void Cast_R()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            if (target != null && R.IsReady())
            {
                if (Player.GetSpellDamage(target, SpellSlot.R) /
                    target.CountEnemiesInRange(R.Range) >
                    target.Health - Player.GetAutoAttackDamage(target) * 2)
                    R.Cast(target);

                var rHpValue = menu.Item("R_If_HP", true).GetValue<Slider>().Value;
                if (Player.HealthPercent <= rHpValue)
                    R.Cast(target);
            }

        }

        private int _lasttick;
        private void ModeSwitch()
        {
            int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
            int lasttime = Utils.TickCount - _lasttick;

            if (menu.Item("Combo_Switch", true).GetValue<KeyBind>().Active && lasttime > Game.Ping)
            {
                if (mode == 0)
                {
                    menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "Normal", "Q-AA-Q-AA-Ult" }, 1));
                    _lasttick = Utils.TickCount + 300;
                }
                else if (mode == 1)
                {
                    menu.Item("Combo_mode", true).SetValue(new StringList(new[] { "Normal", "Q-AA-Q-AA-Ult" }));
                    _lasttick = Utils.TickCount + 300;
                }
            }
        }

        private Obj_AI_Base _dodgeHero;

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            SmartKs();

            ModeSwitch();

            if (Utils.TickCount - R.LastCastAttemptT < 750 && _dodgeHero != null)
                R.Cast(_dodgeHero);

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("LastHitKey", true).GetValue<KeyBind>().Active)
                    Lasthit();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();
            }
        }

        protected override void AfterAttack(AttackableUnit unit, AttackableUnit mytarget)
        {
            var target = (Obj_AI_Base)mytarget;
            if (unit.IsMe)
            {
                if ((menu.Item("ComboActive", true).GetValue<KeyBind>().Active || menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    && (target is Obj_AI_Hero))
                {
                    if (menu.Item("E_Reset", true).GetValue<bool>() && E.IsReady())
                    {
                        E.Cast();
                        Utility.DelayAction.Add(250, OrbwalkManager.ResetAutoAttackTimer);
                    }
                    int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
                    if (mode == 1)
                    {
                        Q.Cast(target);

                        if (QSpell.State == SpellState.Cooldown && R.IsReady())
                            R.Cast(target);
                    }
                }
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (unit.IsMe)
            {
                SpellSlot castedSlot = Player.GetSpellSlot(args.SData.Name);

                if (castedSlot == SpellSlot.Q)
                {
                    Q.LastCastAttemptT = Utils.TickCount;
                }
            }

            if (unit.IsMe)
                return;

            if (OrbwalkManager.IsAutoAttack(args.SData.Name) && args.Target.IsMe && Player.Distance(args.End) < 450)
            {
                if (menu.Item("W_Incoming", true).GetValue<bool>() ||
                    (menu.Item("ComboActive", true).GetValue<KeyBind>().Active && E.IsReady() && menu.Item("UseWCombo", true).GetValue<bool>()) ||
                    (menu.Item("HarassActive", true).GetValue<KeyBind>().Active && menu.Item("UseWHarass", true).GetValue<bool>()))
                {
                    if (!menu.Item("W_minion", true).GetValue<bool>() && !(unit is Obj_AI_Hero))
                        return;

                    if (menu.Item("W_Tower", true).GetValue<bool>() && Player.UnderTurret(true))
                        return;

                    W.Cast();
                }
            }

            if (unit.IsEnemy && (unit is Obj_AI_Hero) && menu.Item(args.SData.Name + "R_Dodge", true) != null)
            {
                if (Player.Distance(args.End) > R.Range || !R.IsReady())
                    return;

                if (menu.Item(args.SData.Name + "R_Dodge", true).GetValue<bool>() && args.SData.Name == "SyndraR")
                {
                    Utility.DelayAction.Add(150, () => R.Cast(unit));
                    return;
                }

                if (menu.Item(args.SData.Name + "R_Dodge", true).GetValue<bool>())
                {
                    //Game.PrintChat("RAWR");
                    R.Cast(unit);
                    _dodgeHero = unit;
                    R.LastCastAttemptT = Utils.TickCount;
                }
            }
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            if (menu.Item("Draw_Q", true).GetValue<bool>())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R_Killable", true).GetValue<bool>() && R.IsReady())
            {
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(5000) && !x.IsDead && x.IsEnemy).OrderBy(x => x.Health))
                {
                    Vector2 wts = Drawing.WorldToScreen(target.Position);
                    if (Player.GetSpellDamage(target, SpellSlot.R) / target.CountEnemiesInRange(R.Range) > target.Health)
                    {
                        Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "KILL!!!");

                    }
                }
            }

            Vector2 wts2 = Drawing.WorldToScreen(Player.Position);
            int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
            if (mode == 0)
                Drawing.DrawText(wts2[0] - 20, wts2[1], Color.White, "Normal");
            else if (mode == 1)
                Drawing.DrawText(wts2[0] - 20, wts2[1], Color.White, "Q-AA-Q-AA-Ult");
        }
    }
}
