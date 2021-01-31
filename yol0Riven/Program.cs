using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
// ReSharper disable InconsistentNaming
namespace yol0Riven
{
    internal class Program
    {
        private static readonly Spell Q = new Spell(SpellSlot.Q, 260);
        private static readonly Spell W = new Spell(SpellSlot.W, 250);
        private static readonly Spell E = new Spell(SpellSlot.E, 325);
        private static readonly Spell R = new Spell(SpellSlot.R, 900);
        private static readonly Items.Item _Tiamat = new Items.Item(3077, 400);
        private static readonly Items.Item _Hydra = new Items.Item(3074, 400);
        private static readonly Items.Item _Ghostblade = new Items.Item(3142, 600);
        private static Menu menu;
        private static int qCount;
        private static int lastQCast;
        private static bool ultiOn;
        private static bool ultiReady;
        private static bool WaitForMove;
        private static Spell nextSpell;
        private static string lastSpellName = "";
        private static bool UseAttack;
        private static bool UseTiamat;
        private static Obj_AI_Hero target;
        private static int lastGapClose;
        private static Orbwalking.Orbwalker orbwalker;

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Riven")
                return;

            menu = new Menu("yol0 Riven", "yol0Riven", true);
            menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            menu.AddSubMenu(new Menu("Target Selector", "Target Selector"));
            menu.AddSubMenu(new Menu("Combo", "Combo"));
            menu.AddSubMenu(new Menu("Killsteal", "KS"));
            menu.AddSubMenu(new Menu("Misc", "Misc"));
            menu.AddSubMenu(new Menu("Drawing", "Draw"));

            orbwalker = new Orbwalking.Orbwalker(menu.SubMenu("Orbwalker"));
            TargetSelector.AddToMenu(menu.SubMenu("Target Selector"));

            menu.SubMenu("Combo").AddItem(new MenuItem("useQ", "Use Q to gapclose").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("useR", "Use Ultimate").SetValue(true));

            menu.SubMenu("KS").AddItem(new MenuItem("ksQ", "KS with Q").SetValue(true));
            menu.SubMenu("KS").AddItem(new MenuItem("ksW", "KS with W").SetValue(true));
            menu.SubMenu("KS").AddItem(new MenuItem("ksT", "KS with Tiamat/Hydra").SetValue(true));
            menu.SubMenu("KS").AddItem(new MenuItem("ksR", "KS with R2").SetValue(true));
            menu.SubMenu("KS").AddItem(new MenuItem("ksRA", "Activate ult for KS").SetValue(false));
            menu.SubMenu("KS").AddSubMenu(new Menu("Don't use R2 for KS", "noKS"));
            foreach (var enemy in HeroManager.Enemies)
            {
                menu.SubMenu("KS")
                    .SubMenu("noKS")
                    .AddItem(new MenuItem(enemy.ChampionName, enemy.ChampionName).SetValue(false));
            }

            menu.SubMenu("Misc")
                .AddItem(new MenuItem("Flee", "Flee Mode").SetValue(new KeyBind("T".ToArray()[0], KeyBindType.Press)));
            menu.SubMenu("Misc").AddSubMenu(new Menu("Auto Stun", "Stun"));
            foreach (var enemy in HeroManager.Enemies)
            {
                menu.SubMenu("Misc")
                    .SubMenu("Stun")
                    .AddItem(new MenuItem(enemy.ChampionName, "Stun " + enemy.ChampionName).SetValue(true));
            }

            menu.SubMenu("Misc").AddItem(new MenuItem("gapclose", "Auto W Gapclosers").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("interrupt", "Auto W Interruptible Spells").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("keepalive", "Keep Q Alive").SetValue(true));

            menu.SubMenu("Draw")
                .AddItem(new MenuItem("drawRange", "Draw Engage Range").SetValue(new Circle(true, Color.Green)));
            menu.SubMenu("Draw")
                .AddItem(new MenuItem("drawTarget", "Draw Current Target").SetValue(new Circle(true, Color.Red)));
            menu.SubMenu("Draw").AddItem(new MenuItem("drawDamage", "Draw Damage on Healthbar").SetValue(true));


            Utility.HpBarDamageIndicator.DamageToUnit = GetDamage;
            menu.AddToMainMenu();

