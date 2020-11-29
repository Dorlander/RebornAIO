using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using Orbwalk = LeagueSharp.Common.Orbwalking;

namespace ZzihaoAIO.Yasuo
{
    internal class ZzihaoYasuo
    {

        public static Spell Q, Q3;
        public static Spell W = new Spell(SpellSlot.W, 400);
        public static Spell E = new Spell(SpellSlot.E, 475);
        public static Spell R = new Spell(SpellSlot.R, 1200);
        public static Spell Ignite;
        public static SpellSlot Flash = SpellSlot.Unknown;
        public static Menu Config;
        public static bool wallCasted;
        public static Orbwalk.Orbwalker Orbwalker;
        public static List<Skillshot> DetectedSkillShots = new List<Skillshot>();
        public static List<Skillshot> EvadeDetectedSkillshots = new List<Skillshot>();
        public static Menu skillShotMenu;
        public static bool isDashing;
        private static int lastE;
        public static YasWall wall = new YasWall();
        public static Obj_AI_Hero myHero { get { return ObjectManager.Player; } }
        public static float HealthPercent { get { return myHero.Health / myHero.MaxHealth * 100; } }

        public struct IsSafeResult
        {
            public bool IsSafe;
            public List<Skillshot> SkillshotList;
            public List<Obj_AI_Base> casters;
        }

        internal class YasWall
        {
            public MissileClient pointL;
            public MissileClient pointR;
            public float endtime = 0;
            public YasWall()
            {

            }

            public YasWall(MissileClient L, MissileClient R)
            {
                pointL = L;
                pointR = R;
                endtime = Game.Time + 4;
            }

            public void setR(MissileClient R)
            {
                pointR = R;
                endtime = Game.Time + 4;
            }

            public void setL(MissileClient L)
            {
                pointL = L;
                endtime = Game.Time + 4;
            }

            public bool isValid(int time = 0)
            {
                return pointL != null && pointR != null && endtime - (time / 1000) > Game.Time;
            }
        }

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs Args)
        {
            Init();
        }

