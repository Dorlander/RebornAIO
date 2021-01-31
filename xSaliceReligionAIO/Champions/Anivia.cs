using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Anivia : Champion
    {
        public Anivia()
        {
            //mana
            QMana = new[] {80, 80, 90, 100, 110, 120};
            WMana = new[] {70, 70, 70, 70, 70, 70};
            EMana = new[] {80, 50, 60, 70, 80, 90};
            RMana = new[] {75, 75, 75, 75, 75};

            LoadSpells();
            LoadMenu();
        }

        //Spell Obj
        //Q
        private GameObject _qMissle;

        //E
        private bool _eCasted;

        //R
        private GameObject _rObj;
        private bool _rFirstCreated;
        private bool _rByMe;

        private void LoadSpells()
        {
            //intalize spell
            Q = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 625);

            Q.SetSkillshot(.5f, 110f, 750f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(.25f, 1f, float.MaxValue, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(.25f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private void LoadMenu()
        {
            //key
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!",true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!",true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!",true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!",true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("snipe", "W/Q Snipe",true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("escape", "RUN FOR YOUR LIFE!",true).SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            //spell menu
            var spellMenu = new Menu("SpellMenu", "SpellMenu");
            {
                var qMenu = new Menu("QMenu", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("qHit", "Combo HitChance",true).SetValue(new Slider(3, 1, 3)));
                    qMenu.AddItem(new MenuItem("qHit2", "Harass HitChance",true).SetValue(new Slider(3, 1, 4)));
                    qMenu.AddItem(new MenuItem("detonateQ", "Auto Detonate Q",true).SetValue(true));
                    qMenu.AddItem(new MenuItem("detonateQ2", "Pop Q Behind Enemy",true).SetValue(true));
                    spellMenu.AddSubMenu(qMenu);
                }

                var wMenu = new Menu("WMenu", "WMenu");
                {
                    wMenu.AddItem(new MenuItem("wallKill", "Wall Enemy on killable",true).SetValue(true));
                    spellMenu.AddSubMenu(wMenu);
                }

                var rMenu = new Menu("RMenu", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("checkR", "Auto turn off R",true).SetValue(true));
                    spellMenu.AddSubMenu(rMenu);
                }

                menu.AddSubMenu(spellMenu);
            }

            //Combo menu:
            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("selected", "Focus Selected Target",true).SetValue(true));
                combo.AddItem(new MenuItem("UseQCombo", "Use Q",true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "Use W",true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E",true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R",true).SetValue(true));
                menu.AddSubMenu(combo);
            }

            //Harass menu:
            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q",true).SetValue(false));
                harass.AddItem(new MenuItem("UseWHarass", "Use W",true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E",true).SetValue(true));
                harass.AddItem(new MenuItem("UseRHarass", "Use R",true).SetValue(true));
                menu.AddSubMenu(harass);
            }

            //Farming menu:
            var farm = new Menu("Farm", "Farm");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q",true).SetValue(false));
                farm.AddItem(new MenuItem("UseEFarm", "Use E",true).SetValue(false));
                farm.AddItem(new MenuItem("UseRFarm", "Use R",true).SetValue(false));
                menu.AddSubMenu(farm);
            }

            //Misc Menu:
            var misc = new Menu("Misc", "Misc");
            {
                misc.AddItem(new MenuItem("UseInt", "Use Spells to Interrupt",true).SetValue(true));
                misc.AddItem(new MenuItem("UseGap", "Use W for GapCloser",true).SetValue(true));
                misc.AddItem(new MenuItem("smartKS", "Use Smart KS System",true).SetValue(true));
                menu.AddSubMenu(misc);
            }

            //draw
            //Drawings menu:
            var draw = new Menu("Drawings", "Drawings"); { 
                draw.AddItem(new MenuItem("QRange", "Q range",true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                draw.AddItem(new MenuItem("WRange", "W range",true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                draw.AddItem(new MenuItem("ERange", "E range",true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                draw.AddItem(new MenuItem("RRange", "R range",true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "Draw Combo Damage",true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "Draw Combo Damage Fill",true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
                draw.AddItem(drawComboDamageMenu);
                draw.AddItem(drawFill);
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
                menu.AddSubMenu(draw);
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            if (enemy == null)
                return 0;

            double damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (E.IsReady() & (Q.IsReady() || R.IsReady()))
                damage += Player.GetSpellDamage(enemy, SpellSlot.E) * 2;
            else if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) * 3;

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
                menu.Item("UseEHarass", true).GetValue<bool>(), menu.Item("UseRHarass", true).GetValue<bool>(), "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, string source)
        {
            var range = Q.IsReady() ? Q.Range : W.Range;
            var focusSelected = menu.Item("selected", true).GetValue<bool>();
            Obj_AI_Hero target = TargetSelector.GetTarget(range, TargetSelector.DamageType.Magical);
            if (TargetSelector.GetSelectedTarget() != null)
                if (focusSelected && TargetSelector.GetSelectedTarget().Distance(Player.ServerPosition) < range)
                    target = TargetSelector.GetSelectedTarget();

            if (target == null)
                return;

            float dmg = GetComboDamage(target);

            if (useE && E.IsReady() && Player.Distance(target.Position) < E.Range && ShouldE(target))
            {
                E.CastOnUnit(target, packets());
            }

            //Q
            if (useQ && Q.IsReady() && Player.Distance(Q.GetPrediction(target).CastPosition) <= Q.Range &&
                Q.GetPrediction(target).Hitchance >= GetHitchance(source) && ShouldQ())
            {
                Q.Cast(Q.GetPrediction(target).CastPosition);
            }

            //items
            if (source == "Combo")
            {
                var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                if (itemTarget != null)
                {
                    ActiveItems.Target = itemTarget;

                    //see if killable
                    if (dmg > itemTarget.Health - 50)
                        ActiveItems.KillableTarget = true;

                    ActiveItems.UseTargetted = true;
                }
            }

            if (useW && W.IsReady() && Player.Distance(target.Position) <= W.Range && ShouldUseW(target))
            {
                CastW(target);
            }

            if (useR && R.IsReady() && Player.Distance(target.Position) < R.Range &&
                R.GetPrediction(target).Hitchance >= HitChance.High)
            {
                if (ShouldR(source))
                    R.Cast(target);
            }
        }

        private void SmartKs()
        {
            if (!menu.Item("smartKS", true).GetValue<bool>())
                return;

            foreach ( Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(1300)))
            {
                //ER
                if (Player.Distance(target.ServerPosition) <= R.Range && !_rFirstCreated &&
                    (Player.GetSpellDamage(target, SpellSlot.R) + Player.GetSpellDamage(target, SpellSlot.E) * 2) >
                    target.Health + 50)
                {
                    if (R.IsReady() && E.IsReady())
                    {
                        E.CastOnUnit(target, packets());
                        R.CastOnUnit(target, packets());
                        return;
                    }
                }

                //QR
                if (Player.Distance(target.ServerPosition) <= R.Range && ShouldQ() &&
                    (Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.R)) >
                    target.Health + 30)
                {
                    if (W.IsReady() && R.IsReady())
                    {
                        W.Cast(target, packets());
                        return;
                    }
                }

                //Q
                if (Player.Distance(target.ServerPosition) <= Q.Range && ShouldQ() &&
                    (Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 30)
                {
                    if (Q.IsReady())
                    {
                        Q.Cast(target);
                        return;
                    }
                }

                //E
                if (Player.Distance(target.ServerPosition) <= E.Range &&
                    (Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 30)
                {
                    if (E.IsReady())
                    {
                        E.CastOnUnit(target, packets());
                        return;
                    }
                }
            }
        }

        private bool ShouldQ()
        {
            if (Environment.TickCount - Q.LastCastAttemptT > 2000)
                return true;

            return false;
        }

        private bool ShouldR(string source)
        {
            if (_rFirstCreated)
            {
                //Game.PrintChat("Bleh");
                return false;
            }
            if (_rByMe)
            {
                Game.PrintChat("Bleh2");
                return false;
            }

            if (_eCasted)
                return true;

            if (source == "Combo")
                return true;

            return false;
        }

        private bool ShouldE(Obj_AI_Hero target)
        {
            if (checkChilled(target))
                return true;

            if (Player.GetSpellDamage(target, SpellSlot.E) > target.Health)
                return true;

            if (R.IsReady() && Player.Distance(target.Position) <= R.Range - 25 && Player.Distance(target.ServerPosition) > 250)
                return true;

            return false;
        }

        private bool ShouldUseW(Obj_AI_Hero target)
        {
            if (GetComboDamage(target) >= target.Health - 20 && menu.Item("wallKill", true).GetValue<bool>())
                return true;

            if (_rFirstCreated && _rObj != null)
            {
                if (_rObj.Position.Distance(target.ServerPosition) > 300)
                {
                    return true;
                }
            }

            return false;
        }

        private void CastW(Obj_AI_Hero target)
        {
            PredictionOutput pred = W.GetPrediction(target);
            var vec = new Vector3(pred.CastPosition.X - Player.ServerPosition.X, 0,
                pred.CastPosition.Z - Player.ServerPosition.Z);
            Vector3 castBehind = pred.CastPosition + Vector3.Normalize(vec) * 125;

            if (W.IsReady())
                W.Cast(castBehind, packets());
        }

        /*private void castWBetween()
        {
            var enemy = (from champ in ObjectManager.Get<Obj_AI_Hero>() where Player.Distance(champ.ServerPosition) < W.Range && champ.IsEnemy && champ.IsValid select champ).ToList();
            enemy.OrderBy(x => rObj.Position.Distance(x.ServerPosition));

            castW(enemy.FirstOrDefault());
        }*/

        private void CastWEscape(Obj_AI_Hero target)
        {
            PredictionOutput pred = W.GetPrediction(target);
            var vec = new Vector3(pred.CastPosition.X - Player.ServerPosition.X, 0,
                pred.CastPosition.Z - Player.ServerPosition.Z);
            Vector3 castBehind = pred.CastPosition - Vector3.Normalize(vec) * 125;

            if (W.IsReady())
                W.Cast(castBehind, packets());
        }

        private bool checkChilled(Obj_AI_Hero target)
        {
            return target.HasBuff("Chilled");
        }

        private void DetonateQ()
        {
            if (_qMissle == null || !Q.IsReady())
                return;

            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(1200) && x.Distance(_qMissle.Position) < 200).OrderByDescending(GetComboDamage))
            {
                if (ShouldDetonate(enemy) && Environment.TickCount - Q.LastCastAttemptT > Game.Ping)
                {
                    Q.Cast(_qMissle.Position);
                }
            }
        }

        private bool ShouldDetonate(Obj_AI_Hero target)
        {
            if (menu.Item("detonateQ2", true).GetValue<bool>())
            {
                if (target.Distance(_qMissle.Position) < Q.Width + target.BoundingRadius && checkChilled(target))
                    return true;
            }

            if (target.Distance(_qMissle.Position) < Q.Width + target.BoundingRadius)
                return true;

            return false;
        }

        private void Snipe()
        {
            var range = Q.Range;
            var focusSelected = menu.Item("selected", true).GetValue<bool>();

            Obj_AI_Hero qTarget = TargetSelector.GetTarget(range, TargetSelector.DamageType.Magical);

            if (TargetSelector.GetSelectedTarget() != null)
                if (focusSelected && TargetSelector.GetSelectedTarget().Distance(Player.ServerPosition) < range)
                    qTarget = TargetSelector.GetSelectedTarget();

            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (qTarget == null)
                return;

            if (W.IsReady() && Q.IsReady() && Player.Distance(qTarget.ServerPosition) < W.Range)
                CastW(qTarget);

            if (!W.IsReady() && Q.IsReady() && Player.Distance(qTarget.ServerPosition) < Q.Range &&
                Q.GetPrediction(qTarget).Hitchance >= HitChance.High && ShouldQ())
            {
                Q.Cast(Q.GetPrediction(qTarget).CastPosition);
            }
        }

        private void CheckR()
        {
            if (_rObj == null)
                return;

            int hit = ObjectManager.Get<Obj_AI_Hero>().Count(x => _rObj.Position.Distance(x.ServerPosition) < 475 && x.IsValidTarget(R.Range + 500));

            if (hit < 1 && R.IsReady() && _rFirstCreated && R.IsReady())
            {
                R.Cast();
            }
        }

        private void Escape()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            List<Obj_AI_Hero> enemy = (from champ in ObjectManager.Get<Obj_AI_Hero>() where champ.IsValidTarget(1500) select champ).ToList();

            if (Q.IsReady() && Player.Distance(enemy.FirstOrDefault().Position) <= Q.Range &&
                Q.GetPrediction(enemy.FirstOrDefault()).Hitchance >= HitChance.High && ShouldQ())
            {
                Q.Cast(enemy.FirstOrDefault());
            }

            if (W.IsReady() && Player.Distance(enemy.FirstOrDefault().Position) <= W.Range)
            {
                CastWEscape(enemy.FirstOrDefault());
            }
        }

        private void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;

            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsR = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, R.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range,
                MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();
            var useR = menu.Item("UseRFarm", true).GetValue<bool>();

            int hit = 0;

            if (useQ && Q.IsReady() && ShouldQ())
            {
                MinionManager.FarmLocation qPos = Q.GetLineFarmLocation(allMinionsQ);
                if (qPos.MinionsHit >= 3)
                {
                    Q.Cast(qPos.Position);
                }
            }

            if (useR & R.IsReady() && !_rFirstCreated)
            {
                MinionManager.FarmLocation rPos = R.GetCircularFarmLocation(allMinionsR);
                if (Player.Distance(rPos.Position) < R.Range)
                    R.Cast(rPos.Position);
            }

            if (!ShouldQ() && _qMissle != null)
            {
                if (useQ && Q.IsReady())
                {
                    hit += allMinionsQ.Count(enemy => enemy.Distance(_qMissle.Position) < 110);
                }

                if (hit >= 2 && Q.IsReady())
                    Q.Cast();
            }

            if (_rFirstCreated)
            {
                hit += allMinionsR.Count(enemy => enemy.Distance(_rObj.Position) < 400);

                if (hit < 2 && R.IsReady())
                    R.Cast();
            }

            if (useE && allMinionsE.Count > 0 && E.IsReady())
                E.Cast(allMinionsE[0]);
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead)
                return;
            
            //detonate Q check
            var detQ = menu.Item("detonateQ", true).GetValue<bool>();
            if (detQ && !ShouldQ())
                DetonateQ();

            //checkR
            var rCheck = menu.Item("checkR", true).GetValue<bool>();
            if (rCheck && _rFirstCreated && !menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active && _rByMe)
                CheckR();


            //check ks
            SmartKs();

            if (menu.Item("escape", true).GetValue<KeyBind>().Active)
            {
                Escape();
            }
            else if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("snipe", true).GetValue<KeyBind>().Active)
                    Snipe();

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
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs attack)
        {
            if (!unit.IsMe) return;

            SpellSlot castedSlot = Player.GetSpellSlot(attack.SData.Name);

            if (castedSlot == SpellSlot.E)
            {
                _eCasted = true;
            }

            if (castedSlot == SpellSlot.Q && ShouldQ())
            {
                Q.LastCastAttemptT = Environment.TickCount;
            }
            
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap", true).GetValue<bool>()) return;

            if (W.IsReady() && gapcloser.Sender.IsValidTarget(W.Range))
            {
                Vector3 vec = Player.ServerPosition -
                              Vector3.Normalize(Player.ServerPosition - gapcloser.Sender.ServerPosition) * 1;
                W.Cast(vec, packets());
            }
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!menu.Item("UseInt", true).GetValue<bool>()) return;

            if (unit.IsValidTarget(Q.Range) && Q.IsReady())
            {
                Q.Cast(unit);
            }

            if (unit.IsValidTarget(W.Range) && W.IsReady())
            {
                W.Cast(unit, packets());
            }
        }

        protected override void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            //if(Player.Distance(obj.Position) < 300)
            //Game.PrintChat("OBJ: " + obj.Name);

            if (obj.Type != GameObjectType.obj_GeneralParticleEmitter || Player.Distance(obj.Position) > 1500)
                return;

            //Q
            if (obj.IsValid && obj.Name == "cryo_FlashFrost_Player_mis.troy")
            {
                _qMissle = obj;
            }

            //R
            if (obj.IsValid && obj.Name.Contains("cryo_storm"))
            {
                if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active || menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active || menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    _rByMe = true;

                _rObj = obj;
                _rFirstCreated = true;
            }
        }

        protected override void GameObject_OnDelete(GameObject obj, EventArgs args)
        {
            if (obj.Type != GameObjectType.obj_GeneralParticleEmitter || Player.Distance(obj.Position) > 1500)
                return;

            //Q
            if (Player.Distance(obj.Position) < 1500)
            {
                if (obj.IsValid && obj.Name == "cryo_FlashFrost_Player_mis.troy")
                {
                    _qMissle = null;
                }

                //R
                if (obj.IsValid && obj.Name.Contains("cryo_storm"))
                {
                    _rObj = null;
                    _rFirstCreated = false;
                    _rByMe = false;
                }
            }
        }
    }
}
