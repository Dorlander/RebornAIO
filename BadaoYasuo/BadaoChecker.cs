using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace BadaoKingdom
{
    public static class BadaoChecker
    {
        public static bool BadaoIsValidTarget(this AttackableUnit unit,
            float range = float.MaxValue,
            bool checkTeam = true,
            Vector3 from = new Vector3())
        {
            return unit.IsValidTarget(range, checkTeam, from) && !unit.IsZombie;
        }
        public static int BadaoGetSmiteDamage()
        {
            return new int[] { 390, 410, 430, 450, 480, 510, 540, 570, 600, 640, 680, 720, 760, 800, 850, 900, 950, 1000 }
                [ObjectManager.Player.Level - 1];
        }
        public static int BadaoGetSmiteDamage(Obj_AI_Hero target)
        {
            return BadaoHasSmiteBlue ? 20 + 8 * ObjectManager.Player.Level :
                   0;
        }
        public static bool BadaoHasSmiteRed
        {
            get
            {
                return (new string[] { "s5_summonersmiteduel" })
                    .Contains(ObjectManager.Player.GetSpell(BadaoMainVariables.Smite).Name);
            }
        }
        public static bool BadaoHasSmiteBlue
        {
            get
            {
                return (new string[] { "s5_summonersmiteplayerganker" })
                    .Contains(ObjectManager.Player.GetSpell(BadaoMainVariables.Smite).Name);
            }
        }
        public static bool BadaoSmiteReady()
        {
            return BadaoMainVariables.Smite.IsReady();
        }
        public static bool BadaoUseTiamat()
        {
            return ItemData.Tiamat.GetItem().Cast()
                || ItemData.Ravenous_Hydra.GetItem().Cast()
                || ItemData.Titanic_Hydra.GetItem().Cast();
        }
        public static bool BadaoHasTiamat()
        {
            return ItemData.Tiamat.GetItem().IsReady()
                || ItemData.Ravenous_Hydra.GetItem().IsReady()
                || ItemData.Titanic_Hydra.GetItem().IsReady();
        }
        public static Vector2 BadaoRotateAround(this Vector2 pointToRotate, Vector2 centerPoint, float angleInRadians)
        {
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Vector2
            {
                X =
                    (float)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (float)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }
        public static float AngleToRadian(this int Angle)
        {
            return Angle * (float)Math.PI / 180f;
        }
        public static double BadaoAngleBetween(Vector2 a, Vector2 center, Vector2 c)
        {
            float a1 = c.Distance(center);
            float b1 = a.Distance(c);
            float c1 = center.Distance(a);
            if (a1 == 0 || c1 == 0) { return 0; }
            else
            {
                return Math.Acos((a1 * a1 + c1 * c1 - b1 * b1) / (2 * a1 * c1)) * (180 / Math.PI);
            }
        }
        public static bool BadaoInTheCone(this Vector2 pos, Vector2 centerconePolar, Vector2 centerconeEnd, double coneAngle)
        {
            return BadaoAngleBetween(pos, centerconePolar, centerconeEnd) < coneAngle / 2
                && pos.Distance(centerconePolar) < centerconePolar.Distance(centerconeEnd);
        }
    }
}
