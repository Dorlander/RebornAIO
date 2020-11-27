using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Geometry = LeagueSharp.Common.Geometry;

namespace YasuoSharpV2
{
    class Yasuo
    {

        public struct IsSafeResult
        {
            public bool IsSafe;
            public List<Skillshot> SkillshotList;
            public List<Obj_AI_Base> casters;
        }

        internal class YasDash
        {
            public Vector3 from = new Vector3(-1, -1, -1);
            public Vector3 to = new Vector3(-1, -1, -1);

            public YasDash()
            {
                from = new Vector3(-1, -1, -1);
                to = new Vector3(-1, -1, -1);
            }

            public YasDash(Vector3 fromV, Vector3 toV)
            {
                from = fromV;
                to = toV;
            }

            public YasDash(YasDash dash)
            {
                from = dash.from;
                to = dash.to;
            }

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
                return pointL != null && pointR != null && endtime-(time/1000) > Game.Time;
            }
        }



        public static List<YasDash> dashes = new List<YasDash>();

        public static YasDash lastDash = new YasDash();

        public static Obj_AI_Hero Player = ObjectManager.Player;

        public static Vector3 test = new Vector3();

        public static Spellbook sBook = Player.Spellbook;

        public static SpellDataInst Qdata = sBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = sBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = sBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = sBook.GetSpell(SpellSlot.R);
        public static Spell Q = new Spell(SpellSlot.Q, 475);
        public static Spell QEmp = new Spell(SpellSlot.Q, 1000);
        public static Spell QCir = new Spell(SpellSlot.Q, 315);
        public static Spell W = new Spell(SpellSlot.W, 400);
        public static Spell E = new Spell(SpellSlot.E, 475);
        public static Spell R = new Spell(SpellSlot.R, 1200);
        //Much Skillshot                    1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8
        public static Spell[] levelUpSeq = { Q, E, W, Q, Q, R, Q, E, Q, E, R, E, W, E, W, R, W, W };

        //Much NotSoMuch                    1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8
        public static Spell[] levelUpSeq2 = { Q, E, Q, W, Q, R, Q, E, Q, E, R, E, W, E, W, R, W, W };

        //Ignore these spells with W
        public static List<string> WIgnore;

        public static Vector3 point1 = new Vector3();
        public static Vector3 point2 = new Vector3();

        public static Vector3 castFrom;
        public static bool isDashigPro = false;
        public static float startDash = 0;
        public static float time = 0;

        public static YasWall wall = new YasWall();


        public static SummonerItems sumItems;

        #region WallDashing

        public static void setDashes()
        {
            #region WallDashingValues
            //botoomside
            dashes.Add(new YasDash(new Vector3(5997.00f, 5065.00f, 51.67f), new Vector3(6447.35f, 5216.45f, 56.11f)));
            dashes.Add(new YasDash(new Vector3(3582.00f, 7936.00f, 53.67f), new Vector3(3845.85f, 7376.56f, 51.56f)));
            dashes.Add(new YasDash(new Vector3(3880.00f, 7978.00f, 51.81f), new Vector3(3824.00f, 7356.00f, 51.50f)));
            dashes.Add(new YasDash(new Vector3(3724.00f, 7408.00f, 51.87f), new Vector3(3631.26f, 7824.56f, 53.78f)));
            dashes.Add(new YasDash(new Vector3(3850.00f, 7968.00f, 51.91f), new Vector3(4042.00f, 7376.00f, 51.00f)));
            dashes.Add(new YasDash(new Vector3(3894.00f, 6446.00f, 52.46f), new Vector3(4480.45f, 6432.95f, 50.77f)));
            dashes.Add(new YasDash(new Vector3(3732.00f, 6528.00f, 52.46f), new Vector3(3732.00f, 7154.00f, 50.53f)));
            dashes.Add(new YasDash(new Vector3(4374.00f, 6258.00f, 51.36f), new Vector3(3946.00f, 6462.00f, 52.46f)));
            dashes.Add(new YasDash(new Vector3(3674.00f, 7058.00f, 50.33f), new Vector3(3734.00f, 6588.00f, 52.46f)));
            dashes.Add(new YasDash(new Vector3(3786.00f, 6534.00f, 52.46f), new Vector3(3470.00f, 6888.00f, 51.15f)));
            dashes.Add(new YasDash(new Vector3(3890.00f, 6520.00f, 52.46f), new Vector3(4258.00f, 6218.00f, 51.94f)));
            dashes.Add(new YasDash(new Vector3(2124.00f, 8506.00f, 51.78f), new Vector3(1880.00f, 7930.00f, 51.43f)));
            dashes.Add(new YasDash(new Vector3(2148.00f, 8370.00f, 51.78f), new Vector3(1690.00f, 8688.00f, 52.66f)));
            dashes.Add(new YasDash(new Vector3(1724.00f, 8156.00f, 52.84f), new Vector3(2108.00f, 8436.00f, 51.78f)));
            dashes.Add(new YasDash(new Vector3(8370.00f, 2698.00f, 51.04f), new Vector3(7977.40f, 3171.12f, 51.58f)));
            dashes.Add(new YasDash(new Vector3(8314.00f, 2678.00f, 51.12f), new Vector3(8376.00f, 3300.00f, 52.56f)));
            dashes.Add(new YasDash(new Vector3(8272.00f, 3208.00f, 51.89f), new Vector3(8324.00f, 2736.00f, 51.13f)));
            dashes.Add(new YasDash(new Vector3(7858.00f, 3912.00f, 53.76f), new Vector3(8362.70f, 3652.84f, 54.42f)));
            dashes.Add(new YasDash(new Vector3(7564.00f, 4112.00f, 54.46f), new Vector3(7686.00f, 4726.00f, 49.53f)));
            dashes.Add(new YasDash(new Vector3(7030.00f, 5460.00f, 54.20f), new Vector3(7410.00f, 5954.00f, 52.48f)));
            dashes.Add(new YasDash(new Vector3(6972.00f, 5508.00f, 55.43f), new Vector3(6395.04f, 5313.03f, 48.53f)));
            dashes.Add(new YasDash(new Vector3(6924.00f, 5492.00f, 54.36f), new Vector3(6334.00f, 5292.00f, 48.53f)));
            dashes.Add(new YasDash(new Vector3(7372.00f, 5858.00f, 52.57f), new Vector3(7062.00f, 5500.00f, 55.03f)));

            #endregion
            sumItems = new SummonerItems(Player);
        }

