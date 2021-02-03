using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace AntiTrap
{
    class Program
    {
        public static Menu Config;
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Config = new Menu("AntiTrap", " AntiTrap", true);
            Config.AddToMainMenu();

            Config.AddItem(new MenuItem("Jinx E", "Jinx E").SetValue(true));
            Config.AddItem(new MenuItem("Caitlyn W", "Caitlyn W").SetValue(true));
            Config.AddItem(new MenuItem("Teemo R", "Teemo R").SetValue(true));
            Config.AddItem(new MenuItem("Draw", "Draw").SetValue(true));
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if ((int)(Game.Time * 10) % 2 == 0)
                return;

            foreach ( var obj in  ObjectManager.Get<Obj_GeneralParticleEmitter>().Where(obj => obj.IsValid ))
            {
                
                var distance = obj.Position.Distance(ObjectManager.Player.Position);
                if (distance > 1500)
                    continue;

                var name = obj.Name.ToLower();

                if (name.Contains("yordleTrap_idle_red.troy".ToLower()) && Config.Item("Caitlyn W").GetValue<bool>())
                {
                    if (distance < 200)
                        TryDodge(obj.Position, 200);
                    if (Config.Item("Draw").GetValue<bool>())
                        Render.Circle.DrawCircle(obj.Position, 100, System.Drawing.Color.OrangeRed, 1);
                }
            }

            foreach (var obj in ObjectManager.Get<Obj_AI_Minion>().Where(obj => obj.IsValid && obj.IsEnemy))
            {
                var distance = obj.Position.Distance(ObjectManager.Player.Position);
                if (distance > 1500)
                    continue;
                if (obj.Name == "k" && Config.Item("Jinx E").GetValue<bool>())
                {
                    if (distance < 260)
                        TryDodge(obj.Position, 260);
                    if(Config.Item("Draw").GetValue<bool>())
                        Render.Circle.DrawCircle(obj.Position, 100, System.Drawing.Color.OrangeRed, 1);
                }
                if (obj.Name == "Noxious Trap")
                {
                    if (distance < 240)
                        TryDodge(obj.Position, 240);
                    if (Config.Item("Draw").GetValue<bool>())
                        Render.Circle.DrawCircle(obj.Position, 100, System.Drawing.Color.OrangeRed, 1);
                }
            }
        }

        private static void TryDodge(Vector3 position, float range)
        {
            var points = CirclePoints(15, 100 , Player.Position);
            var bestPoint = points.Where(x => x.Distance(position) > range).OrderBy(y => y.Distance(Game.CursorPos)).FirstOrDefault();
            
            if(bestPoint != null)
                Player.IssueOrder(GameObjectOrder.MoveTo, bestPoint);

        }

        public static List<Vector3> CirclePoints(float CircleLineSegmentN, float radius, Vector3 position)
        {
            List<Vector3> points = new List<Vector3>();
            for (var i = 1; i <= CircleLineSegmentN; i++)
            {
                var angle = i * 2 * Math.PI / CircleLineSegmentN;
                var point = new Vector3(position.X + radius * (float)Math.Cos(angle), position.Y + radius * (float)Math.Sin(angle), position.Z);
                points.Add(point);
            }
            return points;
        }
    }
}
