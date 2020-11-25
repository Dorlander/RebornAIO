using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace xSaliceResurrected.Utilities
{
    public static class Util 
    {
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        public static bool UnderAllyTurret()
        {
            return
                ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret != null && (turret.IsAlly && !turret.IsDead && turret.Distance(Player) < 800));
        }

        public static bool IsWall(Vector2 pos)
        {
            return (NavMesh.GetCollisionFlags(pos.To3D()).HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(pos.To3D()).HasFlag(CollisionFlags.Building));
        }

        public static bool IsPassWall(Vector3 start, Vector3 end)
        {
            double count = Vector3.Distance(start, end);
            for (uint i = 0; i <= count; i += 25)
            {
                Vector2 pos = start.To2D().Extend(Player.ServerPosition.To2D(), -i);
                if (IsWall(pos))
                    return true;
            }
            return false;
        }

        public static bool IsStunned(Obj_AI_Base target)
        {
            if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) ||
                target.HasBuffOfType(BuffType.Suppression) || target.HasBuffOfType(BuffType.Taunt))
                return true;

            return false;
        }

        public static PredictionOutput GetP(Vector3 pos, Spell spell, Obj_AI_Base target, float delay, bool aoe)
        {
            return Prediction.GetPrediction(new PredictionInput
            {
                Unit = target,
                Delay = spell.Delay + delay,
                Radius = spell.Width,
                Speed = spell.Speed,
                From = pos,
                Range = spell.Range,
                Collision = spell.Collision,
                Type = spell.Type,
                RangeCheckFrom = Player.ServerPosition,
                Aoe = aoe,
            });
        }

        public static PredictionOutput GetP(Vector3 pos, Spell spell, Obj_AI_Base target, bool aoe)
        {
            return Prediction.GetPrediction(new PredictionInput
            {
                Unit = target,
                Delay = spell.Delay,
                Radius = spell.Width,
                Speed = spell.Speed,
                From = pos,
                Range = spell.Range,
                Collision = spell.Collision,
                Type = spell.Type,
                RangeCheckFrom = Player.ServerPosition,
                Aoe = aoe,
            });
        }
        public static PredictionOutput GetPCircle(Vector3 pos, Spell spell, Obj_AI_Base target, bool aoe)
        {
            return Prediction.GetPrediction(new PredictionInput
            {
                Unit = target,
                Delay = spell.Delay,
                Radius = 1,
                Speed = float.MaxValue,
                From = pos,
                Range = float.MaxValue,
                Collision = spell.Collision,
                Type = spell.Type,
                RangeCheckFrom = pos,
                Aoe = aoe,
            });
        }

        public static Object[] VectorPointProjectionOnLineSegment(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float cx = v3.X;
            float cy = v3.Y;
            float ax = v1.X;
            float ay = v1.Y;
            float bx = v2.X;
            float by = v2.Y;
            float rL = ((cx - ax) * (bx - ax) + (cy - ay) * (by - ay)) /
                       ((float)Math.Pow(bx - ax, 2) + (float)Math.Pow(by - ay, 2));
            var pointLine = new Vector2(ax + rL * (bx - ax), ay + rL * (by - ay));
            float rS;
            if (rL < 0)
            {
                rS = 0;
            }
            else if (rL > 1)
            {
                rS = 1;
            }
            else
            {
                rS = rL;
            }
            bool isOnSegment = rS.CompareTo(rL) == 0;
            Vector2 pointSegment = isOnSegment ? pointLine : new Vector2(ax + rS * (bx - ax), ay + rS * (@by - ay));
            return new object[] { pointSegment, pointLine, isOnSegment };
        }

        public static Obj_AI_Hero GetTargetFocus(float range)
        {
            var focusSelected = Champion.menu.Item("selected", true).GetValue<bool>();

            if (TargetSelector.GetSelectedTarget() != null)
                if (focusSelected && TargetSelector.GetSelectedTarget().Distance(Player.ServerPosition) < range + 100 && TargetSelector.GetSelectedTarget().Type == GameObjectType.obj_AI_Hero)
                {
                    //Game.PrintChat("Focusing: " + TargetSelector.GetSelectedTarget().Name);
                    return TargetSelector.GetSelectedTarget();
                }
            return null;
        }
    }
}