            R.SetSkillshot(0.25f, 60f, 2200, false, SkillshotType.SkillshotCone);
            E.SetSkillshot(0, 0, 1450, false, SkillshotType.SkillshotLine);

            Game.OnUpdate += OnUpdate;
            Orbwalking.BeforeAttack += BeforeAttack;
            Orbwalking.AfterAttack += AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
           /* Obj_AI_Base.OnPlayAnimation += OnPlayAnimation;*/
            AttackableUnit.OnDamage += OnDamage;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += OnInterruptableTarget;
        }

        private static void KillSecure()
        {
            foreach (var enemy in HeroManager.Enemies)
            {
                if (!enemy.IsDead && enemy.IsVisible)
                {
                    if (ultiReady && menu.SubMenu("KS").Item("ksR").GetValue<bool>() && qCount == 2 && Q.IsReady() &&
                        enemy.IsValidTarget(Q.Range) && GetRDamage(enemy) + GetUltiQDamage(enemy) - 40 >= enemy.Health &&
                        !menu.SubMenu("KS").SubMenu("noKS").Item(enemy.ChampionName).GetValue<bool>())
                    {
                        R.Cast(enemy, aoe: true);
                    }
                    if (ultiReady && menu.SubMenu("KS").Item("ksR").GetValue<bool>() &&
                        enemy.IsValidTarget(R.Range - 30) && GetRDamage(enemy) - 20 >= enemy.Health &&
                        !menu.SubMenu("KS").SubMenu("noKS").Item(enemy.ChampionName).GetValue<bool>())
                    {
                        R.Cast(enemy, aoe: true);
                    }
                    else if (menu.SubMenu("KS").Item("ksQ").GetValue<bool>() && Q.IsReady() &&
                        enemy.IsValidTarget(Q.Range) && (ultiOn ? GetUltiQDamage(enemy) : GetQDamage(enemy)) - 10 >= enemy.Health)
                    {
                        Q.Cast(enemy.ServerPosition);
                    }
                    else if (menu.SubMenu("KS").Item("ksW").GetValue<bool>() && W.IsReady() &&
                             enemy.IsValidTarget(W.Range) && GetWDamage(enemy) - 10 >= enemy.Health)
                    {
                        Q.Cast(enemy.ServerPosition);
                    }
                    else if (menu.SubMenu("KS").Item("ksT").GetValue<bool>() &&
                             (_Tiamat.IsReady() || _Hydra.IsReady()) && enemy.IsValidTarget(_Tiamat.Range) &&
                             Player.GetItemDamage(enemy, Damage.DamageItems.Tiamat) - 10 >= enemy.Health)
                    {
                        if (_Tiamat.IsReady())
                            _Tiamat.Cast();
                        else if (_Hydra.IsReady())
                            _Hydra.Cast();
                    }
                    else if (!ultiReady && !ultiOn && menu.SubMenu("KS").Item("ksR").GetValue<bool>() &&
                             menu.SubMenu("KS").Item("ksRA").GetValue<bool>() &&
                             enemy.IsValidTarget(R.Range - 30) && GetRDamage(enemy) - 20 >= enemy.Health &&
                             orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                    {
                        R.Cast();
                    }
                }
            }
        }

        private static void CancelAnimation()
        {
            if (WaitForMove)
                return;

            WaitForMove = true;
            var movePos = Game.CursorPos;
            if (Player.Distance(target.Position) < 600)
            {
                movePos = Player.ServerPosition.Extend(target.ServerPosition, 100);
            }
            Player.IssueOrder(GameObjectOrder.MoveTo, movePos);
        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && nextSpell == Q)
            {
                Q.Cast(Program.target.ServerPosition);
                nextSpell = null;
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            Utility.HpBarDamageIndicator.Enabled = menu.SubMenu("Draw").Item("drawDamage").GetValue<bool>();
            CheckBuffs();
            KillSecure();
            if (orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                AutoStun();
            if (menu.SubMenu("Misc").Item("Flee").GetValue<KeyBind>().Active)
                Flee();

            if (target == null)
                orbwalker.SetMovement(true);

            if (target != null && target.IsDead)
                orbwalker.SetMovement(true);

            if (orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                orbwalker.SetMovement(true);

            if (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (target == null)
                    target = TargetSelector.GetTarget(E.Range + Q.Range, TargetSelector.DamageType.Physical);

                if (target != null &&
                    (target.IsDead || !target.IsVisible ||
                     !target.IsValidTarget(E.Range + Q.Range + Player.AttackRange)))
                    orbwalker.SetMovement(true);

                if (target == null)
                    orbwalker.SetMovement(true);
                else
                {
                    if (!target.IsVisible)
                        target = TargetSelector.GetTarget(E.Range + Q.Range, TargetSelector.DamageType.Physical);

                    if (target.IsDead)
                        target = TargetSelector.GetTarget(E.Range + Q.Range, TargetSelector.DamageType.Physical);

                    if (!target.IsValidTarget(E.Range + Q.Range + Player.AttackRange))
                        target = TargetSelector.GetTarget(E.Range + Q.Range, TargetSelector.DamageType.Physical);

                    if (Hud.SelectedUnit != null && Hud.SelectedUnit != target && Hud.SelectedUnit.IsVisible &&
                        Hud.SelectedUnit is Obj_AI_Hero)
                    {
                        var unit = (Obj_AI_Hero) Hud.SelectedUnit;
                        if (unit.IsValidTarget())
                            target = (Obj_AI_Hero) Hud.SelectedUnit;
                    }

                    if (TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget() != target &&
                        TargetSelector.GetSelectedTarget().IsVisible &&
                        TargetSelector.GetSelectedTarget().IsValidTarget())
                    {
                        target = TargetSelector.GetSelectedTarget();
                    }

                    if (target != null && !target.IsDead && target.IsVisible)
                    {
                        GapClose(target);
                        Combo(target);
                    }
                    else
                    {
                        orbwalker.SetMovement(true);
                    }
                }
            }
            else
            {
                orbwalker.SetMovement(true);
                if (!Player.IsRecalling() && qCount != 0 && lastQCast + (3650 - Game.Ping/2) < Utils.TickCount &&
                    menu.SubMenu("Misc").Item("keepalive").GetValue<bool>())
                {
                    Q.Cast(Game.CursorPos);
                }
            }
        }

        private static void Combo(Obj_AI_Hero target)
        {
            orbwalker.SetMovement(false);
            var noRComboDmg = DamageCalcNoR(target);
            if (R.IsReady() && !ultiReady && noRComboDmg < target.Health &&
                menu.SubMenu("Combo").Item("useR").GetValue<bool>())
            {
                R.Cast();
            }

            if (!(_Tiamat.IsReady() || _Hydra.IsReady()) && !Q.IsReady() && W.IsReady() &&
                target.IsValidTarget(W.Range))
            {
                W.Cast();
            }

            if (nextSpell == null && UseTiamat)
            {
                if (_Tiamat.IsReady())
                    _Tiamat.Cast();
                else if (_Hydra.IsReady())
                    _Hydra.Cast();

                UseTiamat = false;
                return;
            }

            if (nextSpell == null && UseAttack)
            {
                Orbwalking.LastAATick = Utils.TickCount + Game.Ping/2;
                orbwalker.SetMovement(false);
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                return;
            }

            if (nextSpell == Q)
            {
                if (lastSpellName.Contains("Attack") && Player.IsWindingUp)
                    return;

                Q.Cast(target.Position);
                nextSpell = null;
            }

            if (nextSpell == W)
            {
                W.Cast();
                nextSpell = null;
            }

            if (nextSpell == E)
            {
                E.Cast(target.Position);
                nextSpell = null;
            }
        }

        private static void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (W.IsReady() && sender.IsValidTarget(W.Range) &&
                menu.SubMenu("Misc").Item("interrupt").GetValue<bool>())
                W.Cast();
        }

        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (W.IsReady() && gapcloser.Sender.IsValidTarget(W.Range) &&
                menu.SubMenu("Misc").Item("gapclose").GetValue<bool>())
            {
                W.Cast();
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (menu.SubMenu("Draw").Item("drawRange").GetValue<Circle>().Active)
                Render.Circle.DrawCircle(Player.Position,
                    menu.SubMenu("Combo").Item("useQ").GetValue<bool>() ? Q.Range + E.Range : E.Range,
                    menu.SubMenu("Draw").Item("drawRange").GetValue<Circle>().Color);

            if (menu.SubMenu("Draw").Item("drawTarget").GetValue<Circle>().Active && target != null &&
                target.IsVisible)
            {
                Render.Circle.DrawCircle(target.Position, target.BoundingRadius + 10,
                    menu.SubMenu("Draw").Item("drawTarget").GetValue<Circle>().Color);
                Render.Circle.DrawCircle(target.Position, target.BoundingRadius + 25,
                    menu.SubMenu("Draw").Item("drawTarget").GetValue<Circle>().Color);
                Render.Circle.DrawCircle(target.Position, target.BoundingRadius + 45,
                    menu.SubMenu("Draw").Item("drawTarget").GetValue<Circle>().Color);
            }
        }

        private static void OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            if (target == null || args.TargetNetworkId != target.NetworkId) return;
            if ((int) args.Type != 70) return;
            if (lastQCast != 0 && lastQCast + 100 > Utils.TickCount)
            {
                WaitForMove = true;
                CancelAnimation();
                Orbwalking.ResetAutoAttackTimer();
            }
            else if (lastSpellName.Contains("Attack"))
            {
                if (_Tiamat.IsReady())
                {
                    nextSpell = null;
                    UseTiamat = true;
                }
                else if (_Hydra.IsReady())
                {
                    nextSpell = null;
                    UseTiamat = true;
                }
                else if (W.IsReady() && target.IsValidTarget(W.Range) && qCount != 0)
                {
                    UseTiamat = false;
                    nextSpell = W;
                }
                else
                {
                    UseTiamat = false;
                    nextSpell = Q;
                }
                UseAttack = false;
                orbwalker.SetMovement(true);
            }
        }

     /*   private static void OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!sender.IsMe || _orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo) return;

            if (args.Animation.Contains("Spell1"))
            {
                Utility.DelayAction.Add(125 + Game.Ping/2, CancelAnimation);
            }
            if (WaitForMove && args.Animation.Contains("Run") && _target != null)
            {
                WaitForMove = false;
                Orbwalking.LastAATick = Utils.TickCount + Game.Ping/2;
                Player.IssueOrder(GameObjectOrder.AttackUnit, _target);
            }
            if (WaitForMove && args.Animation.Contains("Idle") && _target != null)
            {
                WaitForMove = false;
                Orbwalking.LastAATick = Utils.TickCount + Game.Ping/2;
                Player.IssueOrder(GameObjectOrder.AttackUnit, _target);
            }
        } */

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            var spellname = args.SData.Name;

            if (spellname == "RivenTriCleave")
            {
                lastQCast = Utils.TickCount;
            }
            if (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                lastSpellName = spellname;
                if (spellname.Contains("Attack"))
                {
                    if (_Tiamat.IsReady() && target.IsValidTarget(_Tiamat.Range))
                    {
                        nextSpell = null;
                        UseTiamat = true;
                    }
                    else if (_Hydra.IsReady() && target.IsValidTarget(_Hydra.Range))
                    {
                        nextSpell = null;
                        UseTiamat = true;
                    }
                }
                else if (spellname == "RivenTriCleave")
                {
                    nextSpell = null;
                    Utility.DelayAction.Add(125 + Game.Ping/2, CancelAnimation);

                    if (orbwalker.InAutoAttackRange(target))
                    {
                        nextSpell = null;
                        UseAttack = true;
                        return;
                    }

                    if (W.IsReady() && target.IsValidTarget(W.Range))
                    {
                        nextSpell = W;
                    }
                    else
                    {
                        nextSpell = null;
                        UseAttack = true;
                    }
                }
                else if (spellname == "RivenMartyr")
                {
                    if (Q.IsReady())
                    {
                        nextSpell = Q;
                        UseAttack = false;
                        UseTiamat = false;
                        //Utility.DelayAction.Add(175, delegate { nextSpell = _Q; });
                    }
                    else
                    {
                        nextSpell = null;
                        UseAttack = true;
                    }
                }
                else if (spellname == "ItemTiamatCleave")
                {
                    UseTiamat = false;
                    if (W.IsReady() && target.IsValidTarget(W.Range))
                        nextSpell = W;
                    else if (Q.IsReady() && target.IsValidTarget(Q.Range))
                        nextSpell = Q;
                }
                else if (spellname == "RivenFengShuiEngine")
                {
                    ultiOn = true;
                    if ((_Tiamat.IsReady() && target.IsValidTarget(_Tiamat.Range)) ||
                        (_Hydra.IsReady() && target.IsValidTarget(_Hydra.Range)))
                    {
                        nextSpell = null;
                        UseTiamat = true;
                    }
                    else if (Q.IsReady() && target.IsValidTarget(Q.Range))
                    {
                        nextSpell = Q;
                    }
                    else if (E.IsReady())
                    {
                        nextSpell = E;
                    }
                }
            }
        }

