using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceResurrected.Utilities
{
    // ReSharper disable once InconsistentNaming
    public class xSaliceWalker
    {
        private static readonly string[] AttackResets =
        {
            "dariusnoxiantacticsonh", "fioraflurry", "garenq",
            "hecarimrapidslash", "jaxempowertwo", "jaycehypercharge", "leonashieldofdaybreak", "luciane", "lucianq",
            "monkeykingdoubleattack", "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze", "netherblade",
            "parley", "poppydevastatingblow", "powerfist", "renektonpreexecute", "rengarq", "shyvanadoubleattack",
            "sivirw", "takedown", "talonnoxiandiplomacy", "trundletrollsmash", "vaynetumble", "vie", "volibearq",
            "xenzhaocombotarget", "yorickspectral", "reksaiq"
        };

        private static readonly string[] NoAttacks =
        {
            "jarvanivcataclysmattack", "monkeykingdoubleattack",
            "shyvanadoubleattack", "shyvanadoubleattackdragon", "zyragraspingplantattack", "zyragraspingplantattack2",
            "zyragraspingplantattackfire", "zyragraspingplantattack2fire", "viktorpowertransfer", "sivirwattackbounce"
        };

        private static readonly string[] Attacks =
        {
            "caitlynheadshotmissile", "frostarrow", "garenslash2",
            "kennenmegaproc", "lucianpassiveattack", "masteryidoublestrike", "quinnwenhanced", "renektonexecute",
            "renektonsuperexecute", "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust", "xenzhaothrust2",
            "xenzhaothrust3", "viktorqbuff"
        };


        private static Menu _menu;
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        public enum Mode
        {
            Combo,
            Harass,
            LaneClear,
            Lasthit,
            Flee,
            None,
        }

        private static readonly bool _drawing = true;
        private static bool _attack = true;
        private static bool _movement = true;
        private static bool _canmove = true;
        private static bool _disableNextAttack;
        private const float LaneClearWaitTimeMod = 2f;
        private static int _lastAaTick;
        private static AttackableUnit _lastTarget;
        private static Spell _movementPrediction;
        private static int _humanizerTick;
        private static int _windup;
        private static int _lastRealAttack;

        public static void AddToMenu(Menu menu)
        {
            _movementPrediction = new Spell(SpellSlot.Unknown, GetAutoAttackRange());
            _movementPrediction.SetTargetted(Player.BasicAttack.SpellCastTime, Player.BasicAttack.MissileSpeed);

            _menu = menu;

            var menuDrawing = new Menu("Drawing", "orb_Draw");
            menuDrawing.AddItem(new MenuItem("orb_Draw_AARange", "AA Circle").SetValue(new Circle(true, Color.FloralWhite)));
            menuDrawing.AddItem(new MenuItem("orb_Draw_AARange_Enemy", "AA Circle Enemy").SetValue(new Circle(true, Color.Pink)));
            menuDrawing.AddItem(new MenuItem("orb_Draw_Holdzone", "Holdzone").SetValue(new Circle(true, Color.FloralWhite)));
            menuDrawing.AddItem(new MenuItem("orb_Draw_MinionHPBar", "Minion HPBar").SetValue(new Circle(true, Color.Black)));
            menuDrawing.AddItem(new MenuItem("orb_Draw_MinionHPBar_thickness", "^ HPBar Thickness").SetValue(new Slider(1, 1, 3)));
            menuDrawing.AddItem(new MenuItem("orb_Draw_hitbox", "Show HitBoxes").SetValue(new Circle(true, Color.FloralWhite)));
            menuDrawing.AddItem(new MenuItem("orb_Draw_Lasthit", "Minion Lasthit").SetValue(new Circle(true, Color.Lime)));
            menuDrawing.AddItem(new MenuItem("orb_Draw_nearKill", "Minion nearKill").SetValue(new Circle(true, Color.Gold)));
            menu.AddSubMenu(menuDrawing);

            var menuMisc = new Menu("Misc", "orb_Misc");
            menuMisc.AddItem(new MenuItem("orb_Misc_Holdzone", "Hold Position").SetValue(new Slider(50, 0, 200)));
            menuMisc.AddItem(new MenuItem("orb_Misc_Farmdelay", "Farm Delay").SetValue(new Slider(0, 0, 300)));
            menuMisc.AddItem(new MenuItem("orb_Misc_ExtraWindUp", "Extra Winduptime").SetValue(new Slider(80, 200, 0)));
            menuMisc.AddItem(new MenuItem("orb_Misc_Priority_Unit", "Priority Unit").SetValue(new StringList(new[] { "Minion", "Hero" })));
            menuMisc.AddItem(new MenuItem("orb_Misc_Humanizer", "Humanizer Delays").SetValue(new Slider(50, 0, 500)));
            menuMisc.AddItem(new MenuItem("orb_Misc_AllMovementDisabled", "Disable All Movement").SetValue(false));
            menuMisc.AddItem(new MenuItem("orb_Misc_AllAttackDisabled", "Disable All Attacks").SetValue(false));

            menu.AddSubMenu(menuMisc);

            var menuMelee = new Menu("Melee", "orb_Melee");
            menuMelee.AddItem(new MenuItem("orb_Melee_Prediction", "Movement Prediction").SetValue(false));
            menu.AddSubMenu(menuMelee);

            var menuModes = new Menu("Orbwalk Mode", "orb_Modes");
            {
                var modeCombo = new Menu("Combo", "orb_Modes_Combo");
                modeCombo.AddItem(new MenuItem("Combo_Key", "Key").SetValue(new KeyBind(32, KeyBindType.Press)));
                modeCombo.AddItem(new MenuItem("Combo_move", "Movement").SetValue(true));
                modeCombo.AddItem(new MenuItem("Combo_attack", "Attack").SetValue(true));
                menuModes.AddSubMenu(modeCombo);

                var modeHarass = new Menu("Harass", "orb_Modes_Harass");
                modeHarass.AddItem(new MenuItem("Harass_Key", "Key").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                modeHarass.AddItem(new MenuItem("Harass_move", "Movement").SetValue(true));
                modeHarass.AddItem(new MenuItem("Harass_attack", "Attack").SetValue(true));
                modeHarass.AddItem(new MenuItem("Harass_Lasthit", "Lasthit Minions").SetValue(true));
                menuModes.AddSubMenu(modeHarass);

                var modeLaneClear = new Menu("LaneClear", "orb_Modes_LaneClear");
                modeLaneClear.AddItem(new MenuItem("LaneClear_Key", "Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                modeLaneClear.AddItem(new MenuItem("LaneClear_move", "Movement").SetValue(true));
                modeLaneClear.AddItem(new MenuItem("LaneClear_attack", "Attack").SetValue(true));
                menuModes.AddSubMenu(modeLaneClear);

                var modeLasthit = new Menu("LastHit", "orb_Modes_LastHit");
                modeLasthit.AddItem(new MenuItem("LastHit_Key", "Key").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
                modeLasthit.AddItem(new MenuItem("LastHit_move", "Movement").SetValue(true));
                modeLasthit.AddItem(new MenuItem("LastHit_attack", "Attack").SetValue(true));
                modeLasthit.AddItem(new MenuItem("NewMode", "Test Turret Farming").SetValue(false));
                menuModes.AddSubMenu(modeLasthit);

                var modeFlee = new Menu("Flee", "orb_Modes_Flee");
                modeFlee.AddItem(new MenuItem("Flee_Key", "Key").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                menuModes.AddSubMenu(modeFlee);
            }
            menu.AddSubMenu(menuModes);

            menu.AddItem(new MenuItem("AnimationCheck", "Animation Check").SetValue(true));
            menu.AddItem(new MenuItem("MissileCheck", "Missile Check").SetValue(true));
            menu.AddItem(new MenuItem("OrbwalkingTargetMode", "Mode").SetValue(new StringList(new[] { "To Mouse", "To Target" })));
            menu.AddItem(new MenuItem("orb_Misc_AutoWindUp", "Autoset Windup").SetValue(new KeyBind("O".ToCharArray()[0], KeyBindType.Press)));

            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            MissileClient.OnCreate += MissileClientOnOnCreate;
            Spellbook.OnStopCast += SpellbookCancelAa;
            Obj_AI_Base.OnPlayAnimation += ObjAiBaseOnOnPlayAnimation;
        }


        #region events

        private static void SpellbookCancelAa(Spellbook spellbook, SpellbookStopCastEventArgs args)
        {
            if (spellbook.Owner.IsValid && spellbook.Owner.IsMe && args.DestroyMissile && args.StopAnimation)
            {
                ResetAutoAttackTimer();
                _canmove = true;
            }
        }

        private static void ObjAiBaseOnOnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!_menu.Item("MissileCheck").GetValue<bool>())
                return;

            if (!sender.IsMe || sender.IsMelee() || args.Animation == "Run" || args.Animation != "Idle")
                return;

            if (args.Animation.Contains("Attack") || args.Animation == "Crit")
            {
                Utility.DelayAction.Add((int)(Player.AttackCastDelay * 100 + Game.Ping), () => _canmove = true);
            }
        }

        private static void MissileClientOnOnCreate(GameObject sender, EventArgs args)
        {
            if (!_menu.Item("MissileCheck").GetValue<bool>())
                return;

            var missile = sender as MissileClient;
            if (missile != null && missile.SpellCaster.IsMe && IsAutoAttack(missile.SData.Name) && !missile.SpellCaster.IsMelee)
            {
                _canmove = true;
                _lastRealAttack = Utils.GameTimeTickCount;
                FireAfterAttack(Player, (AttackableUnit)missile.Target);
            }
        }

        private static void OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {

                if (IsAutoAttackReset(spell.SData.Name))
                    Utility.DelayAction.Add(250, ResetAutoAttackTimer);

                if (!IsAutoAttack(spell.SData.Name))
                    return;

                if (spell.Target is Obj_AI_Base || spell.Target is Obj_BarracksDampener || spell.Target is Obj_HQ)
                {

                    _lastAaTick = Utils.GameTimeTickCount - Game.Ping/2;

                    if (spell.Target is Obj_AI_Base)
                    {
                        _lastTarget = (Obj_AI_Base) spell.Target;

                        FireOnTargetSwitch((Obj_AI_Base) spell.Target);

                        //for melle
                        if (unit.IsMelee)
                            Utility.DelayAction.Add((int) (unit.AttackCastDelay*1000 + Game.Ping*0.5) + 50,
                                () => FireAfterAttack(unit, _lastTarget));
                    }

                    FireOnAttack(unit, _lastTarget);

                }
            }

            if (!unit.IsValidTarget(2000, false) || !unit.IsAlly || unit is Obj_AI_Hero || !(unit is Obj_AI_Turret) || !IsAutoAttack(spell.SData.Name) || !(spell.Target is Obj_AI_Base))
                return;

            _turretTarget = (Obj_AI_Base)spell.Target;
            _turretAttacking = (Obj_AI_Turret)unit;

        }

        private static Obj_AI_Base _turretTarget;
        private static Obj_AI_Turret _turretAttacking;

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                _canmove = true;
                return;
            }
            CheckAutoWindUp();
            if (CurrentMode == Mode.None || MenuGUI.IsChatOpen || Player.IsChannelingImportantSpell())
                return;

            if (Utils.GameTimeTickCount - _humanizerTick < _menu.Item("orb_Misc_Humanizer").GetValue<Slider>().Value)
                return;

            var target = GetTarget();

            var mode = _menu.Item("OrbwalkingTargetMode").GetValue<StringList>().SelectedIndex;
            
            Orbwalk(target, mode == 0 || !(target is Obj_AI_Hero) ? Game.CursorPos : target.Position);
        }

        private static void OnDraw(EventArgs args)
        {
            if (!_drawing)
                return;

            if (_menu.Item("orb_Draw_AARange").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(Player.Position, GetAutoAttackRange(), _menu.Item("orb_Draw_AARange").GetValue<Circle>().Color);
            }

            if (_menu.Item("orb_Draw_AARange_Enemy").GetValue<Circle>().Active ||
                _menu.Item("orb_Draw_hitbox").GetValue<Circle>().Active)
            {
                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(1500)))
                {
                    if (_menu.Item("orb_Draw_AARange_Enemy").GetValue<Circle>().Active)
                        Render.Circle.DrawCircle(enemy.Position, GetAutoAttackRange(enemy, Player), _menu.Item("orb_Draw_AARange_Enemy").GetValue<Circle>().Color);
                    if (_menu.Item("orb_Draw_hitbox").GetValue<Circle>().Active)
                        Render.Circle.DrawCircle(enemy.Position, enemy.BoundingRadius, _menu.Item("orb_Draw_hitbox").GetValue<Circle>().Color);
                }
            }

            if (_menu.Item("orb_Draw_AARange_Enemy").GetValue<Circle>().Active)
            {
                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(1500)))
                {
                    Render.Circle.DrawCircle(enemy.Position, GetAutoAttackRange(enemy, Player), _menu.Item("orb_Draw_AARange_Enemy").GetValue<Circle>().Color);

                }
            }

            if (_menu.Item("orb_Draw_Holdzone").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(Player.Position, _menu.Item("orb_Misc_Holdzone").GetValue<Slider>().Value, _menu.Item("orb_Draw_Holdzone").GetValue<Circle>().Color);
            }

            if (_menu.Item("orb_Draw_MinionHPBar").GetValue<Circle>().Active || _menu.Item("orb_Draw_Lasthit").GetValue<Circle>().Active || _menu.Item("orb_Draw_nearKill").GetValue<Circle>().Active)
            {
                var minionList = MinionManager.GetMinions(Player.Position, GetAutoAttackRange() + 500, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
                foreach (var minion in minionList.Where(minion => minion.IsValidTarget(GetAutoAttackRange() + 500)))
                {
                    var attackToKill = Math.Ceiling(minion.MaxHealth / Player.GetAutoAttackDamage(minion, true));
                    var hpBarPosition = minion.HPBarPosition;
                    var barWidth = minion.IsMelee() ? 75 : 80;
                    if (minion.HasBuff("turretshield", true))
                        barWidth = 70;
                    var barDistance = (float)(barWidth / attackToKill);
                    if (_menu.Item("orb_Draw_MinionHPBar").GetValue<Circle>().Active)
                    {
                        for (var i = 1; i < attackToKill; i++)
                        {
                            var startposition = hpBarPosition.X + 45 + barDistance * i;
                            Drawing.DrawLine(
                                new Vector2(startposition, hpBarPosition.Y + 18),
                                new Vector2(startposition, hpBarPosition.Y + 23),
                                _menu.Item("orb_Draw_MinionHPBar_thickness").GetValue<Slider>().Value,
                                _menu.Item("orb_Draw_MinionHPBar").GetValue<Circle>().Color);
                        }
                    }
                    if (_menu.Item("orb_Draw_Lasthit").GetValue<Circle>().Active &&
                        minion.Health <= Player.GetAutoAttackDamage(minion, true))
                        Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius, _menu.Item("orb_Draw_Lasthit").GetValue<Circle>().Color);
                    else if (_menu.Item("orb_Draw_nearKill").GetValue<Circle>().Active &&
                             minion.Health <= Player.GetAutoAttackDamage(minion, true) * 2)
                        Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius, _menu.Item("orb_Draw_nearKill").GetValue<Circle>().Color);
                }
            }
        }
        #endregion


        public static void Orbwalk(AttackableUnit mytarget, Vector3 goalPosition)
        {
            
            if (Player.IsChannelingImportantSpell())
                return;

            if (mytarget is Obj_AI_Base)
            {
                var target = (Obj_AI_Base) mytarget;

                if (target != null && (CanAttack || HaveCancled) && IsAllowedToAttack())
                {
                    _disableNextAttack = false;
                    FireBeforeAttack(target);
                    if (!_disableNextAttack)
                    {
                        if (CurrentMode != Mode.Harass || !target.IsMinion ||
                            _menu.Item("Harass_Lasthit").GetValue<bool>())
                        {
                            Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                            _lastAaTick = Utils.GameTimeTickCount + Game.Ping/2;
                            _canmove = false;
                        }
                    }
                }
                if (!CanMoves || !IsAllowedToMove())
                    return;
                if (Player.IsMelee() && target != null &&
                    target.Distance(Player.Position) < GetAutoAttackRange(Player, target) &&
                    _menu.Item("orb_Melee_Prediction").GetValue<bool>() && target is Obj_AI_Hero &&
                    Game.CursorPos.Distance(target.Position) < 300)
                {
                    _movementPrediction.Delay = Player.BasicAttack.SpellCastTime;
                    _movementPrediction.Speed = Player.BasicAttack.MissileSpeed;
                    MoveTo(_movementPrediction.GetPrediction(target).UnitPosition);
                }
                else
                    MoveTo(goalPosition);
            }
            else
            {
                if (mytarget != null && (CanAttack || HaveCancled) && IsAllowedToAttack())
                {
                    _disableNextAttack = false;
                    FireBeforeAttack(mytarget);
                    if (!_disableNextAttack)
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, mytarget);
                        _lastAaTick = Utils.GameTimeTickCount + Game.Ping / 2;
                        _canmove = false;
                        
                    }
                }
                if (!CanMoves || !IsAllowedToMove())
                    return;
                    MoveTo(goalPosition);
            }
        }


        private static void MoveTo(Vector3 position)
        {
            var delay = _menu.Item("orb_Misc_Humanizer").GetValue<Slider>().Value;

            if (Utils.GameTimeTickCount - _humanizerTick < delay)
                return;

            _humanizerTick = Utils.GameTimeTickCount;

            var holdAreaRadius = _menu.Item("orb_Misc_Holdzone").GetValue<Slider>().Value;
            if (Player.ServerPosition.Distance(position) < holdAreaRadius)
            {
                if (Player.Path.Count() > 1)
                    Player.IssueOrder(GameObjectOrder.HoldPosition, Player.Position);
                return;
            }
            var point = Player.ServerPosition +
            300 * (position.To2D() - Player.ServerPosition.To2D()).Normalized().To3D();
            Player.IssueOrder(GameObjectOrder.MoveTo, point);

        }

        private static bool IsAllowedToMove()
        {
            if (!_movement)
                return false;
            if (_menu.Item("orb_Misc_AllMovementDisabled").GetValue<bool>())
                return false;
            if (CurrentMode == Mode.Combo && !_menu.Item("Combo_move").GetValue<bool>())
                return false;
            if (CurrentMode == Mode.Harass && !_menu.Item("Harass_move").GetValue<bool>())
                return false;
            if (CurrentMode == Mode.LaneClear && !_menu.Item("LaneClear_move").GetValue<bool>())
                return false;
            return CurrentMode != Mode.Lasthit || _menu.Item("LastHit_move").GetValue<bool>();
        }

        private static bool IsAllowedToAttack()
        {
            if (!_attack)
                return false;
            if (_menu.Item("orb_Misc_AllAttackDisabled").GetValue<bool>())
                return false;
            if (CurrentMode == Mode.Combo && !_menu.Item("Combo_attack").GetValue<bool>())
                return false;
            if (CurrentMode == Mode.Harass && !_menu.Item("Harass_attack").GetValue<bool>())
                return false;
            if (CurrentMode == Mode.LaneClear && !_menu.Item("LaneClear_attack").GetValue<bool>())
                return false;
            return CurrentMode != Mode.Lasthit || _menu.Item("LastHit_attack").GetValue<bool>();

        }

        private static AttackableUnit GetTarget()
        {
            AttackableUnit tempTarget = null;

            if (_menu.Item("orb_Misc_Priority_Unit").GetValue<StringList>().SelectedIndex == 1 && (CurrentMode == Mode.Harass || CurrentMode == Mode.LaneClear))
            {
                tempTarget = TargetSelector.GetTarget(-1, TargetSelector.DamageType.Physical);
                if (tempTarget != null)
                    return tempTarget;
            }

            switch (CurrentMode)
            {
                case Mode.Combo:
                    tempTarget = GetBestHeroTarget();
                    break;
                case Mode.Harass:
                    tempTarget = GetBestMinion(true);

                    if (tempTarget != null)
                        return tempTarget;

                    if (GetBestHeroTarget() != null)
                        tempTarget = GetBestHeroTarget();

                    if (GetBaseStructures() != null)
                        return GetBaseStructures();
                    break;
                case Mode.Lasthit:
                    tempTarget = GetBestMinion(true);

                    if (tempTarget != null)
                        return tempTarget;
                    break;
                case Mode.LaneClear:
                    tempTarget = GetBestMinion(false);

                    if (tempTarget != null)
                        return tempTarget;

                    if (GetBaseStructures() != null)
                        return GetBaseStructures();
                    break;
                case Mode.None:
                    break;
            }

            return tempTarget;
        }

        private static AttackableUnit GetBaseStructures()
        {
            //turrets 
            foreach (
                    var turret in
                        ObjectManager.Get<Obj_AI_Turret>().Where(turret => turret.IsValidTarget(GetAutoAttackRange(Player, turret))))
                return turret;

            //inhib
            foreach (var barrack in
                ObjectManager.Get<Obj_BarracksDampener>().Where(t => t.IsValidTarget(GetAutoAttackRange(Player, t))))
            {
                return barrack;
            }

            //nexus
            return ObjectManager.Get<Obj_HQ>().FirstOrDefault(t => t.IsValidTarget(GetAutoAttackRange(Player, t)));
        }

        private static AttackableUnit GetBestMinion(bool lastHitOnly)
        {
            AttackableUnit tempTarget = null;
            var enemies = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget() && x.Name != "Beacon" && InAutoAttackRange(x)).ToList();

            foreach (var minion in from minion in enemies
                                   let t = ProjectTime(minion)
                                   let predHealth = HealthPrediction.GetHealthPrediction(minion, t, FarmDelay())
                                   where minion.Team != GameObjectTeam.Neutral && predHealth > 0 &&
                                         (predHealth <= Player.GetAutoAttackDamage(minion, true))
                                   select minion)
                return minion;

            if (_menu.Item("NewMode").GetValue<bool>()) { 
                var turret =
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(x => x.IsValid && x.IsAlly)
                        .OrderBy(x => Player.Distance(x))
                        .FirstOrDefault();

                if (turret != null && lastHitOnly)
                {
                    foreach (var minion in enemies.Where(x => turret.Distance(x.ServerPosition) < 1000).OrderBy(x => x.Distance(turret)))
                    {
                        var playerProjectile = ProjectTime(minion);
                        var predHealth = HealthPrediction.GetHealthPrediction(minion, playerProjectile, FarmDelay());
                        var turretProjectile = turret.AttackCastDelay * 1000 + turret.Distance(minion) / turret.BasicAttack.MissileSpeed * 1000;

                        if (predHealth < 0 || playerProjectile* 1.8 > turretProjectile)
                            continue;

                        if (predHealth - turret.GetAutoAttackDamage(minion) - Player.GetAutoAttackDamage(minion, true)*2 <=
                            0 &&
                            predHealth - turret.GetAutoAttackDamage(minion) - Player.GetAutoAttackDamage(minion, true) > 0 &&
                            predHealth - turret.GetAutoAttackDamage(minion)*2 < 0)
                        {
                            return minion;
                        }
                    }
                }
            }

            if (lastHitOnly)
                return null;

            if (ShouldWait())
                return null;

            //LANE CLEAR
            var maxhealth = new float[] { 0 };
            foreach (var minion in from minion in ObjectManager.Get<Obj_AI_Minion>()
                .Where(minion => minion.IsValidTarget(GetAutoAttackRange(Player, minion)) && minion.Name != "Beacon")
                                   let predHealth = HealthPrediction.LaneClearHealthPrediction(minion, (int)((Player.AttackDelay * 1000) * LaneClearWaitTimeMod), FarmDelay())
                                   where predHealth >=
                                         2 * Player.GetAutoAttackDamage(minion, true) ||
                                         Math.Abs(predHealth - minion.Health) < float.Epsilon
                                   where minion.Health >= maxhealth[0] || Math.Abs(maxhealth[0] - float.MaxValue) < float.Epsilon
                                   select minion)
            {
                tempTarget = minion;
                maxhealth[0] = minion.MaxHealth;
            }

            return tempTarget;
        }


        private static int ProjectTime(Obj_AI_Base target)
        {
            return (int) (Player.AttackCastDelay*1000) - 100 + Game.Ping/2 +
                   1000*(int) Player.Distance(target.Position)/(int) MyProjectileSpeed;
        }

        private static bool ShouldWait(Obj_AI_Turret turret = null)
        {
            if (turret != null && turret.Target.IsValidTarget())
                return ObjectManager.Get<Obj_AI_Minion>()
                    .Any(
                    minion =>
                    minion.IsValidTarget() && minion.Team != GameObjectTeam.Neutral &&
                    InAutoAttackRange(minion) && minion.NetworkId != turret.Target.NetworkId &&
                    HealthPrediction.LaneClearHealthPrediction(
                    minion, (int)((Player.AttackDelay * 1000) * LaneClearWaitTimeMod), FarmDelay()) <= Player.GetAutoAttackDamage(minion));

            return ObjectManager.Get<Obj_AI_Minion>()
            .Any(
            minion =>
            minion.IsValidTarget() && minion.Team != GameObjectTeam.Neutral &&
            InAutoAttackRange(minion) &&
            HealthPrediction.LaneClearHealthPrediction(
            minion, (int)((Player.AttackDelay * 1000) * LaneClearWaitTimeMod), FarmDelay()) <= Player.GetAutoAttackDamage(minion));
        }

        private static int FarmDelay(int offset = 0)
        {
            if (offset != 0)
                return offset;
            return _menu.Item("orb_Misc_Farmdelay").GetValue<Slider>().Value;
        }

        private static Obj_AI_Hero GetBestHeroTarget()
        {
            Obj_AI_Hero killableEnemy = null;
            var hitsToKill = double.MaxValue;
            foreach (var enemy in HeroManager.Enemies.Where(hero => hero.IsValidTarget() && InAutoAttackRange(hero)))
            {
                var killHits = CountKillhits(enemy);
                if (killableEnemy != null && (!(killHits < hitsToKill) || enemy.HasBuffOfType(BuffType.Invulnerability)))
                    continue;
                hitsToKill = killHits;
                killableEnemy = enemy;
            }
            return hitsToKill <= 3 ? killableEnemy : TargetSelector.GetTarget(GetAutoAttackRange(), TargetSelector.DamageType.Physical);
        }

        private static void CheckAutoWindUp()
        {
            if (!_menu.Item("orb_Misc_AutoWindUp").GetValue<KeyBind>().Active)
            {
                _windup = GetCurrentWindupTime();
                return;
            }
            var additional = 0;
            if (Game.Ping >= 100)
                additional = Game.Ping / 100 * 5;
            else if (Game.Ping > 40 && Game.Ping < 100)
                additional = Game.Ping / 100 * 10;
            else if (Game.Ping <= 40)
                additional = +20;

            var windUp = Game.Ping - 20 + additional;

            if (windUp < 40)
                windUp = 40;

            _menu.Item("orb_Misc_ExtraWindUp").SetValue(windUp < 200 ? new Slider(windUp, 200, 0) : new Slider(200, 200, 0));
            _windup = windUp;
        }

        #region API/References
        public static bool IsAutoAttack(string name)
        {
            return (name.ToLower().Contains("attack") && !NoAttacks.Contains(name.ToLower())) || Attacks.Contains(name.ToLower());
        }

        public static void ResetAutoAttackTimer()
        {
            _lastAaTick = 0;
        }

        public static bool IsAutoAttackReset(string name)
        {
            return AttackResets.Contains(name.ToLower());
        }

        public static bool CanAttack
        {
            get
            {
                return _lastAaTick <= Utils.GameTimeTickCount &&
                       (Utils.GameTimeTickCount + Game.Ping / 2 + 25 >= _lastAaTick + Player.AttackDelay * 1000 && _attack);
            }
        }

        private static bool HaveCancled
        {
            get
            {
                return _lastAaTick - Utils.GameTimeTickCount > Player.AttackCastDelay * 1000 + 25 && _lastRealAttack < _lastAaTick;
            }
        }

        public static bool CanMove(float delay)
        {
            return CanMoves;
        }

        private static bool CanMoves
        {
            get
            {
                return _canmove || _lastAaTick <= Utils.GameTimeTickCount &&
                       (Utils.GameTimeTickCount + Game.Ping / 2 >= _lastAaTick + Player.AttackCastDelay * 1000 + _windup &&
                        _movement);
            }
        }

        private static float MyProjectileSpeed
        {
            get
            {
                return Player.CombatType == GameObjectCombatType.Melee ? float.MaxValue : Player.BasicAttack.MissileSpeed;
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

        public static double CountKillhits(Obj_AI_Base enemy)
        {
            return enemy.Health / Player.GetAutoAttackDamage(enemy);
        }

        public static int GetCurrentWindupTime()
        {
            return _menu.Item("orb_Misc_ExtraWindUp").GetValue<Slider>().Value;
        }

        public static float GetAutoAttackRange(Obj_AI_Base source = null, AttackableUnit target = null)
        {
            if (source == null)
                source = Player;
            var ret = source.AttackRange + Player.BoundingRadius;
            if (target != null)
                ret += target.BoundingRadius;
            return ret;
        }

        public static bool InAutoAttackRange(AttackableUnit target)
        {
            if (target == null)
                return false;
            var myRange = GetAutoAttackRange(Player, target);
            return target.IsValidTarget(myRange);
        }

        public static Mode CurrentMode
        {
            get
            {
                if (_menu.Item("Combo_Key").GetValue<KeyBind>().Active)
                    return Mode.Combo;
                if (_menu.Item("Harass_Key").GetValue<KeyBind>().Active)
                    return Mode.Harass;
                if (_menu.Item("LaneClear_Key").GetValue<KeyBind>().Active)
                    return Mode.LaneClear;
                if (_menu.Item("LastHit_Key").GetValue<KeyBind>().Active)
                    return Mode.Lasthit;
                return _menu.Item("Flee_Key").GetValue<KeyBind>().Active ? Mode.Flee : Mode.None;
            }
        }

        #endregion

        #region Events

        public delegate void BeforeAttackEvenH(BeforeAttackEventArgs args);
        public delegate void OnTargetChangeH(AttackableUnit oldTarget, AttackableUnit newTarget);
        public delegate void AfterAttackEvenH(AttackableUnit unit, AttackableUnit target);
        public delegate void OnAttackEvenH(AttackableUnit unit, AttackableUnit target);

        public static event BeforeAttackEvenH BeforeAttack;
        public static event OnTargetChangeH OnTargetChange;
        public static event AfterAttackEvenH AfterAttack;
        public static event OnAttackEvenH OnAttack;

        public class BeforeAttackEventArgs
        {
            public AttackableUnit Target;
            public AttackableUnit Unit = ObjectManager.Player;
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

        private static void FireBeforeAttack(AttackableUnit target)
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

        private static void FireOnTargetSwitch(AttackableUnit newTarget)
        {
            if (OnTargetChange != null && (_lastTarget == null || _lastTarget.NetworkId != newTarget.NetworkId))
            {
                OnTargetChange(_lastTarget, newTarget);
            }
        }

        private static void FireAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (AfterAttack != null)
            {
                AfterAttack(unit, target);
            }
        }

        private static void FireOnAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (OnAttack != null)
            {
                OnAttack(unit, target);
            }
        }
        #endregion

    }
}
