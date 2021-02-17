using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using LeagueSharp.Common.Data;
using ItemData = LeagueSharp.Common.Data.ItemData;
using FioraProject.Evade;

namespace FioraProject
{
    class HINH1
    {
        #region HINH1
        private enum DrawType
        {
            Circle = 1,
            HINH1 = 2
        }
        private static int drawtick = 0;
        private static int drawstate = 0;
        private static void DrawDraw(Vector3 center, float radius, Color color, DrawType DrawedType, int width = 5)
        {
            switch (DrawedType)
            {
                case DrawType.Circle:
                    DrawCircle(center, radius, color, width);
                    break;
                case DrawType.HINH1:
                    DrawHinh1(center, radius, color, width);
                    break;
            }
        }
        private static void DrawHinh1(Vector3 center, float radius, Color color, int width = 5)
        {
            Render.Circle.DrawCircle(center, radius, color, width, false);
            return;
            var pos1y = center;
            pos1y.X = pos1y.X + radius;
            var pos1 = pos1y.To2D().RotateAround(center.To2D(), drawstate.AngleToRadian());
            var pos1a = center.Extend(pos1.To3D(), radius * 5 / 8).To2D().RotateAround(center.To2D(), (18).AngleToRadian());
            var pos2 = pos1.RotateAround(center.To2D(), (36).AngleToRadian());
            var pos3 = pos1.RotateAround(center.To2D(), (72).AngleToRadian());
            var pos4 = pos1.RotateAround(center.To2D(), (108).AngleToRadian());
            var pos5 = pos1.RotateAround(center.To2D(), (144).AngleToRadian());
            var pos6 = pos1.RotateAround(center.To2D(), (180).AngleToRadian());
            var pos7 = pos1.RotateAround(center.To2D(), (216).AngleToRadian());
            var pos8 = pos1.RotateAround(center.To2D(), (252).AngleToRadian());
            var pos9 = pos1.RotateAround(center.To2D(), (288).AngleToRadian());
            var pos10 = pos1.RotateAround(center.To2D(), (324).AngleToRadian());
            var pos2a = pos1a.RotateAround(center.To2D(), (36).AngleToRadian());
            var pos3a = pos1a.RotateAround(center.To2D(), (72).AngleToRadian());
            var pos4a = pos1a.RotateAround(center.To2D(), (108).AngleToRadian());
            var pos5a = pos1a.RotateAround(center.To2D(), (144).AngleToRadian());
            var pos6a = pos1a.RotateAround(center.To2D(), (180).AngleToRadian());
            var pos7a = pos1a.RotateAround(center.To2D(), (216).AngleToRadian());
            var pos8a = pos1a.RotateAround(center.To2D(), (252).AngleToRadian());
            var pos9a = pos1a.RotateAround(center.To2D(), (288).AngleToRadian());
            var pos10a = pos1a.RotateAround(center.To2D(), (324).AngleToRadian());
            Drawing.DrawLine(Drawing.WorldToScreen(pos1.To3D()), Drawing.WorldToScreen(pos1a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos2.To3D()), Drawing.WorldToScreen(pos1a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos2.To3D()), Drawing.WorldToScreen(pos2a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos3.To3D()), Drawing.WorldToScreen(pos2a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos3.To3D()), Drawing.WorldToScreen(pos3a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos4.To3D()), Drawing.WorldToScreen(pos3a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos4.To3D()), Drawing.WorldToScreen(pos4a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos5.To3D()), Drawing.WorldToScreen(pos4a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos5.To3D()), Drawing.WorldToScreen(pos5a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos6.To3D()), Drawing.WorldToScreen(pos5a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos6.To3D()), Drawing.WorldToScreen(pos6a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos7.To3D()), Drawing.WorldToScreen(pos6a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos7.To3D()), Drawing.WorldToScreen(pos7a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos8.To3D()), Drawing.WorldToScreen(pos7a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos8.To3D()), Drawing.WorldToScreen(pos8a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos9.To3D()), Drawing.WorldToScreen(pos8a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos9.To3D()), Drawing.WorldToScreen(pos9a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos10.To3D()), Drawing.WorldToScreen(pos9a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos10.To3D()), Drawing.WorldToScreen(pos10a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos1.To3D()), Drawing.WorldToScreen(pos10a.To3D()), width, color);

            Drawing.DrawLine(Drawing.WorldToScreen(pos1.To3D()), Drawing.WorldToScreen(pos2.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos3.To3D()), Drawing.WorldToScreen(pos2.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos3.To3D()), Drawing.WorldToScreen(pos4.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos5.To3D()), Drawing.WorldToScreen(pos4.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos5.To3D()), Drawing.WorldToScreen(pos6.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos7.To3D()), Drawing.WorldToScreen(pos6.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos7.To3D()), Drawing.WorldToScreen(pos8.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos9.To3D()), Drawing.WorldToScreen(pos8.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos9.To3D()), Drawing.WorldToScreen(pos10.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos1.To3D()), Drawing.WorldToScreen(pos10.To3D()), width, color);

            Drawing.DrawLine(Drawing.WorldToScreen(pos1a.To3D()), Drawing.WorldToScreen(pos2a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos3a.To3D()), Drawing.WorldToScreen(pos2a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos3a.To3D()), Drawing.WorldToScreen(pos4a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos5a.To3D()), Drawing.WorldToScreen(pos4a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos5a.To3D()), Drawing.WorldToScreen(pos6a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos7a.To3D()), Drawing.WorldToScreen(pos6a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos7a.To3D()), Drawing.WorldToScreen(pos8a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos9a.To3D()), Drawing.WorldToScreen(pos8a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos9a.To3D()), Drawing.WorldToScreen(pos10a.To3D()), width, color);
            Drawing.DrawLine(Drawing.WorldToScreen(pos1a.To3D()), Drawing.WorldToScreen(pos10a.To3D()), width, color);

            DrawCircle(center, radius * 2 / 8, color, width, 10);

            if (Utils.GameTimeTickCount >= drawtick + 10)
            {
                drawtick = Utils.GameTimeTickCount;
                drawstate += 2;
            }


        }