        public static YasDash getClosestDash(float dist = 350)
        {
            YasDash closestWall = dashes[0];
            for (int i = 1; i < dashes.Count; i++)
            {
                closestWall = closestDashToMouse(closestWall, dashes[i]);
            }
            if (closestWall.to.Distance(Game.CursorPos) < dist)
                return closestWall;
            return null;
        }

        public static YasDash closestDashToMouse(YasDash w1, YasDash w2)
        {
            return Vector3.DistanceSquared(w1.to, Game.CursorPos) + Vector3.DistanceSquared(w1.from, Player.Position) > Vector3.DistanceSquared(w2.to, Game.CursorPos) + Vector3.DistanceSquared(w2.from, Player.Position) ? w2 : w1;
        }

        public static void saveLastDash()
        {
            if (lastDash.from.X != -1 && lastDash.from.Y != -1)
                dashes.Add(new YasDash(lastDash));
            lastDash = new YasDash();
        }

        public static void fleeToMouse()
        {
            try
            {
                YasDash closeDash = getClosestDash();
                if (closeDash != null)
                {
                    List<Obj_AI_Base> jumps = canGoThrough(closeDash);
                    if (jumps.Count > 0 || ((W.IsReady() || (Yasuo.wall != null && (Yasuo.wall.endtime - Game.Time) > 3f))))
                    {
                        var distToDash = Player.Distance(closeDash.from);

                        if (W.IsReady() && distToDash < 136f && jumps.Count == 0 && NavMesh.Equals(closeDash.to, closeDash.to)
                           && MinionManager.GetMinions(Game.CursorPos, 350).Where(min => min.IsVisible).Count() < 2)
                        {
                            W.Cast(closeDash.to);
                        }

                        if (distToDash > 2f)
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, closeDash.from);
                            return;
                        }

                        if (distToDash < 3f && jumps.Count > 0 && jumps.First().Distance(Player)<=473)
                        {
                            E.Cast(jumps.First());
                        }
                        return;
                    }
                }
                if (getClosestDash(400) == null)
                    Yasuo.gapCloseE(Game.CursorPos.To2D());
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static List<Obj_AI_Base> canGoThrough(YasDash dash)
        {
            List<Obj_AI_Base> jumps = ObjectManager.Get<Obj_AI_Base>().Where(enemy => enemyIsJumpable(enemy) && enemy.IsValidTarget(550, true, dash.to)).ToList();
            List<Obj_AI_Base> canBejump = new List<Obj_AI_Base>();
            foreach (var jumpe in jumps)
            {
                if (YasMath.interCir(dash.from.To2D(), dash.to.To2D(), jumpe.Position.To2D(), 35) /*&& jumpe.Distance(dash.to) < Player.Distance(dash.to)*/)
                {
                    canBejump.Add(jumpe);
                }
            }
            return canBejump.OrderBy(jum => Player.Distance(jum)).ToList();
        }


