using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using ObjectManager = LeagueSharp.ObjectManager;

namespace xSaliceReligionAIO.Champions
{
    class Rumble : Champion
    {
        public Rumble()
        {
            LoadSpells();
            LoadMenu();
        }

        private void LoadSpells()
        {
            //intalize spell
            Q = new Spell(SpellSlot.Q, 500);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 950);
            R = new Spell(SpellSlot.R, 1700);
            R2 = new Spell(SpellSlot.R, 800);

            E.SetSkillshot(0.45f, 90, 1200, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.25f, 110, 2500, false, SkillshotType.SkillshotLine);
            R2.SetSkillshot(0.25f, 110, 2600, false, SkillshotType.SkillshotLine);
        }

        private void LoadMenu()
        {
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitE", "Last hit with E!", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("UseMecR", "Force Best Mec Ult", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("SpellMenu", "SpellMenu");
            {
                var qMenu = new Menu("QMenu", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("Q_Auto_Heat", "Use Q To generate Heat", true).SetValue(true));
                    qMenu.AddItem(new MenuItem("Q_Over_Heat", "Q Smart OverHeat KS", true).SetValue(true));
                    spellMenu.AddSubMenu(qMenu);
                }

                var wMenu = new Menu("WMenu", "WMenu");
                {
                    wMenu.AddItem(new MenuItem("W_Auto_Heat", "Use W To generate Heat", true).SetValue(true));
                    wMenu.AddItem(new MenuItem("W_Always", "Use W Always On Combo/Harass", true).SetValue(false));
                    wMenu.AddItem(new MenuItem("W_Block_Spell", "Use W On Incoming Spells", true).SetValue(true));
                    spellMenu.AddSubMenu(wMenu);
                }

                var eMenu = new Menu("EMenu", "EMenu");
                {
                    eMenu.AddItem(new MenuItem("E_Auto_Heat", "Use E To generate Heat", true).SetValue(false));
                    eMenu.AddItem(new MenuItem("E_Over_Heat", "E Smart OverHeat KS", true).SetValue(true));
                    spellMenu.AddSubMenu(eMenu);
                }

                var rMenu = new Menu("RMenu", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("R_If_Enemy_Count", "Auto R If >= Enemy, 6 = Off", true).SetValue(new Slider(4, 1, 6)));
                    rMenu.AddItem(new MenuItem("R_If_Enemy_Count_Combo", "R if >= In Combo, 6 = off", true).SetValue(new Slider(3, 1, 6)));
                    spellMenu.AddSubMenu(rMenu);
                }

                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "Use Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "Use W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E", true).SetValue(true));
                combo.AddItem(new MenuItem("qHit", "E HitChance", true).SetValue(new Slider(3, 1, 3)));
                combo.AddItem(new MenuItem("UseRCombos", "Use R", true).SetValue(false));
                //add to menu
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(false));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                harass.AddItem(new MenuItem("qHit2", "E HitChance", true).SetValue(new Slider(3, 1, 4)));
                //add to menu
                menu.AddSubMenu(harass);
            }

            var farm = new Menu("LaneClear", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "Use E", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(farm);
            }

            var miscMenu = new Menu("Misc", "Misc");
            {
                miscMenu.AddItem(new MenuItem("Stay_Danger", "Stay In Danger Zone", true).SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Toggle)));
                miscMenu.AddItem(new MenuItem("E_Gap_Closer", "Use E On Gap Closer", true).SetValue(true));
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
                drawMenu.AddItem(new MenuItem("Draw_R_Pred", "Draw R Best Line", true).SetValue(true));

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
        }

        private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += GetCurrentHeat() > 50 ? Player.GetSpellDamage(target, SpellSlot.Q) * 2 : Player.GetSpellDamage(target, SpellSlot.Q);

            if (E.IsReady())
                comboDamage += GetCurrentHeat() > 50 ? Player.GetSpellDamage(target, SpellSlot.E) * 1.5: Player.GetSpellDamage(target, SpellSlot.E);

            if (R.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R) * 3;

            comboDamage = ActiveItems.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target));
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombos", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, string source)
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (target == null)
                return;

            if (useQ && ShouldQ(target))
                Q.Cast(target);

            if (useW && menu.Item("W_Always", true).GetValue<bool>() && W.IsReady())
                W.Cast(packets());

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

            if (useE && ShouldE(target, source))
                E.Cast(target, packets());

            if (useR && GetComboDamage(target) > target.Health)
                CastSingleR();
        }

        private void Farm()
        {
            if (!Orbwalking.CanMove(40))
                return;

            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range,
                MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ && allMinionsQ.Count > 0)
                Q.Cast(allMinionsQ[0]);

            if (useE && allMinionsE.Count > 0)
                E.Cast(allMinionsE[0]);
        }

        private void LastHit()
        {
            if (!Orbwalking.CanMove(40))
                return;

            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range,
                MinionTypes.All, MinionTeam.NotAlly);

            if (allMinionsE.Count > 0 && E.IsReady())
            {
                foreach (var minion in allMinionsE)
                {
                    if(E.IsKillable(minion))
                        E.Cast(minion);
                }
            }

        }

        private bool ShouldQ(Obj_AI_Hero target)
        {
            if (!Q.IsReady())
                return false;

            if (Player.Distance(target.Position) > Q.Range)
                return false;

            if (!menu.Item("Q_Over_Heat", true).GetValue<bool>() && GetCurrentHeat() > 80)
                return false;

            if (GetCurrentHeat() > 80 && !(Player.GetSpellDamage(target, SpellSlot.Q, 1) + Player.GetAutoAttackDamage(target) * 2 > target.Health))
                return false;

            return true;
        }

        private bool ShouldE(Obj_AI_Hero target, string source)
        {
            if (!E.IsReady())
                return false;

            if (Player.Distance(target.Position) > E.Range)
                return false;

            if(E.GetPrediction(target).Hitchance < GetHitchance(source))

            if (!menu.Item("E_Over_Heat", true).GetValue<bool>() && GetCurrentHeat() > 80)
                return false;

            if (GetCurrentHeat() > 80 && !(Player.GetSpellDamage(target, SpellSlot.E, 1) + Player.GetAutoAttackDamage(target) * 2 > target.Health))
                return false;

            return true;
        }

        private void StayInDangerZone()
        {
            if (Player.InFountain() || IsRecalling()) 
                return;

            if (GetCurrentHeat() < 31 && W.IsReady() && menu.Item("W_Auto_Heat", true).GetValue<bool>())
            {
                W.Cast(packets());
                return;
            }

            if (GetCurrentHeat() < 31 && Q.IsReady() && menu.Item("Q_Auto_Heat", true).GetValue<bool>())
            {
                var enemy = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy).OrderBy(x => Player.Distance(x.Position)).FirstOrDefault();

                if(enemy != null)
                    Q.Cast(enemy.ServerPosition, packets());
                return;
            }

            if (GetCurrentHeat() < 31 && E.IsReady() && menu.Item("E_Auto_Heat", true).GetValue<bool>())
            { 
                var enemy = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && !x.IsDead).OrderBy(x => Player.Distance(x.Position)).FirstOrDefault();

                if (enemy != null)
                    E.Cast(enemy, packets());
            }

        }

        private float GetCurrentHeat()
        {
            return Player.Mana;
        }

        private void CastSingleR()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (target == null)
                return;

            var vector1 = target.ServerPosition - Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * 300;

            R2.UpdateSourcePosition(vector1, vector1);

            var pred = R2.GetPrediction(target, true);

            if (Player.Distance(target.Position) < 400)
            {
                var midpoint = (Player.ServerPosition + pred.UnitPosition)/2;

                vector1 = midpoint + Vector3.Normalize(pred.UnitPosition - Player.ServerPosition) * 800;
                var vector2 = midpoint - Vector3.Normalize(pred.UnitPosition - Player.ServerPosition) * 300;

                if(!IsPassWall(pred.UnitPosition, vector1) && !IsPassWall(pred.UnitPosition, vector2))
                    CastR(vector1, vector2);
            }
            else if (!IsPassWall(pred.UnitPosition, vector1) && !IsPassWall(pred.UnitPosition, pred.CastPosition))
            {
                //wall check
                if(pred.Hitchance >= HitChance.Medium)
                    CastR(vector1, pred.CastPosition);
            }
        }

        private void CastMecR(bool forceUlt)
        {
            //check if only one target
            if (countEnemiesNearPosition(Player.ServerPosition, R.Range + 500) < 2 && forceUlt)
            {
                CastSingleR();
                return;
            }

            int maxHit = 0;
            Vector3 start = Vector3.Zero;
            Vector3 end = Vector3.Zero;

            //loop one
            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(R.Range)).OrderByDescending(GetComboDamage))
            {
                //loop 2
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(R.Range + 1000) && x.NetworkId != target.NetworkId && x.Distance(target.Position) < 900)
                    .OrderByDescending(x => x.Distance(target.Position)))
                {
                    int hit = 2;

                    var targetPred = Prediction.GetPrediction(target, .25f);
                    var enemyPred = Prediction.GetPrediction(enemy, .25f);

                    var midpoint = (enemyPred.CastPosition + targetPred.CastPosition) / 2;

                    var startpos = midpoint + Vector3.Normalize(enemyPred.CastPosition - targetPred.CastPosition) * 600;
                    var endPos = midpoint - Vector3.Normalize(enemyPred.CastPosition - targetPred.CastPosition) * 600;

                    if (!IsPassWall(midpoint, startpos) && !IsPassWall(midpoint, endPos) && countEnemiesNearPosition(Player.ServerPosition, R.Range + 1000) > 2)
                    {
                        //loop 3
                        foreach (var enemy2 in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(R.Range + 1000) && x.NetworkId != target.NetworkId && x.NetworkId != enemy.NetworkId && x.Distance(target.Position) < 1000))
                        {
                            var enemy2Pred = Prediction.GetPrediction(enemy2, .25f);
                            Object[] obj = VectorPointProjectionOnLineSegment(startpos.To2D(), endPos.To2D(), enemy2Pred.CastPosition.To2D());
                            var isOnseg = (bool)obj[2];
                            var pointLine = (Vector2)obj[1];

                            if (pointLine.Distance(enemy2Pred.CastPosition.To2D()) < 110 && isOnseg)
                            {
                                hit++;
                            }
                        }
                    }

                    if (hit > maxHit && hit > 1)
                    {
                        maxHit = hit;
                        start = startpos;
                        end = endPos;
                    }
                }
            }

                if (start != Vector3.Zero && end != Vector3.Zero && R.IsReady())
                {
                    if (forceUlt)
                        CastR(start, end);
                    if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active && maxHit >= menu.Item("R_If_Enemy_Count_Combo", true).GetValue<Slider>().Value)
                        CastR(start, end);
                    if (maxHit >= menu.Item("R_If_Enemy_Count", true).GetValue<Slider>().Value)
                        CastR(start, end);
                }
            

        }

        private void CastR(Vector3 source, Vector3 destination)
        {
            if (!R.IsReady())
                return;

            /*new PKT_NPC_CastSpellReq()
            {
                From = source.To2D(),
                To = destination.To2D(),
                NetworkId = ObjectManager.Player.NetworkId,
                SpellSlot = (byte)SpellSlot.R,
                Unknown1 = true,
                Unknown2 = true,
            }.Encode().SendAsPacket();
             * */

            R.Cast(source, destination);
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            CastMecR(false);

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("UseMecR", true).GetValue<KeyBind>().Active)
                    CastMecR(true);

                if (menu.Item("LastHitE", true).GetValue<KeyBind>().Active)
                    LastHit();

                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                        Harass();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();
            }
            //stay in dangerzone
            if(menu.Item("Stay_Danger", true).GetValue<KeyBind>().Active)
                StayInDangerZone();
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (unit.IsEnemy && unit.Type == GameObjectType.obj_AI_Hero && W.IsReady() && menu.Item("W_Block_Spell", true).GetValue<bool>())
            {
                if (Player.Distance(args.End) < 400 && GetCurrentHeat() < 70)
                {
                    //Game.PrintChat("shielding");
                    W.Cast(packets());
                }
            }

            /*
            if (!unit.IsMe)
                return;
            
            SpellSlot castedSlot = Player.GetSpellSlot(args.SData.Name, false);

            if (castedSlot == SpellSlot.E)
            {
                E.LastCastAttemptT = Environment.TickCount;
            }
            */
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("E_Gap_Closer", true).GetValue<bool>()) return;

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
                    Render.Circle.DrawCircle(Player.Position, W.Range - 2, W.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);


            if (menu.Item("Draw_R_Pred", true).GetValue<bool>() && R.IsReady())
            {
                if (countEnemiesNearPosition(Player.ServerPosition, R.Range + 500) < 2)
                {
                    var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

                    if (target == null)
                        return;

                    var vector1 = target.ServerPosition - Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * 300;

                    R2.UpdateSourcePosition(vector1, vector1);

                    var pred = R2.GetPrediction(target, true);

                    var midpoint = (Player.ServerPosition + pred.UnitPosition) / 2;
                    var vector2 = midpoint - Vector3.Normalize(pred.UnitPosition - Player.ServerPosition) * 300;

                    if (Player.Distance(target.Position) < 400)
                    {
                        vector1 = midpoint + Vector3.Normalize(pred.UnitPosition - Player.ServerPosition)*800;
                        if (!IsPassWall(pred.UnitPosition, vector1) && !IsPassWall(pred.UnitPosition, vector2))
                        {
                            Vector2 wts = Drawing.WorldToScreen(Player.Position);
                            Drawing.DrawText(wts[0], wts[1], Color.Wheat, "Hit: " + 1);

                            Vector2 wtsPlayer = Drawing.WorldToScreen(vector1);
                            Vector2 wtsPred = Drawing.WorldToScreen(vector2);

                            Drawing.DrawLine(wtsPlayer, wtsPred, 1, Color.Wheat);
                            Render.Circle.DrawCircle(vector1, 50, Color.Aqua);
                            Render.Circle.DrawCircle(vector2, 50, Color.Yellow);
                            Render.Circle.DrawCircle(pred.UnitPosition, 50, Color.Red);
                        }
                    }
                    else if (!IsPassWall(pred.UnitPosition, vector1) && !IsPassWall(pred.UnitPosition, pred.CastPosition))
                    {
                        if (pred.Hitchance >= HitChance.Medium)
                        {
                            Vector2 wts = Drawing.WorldToScreen(Player.Position);
                            Drawing.DrawText(wts[0], wts[1], Color.Wheat, "Hit: " + 1);

                            Vector2 wtsPlayer = Drawing.WorldToScreen(vector1);
                            Vector2 wtsPred = Drawing.WorldToScreen(pred.CastPosition);

                            Drawing.DrawLine(wtsPlayer, wtsPred, 1, Color.Wheat);
                            Render.Circle.DrawCircle(vector1, 50, Color.Aqua);
                            Render.Circle.DrawCircle(pred.CastPosition, 50, Color.Yellow);
                        }
                    }
                    return;
                }
                //-----------------------------------------------------------------Draw Ult Mec-----------------------------------------------
                int maxHit = 0;
                Vector3 start = Vector3.Zero;
                Vector3 end = Vector3.Zero;
                Vector3 mid = Vector3.Zero;
                //loop one
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(R.Range)).OrderByDescending(GetComboDamage))
                {
                    //loop 2
                    foreach (
                        var enemy in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(
                                    x =>
                                        x.IsValidTarget(R.Range + 1000) && x.NetworkId != target.NetworkId &&
                                        x.Distance(target.Position) < 900)
                                .OrderByDescending(x => x.Distance(target.Position)))
                    {
                        int hit = 2;

                        var targetPred = Prediction.GetPrediction(target, .25f);
                        var enemyPred = Prediction.GetPrediction(enemy, .25f);

                        var midpoint = (enemyPred.CastPosition + targetPred.CastPosition) / 2;

                        var startpos = midpoint + Vector3.Normalize(enemyPred.CastPosition - targetPred.CastPosition) * 600;
                        var endPos = midpoint - Vector3.Normalize(enemyPred.CastPosition - targetPred.CastPosition) * 600;

                        if (!IsPassWall(midpoint, startpos) && !IsPassWall(midpoint, endPos) && countEnemiesNearPosition(Player.ServerPosition, R.Range + 1000) > 2)
                        {
                            //loop 3
                            foreach (var enemy2 in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(R.Range + 1000) && x.NetworkId != target.NetworkId && x.NetworkId != enemy.NetworkId && x.Distance(target.Position) < 1000))
                            {
                                var enemy2Pred = Prediction.GetPrediction(enemy2, .25f);

                                Object[] obj = VectorPointProjectionOnLineSegment(startpos.To2D(), endPos.To2D(), enemy2Pred.CastPosition.To2D());
                                var isOnseg = (bool)obj[2];
                                var pointLine = (Vector2)obj[1];

                                if (pointLine.Distance(enemy2Pred.CastPosition.To2D()) < 100 + enemy2.BoundingRadius &&
                                    isOnseg)
                                {
                                    hit++;
                                }
                            }
                        }
                        if (hit > maxHit)
                        {
                            maxHit = hit;
                            start = startpos;
                            end = endPos;
                            mid = midpoint;
                        }
                    }
                }

                if (maxHit >= 2)
                {
                    Vector2 wts = Drawing.WorldToScreen(Player.Position);
                    Drawing.DrawText(wts[0], wts[1], Color.Wheat, "Hit: " + maxHit);

                    Vector2 wtsPlayer = Drawing.WorldToScreen(start);
                    Vector2 wtsPred = Drawing.WorldToScreen(end);

                    Drawing.DrawLine(wtsPlayer, wtsPred, 1, Color.Wheat);
                    Render.Circle.DrawCircle(start, 50, Color.Aqua);
                    Render.Circle.DrawCircle(end, 50, Color.Yellow);
                    Render.Circle.DrawCircle(mid, 50, Color.Red);
                }
                //---------------------------------------------------End drawing Ult Mec---------------------------------------
            }
        }
    }
}
