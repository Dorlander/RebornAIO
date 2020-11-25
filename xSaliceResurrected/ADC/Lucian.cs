using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSaliceResurrected.Managers;
using xSaliceResurrected.Utilities;
using Color = System.Drawing.Color;
using Geometry = LeagueSharp.Common.Geometry;

namespace xSaliceResurrected.ADC
{
    class Lucian : Champion
    {
        public Lucian()
        {
            LoadSpells();
            LoadMenu();
        }

        private void LoadSpells()
        {
            SpellManager.Q = new Spell(SpellSlot.Q, 650);
            SpellManager.Q.SetTargetted(0.25f, float.MaxValue);

            SpellManager.QExtend = new Spell(SpellSlot.Q, 1100);
            SpellManager.QExtend.SetSkillshot(0.35f, 25f, float.MaxValue, false, SkillshotType.SkillshotLine);

            SpellManager.W = new Spell(SpellSlot.W, 1000);
            SpellManager.W.SetSkillshot(0.3f, 80, 1600, true, SkillshotType.SkillshotLine);

            SpellManager.E = new Spell(SpellSlot.E, 425);
            SpellManager.E.SetSkillshot(.25f, 1f, float.MaxValue, false, SkillshotType.SkillshotLine);

            SpellManager.R = new Spell(SpellSlot.R, 1400);
            SpellManager.R.SetSkillshot(.1f, 110, 2800, true, SkillshotType.SkillshotLine);
        }

        private void LoadMenu()
        {
            //key
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "Use Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseQExtendCombo", "Use Q Extended", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "Use W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R", true).SetValue(true));
                combo.AddSubMenu(HitChanceManager.AddHitChanceMenuCombo(false, true, false, false));
                //add to menu
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                harass.AddSubMenu(HitChanceManager.AddHitChanceMenuHarass(false, true, false, false));
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

            var misc = new Menu("Misc", "Misc");
            {
                misc.AddSubMenu(AoeSpellManager.AddHitChanceMenuCombo(false, true, false, false));
                misc.AddItem(new MenuItem("MovementCheck", "Only Cast Extended Q When Enemy is moving(More Accurate)", true).SetValue(false));
                misc.AddItem(new MenuItem("CheckPassive", "Smart Passive", true).SetValue(true));
                misc.AddItem(new MenuItem("smartKS", "Use Smart KS System", true).SetValue(true));
                misc.AddItem(new MenuItem("E_If_HP", "Do not E If HP <=", true).SetValue(new Slider(20)));
                //add to menu
                menu.AddSubMenu(misc);
            }

            var drawMenu = new Menu("Drawing", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "Disable All", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "Draw Q", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_Q_Extended", "Draw Q Extended", true).SetValue(true));
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
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R) * GetShots();

