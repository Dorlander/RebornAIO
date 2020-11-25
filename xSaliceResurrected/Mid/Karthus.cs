using System;
using System.CodeDom;
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
    class Karthus : Champion
    {
        public Karthus()
        {
            LoadSpell();
            LoadMenu();
        }

        private const int QWidth = 200;
        private static int _lastNotification;

        private void LoadSpell()
        {
            SpellManager.Q = new Spell(SpellSlot.Q, 875);
            SpellManager.W = new Spell(SpellSlot.W, 1000);
            SpellManager.E = new Spell(SpellSlot.E, 520);
            SpellManager.R = new Spell(SpellSlot.R);

            Q.SetSkillshot(.5f, 190f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 50f, 1600f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(3f, float.MaxValue, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private void LoadMenu()
        {
            //Keys
            var keys = new Menu("Keys", "Keys");
            {
                keys.AddItem(new MenuItem("ComboActive", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                keys.AddItem(new MenuItem("HarassActive", "Harass!", true).SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
                keys.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!", true).SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));
                keys.AddItem(new MenuItem("wTar", "Cast W On Selected", true).SetValue(new KeyBind("W".ToCharArray()[0], KeyBindType.Press)));
                keys.AddItem(new MenuItem("lastHitQ", "Last Hith Q", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                keys.AddItem(new MenuItem("LaneClearActive", "Farm!", true).SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
                menu.AddSubMenu(keys);
            }

            //Spell Menu
            var spells = new Menu("Spell", "Spell");
            {
                //Q Menu
                var qMenu = new Menu("QSpell", "QSpell");
                {
                    qMenu.AddItem(new MenuItem("qImmo", "Auto Q Immobile", true).SetValue(true));
                    qMenu.AddItem(new MenuItem("qDash", "Auto Q Dashing", true).SetValue(true));
                    spells.AddSubMenu(qMenu);
                }

                //W
                var wMenu = new Menu("WSpell", "WSpell");
                {
                    wMenu.AddItem(new MenuItem("wTower", "Auto W Enemy in Tower", true).SetValue(true));
                    ManaManager.AddManaManagertoMenu(wMenu, "WMana", 30);
                    spells.AddSubMenu(wMenu);
                }

                //E
                var eMenu = new Menu("ESpell", "ESpell");
                {
                    eMenu.AddItem(new MenuItem("eManaCombo", "Min Mana Combo", true).SetValue(new Slider(10)));
                    eMenu.AddItem(new MenuItem("eManaHarass", "Min Mana Harass", true).SetValue(new Slider(70)));
                    eMenu.AddItem(new MenuItem("EDelay", "E Delay Before Turning Off (Milliseconds)", true).SetValue(new Slider(100, 0, 2000)));
                    spells.AddSubMenu(eMenu);
                }

                //R
                var rMenu = new Menu("RSpell", "RSpell");
                {
                    rMenu.AddItem(new MenuItem("rPing", "Ping if Enemy Is Killable", true).SetValue(true));
                    spells.AddSubMenu(rMenu);
                }

                menu.AddSubMenu(spells);
            }

            //Combo menu:
            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "Use Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "Use W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E", true).SetValue(true));
                combo.AddSubMenu(HitChanceManager.AddHitChanceMenuCombo(true, true, false, false));
                menu.AddSubMenu(combo);
            }

            //Harass menu:
            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "Use W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                ManaManager.AddManaManagertoMenu(harass, "Harass", 30);
                harass.AddSubMenu(HitChanceManager.AddHitChanceMenuHarass(true, true, false, false));
                menu.AddSubMenu(harass);
            }

            //Farming menu:
            var farm = new Menu("Farm", "Farm");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(false));
                farm.AddItem(new MenuItem("UseEFarm", "Use E", true).SetValue(false));
                ManaManager.AddManaManagertoMenu(harass, "Farm", 30);
                menu.AddSubMenu(farm);
            }

            //Misc Menu:
            var misc = new Menu("Misc", "Misc");
            {
                misc.AddSubMenu(AoeSpellManager.AddHitChanceMenuCombo(true, false, false, false));
                misc.AddItem(new MenuItem("UseGap", "Use W for GapCloser", true).SetValue(true));
                misc.AddItem(new MenuItem("smartKS", "Use Smart KS System", true).SetValue(true));
                menu.AddSubMenu(misc);
            }

            //Drawings menu:
            var drawMenu = new Menu("Drawings", "Drawings");
            {
                drawMenu.AddItem(new MenuItem("QRange", "Q range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawMenu.AddItem(new MenuItem("WRange", "W range", true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                drawMenu.AddItem(new MenuItem("ERange", "E range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawMenu.AddItem(new MenuItem("RRange", "R range", true).SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                drawMenu.AddItem(new MenuItem("drawUlt", "Killable With ult", true).SetValue(true));
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
                customMenu.AddItem(myCust.AddToMenu("Laneclear Active: ", "LaneClearActive"));
                customMenu.AddItem(myCust.AddToMenu("W Target Active: ", "wTar"));
                customMenu.AddItem(myCust.AddToMenu("Lasthit Q Active: ", "lastHitQ"));
                menu.AddSubMenu(customMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            if (!enemy.IsValidTarget())
                return 0;

            double damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q) * 2;

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E) * 2;

            if (R.IsReady())
                damage += GetUltDmg((Obj_AI_Hero)enemy);

            damage = ItemManager.CalcDamage(enemy, damage);

            return (float)damage;
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>(), "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, string source)
        {
            if (source == "Harass" && !ManaManager.HasMana("Harass"))
                return;

            if (source == "Combo")
            {
                var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                if (itemTarget != null)
                {
                    ItemManager.Target = itemTarget;

                    float dmg = GetComboDamage(itemTarget);

                    //see if killable
                    if (dmg > itemTarget.Health - 50)
                        ItemManager.KillableTarget = true;

                    ItemManager.UseTargetted = true;
                }
            }

            //W
            if (useW && W.IsReady())
            {
                if (ShouldW())
                    SpellCastManager.CastBasicSkillShot(W, W.Range, TargetSelector.DamageType.Magical,
                        HitChanceManager.GetWHitChance(source));
            }

            //Q
            if (useQ && Q.IsReady())
            {
                SpellCastManager.CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Magical,
                    HitChanceManager.GetQHitChance(source));
            }

            //E
            if (useE && E.IsReady() && ESpell.ToggleState == 1 && HasManaForE(source) &&
                Utils.TickCount - E.LastCastAttemptT > 500)
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

                if (!target.IsValidTarget(E.Range))
                    return;

                E.Cast();
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (!unit.IsMe)
                return;

            SpellSlot castedSlot = ObjectManager.Player.GetSpellSlot(args.SData.Name);

            if (castedSlot == SpellSlot.E)
            {
                E.LastCastAttemptT = Utils.TickCount;
            }
        }

        private bool ShouldW()
        {
            if (ManaManager.HasMana("WMana"))
                return true;

            return false;
        }

        private float GetUltDmg(Obj_AI_Hero target)
        {
            double dmg = 0;

            dmg += Player.GetSpellDamage(target, SpellSlot.R);

            dmg -= target.HPRegenRate * 3.25;

            if (Items.HasItem(3155, target))
            {
                dmg = dmg - 250;
            }

            if (Items.HasItem(3156, target))
            {
                dmg = dmg - 400;
            }

            return (float)dmg;
        }

        private void DrawEnemyKillable()
        {
            int kill = 0;

            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(
                            x => x.IsValidTarget()))
            {
                if (GetUltDmg(enemy) > enemy.Health - 30)
                {
                    if (menu.Item("rPing", true).GetValue<bool>() && Utils.TickCount - _lastNotification > 5000)
                    {
                        if (Utils.TickCount - _lastNotification > 0)
                        {
                            Notifications.AddNotification(enemy.CharData.BaseSkinName + " Is Killable!", 500);
                            _lastNotification = Utils.TickCount + 5000;
                        }

                    }
                    kill++;
                }
            }

            if (kill > 0)
            {
                Vector2 wts = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(wts[0] - 100, wts[1], Color.Red, "Killable with R: " + kill);
            }
            else
            {
                Vector2 wts = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(wts[0] - 100, wts[1], Color.White, "Killable with R: " + kill);
            }
        }


        private bool HasManaForE(string source)
        {
            var eManaCombo = menu.Item("eManaCombo", true).GetValue<Slider>().Value;
            var eManaHarass = menu.Item("eManaHarass", true).GetValue<Slider>().Value;

            if (source == "Combo" && Player.ManaPercent > eManaCombo)
                return true;

            if (source == "Harass" && Player.ManaPercent > eManaHarass)
                return true;

            return false;
        }

        private void AutoQ()
        {
            var qDashing = menu.Item("qImmo", true).GetValue<bool>();
            var qImmo = menu.Item("qDash", true).GetValue<bool>();

            if (!Q.IsReady())
                return;

            if (!qDashing && !qImmo)
                return;

            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Q.Range)))
            {
                if ((Q.GetPrediction(target).Hitchance == HitChance.Immobile || IsStunned(target)) && qImmo && Player.Distance(target.Position) < Q.Range)
                {
                    SpellCastManager.CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Magical, HitChanceManager.GetQHitChance("Combo"));
                    return;
                }

                if (Q.GetPrediction(target).Hitchance == HitChance.Dashing && qDashing && Player.Distance(target.Position) < Q.Range)
                {
                    SpellCastManager.CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Magical, HitChanceManager.GetQHitChance("Combo"));
                }
            }
        }

        private bool IsStunned(Obj_AI_Base target)
        {
            if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) ||
                target.HasBuffOfType(BuffType.Suppression) || target.HasBuffOfType(BuffType.Taunt))
                return true;

            return false;
        }

        private void CheckUnderTower()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.Distance(x.Position) < W.Range && x.IsValidTarget(W.Range) && !x.IsDead && x.IsEnemy && x.IsVisible))
            {
                if (ObjectManager.Get<Obj_AI_Turret>().Where(turret => turret != null && turret.IsValid && turret.IsAlly && turret.Health > 0).Any(turret => Vector2.Distance(enemy.Position.To2D(), turret.Position.To2D()) < 750 && W.IsReady()))
                {
                    var vec = enemy.ServerPosition +
                              Vector3.Normalize(enemy.ServerPosition - Player.ServerPosition) * 100;

                    W.Cast(vec);
                    return;
                }
            }
        }

        private void SmartKs()
        {
            if (!menu.Item("smartKS", true).GetValue<bool>())
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Q.Range) && !x.HasBuffOfType(BuffType.Invulnerability)).OrderByDescending(GetComboDamage))
            {
                //Q
                if (Player.Distance(target.ServerPosition) <= Q.Range &&
                    (Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 30)
                {
                    if (Q.IsReady())
                    {
                        Q.Cast(target);
                        return;
                    }
                }

                //E
                if (Player.Distance(target.ServerPosition) <= E.Range && ESpell.ToggleState == 1 &&
                    (Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 30)
                {
                    if (E.IsReady())
                    {
                        E.Cast();
                        return;
                    }
                }

            }
        }

        private int _lastE;

        private void CheckEState()
        {
            if (ESpell.ToggleState == 1 || Utils.TickCount - _lastE < menu.Item("EDelay", true).GetValue<Slider>().Value)
                return;

            var target = ObjectManager.Get<Obj_AI_Hero>().Count(x => x.IsValidTarget(E.Range));

            //return if Target in range
            if (target > 0)
                return;

            //check if around minion
            if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
            {
                var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range,
                MinionTypes.All, MinionTeam.NotAlly);

                if (allMinionsE.Count > 0)
                    return;
            }

            if (Utils.TickCount - (_lastE + 250) < menu.Item("EDelay", true).GetValue<Slider>().Value)
                E.Cast();

            if (E.IsReady() && ESpell.ToggleState != 1)
            {
                _lastE = Utils.TickCount;
            }
        }


        protected override void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsChannelingImportantSpell())
                return;

            SmartKs();

            AutoQ();

            CheckEState();

            if (menu.Item("wTower", true).GetValue<bool>())
                CheckUnderTower();

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {

                if (menu.Item("lastHitQ", true).GetValue<KeyBind>().Active)
                    LastHitQ();

                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();
            }

            if (menu.Item("wTar", true).GetValue<KeyBind>().Active)
            {
                Obj_AI_Hero target = null;

                if (TargetSelector.GetSelectedTarget() != null)
                    target = TargetSelector.GetSelectedTarget();

                if (target != null && target.IsEnemy && target.Type == GameObjectType.obj_AI_Hero)
                {
                    if (W.GetPrediction(target).Hitchance >= HitChance.High)
                        W.Cast(target);
                }
            }
        }

        private void LastHitQ()
        {
            if (!Q.IsReady())
                return;

            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                Q.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (allMinionsQ.Count > 0)
            {
                foreach (var minion in allMinionsQ)
                {
                    var health = HealthPrediction.GetHealthPrediction(minion, 700);

                    var qPred = Q.GetCircularFarmLocation(allMinionsQ, 210);

                    if (qPred.MinionsHit == 1)
                    {
                        if (Player.GetSpellDamage(minion, SpellSlot.Q) - 15 > health)
                            Q.Cast(minion);
                    }
                    else
                    {
                        if (Player.GetSpellDamage(minion, SpellSlot.Q, 1) - 15 > health)
                            Q.Cast(minion);
                    }
                }
            }
        }

        private void Farm()
        {
            if (!ManaManager.HasMana("Farm"))
                return;

            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range,
                MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ && Q.IsReady())
            {
                SpellCastManager.CastBasicFarm(Q);
            }

            if (useE && allMinionsE.Count > 0 && E.IsReady() && ESpell.ToggleState == 1)
            {
                MinionManager.FarmLocation ePos = E.GetCircularFarmLocation(allMinionsE);

                if (ePos.MinionsHit > 1)
                    E.Cast();
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

            if (R.IsReady() && menu.Item("drawUlt", true).GetValue<bool>())
                DrawEnemyKillable();
        }


        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap", true).GetValue<bool>()) return;

            if (W.IsReady() && gapcloser.Sender.IsValidTarget(W.Range))
                W.Cast(gapcloser.Sender);
        }
    }
}
