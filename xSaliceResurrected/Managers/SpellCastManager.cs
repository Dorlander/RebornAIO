using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSaliceResurrected.Utilities;
using Color = System.Drawing.Color;
using Geometry = LeagueSharp.Common.Geometry;

namespace xSaliceResurrected.Managers
{
    class SpellCastManager
    {
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        public static void CastBasicSkillShot(Spell spell, float range, TargetSelector.DamageType type, HitChance hitChance, bool towerCheck = false)
        {
            var target = TargetSelector.GetTarget(range, type);

            if (target == null || !spell.IsReady())
                return;

            if (towerCheck && target.UnderTurret(true))
                return;

            spell.UpdateSourcePosition();

            if (spell.Type == SkillshotType.SkillshotCircle)
            {
                var pred = Prediction.GetPrediction(target, spell.Delay);

                if (pred.Hitchance >= hitChance)
                    spell.Cast(pred.CastPosition);
            }
            else
            {
                spell.CastIfHitchanceEquals(target, hitChance); 
            }
           
        }

        public static void CastBasicFarm(Spell spell)
        {
            if (!spell.IsReady())
                return;
            var minion = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spell.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (minion.Count == 0)
                return;

            if (spell.Type == SkillshotType.SkillshotCircle)
            {
                spell.UpdateSourcePosition();

                var predPosition = spell.GetCircularFarmLocation(minion);

                if (predPosition.MinionsHit >= 2)
                {
                    spell.Cast(predPosition.Position);
                }
            }
            else if (spell.Type == SkillshotType.SkillshotLine || spell.Type == SkillshotType.SkillshotCone)
            {
                spell.UpdateSourcePosition();

                var predPosition = spell.GetLineFarmLocation(minion);

                if (predPosition.MinionsHit >= 2)
                    spell.Cast(predPosition.Position);
            }
        }

        private static int _lastCast;
        public static void CastSingleLine(Spell spell, Spell spell2, bool wallCheck, float extraPrerange = 1)
        {
            if (!spell.IsReady() || Utils.TickCount - _lastCast < 0)
                return;

            //-------------------------------Single---------------------------
            var target = TargetSelector.GetTarget(spell.Range + spell2.Range, TargetSelector.DamageType.Magical);

            if (target == null)
                return;

            var vector1 = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * (spell.Range * extraPrerange);

            spell2.UpdateSourcePosition(vector1, vector1);

            var pred = Prediction.GetPrediction(target, spell.Delay);
            Geometry.Polygon.Rectangle rec1 = new Geometry.Polygon.Rectangle(vector1, vector1.Extend(pred.CastPosition, spell2.Range), spell.Width);

            if (Player.ServerPosition.Distance(target.ServerPosition) < spell.Range)
            {
                var vector2 = pred.CastPosition.Extend(target.ServerPosition, spell2.Range*.3f);
                Geometry.Polygon.Rectangle rec2 = new Geometry.Polygon.Rectangle(vector2, vector2.Extend(pred.CastPosition, spell2.Range), spell.Width);

                if ((!rec2.Points.Exists(Util.IsWall) || !wallCheck) && pred.Hitchance >= HitChance.Medium && target.IsMoving)
                {
                    spell2.UpdateSourcePosition(vector2, vector2);
                    CastLineSpell(vector2, vector2.Extend(pred.CastPosition, spell2.Range));
                    _lastCast = Utils.TickCount + 500;
                }

            }
            else if (!rec1.Points.Exists(Util.IsWall) || !wallCheck)
            {
                //wall check
                if (pred.Hitchance >= HitChance.High)
                {
                    CastLineSpell(vector1, pred.CastPosition);
                }
            }
        }