        public static void Init()
        {
            Q = new Spell(SpellSlot.Q, 475);
            Q3 = new Spell(SpellSlot.Q, 1000);

            Q.SetSkillshot(0.25f, 50f, float.MaxValue, false, SkillshotType.SkillshotLine);
            Q3.SetSkillshot(0.5f, 90f, 1200f, false, SkillshotType.SkillshotLine);

            var slot = ObjectManager.Player.GetSpellSlot("summonerdot");
            if (slot != SpellSlot.Unknown)
            {
                Ignite = new Spell(slot, 600, TargetSelector.DamageType.True);
            }

            //  Fslot = ObjectManager.Player.GetSpellSlot("SummonerFlash");

            Config = new Menu("ZzihaoAIO", "ZzihaoAIO", true);
            //Orbwalker
            Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalk.Orbwalker(Config.SubMenu("Orbwalker"));
            Config.AddSubMenu(new Menu("Combo", "combo"));
            Config.SubMenu("combo").SubMenu("Q").AddItem(new MenuItem("QC", "Use Q")).SetValue(true);
            Config.SubMenu("combo").SubMenu("Q").AddItem(new MenuItem("AutoQ1", "Auto Q")).SetValue(true);
            Config.SubMenu("combo").SubMenu("Q").AddItem(new MenuItem("AutoQToggle", "AutoQ Toggle")).SetValue(new KeyBind('U', KeyBindType.Toggle, false));
            Config.SubMenu("combo").SubMenu("E").AddItem(new MenuItem("EC", "Use E")).SetValue(true);
            Config.SubMenu("combo").SubMenu("E").AddItem(new MenuItem("E1", "E range >=")).SetValue(new Slider(375, 475, 1));
            Config.SubMenu("combo").SubMenu("E").AddItem(new MenuItem("E2", "E gapclose to enemy >=")).SetValue(new Slider(230, 1300, 1));
            Config.SubMenu("combo").SubMenu("E").AddItem(
               new MenuItem("ComboEMode", "ComboEMode: ", true).SetValue(
                   new StringList(new[] { "Enemy", "Сursor" }, 0)));
            Config.SubMenu("combo").SubMenu("E").AddItem(new MenuItem("ETower", "E UnderTower Toggle")).SetValue(new KeyBind('T', KeyBindType.Toggle, true));
            //          Config.SubMenu("combo").AddItem(new MenuItem("flash", "使用闪现EQR")).SetValue(true);
            //        Config.SubMenu("combo").AddItem(new MenuItem("flash2", "如果可击中敌人 >=")).SetValue(new Slider(3, 5, 1));
            //        Config.SubMenu("combo").AddItem(new MenuItem("flash3", "并且范围内的队友 >=")).SetValue(new Slider(2, 5, 1));
            Config.SubMenu("combo").AddItem(new MenuItem("Ignite", "Use Ignite")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("myHP", "MyHealthLimit <=")).SetValue(new Slider(70, 100, 1));
            //SmartR
            Config.SubMenu("combo").SubMenu("R").AddItem(new MenuItem("R", "Use Smart R")).SetValue(true);
            Config.SubMenu("combo").SubMenu("R").AddItem(new MenuItem("R1", "Use R if enemy HP <=")).SetValue(new Slider(50, 100, 1));
            Config.SubMenu("combo").SubMenu("R").AddItem(new MenuItem("R2", "Use R if hit X enemys  >=")).SetValue(new Slider(2, 5, 1));
            Config.SubMenu("combo").SubMenu("R").AddItem(new MenuItem("R3", "Use R instantly if Ally around")).SetValue(true);
            //Auto R
            Config.SubMenu("combo").SubMenu("R").AddItem(new MenuItem("R4", "Auto R")).SetValue(true);
            Config.SubMenu("combo").SubMenu("R").AddItem(new MenuItem("R5", "If R hit X enemys >=")).SetValue(new Slider(3, 5, 1));
            Config.SubMenu("combo").SubMenu("R").AddItem(new MenuItem("R6", "If X enemys around <=")).SetValue(new Slider(2, 5, 1));
            Config.SubMenu("combo").SubMenu("R").AddItem(new MenuItem("R7", "If my HP >=")).SetValue(new Slider(50, 100, 1));
            //R whitelist
            Config.SubMenu("combo").SubMenu("R").AddSubMenu(new Menu("R whitelist", "ult"));
            foreach (var hero in HeroManager.Enemies.Where(x => x.IsEnemy))
            {
                Config.SubMenu("combo").SubMenu("R").SubMenu("ult").AddItem(new MenuItem(hero.ChampionName, "Use R vs " + hero.ChampionName)).SetValue(true);
            }

            Config.AddSubMenu(new Menu("Smart W", "aShots"));
            //SmartW
            Config.SubMenu("aShots").AddItem(new MenuItem("smartW", "Auto W")).SetValue(true);
            Config.SubMenu("aShots").AddItem(new MenuItem("smartWDanger", "Danger level >=")).SetValue(new Slider(3, 5, 1));
            Config.SubMenu("aShots").AddItem(new MenuItem("smartWDelay", "Delay W (500 = Minimum response time)")).SetValue(new Slider(500, 3000, 500));
            Config.SubMenu("aShots").AddItem(new MenuItem("smartEDogue", "Use E evade")).SetValue(true);
            Config.SubMenu("aShots").AddItem(new MenuItem("smartEDogueDanger", "If Danger level >=")).SetValue(new Slider(1, 5, 1));
            Config.SubMenu("aShots").AddItem(new MenuItem("wwDanger", "Only Dangerous")).SetValue(false);
            skillShotMenu = getSkilshotMenu();
            Config.SubMenu("aShots").AddSubMenu(skillShotMenu);

            Config.SubMenu("aShots").AddSubMenu(new Menu("Use whitelist W in combo", "ww"));
            foreach (var hero in HeroManager.Enemies.Where(x => x.IsEnemy))
            {
                Config.SubMenu("aShots").SubMenu("ww").AddItem(new MenuItem(hero.ChampionName, "Use W melee vs " + hero.ChampionName)).SetValue(true);
            }
            //Harass / AutoQ
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("AutoQHarass", "AutoQ Harass")).SetValue(new KeyBind('L', KeyBindType.Toggle, false));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassQ", "Use Q")).SetValue(true);
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassQ3", "Use Q3")).SetValue(true);
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassTower", "Enable UnderTower")).SetValue(true);
            //LastHit
            Config.AddSubMenu(new Menu("LastHit", "LastHit"));
            Config.SubMenu("LastHit").AddItem(new MenuItem("LastHitQ1", "Use Q")).SetValue(true);
            Config.SubMenu("LastHit").AddItem(new MenuItem("LastHitQ3", "Use Q3")).SetValue(true);
            Config.SubMenu("LastHit").AddItem(new MenuItem("LastHitE", "Use E")).SetValue(true);
            //LaneClear
            Config.AddSubMenu(new Menu("Clear", "Clear"));
            Config.SubMenu("Clear").AddItem(new MenuItem("LaneClearQ1", "Use Q")).SetValue(true);
            Config.SubMenu("Clear").AddItem(new MenuItem("LaneClearQ3", "Use Q3")).SetValue(true);
            Config.SubMenu("Clear").AddItem(new MenuItem("LaneClearQ3count", "Use Q3 if hit X minions >= ")).SetValue(new Slider(2, 5, 1));
            Config.SubMenu("Clear").AddItem(new MenuItem("LaneClearE", "Use E")).SetValue(true);
            Config.SubMenu("Clear").AddItem(new MenuItem("LaneClearItems", "Use Items")).SetValue(true);
            //JungleClear
            Config.AddSubMenu(new Menu("JungleClear", "JungleClear"));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("JungleClearQ12", "Use Q")).SetValue(true);
            Config.SubMenu("JungleClear").AddItem(new MenuItem("JungleClearQ3", "Use Q3")).SetValue(true);
            Config.SubMenu("JungleClear").AddItem(new MenuItem("JungleClearE", "Use E")).SetValue(true);
            //Flee away
            Config.AddSubMenu(new Menu("Flee", "Escape"));
            Config.SubMenu("Escape").AddItem(new MenuItem("flee", "FleeMode")).SetValue(new KeyBind('Z', KeyBindType.Press, false));
            //  Config.SubMenu("Escape").AddItem(new MenuItem("wall", "跳墙（尽量勿改动）")).SetValue(new KeyBind('A', KeyBindType.Press, false));

            //Misc
            Config.AddSubMenu(new Menu("Misc", "misc"));
            Config.SubMenu("misc").AddItem(new MenuItem("KS", "Use KillSteal")).SetValue(true);
            Config.SubMenu("misc").AddItem(new MenuItem("IntAnt", "Q3 Interrupter/Antigupcloser")).SetValue(true);
            //draw
            Config.AddSubMenu(new Menu("Draw", "Draw"));
            Config.SubMenu("Draw").AddItem(new MenuItem("Disable", "Disable Drawings")).SetValue(false);
            Config.SubMenu("Draw").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            Config.SubMenu("Draw").AddItem(new MenuItem("DrawQ3", "Draw Q3")).SetValue(true);
            Config.SubMenu("Draw").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            Config.SubMenu("Draw").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            Config.SubMenu("Draw").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
            //   Config.SubMenu("Draw").AddItem(new MenuItem("DrawSpots", "Draw Spots")).SetValue(false);


            Config.AddItem(new MenuItem("EQ3F", "E-Q3-Flash")).SetValue(new KeyBind('G', KeyBindType.Press, false));

            Config.AddToMainMenu();
     
            SkillshotDetector.OnDetectSkillshot += OnDetectSkillshot;
            SkillshotDetector.OnDeleteMissile += OnDeleteMissile;
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            GameObject.OnDelete += Obj_AI_Base_OnDelete;
            CustomEvents.Unit.OnDash += UnitOnOnDash;
         //   Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private static void Obj_AI_Base_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender is MissileClient)
            {
                MissileClient missle = (MissileClient)sender;
                if (missle.SData.Name == "yasuowmovingwallmisl")
                {
                    wall.setL(missle);
                }
                if (missle.SData.Name == "yasuowmovingwallmisl")
                {
                    wallCasted = false;
                }
                if (missle.SData.Name == "yasuowmovingwallmisr")
                {
                    wall.setR(missle);
                }
            }
        }

        private static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender is MissileClient)
            {
                MissileClient missle = (MissileClient)sender;
                if (missle.SData.Name == "yasuowmovingwallmisl")
                {
                    wall.setL(missle);
                }
                if (missle.SData.Name == "yasuowmovingwallmisl")
                {
                    wallCasted = true;
                }
                if (missle.SData.Name == "yasuowmovingwallmisr")
                {
                    wall.setR(missle);
                }
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var target = gapcloser.Sender;

            if (!target.IsValidTarget(Q3.Range))
            {
                return;
            }

            if (Q3.IsReady() && Q3READY() && Config.Item("IntAnt").GetValue<bool>())
            {
                Q3.Cast(target);
            }
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender != null && Q3.IsReady() && Q3READY() && sender.IsValidTarget(Q3.Range) && Config.Item("IntAnt").GetValue<bool>())
            {
                Q3.Cast(sender);
            }
        }

        private static void UnitOnOnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            if (!sender.IsMe || !args.IsBlink)
            {
                return;
            }
            isDashing = true;
            lastE = Environment.TickCount;
            Utility.DelayAction.Add(300, () => { if (myHero.IsDashing()) { isDashing = false; } });

            Utility.DelayAction.Add(450, () => isDashing = false);
        }
        private static void Game_OnUpdate(EventArgs args)
        {
            if (myHero.IsDead || myHero.IsRecalling())
            {
                return;
            }

            EvadeDetectedSkillshots.RemoveAll(skillshot => !skillshot.IsActive());

            foreach (var mis in EvadeDetectedSkillshots)
            {
                if (Config.Item("smartW").GetValue<bool>())
                    useWSmart(mis);

                if (Config.Item("smartEDogue").GetValue<bool>() && !W.IsReady() && !isSafePoint(ObjectManager.Player.Position.To2D(), true).IsSafe)
                    useEtoSafe(mis);
            }
            if (Config.Item("flee").GetValue<KeyBind>().Active)
            {
                Flee();
                AutoQFlee();
                myHero.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
            //if (Config.Item("wall").GetValue<KeyBind>().Active)
            //{
            //        Yasuo.WallJump();
            //        Yasuo.WallDash();
            //}
    
            if (Config.Item("AutoQToggle").GetValue<KeyBind>().Active && Orbwalker.ActiveMode == Orbwalk.OrbwalkingMode.None && !Config.Item("flee").GetValue<KeyBind>().Active && !Config.Item("EQ3F").GetValue<KeyBind>().Active)
            {
                AutoQ();
            }
            if (Orbwalker.ActiveMode == Orbwalk.OrbwalkingMode.Combo)
            {
                Combo();
            }
            if (Orbwalker.ActiveMode == Orbwalk.OrbwalkingMode.LaneClear)
            {
                LaneClear();
            }
            if (Orbwalker.ActiveMode == Orbwalk.OrbwalkingMode.LastHit)
            {
                LastHit();
            }
            if (Orbwalker.ActiveMode == Orbwalk.OrbwalkingMode.Mixed)
            {
                Harass();
                HarassLastHit();
            }
            if (Orbwalker.ActiveMode == Orbwalk.OrbwalkingMode.None)
            {
              if(Config.Item("EQ3F").GetValue<KeyBind>().Active)
                {
                    myHero.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                    BeyBlade();
                }
            }

            AutoR();
            KillSteal();

            if (!IsDashing && Config.Item("AutoQHarass").GetValue<KeyBind>().Active && Orbwalker.ActiveMode != Orbwalk.OrbwalkingMode.Combo)
            {
                var TsTarget = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);

                if (TsTarget != null && TsTarget.CharData.BaseSkinName != "gangplankbarrel" && Config.Item("HarassTower").GetValue<bool>() && Q3.IsReady() && Config.Item("HarassQ3").GetValue<bool>() && !IsDashing && Q3READY() && Q3.IsInRange(TsTarget))
                {
                    CastQ3(TsTarget);
                }
                else if (TsTarget != null && !Config.Item("HarassTower").GetValue<bool>() && TsTarget.CharData.BaseSkinName != "gangplankbarrel" && !UnderTower(myHero.ServerPosition.To2D()) && Q3.IsReady() && Config.Item("HarassQ3").GetValue<bool>() && !IsDashing && Q3READY() && Q3.IsInRange(TsTarget))
                {
                    CastQ3(TsTarget);
                }
            }

            if (!IsDashing && Config.Item("AutoQHarass").GetValue<KeyBind>().Active && Orbwalker.ActiveMode != Orbwalk.OrbwalkingMode.Combo)
            {
                var TsTarget = TargetSelector.GetTarget(475, TargetSelector.DamageType.Physical);

                if (TsTarget != null && TsTarget.CharData.BaseSkinName != "gangplankbarrel" && Config.Item("HarassTower").GetValue<bool>() && !Q3READY() && Q.IsReady() && Config.Item("HarassQ").GetValue<bool>() && !IsDashing && Q.IsInRange(TsTarget))
                {
                    CastQ12(TsTarget);
                }
                else if (TsTarget != null && TsTarget.CharData.BaseSkinName != "gangplankbarrel" && !Config.Item("HarassTower").GetValue<bool>() && !Q3READY() && Q.IsReady() && Config.Item("HarassQ").GetValue<bool>() && !IsDashing && !UnderTower(myHero.ServerPosition.To2D()) && Q.IsInRange(TsTarget))
                {
                    CastQ12(TsTarget);
                }
            }
        }

        private static void BeyBlade()
        {
             Flash = ObjectManager.Player.GetSpellSlot("SummonerFlash");
            if (Flash == SpellSlot.Unknown || !Flash.IsReady() || !Q.IsReady() || !Q3READY())
            {
                return;
            }
            if (ObjectManager.Player.IsDashing())
            {

                var pos =
                    FlashPoints().Where(x => HeroManager.Enemies.Count(a => a.IsValidTarget(600f)) > 0)
                        .OrderByDescending(x => HeroManager.Enemies.Count(i => i.Distance(x, true) <= 220 * 220))
                        .FirstOrDefault();

                if (pos.IsValid() && Q3.Cast(pos, true))
                {
                    Utility.DelayAction.Add(20, () => ObjectManager.Player.Spellbook.CastSpell(Flash, pos));
                }
            }
            if (!E.IsReady())
            {
                return;
            }
            var allTargets = new List<Obj_AI_Base>();

            allTargets.AddRange(MinionManager.GetMinions(ObjectManager.Player.Position, E.Range + 65, MinionTypes.All,
                MinionTeam.NotAlly));
            allTargets.AddRange(HeroManager.Enemies.Where(x => !x.IsDead && x.DistanceToPlayer() <= E.Range + 65));

            if (allTargets.Any())
            {
                var obj =
                    allTargets.Where(x => x.IsValidTarget(E.Range + 50))
                        .OrderByDescending(
                            i =>
                                HeroManager.Enemies.Count(
                                    t => t.IsValidTarget(600f, true, GetPosAfterDash(i))))
                        .FirstOrDefault();

                if (obj != null)
                {
                    E.CastOnUnit(obj, true);
                }
            }
        }

        private static Vector3 GetPosAfterDash(Obj_AI_Base target)
        {
            return ObjectManager.Player.IsFacing(target)
                       ? ObjectManager.Player.ServerPosition.Extend(target.ServerPosition, 475f)
                       : ObjectManager.Player.ServerPosition.Extend(Prediction.GetPrediction(target, 350f).UnitPosition, 475f);
        }

        private static List<Vector3> FlashPoints()
        {
            var points = new List<Vector3>();

            for (var i = 1; i <= 360; i++)
            {
                var angle = i * 2 * Math.PI / 360;
                var point = new Vector2(ObjectManager.Player.Position.X + 425f * (float)Math.Cos(angle),
                    ObjectManager.Player.Position.Y + 425f * (float)Math.Sin(angle)).To3D();

                points.Add(point);
            }

            return points;
        }


        public static void HarassLastHit()
        {
            foreach (Obj_AI_Base minion in MinionManager.GetMinions(myHero.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy).OrderByDescending(m => m.Health))
            {
                if (minion == null)
                {
                    return;
                }

                if (!minion.IsDead && minion != null && Config.Item("LastHitQ1").GetValue<bool>() && Q.IsReady() && minion.IsValidTarget(500) && !Q3READY() && Q.IsInRange(minion))
                {
                    var predHealth = HealthPrediction.GetHealthPrediction(minion, (int)(myHero.Distance(minion.Position) * 1000 / 2000));
                    if (predHealth <= GetQDmg(minion))
                    {
                        CastQ12(minion);
                    }
                }
            }
        }

        public static void Harass()
        {
            var TsTarget = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Physical);

            if (TsTarget == null || TsTarget.CharData.BaseSkinName == "gangplankbarrel")
            {
                return;
            }
            if (TsTarget != null && Config.Item("HarassTower").GetValue<bool>())
            {

                if (Q3.IsReady() && Config.Item("HarassQ3").GetValue<bool>() && !IsDashing && Q3READY() && Q3.IsInRange(TsTarget))
                {
                    CastQ3(TsTarget);
                }
                else if (!Q3READY() && Q.IsReady() && Config.Item("HarassQ").GetValue<bool>() && !IsDashing && Q.IsInRange(TsTarget))
                {
                    CastQ12(TsTarget);
                }
            }
            else if (TsTarget != null && !Config.Item("HarassTower").GetValue<bool>())
            {
                if (!UnderTower(myHero.ServerPosition.To2D()) && Q3.IsReady() && Config.Item("HarassQ3").GetValue<bool>() && !IsDashing &&
                    Q3READY() && Q3.IsInRange(TsTarget))
                {
                    CastQ3(TsTarget);
                }
                if (!Q3READY() && Q.IsReady() && Config.Item("HarassQ").GetValue<bool>() && !IsDashing && !UnderTower(myHero.ServerPosition.To2D()) && Q.IsInRange(TsTarget))
                {
                    CastQ12(TsTarget);
                }
            }
        }
        public static void Flee()
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range);
            if (E.IsReady())
            {
                var bestMinion =
                   ObjectManager.Get<Obj_AI_Base>()
                       .Where(x => x.IsValidTarget(E.Range))
                       .Where(x => x.Distance(Game.CursorPos) < ObjectManager.Player.Distance(Game.CursorPos))
                       .OrderByDescending(x => x.Distance(ObjectManager.Player))
                       .FirstOrDefault();

                if (bestMinion != null  && CanCastE(bestMinion) && E.IsReady())
                {
                    E.CastOnUnit(bestMinion, true);
                }
            }
        }
        public static void AutoQFlee()
        {
            var TsTarget = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);
            List<Obj_AI_Base> Qminion = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 1000, MinionTypes.All, MinionTeam.NotAlly);
            foreach (var minion in Qminion.Where(minion => minion.IsValidTarget(Q.Range) && !minion.IsDead))
            {
                if (minion == null)
                {
                    return;
                }
                if ((!Q3READY()|| !TsTarget.IsValidTarget(Q3.Range)) && Config.Item("AutoQ1").GetValue<bool>() && minion.IsValidTarget(Q.Range) && IsDashing )
                {
                    Q.Cast(new Vector3(99999, 99999, 99999));
                }
            }

            //if (!Q3.IsReady() && IsDashing)
            //{
            //    // CastQ12(minion);
            //    if (TsTarget is Obj_AI_Hero)
            //    {
            //        Q.Cast((ObjectManager.Player.Position).Extend(TsTarget.Position, 999999));
            //    }
            //    else
            //    {
            //        Q.Cast(new Vector3(99999, 99999, 99999));
            //    }
            //}

            if (Q3.IsReady() && IsDashing && myHero.Distance(TsTarget, true) <= 220 * 220 && Q3READY())
            {
                Utility.DelayAction.Add(200, () => { CastQ3(TsTarget); });
            }
            else if (IsDashing && myHero.Distance(TsTarget, true) <= 220 * 220)
            {
                Q.Cast(new Vector3(99999, 99999, 99999));
            }

            if (Q3READY() && Q3.IsReady() && TsTarget.IsValidTarget(Q3.Range) && !IsDashing)
            {
                PredictionOutput Q3Pred = Q3.GetPrediction(TsTarget);
                if (Q3.IsInRange(TsTarget) && Q3Pred.Hitchance >= HitChance.Medium)
                {
                    Q3.Cast(Q3Pred.CastPosition, true);
                }
            }
        }
        public static void AutoQ()
        {
            List<Obj_AI_Base> Qminion = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 1000, MinionTypes.All, MinionTeam.NotAlly);
            foreach (var minion in Qminion.Where(minion => minion.IsValidTarget(Q.Range) && !minion.IsDead))
            {
                if (minion == null && minion.CharData.BaseSkinName == "gangplankbarrel")
                {
                    return;
                }
                if (!Q3READY() && Config.Item("AutoQ1").GetValue<bool>() && minion.IsValidTarget(Q.Range) && !IsDashing)
                {
                    CastQ12(minion);
                }
            }
        }
        public static void LastHit()
        {
            foreach (Obj_AI_Base minion in MinionManager.GetMinions(myHero.ServerPosition, Q3.Range, MinionTypes.All, MinionTeam.Enemy).OrderByDescending(m => m.Health))
            {
                if (minion == null)
                {
                    return;
                }

                if (!minion.IsDead && minion != null && Config.Item("LastHitQ1").GetValue<bool>() && Q.IsReady() && minion.IsValidTarget(500) && !Q3READY() && Q.IsInRange(minion) && !IsDashing)
                {
                    var predHealth = HealthPrediction.GetHealthPrediction(minion, (int)(myHero.Distance(minion.Position) * 1000 / 2000));
                    if (predHealth <= GetQDmg(minion))
                    {
                        CastQ12(minion);
                    }
                }
                if (!minion.IsDead && minion != null && Config.Item("LastHitQ3").GetValue<bool>() && Q.IsReady() && minion.IsValidTarget(1100) && Q3READY() && Q3.IsInRange(minion) && !IsDashing)
                {
                    var predHealth = HealthPrediction.GetHealthPrediction(minion, (int)(myHero.Distance(minion.Position) * 1000 / 2000));
                    if (predHealth <= GetQDmg(minion))
                    {
                        CastQ3(minion);
                    }
                }
                if (Config.Item("LastHitE").GetValue<bool>() && E.IsReady() && minion.IsValidTarget(475))
                {
                    if (!UnderTower(PosAfterE(minion)) && CanCastE(minion))
                    {
                        var predHealth = HealthPrediction.GetHealthPrediction(minion, (int)(myHero.Distance(minion.Position) * 1000 / 2000));
                        if (predHealth <= GetEDmg(minion) && !isDangerous(minion, 600))
                        {
                            E.CastOnUnit(minion, true);
                        }
                    }
                }
            }
        }
        public static void LaneClear()
        {
            List<Obj_AI_Base> Qminion = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 1000, MinionTypes.All, MinionTeam.Enemy);
            foreach (var minion in Qminion.Where(minion => minion.IsValidTarget(Q3.Range)))
            {
                if (minion == null)
                {
                    return;
                }
                if (minion != null && Config.Item("LaneClearQ1").GetValue<bool>() && Q.IsReady() && minion.IsValidTarget(500) && !Q3READY() && Q.IsInRange(minion))
                {
                    var predHealth = HealthPrediction.LaneClearHealthPrediction(minion, (int)(myHero.Distance(minion.Position) * 1000 / 2000));
                    if (predHealth <= GetQDmg(minion) && !IsDashing)
                    {
                        CastQ12(minion);
                    }
                    else if (!Q3READY() && Q.IsInRange(minion) && !IsDashing)
                    {
                        List<Vector2> minionPs = GetCastMinionsPredictedPositions(Qminion, .025f, 50f, float.MaxValue, myHero.ServerPosition, 475f, false, SkillshotType.SkillshotLine);
                        MinionManager.FarmLocation farm = Q.GetLineFarmLocation(minionPs);
                        if (farm.MinionsHit >= 1)
                        {
                            CastQ12(minion);
                        }
                        else if (Q.IsReady() && IsDashing && Q.IsInRange(minion))
                        {
                            if (farm.MinionsHit >= 2)
                            {
                              //  CastQ12(minion);
                                Q.Cast((ObjectManager.Player.Position).Extend(minion.Position, 80000));
                            }
                        }
                    }
                }
                if (!minion.IsDead && minion != null && Config.Item("LaneClearQ3").GetValue<bool>() && Q3.IsReady() && minion.IsValidTarget(1100) && Q3READY() && Q3.IsInRange(minion))
                {
                    var predHealth = HealthPrediction.LaneClearHealthPrediction(minion, (int)(myHero.Distance(minion.Position) * 1000 / 2000));
                    if (predHealth <= GetQDmg(minion) && !IsDashing)
                    {
                        CastQ3(minion);
                    }
                    else if (Q3READY() && Q3.IsInRange(minion) && !IsDashing)
                    {
                        List<Vector2> minionPs = GetCastMinionsPredictedPositions(Qminion, .025f, 50f, float.MaxValue, myHero.ServerPosition, 1000f, false, SkillshotType.SkillshotLine);
                        MinionManager.FarmLocation farm = Q3.GetLineFarmLocation(minionPs);
                        if (farm.MinionsHit >= Config.Item("LaneClearQ3count").GetValue<Slider>().Value)
                        {
                            CastQ3(minion);
                        }
                        else if (Q3.IsReady() && IsDashing && Q.IsInRange(minion) && Q3READY())
                        {
                            if (farm.MinionsHit >= 2)
                            {
                                CastQ3(minion);
                            }
                        }
                    }
                }
            }
            var allMinionsE = MinionManager.GetMinions(myHero.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy);
            foreach (var minion in allMinionsE.Where(x => x.IsValidTarget(E.Range) && CanCastE(x)))
            {
                if (minion == null)
                {
                    return;
                }

                if (Config.Item("LaneClearE").GetValue<bool>() && E.IsReady() && minion.IsValidTarget(E.Range) && CanCastE(minion))
                {
                    if (!UnderTower(PosAfterE(minion)))
                    {

                        var predHealth = HealthPrediction.LaneClearHealthPrediction(minion, (int)(myHero.Distance(minion.Position) * 1000 / 2000));
                        if (predHealth <= GetEDmg(minion) && !isDangerous(minion, 600))
                        {
                            E.CastOnUnit(minion, true);
                        }
                    }
                }
                if (Config.Item("LaneClearItems").GetValue<bool>())
                {
                    UseItems(minion);
                }
            }
            var jminions = MinionManager.GetMinions(myHero.ServerPosition, 1000, MinionTypes.All, MinionTeam.Neutral);
            foreach (var jungleMobs in jminions.Where(x => x.IsValidTarget(Q3.Range)))
            {
                if (jungleMobs == null)
                {
                    return;
                }
                if (Config.Item("JungleClearE").GetValue<bool>() && E.IsReady() && jungleMobs != null && jungleMobs.IsValidTarget(E.Range) && CanCastE(jungleMobs))
                {
                    E.CastOnUnit(jungleMobs);
                }
                if (jungleMobs != null && Config.Item("JungleClearQ3").GetValue<bool>() && Q3.IsReady() && jungleMobs.IsValidTarget(1000) && Q3READY() && Q3.IsInRange(jungleMobs))
                {
                    CastQ3(jungleMobs);
                }
                if (jungleMobs != null && Config.Item("JungleClearQ12").GetValue<bool>() && Q.IsReady() && jungleMobs.IsValidTarget(500) && !Q3READY() && Q.IsInRange(jungleMobs))
                {
                    CastQ12(jungleMobs);
                }
            }
        }
        public static void UseItems(Obj_AI_Base unit)
        {
            if (Items.HasItem((int)ItemId.Youmuus_Ghostblade, myHero) && Items.CanUseItem((int)ItemId.Youmuus_Ghostblade)
               && myHero.Distance(unit.Position) <= Q.Range)
            {
                Items.UseItem((int)ItemId.Youmuus_Ghostblade);
            }
            if (Items.HasItem((int)ItemId.Ironspike_Whip, myHero) && Items.CanUseItem((int)ItemId.Ironspike_Whip)
               && myHero.Distance(unit.Position) <= 400)
            {
                Items.UseItem((int)ItemId.Ironspike_Whip);
            }
            if (Items.HasItem((int)ItemId.Goredrinker, myHero) && Items.CanUseItem((int)ItemId.Goredrinker)
               && myHero.Distance(unit.Position) <= 400)
            {
                Items.UseItem((int)ItemId.Goredrinker);
            }
            if (Items.HasItem((int)ItemId.Randuins_Omen, myHero) && Items.CanUseItem((int)ItemId.Randuins_Omen)
               && myHero.Distance(unit.Position) <= 400)
            {
                Items.UseItem((int)ItemId.Randuins_Omen);
            }
        }
        public static void AutoR()
        {
            if (!R.IsReady())
            {
                return;
            }

            var useR = Config.Item("R4").GetValue<bool>();
            var autoREnemies = Config.Item("R5").GetValue<Slider>().Value;
            var MyHP = Config.Item("R7").GetValue<Slider>().Value;
            var enemyInRange = Config.Item("R6").GetValue<Slider>().Value;
            //var useRDown = SubMenu["Combo"]["AutoR3"].Cast<Slider>().CurrentValue;

            if (!useR)
            {
                return;
            }

            var enemiesKnockedUp =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsValidTarget(R.Range))
                    .Where(x => x.HasBuffOfType(BuffType.Knockup) || x.HasBuffOfType(BuffType.Knockback) && x.IsEnemy);

            var enemies = enemiesKnockedUp as IList<Obj_AI_Hero> ?? enemiesKnockedUp.ToList();

            if (enemies.Count() >= autoREnemies && myHero.Health >= MyHP && myHero.CountEnemiesInRange(1500) <= enemyInRange)
            {
                R.Cast(enemies.FirstOrDefault().ServerPosition);
            }
        }
        public static void Combo()
        {
            var TsTarget = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Physical);
            Orbwalker.ForceTarget(TsTarget);

            if (TsTarget == null || TsTarget.CharData.BaseSkinName == "gangplankbarrel")
            {
                return;
            }
            if (TsTarget != null && Config.Item("QC").GetValue<bool>())
            {
                if (Q3READY() && Q3.IsReady() && TsTarget.IsValidTarget(Q3.Range) && !IsDashing)
                {                  
                    PredictionOutput Q3Pred = Q3.GetPrediction(TsTarget);
                    if (Q3.IsInRange(TsTarget) && Q3Pred.Hitchance >= HitChance.High) 
                    {
                        Q3.Cast(Q3Pred.CastPosition, true);
                    }                   
                }
                if (!Q3READY() && Q.IsReady() && TsTarget.IsValidTarget(Q.Range))
                {
                    if (IsDashing)
                    {
                        Q.Cast((ObjectManager.Player.Position).Extend(TsTarget.Position, 999999));
                    }
                    else
                    {
                        PredictionOutput QPred = Q.GetPrediction(TsTarget);
                        if (Q.IsInRange(TsTarget) && QPred.Hitchance >= HitChance.High)
                        {
                            Q.Cast(QPred.CastPosition, true);
                        }
                    }
                } 
            }
            if (Config.Item("smartW").GetValue<bool>())
            {
                putWallBehind(TsTarget);
            }
            if (Config.Item("smartW").GetValue<bool>() && wallCasted && myHero.Distance(TsTarget.Position) < 300)
            {
                eBehindWall(TsTarget);
            }
            if (Config.Item("EC").GetValue<bool>() && TsTarget != null)
            {
                var dmg = ((float)myHero.GetSpellDamage(TsTarget, SpellSlot.Q) + (float)myHero.GetSpellDamage(TsTarget, SpellSlot.E) + (float)myHero.GetSpellDamage(TsTarget, SpellSlot.R));
                if (E.IsReady() && TsTarget.Distance(myHero) >= (Config.Item("E1").GetValue<Slider>().Value) && dmg >= TsTarget.Health && UnderTower(PosAfterE(TsTarget)) && CanCastE(TsTarget) && myHero.IsFacing(TsTarget))
                {
                    E.CastOnUnit(TsTarget);
                }
                else if(TsTarget.Distance(myHero) >= (Config.Item("E1").GetValue<Slider>().Value) && dmg <= TsTarget.Health && CanCastE(TsTarget) && myHero.IsFacing(TsTarget))
                {
                    useENormal(TsTarget);
                }
                else if (Q.IsReady() && IsDashing && Vector3.DistanceSquared(TsTarget.Position, myHero.Position) <= 275 * 275 && !Q3READY())
                {
                    Utility.DelayAction.Add(200, () => {
                        Q.Cast((ObjectManager.Player.Position).Extend(TsTarget.Position, 999999));
                        //  CastQ12(TsTarget); 
                    } );
                }
                else if (Q3.IsReady() && myHero.Distance(TsTarget) <= E.Range && Q3READY() && TsTarget != null && E.IsReady() && CanCastE(TsTarget))
                {
                    E.CastOnUnit(TsTarget, true);
                }
                else if (Q3.IsReady() && IsDashing && Vector3.DistanceSquared(TsTarget.Position, myHero.Position) <= 275 * 275 && Q3READY())
                {
                    Utility.DelayAction.Add(200, () => { CastQ3(TsTarget); });
                }           

                if (Config.Item("ComboEMode", true).GetValue<StringList>().SelectedIndex == 0 && E.IsReady())
                {
                    var bestMinion =
                    ObjectManager.Get<Obj_AI_Base>()
                    .Where(x => x.IsValidTarget(E.Range))
                    .Where(x => x.Distance(TsTarget) < myHero.Distance(TsTarget))
                    .OrderByDescending(x => x.Distance(myHero))
                    .FirstOrDefault();
                    var dmg2 = ((float)myHero.GetSpellDamage(TsTarget, SpellSlot.Q) + (float)myHero.GetSpellDamage(TsTarget, SpellSlot.E) + (float)myHero.GetSpellDamage(TsTarget, SpellSlot.R));
                    if (bestMinion != null && TsTarget != null && dmg2 >= TsTarget.Health && UnderTower(PosAfterE(bestMinion)) && myHero.IsFacing(bestMinion) && TsTarget.Distance(myHero) >= (Config.Item("E2").GetValue<Slider>().Value) && CanCastE(bestMinion) && myHero.IsFacing(bestMinion))
                    {
                        E.CastOnUnit(bestMinion, true);
                    }
                    else if (bestMinion != null && TsTarget != null && dmg2 <= TsTarget.Health && myHero.IsFacing(bestMinion) && TsTarget.Distance(myHero) >= (Config.Item("E2").GetValue<Slider>().Value) && CanCastE(bestMinion) && myHero.IsFacing(bestMinion))
                    {
                        useENormal(bestMinion);
                    }
                }
                if (Config.Item("ComboEMode", true).GetValue<StringList>().SelectedIndex == 1 && E.IsReady())
                {
                       var bestMinion =
                       ObjectManager.Get<Obj_AI_Base>()
                      .Where(x => x.IsValidTarget(E.Range))
                      .Where(x => x.Distance(Game.CursorPos) < ObjectManager.Player.Distance(Game.CursorPos))
                      .OrderByDescending(x => x.Distance(myHero))
                      .FirstOrDefault();

                    var dmg3 = ((float)myHero.GetSpellDamage(TsTarget, SpellSlot.Q) + (float)myHero.GetSpellDamage(TsTarget, SpellSlot.E) + (float)myHero.GetSpellDamage(TsTarget, SpellSlot.R));
                    if (bestMinion != null && TsTarget != null && dmg3 >= TsTarget.Health && UnderTower(PosAfterE(bestMinion)) && myHero.IsFacing(bestMinion) && TsTarget.Distance(myHero) >= (Config.Item("E2").GetValue<Slider>().Value) && CanCastE(bestMinion) && myHero.IsFacing(bestMinion))
                    {
                        E.CastOnUnit(bestMinion, true);
                    }
                    else if (bestMinion != null && TsTarget != null && dmg3 <= TsTarget.Health && myHero.IsFacing(bestMinion) && TsTarget.Distance(myHero) >= (Config.Item("E2").GetValue<Slider>().Value) && CanCastE(bestMinion) && myHero.IsFacing(bestMinion))
                    {
                        useENormal(bestMinion);
                    }
                }
            }
            /*
            if (Config.Item("flash").GetValue<bool>() && E.IsReady() && R.IsReady() && Flash.IsReady())
            {
                if (TsTarget == null)
                {
                    return;
                }
                var flashQ3range = ((Flash.Range + E.Range) - 25);
                if (Flash != null && TsTarget != null && myHero.Distance(TsTarget) <= flashQ3range && TsTarget.CountEnemiesInRange(400) >= Config.Item("flash2").GetValue<Slider>().Value 
                    && myHero.CountAlliesInRange(1000) >= Config.Item("flash3").GetValue<Slider>().Value)
                {
                    myHero.Spellbook.CastSpell(Flash.Slot, TsTarget.ServerPosition);
                    Utility.DelayAction.Add(10, () => { E.CastOnUnit(TsTarget, true); });
                    Utility.DelayAction.Add(200, () => { CastQ3(TsTarget); });
                }
            }
*/

            if (R.IsReady() && Config.Item("R").GetValue<bool>())
            {
                List<Obj_AI_Hero> enemies = HeroManager.Enemies;
                foreach (Obj_AI_Hero enemy in enemies)
                {
                    if (ObjectManager.Player.Distance(enemy) <= 1200)
                    {
                        var enemiesKnockedUp =
                            ObjectManager.Get<Obj_AI_Hero>()
                            .Where(x => x.IsValidTarget(R.Range))
                            .Where(x => x.HasBuffOfType(BuffType.Knockup));

                        var enemiesKnocked = enemiesKnockedUp as IList<Obj_AI_Hero> ?? enemiesKnockedUp.ToList();
                        if (enemy.IsValidTarget(R.Range) && CanCastDelayR(enemy) && enemiesKnocked.Count() >= (Config.Item("R2").GetValue<Slider>().Value))
                        {
                            R.Cast(enemy.ServerPosition);
                        }
                    }
                    if (enemy.IsValidTarget(R.Range))
                    {
                        
                        if (IsKnockedUp(enemy) && CanCastDelayR(enemy) && enemy.Health <= ((Config.Item("R1").GetValue<Slider>().Value / 100 * enemy.MaxHealth) * 1.5f) && Config.Item(enemy.ChampionName).GetValue<bool>())
                        {
                            R.Cast(enemy.ServerPosition);
                        }
                        else if (IsKnockedUp(enemy) && CanCastDelayR(enemy) && enemy.Health >= ((Config.Item("R1").GetValue<Slider>().Value / 100 * enemy.MaxHealth) * 1.5f) && (Config.Item("R3").GetValue<bool>()))
                        {
                            if (AlliesNearTarget(enemy, 600))
                            {
                                R.Cast(enemy.ServerPosition);
                            }
                        }
                    }
                }
                if (Config.Item("Ignite").GetValue<bool>() && TsTarget.Health <= myHero.GetSummonerSpellDamage(TsTarget, Damage.SummonerSpell.Ignite))
                {
                    Ignite.Cast(TsTarget);
                }
                
                if (Config.Item("comboItems").GetValue<bool>() && TsTarget != null && TsTarget.IsValidTarget())
                {
                    UseItems(TsTarget);
                }
            }
        }
        public static void KillSteal()
        {
            foreach (Obj_AI_Hero enemy in HeroManager.Enemies)
            {
                if (enemy.IsValidTarget(Q.Range) && Config.Item("KS").GetValue<bool>())
                {
                    if (Q.IsReady() && !Q3READY())
                    {
                        
                        if (enemy.Health <= GetQDmg(enemy))
                        {
                            CastQ12(enemy);
                        }
                    }
                    if (Q3.IsReady() && Q3READY())
                    {

                        if (enemy.Health <= GetQDmg(enemy))
                        {
                            CastQ3(enemy);
                        }
                    }
                    if (!Q.IsReady() && E.IsReady() && CanCastE(enemy))
                    {
                        
                        if (enemy.Health <= GetEDmg(enemy))
                        {
                            E.CastOnUnit(enemy, true);
                        }
                    }
                    if (Ignite != null && Ignite.IsReady())
                    {
                        
                        if (enemy.Health <= myHero.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite))
                        {
                            Ignite.Cast(enemy);
                        }
                    }
                }
            }
        }
        public static void CastQ12(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }
            PredictionOutput QPred = Q.GetPrediction(target, true);
            if (QPred.Hitchance >= HitChance.High && Q.IsInRange(target))
            {
                Q.Cast(QPred.CastPosition, true);
            }
        }
        public static void CastQ3(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }
            PredictionOutput Q3Pred = Q3.GetPrediction(target, true);
            if (Q3Pred.Hitchance >= HitChance.High && Q3.IsInRange(target))
            {
                Q.Cast(Q3Pred.CastPosition, true);
            }
        }
        public static void CastQ3AoE()
        {
            foreach (Obj_AI_Hero target in HeroManager.Enemies.Where(x => x.IsValidTarget(1100)))
            {
                PredictionOutput Q3Pred = Q3.GetPrediction(target, true);
                if (Q3Pred.Hitchance >= HitChance.High && Q3.IsInRange(target) && Q3Pred.AoeTargetsHitCount >= 2)
                {
                    Q3.Cast(Q3Pred.CastPosition, true);
                }
            }          
        }
        public static bool IsKnockedUp(Obj_AI_Hero target)
        {
            return target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Knockback);
        }

        public static bool AlliesNearTarget(Obj_AI_Base target, float range)
        {
            return HeroManager.Allies.Where(tar => tar.Distance(target) < range).Any(tar => tar != null);
        }
        public static bool isDangerous(Obj_AI_Base target, float range)
        {
            return HeroManager.Enemies.Where(tar => tar.Distance(PosAfterE(target)) < range).Any(tar => tar != null);
        }

        //copy pasta from valvesharp
        private static bool CanCastDelayR(Obj_AI_Hero target)
        {
            var buff = target.Buffs.FirstOrDefault(i => i.Type == BuffType.Knockback || i.Type == BuffType.Knockup);
            return buff != null && buff.EndTime - Game.Time <= (buff.EndTime - buff.StartTime) / (buff.EndTime - buff.StartTime <= 0.5 ? 1.5 : 3);
        }
        public static Vector2 PosAfterE(Obj_AI_Base target)
        {
            return myHero.ServerPosition.Extend(target.ServerPosition, myHero.Distance(target) < 410 ? E.Range : myHero.Distance(target) + 65).To2D();
        }
        public static bool UnderTower(Vector2 pos)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Any(i => i.Health > 0 && i.Distance(pos) <= 950 && i.IsEnemy);
        }
        public static bool IsDashing
        {
            get
            {
                return Environment.TickCount - lastE <= 100 || myHero.IsDashing();
            }
        }
        public static bool Q3READY()
        {
            return ObjectManager.Player.HasBuff("YasuoQ2");
        }

        public static bool CanCastE(Obj_AI_Base target)
        {
            return !target.HasBuff("YasuoDashWrapper");
        }
        public static double GetEDmg(Obj_AI_Base target)
        {
         //   var stacksPassive = myHero.Buffs.Find(b => b.DisplayName.Equals("YasuoDashScalar"));
            var Estacks = ObjectManager.Player.GetBuffCount("yasuodashscalar");
            var damage = ((E.Level * 20) + 50) * (1 + 0.25 * Estacks) + (myHero.FlatMagicDamageMod * 0.6);
            return myHero.CalcDamage(target, Damage.DamageType.Magical, damage);
        }
        public static Vector2 getNextPos(Obj_AI_Hero target)
        {
            Vector2 dashPos = target.Position.To2D();
            if (target.IsMoving && target.Path.Count() != 0)
            {
                Vector2 tpos = target.Position.To2D();
                Vector2 path = target.Path[0].To2D() - tpos;
                path.Normalize();
                dashPos = tpos + (path * 100);
            }
            return dashPos;
        }
        public static void putWallBehind(Obj_AI_Hero target)
        {
            if (!W.IsReady() || !E.IsReady() || target.IsMelee())
                return;
            Vector2 dashPos = getNextPos(target);
            PredictionOutput po = Prediction.GetPrediction(target, 0.5f);

            float dist = myHero.Distance(po.UnitPosition);
            if (!target.IsMoving || myHero.Distance(dashPos) <= dist + 40)
                if (dist < 330 && dist > 100 && W.IsReady() && Config.Item(target.ChampionName).GetValue<bool>())
                {
                    W.Cast(po.UnitPosition, true);
                }
        }

        public static void eBehindWall(Obj_AI_Hero target)
        {
            if (!E.IsReady() || !enemyIsJumpable(target) || target.IsMelee())
                return;
            float dist = myHero.Distance(target);
            var pPos = myHero.Position.To2D();
            Vector2 dashPos = target.Position.To2D();
            if (!target.IsMoving || myHero.Distance(dashPos) <= dist)
            {
                foreach (Obj_AI_Base enemy in ObjectManager.Get<Obj_AI_Base>().Where(enemy => enemyIsJumpable(enemy)))
                {
                    Vector2 posAfterE = pPos + (Vector2.Normalize(enemy.Position.To2D() - pPos) * E.Range);
                    if ((target.Distance(posAfterE) < dist
                        || target.Distance(posAfterE) < Orbwalk.GetRealAutoAttackRange(target) + 100)
                        && goesThroughWall(target.Position, posAfterE.To3D()))
                    {
                        if (useENormal(target))
                            return;
                    }
                }
            }
        }


        public static IsSafeResult isSafePoint(Vector2 point, bool igonre = false)
        {
            var result = new IsSafeResult();
            result.SkillshotList = new List<Skillshot>();
            result.casters = new List<Obj_AI_Base>();


            bool safe = Orbwalker.ActiveMode == Orbwalk.OrbwalkingMode.Combo ||
                        point.To3D().GetEnemiesInRange(500).Count > myHero.HealthPercent % 65;
            if (!safe)
            {
                result.IsSafe = false;
                return result;
            }
            foreach (var skillshot in EvadeDetectedSkillshots)
            {
                if (skillshot.IsDanger(point) && skillshot.IsAboutToHit(500, myHero))
                {
                    result.SkillshotList.Add(skillshot);
                    result.casters.Add(skillshot.Unit);
                }
            }

            result.IsSafe = (result.SkillshotList.Count == 0);
            return result;
        }

        public static Obj_AI_Minion GetCandidates(Obj_AI_Hero player, List<Skillshot> skillshots)
        {
            float currentDashSpeed = 700 + player.MoveSpeed;//At least has to be like this
            IEnumerable<Obj_AI_Minion> minions = ObjectManager.Get<Obj_AI_Minion>();
            Obj_AI_Minion candidate = new Obj_AI_Minion();
            double closest = 10000000000000;
            foreach (Obj_AI_Minion minion in minions)
            {
                if (Vector2.Distance(player.Position.To2D(), minion.Position.To2D()) < 475 && minion.IsEnemy && enemyIsJumpable(minion) && closest > Vector3.DistanceSquared(Game.CursorPos, minion.Position))
                {
                    foreach (Skillshot skillshot in skillshots)
                    {
                        //Get intersection point
                        //  Vector2 intersectionPoint = LineIntersectionPoint(startPos, player.Position.To2D(), endPos, V2E(player.Position, minion.Position, 475));
                        //Time when yasuo will be in intersection point
                        //  float arrivingTime = Vector2.Distance(player.Position.To2D(), intersectionPoint) / currentDashSpeed;
                        //Estimated skillshot position
                        //  Vector2 skillshotPosition = V2E(startPos.To3D(), intersectionPoint.To3D(), speed * arrivingTime);
                        if (skillshot.IsDanger(V2E(player.Position, minion.Position, 475)))
                        {
                            candidate = minion;
                            closest = Vector3.DistanceSquared(Game.CursorPos, minion.Position);
                        }
                    }
                }
            }
            return candidate;
        }

        private static Vector2 V2E(Vector3 from, Vector3 direction, float distance)
        {
            return (from + distance * Vector3.Normalize(direction - from)).To2D();
        }
        public static Vector2 LineIntersectionPoint(Vector2 ps1, Vector2 pe1, Vector2 ps2,
                Vector2 pe2)
        {
            // Get A,B,C of first line - points : ps1 to pe1
            float A1 = pe1.Y - ps1.Y;
            float B1 = ps1.X - pe1.X;
            float C1 = A1 * ps1.X + B1 * ps1.Y;

            // Get A,B,C of second line - points : ps2 to pe2
            float A2 = pe2.Y - ps2.Y;
            float B2 = ps2.X - pe2.X;
            float C2 = A2 * ps2.X + B2 * ps2.Y;

            // Get delta and check if the lines are parallel
            float delta = A1 * B2 - A2 * B1;
            if (delta == 0)
                return new Vector2(-1, -1);

            // now return the Vector2 intersection point
            return new Vector2(
                (B2 * C1 - B1 * C2) / delta,
                (A1 * C2 - A2 * C1) / delta
            );
        }
        public static bool willColide(Skillshot ss, Vector2 from, float speed, Vector2 direction, float radius)
        {
            Vector2 ssVel = ss.Direction.Normalized() * ss.SpellData.MissileSpeed;
            Vector2 dashVel = direction * speed;
            Vector2 a = ssVel - dashVel;//true direction + speed
            Vector2 realFrom = from.Extend(direction, ss.SpellData.Delay + speed);
            if (!ss.IsAboutToHit((int)((dashVel.Length() / 475) * 1000) + Game.Ping + 100, ObjectManager.Player))
                return false;
            if (ss.IsAboutToHit(1000, ObjectManager.Player) && interCir(ss.MissilePosition, ss.MissilePosition.Extend(ss.MissilePosition + a, ss.SpellData.Range + 50), from,
                radius))
                return true;
            return false;
        }
        public static bool interCir(Vector2 E, Vector2 L, Vector2 C, float r)
        {
            Vector2 d = L - E;
            Vector2 f = E - C;

            float a = Vector2.Dot(d, d);
            float b = 2 * Vector2.Dot(f, d);
            float c = Vector2.Dot(f, f) - r * r;

            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                // no intersection
            }
            else
            {
                // ray didn't totally miss sphere,
                // so there is a solution to
                // the equation.

                discriminant = (float)Math.Sqrt(discriminant);

                // either solution may be on or off the ray so need to test both
                // t1 is always the smaller value, because BOTH discriminant and
                // a are nonnegative.
                float t1 = (-b - discriminant) / (2 * a);
                float t2 = (-b + discriminant) / (2 * a);

                // 3x HIT cases:
                //          -o->             --|-->  |            |  --|->
                // Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit), 

                // 3x MISS cases:
                //       ->  o                     o ->              | -> |
                // FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)

                if (t1 >= 0 && t1 <= 1)
                {
                    // t1 is the intersection, and it's closer than t2
                    // (since t1 uses -b - discriminant)
                    // Impale, Poke
                    return true;
                }

                // here t1 didn't intersect so we are either started
                // inside the sphere or completely past it
                if (t2 >= 0 && t2 <= 1)
                {
                    // ExitWound
                    return true;
                }

                // no intn: FallShort, Past, CompletelyInside
                return false;
            }
            return false;
        }
        public static bool wontHitOnDash(Skillshot ss, Obj_AI_Base jumpOn, Skillshot skillShot, Vector2 dashDir)
        {
            float currentDashSpeed = 700 + myHero.MoveSpeed;//At least has to be like this
            //Get intersection point
            Vector2 intersectionPoint = LineIntersectionPoint(myHero.Position.To2D(), V2E(myHero.Position, jumpOn.Position, 475), ss.Start, ss.End);
            //Time when yasuo will be in intersection point
            float arrivingTime = Vector2.Distance(myHero.Position.To2D(), intersectionPoint) / currentDashSpeed;
            //Estimated skillshot position
            Vector2 skillshotPosition = ss.GetMissilePosition((int)(arrivingTime * 1000));
            if (Vector2.DistanceSquared(skillshotPosition, intersectionPoint) <
                (ss.SpellData.Radius + myHero.BoundingRadius) && !willColide(skillShot, myHero.Position.To2D(), 700f + myHero.MoveSpeed, dashDir, myHero.BoundingRadius + skillShot.SpellData.Radius))
                return false;
            return true;
        }

        public static void useEtoSafe(Skillshot skillShot)
        {
            if (!E.IsReady())
                return;
            float closest = float.MaxValue;
            Obj_AI_Base closestTarg = null;
            float currentDashSpeed = 700 + myHero.MoveSpeed;
            foreach (Obj_AI_Base enemy in ObjectManager.Get<Obj_AI_Base>().Where(ob => ob.NetworkId != skillShot.Unit.NetworkId && enemyIsJumpable(ob) && ob.Distance(myHero) < E.Range).OrderBy(ene => ene.Distance(Game.CursorPos, true)))
            {
                var pPos = myHero.Position.To2D();
                Vector2 posAfterE = V2E(myHero.Position, enemy.Position, 475);
                Vector2 dashDir = (posAfterE - myHero.Position.To2D()).Normalized();

                if (isSafePoint(posAfterE).IsSafe && wontHitOnDash(skillShot, enemy, skillShot, dashDir) /*&& skillShot.IsSafePath(new List<Vector2>() { posAfterE }, 0, (int)currentDashSpeed, 0).IsSafe*/)
                {
                    float curDist = Vector2.DistanceSquared(Game.CursorPos.To2D(), posAfterE);
                    if (curDist < closest)
                    {
                        closestTarg = enemy;
                        closest = curDist;
                    }
                }
            }
            if (closestTarg != null && closestTarg.CountEnemiesInRange(600) <= 2 && skillShotMenu.Item("DangerLevel" + skillShot.SpellData.MenuItemName) != null && skillShotMenu.Item("DangerLevel" + skillShot.SpellData.MenuItemName).GetValue<Slider>().Value >= Config.Item("smartEDogueDanger").GetValue<Slider>().Value)
                useENormal(closestTarg);
        }

        public static void useWSmart(Skillshot skillShot)
        {
            //try douge with E if cant windWall
            var WDelay = Config.Item("smartWDelay").GetValue<Slider>().Value;

            if (!W.IsReady() || skillShot.SpellData.Type == SkillShotType.SkillshotCircle || skillShot.SpellData.Type == SkillShotType.SkillshotRing)
                return;
            if (skillShot.IsAboutToHit(WDelay, myHero))
            {
                var sd = SpellDatabase.GetByMissileName(skillShot.SpellData.MissileSpellName);
                if (sd == null)
                    return;

                //If enabled
                if (!EvadeSpellEnabled(sd.MenuItemName))
                    return;

                //if only dangerous
                if (Config.Item("wwDanger").GetValue<bool>() && skillShotIsDangerous(sd.MenuItemName))
                {
                    myHero.Spellbook.CastSpell(SpellSlot.W, skillShot.Start.To3D(), skillShot.Start.To3D());
                }
                if (!Config.Item("wwDanger").GetValue<bool>() && skillShotMenu.Item("DangerLevel" + sd.MenuItemName) != null && skillShotMenu.Item("DangerLevel" + sd.MenuItemName).GetValue<Slider>().Value >= Config.Item("smartWDanger").GetValue<Slider>().Value)
                {
                    myHero.Spellbook.CastSpell(SpellSlot.W, skillShot.Start.To3D(), skillShot.Start.To3D());
                }
            }
        }

        public static bool enemyIsJumpable(Obj_AI_Base enemy, List<Obj_AI_Hero> ignore = null)
        {
            if (enemy.IsValid && enemy.IsEnemy && !enemy.IsInvulnerable && !enemy.MagicImmune && !enemy.IsDead )
            {
                if (ignore != null)
                    foreach (Obj_AI_Hero ign in ignore)
                    {
                        if (ign.NetworkId == enemy.NetworkId)
                            return false;
                    }
                foreach (BuffInstance buff in enemy.Buffs)
                {
                    if (buff.Name == "YasuoDashWrapper")
                        return false;
                }
                return true;
            }
            return false;
        }
        public static void setUpWall()
        {
            if (wall == null)
                return;

        }
    
        public static bool useENormal(Obj_AI_Base target)
        {
            if (!E.IsReady() || target.Distance(myHero) > 470)
                return false;
            Vector2 posAfter = V2E(myHero.Position, target.Position, 475);
            if (!Config.Item("ETower").GetValue<KeyBind>().Active)
            {
                if (isSafePoint(posAfter).IsSafe)
                {
                    E.Cast(target, true, false);
                }
                return true;
            }
            else
            {
                Vector2 pPos = myHero.ServerPosition.To2D();
                Vector2 posAfterE = pPos + (Vector2.Normalize(target.Position.To2D() - pPos) * E.Range);
                if (!(posAfterE.To3D().UnderTurret(true)))
                {
                    if (isSafePoint(posAfter, true).IsSafe)
                    {
                        E.Cast(target, true, false);
                    }
                    return true;
                }
            }
            return false;

        }
        public static bool goesThroughWall(Vector3 vec1, Vector3 vec2)
        {
            if (wall.endtime < Game.Time || wall.pointL == null || wall.pointL == null)
                return false;
            Vector2 inter = LineIntersectionPoint(vec1.To2D(), vec2.To2D(), wall.pointL.Position.To2D(), wall.pointR.Position.To2D());
            float wallW = (300 + 50 * W.Level);
            if (wall.pointL.Position.To2D().Distance(inter) > wallW ||
                wall.pointR.Position.To2D().Distance(inter) > wallW)
                return false;
            var dist = vec1.Distance(vec2);
            if (vec1.To2D().Distance(inter) + vec2.To2D().Distance(inter) - 30 > dist)
                return false;

            return true;
        }
        public static Menu getSkilshotMenu()
        {
            //Create the skillshots submenus.
            var skillShots = new Menu("Enemy Skillshots", "aShotsSkills");

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.Team != ObjectManager.Player.Team)
                {
                    foreach (var spell in SpellDatabase.Spells)
                    {
                        if (spell.ChampionName == hero.ChampionName)
                        {
                            var subMenu = new Menu(spell.MenuItemName, spell.MenuItemName);

                            subMenu.AddItem(
                                new MenuItem("DangerLevel" + spell.MenuItemName, "Danger level").SetValue(
                                    new Slider(spell.DangerValue, 5, 1)));

                            subMenu.AddItem(
                                new MenuItem("IsDangerous" + spell.MenuItemName, "Is Dangerous").SetValue(
                                    spell.IsDangerous));

                            //subMenu.AddItem(new MenuItem("Draw" + spell.MenuItemName, "Draw").SetValue(true));
                            subMenu.AddItem(new MenuItem("Enabled" + spell.MenuItemName, "Enabled").SetValue(true));

                            skillShots.AddSubMenu(subMenu);
                        }
                    }
                }
            }
            return skillShots;
        }

        public static bool skillShotIsDangerous(string Name)
        {
            if (skillShotMenu.Item("IsDangerous" + Name) != null)
            {
                return skillShotMenu.Item("IsDangerous" + Name).GetValue<bool>();
            }
            return true;
        }
        public static bool EvadeSpellEnabled(string Name)
        {
            if (skillShotMenu.Item("Enabled" + Name) != null)
            {
                return skillShotMenu.Item("Enabled" + Name).GetValue<bool>();
            }
            return true;
        }

        public static void updateSkillshots()
        {
            foreach (var ss in EvadeDetectedSkillshots)
            {
                ss.Game_OnGameUpdate();
            }
        }

        private static void OnDeleteMissile(Skillshot skillshot, MissileClient missile)
        {
            if (skillshot.SpellData.SpellName == "VelkozQ")
            {
                var spellData = SpellDatabase.GetByName("VelkozQSplit");
                var direction = skillshot.Direction.Perpendicular();
                if (EvadeDetectedSkillshots.Count(s => s.SpellData.SpellName == "VelkozQSplit") == 0)
                {
                    for (var i = -1; i <= 1; i = i + 2)
                    {
                        var skillshotToAdd = new Skillshot(
                            DetectionType.ProcessSpell, spellData, Environment.TickCount, missile.Position.To2D(),
                            missile.Position.To2D() + i * direction * spellData.Range, skillshot.Unit);
                        EvadeDetectedSkillshots.Add(skillshotToAdd);
                    }
                }
            }
        }
        private static double GetQDmg(Obj_AI_Base target)
        {
            var dmgItem = 0d;
            if (Items.HasItem(3057) && (Items.CanUseItem(3057) || myHero.HasBuff("Sheen")))
            {
                dmgItem = myHero.BaseAttackDamage;
            }
            if (Items.HasItem(3078) && (Items.CanUseItem(3078) || myHero.HasBuff("Sheen")))
            {
                dmgItem = myHero.BaseAttackDamage * 2;
            }
            var damageModifier = 1d;
            var reduction = 0d;
            var result = dmgItem
                         + myHero.TotalAttackDamage * (myHero.Crit >= 0.85f ? (Items.HasItem(3031) ? 1.875 : 1.5) : 1);
            if (Items.HasItem(3153))
            {
                var dmgBotrk = Math.Max(0.08 * target.Health, 10);
                result += target is Obj_AI_Minion ? Math.Min(dmgBotrk, 60) : dmgBotrk;
            }
            var targetHero = target as Obj_AI_Hero;
            if (targetHero != null)
            {
                if (Items.HasItem(3047, targetHero))
                {
                    damageModifier *= 0.9d;
                }
                if (targetHero.ChampionName == "Fizz")
                {
                    reduction += 4 + (targetHero.Level - 1 / 3) * 2;
                }
            }
            return myHero.CalcDamage(
                target,
                Damage.DamageType.Physical,
                20 * Q.Level + (result - reduction) * damageModifier)
                   + (HaveStatik
                          ? myHero.CalcDamage(
                              target,
                              Damage.DamageType.Magical,
                              100 * (myHero.Crit >= 0.85f ? (Items.HasItem(3031) ? 2.25 : 1.8) : 1))
                          : 0);
        }
        private static bool HaveStatik
        {
            get
            {
                return myHero.GetBuffCount("ItemStatikShankCharge") == 100;
            }
        }
        public static List<Vector2> GetCastMinionsPredictedPositions(List<Obj_AI_Base> minions,
            float delay,
            float width,
            float speed,
            Vector3 from,
            float range,
            bool collision,
            SkillshotType stype,
            Vector3 rangeCheckFrom = new Vector3())
        {
            var result = new List<Vector2>();
            from = from.To2D().IsValid() ? from : ObjectManager.Player.ServerPosition;
            foreach (var minion in minions)
            {
                var pos = Prediction.GetPrediction(new PredictionInput
                {
                    Unit = minion,
                    Delay = delay,
                    Radius = width,
                    Speed = speed,
                    From = from,
                    Range = range,
                    Collision = collision,
                    Type = stype,
                    RangeCheckFrom = rangeCheckFrom
                });

                if (pos.Hitchance >= HitChance.High)
                {
                    result.Add(pos.CastPosition.To2D());
                }
            }

            return result;
        }
        private static void OnDetectSkillshot(Skillshot skillshot)
        {
            var alreadyAdded = false;

            foreach (var item in EvadeDetectedSkillshots)
            {
                if (item.SpellData.SpellName == skillshot.SpellData.SpellName &&
                    (item.Unit.NetworkId == skillshot.Unit.NetworkId &&
                     (skillshot.Direction).AngleBetween(item.Direction) < 5 &&
                     (skillshot.Start.Distance(item.Start) < 100 || skillshot.SpellData.FromObjects.Length == 0)))
                {
                    alreadyAdded = true;
                }
            }

            //Check if the skillshot is from an ally.
            if (skillshot.Unit.Team == ObjectManager.Player.Team)
            {
                return;
            }

            //Check if the skillshot is too far away.
            if (skillshot.Start.Distance(ObjectManager.Player.ServerPosition.To2D()) >
                (skillshot.SpellData.Range + skillshot.SpellData.Radius + 1000) * 1.5)
            {
                return;
            }

            //Add the skillshot to the detected skillshot list.
            if (!alreadyAdded)
            {
                //Multiple skillshots like twisted fate Q.
                if (skillshot.DetectionType == DetectionType.ProcessSpell)
                {
                    if (skillshot.SpellData.MultipleNumber != -1)
                    {
                        var originalDirection = skillshot.Direction;

                        for (var i = -(skillshot.SpellData.MultipleNumber - 1) / 2;
                            i <= (skillshot.SpellData.MultipleNumber - 1) / 2;
                            i++)
                        {
                            var end = skillshot.Start +
                                      skillshot.SpellData.Range *
                                      originalDirection.Rotated(skillshot.SpellData.MultipleAngle * i);
                            var skillshotToAdd = new Skillshot(
                                skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                                skillshot.Unit);

                            EvadeDetectedSkillshots.Add(skillshotToAdd);
                        }
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "UFSlash")
                    {
                        skillshot.SpellData.MissileSpeed = 1600 + (int)skillshot.Unit.MoveSpeed;
                    }

                    if (skillshot.SpellData.Invert)
                    {
                        var newDirection = -(skillshot.End - skillshot.Start).Normalized();
                        var end = skillshot.Start + newDirection * skillshot.Start.Distance(skillshot.End);
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                            skillshot.Unit);
                        EvadeDetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.Centered)
                    {
                        var start = skillshot.Start - skillshot.Direction * skillshot.SpellData.Range;
                        var end = skillshot.Start + skillshot.Direction * skillshot.SpellData.Range;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        EvadeDetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "SyndraE" || skillshot.SpellData.SpellName == "syndrae5")
                    {
                        var angle = 60;
                        var edge1 =
                            (skillshot.End - skillshot.Unit.ServerPosition.To2D()).Rotated(
                                -angle / 2 * (float)Math.PI / 180);
                        var edge2 = edge1.Rotated(angle * (float)Math.PI / 180);

                        foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
                        {
                            var v = minion.ServerPosition.To2D() - skillshot.Unit.ServerPosition.To2D();
                            if (minion.Name == "Seed" && edge1.CrossProduct(v) > 0 && v.CrossProduct(edge2) > 0 &&
                                minion.Distance(skillshot.Unit) < 800 &&
                                (minion.Team != ObjectManager.Player.Team))
                            {
                                var start = minion.ServerPosition.To2D();
                                var end = skillshot.Unit.ServerPosition.To2D()
                                    .Extend(
                                        minion.ServerPosition.To2D(),
                                        skillshot.Unit.Distance(minion) > 200 ? 1300 : 1000);

                                var skillshotToAdd = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                                    skillshot.Unit);
                                EvadeDetectedSkillshots.Add(skillshotToAdd);
                            }
                        }
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "AlZaharCalloftheVoid")
                    {
                        var start = skillshot.End - skillshot.Direction.Perpendicular() * 400;
                        var end = skillshot.End + skillshot.Direction.Perpendicular() * 400;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        EvadeDetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "ZiggsQ")
                    {
                        var d1 = skillshot.Start.Distance(skillshot.End);
                        var d2 = d1 * 0.4f;
                        var d3 = d2 * 0.69f;


                        var bounce1SpellData = SpellDatabase.GetByName("ZiggsQBounce1");
                        var bounce2SpellData = SpellDatabase.GetByName("ZiggsQBounce2");

                        var bounce1Pos = skillshot.End + skillshot.Direction * d2;
                        var bounce2Pos = bounce1Pos + skillshot.Direction * d3;

                        bounce1SpellData.Delay =
                            (int)(skillshot.SpellData.Delay + d1 * 1000f / skillshot.SpellData.MissileSpeed + 500);
                        bounce2SpellData.Delay =
                            (int)(bounce1SpellData.Delay + d2 * 1000f / bounce1SpellData.MissileSpeed + 500);

                        var bounce1 = new Skillshot(
                            skillshot.DetectionType, bounce1SpellData, skillshot.StartTick, skillshot.End, bounce1Pos,
                            skillshot.Unit);
                        var bounce2 = new Skillshot(
                            skillshot.DetectionType, bounce2SpellData, skillshot.StartTick, bounce1Pos, bounce2Pos,
                            skillshot.Unit);

                        EvadeDetectedSkillshots.Add(bounce1);
                        EvadeDetectedSkillshots.Add(bounce2);
                    }

                    if (skillshot.SpellData.SpellName == "ZiggsR")
                    {
                        skillshot.SpellData.Delay =
                            (int)(1500 + 1500 * skillshot.End.Distance(skillshot.Start) / skillshot.SpellData.Range);
                    }

                    if (skillshot.SpellData.SpellName == "JarvanIVDragonStrike")
                    {
                        var endPos = new Vector2();

                        foreach (var s in EvadeDetectedSkillshots)
                        {
                            if (s.Unit.NetworkId == skillshot.Unit.NetworkId && s.SpellData.Slot == SpellSlot.E)
                            {
                                endPos = s.End;
                            }
                        }

                        foreach (var m in ObjectManager.Get<Obj_AI_Minion>())
                        {
                            if (m.CharData.BaseSkinName == "jarvanivstandard" && m.Team == skillshot.Unit.Team &&
                                skillshot.IsDanger(m.Position.To2D()))
                            {
                                endPos = m.Position.To2D();
                            }
                        }

                        if (!endPos.IsValid())
                        {
                            return;
                        }

                        skillshot.End = endPos + 200 * (endPos - skillshot.Start).Normalized();
                        skillshot.Direction = (skillshot.End - skillshot.Start).Normalized();
                    }
                }

                if (skillshot.SpellData.SpellName == "OriannasQ")
                {
                    var endCSpellData = SpellDatabase.GetByName("OriannaQend");

                    var skillshotToAdd = new Skillshot(
                        skillshot.DetectionType, endCSpellData, skillshot.StartTick, skillshot.Start, skillshot.End,
                        skillshot.Unit);

                    EvadeDetectedSkillshots.Add(skillshotToAdd);
                }


                //Dont allow fow detection.
                if (skillshot.SpellData.DisableFowDetection && skillshot.DetectionType == DetectionType.RecvPacket)
                {
                    return;
                }

                EvadeDetectedSkillshots.Add(skillshot);
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("Disable").GetValue<bool>())
            {
                return;
            }
            if (Config.Item("DrawQ").GetValue<bool>() && Q.IsReady())
            {
                Render.Circle.DrawCircle(myHero.Position, Q.Range, Color.LightGreen);
            }
            if (Config.Item("DrawQ3").GetValue<bool>() && Q3.IsReady())
            {
                Render.Circle.DrawCircle(myHero.Position, Q3.Range, Color.LightGreen);
            }
            if (Config.Item("DrawW").GetValue<bool>() && W.IsReady())
            {
                Render.Circle.DrawCircle(myHero.Position, W.Range, Color.LightGreen);
            }
            if (Config.Item("DrawE").GetValue<bool>() && E.IsReady())
            {
                Render.Circle.DrawCircle(myHero.Position, E.Range, Color.LightGreen);
            }
            if (Config.Item("DrawR").GetValue<bool>() && R.IsReady())
            {
                Render.Circle.DrawCircle(myHero.Position, R.Range, Color.LightGreen);
            }

            if (!Config.Item("ETower").GetValue<KeyBind>().Active)
            {
                Drawing.DrawText(Drawing.WorldToScreen(myHero.Position)[0] - 10,
                          Drawing.WorldToScreen(myHero.Position)[1] + 30, Color.GreenYellow,
                          "UnderTower[T]:On");
            }
            else {
                Drawing.DrawText(Drawing.WorldToScreen(myHero.Position)[0] - 10,
                         Drawing.WorldToScreen(myHero.Position)[1] + 30, Color.Red,
                         "UnderTower[T]:Off");
            }

            //if (Config.Item("DrawSpots").GetValue<bool>())
            //{
            //    Render.Circle.DrawCircle(Yasuo.spot1.To3D(), 150, Color.Red, 2);
            //    Render.Circle.DrawCircle(Yasuo.spot2.To3D(), 150, Color.Red, 2);
            //    Render.Circle.DrawCircle(Yasuo.spot3.To3D(), 150, Color.Red, 2);
            //    Render.Circle.DrawCircle(Yasuo.spot5.To3D(), 150, Color.Red, 2);
            //    Render.Circle.DrawCircle(Yasuo.spot6.To3D(), 150, Color.Red, 2);
            //    Render.Circle.DrawCircle(Yasuo.spot7.To3D(), 150, Color.Red, 2);
            //    Render.Circle.DrawCircle(Yasuo.spot9.To3D(), 150, Color.Red, 2);
            //    Render.Circle.DrawCircle(Yasuo.spot10.To3D(), 150, Color.Red, 2);
            //    Render.Circle.DrawCircle(Yasuo.spot11.To3D(), 150, Color.Red, 2);
            //    Render.Circle.DrawCircle(Yasuo.spot12.To3D(), 150, Color.Red, 2);
            //    Render.Circle.DrawCircle(Yasuo.spot13.To3D(), 150, Color.Red, 2);
            //    Render.Circle.DrawCircle(Yasuo.spot14.To3D(), 150, Color.Red, 2);
            //    Render.Circle.DrawCircle(Yasuo.spot16.To3D(), 150, Color.Red, 2);

            //    Render.Circle.DrawCircle(Yasuo.spotC.To3D(), 400, Color.Green, 1);
            //    Render.Circle.DrawCircle(Yasuo.spotD.To3D(), 400, Color.Green, 1);
            //    Render.Circle.DrawCircle(Yasuo.spotE.To3D(), 400, Color.Green, 1);
            //    Render.Circle.DrawCircle(Yasuo.spotG.To3D(), 400, Color.Green, 1);
            //    Render.Circle.DrawCircle(Yasuo.spotH.To3D(), 400, Color.Green, 1);
            //    Render.Circle.DrawCircle(Yasuo.spotI.To3D(), 120, Color.Green, 1);
            //    Render.Circle.DrawCircle(Yasuo.spotJ.To3D(), 120, Color.Green, 1);
            //    Render.Circle.DrawCircle(Yasuo.spotL.To3D(), 400, Color.Green, 1);
            //    Render.Circle.DrawCircle(Yasuo.spotN.To3D(), 400, Color.Green, 1);
            //}


        }
    }
}