        public static float getLengthTillPos(Vector3 pos)
        {
            return 0;
        }

        #endregion

        public static void setSkillShots()
        {
            Q.SetSkillshot(getNewQSpeed(), 50f, float.MaxValue, false, SkillshotType.SkillshotLine);
            QEmp.SetSkillshot(0.1f, 50f, 1200f, false, SkillshotType.SkillshotLine);
            QCir.SetSkillshot(0f, 375f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        public static float getNewQSpeed()
        {
            float ds = 0.5f;//s
            float a = 1 / ds * Yasuo.Player.AttackSpeedMod;
            return 1 / a;
        }

        public static void doCombo(Obj_AI_Hero target)
        {

            if (target == null) return;
            useHydra(target);
            if (target.Distance(Player) < 500)
            {
                sumItems.cast(SummonerItems.ItemIds.Ghostblade);
            }
            if (target.Distance(Player) < 500 && (Player.Health / Player.MaxHealth) * 100 < 85)
            {
                sumItems.cast(SummonerItems.ItemIds.BotRK, target);

            }
            if (YasuoSharp.Config.Item("smartW").GetValue<bool>())
                putWallBehind(target);
            if (YasuoSharp.Config.Item("useEWall").GetValue<bool>())
                eBehindWall(target);

            Obj_AI_Base goodTarg = canDoEQEasly(target);
            var outPut = Prediction.GetPrediction(goodTarg, 700 + Player.MoveSpeed);
            if (goodTarg != null && outPut.UnitPosition.Distance(Player.Position) <= 470)
            {

                E.Cast(goodTarg);

                SmoothMouse.addMouseEvent(target.Position);
                Q.Cast(target);
            }
            if (!useESmart(target))
            {
                List<Obj_AI_Hero> ignore = new List<Obj_AI_Hero>();
                ignore.Add(target);
                Obj_AI_Base bestos = null;
                gapCloseE(target.Position.To2D());
            }

            useQSmart(target);
        }


        public static void stackQ()
        {
            if (!Q.IsReady() || isQEmpovered() || !YasuoSharp.Config.Item("fleeStack").GetValue<bool>())//fleeStack
                return;
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + 50);
            
            if (!isDashigPro)
            {
                
                List<Vector2> minionPs = YasMath.GetCastMinionsPredictedPositions(minions, getNewQSpeed()*0.3f, 30f,
                    float.MaxValue, Player.ServerPosition, 465, false, SkillshotType.SkillshotLine);
                Vector2 clos = LeagueSharp.Common.Geometry.Closest(Player.ServerPosition.To2D(), minionPs);
                if (Player.Distance(clos) < 475)
                {
                    SmoothMouse.addMouseEvent(clos.To3D());
                    Q.Cast(clos, false);
                    return;
                }
            }
            else
            {
                if (minions.Count(min => !min.IsDead && min.IsValid && min.Distance(getDashEndPos()) < QCir.Range) >
                    1)
                {
                    QCir.Cast(Player.Position, false);
                    return;
                }
            }
        }

        public static Obj_AI_Base canDoEQEasly(Obj_AI_Hero target)
        {
            if (!E.IsReady() || Q.IsReady(150) || !isQEmpovered())
                return null;
            List<Obj_AI_Base> jumps = ObjectManager.Get<Obj_AI_Base>().Where(enemy => enemy.NetworkId != target.NetworkId && enemyIsJumpable(enemy) && enemy.IsValidTarget(470, true)).OrderBy(jp => V2E(Player.Position, jp.Position, 475).Distance(target.Position, true)).ToList();

            if (jumps.Any() && V2E(Player.Position, jumps.First().Position, 475).Distance(target.Position, true) < 250 * 250)
            {
                return jumps.First();
            }
            return null;
        }


        public static Vector2 getNextPos(Obj_AI_Hero target)
        {
            Vector2 dashPos = target.Position.To2D();
            if (target.IsMoving && target.Path.Count()!=0)
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

            float dist = Player.Distance(po.UnitPosition);
            if (!target.IsMoving || Player.Distance(dashPos) <= dist + 40)
                if (dist < 330 && dist > 100 && W.IsReady())
                {
                    SmoothMouse.addMouseEvent(po.UnitPosition);
                    W.Cast(po.UnitPosition);
                }
        }

        public static void eBehindWall(Obj_AI_Hero target)
        {
            if (!E.IsReady() || !enemyIsJumpable(target) || target.IsMelee())
                return;
            float dist = Player.Distance(target);
            var pPos = Player.Position.To2D();
            Vector2 dashPos = target.Position.To2D();
            if (!target.IsMoving || Player.Distance(dashPos) <= dist)
            {
                foreach (Obj_AI_Base enemy in ObjectManager.Get<Obj_AI_Base>().Where(enemy => enemyIsJumpable(enemy)))
                {
                    Vector2 posAfterE = pPos + (Vector2.Normalize(enemy.Position.To2D() - pPos) * E.Range);
                    if ((target.Distance(posAfterE) < dist
                        || target.Distance(posAfterE) < Orbwalking.GetRealAutoAttackRange(target) + 100)
                        && goesThroughWall(target.Position, posAfterE.To3D()))
                    {
                        if (useENormal(target))
                            return;
                    }
                }
            }
        }



        public static bool goesThroughWall(Vector3 vec1, Vector3 vec2)
        {
            if (wall.endtime < Game.Time || wall.pointL == null || wall.pointL == null)
                return false;
            Vector2 inter = YasMath.LineIntersectionPoint(vec1.To2D(), vec2.To2D(), wall.pointL.Position.To2D(), wall.pointR.Position.To2D());
            float wallW = (300 + 50 * W.Level);
            if (wall.pointL.Position.To2D().Distance(inter) > wallW ||
                wall.pointR.Position.To2D().Distance(inter) > wallW)
                return false;
            var dist = vec1.Distance(vec2);
            if (vec1.To2D().Distance(inter) + vec2.To2D().Distance(inter) - 30 > dist)
                return false;

            return true;
        }

        public static void doLastHit(Obj_AI_Hero target)
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + 50);
            foreach (var minion in minions.Where(minion => minion.IsValidTarget(Q.Range)))
            {
                if (Player.Distance(minion) < Orbwalking.GetRealAutoAttackRange(minion) && minion.Health < Player.GetAutoAttackDamage(minion))
                    return;
                if (YasuoSharp.Config.Item("useElh").GetValue<bool>() && minion.Health < Player.GetSpellDamage(minion, E.Slot))
                    useENormal(minion);

                if (YasuoSharp.Config.Item("useQlh").GetValue<bool>() && !isQEmpovered() && minion.Health < Player.GetSpellDamage(minion, Q.Slot))
                    if (!(target != null && isQEmpovered() && Player.Distance(target) < 1050))
                    {
                        if (canCastFarQ())
                        {
                            SmoothMouse.addMouseEvent(minion.Position);
                            Q.Cast(minion);
                        }
                    }
            }
        }

        public static void doLaneClear(Obj_AI_Hero target)
        {
            List<Obj_AI_Base> minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 1000, MinionTypes.All, MinionTeam.NotAlly);
            if (YasuoSharp.Config.Item("useElc").GetValue<bool>() && E.IsReady())
                foreach (var minion in minions.Where(minion => minion.IsValidTarget(E.Range) && enemyIsJumpable(minion)))
                {
                    if (minion.Health < Player.GetSpellDamage(minion, E.Slot)
                        || Q.IsReady() && minion.Health < (Player.GetSpellDamage(minion, E.Slot) + Player.GetSpellDamage(minion, Q.Slot)))
                    {
                        if (useENormal(minion))
                            return;
                    }
                }

            if (Q.IsReady() && YasuoSharp.Config.Item("useQlc").GetValue<bool>())
            {
                if (isQEmpovered() && !(target != null && Player.Distance(target) < 1050))
                {
                    if (canCastFarQ())
                    {
                        List<Vector2> minionPs = YasMath.GetCastMinionsPredictedPositions(minions, getNewQSpeed(), 50f, 1200f, Player.ServerPosition, 900f, false, SkillshotType.SkillshotLine);
                        MinionManager.FarmLocation farm = QEmp.GetLineFarmLocation(minionPs); //MinionManager.GetBestLineFarmLocation(minionPs, 50f, 900f);
                        if (farm.MinionsHit >= YasuoSharp.Config.Item("useEmpQHit").GetValue<Slider>().Value)
                        {
                            //Console.WriteLine("Cast q simp Emp");
                            SmoothMouse.addMouseEvent(farm.Position.To3D());
                            QEmp.Cast(farm.Position, false);
                            return;
                        }
                    }
                    else
                    {
                        if (minions.Count(min => min.IsValid && !min.IsDead && min.Distance(getDashEndPos()) < QCir.Range) >= YasuoSharp.Config.Item("useEmpQHit").GetValue<Slider>().Value)
                        {
                            QCir.Cast(Player.Position, false);
                            Console.WriteLine("Cast q circ simp");
                        }
                    }
                }
                else
                {
                    if (!isDashigPro)
                    {
                        List<Vector2> minionPs = YasMath.GetCastMinionsPredictedPositions(minions, getNewQSpeed() * 0.3f, 30f, float.MaxValue, Player.ServerPosition, 465, false, SkillshotType.SkillshotLine);
                        Vector2 clos = LeagueSharp.Common.Geometry.Closest(Player.ServerPosition.To2D(), minionPs);
                        if (Player.Distance(clos) < 475)
                        {
                            Console.WriteLine("Cast q simp");
                            SmoothMouse.addMouseEvent(clos.To3D());
                            Q.Cast(clos, false);
                            return;
                        }
                    }
                    else
                    {
                        if (minions.Count(min => !min.IsDead && min.IsValid && min.Distance(getDashEndPos()) < QCir.Range) > 1)
                        {
                            QCir.Cast(Player.Position, false);
                            Console.WriteLine("Cast q circ simp");
                            return;
                        }
                    }
                }
            }

        }

        public static void doHarass(Obj_AI_Hero target)
        {
            if (!Player.ServerPosition.UnderTurret(true) || YasuoSharp.Config.Item("harassTower").GetValue<bool>())
                useQSmart(target);
        }



        public static void useHydra(Obj_AI_Base target)
        {

            if ((Items.CanUseItem(3074) || Items.CanUseItem(3074)) && target.Distance(Player.ServerPosition) < (400 + target.BoundingRadius - 20))
            {
                Items.UseItem(3074, target);
                Items.UseItem(3077, target);
            }
        }


        public static Vector3 getDashEndPos()
        {
            Vector2 dashPos2 = Player.GetDashInfo().EndPos;
            return new Vector3(dashPos2, Player.ServerPosition.Z);
        }

        public static bool isQEmpovered()
        {
            return Player.HasBuffIn("yasuoq3w", 0.25f, true);
        }

        public static bool isDashing()
        {
            return isDashigPro;
        }

        public static bool canCastFarQ()
        {
            return !isDashigPro;
        }

        public static bool canCastCircQ()
        {
            return isDashigPro;
        }


        public static List<Obj_AI_Hero> getKockUpEnemies(ref float lessKnockTime)
        {
            List<Obj_AI_Hero> enemKonck = new List<Obj_AI_Hero>();
            foreach (Obj_AI_Hero enem in ObjectManager.Get<Obj_AI_Hero>().Where(enem => enem.IsEnemy))
            {
                foreach (BuffInstance buff in enem.Buffs)
                {
                    if (buff.Type == BuffType.Knockback || buff.Type == BuffType.Knockup)
                    {
                        if (buff.Type == BuffType.Knockup)
                            lessKnockTime = (buff.EndTime - Game.Time) < lessKnockTime
                                ? (buff.EndTime - Game.Time)
                                : lessKnockTime;
                        enemKonck.Add(enem);
                        break;
                    }
                }
            }
            if (!YasuoSharp.Config.Item("useRHitTime").GetValue<bool>())
                lessKnockTime = 0;
            return enemKonck;
        }


        public static void setUpWall()
        {
            if (wall == null)
                return;

        }

        public static void useQSmart(Obj_AI_Hero target, bool onlyEmp = false)
        {
            if (!Q.IsReady() || target == null)
                return;
            if (isQEmpovered())
            {
                if (canCastFarQ())
                {
                    PredictionOutput po = QEmp.GetPrediction(target); //QEmp.GetPrediction(target, true);
                    if (po.Hitchance >= HitChance.Medium)
                    {
                        SmoothMouse.addMouseEvent(po.CastPosition);
                        QEmp.Cast(po.CastPosition);
                        return;
                    }
                }
                else//dashing
                {
                    Vector3 endPos = getDashEndPos();
                    if (Player.Distance(endPos) < 40 && target.Distance(endPos) < QCir.Range)
                    {
                        QCir.Cast(target.Position);
                        return;
                    }
                }
            }
            else if (!onlyEmp)
            {
                if (canCastFarQ())
                {
                    PredictionOutput po = Q.GetPrediction(target);
                    if (po.Hitchance >= HitChance.Medium)
                    {
                        SmoothMouse.addMouseEvent(po.CastPosition);
                        Q.Cast(po.CastPosition);
                    }
                    return;

                }
                else//dashing
                {
                    float trueRange = QCir.Range - 10;
                    Vector3 endPos = getDashEndPos();
                    if (Player.Distance(endPos) < 40 && target.Distance(endPos) < QCir.Range)
                    {
                        QCir.Cast(target.Position);
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
            if (false)
            {
                bool safe = YasuoSharp.Orbwalker.ActiveMode.ToString() == "Combo" ||
                            point.To3D().GetEnemiesInRange(500).Count > Player.HealthPercent%65;
                if (!safe)
                {
                    result.IsSafe = false;
                    return result;
                }
            }
            foreach (var skillshot in YasuoSharp.DetectedSkillshots)
            {
                if (skillshot.IsDanger(point) && skillshot.IsAboutToHit(500,Player))
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

        public static bool wontHitOnDash(Skillshot ss, Obj_AI_Base jumpOn, Skillshot skillShot, Vector2 dashDir)
        {
            float currentDashSpeed = 700 + Player.MoveSpeed;//At least has to be like this
            //Get intersection point
            Vector2 intersectionPoint = YasMath.LineIntersectionPoint(Player.Position.To2D(), V2E(Player.Position, jumpOn.Position, 475), ss.Start, ss.End);
            //Time when yasuo will be in intersection point
            float arrivingTime = Vector2.Distance(Player.Position.To2D(), intersectionPoint) / currentDashSpeed;
            //Estimated skillshot position
            Vector2 skillshotPosition = ss.GetMissilePosition((int)(arrivingTime * 1000));
            if (Vector2.DistanceSquared(skillshotPosition, intersectionPoint) <
                (ss.SpellData.Radius + Player.BoundingRadius) && !YasMath.willColide(skillShot, Player.Position.To2D(), 700f + Player.MoveSpeed, dashDir, Player.BoundingRadius + skillShot.SpellData.Radius))
                return false;
            return true;
        }

        public static void useEtoSafe(Skillshot skillShot)
        {
            if (!E.IsReady())
                return;
            float closest = float.MaxValue;
            Obj_AI_Base closestTarg = null;
            float currentDashSpeed = 700 + Player.MoveSpeed;
            foreach (Obj_AI_Base enemy in ObjectManager.Get<Obj_AI_Base>().Where(ob => ob.NetworkId != skillShot.Unit.NetworkId && enemyIsJumpable(ob) && ob.Distance(Player) < E.Range).OrderBy(ene => ene.Distance(Game.CursorPos,true)))
            {
                var pPos = Player.Position.To2D();
                Vector2 posAfterE = V2E(Player.Position, enemy.Position, 475);
                Vector2 dashDir = (posAfterE - Player.Position.To2D()).Normalized();

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
            if (closestTarg != null)
                useENormal(closestTarg);
        }

        public static void useWSmart(Skillshot skillShot)
        {
            //try douge with E if cant windWall

            if (!W.IsReady() || skillShot.SpellData.Type == SkillShotType.SkillshotCircle || skillShot.SpellData.Type == SkillShotType.SkillshotRing)
                return;
            if (skillShot.IsAboutToHit(500, Player))
            {
                var sd = SpellDatabase.GetByMissileName(skillShot.SpellData.MissileSpellName);
                if (sd == null)
                    return;

                //If enabled
                if (!YasuoSharp.EvadeSpellEnabled(sd.MenuItemName))
                    return;

                //if only dangerous
                if (YasuoSharp.Config.Item("wwDanger").GetValue<bool>() &&
                    !YasuoSharp.skillShotIsDangerous(sd.MenuItemName))
                    return;

                //Console.WriteLine("dmg: " + missle.SpellCaster.GetSpellDamage(Player, sd.SpellName));
                float spellDamage = (float)skillShot.Unit.GetSpellDamage(Player, sd.SpellName);
                int procHp = (int)((spellDamage / Player.MaxHealth) * 100);

                if (procHp < YasuoSharp.Config.Item("wwDmg").GetValue<Slider>().Value && Player.Health - spellDamage > 0)
                    return;


                Vector3 blockwhere = Player.ServerPosition + Vector3.Normalize(skillShot.MissilePosition.To3D() - Player.ServerPosition) * 10; // missle.Position; 
                SmoothMouse.addMouseEvent(blockwhere);
                W.Cast(blockwhere);
            }

        }

        public static void useWSmartOld(MissileClient missle)
        {
            if (!W.IsReady())
                return;
            try
            {
                if (missle.SpellCaster is Obj_AI_Hero && missle.IsEnemy)
                {
                    var sd = SpellDatabase.GetByMissileName(missle.SData.Name);
                    if (sd == null)
                        return;

                    //If enabled
                    if (!YasuoSharp.EvadeSpellEnabled(sd.MenuItemName))
                        return;

                    //if only dangerous
                    if (YasuoSharp.Config.Item("wwDanger").GetValue<bool>() &&
                        !YasuoSharp.skillShotIsDangerous(sd.MenuItemName))
                        return;

                    //Console.WriteLine("dmg: " + missle.SpellCaster.GetSpellDamage(Player, sd.SpellName));
                    float spellDamage = (float)missle.SpellCaster.GetSpellDamage(Player, sd.SpellName);
                    int procHp = (int)((spellDamage / Player.MaxHealth) * 100);

                    if (procHp < YasuoSharp.Config.Item("wwDmg").GetValue<Slider>().Value && Player.Health - spellDamage > 0)
                        return;

                    Obj_AI_Base enemHero = missle.SpellCaster;
                    float dmg = (float)enemHero.GetAutoAttackDamage(Player);
                    //enemHero.BaseAttackDamage + enemHero.FlatPhysicalDamageMod);
                    if (missle.SData.Name.Contains("Crit"))
                        dmg *= 2;
                    if (!missle.SData.Name.Contains("Attack") ||
                        (enemHero.CombatType == GameObjectCombatType.Ranged && dmg > Player.MaxHealth / 8))
                    {
                        if (missleWillHit(missle))
                        {
                            Vector3 blockWhere = missle.Position;
                           if (Player.Distance(missle.Position) < 420)
                            {
                                if (missle.Target.IsMe || isMissileCommingAtMe(missle))
                                {
                                    YasuoSharp.lastSpell = missle.SData.Name;
                                    SmoothMouse.addMouseEvent(blockWhere);
                                    W.Cast(blockWhere, true);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static bool missleWillHit(MissileClient missle)
        {
            if (missle.Target.IsMe || YasMath.interCir(missle.StartPosition.To2D(), missle.EndPosition.To2D(), Player.Position.To2D(), missle.SData.LineWidth + Player.BoundingRadius))
            {
                if (missle.StartPosition.Distance(Player.Position) < (missle.StartPosition.Distance(missle.EndPosition)))
                    return true;
            }
            return false;
        }


        public static bool useENormal(Obj_AI_Base target)
        {
            if (!E.IsReady() || target.Distance(Player)>470)
                return false;
            Vector2 posAfter = V2E(Player.Position, target.Position, 475);
            if (!YasuoSharp.Config.Item("djTur").GetValue<bool>())
            {
                if (isSafePoint(posAfter).IsSafe)
                {
                    SmoothMouse.addMouseEvent(target.Position);
                    E.Cast(target, false);
                }
                return true;
            }
            else
            {
                Vector2 pPos = Player.ServerPosition.To2D();
                Vector2 posAfterE = pPos + (Vector2.Normalize(target.Position.To2D() - pPos) * E.Range);
                if (!(posAfterE.To3D().UnderTurret(true)))
                {
                    Console.WriteLine("use gap?");
                    if (isSafePoint(posAfter,true).IsSafe)
                    {
                        SmoothMouse.addMouseEvent(target.Position);
                        E.Cast(target, false);
                    }
                    return true;
                }
            }
            return false;

        }

        public static bool useESmart(Obj_AI_Hero target, List<Obj_AI_Hero> ignore = null)
        {
            if (!E.IsReady())
                return false;
            float trueAARange = Player.AttackRange + target.BoundingRadius;
            float trueERange = target.BoundingRadius + E.Range;

            float dist = Player.Distance(target);
            Vector2 dashPos = new Vector2();
            if (target.IsMoving && target.Path.Count() != 0)
            {
                Vector2 tpos = target.Position.To2D();
                Vector2 path = target.Path[0].To2D() - tpos;
                path.Normalize();
                dashPos = tpos + (path * 100);
            }
            float targ_ms = (target.IsMoving && Player.Distance(dashPos) > dist) ? target.MoveSpeed : 0;
            float msDif = (Player.MoveSpeed - targ_ms) == 0 ? 0.0001f : (Player.MoveSpeed - targ_ms);
            float timeToReach = (dist - trueAARange) / msDif;
            if (dist > trueAARange && dist < E.Range)
            {
                if (timeToReach > 1.7f || timeToReach < 0.0f)
                {
                    if (useENormal(target))
                        return true;
                }
            }
            return false;
        }

        public static void gapCloseE(Vector2 pos, List<Obj_AI_Hero> ignore = null)
        {
            if (!E.IsReady())
                return;

            Vector2 pPos = Player.ServerPosition.To2D();
            Obj_AI_Base bestEnem = null;


            float distToPos = Player.Distance(pos);
            if (((distToPos < Q.Range)) &&
                goesThroughWall(pos.To3D(), Player.Position))
                return;
            Vector2 bestLoc = pPos + (Vector2.Normalize(pos - pPos) * (Player.MoveSpeed * 0.35f));
            float bestDist = pos.Distance(pPos)-50;
            try
            {
                foreach (Obj_AI_Base enemy in ObjectManager.Get<Obj_AI_Base>().Where(ob => enemyIsJumpable(ob, ignore)))
                {

                    float trueRange = E.Range + enemy.BoundingRadius;
                    float distToEnem = Player.Distance(enemy);
                    if (distToEnem < trueRange && distToEnem > 15)
                    {
                        Vector2 posAfterE = pPos + (Vector2.Normalize(enemy.Position.To2D() - pPos) * E.Range);
                        float distE = pos.Distance(posAfterE);
                        if (distE < bestDist)
                        {
                            bestLoc = posAfterE;
                            bestDist = distE;
                            bestEnem = enemy;
                           // Console.WriteLine("Gap to best enem");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (bestEnem != null)
            {
                Console.WriteLine("should use gap");
                useENormal(bestEnem);
            }

        }


        public static void useRSmart()
        {
            float timeToLand = float.MaxValue;
            List<Obj_AI_Hero> enemInAir = getKockUpEnemies(ref timeToLand);
            foreach (Obj_AI_Hero enem in enemInAir)
            {
                int aroundAir = 0;
                foreach (Obj_AI_Hero enem2 in enemInAir)
                {
                    if (Vector3.DistanceSquared(enem.ServerPosition, enem2.ServerPosition) < 400 * 400)
                        aroundAir++;

                }
                if (aroundAir >= YasuoSharp.Config.Item("useRHit").GetValue<Slider>().Value && timeToLand < 0.4f)
                    R.Cast(enem);
            }
        }



        public static bool isMissileCommingAtMe(MissileClient missle)
        {
            Vector3 step = missle.StartPosition + Vector3.Normalize(missle.StartPosition - missle.EndPosition) * 10;
            return (!(Player.Distance(step) < Player.Distance(missle.StartPosition)));
        }

        public static bool enemyIsJumpable(Obj_AI_Base enemy, List<Obj_AI_Hero> ignore = null)
        {
            if (enemy.IsValid && enemy.IsEnemy && !enemy.IsInvulnerable && !enemy.MagicImmune && !enemy.IsDead)
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

        public static float getSpellCastTime(Spell spell)
        {
            return sBook.GetSpell(spell.Slot).SData.SpellCastTime;
        }

        public static float getSpellCastTime(SpellSlot slot)
        {
            return sBook.GetSpell(slot).SData.SpellCastTime;
        }

        public static void processTargetedSpells()
        {
            if (!W.IsReady(300) && (wall == null || !E.IsReady(200) || !wall.isValid()))
                return;
            foreach (var targMis in TargetSpellDetector.ActiveTargeted)
            {
                if (targMis == null || targMis.particle == null || targMis.blockBelow <Player.HealthPercent)
                    continue;
                try
                {
                    var misDist = targMis.particle.Position.Distance(Player.Position);
                    if (misDist < 700)
                    {
                        if (W.IsReady() && misDist < 500)
                        {
                            Vector3 blockwhere = Player.ServerPosition +
                                                 Vector3.Normalize(targMis.particle.Position - Player.Position)*150;
                            SmoothMouse.addMouseEvent(blockwhere);
                            W.Cast(blockwhere,true);
                            return;
                        }
                        else if (E.IsReady() && wall != null && wall.isValid(500) &&
                                 !goesThroughWall(Player.Position, targMis.particle.Position))
                        {
                            foreach (
                                Obj_AI_Base enemy in
                                    ObjectManager.Get<Obj_AI_Base>()
                                        .Where(ob => enemyIsJumpable(ob))
                                        .OrderBy(ene => ene.Position.Distance(Game.CursorPos, true)))
                            {
                                if (goesThroughWall(Player.Position, Player.Position.Extend(enemy.Position, 475)))
                                {
                                    E.CastOnUnit(enemy);
                                    return;
                                }

                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
        }
    }
}