        public static void CastBestLine(bool forceUlt, Spell spell, Spell spell2, int midPointRange, Menu menu, float extraPrerange = 1, bool wallCheck = true)
        {
            if (!spell.IsReady())
                return;

            int maxHit = 0;
            Vector3 start = Vector3.Zero;
            Vector3 end = Vector3.Zero;

            //loop one
            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(spell.Range)))
            {
               //loop 2
                var target1 = target;
                var target2 = target;
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(spell.Range + spell2.Range) && x.NetworkId != target1.NetworkId
                    && x.Distance(target1.Position) < spell2.Range - 100).OrderByDescending(x => x.Distance(target2.Position)))
                {
                    int hit = 2;

                    var targetPred = Prediction.GetPrediction(target, spell.Delay);
                    var enemyPred = Prediction.GetPrediction(enemy, spell.Delay);

                    var midpoint = (enemyPred.CastPosition + targetPred.CastPosition) / 2;

                    var startpos = midpoint + Vector3.Normalize(enemyPred.CastPosition - targetPred.CastPosition) * midPointRange;
                    var endPos = midpoint - Vector3.Normalize(enemyPred.CastPosition - targetPred.CastPosition) * midPointRange;

                    Geometry.Polygon.Rectangle rec1 = new Geometry.Polygon.Rectangle(startpos, endPos, spell.Width);

                    if (!rec1.Points.Exists(Util.IsWall) && Player.CountEnemiesInRange(spell.Range + spell2.Range) > 2)
                    {
                        //loop 3
                        var target3 = target;
                        var enemy1 = enemy;
                        foreach (var enemy2 in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(spell.Range + spell2.Range) && x.NetworkId != target3.NetworkId && x.NetworkId != enemy1.NetworkId && x.Distance(target3.Position) < 1000))
                        {
                            var enemy2Pred = Prediction.GetPrediction(enemy2, spell.Delay);
                            Object[] obj = Util.VectorPointProjectionOnLineSegment(startpos.To2D(), endPos.To2D(), enemy2Pred.CastPosition.To2D());
                            var isOnseg = (bool)obj[2];
                            var pointLine = (Vector2)obj[1];

                            if (pointLine.Distance(enemy2Pred.CastPosition.To2D()) < spell.Width && isOnseg)
                            {
                                hit++;
                            }
                        }
                    }

                    if (hit > maxHit && hit > 1 && !rec1.Points.Exists(Util.IsWall))
                    {
                        maxHit = hit;
                        start = startpos;
                        end = endPos;
                    }
                }
            }

            if (start != Vector3.Zero && end != Vector3.Zero && spell.IsReady())
            {
                spell2.UpdateSourcePosition(start, start);
                if (forceUlt)
                    CastLineSpell(start, end);
                if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active && maxHit >= menu.Item("Line_If_Enemy_Count_Combo", true).GetValue<Slider>().Value)
                    CastLineSpell(start, end);
                if (maxHit >= menu.Item("Line_If_Enemy_Count", true).GetValue<Slider>().Value)
                    CastLineSpell(start, end);
            }            

            //check if only one target
            if (forceUlt)
            {
                CastSingleLine(spell, spell2, wallCheck, extraPrerange);
            }
        }

        public static void DrawBestLine(Spell spell, Spell spell2, int midPointRange, float extraPrerange = 1, bool wallCheck = true)
        {
            //---------------------------------MEC----------------------------
            int maxHit = 0;
            Vector3 start = Vector3.Zero;
            Vector3 end = Vector3.Zero;

            //loop one
            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(spell.Range)))
            {
                //loop 2
                var target1 = target;
                var target2 = target;
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(spell.Range + spell2.Range) && x.NetworkId != target1.NetworkId
                    && x.Distance(target1.Position) < spell2.Range - 100).OrderByDescending(x => x.Distance(target2.Position)))
                {
                    int hit = 2;

                    var targetPred = Prediction.GetPrediction(target, spell.Delay);
                    var enemyPred = Prediction.GetPrediction(enemy, spell.Delay);

                    var midpoint = (enemyPred.CastPosition + targetPred.CastPosition) / 2;

                    var startpos = midpoint + Vector3.Normalize(enemyPred.CastPosition - targetPred.CastPosition) * midPointRange;
                    var endPos = midpoint - Vector3.Normalize(enemyPred.CastPosition - targetPred.CastPosition) * midPointRange;

                    Geometry.Polygon.Rectangle rec1 = new Geometry.Polygon.Rectangle(startpos, endPos, spell.Width);

                    if (!rec1.Points.Exists(Util.IsWall) && Player.CountEnemiesInRange(spell.Range + spell2.Range) > 2)
                    {
                        //loop 3
                        var target3 = target;
                        var enemy1 = enemy;
                        foreach (var enemy2 in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(spell.Range + spell2.Range) && x.NetworkId != target3.NetworkId && x.NetworkId != enemy1.NetworkId && x.Distance(target3.Position) < spell2.Range))
                        {
                            var enemy2Pred = Prediction.GetPrediction(enemy2, spell.Delay);
                            Object[] obj = Util.VectorPointProjectionOnLineSegment(startpos.To2D(), endPos.To2D(), enemy2Pred.CastPosition.To2D());
                            var isOnseg = (bool)obj[2];
                            var pointLine = (Vector2)obj[1];

                            if (pointLine.Distance(enemy2Pred.CastPosition.To2D()) < spell.Width && isOnseg)
                            {
                                hit++;
                            }
                        }
                    }

                    if (hit > maxHit && hit > 1 && !rec1.Points.Exists(Util.IsWall))
                    {
                        maxHit = hit;
                        start = startpos;
                        end = endPos;
                    }
                }
            }

            if (maxHit >= 2)
            {
                Vector2 wts = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(wts[0], wts[1], Color.Wheat, "Hit: " + maxHit);

                Geometry.Polygon.Rectangle rec1 = new Geometry.Polygon.Rectangle(start, end, spell.Width);
                rec1.Draw(Color.Blue, 4);
            }
            else { 

                //-------------------------------Single---------------------------
                var target = TargetSelector.GetTarget(spell.Range + spell2.Range, TargetSelector.DamageType.Magical);

                if (target == null)
                    return;

                var vector1 = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * (spell.Range * extraPrerange);

                var pred = Prediction.GetPrediction(target, spell.Delay);
                Geometry.Polygon.Rectangle rec1 = new Geometry.Polygon.Rectangle(vector1, vector1.Extend(pred.CastPosition, spell2.Range), spell.Width);

                if (Player.ServerPosition.Distance(target.ServerPosition) < spell.Range)
                {
                    vector1 = pred.CastPosition.Extend(target.ServerPosition, spell2.Range * .3f);
                    Geometry.Polygon.Rectangle rec2 = new Geometry.Polygon.Rectangle(vector1, vector1.Extend(pred.CastPosition, spell2.Range), spell.Width);

                    if ((!rec2.Points.Exists(Util.IsWall) || !wallCheck) && pred.Hitchance >= HitChance.Medium && target.IsMoving)
                    {
                        Vector2 wts = Drawing.WorldToScreen(Player.Position);
                        Drawing.DrawText(wts[0], wts[1], Color.Wheat, "Hit: " + 1);

                        rec2.Draw(Color.Blue, 4);
                    }

                }
                else if (!rec1.Points.Exists(Util.IsWall) || !wallCheck)
                   {
                    //wall check
                    if (pred.Hitchance >= HitChance.High)
                    {
                        Vector2 wts = Drawing.WorldToScreen(Player.Position);
                        Drawing.DrawText(wts[0], wts[1], Color.Wheat, "Hit: " + 1);

                        rec1.Draw(Color.Blue, 4);
                    }
                }
            }
        }

        public static void CastLineSpell(Vector3 start, Vector3 end)
        {
            if (!SpellManager.P.IsReady())
                return;

            SpellManager.P.Cast(start, end);
        }
    }
}
