using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace ShineCommon
{
    public class LXOrbwalker
    {
        private static readonly string[] AttackResets = { "meditate", "dariusnoxiantacticsonh", "fioraflurry", "garenq", "hecarimrapidslash", "jaxempowertwo", "jaycehypercharge", "leonashieldofdaybreak", "luciane", "lucianq", "monkeykingdoubleattack", "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze", "netherblade", "parley", "poppydevastatingblow", "powerfist", "renektonpreexecute", "rengarq", "shyvanadoubleattack", "sivirw", "takedown", "talonnoxiandiplomacy", "trundletrollsmash", "vaynetumble", "vie", "volibearq", "xenzhaocombotarget", "yorickspectral" };
        private static readonly string[] NoAttacks = { "jarvanivcataclysmattack", "monkeykingdoubleattack", "shyvanadoubleattack", "shyvanadoubleattackdragon", "zyragraspingplantattack", "zyragraspingplantattack2", "zyragraspingplantattackfire", "zyragraspingplantattack2fire" };
        private static readonly string[] Attacks = { "caitlynheadshotmissile", "frostarrow", "garenslash2", "kennenmegaproc", "lucianpassiveattack", "masteryidoublestrike", "quinnwenhanced", "renektonexecute", "renektonsuperexecute", "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust", "viktorqbuff", "xenzhaothrust2", "xenzhaothrust3" };


        public static Menu Menu;
        public static Obj_AI_Hero MyHero = ObjectManager.Player;
        public static Obj_AI_Base ForcedTarget = null;
        public static IEnumerable<Obj_AI_Hero> AllEnemys = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy);
        public static IEnumerable<Obj_AI_Hero> AllAllys = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly);
        public static bool CustomOrbwalkMode;

        public delegate void BeforeAttackEvenH(BeforeAttackEventArgs args);
        public delegate void OnTargetChangeH(Obj_AI_Base oldTarget, Obj_AI_Base newTarget);
        public delegate void AfterAttackEvenH(Obj_AI_Base unit, Obj_AI_Base target);
        public delegate void OnAttackEvenH(Obj_AI_Base unit, Obj_AI_Base target);

        public static event BeforeAttackEvenH BeforeAttack;
        public static event OnTargetChangeH OnTargetChange;
        public static event AfterAttackEvenH AfterAttack;
        public static event OnAttackEvenH OnAttack;

        public enum Mode
        {
            Combo,
            Harass,
            LaneClear,
            LaneFreeze,
            Lasthit,
            Flee,
            None,
        }

        private static bool _drawing = true;
        private static bool _attack = true;
        private static bool _movement = true;
        private static bool _disableNextAttack;
        private const float LaneClearWaitTimeMod = 2f;
        private static int _lastAATick;
        private static Obj_AI_Base _lastTarget;
        private static Spell _movementPrediction;
        private static int _lastMovement;

        public static float cantMoveTill = 0;

        public static void Disable()
        {
            Drawing.OnDraw -= OnDraw;
            Game.OnUpdate -= OnUpdate;
            Obj_AI_Base.OnProcessSpellCast -= OnProcessSpell;
            GameObject.OnCreate -= MissileClient_OnCreate;
        }

        public static void Enable()
        {
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            GameObject.OnCreate += MissileClient_OnCreate;
        }

        private static double GetAADamage(Obj_AI_Base target)
        {
            double dmg = MyHero.GetAutoAttackDamage(target);

            // BotRK
            if (Items.HasItem(3153, MyHero))
            {
                var d = 0.08 * target.Health;
                if (target is Obj_AI_Minion)
                    d = Math.Min(d, 60);

                dmg += d;
            }

            

            if (MyHero.ChampionName.ToLower() == "orianna")
            {
                float[] orianna_passive = new float[] { 10, 10, 10, 18, 18, 18, 26, 26, 26, 34, 34, 34, 42, 42, 42, 50, 50, 50 };
                if (MyHero.HasBuff("orianaspellsword"))
                {
                    int count = MyHero.GetBuff("orianaspellsword").Count;
                    if (_lastTarget == target)
                    {
                        if (count < 2)
                            count++;
                    }
                    else
                        count = 0;

                    dmg += MyHero.CalcDamage(target, Damage.DamageType.Magical, (0.15 * MyHero.FlatMagicDamageMod + orianna_passive[MyHero.Level - 1]) * (1 + count * 0.2));
                }
                else
                    dmg += MyHero.CalcDamage(target, Damage.DamageType.Magical, 0.15 * MyHero.FlatMagicDamageMod + orianna_passive[MyHero.Level - 1]);
            }


            return dmg;
        }

        public static void AddToMenu(Menu menu)
        {
            _movementPrediction = new Spell(SpellSlot.Unknown, GetAutoAttackRange());
            _movementPrediction.SetTargetted(MyHero.BasicAttack.SpellCastTime, MyHero.BasicAttack.MissileSpeed);

            Menu = menu;

            var menuDrawing = new Menu("Drawing", "lxOrbwalker_Draw");
            menuDrawing.AddItem(new MenuItem("lxOrbwalker_Draw_AARange", "AA Circle").SetValue(new Circle(true, Color.FloralWhite)));
            menuDrawing.AddItem(new MenuItem("lxOrbwalker_Draw_AARange_Enemy", "AA Circle Enemy").SetValue(new Circle(true, Color.Pink)));
            menuDrawing.AddItem(new MenuItem("lxOrbwalker_Draw_Holdzone", "Holdzone").SetValue(new Circle(true, Color.FloralWhite)));
            menuDrawing.AddItem(new MenuItem("lxOrbwalker_Draw_MinionHPBar", "Minion HPBar").SetValue(new Circle(true, Color.Black)));
            menuDrawing.AddItem(new MenuItem("lxOrbwalker_Draw_MinionHPBar_thickness", "^ HPBar Thickness").SetValue(new Slider(1, 1, 3)));
            menuDrawing.AddItem(new MenuItem("lxOrbwalker_Draw_hitbox", "Show HitBoxes").SetValue(new Circle(true, Color.FloralWhite)));
            menuDrawing.AddItem(new MenuItem("lxOrbwalker_Draw_Lasthit", "Minion Lasthit").SetValue(new Circle(true, Color.Lime)));
            menuDrawing.AddItem(new MenuItem("lxOrbwalker_Draw_nearKill", "Minion nearKill").SetValue(new Circle(true, Color.Gold)));
            menu.AddSubMenu(menuDrawing);

            var menuMisc = new Menu("Misc", "lxOrbwalker_Misc");
            menuMisc.AddItem(new MenuItem("lxOrbwalker_Misc_Holdzone", "Hold Position").SetValue(new Slider(50, 100, 0)));
            menuMisc.AddItem(new MenuItem("lxOrbwalker_Misc_Farmdelay", "Farm Delay").SetValue(new Slider(0, 200, 0)));
            menuMisc.AddItem(new MenuItem("lxOrbwalker_Misc_ExtraWindUp", "Extra Winduptime").SetValue(new Slider(80, 200, 0)));
            menuMisc.AddItem(new MenuItem("lxOrbwalker_Misc_AutoWindUp", "Autoset Windup").SetValue(false));
            menuMisc.AddItem(new MenuItem("lxOrbwalker_Misc_Priority_Unit", "Priority Unit").SetValue(new StringList(new[] { "Minion", "Hero" })));
            menuMisc.AddItem(new MenuItem("lxOrbwalker_Misc_Humanizer", "Humanizer Delay").SetValue(new Slider(50, 100, 0)));
            menuMisc.AddItem(new MenuItem("lxOrbwalker_Misc_AllMovementDisabled", "Disable All Movement").SetValue(false));
            menuMisc.AddItem(new MenuItem("lxOrbwalker_Misc_AllAttackDisabled", "Disable All Attacks").SetValue(false));

            menu.AddSubMenu(menuMisc);

            var menuMelee = new Menu("Melee", "lxOrbwalker_Melee");
            menuMelee.AddItem(new MenuItem("lxOrbwalker_Melee_Prediction", "Movement Prediction").SetValue(false));
            menu.AddSubMenu(menuMelee);

            var menuModes = new Menu("Orbwalk Mode", "lxOrbwalker_Modes");
            {
                var modeCombo = new Menu("Combo", "lxOrbwalker_Modes_Combo");
                modeCombo.AddItem(new MenuItem("Combo_Key", "Key").SetValue(new KeyBind(32, KeyBindType.Press)));
                modeCombo.AddItem(new MenuItem("Combo_move", "Movement").SetValue(true));
                modeCombo.AddItem(new MenuItem("Combo_attack", "Attack").SetValue(true));
                modeCombo.AddItem(new MenuItem("Move_target", "Move to target").SetValue(true));
                menuModes.AddSubMenu(modeCombo);

                var modeHarass = new Menu("Harass", "lxOrbwalker_Modes_Harass");
                modeHarass.AddItem(new MenuItem("Harass_Key", "Key").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                modeHarass.AddItem(new MenuItem("Harass_move", "Movement").SetValue(true));
                modeHarass.AddItem(new MenuItem("Harass_attack", "Attack").SetValue(true));
                modeHarass.AddItem(new MenuItem("Harass_Lasthit", "Lasthit Minions").SetValue(true));
                menuModes.AddSubMenu(modeHarass);

                var modeLaneClear = new Menu("LaneClear", "lxOrbwalker_Modes_LaneClear");
                modeLaneClear.AddItem(new MenuItem("LaneClear_Key", "Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                modeLaneClear.AddItem(new MenuItem("LaneClear_move", "Movement").SetValue(true));
                modeLaneClear.AddItem(new MenuItem("LaneClear_attack", "Attack").SetValue(true));
                menuModes.AddSubMenu(modeLaneClear);

                var modeLaneFreeze = new Menu("LaneFreeze", "lxOrbwalker_Modes_LaneFreeze");
                modeLaneFreeze.AddItem(new MenuItem("LaneFreeze_Key", "Key").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                modeLaneFreeze.AddItem(new MenuItem("LaneFreeze_move", "Movement").SetValue(true));
                modeLaneFreeze.AddItem(new MenuItem("LaneFreeze_attack", "Attack").SetValue(true));
                menuModes.AddSubMenu(modeLaneFreeze);

                var modeLasthit = new Menu("LastHit", "lxOrbwalker_Modes_LastHit");
                modeLasthit.AddItem(new MenuItem("LastHit_Key", "Key").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
                modeLasthit.AddItem(new MenuItem("LastHit_move", "Movement").SetValue(true));
                modeLasthit.AddItem(new MenuItem("LastHit_attack", "Attack").SetValue(true));
                menuModes.AddSubMenu(modeLasthit);

                var modeFlee = new Menu("Flee", "lxOrbwalker_Modes_Flee");
                modeFlee.AddItem(new MenuItem("Flee_Key", "Key").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                menuModes.AddSubMenu(modeFlee);
            }
            menu.AddSubMenu(menuModes);
        }

        private static void MissileClient_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.IsMe)
            {
                var obj = (Obj_AI_Hero)sender;
                if (obj.IsMelee())
                    return;
            }
            if (!(sender is MissileClient) || !sender.IsValid)
                return;
            var missile = (MissileClient)sender;
            if (missile.SpellCaster is Obj_AI_Hero && missile.SpellCaster.IsValid && IsAutoAttack(missile.SData.Name))
                FireAfterAttack(missile.SpellCaster, _lastTarget);
        }

        private static void OnUpdate(EventArgs args)
        {
            CheckAutoWindUp();
            if (CurrentMode == Mode.None || MenuGUI.IsChatOpen)
                return;
            var target = GetPossibleTarget();
            if (Menu.Item("Move_target").GetValue<bool>() && target != null && target is Obj_AI_Hero)
            {
                if (target.Path.Count() == 0)
                    Orbwalk(target.Position.To2D().Extend(target.Direction.To2D(), 140).To3D(), target);
                else
                    Orbwalk(target.Position.To2D().Extend(target.Path[0].To2D(), 140).To3D(), target);
            }
            else
            {
                Orbwalk(Game.CursorPos, target);
            }
        }

        public static void Orbwalk(Vector3 goalPosition, Obj_AI_Base target)
        {
            if (target != null && CanAttack() && IsAllowedToAttack())
            {
                _disableNextAttack = false;
                FireBeforeAttack(target);
                if (!_disableNextAttack)
                {
                    if (MyHero.IssueOrder(GameObjectOrder.AttackUnit, target))
                        _lastAATick = Environment.TickCount + Game.Ping / 2;
                }
            }
            if (!CanMove() || !IsAllowedToMove())
                return;
            if (MyHero.IsMelee() && target != null && target.ServerPosition.Distance(MyHero.ServerPosition) < GetAutoAttackRange(MyHero, target)
                && Menu.Item("lxOrbwalker_Melee_Prediction").GetValue<bool>() && target is Obj_AI_Hero
                && Game.CursorPos.Distance(target.Position) < 300)
            {
                _movementPrediction.Delay = MyHero.BasicAttack.SpellCastTime;
                _movementPrediction.Speed = MyHero.BasicAttack.MissileSpeed;

                MoveTo(_movementPrediction.GetPrediction(target).UnitPosition);
            }
            else
            {
                MoveTo(goalPosition);
            }

        }

        private static void MoveTo(Vector3 position, float holdAreaRadius = -1)
        {
            var delay = Menu.Item("lxOrbwalker_Misc_Humanizer").GetValue<Slider>().Value;
            if (Environment.TickCount - _lastMovement < delay)
                return;
            _lastMovement = Environment.TickCount;

            if (!CanMove())
                return;
            if (holdAreaRadius < 0)
                holdAreaRadius = Menu.Item("lxOrbwalker_Misc_Holdzone").GetValue<Slider>().Value;
            if (MyHero.ServerPosition.Distance(position) < holdAreaRadius)
            {
                if (MyHero.Path.Count() > 1)
                    MyHero.IssueOrder(GameObjectOrder.HoldPosition, MyHero.ServerPosition);
                return;
            }
            if (position.Distance(MyHero.Position) < 200)
                MyHero.IssueOrder(GameObjectOrder.MoveTo, position);
            else
            {
                var point = MyHero.ServerPosition +
                200 * (position.To2D() - MyHero.ServerPosition.To2D()).Normalized().To3D();
                MyHero.IssueOrder(GameObjectOrder.MoveTo, position);
            }

        }

        private static bool IsAllowedToMove()
        {
            if (!_movement)
                return false;
            if (Menu.Item("lxOrbwalker_Misc_AllMovementDisabled").GetValue<bool>())
                return false;
            if (CurrentMode == Mode.Combo && !Menu.Item("Combo_move").GetValue<bool>())
                return false;
            if (CurrentMode == Mode.Harass && !Menu.Item("Harass_move").GetValue<bool>())
                return false;
            if (CurrentMode == Mode.LaneClear && !Menu.Item("LaneClear_move").GetValue<bool>())
                return false;
            if (CurrentMode == Mode.LaneFreeze && !Menu.Item("LaneFreeze_move").GetValue<bool>())
                return false;
            return CurrentMode != Mode.Lasthit || Menu.Item("LastHit_move").GetValue<bool>();
        }

        private static bool IsAllowedToAttack()
        {
            if (!_attack)
                return false;
            if (Menu.Item("lxOrbwalker_Misc_AllAttackDisabled").GetValue<bool>())
                return false;
            if (CurrentMode == Mode.Combo && !Menu.Item("Combo_attack").GetValue<bool>())
                return false;
            if (CurrentMode == Mode.Harass && !Menu.Item("Harass_attack").GetValue<bool>())
                return false;
            if (CurrentMode == Mode.LaneClear && !Menu.Item("LaneClear_attack").GetValue<bool>())
                return false;
            if (CurrentMode == Mode.LaneFreeze && !Menu.Item("LaneFreeze_attack").GetValue<bool>())
                return false;
            return CurrentMode != Mode.Lasthit || Menu.Item("LastHit_attack").GetValue<bool>();

        }

        private static void OnDraw(EventArgs args)
        {
            if (!_drawing)
                return;

            if (Menu.Item("lxOrbwalker_Draw_AARange").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(MyHero.Position, GetAutoAttackRange(), Menu.Item("lxOrbwalker_Draw_AARange").GetValue<Circle>().Color);
            }

            if (Menu.Item("lxOrbwalker_Draw_AARange_Enemy").GetValue<Circle>().Active ||
                Menu.Item("lxOrbwalker_Draw_hitbox").GetValue<Circle>().Active)
            {
                foreach (var enemy in AllEnemys.Where(enemy => enemy.IsValidTarget(1500)))
                {
                    if (Menu.Item("lxOrbwalker_Draw_AARange_Enemy").GetValue<Circle>().Active)
                        Render.Circle.DrawCircle(enemy.Position, GetAutoAttackRange(enemy, MyHero), Menu.Item("lxOrbwalker_Draw_AARange_Enemy").GetValue<Circle>().Color);
                    if (Menu.Item("lxOrbwalker_Draw_hitbox").GetValue<Circle>().Active)
                        Render.Circle.DrawCircle(enemy.Position, enemy.BoundingRadius, Menu.Item("lxOrbwalker_Draw_hitbox").GetValue<Circle>().Color);
                }
            }

            if (Menu.Item("lxOrbwalker_Draw_AARange_Enemy").GetValue<Circle>().Active)
            {
                foreach (var enemy in AllEnemys.Where(enemy => enemy.IsValidTarget(1500)))
                {
                    Render.Circle.DrawCircle(enemy.Position, GetAutoAttackRange(enemy, MyHero), Menu.Item("lxOrbwalker_Draw_AARange_Enemy").GetValue<Circle>().Color);

                }
            }

            if (Menu.Item("lxOrbwalker_Draw_Holdzone").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(MyHero.Position, Menu.Item("lxOrbwalker_Misc_Holdzone").GetValue<Slider>().Value, Menu.Item("lxOrbwalker_Draw_Holdzone").GetValue<Circle>().Color);
            }

            if (Menu.Item("lxOrbwalker_Draw_MinionHPBar").GetValue<Circle>().Active ||
                Menu.Item("lxOrbwalker_Draw_Lasthit").GetValue<Circle>().Active ||
                Menu.Item("lxOrbwalker_Draw_nearKill").GetValue<Circle>().Active)
            {
                var minionList = MinionManager.GetMinions(MyHero.Position, GetAutoAttackRange() + 500, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
                foreach (var minion in minionList.Where(minion => minion.IsValidTarget(GetAutoAttackRange() + 500)))
                {
                    var attackToKill = Math.Ceiling(minion.MaxHealth / GetAADamage(minion));
                    var hpBarPosition = minion.HPBarPosition;
                    var barWidth = minion.IsMelee() ? 75 : 80;
                    if (minion.HasBuff("turretshield"))
                        barWidth = 70;
                    var barDistance = (float)(barWidth / attackToKill);
                    if (Menu.Item("lxOrbwalker_Draw_MinionHPBar").GetValue<Circle>().Active)
                    {
                        for (var i = 1; i < attackToKill; i++)
                        {
                            var startposition = hpBarPosition.X + 45 + barDistance * i;
                            Drawing.DrawLine(
                                new Vector2(startposition, hpBarPosition.Y + 18),
                                new Vector2(startposition, hpBarPosition.Y + 23),
                                Menu.Item("lxOrbwalker_Draw_MinionHPBar_thickness").GetValue<Slider>().Value,
                                Menu.Item("lxOrbwalker_Draw_MinionHPBar").GetValue<Circle>().Color);
                        }
                    }
                    if (Menu.Item("lxOrbwalker_Draw_Lasthit").GetValue<Circle>().Active &&
                        minion.Health <= GetAADamage(minion))
                        Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius, Menu.Item("lxOrbwalker_Draw_Lasthit").GetValue<Circle>().Color);
                    else if (Menu.Item("lxOrbwalker_Draw_nearKill").GetValue<Circle>().Active &&
                             minion.Health <= GetAADamage(minion) * 2)
                        Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius, Menu.Item("lxOrbwalker_Draw_nearKill").GetValue<Circle>().Color);
                }
            }
        }

        private static void OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (IsAutoAttackReset(spell.SData.Name) && unit.IsMe)
            {
                LeagueSharp.Common.Utility.DelayAction.Add(250, ResetAutoAttackTimer);
            }

            if (!IsAutoAttack(spell.SData.Name))
                return;
            if (unit.IsMe)
            {
                _lastAATick = Environment.TickCount;
                // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
                if (spell.Target is Obj_AI_Base)
                {
                    FireOnTargetSwitch((Obj_AI_Base)spell.Target);
                    _lastTarget = (Obj_AI_Base)spell.Target;
                }
                if (unit.IsMelee())
                    LeagueSharp.Common.Utility.DelayAction.Add(
                        (int)(unit.AttackCastDelay * 1000 + Game.Ping * 0.5) + 50, () => FireAfterAttack(unit, _lastTarget));
            }
            FireOnAttack(unit, _lastTarget);
        }


        public static Obj_AI_Base GetPossibleTarget()
        {
            if (ForcedTarget != null)
            {
                if (InAutoAttackRange(ForcedTarget))
                    return ForcedTarget;

                ForcedTarget = null;
            }

            Obj_AI_Base tempTarget = null;

            if (Menu.Item("lxOrbwalker_Misc_Priority_Unit").GetValue<StringList>().SelectedIndex == 1 &&
                (CurrentMode == Mode.Harass || CurrentMode == Mode.LaneClear))
            {
                tempTarget = GetBestHeroTarget();
                if (tempTarget != null)
                    return tempTarget;
            }

            if (CurrentMode == Mode.Harass || CurrentMode == Mode.Lasthit || CurrentMode == Mode.LaneClear || CurrentMode == Mode.LaneFreeze)
            {
                foreach (
                    var minion in
                        from minion in
                            ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget() && InAutoAttackRange(minion))
                        let t = (int)(MyHero.AttackCastDelay * 1000) - 100 + Game.Ping / 2 +
                                1000 * (int)MyHero.ServerPosition.Distance(minion.ServerPosition) / (int)MyProjectileSpeed()
                        let predHealth = HealthPrediction.GetHealthPrediction(minion, t, FarmDelay())
                        where minion.Team != GameObjectTeam.Neutral && predHealth > 0 &&
                              predHealth <= GetAADamage(minion)
                        select minion)
                    return minion;
            }

            if (CurrentMode == Mode.Harass || CurrentMode == Mode.LaneClear || CurrentMode == Mode.LaneFreeze)
            {
                foreach (
                    var turret in
                        ObjectManager.Get<Obj_AI_Turret>().Where(turret => turret.IsValidTarget(GetAutoAttackRange(MyHero, turret))))
                    return turret;
            }

            if (CurrentMode != Mode.Lasthit)
            {
                tempTarget = GetBestHeroTarget();
                if (tempTarget != null)
                    return tempTarget;
            }

            float[] maxhealth;
            if (CurrentMode == Mode.LaneClear || CurrentMode == Mode.Harass || CurrentMode == Mode.LaneFreeze)
            {
                maxhealth = new float[] { 0 };
                var maxhealth1 = maxhealth;
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(GetAutoAttackRange(MyHero, minion)) && minion.Team == GameObjectTeam.Neutral).Where(minion => minion.MaxHealth >= maxhealth1[0] || Math.Abs(maxhealth1[0] - float.MaxValue) < float.Epsilon))
                {
                    tempTarget = minion;
                    maxhealth[0] = minion.MaxHealth;
                }
                if (tempTarget != null)
                    return tempTarget;
            }

            if (CurrentMode != Mode.LaneClear || ShouldWait())
                return null;
            maxhealth = new float[] { 0 };
            foreach (var minion in from minion in ObjectManager.Get<Obj_AI_Minion>()
                .Where(minion => minion.IsValidTarget(GetAutoAttackRange(MyHero, minion)))
                                   let predHealth = HealthPrediction.LaneClearHealthPrediction(minion, (int)((MyHero.AttackDelay * 1000) * LaneClearWaitTimeMod), FarmDelay())
                                   where predHealth >=
                                         2 * GetAADamage(minion) ||
                                         Math.Abs(predHealth - minion.Health) < float.Epsilon
                                   where minion.Health >= maxhealth[0] || Math.Abs(maxhealth[0] - float.MaxValue) < float.Epsilon
                                   select minion)
            {
                tempTarget = minion;
                maxhealth[0] = minion.MaxHealth;
            }
            return tempTarget;
        }

        private static bool ShouldWait()
        {
            return
            ObjectManager.Get<Obj_AI_Minion>()
            .Any(
            minion =>
            minion.IsValidTarget() && minion.Team != GameObjectTeam.Neutral &&
            InAutoAttackRange(minion) &&
            HealthPrediction.LaneClearHealthPrediction(
            minion, (int)((MyHero.AttackDelay * 1000) * LaneClearWaitTimeMod), FarmDelay()) <= GetAADamage(minion));
        }

        public static bool IsAutoAttack(string name)
        {
            return (name.ToLower().Contains("attack") && !NoAttacks.Contains(name.ToLower())) ||
            Attacks.Contains(name.ToLower());
        }

        public static void ResetAutoAttackTimer()
        {
            _lastAATick = 0;
        }

        public static bool IsAutoAttackReset(string name)
        {
            return AttackResets.Contains(name.ToLower());
        }

        public static bool CanAttack()
        {
            if (_lastAATick <= Environment.TickCount && cantMoveTill < Environment.TickCount)
            {
                return Environment.TickCount + Game.Ping / 2 + 25 >= _lastAATick + MyHero.AttackDelay * 1000 && _attack;
            }
            return false;
        }

        public static bool CanMove()
        {
            var extraWindup = Menu.Item("lxOrbwalker_Misc_ExtraWindUp").GetValue<Slider>().Value;
            if (_lastAATick <= Environment.TickCount && cantMoveTill < Environment.TickCount)
                return Environment.TickCount + Game.Ping / 2 >= _lastAATick + MyHero.AttackCastDelay * 1000 + extraWindup && _movement;
            return false;
        }

        private static float MyProjectileSpeed()
        {
            return (MyHero.CombatType == GameObjectCombatType.Melee) ? float.MaxValue : MyHero.BasicAttack.MissileSpeed;
        }

        private static int FarmDelay()
        {
            var ret = 0;
            if (MyHero.ChampionName == "Azir")
                ret += 125;
            return Menu.Item("lxOrbwalker_Misc_Farmdelay").GetValue<Slider>().Value + ret;
        }

        private static Obj_AI_Base GetBestHeroTarget()
        {
            Obj_AI_Hero killableEnemy = null;
            var hitsToKill = double.MaxValue;
            foreach (var enemy in AllEnemys.Where(hero => hero.IsValidTarget() && InAutoAttackRange(hero)))
            {
                var killHits = CountKillhits(enemy);
                if (killableEnemy != null && !(killHits < hitsToKill))
                    continue;
                killableEnemy = enemy;
                hitsToKill = killHits;
            }
            return hitsToKill < 4 ? killableEnemy : TargetSelector.GetTarget(GetAutoAttackRange() + 100, TargetSelector.DamageType.Physical);
        }

        private static double CountKillhits(Obj_AI_Hero enemy)
        {
            return enemy.Health / GetAADamage(enemy);
        }


        private static void CheckAutoWindUp()
        {
            if (!Menu.Item("lxOrbwalker_Misc_AutoWindUp").GetValue<bool>())
                return;
            var additional = 0;
            if (Game.Ping >= 100)
                additional = Game.Ping / 100 * 10;
            else if (Game.Ping > 40 && Game.Ping < 100)
                additional = Game.Ping / 100 * 20;
            else if (Game.Ping <= 40)
                additional = +20;
            var windUp = Game.Ping + additional;
            if (windUp < 40)
                windUp = 40;
            Menu.Item("lxOrbwalker_Misc_ExtraWindUp").SetValue(windUp < 200 ? new Slider(windUp, 200, 0) : new Slider(200, 200, 0));
        }

        public static int GetCurrentWindupTime()
        {
            return Menu.Item("lxOrbwalker_Misc_ExtraWindUp").GetValue<Slider>().Value;
        }

        public void EnableDrawing()
        {
            _drawing = true;
        }

        public void DisableDrawing()
        {
            _drawing = false;
        }

        public static float GetAutoAttackRange(Obj_AI_Base source = null, Obj_AI_Base target = null)
        {
            if (source == null)
                source = MyHero;
            var ret = source.AttackRange + MyHero.BoundingRadius;
            if (target != null)
                ret += target.BoundingRadius;
            return ret;
        }

        public static bool InAutoAttackRange(Obj_AI_Base target)
        {
            if (target == null)
                return false;

            var myRange = GetAutoAttackRange(MyHero, target);

            return Vector2.DistanceSquared(target.ServerPosition.To2D(), MyHero.ServerPosition.To2D()) <= myRange * myRange + 500;
        }

        public static Mode CurrentMode
        {
            get
            {
                if (Menu.Item("Combo_Key").GetValue<KeyBind>().Active)
                    return Mode.Combo;
                if (Menu.Item("Harass_Key").GetValue<KeyBind>().Active)
                    return Mode.Harass;
                if (Menu.Item("LaneClear_Key").GetValue<KeyBind>().Active)
                    return Mode.LaneClear;
                if (Menu.Item("LaneFreeze_Key").GetValue<KeyBind>().Active)
                    return Mode.LaneFreeze;
                if (Menu.Item("LastHit_Key").GetValue<KeyBind>().Active)
                    return Mode.Lasthit;
                return Menu.Item("Flee_Key").GetValue<KeyBind>().Active ? Mode.Flee : Mode.None;
            }
        }

        public static void SetAttack(bool value)
        {
            _attack = value;
        }

        public static void SetMovement(bool value)
        {
            _movement = value;
        }

        public static bool GetAttack()
        {
            return _attack;
        }

        public static bool GetMovement()
        {
            return _movement;
        }

        public class BeforeAttackEventArgs
        {
            public Obj_AI_Base Target;
            public Obj_AI_Base Unit = ObjectManager.Player;
            private bool _process = true;
            public bool Process
            {
                get
                {
                    return _process;
                }
                set
                {
                    _disableNextAttack = !value;
                    _process = value;
                }
            }
        }
        private static void FireBeforeAttack(Obj_AI_Base target)
        {
            if (BeforeAttack != null)
            {
                BeforeAttack(new BeforeAttackEventArgs
                {
                    Target = target
                });
            }
            else
            {
                _disableNextAttack = false;
            }
        }

        private static void FireOnTargetSwitch(Obj_AI_Base newTarget)
        {
            if (OnTargetChange != null && (_lastTarget == null || _lastTarget.NetworkId != newTarget.NetworkId))
            {
                OnTargetChange(_lastTarget, newTarget);
            }
        }

        private static void FireAfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (AfterAttack != null)
            {
                AfterAttack(unit, target);
            }
        }

        private static void FireOnAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (OnAttack != null)
            {
                OnAttack(unit, target);
            }
        }
    }
}