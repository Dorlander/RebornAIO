using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Orianna : Champion
    {
        //ball manager
        private bool _isBallMoving;
        private Vector3 _currentBallPosition;
        private Vector3 _allyDraw;
        private int _ballStatus;

        public Orianna()
        {
            SetupSpells();
            LoadMenu();
        }

        private void SetupSpells()
        {
            //intalize spell
            Q = new Spell(SpellSlot.Q, 825);
            W = new Spell(SpellSlot.W, 250);
            E = new Spell(SpellSlot.E, 1095);
            R = new Spell(SpellSlot.R, 370);

            Q.SetSkillshot(0.25f, 80, 1300, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0f, 250, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 145, 1700, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.60f, 370, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private void LoadMenu()
        {
            //Keys
            var key = new Menu("Keys", "Keys"); { 
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitQQ", "Last hit with Q", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("escape", "RUN FOR YOUR LIFE!", true).SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            //Spell Menu
            var spellMenu = new Menu("SpellMenu", "SpellMenu");
            {
                //Q Menu
                var qMenu = new Menu("QSpell", "QSpell");{
                    qMenu.AddItem(new MenuItem("qHit", "Q HitChance Combo", true).SetValue(new Slider(3, 1, 3)));
                    qMenu.AddItem(new MenuItem("qHit2", "Q HitChance Harass", true).SetValue(new Slider(3, 1, 4)));
                    spellMenu.AddSubMenu(qMenu);
                }
                //W
                var wMenu = new Menu("WSpell", "WSpell");
                {
                    wMenu.AddItem(new MenuItem("autoW", "Use W if hit", true).SetValue(new Slider(2, 1, 5)));
                    spellMenu.AddSubMenu(wMenu);
                }
                //E
                var eMenu = new Menu("ESpell", "ESpell");
                {
                    eMenu.AddItem(new MenuItem("UseEDmg", "Use E to Dmg", true).SetValue(true));

                    eMenu.AddSubMenu(new Menu("E Ally Inc Spell", "shield"));
                    eMenu.SubMenu("shield").AddItem(new MenuItem("eAllyIfHP", "If HP < %", true).SetValue(new Slider(40)));
                    foreach (Obj_AI_Hero ally in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly))
                        eMenu.SubMenu("shield").AddItem(new MenuItem("shield" + ally.BaseSkinName, ally.BaseSkinName, true).SetValue(false));

                    spellMenu.AddSubMenu(eMenu);
                }
                //R
                var rMenu = new Menu("RSpell", "RSpell"); {
                    rMenu.AddItem(new MenuItem("autoR", "Use R if hit", true).SetValue(new Slider(3, 1, 5)));
                    rMenu.AddItem(new MenuItem("blockR", "Block R if no enemy", true).SetValue(true));
                    rMenu.AddItem(new MenuItem("overK", "OverKill Check", true).SetValue(true));
                    rMenu.AddItem(new MenuItem("killR", "R Multi Only Toggle", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));

                    rMenu.AddSubMenu(new Menu("Auto use R on", "intR"));
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                        rMenu.SubMenu("intR").AddItem(new MenuItem("intR" + enemy.BaseSkinName, enemy.BaseSkinName, true).SetValue(false));

                    spellMenu.AddSubMenu(rMenu);
                }
                menu.AddSubMenu(spellMenu);
            }

            //Combo menu:
            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("selected", "Focus Selected Target", true).SetValue(true));
                combo.AddItem(new MenuItem("UseQCombo", "Use Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "Use W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R", true).SetValue(true));
                combo.AddItem(new MenuItem("autoRCombo", "Use R if hit", true).SetValue(new Slider(2, 1, 5)));
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
            //Farming menu:
            var farm = new Menu("Farm", "Farm");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(false));
                farm.AddItem(new MenuItem("UseWFarm", "Use W", true).SetValue(false));
                farm.AddItem(new MenuItem("qFarm", "Only Q/W if > minion", true).SetValue(new Slider(3, 0, 5)));
                menu.AddSubMenu(farm);
            }

            //intiator list:
            var initator = new Menu("Initiator", "Initiator");
            {
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
                {
                    foreach (Initiator intiator in Initiator.InitatorList)
                    {
                        if (intiator.HeroName == hero.BaseSkinName)
                        {
                            initator.AddItem(new MenuItem(intiator.SpellName, intiator.SpellName, true)).SetValue(false);
                        }
                    }
                }
                menu.AddSubMenu(initator);
            }

            //Misc Menu:
            var misc = new Menu("Misc", "Misc");
            {
                misc.AddItem(new MenuItem("UseInt", "Use R to Interrupt", true).SetValue(true));
            }

            //Damage after combo:
            MenuItem dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo", true).SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

            //Drawings menu:
            var drawing = new Menu("Drawings", "Drawings"); { 
                drawing.AddItem(new MenuItem("QRange", "Q range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(new MenuItem("WRange", "W range", true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(new MenuItem("ERange", "E range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(new MenuItem("RRange", "R range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(new MenuItem("rModeDraw", "R mode", true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                drawing.AddItem(dmgAfterComboItem);
                menu.AddSubMenu(drawing);
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            double damage = 0d;

            //if (Q.IsReady())
            damage += Player.GetSpellDamage(enemy, SpellSlot.Q) * 1.5;

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) - 25;

            damage = ActiveItems.CalcDamage(enemy, damage);

            return (float)damage;
        }

        private void Combo()
        {
            //Orbwalker.SetAttacks(!(Q.IsReady()));
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");
        }
        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, String source)
        {
            if (source == "Harass" && !HasMana("Harass"))
                return;

            var range = E.IsReady() ? E.Range : Q.Range;
            Obj_AI_Hero target = TargetSelector.GetTarget(range, TargetSelector.DamageType.Magical);

            if (GetTargetFocus(range) != null)
                target = GetTargetFocus(range);

            if (useQ && Q.IsReady())
            {
                CastQ(target, source);
            }

            if (_isBallMoving)
                return;

            if (useW && target != null && W.IsReady())
            {
                CastW(target);
            }

            //items
            if (source == "Combo")
            {
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
            }

            if (useE && target != null && E.IsReady())
            {
                CastE(target);
            }

            if (useR && target != null && R.IsReady())
            {
                if (menu.Item("intR" + target.BaseSkinName, true) != null)
                {
                    foreach (
                        Obj_AI_Hero enemy in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(x => Player.Distance(x.Position) < 1500 && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
                    {
                        if (enemy != null && !enemy.IsDead && menu.Item("intR" + enemy.BaseSkinName, true).GetValue<bool>())
                        {
                            CastR(enemy);
                            return;
                        }
                    }
                }

                if (!(menu.Item("killR", true).GetValue<KeyBind>().Active)) //check if multi
                {
                    if (menu.Item("overK", true).GetValue<bool>() &&
                        (Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.W)) >= target.Health)
                    {
                        return;
                    }
                    if (GetComboDamage(target) >= target.Health - 100 && !target.IsZombie)
                        CastR(target);
                }
            }
        }

        private void CastW(Obj_AI_Base target)
        {
            if (_isBallMoving) return;

            PredictionOutput prediction = GetPCircle(_currentBallPosition, W, target, true);

            if (W.IsReady() && prediction.UnitPosition.Distance(_currentBallPosition) < W.Width)
            {
                W.Cast();
            }

        }

        private void CastR(Obj_AI_Base target)
        {
            if (_isBallMoving) return;

            PredictionOutput prediction = GetPCircle(_currentBallPosition, R, target, true);

            if (R.IsReady() && prediction.UnitPosition.Distance(_currentBallPosition) <= R.Width)
            {
                R.Cast();
            }
        }

        private void CastE(Obj_AI_Base target)
        {
            if (_isBallMoving) return;

            Obj_AI_Hero etarget = Player;

            switch (_ballStatus)
            {
                case 0:
                    if (target != null)
                    {
                        float travelTime = target.Distance(Player.ServerPosition) / Q.Speed;
                        float minTravelTime = 10000f;

                        foreach (
                            Obj_AI_Hero ally in
                                ObjectManager.Get<Obj_AI_Hero>()
                                    .Where(x => x.IsAlly && Player.Distance(x.ServerPosition) <= E.Range && !x.IsMe))
                        {
                            if (ally != null)
                            {
                                //dmg enemy with E
                                if (menu.Item("UseEDmg", true).GetValue<bool>())
                                {
                                    PredictionOutput prediction3 = GetP(Player.ServerPosition, E, target, true);
                                    Object[] obj = VectorPointProjectionOnLineSegment(Player.ServerPosition.To2D(),
                                        ally.ServerPosition.To2D(), prediction3.UnitPosition.To2D());
                                    var isOnseg = (bool)obj[2];
                                    var pointLine = (Vector2)obj[1];

                                    if (E.IsReady() && isOnseg &&
                                        prediction3.UnitPosition.Distance(pointLine.To3D()) < E.Width)
                                    {
                                        //Game.PrintChat("Dmg 1");
                                        E.CastOnUnit(ally, packets());
                                        return;
                                    }
                                }

                                float allyRange = target.Distance(ally.ServerPosition) / Q.Speed +
                                                  ally.Distance(Player.ServerPosition) / E.Speed;
                                if (allyRange < minTravelTime)
                                {
                                    etarget = ally;
                                    minTravelTime = allyRange;
                                }
                            }
                        }

                        if (minTravelTime < travelTime && Player.Distance(etarget.ServerPosition) <= E.Range &&
                            E.IsReady())
                        {
                            E.CastOnUnit(etarget, packets());
                        }
                    }
                    break;
                case 1:
                    //dmg enemy with E
                    if (menu.Item("UseEDmg", true).GetValue<bool>())
                    {
                        PredictionOutput prediction = GetP(_currentBallPosition, E, target, true);
                        Object[] obj = VectorPointProjectionOnLineSegment(_currentBallPosition.To2D(),
                            Player.ServerPosition.To2D(), prediction.UnitPosition.To2D());
                        var isOnseg = (bool)obj[2];
                        var pointLine = (Vector2)obj[1];

                        if (E.IsReady() && isOnseg && prediction.UnitPosition.Distance(pointLine.To3D()) < E.Width)
                        {
                            //Game.PrintChat("Dmg 2");
                            E.CastOnUnit(Player, packets());
                            return;
                        }
                    }

                    float travelTime2 = target.Distance(_currentBallPosition) / Q.Speed;
                    float minTravelTime2 = target.Distance(Player.ServerPosition) / Q.Speed +
                                            Player.Distance(_currentBallPosition) / E.Speed;

                    if (minTravelTime2 < travelTime2 && target.Distance(Player.ServerPosition) <= Q.Range + Q.Width &&
                        E.IsReady())
                    {
                        E.CastOnUnit(Player, packets());
                    }

                    break;
                case 2:
                    float travelTime3 = target.Distance(_currentBallPosition) / Q.Speed;
                    float minTravelTime3 = 10000f;

                    foreach (
                        Obj_AI_Hero ally in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(x => x.IsAlly && Player.Distance(x.ServerPosition) <= E.Range && !x.IsMe))
                    {
                        if (ally != null)
                        {
                            //dmg enemy with E
                            if (menu.Item("UseEDmg", true).GetValue<bool>())
                            {
                                PredictionOutput prediction2 = GetP(_currentBallPosition, E, target, true);
                                Object[] obj = VectorPointProjectionOnLineSegment(_currentBallPosition.To2D(),
                                    ally.ServerPosition.To2D(), prediction2.UnitPosition.To2D());
                                var isOnseg = (bool)obj[2];
                                var pointLine = (Vector2)obj[1];

                                if (E.IsReady() && isOnseg &&
                                    prediction2.UnitPosition.Distance(pointLine.To3D()) < E.Width)
                                {
                                    //Game.PrintChat("Dmg 3");
                                    E.CastOnUnit(ally, packets());
                                    return;
                                }
                            }

                            float allyRange2 = target.Distance(ally.ServerPosition) / Q.Speed +
                                                ally.Distance(_currentBallPosition) / E.Speed;

                            if (allyRange2 < minTravelTime3)
                            {
                                etarget = ally;
                                minTravelTime3 = allyRange2;
                            }
                        }
                    }

                    if (minTravelTime3 < travelTime3 && Player.Distance(etarget.ServerPosition) <= E.Range &&
                        E.IsReady())
                    {
                        E.CastOnUnit(etarget, packets());
                    }

                    break;
            }
        }

        private void CastQ(Obj_AI_Base target, String source)
        {
            if (_isBallMoving) return;

            PredictionOutput prediction = GetP(_currentBallPosition, Q, target, true);

            if (Q.IsReady() && prediction.Hitchance >= GetHitchance(source) && Player.Distance(target.Position) <= Q.Range + Q.Width)
            {
                Q.Cast(prediction.CastPosition, packets());
            }
        }

        private void CheckWMec()
        {
            if (!W.IsReady() || _isBallMoving)
                return;

            int minHit = menu.Item("autoW", true).GetValue<Slider>().Value;

            int hit = (from x in ObjectManager.Get<Obj_AI_Hero>().Where(champ => champ.IsValidTarget(1500) && champ.IsVisible && !champ.IsZombie) 
                       where x != null select GetPCircle(_currentBallPosition, W, x, true)).Count(prediction => W.IsReady() && prediction.UnitPosition.Distance(_currentBallPosition) < W.Width);

            if (hit >= minHit && W.IsReady())
                W.Cast();
        }

        private void CheckRMec()
        {
            if (!R.IsReady() || _isBallMoving)
                return;

            int minHit = menu.Item("autoRCombo", true).GetValue<Slider>().Value;

            int hit = (from x in ObjectManager.Get<Obj_AI_Hero>().Where(champ => champ.IsValidTarget(1500) && champ.IsVisible && !champ.IsZombie)
                       where x != null
                       select GetPCircle(_currentBallPosition, R, x, true)).Count(prediction => R.IsReady() && prediction.UnitPosition.Distance(_currentBallPosition) < R.Width);

            if (hit >= minHit && R.IsReady())
                R.Cast();
        }

        private void CheckRMecGlobal()
        {
            if (!R.IsReady() || _isBallMoving)
                return;

            int minHit = menu.Item("autoR", true).GetValue<Slider>().Value;

            int hit = (from x in ObjectManager.Get<Obj_AI_Hero>().Where(champ => champ.IsValidTarget(1500) && champ.IsVisible && !champ.IsZombie)
                       where x != null
                       select GetPCircle(_currentBallPosition, R, x, true)).Count(prediction => R.IsReady() && prediction.UnitPosition.Distance(_currentBallPosition) < R.Width);


            if (hit >= minHit && R.IsReady())
                R.Cast();
        }

        private int CountR()
        {
            if (!R.IsReady())
                return 0;

            return (from enemy in ObjectManager.Get<Obj_AI_Hero>().Where(champ => champ.IsValidTarget(1500) && champ.IsVisible && !champ.IsZombie) 
                    where enemy != null select GetPCircle(_currentBallPosition, R, enemy, true)).Count(prediction => R.IsReady() && prediction.UnitPosition.Distance(_currentBallPosition) <= R.Width);
        }

        private void LastHit()
        {
            if (!Orbwalking.CanMove(40)) return;

            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

            if (Q.IsReady())
            {
                foreach (Obj_AI_Base minion in allMinions)
                {
                    if (minion.IsValidTarget() &&
                        HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion.Position) * 1000 / 1400)) <
                        Player.GetSpellDamage(minion, SpellSlot.Q) - 10)
                    {
                        PredictionOutput prediction = GetP(_currentBallPosition, Q, minion, true);

                        if (prediction.Hitchance >= HitChance.High && Q.IsReady())
                            Q.Cast(prediction.CastPosition, packets());
                    }
                }
            }
        }

        private void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;

            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                Q.Range + Q.Width);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                Q.Range + Q.Width);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();
            int min = menu.Item("qFarm", true).GetValue<Slider>().Value;

            if (useQ && Q.IsReady())
            {
                Q.From = _currentBallPosition;

                MinionManager.FarmLocation pred = Q.GetCircularFarmLocation(allMinionsQ, Q.Width + 15);

                if (pred.MinionsHit >= min)
                    Q.Cast(pred.Position, packets());
            }

            int hit = 0;
            if (useW && W.IsReady())
            {
                hit += allMinionsW.Count(enemy => enemy.Distance(_currentBallPosition) < W.Range);

                if (hit >= min && W.IsReady())
                    W.Cast();
            }
        }

        private void Escape()
        {
            if (_ballStatus == 0 && W.IsReady())
                W.Cast();
            else if (E.IsReady() && _ballStatus != 0)
                E.CastOnUnit(Player, packets());
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            OnGainBuff();

            CheckRMecGlobal();

            if (menu.Item("escape", true).GetValue<KeyBind>().Active)
            {
                Escape();
            }
            else if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                CheckRMec();
                Combo();
            }
            else
            {
                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active ||
                    menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                {
                    Farm();
                }

                if (menu.Item("LastHitQQ", true).GetValue<KeyBind>().Active)
                {
                    LastHit();
                }
            }

            CheckWMec();
        }

        private void OnGainBuff()
        {
            if (Player.HasBuff("OrianaGhostSelf"))
            {
                _ballStatus = 0;
                _currentBallPosition = Player.ServerPosition;
                _isBallMoving = false;
                return;
            }

            foreach (Obj_AI_Hero ally in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(ally => ally.IsAlly && !ally.IsDead && ally.HasBuffIn("orianaghost", 0f, true)))
            {
                _ballStatus = 2;
                _currentBallPosition = ally.ServerPosition;
                _allyDraw = ally.Position;
                _isBallMoving = false;
                return;
            }

            _ballStatus = 1;
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            foreach (Spell spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range", true).GetValue<Circle>();
                if ((spell.Slot == SpellSlot.R && menuItem.Active) || (spell.Slot == SpellSlot.W && menuItem.Active))
                {
                    if (_ballStatus == 0)
                        Render.Circle.DrawCircle(Player.Position, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
                    else if (_ballStatus == 2)
                        Render.Circle.DrawCircle(_allyDraw, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
                    else
                        Render.Circle.DrawCircle(_currentBallPosition, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
                }
                else if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
            }
            if (menu.Item("rModeDraw", true).GetValue<Circle>().Active)
            {
                if (menu.Item("killR", true).GetValue<KeyBind>().Active)
                {
                    Vector2 wts = Drawing.WorldToScreen(Player.Position);
                    Drawing.DrawText(wts[0], wts[1], Color.White, "R Multi On");
                }
                else
                {
                    Vector2 wts = Drawing.WorldToScreen(Player.Position);
                    Drawing.DrawText(wts[0], wts[1], Color.Red, "R Multi Off");
                }
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            //Shield Ally
            if (unit.IsEnemy && unit.Type == GameObjectType.obj_AI_Hero && E.IsReady())
            {
                foreach (
                    Obj_AI_Hero ally in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(x => Player.Distance(x.Position) < E.Range && Player.Distance(unit.Position) < 1500 && x.IsAlly && !x.IsDead).OrderBy(x => x.Distance(args.End)))
                {
                    if (menu.Item("shield" + ally.BaseSkinName, true) != null)
                    {
                        if (menu.Item("shield" + ally.BaseSkinName, true).GetValue<bool>())
                        {
                            int hp = menu.Item("eAllyIfHP", true).GetValue<Slider>().Value;
                            float hpPercent = ally.Health / ally.MaxHealth * 100;

                            if (ally.Distance(args.End) < 500 && hpPercent <= hp)
                            {
                                //Game.PrintChat("shielding");
                                E.CastOnUnit(ally, packets());
                                _isBallMoving = true;
                                return;
                            }
                        }
                    }
                }
            }

            //intiator
            if (unit.IsAlly)
            {
                if (Initiator.InitatorList.Where(spell => args.SData.Name == spell.SDataName).Where(spell => menu.Item(spell.SpellName, true).GetValue<bool>()).Any(spell => E.IsReady() && Player.Distance(unit.Position) < E.Range))
                {
                    E.CastOnUnit(unit, packets());
                    _isBallMoving = true;
                    return;
                }
            }

            if (!unit.IsMe) return;

            SpellSlot castedSlot = ObjectManager.Player.GetSpellSlot(args.SData.Name);

            if (castedSlot == SpellSlot.Q)
            {
                _isBallMoving = true;
                Utility.DelayAction.Add(
                    (int)Math.Max(1, 1000 * (args.End.Distance(_currentBallPosition) - Game.Ping - 0.1) / Q.Speed), () =>
                    {
                        _currentBallPosition = args.End;
                        _ballStatus = 1;
                        _isBallMoving = false;
                        //Game.PrintChat("Stopped");
                    });
            }
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!menu.Item("UseInt", true).GetValue<bool>()) return;

            if (Player.Distance(unit.Position) < R.Range && unit != null)
            {
                CastR(unit);
            }
            else
            {
                CastQ(unit, "Combo");
            }
        }

    /*    protected override void Game_OnSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.C2S.Cast.Header)
            {
                Packet.C2S.Cast.Struct decodedPacket = Packet.C2S.Cast.Decoded(args.PacketData);
                if (decodedPacket.Slot == SpellSlot.R)
                {
                    if (CountR() == 0 && menu.Item("blockR", true).GetValue<bool>())
                    {
                        //Block packet if enemies hit is 0
                        args.Process = false;
                    }
                }
            }
        }*/
    }
}