        private static void GapClose(Obj_AI_Hero target)
        {
            var useE = E.IsReady();
            var useQ = Q.IsReady() && qCount < 2 && menu.SubMenu("Combo").Item("useQ").GetValue<bool>();

            if (lastGapClose + 400 > Utils.TickCount && lastGapClose != 0)
                return;

            lastGapClose = Utils.TickCount;

            var aRange = Player.AttackRange + Player.BoundingRadius + target.BoundingRadius;
            var eRange = aRange + E.Range;
            var qRange = aRange + Q.Range;
            var eqRange = Q.Range + E.Range;
            var distance = Player.Distance(target.ServerPosition);
            if (distance < aRange)
                return;

            nextSpell = null;
            UseTiamat = false;
            UseAttack = true;
            if (_Ghostblade.IsReady())
                _Ghostblade.Cast();

            if (useQ && qRange > distance && !E.IsReady())
            {
                var comboDmgNoR = DamageCalcNoR(target);
                if (R.IsReady() && !ultiReady && comboDmgNoR < target.Health &&
                    menu.SubMenu("Combo").Item("useR").GetValue<bool>())
                {
                    R.Cast();
                }
                Q.Cast(target.ServerPosition);
            }
            else if (useE && eRange > distance + aRange)
            {
                var pred = Prediction.GetPrediction(target, 0, 0, 1450);
                E.Cast(pred.CastPosition);
            }
            else if (useQ && eqRange + aRange > distance)
            {
                var pred = Prediction.GetPrediction(target, 0, 0, 1450);
                E.Cast(pred.CastPosition);
            }
        }

        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            var t = args.Target as Obj_AI_Minion;
            if (t == null)
            {
                orbwalker.SetMovement(false);
            }
        }

        private static void CheckBuffs()
        {
            var ulti = false;
            var ulti2 = false;
            var q = false;

            foreach (var buff in Player.Buffs)
            {
                if (buff.Name == "rivenwindslashready")
                {
                    ulti = true;
                    ultiReady = true;
                }

                if (buff.Name == "RivenTriCleave")
                {
                    q = true;
                    qCount = buff.Count;
                }

                if (buff.Name == "RivenFengShuiEngine")
                {
                    ulti2 = true;
                    ultiOn = true;
                }
            }

            if (!q)
                qCount = 0;

            if (!ulti)
            {
                ultiReady = false;
            }

            if (!ulti2)
                ultiOn = false;
        }

        private static void AutoStun()
        {
            if (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                return;

            foreach (var enemy in HeroManager.Enemies)
            {
                if (W.IsReady() && enemy.IsValidTarget(W.Range) &&
                    menu.SubMenu("Misc").SubMenu("Stun").Item(enemy.ChampionName).GetValue<bool>())
                {
                    W.Cast();
                }
            }
        }

        private static void Flee()
        {
            orbwalker.SetMovement(true);
            if (Q.IsReady())
            {
                Q.Cast(Game.CursorPos);
            }
            if (E.IsReady())
            {
                E.Cast(Game.CursorPos);
            }
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
        }

        private static float GetDamage(Obj_AI_Hero target)
        {
            if (R.IsReady() || (ultiReady))
            {
                return (float) DamageCalcR(target);
            }
            return (float) DamageCalcNoR(target);
        }

        private static double GetRDamage(Obj_AI_Base target, float otherdmg = 0.0f)
        {
            if (R.Level == 0)
                return 0.0;

            var minDmg = (80 + (40*(R.Level - 1))) +
                            0.6*((0.2*(Player.BaseAttackDamage + Player.FlatPhysicalDamageMod)) + Player.FlatPhysicalDamageMod);

            var targetPercentHealthMissing = 100*(1 - (target.Health - otherdmg)/target.MaxHealth);
            double dmg;
            if (targetPercentHealthMissing > 75.0f)
            {
                dmg = minDmg*3;
            }
            else
            {
                dmg = minDmg + minDmg*(0.0267*targetPercentHealthMissing);
            }

            var realDmg = Player.CalcDamage(target, Damage.DamageType.Physical, dmg - 20);
            return realDmg;
        }

        private static double GetUltiQDamage(Obj_AI_Base target)
        {
            var dmg = 10 + ((W.Level - 1)*20) + 0.6*(1.2*(Player.BaseAttackDamage + Player.FlatPhysicalDamageMod));
            return Player.CalcDamage(target, Damage.DamageType.Physical, dmg - 10);
        }

        private static double GetUltiWDamage(Obj_AI_Base target)
        {
            var totalAD = Player.FlatPhysicalDamageMod + Player.BaseAttackDamage;
            var dmg = 50 + ((W.Level - 1)*30) + (0.2*totalAD + Player.FlatPhysicalDamageMod);
            return Player.CalcDamage(target, Damage.DamageType.Physical, dmg - 10);
        }

        private static double GetQDamage(Obj_AI_Base target)
        {
            var totalAD = Player.FlatPhysicalDamageMod + Player.BaseAttackDamage;
            var dmg = 10 + ((Q.Level - 1)*20) + (0.35 + (Player.Level*0.05))*totalAD;
            return Player.CalcDamage(target, Damage.DamageType.Physical, dmg - 10);
        }

        private static double GetWDamage(Obj_AI_Base target)
        {
            var dmg = 50 + (W.Level*30) + Player.FlatPhysicalDamageMod;
            return Player.CalcDamage(target, Damage.DamageType.Physical, dmg - 10);
        }

        private static double DamageCalcNoR(Obj_AI_Base target)
        {
            var qDamage = GetQDamage(target);
            var wDamage = GetWDamage(target);
            var tDamage = 0.0;
            var aDamage = Player.GetAutoAttackDamage(target);
            var pDmgMultiplier = 0.2 + (0.05*Math.Floor(Player.Level/3.0));
            
            var totalAD = Player.BaseAttackDamage + Player.FlatPhysicalDamageMod;
            var pDamage = Player.CalcDamage(target, Damage.DamageType.Physical, pDmgMultiplier*totalAD);

            if (_Tiamat.IsReady() || _Hydra.IsReady())
                tDamage = Player.GetItemDamage(target, Damage.DamageItems.Tiamat);

            if (!Q.IsReady() && qCount == 0)
                qDamage = 0.0;

            if (!W.IsReady())
                wDamage = 0.0;

            return wDamage + tDamage + (qDamage*(3 - qCount)) + (pDamage*(3 - qCount)) + aDamage*(3 - qCount);
        }

        public static double DamageCalcR(Obj_AI_Base target)
        {
            var qDamage = GetUltiQDamage(target);
            var wDamage = GetUltiWDamage(target);

            var tDamage = 0.0;
            var totalAD = Player.FlatPhysicalDamageMod + Player.BaseAttackDamage;


            var aDamage = Player.CalcDamage(target, Damage.DamageType.Physical, 0.2*totalAD + totalAD);
            var pDmgMultiplier = 0.2 + (0.05*Math.Floor(Player.Level/3.0));
            var pDamage = Player.CalcDamage(target, Damage.DamageType.Physical,
                pDmgMultiplier*(0.2*totalAD + totalAD));
            if (_Tiamat.IsReady() || _Hydra.IsReady())
                tDamage = Player.GetItemDamage(target, Damage.DamageItems.Tiamat);

            if (!Q.IsReady() && qCount == 0)
                qDamage = 0.0;

            if (!W.IsReady())
                wDamage = 0.0;


            var dmg = (pDamage*(3 - qCount)) + (aDamage*(3 - qCount)) + wDamage + tDamage +
                         (qDamage*(3 - qCount));

            var rDamage = GetRDamage(target, (float) dmg);

            if (R.IsReady())
                rDamage = 0.0;

            return dmg + rDamage;
        }
    }
}