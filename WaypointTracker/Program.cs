#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing;

#endregion

namespace WaypointTracker
{
    internal class Program
    {
        public static Dictionary<Obj_AI_Hero, Render.Circle> Circles = new Dictionary<Obj_AI_Hero, Render.Circle>();
        public static Dictionary<Obj_AI_Hero, Render.Text> Text = new Dictionary<Obj_AI_Hero, Render.Text>();
        public static Menu Menu;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("Waypoint Tracker", "WaypointTracker", true);
            Menu.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));
            Menu.AddItem(new MenuItem("Distance", "Distance").SetValue(new Slider(500, 50, 1000)));
            Menu.AddItem(new MenuItem("Time", "Draw Arrival Time").SetValue(true));
            Menu.AddItem(new MenuItem("Name", "Draw Champ Name").SetValue(false));

            // var sub = Menu.AddSubMenu(new MenuWrapper.SubMenu(Menu, "Enemies"));
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy))
            {
                Menu.AddItem(new MenuItem(hero.ChampionName, "Draw " + hero.ChampionName).SetValue(true));
                Circles.Add(hero, new Render.Circle(new Vector3(0, 0, 0), 50, Color.Color.Red, 2));
                Circles[hero].Visible = false;
                Circles[hero].Add();
                Text.Add(hero, new Render.Text(new Vector2(0, 0), "", 20, SharpDX.Color.Red));
                Text[hero].Visible = false;
                Text[hero].Add();
            }

            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            foreach (var data in Circles)
            {
                var hero = data.Key;
                var circle = data.Value;
                var text = Text[hero];

                var wp = hero.GetWaypoints();

                if (!Menu.Item(hero.ChampionName).GetValue<bool>() || hero.IsDead || !hero.IsVisible || wp.Count <= 1 ||
                    ObjectManager.Player.Distance(wp.Last()) > Menu.Item("Distance").GetValue<Slider>().Value)
                {
                    text.Visible = false;
                    circle.Visible = false;
                    return;
                }

                text.text = "Test";
                // text.P
                circle.Position = wp.Last().To3D();
                circle.Visible = true;
            }
        }
    }
}