using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Blitzcrank : Champion
    {
        public Blitzcrank()
        {
            LoadSpells();
            LoadMenu();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 140);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.22f, 70f, 1800, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.25f, 600, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private void LoadMenu()
        {
            //Keys
            var key = new Menu("Keys", "Keys"); {
                key.AddItem(new MenuItem("ComboActive", "Combo!",true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!",true).SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!",true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("panic", "Panic Key(no spell)",true).SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                menu.AddSubMenu(key);
            }

            //Q Menu
            var qMenu = new Menu("qMenu", "qMenu");
            {
                qMenu.AddItem(new MenuItem("qRange2", "Q Min Range Slider",true).SetValue(new Slider(300, 1, 950)));
                qMenu.AddItem(new MenuItem("qRange", "Q Max Range Slider",true).SetValue(new Slider(900, 1, 950)));
                qMenu.AddItem(new MenuItem("qSlow", "Auto Q Slow",true).SetValue(true));
                qMenu.AddItem(new MenuItem("qImmobile", "Auto Q Immobile",true).SetValue(true));
                qMenu.AddItem(new MenuItem("qDashing", "Auto Q Dashing",true).SetValue(true));
                menu.AddSubMenu(qMenu);
            }

            //Combo menu:
            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "Use Q",true).SetValue(true));
                combo.AddItem(new MenuItem("qHit", "Q HitChance",true).SetValue(new Slider(3, 1, 4)));
                combo.AddItem(new MenuItem("UseWCombo", "Use W",true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E",true).SetValue(true));
                combo.AddItem(new MenuItem("QE", "Use E on Grab",true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R",true).SetValue(true));
                combo.AddItem(new MenuItem("useRQ", "Use R After Q",true).SetValue(true));
                menu.AddSubMenu(combo);
            }

            //Harass menu:
            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q",true).SetValue(true));
                harass.AddItem(new MenuItem("qHit2", "Q HitChance",true).SetValue(new Slider(3, 1, 4)));
                harass.AddItem(new MenuItem("UseWHarass", "Use W",true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E",true).SetValue(true));
                menu.AddSubMenu(harass);
            }
            //Misc Menu:
            var misc = new Menu("Misc", "Misc");
            {
                misc.AddItem(new MenuItem("UseInt", "Use R to Interrupt",true).SetValue(true));
                misc.AddItem(new MenuItem("resetE", "Use E AA reset Only",true).SetValue(true));
                misc.AddItem(new MenuItem("autoR", "Use R if hit",true).SetValue(new Slider(3, 0, 5)));

                misc.AddSubMenu(new Menu("Don't use Q on", "intR"));

                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                    misc
                        .SubMenu("intR")
                        .AddItem(new MenuItem("intR" + enemy.BaseSkinName, enemy.BaseSkinName,true).SetValue(false));

                menu.AddSubMenu(misc);
            }

            //Drawings menu:
            var drawMenu = new Menu("Drawings", "Drawings"); {
                drawMenu.AddItem(new MenuItem("QRange", "Q range",true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawMenu.AddItem(new MenuItem("WRange", "W range",true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                drawMenu.AddItem(new MenuItem("ERange", "E range",true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawMenu.AddItem(new MenuItem("RRange", "R range",true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "Draw Combo Damage",true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "Draw Combo Damage Fill",true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
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

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);

            damage += Player.GetAutoAttackDamage(enemy);
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
            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);

            var rq = menu.Item("useRQ", true).GetValue<bool>();
            var qe = menu.Item("QE", true).GetValue<bool>();


            if (useW && target != null && W.IsReady() && Player.Distance(target.Position) <= 500)
            {
                W.Cast();
            }

            if (useQ && Q.IsReady() && Player.Distance(target.Position) <= Q.Range && target != null && (Q.GetPrediction(target).Hitchance >= GetHitchance(source) || ShouldUseQ(target)) && UseQonEnemy(target))
            {
                if (qe && useE && E.IsReady())
                    E.Cast();

                Q.Cast(target, packets());
                return;
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

            if (useE && target != null && E.IsReady() && Player.Distance(target.Position) < 300 && !menu.Item("resetE", true).GetValue<bool>())
            {
                E.Cast();
            }

            if (useR && target != null && R.IsReady() && Player.Distance(target.Position) < R.Range)
            {
                if (rq && Q.IsReady())
                    return;

                R.Cast();
            }

        }

        private bool ShouldUseQ(Obj_AI_Hero target)
        {
            var slow = menu.Item("qSlow", true).GetValue<bool>();
            var immobile = menu.Item("qImmobile", true).GetValue<bool>();
            var dashing = menu.Item("qDashing", true).GetValue<bool>();

            if (Q.GetPrediction(target).Hitchance == HitChance.Dashing && dashing)
                return true;

            if (Q.GetPrediction(target).Hitchance == HitChance.Immobile && immobile)
                return true;

            if (target.HasBuffOfType(BuffType.Slow) && slow && Q.GetPrediction(target).Hitchance >= HitChance.High)
                return true;

            return false;
        }

        private void AutoQ()
        {
            var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where champ.IsValidTarget(Q.Range) select champ).ToList();

            foreach (var target in nearChamps)
            {
                if (target != null)
                {
                    if (ShouldUseQ(target) && UseQonEnemy(target) && Q.IsReady())
                        Q.Cast(target.ServerPosition, packets());
                }
            }
        }

        private void CheckRMec()
        {
            int hit = 0;
            var minHit = menu.Item("autoR", true).GetValue<Slider>().Value;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(R.Range)))
            {
                var prediction = GetPCircle(Player.ServerPosition, R, enemy, true);

                if (R.IsReady() && enemy.Distance(Player.ServerPosition) <= R.Width && prediction.CastPosition.Distance(Player.ServerPosition) <= R.Width && prediction.Hitchance >= HitChance.High)
                {
                    hit++;
                }
            }

            if (hit >= minHit && R.IsReady())
                R.Cast();
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            if (menu.Item("panic", true).GetValue<KeyBind>().Active)
            {
                if (W.IsReady())
                    W.Cast();
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                return;
            }

            Q.Range = menu.Item("qRange", true).GetValue<Slider>().Value;

            //Q grab on immobile
            AutoQ();

            //Rmec
            CheckRMec();

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();
            }
        }

        private bool UseQonEnemy(Obj_AI_Hero target)
        {
            if (target.HasBuffOfType(BuffType.SpellImmunity))
            {
                return false;
            }

            var qRangeMin = menu.Item("qRange2", true).GetValue<Slider>().Value;
            if (Player.Distance(target.ServerPosition) < qRangeMin)
                return false;

            if (menu.Item("intR" + target.BaseSkinName, true) != null)
                if (menu.Item("intR" + target.BaseSkinName, true).GetValue<bool>())
                    return false;

            return true;
        }

        protected override void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var useECombo = menu.Item("UseECombo", true).GetValue<bool>();
            var useEHarass = menu.Item("UseEHarass", true).GetValue<bool>();

            if (unit.IsMe && menu.Item("resetE", true).GetValue<bool>())
            {
                if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
                {
                    if (useECombo && E.IsReady())
                    {
                        Orbwalking.ResetAutoAttackTimer();
                        E.Cast();
                    }
                }

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active || menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                {
                    if (useEHarass && E.IsReady())
                    {
                        Orbwalking.ResetAutoAttackTimer();
                        E.Cast();
                    }
                }
            }
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range", true).GetValue<Circle>();
                if (menuItem.Active)
                {
                    if (spell == Q)
                    {
                        var qRange = menu.Item("qRange", true).GetValue<Slider>().Value;

                        Q.Range = qRange;
                    }
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            }

        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!menu.Item("UseInt", true).GetValue<bool>()) return;

            if (Player.Distance(unit.Position) < Q.Range && unit != null && Q.IsReady())
            {
                if (Q.GetPrediction(unit).Hitchance >= HitChance.High)
                    Q.Cast(unit, packets());
            }

            if (Player.Distance(unit.Position) < R.Range && unit != null & R.IsReady())
            {
                R.Cast();
            }
        }
    }
}