        private static void DrawHinh2(Vector3 center, float radius, Color color, int width = 5)
        {
            var n = 100 - (drawstate % 102);
            DrawCircle(center, radius * n / 100, Color.Yellow, width * 3, 10);
            DrawCircle(center, radius * (n + 20 > 100 ? n - 80 : n + 20) / 100, Color.LightGreen);
            DrawCircle(center, radius * (n + 40 > 100 ? n - 60 : n + 40) / 100, Color.Orange);
            DrawCircle(center, radius * (n + 60 > 100 ? n - 40 : n + 60) / 100, Color.LightPink);
            DrawCircle(center, radius * (n + 80 > 100 ? n - 20 : n + 80) / 100, Color.PaleVioletRed);

            if (Utils.GameTimeTickCount >= drawtick + 10)
            {
                drawtick = Utils.GameTimeTickCount;
                drawstate += 2;
            }
        }

        public static void DrawCircle(Vector3 center,
            float radius,
            Color color,
            int thickness = 5,
            int quality = 60)
        {
            Render.Circle.DrawCircle(center, radius, color, thickness, false);

            //var pointList = new List<Vector3>();
            //for (var i = 0; i < quality; i++)
            //{
            //    var angle = i * Math.PI * 2 / quality;
            //    pointList.Add(
            //        new Vector3(
            //            center.X + radius * (float)Math.Cos(angle), center.Y + radius * (float)Math.Sin(angle),
            //            center.Z));
            //}

            //for (var i = 0; i < pointList.Count; i++)
            //{
            //    var a = pointList[i];
            //    var b = pointList[i == pointList.Count - 1 ? 0 : i + 1];

            //    var aonScreen = Drawing.WorldToScreen(a);
            //    var bonScreen = Drawing.WorldToScreen(b);

            //    Drawing.DrawLine(aonScreen.X, aonScreen.Y, bonScreen.X, bonScreen.Y, thickness, color);
            //}
        }

        #endregion HINH1
    }
}
