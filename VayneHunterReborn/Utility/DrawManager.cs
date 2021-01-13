using System;
using System.Linq;
using ClipperLib;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using VayneHunter_Reborn.Skills.Tumble;
using VayneHunter_Reborn.Skills.Tumble.VHRQ;
using VayneHunter_Reborn.Utility.Helpers;
using VayneHunter_Reborn.Utility.MenuUtility;
using Color = System.Drawing.Color;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace VayneHunter_Reborn.Utility
{
    class DrawManager
    {
        public static void OnLoad()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            var drakeWallQPos = new Vector2(11514, 4462);
            var midWallQPos = new Vector2(6962, 8952);
            if (MenuExtensions.GetItemValue<bool>("dz191.vhr.draw.spots"))
            {
                if (ObjectManager.Player.Distance(drakeWallQPos) <= 1500f && PlayerHelper.IsSummonersRift())
                {
                    Render.Circle.DrawCircle(new Vector2(12050, 4827).To3D(), 65f, Color.AliceBlue);
                }
                if (ObjectManager.Player.Distance(midWallQPos) <= 1500f && PlayerHelper.IsSummonersRift())
                {
                    Render.Circle.DrawCircle(new Vector2(6962, 8952).To3D(), 65f, Color.AliceBlue);
                }
            }

            if (MenuExtensions.GetItemValue<bool>("dz191.vhr.draw.range"))
            {
                DrawEnemyZone();
            }

            if (MenuExtensions.GetItemValue<bool>("dz191.vhr.draw.qpos"))
            {
                var QPosition = VHRQLogic.GetVHRQPosition();
                Render.Circle.DrawCircle(QPosition, 35, Color.Yellow);
            }  
        }

        public static void DrawEnemyZone()
        {
            var currentPath = TumblePositioning.GetEnemyPoints().Select(v2 => new IntPoint(v2.X, v2.Y)).ToList();
            var currentPoly = Helpers.Geometry.ToPolygon(currentPath);
            currentPoly.Draw(Color.White);
        }
    }
}
