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
    class Viktor : Champion
    {
        private static Obj_AI_Minion _rObj;
        private static int _lastR;

        public Viktor()
        {
            LoadSpells();
            LoadMenu();
            _lastR = Utils.TickCount;
        }

        private void LoadSpells()
        {
            //intalize spell
            SpellManager.P = new Spell(SpellSlot.E, 3000);
            SpellManager.Q = new Spell(SpellSlot.Q, 700);
            SpellManager.W = new Spell(SpellSlot.W, 700);
            SpellManager.E = new Spell(SpellSlot.E, 540);
            SpellManager.E2 = new Spell(SpellSlot.E, 700);
            SpellManager.R = new Spell(SpellSlot.R, 700);

            SpellManager.Q.SetTargetted(0.25f, 2000);
            SpellManager.W.SetSkillshot(.25f, 300, float.MaxValue, false, SkillshotType.SkillshotCircle);
            SpellManager.E.SetSkillshot(0.2f, 90, 1000, false, SkillshotType.SkillshotCircle);
            SpellManager.E2.SetSkillshot(0.2f, 90, 1000, false, SkillshotType.SkillshotCircle);
            SpellManager.P.SetSkillshot(0.2f, 90, 1000, false, SkillshotType.SkillshotCircle);
            SpellManager.R.SetSkillshot(0.25f, 250, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        private void LoadMenu()
        {
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!", true).SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitQQ", "Last hit with Q", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("rAlways", "Ult Always Combo", true).SetValue(new KeyBind("K".ToCharArray()[0], KeyBindType.Toggle)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("SpellMenu", "SpellMenu");
            {
                var qMenu = new Menu("QMenu", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("QAARange", "Q only if in AA Range", true).SetValue(true));
                    spellMenu.AddSubMenu(qMenu);
                }

                var wMenu = new Menu("WMenu", "WMenu");
                {
                    wMenu.AddItem(new MenuItem("wSlow", "Auto W Slow", true).SetValue(true));
                    wMenu.AddItem(new MenuItem("wImmobile", "Auto W Immobile", true).SetValue(true));
                    wMenu.AddItem(new MenuItem("wDashing", "Auto W Dashing", true).SetValue(true));
                    spellMenu.AddSubMenu(wMenu);
                }

                var eMenu = new Menu("EMenu", "EMenu");
                {
                    eMenu.AddItem(new MenuItem("Line_If_Enemy_Count", "Auto E If >= Enemy, 6 = Off", true).SetValue(new Slider(4, 1, 6)));
                    eMenu.AddItem(new MenuItem("Line_If_Enemy_Count_Combo", "E if >= In Combo, 6 = off", true).SetValue(new Slider(3, 1, 6)));
                    spellMenu.AddSubMenu(eMenu);
                }

                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "Use Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "Use W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombos", "Use R", true).SetValue(true));
                combo.AddSubMenu(HitChanceManager.AddHitChanceMenuCombo(false, true, false, true));
                //add to menu
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(false));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                harass.AddSubMenu(HitChanceManager.AddHitChanceMenuHarass(false, true, false, false));
                //add to menu
                menu.AddSubMenu(harass);
            }

            var farm = new Menu("LaneClear", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "Use E", true).SetValue(true));
                farm.AddItem(new MenuItem("MinMinion", "Min Minion To E >=", true).SetValue(new Slider(3, 1, 5)));
                //add to menu
                menu.AddSubMenu(farm);
            }

            var miscMenu = new Menu("Misc", "Misc");
            {
                miscMenu.AddSubMenu(AoeSpellManager.AddHitChanceMenuCombo(false, true, false, true));
                miscMenu.AddItem(new MenuItem("UseInt", "Use R to Interrupt", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("UseGap", "Use Q for GapCloser", true).SetValue(true));
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
                drawMenu.AddItem(new MenuItem("Draw_E_Pred", "Draw E Best Line", true).SetValue(true));

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
                customMenu.AddItem(myCust.AddToMenu("Harass(T) Active: ", "HarassActiveT"));
                customMenu.AddItem(myCust.AddToMenu("Laneclear Active: ", "LaneClearActive"));
                customMenu.AddItem(myCust.AddToMenu("Lasthit Q Active: ", "LastHitQQ"));
                menu.AddSubMenu(customMenu);
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
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
                damage += 5 * Player.GetSpellDamage(enemy, SpellSlot.R, 1);
            }

            if (_rObj != null)
                damage += Player.GetSpellDamage(enemy, SpellSlot.R, 1);

            return (float)damage;
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombos", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), false,
                menu.Item("UseEHarass", true).GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, string source)
        {
            var range = E.IsReady() ? (E.Range + E2.Range) : Q.Range;
            Obj_AI_Hero target = TargetSelector.GetTarget(range, TargetSelector.DamageType.Magical);
            
            if (!target.IsValidTarget(range))
                return;

            var dmg = GetComboDamage(target);

            if (useE && E.IsReady())
            {
                SpellCastManager.CastBestLine(true, E, E2, (int)(E2.Range / 2), menu, 1, false);
            }

            if (useW &&  W.IsReady() && ShouldW(target, dmg))
            {
                W.Cast(target);
            }

            if (menu.Item("QAARange", true).GetValue<bool>())
            {
                if (useQ && target != null && Q.IsReady() && Player.ServerPosition.Distance(target.ServerPosition) <= Player.AttackRange) // Q only in AA range for guaranteed AutoAttack
                {
                    Q.Cast(target);
                    return;
                }
            }

            else if (useQ && Q.IsReady() && Player.ServerPosition.Distance(target.ServerPosition) <= Q.Range)
            {
                Q.Cast(target);
                return;
            }

            if (useR &&  R.IsReady() && _rObj == null && ShouldR(target, dmg) && R.GetPrediction(target).Hitchance >= HitChanceManager.GetRHitChance(source))
            {
                if (target != null) R.Cast(target.Position);
            }

        }

        private bool ShouldW(Obj_AI_Hero target, float dmg)
        {
            if (dmg >= target.Health)
                return true;

            var immobile = menu.Item("wSlow", true).GetValue<bool>();
            var slow = menu.Item("wImmobile", true).GetValue<bool>();
            var dashing = menu.Item("wImmobile", true).GetValue<bool>();

            if (W.GetPrediction(target).Hitchance == HitChance.Immobile && immobile)
                return true;

            if (target.HasBuffOfType(BuffType.Slow) && slow)
                return true;

            if (W.GetPrediction(target).Hitchance == HitChance.Dashing && dashing)
                return true;

            if (Player.Distance(target.ServerPosition) < 300)
                return true;

            return false;
        }

        private bool ShouldR(Obj_AI_Hero target, float dmg)
        {
            if (dmg + 200 > target.Health)
                return true;

            if (menu.Item("rAlways", true).GetValue<KeyBind>().Active)
                return true;

            return false;
        }

        private void AutoR()
        {
            if (_rObj != null && Utils.TickCount - _lastR > 200)
            {
                foreach (
                    Obj_AI_Hero target in
                        HeroManager.Enemies
                            .Where(x => x.IsValidTarget(3500) && !x.IsDead && x.Health > 0).OrderByDescending(x => x.Distance(_rObj.Position)))
                {
                    Player.Spellbook.CastSpell(SpellSlot.R, target.ServerPosition);
                    _lastR = Utils.TickCount;
                }
            }
        }

        protected override void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (!menu.Item("ComboActive", true).GetValue<KeyBind>().Active &&
                !menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                return;

            if (args.Target.Type == GameObjectType.obj_AI_Hero)
            {
                args.Process = !(Q.IsReady() && Player.Mana >= QSpell.ManaCost);
            }
            else
                args.Process = true;
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap", true).GetValue<bool>()) return;

            if (Q.IsReady() && gapcloser.Sender.IsValidTarget(Q.Range))
                Q.Cast(gapcloser.Sender);
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            SpellCastManager.CastBestLine(false, E, E2, (int)(E2.Range/2), menu);

            AutoR();
            

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }    
            else
            {
                if (menu.Item("LastHitQQ", true).GetValue<KeyBind>().Active)
                    LastHit();

                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active || menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();

            }
        }

        private void LastHit()
        {
            if (!OrbwalkManager.CanMove(40)) return;

            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

            if (Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion) * 1000 / 1400)) < Player.GetSpellDamage(minion, SpellSlot.Q) - 10)
                    {
                        Q.Cast(minion);
                        return;
                    }
                }
            }
        }

        private static MinionManager.FarmLocation GetBestLineFarmLocation(Vector2 lineSource, List<Vector2> minionPositions, float width, float range)
        {
            var result = new Vector2();
            var minionCount = 0;
            var startPos = lineSource;

            var max = minionPositions.Count;
            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < max; j++)
                {
                    if (minionPositions[j] != minionPositions[i])
                    {
                        minionPositions.Add((minionPositions[j] + minionPositions[i]) / 2);
                    }
                }
            }

            foreach (var pos in minionPositions)
            {
                if (pos.Distance(startPos, true) <= range * range)
                {
                    var endPos = startPos + range * (pos - startPos).Normalized();

                    var count =
                        minionPositions.Count(pos2 => pos2.Distance(startPos, endPos, true, true) <= width * width);

                    if (count >= minionCount)
                    {
                        result = endPos;
                        minionCount = count;
                    }
                }
            }

            return new MinionManager.FarmLocation(result, minionCount);
        }

        private void Farm()
        {
            if (!OrbwalkManager.CanMove(40)) return;


            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ && allMinionsQ.Count > 0 && Q.IsReady())
            {
                Q.Cast(allMinionsQ[0]);
            }

            var minionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);
            if (minionsE.Count == 0)
                return;

            var minionsE2 = MinionManager.GetMinions(minionsE[0].ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);
            var points = minionsE2.Select(x => x.ServerPosition.To2D()).ToList();
            if (useE && E.IsReady() && minionsE.Count > 0)
            {
                E2.UpdateSourcePosition(minionsE[0].ServerPosition, minionsE[0].ServerPosition);
                var pred = GetBestLineFarmLocation(minionsE[0].ServerPosition.To2D(), points, E.Width, E2.Range);
                var min = menu.Item("MinMinion", true).GetValue<Slider>().Value;

                if (pred.MinionsHit >= min)
                {
                    SpellCastManager.CastLineSpell(minionsE[0].Position, pred.Position.To3D());
                }
            }
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!menu.Item("UseInt", true).GetValue<bool>()) return;

            if (Player.Distance(unit) < R.Range && R.IsReady())
            {
                R.Cast(unit.Position);
            }
        }

        protected override void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            if (!(obj is Obj_AI_Minion) || obj.IsEnemy)
                return;

            Console.WriteLine(obj.Name);
            if (Player.Distance(obj.Position) < 3000)
            {
                if (obj.IsValid && obj.Name == "Storm")
                {
                    Console.WriteLine(obj.Type);
                    _rObj = (Obj_AI_Minion)obj;
                }
            }

        }

        protected override void GameObject_OnDelete(GameObject obj, EventArgs args)
        {
            if (!(obj is Obj_AI_Minion) || obj.IsEnemy)
                return;

            if (Player.Distance(obj.Position) < 3000)
            {
                if (obj.IsValid && obj.Name == "Storm")
                {
                    Console.WriteLine(obj.Type);
                    _rObj = null;
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

            if (menu.Item("Draw_W", true).GetValue<bool>())
                if (W.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, W.Range - 2, W.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E_Pred", true).GetValue<bool>() && E.IsReady())
            {
                SpellCastManager.DrawBestLine(E, E2, (int)(E2.Range / 2), 1, false);
            }
            
        }

    }
}
