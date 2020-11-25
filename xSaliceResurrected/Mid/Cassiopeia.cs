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
    class Cassiopeia : Champion
    {
        public Cassiopeia()
        {
            SetSpells();
            LoadMenu();
        }

        private void SetSpells()
        {
            SpellManager.Q = new Spell(SpellSlot.Q, 850);
            SpellManager.Q.SetSkillshot(0.6f, 80f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellManager.W = new Spell(SpellSlot.W, 850);
            SpellManager.W.SetSkillshot(0.5f, 90f, 2500, false, SkillshotType.SkillshotCircle);

            SpellManager.E = new Spell(SpellSlot.E, 700);
            SpellManager.E.SetTargetted(0.2f, float.MaxValue);

            SpellManager.R = new Spell(SpellSlot.R, 750);
            SpellManager.R.SetSkillshot(0.3f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);

            SpellManager.R2 = new Spell(SpellSlot.R,  1200);
            SpellManager.R2.SetSkillshot(0.3f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);
        }

        private void LoadMenu()
        {
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("Jungle", "Jungle Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitE", "Last Hit With E", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("forceUlt", "Ult Helper", true).SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("flashUlt", "Ult Flash", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("aoeUltOnly", "AOE Ult Only", true).SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("SpellMenu", "SpellMenu");
            {
                var qMenu = new Menu("QMenu", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("Auto_Q_Immobile", "Auto Q Immobile", true).SetValue(true));
                    qMenu.AddItem(new MenuItem("Auto_Q_Dashing", "Auto Q Dashing", true).SetValue(true));
                    spellMenu.AddSubMenu(qMenu);
                }

                var wMenu = new Menu("WMenu", "WMenu");
                {
                    wMenu.AddItem(new MenuItem("OnlyWIfnotPoison", "Only W if Q is offcd and enemy not poison", true).SetValue(false));
                    spellMenu.AddSubMenu(wMenu);
                }

                var eMenu = new Menu("EMenu", "EMenu");
                {
                    eMenu.AddItem(new MenuItem("E_Poison", "Auto E Poison Target", true).SetValue(true));
                    eMenu.AddItem(new MenuItem("E_Delay", "Delay between 0-1500(Milliseconds or tick)", true).SetValue(new Slider(0, 0, 1500)));
                    spellMenu.AddSubMenu(eMenu);
                }

                var rMenu = new Menu("RMenu", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("overKillCheck", "Over Kill Check", true).SetValue(true));
                    rMenu.AddItem(new MenuItem("blockR", "Block R if no enemy hit", true).SetValue(true));
                    rMenu.AddItem(new MenuItem("AOEStun", "Ult if Stun >= ", true).SetValue(new Slider(3, 1, 5)));
                    rMenu.AddItem(new MenuItem("KillableCombo", "Cast If target is Killable with Combo", true).SetValue(true));
                    rMenu.AddItem(new MenuItem("faceCheck", "Face Check for Killable with combo", true).SetValue(true));
                    rMenu.AddItem(new MenuItem("faceCheckHelper", "Face Check with UltHelper", true).SetValue(true));
                    rMenu.AddSubMenu(new Menu("Don't use R on", "Dont_R"));
                    foreach (var enemy in HeroManager.Enemies)
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
                combo.AddSubMenu(HitChanceManager.AddHitChanceMenuCombo(true, true, false, true));
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                harass.AddSubMenu(HitChanceManager.AddHitChanceMenuHarass(true, true, false, true));
                ManaManager.AddManaManagertoMenu(harass, "Harass", 50);
                menu.AddSubMenu(harass);
            }

            var farm = new Menu("LaneClear", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseWFarm", "Use W", true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "Use E", true).SetValue(false));
                farm.AddItem(new MenuItem("EMode", "E Mode", true).SetValue(new StringList(new[] { "Poisoned", "LastHit", "PoisonLastHit"})));
                farm.AddItem(new MenuItem("QMinHit", "Min Minion to Q", true).SetValue(new Slider(3, 1, 6)));
                farm.AddItem(new MenuItem("WMinHit", "Min Minion to W", true).SetValue(new Slider(3, 1, 6)));
                ManaManager.AddManaManagertoMenu(farm, "LaneClear", 50);
                menu.AddSubMenu(farm);
            }

            var jungle = new Menu("Jungle", "Jungle");
            {
                jungle.AddItem(new MenuItem("UseQJungle", "Use Q", true).SetValue(true));
                jungle.AddItem(new MenuItem("UseWJungle", "Use W", true).SetValue(true));
                jungle.AddItem(new MenuItem("UseEJungle", "Use E", true).SetValue(true));
                menu.AddSubMenu(jungle);
            }

            var miscMenu = new Menu("Misc", "Misc");
            {
                //aoe
                miscMenu.AddSubMenu(AoeSpellManager.AddHitChanceMenuCombo(true, true, false, true));
                miscMenu.AddItem(new MenuItem("UseGap", "Use E for GapCloser", true).SetValue(false));
                miscMenu.AddItem(new MenuItem("UseInt", "Use R to Interrupt", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("smartKS", "Smart KS", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("disableAA", "Disable AA on Combo if spells are Ready", true).SetValue(true));
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
                drawMenu.AddItem(new MenuItem("FlashUltNotification", "Ult Flash Killable Notification", true).SetValue(true));

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
                customMenu.AddItem(myCust.AddToMenu("JungleClear Active: ", "Jungle"));
                customMenu.AddItem(myCust.AddToMenu("LastHitE Active: ", "LastHitE"));
                customMenu.AddItem(myCust.AddToMenu("Ult help Active: ", "forceUlt"));
                customMenu.AddItem(myCust.AddToMenu("Ult Flash Active: ", "flashUlt"));
                customMenu.AddItem(myCust.AddToMenu("AOE Ult Active: ", "aoeUltOnly"));
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

            if (E.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.E) * 2;

            if (R.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R);

            comboDamage = ItemManager.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target));
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
            var magicTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (source == "Harass" && !ManaManager.HasMana("Harass"))
                return;

            //items
            if (source == "Combo")
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (target.IsValidTarget(Q.Range))
                {


                    var dmg = GetComboDamage(target);
                    ItemManager.Target = target;

                    //see if killable
                    if (dmg > target.Health - 50)
                        ItemManager.KillableTarget = true;

                    ItemManager.UseTargetted = true;
                }
            }

            if (useR && R.IsReady())
                Cast_R(source);
            if (useE && E.IsReady())
                Cast_E();
            if (useQ && Q.IsReady() && !magicTarget.HasBuffOfType(BuffType.Poison))
            {
                Q.CastIfHitchanceEquals(magicTarget, HitChance.High);
                return;
            }
            if (useW && W.IsReady() && !magicTarget.HasBuffOfType(BuffType.Poison))
            {
                if (!Q.IsReady())
                {
                    W.CastIfHitchanceEquals(magicTarget, HitChance.VeryHigh);
                    return;
                }
            }
        }

        protected override void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (!menu.Item("disableAA", true).GetValue<bool>() || !(args.Target is Obj_AI_Hero))
                return;

            if (Q.IsReady() || W.IsReady() || (E.IsReady() && _poisonTargets.Any(x => x.NetworkId == args.Target.NetworkId)))
                args.Process = false;
            else
                args.Process = true;
        }

        private float PoisonDuration(Obj_AI_Base target)
        {
            return target.Buffs.Where(x => x.Type == BuffType.Poison).Select(buff => buff.EndTime - Game.Time).FirstOrDefault();
        }

        private void Cast_E()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (target.IsValidTarget(E.Range))
            {

                if (Player.GetSpellDamage(target, SpellSlot.E) - 20 > target.Health)
                {
                    E.Cast(target);
                    return;
                }

                if (PoisonDuration(target) > E.Delay)
                {
                    E.Cast(target);
                    return;
                }
            }

            AutoEPoisonTargets();
        }

        private float _lastFlash;
        private Vector3 _flashVec;
        private void FlashUlt()
        {
            if (!SummonerManager.Flash_Ready() || Utils.TickCount - _lastFlash < 500)
                return;

            var vec = Player.ServerPosition.Extend(Game.CursorPos, R.Range + 400);

            var count = HeroManager.Enemies.Count(x => x.IsValidTarget() && R2.WillHit(x, vec));

            if (count == 0)
                return;

            Player.Spellbook.CastSpell(SpellSlot.R, vec);
            _flashVec = vec;
            _lastFlash = Utils.TickCount;
        }

        private bool QSuccessfullyCasted(Obj_AI_Base target)
        {
            if (!menu.Item("OnlyWIfnotPoison", true).GetValue<bool>())
                return true;

            if (target.HasBuffOfType(BuffType.Poison))
                return false;

            return Utils.TickCount - Q.LastCastAttemptT > 800 + Game.Ping;
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (!unit.IsMe) return;

            SpellSlot castedSlot = ObjectManager.Player.GetSpellSlot(args.SData.Name);

            if (castedSlot == SpellSlot.Q)
            {
                Q.LastCastAttemptT = Utils.TickCount; 
            }

            if (castedSlot == SpellSlot.E)
            {
                _lastE = Utils.TickCount;
            }

            if (castedSlot == SpellSlot.R)
            {
                if(Utils.TickCount - _lastFlash < 500 && _lastFlash > 0)
                    SummonerManager.UseFlash(_flashVec);
            }
        }

        private void AutoEPoisonTargets()
        {
            if (!E.IsReady())
                return;

            if (_poisonTargets.Count > 0)
            {
                var target = _poisonTargets.Where(x => x.IsValidTarget(E.Range)).Where(x => PoisonDuration(x) > E.Delay).OrderByDescending(GetComboDamage).FirstOrDefault();

                if(target != null)
                    E.Cast(target);
            }
        }

        private readonly List<Obj_AI_Hero> _poisonTargets = new List<Obj_AI_Hero>();

        protected override void ObjAiBaseOnOnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if (sender.IsMe || !(sender is Obj_AI_Hero) || !sender.IsEnemy)
                return;

            if (args.Buff.Type == BuffType.Poison)
            {
                _poisonTargets.Add((Obj_AI_Hero)sender);
                Console.WriteLine("Added: " + _poisonTargets.Count);
            }
        }

        protected override void ObjAiBaseOnOnBuffRemove(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args)
        {
            if (sender.IsMe || !(sender is Obj_AI_Hero) || !sender.IsEnemy)
                return;
            if (args.Buff.Type == BuffType.Poison)
            {
                _poisonTargets.RemoveAll(x => x.NetworkId == sender.NetworkId && x.Buffs.Count(y => y.Type == BuffType.Poison) == 1);
                Console.WriteLine("Added: " + _poisonTargets.Count);
            }
        }

        private void ImmobileCast()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target == null || !Q.IsReady())
                return;

            if (Q.GetPrediction(target).Hitchance == HitChance.Immobile &&
                menu.Item("Auto_Q_Immobile", true).GetValue<bool>())
            {
                Q.Cast(target);
                return;
            }

            if (Q.GetPrediction(target).Hitchance == HitChance.Dashing &&
                menu.Item("Auto_Q_Dashing", true).GetValue<bool>())
            {
                Q.Cast(target);
            }
        }

        private void Farm()
        {
            if (!ManaManager.HasMana("LaneClear"))
                return;

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ)
            {
                var min = menu.Item("QMinHit", true).GetValue<Slider>().Value;
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);

                var pred = Q.GetCircularFarmLocation(minionQ, 120);

                if (pred.MinionsHit >= min)
                    Q.Cast(pred.Position);
            }
            if (useW)
            {
                var min = menu.Item("WMinHit", true).GetValue<Slider>().Value;
                var minionW = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.NotAlly);

                var pred = W.GetCircularFarmLocation(minionW, 200);

                if (pred.MinionsHit >= min)
                    W.Cast(pred.Position);
            }
            if (useE)
            {
                var mode = menu.Item("EMode", true).GetValue<StringList>().SelectedIndex;
                var minions = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

                if (minions.Count == 0)
                    return;

                if (mode == 0)
                {
                    var minion = minions.FirstOrDefault(x => PoisonDuration(x) > E.Delay);

                    if (minion != null)
                        E.Cast(minion);
                }
                else if(mode == 1)
                {
                    foreach (var x in minions)
                    {
                        var healthPred = HealthPrediction.GetHealthPrediction(x, (int)Player.Distance(x), Game.Ping +  200);

                        if (healthPred <= Player.GetSpellDamage(x, SpellSlot.E))
                            E.Cast(x);
                    }
                }
                else if (mode == 2)
                {
                    foreach (var x in minions.Where(x => PoisonDuration(x) > E.Delay))
                    {
                        var healthPred = HealthPrediction.GetHealthPrediction(x, (int)Player.Distance(x), Game.Ping + 200);

                        if (healthPred <= Player.GetSpellDamage(x, SpellSlot.E))
                            E.Cast(x);
                    }
                }
            }
        }

        private void Jungle()
        {
            var useQ = menu.Item("UseQJungle", true).GetValue<bool>();
            var useW = menu.Item("UseWJungle", true).GetValue<bool>();
            var useE = menu.Item("UseEJungle", true).GetValue<bool>();

            var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral);

            var minionW = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.Neutral);


            var minions = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral);

            if (useQ && minionQ.Count > 0)
            {
                var pred = Q.GetCircularFarmLocation(minionQ, 120);

                Q.Cast(pred.Position);
            }
            if (useW && minionW.Count > 0)
            {
                var pred = W.GetCircularFarmLocation(minionW, 200);

                W.Cast(pred.Position);
            }
            if (useE && minions.Count > 0)
            {
                if (minions.Count == 0)
                    return;

                var minion = minions.FirstOrDefault(x => PoisonDuration(x) > E.Delay);

                if (minion != null)
                    E.Cast(minion);
            }
        }

        private void LastHit()
        {
            if (!E.IsReady())
                return;

            var minions = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (minions.Count == 0)
                return;

            foreach (var x in from x in minions let healthPred = HealthPrediction.GetHealthPrediction(x, (int)Player.Distance(x), Game.Ping + 200) where healthPred <= Player.GetSpellDamage(x, SpellSlot.E) select x)
            {
                E.Cast(x);
            }
        }

        private void Cast_R(string source)
        {
            if (menu.Item("aoeUltOnly", true).GetValue<KeyBind>().Active)
                return;

            foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range)).OrderByDescending(GetComboDamage))
            {
                if (menu.Item("Dont_R" + target.CharData.BaseSkinName, true) != null)
                {
                    if (!menu.Item("Dont_R" + target.CharData.BaseSkinName, true).GetValue<bool>())
                    {
                        if (menu.Item("overKillCheck", true).GetValue<bool>())
                        {
                            if (Player.GetSpellDamage(target, SpellSlot.Q) +
                                Player.GetSpellDamage(target, SpellSlot.E)*2 > target.Health)
                                continue;
                        }
                        //if killable
                        if (menu.Item("KillableCombo", true).GetValue<bool>())
                        {
                            if (GetComboDamage(target) > target.Health && R.GetPrediction(target).Hitchance >= HitChanceManager.GetRHitChance(source))
                            {
                                if (ShouldR(target))
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

        private void AoeStun()
        {
            var minHit = menu.Item("AOEStun", true).GetValue<Slider>().Value;

            foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range)))
            {
                var pred = R.GetPrediction(target, true);

                var enemyHit = pred.AoeTargetsHit.Where(x => x.IsValidTarget(R.Range));

                var count = enemyHit.Count(x => x.IsFacing(Player));

                if (count >= minHit)
                {
                    R.Cast(target);
                    return;
                }
            }
        }
        private bool ShouldR(Obj_AI_Hero target)
        {
            return !menu.Item("faceCheck", true).GetValue<bool>() || target.IsFacing(Player);
        }

        private void CheckKs()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Q.Range)).OrderByDescending(GetComboDamage))
            {
                //Q+E
                if (Player.ServerPosition.Distance(target.ServerPosition) <= E.Range && Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E) > target.Health && Q.IsReady() && E.IsReady())
                {
                    Q.Cast(target);
                    E.Cast(target);
                    return;
                }

                //Q+W
                if (Player.ServerPosition.Distance(target.ServerPosition) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.W) > target.Health && Q.IsReady() && W.IsReady())
                {
                    Q.Cast(target);
                    W.Cast(target);
                    return;
                }

                //W+E
                if (Player.ServerPosition.Distance(target.ServerPosition) <= E.Range && Player.GetSpellDamage(target, SpellSlot.W) + Player.GetSpellDamage(target, SpellSlot.E) > target.Health && W.IsReady() && E.IsReady() && QSuccessfullyCasted(target))
                {
                    W.Cast(target);
                    E.Cast(target);
                    return;
                }

                //Q
                if (Player.ServerPosition.Distance(target.ServerPosition) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && Q.IsReady())
                {
                    Q.Cast(target);
                    return;
                }

                //W
                if (Player.ServerPosition.Distance(target.ServerPosition) <= W.Range && Player.GetSpellDamage(target, SpellSlot.W) > target.Health && W.IsReady())
                {
                    W.Cast(target);
                    return;
                }

                //E
                if (Player.ServerPosition.Distance(target.ServerPosition) <= E.Range && Player.GetSpellDamage(target, SpellSlot.E) > target.Health && E.IsReady())
                {
                    E.Cast(target);
                    return;
                }
            }
        }

        private int _lastE;

        protected override void SpellbookOnOnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.E && menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                if (Utils.TickCount - _lastE < menu.Item("E_Delay", true).GetValue<Slider>().Value)
                {
                    args.Process = false;
                }
            }

            if (args.Slot != SpellSlot.R || !menu.Item("blockR", true).GetValue<bool>() || menu.Item("flashUlt", true).GetValue<KeyBind>().Active)
                return;

            var count = HeroManager.Enemies.Count(x => x.IsValidTarget() && R.WillHit(x, R.GetPrediction(x, true).CastPosition));

            if (!menu.Item("forceUlt", true).GetValue<KeyBind>().Active && !menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
                count = HeroManager.Enemies.Count(x => x.IsValidTarget() && R.WillHit(x, Game.CursorPos));

            if (count == 0)
                args.Process = false;
        }

        private void ForceUlt()
        {
            if (R.IsReady())
                return;

            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget(R.Range))
                return;

            if (target.IsFacing(Player) || !menu.Item("faceCheckHelper", true).GetValue<bool>())
                R.Cast(target);
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            if (menu.Item("smartKS", true).GetValue<bool>())
                CheckKs();

            AoeStun();

            if (menu.Item("forceUlt", true).GetValue<KeyBind>().Active)
            {
                OrbwalkManager.Orbwalk(null, Game.CursorPos);
                ForceUlt();
            }
            else if (menu.Item("flashUlt", true).GetValue<KeyBind>().Active)
            {
                OrbwalkManager.Orbwalk(null, Game.CursorPos);
                FlashUlt();
            }
            else if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("E_Poison", true).GetValue<bool>())
                    AutoEPoisonTargets();

                if (menu.Item("LastHitE", true).GetValue<KeyBind>().Active)
                    LastHit();

                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("Jungle", true).GetValue<KeyBind>().Active)
                    Jungle();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();
            }

            ImmobileCast();
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!menu.Item("UseInt", true).GetValue<bool>()) return;
            
            if (sender.IsValidTarget())
            {
                if (Player.Distance(sender) <= R.Range && sender.IsFacing(Player))
                {
                    var pred = R.GetPrediction(sender);
                    if (pred.Hitchance >= HitChance.VeryHigh && sender.IsFacing(Player))
                    {
                        R.Cast(pred.CastPosition);
                    }
                }
            }
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Player.IsRecalling() || !menu.Item("UseGap", true).GetValue<bool>())
                return;

            if (R.IsReady() && gapcloser.Sender.IsValidTarget(R.Range) && Player.IsFacing(gapcloser.Sender))
                R.Cast(gapcloser.Sender);
        }

        private int _lastNotification;

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
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("FlashUltNotification", true).GetValue<bool>() && SummonerManager.Flash_Ready() && R.IsReady())
            {
                var enemy = HeroManager.Enemies.Where(x => R.IsKillable(x)).ToList();

                if (!enemy.Any())
                    return;

                foreach (var x in enemy.Where(x => x.IsValidTarget(R2.Range)))
                {
                    if (Utils.TickCount - _lastNotification > 0)
                    {
                        Notifications.AddNotification(x.CharData.BaseSkinName + " Flash Ult Killable", 5000);
                        _lastNotification = Utils.TickCount + 5000;
                    }
                }
            }
        }
    }
}