            comboDamage = ItemManager.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target) * 2);
        }

        private double GetShots()
        {
            double shots = 0;

            if (R.Level == 1)
                shots = 7.5 + 7.5 * (Player.AttackSpeedMod - .6);
            if (R.Level == 2)
                shots = 7.5 + 9 * (Player.AttackSpeedMod - .6);
            if (R.Level == 3)
                shots = 7.5 + 10.5 * (Player.AttackSpeedMod - .6);

            return shots / 1.4;
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(), menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(), menu.Item("UseEHarass", true).GetValue<bool>(), false, "Harass");
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

            _ticker++;
            if (_ticker > 45)
                _ticker = 0;

            if (useQ && _ticker < 15)
                Cast_Q();
            if (useW && _ticker < 30)
                Cast_W(source);
            if (useE && _ticker == 45)
                Cast_E();
            if (useR)
                Cast_R();
        }

        private int _ticker;

        private void Cast_Q(Obj_AI_Hero forceTarget = null)
        {
            if (!Q.IsReady() || !PassiveCheck())
                return;

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (forceTarget != null)
                target = forceTarget;

            if (target != null && target.IsValidTarget(Q.Range))
            {
                if (Q.Cast(target) == Spell.CastStates.SuccessfullyCasted)
                {
                    Q.LastCastAttemptT = Utils.TickCount;
                    return;
                }
            }

            target = TargetSelector.GetTarget(QExtend.Range, TargetSelector.DamageType.Physical);

            if (forceTarget != null)
                target = forceTarget;

            if (target == null || !menu.Item("UseQExtendCombo", true).GetValue<bool>())
                return;

            var pred = QExtend.GetPrediction(target, true);
            var collisions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (!collisions.Any() || (!target.IsMoving && menu.Item("MovementCheck", true).GetValue<bool>()))
                return;

            foreach (var minion in collisions)
            {
                var poly = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(minion.ServerPosition, QExtend.Range), QExtend.Width);

                if (poly.IsInside(pred.UnitPosition))
                {
                    if (Q.Cast(minion) == Spell.CastStates.SuccessfullyCasted)
                    {
                        Q.LastCastAttemptT = Utils.TickCount;
                        return;
                    }
                }
            }
        }

        private void Cast_W(string source)
        {
            if (!W.IsReady() || !PassiveCheck())
                return;

            SpellCastManager.CastBasicSkillShot(W, W.Range, TargetSelector.DamageType.Magical, HitChanceManager.GetWHitChance(source));
        }

        private void Cast_E()
        {
            if (!E.IsReady() || !PassiveCheck())
                return;

            var target = TargetSelector.GetTarget(E.Range + Player.AttackRange, TargetSelector.DamageType.Physical);

            if (target == null)
                return;

            Vector3 vec = Player.ServerPosition + Vector3.Normalize(Game.CursorPos - Player.ServerPosition) * E.Range;

            if (Player.Distance(Game.CursorPos) < E.Range & Player.Distance(Game.CursorPos) > 150)
                vec = Game.CursorPos;

            if (vec.CountEnemiesInRange(500) >= 3 && vec.CountAlliesInRange(400) < 3)
                return;

            if (Player.HealthPercent <= menu.Item("E_If_HP", true).GetValue<Slider>().Value)
                return;

            if (vec.Distance(target.ServerPosition) < Player.AttackRange)
            {
                E.Cast(vec);
            }
        }

        private void Cast_R()
        {
            if (!R.IsReady())
                return;

            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            if (target == null)
                return;

            if (Player.GetSpellDamage(target, SpellSlot.R) * GetShots() > target.Health)
                R.Cast(target);
        }

        private bool PassiveCheck()
        {
            if (!menu.Item("CheckPassive", true).GetValue<bool>())
                return true;

            if (_hasBuff || Player.HasBuff("LucianPassiveBuff"))
            {
                return false;
            }

            if (Utils.TickCount - Q.LastCastAttemptT < 500)
                return false;

            if (Utils.TickCount - W.LastCastAttemptT < 500)
                return false;

            if (Utils.TickCount - E.LastCastAttemptT < 500)
                return false;

            return true;
        }

        private bool _hasBuff;

        protected override void ObjAiBaseOnOnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if (!sender.IsMe)
                return;

            if (args.Buff.DisplayName == "LucianPassiveBuff")
                _hasBuff = true;
        }

        protected override void ObjAiBaseOnOnBuffRemove(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args)
        {
            if (!sender.IsMe)
                return;

            if (args.Buff.DisplayName == "LucianPassiveBuff")
                _hasBuff = false;
        }

        private void SmartKs()
        {
            if (!menu.Item("smartKS", true).GetValue<bool>())
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(QExtend.Range) && !x.IsDead && !x.HasBuffOfType(BuffType.Invulnerability)))
            {
                //Q
                if (Q.IsKillable(target) && Player.Distance(target.Position) < QExtend.Range && Q.IsReady())
                {
                    Cast_Q(target);
                }
                //E
                if (W.IsKillable(target) && Player.Distance(target.Position) < W.Range && W.IsReady())
                {
                    W.Cast(target);
                }
            }
        }

        private void Farm()
        {
            if (!ManaManager.HasMana("LaneClear"))
                return;

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();

            if (useQ && PassiveCheck())
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
                var minion = allMinions.FirstOrDefault(minionn => minionn.Distance(Player.Position) <= Q.Range && HealthPrediction.LaneClearHealthPrediction(minionn, 500) > 0);
                if (minion == null)
                    return;

                Q.Cast(minion);
            }
            if (useW && PassiveCheck())
            {
                var allMinionE = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.NotAlly);

                if (allMinionE.Count > 1)
                {
                    var pred = W.GetCircularFarmLocation(allMinionE);

                    W.Cast(pred.Position);
                }
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            if (Player.IsChannelingImportantSpell())
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                return;
            }

            SmartKs();

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
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (!unit.IsMe)
                return;

            SpellSlot castedSlot = ObjectManager.Player.GetSpellSlot(args.SData.Name);

            if (castedSlot == SpellSlot.Q)
            {
                Q.LastCastAttemptT = Utils.TickCount;
            }
            if (castedSlot == SpellSlot.W)
            {
                W.LastCastAttemptT = Utils.TickCount;
            }
            if (castedSlot == SpellSlot.E)
            {
                E.LastCastAttemptT = Utils.TickCount;
            }
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            if (menu.Item("Draw_Q", true).GetValue<bool>())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_Q_Extended", true).GetValue<bool>())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, QExtend.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_W", true).GetValue<bool>())
                if (W.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }
    }
}