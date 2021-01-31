using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Veigar : Champion
    {
        public Veigar()
        {
            QMana = new []{60, 60, 65, 70, 75, 80};
            WMana = new []{70, 70, 80, 90, 100, 110};
            EMana = new []{80, 80, 90, 100, 110, 120};
            RMana = new []{125, 125, 175, 225};
            LoadSpell();
            LoadMenu();
        }

        private void LoadSpell()
        {
            //intalize spell
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 1005);
            R = new Spell(SpellSlot.R, 650);

            W.SetSkillshot(1.25f, 230f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(.2f, 330f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private void LoadMenu()
        {
            //Keys
            var key = new Menu("Keys", "Keys");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!", true).SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LastHitQQ", "Last hit with Q", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitQQ2", "Last hit with Q(Togg)", true).SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("wPoke", "Cast W Only on Stun", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("escape", "Escape", true).SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                menu.AddSubMenu(key);
            }

            //Combo menu:
            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("selected", "Focus Selected Target", true).SetValue(true));
                combo.AddItem(new MenuItem("UseQCombo", "Use Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "Use W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E", true).SetValue(true));
                combo.AddItem(new MenuItem("waitW", "Wait For W to E", true).SetValue(true));
                combo.AddItem(new MenuItem("castWonTopOfE", "Cast W Immediately After E", true).SetValue(false));
                combo.AddItem(new MenuItem("UseRCombo", "Use R", true).SetValue(true));
                combo.AddItem(new MenuItem("DFGMode", "DFG Mode", true).SetValue(new StringList(new[] {"Combo", "DFG-R"})));
                menu.AddSubMenu(combo);
            }

            //Harass menu:
            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                AddManaManagertoMenu(harass, "Harass", 30);
                menu.AddSubMenu(harass);
            }

            //Misc Menu:
            var misc = new Menu("Misc", "Misc"); { 
                misc.AddItem(new MenuItem("UseInt", "Use R to Interrupt", true).SetValue(true));
                misc.AddItem(new MenuItem("UseGap", "Use W for GapCloser", true).SetValue(true));
                misc.AddItem(new MenuItem("overKill", "Over Kill Check", true).SetValue(true));
                misc.AddItem(new MenuItem("smartKS", "Use Smart KS System", true).SetValue(true));

                misc.AddSubMenu(new Menu("Dont use R on", "DontUlt"));

                foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                    misc
                        .SubMenu("DontUlt")
                        .AddItem(new MenuItem("DontUlt" + enemy.BaseSkinName, enemy.BaseSkinName, true).SetValue(false));

                misc.AddSubMenu(new Menu("Dont use DFG on", "DontDFG"));

                foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                    misc
                        .SubMenu("DontDFG")
                        .AddItem(new MenuItem("DontDFG" + enemy.BaseSkinName, enemy.BaseSkinName, true).SetValue(false));

                menu.AddSubMenu(misc);
            }

            //Drawings menu:
            var drawing = new Menu("Drawings", "Drawings"); { 
               drawing.AddItem(new MenuItem("QRange", "Q range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
               drawing.AddItem(new MenuItem("WRange", "W range", true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
               drawing.AddItem(new MenuItem("ERange", "E range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
               drawing.AddItem(new MenuItem("RRange", "R range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
               drawing.AddItem(new MenuItem("manaStatus", "Mana status", true).SetValue(true));

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
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            double damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            damage = ActiveItems.CalcDamage(enemy, damage);

            if (Items.HasItem(3155, (Obj_AI_Hero)enemy))
            {
                damage = damage - 250;
            }

            if (Items.HasItem(3156, (Obj_AI_Hero)enemy))
            {
                damage = damage - 400;
            }
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
            var range = E.IsReady() ? E.Range : Q.Range;
            var focusSelected = menu.Item("selected", true).GetValue<bool>();
            Obj_AI_Hero target = TargetSelector.GetTarget(range, TargetSelector.DamageType.Magical);
            if (TargetSelector.GetSelectedTarget() != null)
                if (focusSelected && TargetSelector.GetSelectedTarget().Distance(Player.ServerPosition) < range)
                    target = TargetSelector.GetSelectedTarget();

            int dfgMode = menu.Item("DFGMode", true).GetValue<StringList>().SelectedIndex;

            bool hasMana = ManaCheck2();

            float dmg = GetComboDamage(target);
            var waitW = menu.Item("waitW", true).GetValue<bool>();

            if (source == "Harass" && !HasMana("Harass"))
                return;

            if (useW && target != null && Player.Distance(target.Position) <= W.Range && W.IsReady())
            {
                if (menu.Item("wPoke", true).GetValue<KeyBind>().Active)
                {
                    var pred = W.GetPrediction(target);
                    if (pred.Hitchance == HitChance.Immobile || IsStunned(target))
                        W.Cast(target.ServerPosition, packets());
                }
                else
                {
                    PredictionOutput pred = Prediction.GetPrediction(target, 1.25f);
                    if (pred.Hitchance >= HitChance.High && W.IsReady())
                        W.Cast(pred.CastPosition, packets());
                }
            }

            if (useE && target != null && E.IsReady() && Player.Distance(target.Position) < E.Range)
            {
                if (!waitW || W.IsReady())
                {
                    CastE(target);
                    return;
                }
            }

            //DFG
            if (target != null && dfgMode == 0 && source == "Combo" && hasMana)
            {
                ActiveItems.Target = target;

                //see if killable
                if (dmg > target.Health - 50)
                {
                    ActiveItems.KillableTarget = true;
                }
                if ((menu.Item("DontDFG" + target.BaseSkinName, true) != null &&
                     menu.Item("DontDFG" + target.BaseSkinName, true).GetValue<bool>() == false))
                {
                    ActiveItems.UseTargetted = true;
                }
            }

            //Q
            if (useQ && Q.IsReady() && Player.Distance(target.Position) <= Q.Range && target != null)
            {
                Q.CastOnUnit(target, packets());
            }

            //R
            if (target != null && R.IsReady())
            {
                useR = rTarget(target) && useR;
                if (useR)
                {
                    CastR(target, dmg);
                }
            }
        }

        private void SmartKs()
        {
            if (!menu.Item("smartKS", true).GetValue<bool>())
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.IsValidTarget(700)))
            {
                if (target != null)
                {
                    //Q
                    if (Player.Distance(target.ServerPosition) <= Q.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 30)
                    {
                        if (Q.IsReady())
                        {
                            Q.CastOnUnit(target, packets());
                            return;
                        }
                    }

                    //R
                    if (Player.Distance(target.ServerPosition) <= R.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.R)) > target.Health + 50)
                    {
                        if (R.IsReady() && rTarget(target))
                        {
                            R.CastOnUnit(target, packets());
                            return;
                        }
                    }
                }
            }
        }

        private bool rTarget(Obj_AI_Hero target)
        {
            if ((menu.Item("DontUlt" + target.BaseSkinName, true) != null &&
                 menu.Item("DontUlt" + target.BaseSkinName, true).GetValue<bool>() == false))
                return true;
            return false;
        }

        private void CastE(Obj_AI_Hero target)
        {
            PredictionOutput pred = Prediction.GetPrediction(target, E.Delay);
            Vector2 castVec = pred.UnitPosition.To2D() -
                              Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;

            if (pred.Hitchance >= HitChance.High && E.IsReady())
            {
                E.Cast(castVec);

                if (menu.Item("castWonTopOfE", true).GetValue<bool>())
                    Utility.DelayAction.Add(50, () => W.Cast(pred.UnitPosition));
                    
            }
        }

        private void CastR(Obj_AI_Hero target, float dmg)
        {
            if (menu.Item("overKill", true).GetValue<bool>() && Player.GetSpellDamage(target, SpellSlot.Q) > target.Health)
                return;

            if (Player.Distance(target.Position) > R.Range)
                return;

            //DFG + R
            if (Dfg.IsReady() && R.IsReady() && Player.Distance(target.ServerPosition) < R.Range &&
                Player.GetItemDamage(target, Damage.DamageItems.Dfg) +
                (Player.GetSpellDamage(target, SpellSlot.R) * 1.2) > target.Health + 50)
            {
                Use_DFG(target);
                R.CastOnUnit(target, packets());
                return;
            }

            if (dmg > target.Health + 20 && R.IsReady())
            {
                R.CastOnUnit(target, packets());
            }
        }

        private void LastHit()
        {
            if (!Orbwalking.CanMove(40)) return;
            if (!xSLxOrbwalker.CanMove()) return;

            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

            if (Q.IsReady())
            {
                foreach (Obj_AI_Base minion in allMinions)
                {
                    if (minion.IsValidTarget() &&
                        HealthPrediction.GetHealthPrediction(minion, (int)((minion.Distance(Player.Position) / 1500) * 1000 + .25f * 1000), 100) <
                        Player.GetSpellDamage(minion, SpellSlot.Q) - 35)
                    {
                        if (Q.IsReady())
                        {
                            Q.Cast(minion, packets());
                            return;
                        }
                    }
                }
            }
        }

        private Obj_AI_Hero GetNearestEnemy(Obj_AI_Hero unit)
        {
            return ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsValidTarget(E.Range))
                .OrderBy(x => unit.ServerPosition.Distance(x.ServerPosition))
                .FirstOrDefault();
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            SmartKs();

            if (menu.Item("escape", true).GetValue<KeyBind>().Active)
            {
                if (E.IsReady())
                    CastE(GetNearestEnemy(Player));
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
            else if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("LastHitQQ", true).GetValue<KeyBind>().Active)
                {
                    LastHit();
                }

                if (menu.Item("LastHitQQ2", true).GetValue<KeyBind>().Active)
                {
                    LastHit();
                }

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

            if (menu.Item("manaStatus", true).GetValue<bool>())
            {
                Vector2 wts = Drawing.WorldToScreen(Player.Position);

                Drawing.DrawText(wts[0] - 30, wts[1], Color.White, ManaCheck2() ? "Mana Rdy" : "No Mana Full Combo");
            }
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap", true).GetValue<bool>()) return;

            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
                CastE((Obj_AI_Hero)gapcloser.Sender);
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!menu.Item("UseInt", true).GetValue<bool>()) return;

            if (Player.Distance(unit.Position) < E.Range && unit != null && E.IsReady())
            {
                CastE((Obj_AI_Hero)unit);
            }
        }
    }
}
