using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D9;
using TeamStats.Properties;
using Color = System.Drawing.Color;
namespace TeamStats
{
    class Program
    {
        public static Menu Config;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Teams teams;
        public static int range = 2500;
        public static Timer timer = new Timer(300);
        private static Render.Sprite frame;
        private static bool refresh = true;
        public static int myTeamHpX = (int)(Drawing.Width*0.68);
        public static int myTeamHpY = (int)(Drawing.Height * 0.97);
        public static int enemyTeamHpX = (int)(Drawing.Width * 0.68);
        public static int enemyTeamHpY = (int)(Drawing.Height * 0.97) - 40;
        public static int myTeamDmgX = (int)(Drawing.Width * 0.68);
        public static int myTeamDmgY = (int)(Drawing.Height * 0.97) - 20;
        public static int enemyTeamDmgX = (int)(Drawing.Width * 0.68);
        public static int enemyTeamDmgY = (int)(Drawing.Height * 0.97) - 60;
        public static Render.Text gPower;
        public static Render.Text ePower;
        public static Render.Text aDmg;
        public static Render.Text eDmg;
        public static Render.Text aHealt;
        public static Render.Text eHealt;
        public static Render.Text eNum;
        public static Render.Text aNum;
        public static Render.Text versus;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Config = new Menu("TeamStats", "TeamStats", true);
            Config.AddItem(new MenuItem("X-pos", "X offset").SetValue(new Slider(0, -1500, 400)));
            Config.AddItem(new MenuItem("Y-pos", "Y offset").SetValue(new Slider(0, 200, -1080)));
            Config.AddItem(new MenuItem("Range", "Range").SetValue(new Slider(2200, 0, 20000)));
            Config.AddItem(new MenuItem("Default", "Default").SetValue(false));
            Config.AddItem(new MenuItem("Chart", "Chart").SetValue(true));
            Config.AddItem(new MenuItem("Small", "Small chart").SetValue(false));
            Config.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));
            Config.AddItem(new MenuItem("draw", "Draw range")).SetValue(new Circle(false, Color.LightBlue));
            Config.AddToMainMenu();
            //frame = loadFrame();
            gPower = loadText("", new ColorBGRA(Color.FromArgb(255, 34, 139, 34).ToArgb()));
            ePower = loadText("", new ColorBGRA(Color.FromArgb(255, 178, 34, 34).ToArgb()));
            aDmg = loadText("", new ColorBGRA(Color.White.ToArgb()));
            eDmg = loadText("", new ColorBGRA(Color.White.ToArgb()));
            aHealt = loadText("", new ColorBGRA(Color.White.ToArgb()));
            eHealt = loadText("", new ColorBGRA(Color.White.ToArgb()));
            aNum = loadText("", new ColorBGRA(Color.FromArgb(255, 34, 139, 34).ToArgb()));
            eNum = loadText("", new ColorBGRA(Color.FromArgb(255, 178, 34, 34).ToArgb()));
            versus = loadText("", new ColorBGRA(Color.White.ToArgb()));
            timer.Elapsed += OnTimerTick;
            timer.Enabled = true;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- TeamStats</font>");
        }

        private static void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            if (refresh == false)
            {
                refresh = true;
            }
            
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Config.Item("Small").ValueChanged += resetSmallText;
            if (Config.Item("Default").GetValue<bool>())
            {
                Config.Item("Y-pos").SetValue(new Slider(0, 200, -1080));
                Config.Item("X-pos").SetValue(new Slider(0, -1500, 400));
                Config.Item("Range").SetValue(new Slider(2200, 0, 20000));
                Config.Item("Default").SetValue(false);
            }
            range = Config.Item("Range").GetValue<Slider>().Value;
            try
            {
                if (Config.Item("Enabled").GetValue<bool>() && refresh && player.CountEnemiesInRange(range) > 0 &&
                    countAllies(range) > 0)
                {
                    teams = new Teams();
                    refresh = false;
                }
            }
            catch (Exception d)
            {
                Console.Write(d.ToString());
            }
        }

        private static void resetSmallText(object sender, OnValueChangeEventArgs e)
        {
            gPower.text = "";
            ePower.text = "";
            aDmg.text = "";
            eDmg.text = "";
            aHealt.text = "";
            eHealt.text = "";
            versus.text = "";
            eNum.text = "";
            aNum.text = "";
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("Enabled").GetValue<bool>() && player.CountEnemiesInRange(range) > 0 &&
                countAllies(range) > 0 && teams != null)
            {
                var OffsetX = Config.Item("X-pos").GetValue<Slider>().Value;
                var OffsetY = Config.Item("Y-pos").GetValue<Slider>().Value;
                int myteamhpBar = teams.myTeamHP;
                int enemyteamhpBar = teams.enemyTeamHP;
                int myteamdmgBar = teams.myTeamDmg;
                int enemyteamdmgBar = teams.enemyTeamDmg;
                int highest1 = Math.Max(myteamhpBar, enemyteamhpBar);
                int highest2 = Math.Max(myteamdmgBar, enemyteamdmgBar);
                int highest = Math.Max(highest1, highest2);
                float scale = 300f/highest;
                if (Config.Item("Small").GetValue<bool>())
                {
                    scale = 200f/highest;
                }
                var green = Color.FromArgb(255, 34, 139, 34);
                var red = Color.FromArgb(255, 178, 34, 34);
                var white = Color.FromArgb(255, 255, 255, 255);
                if (Config.Item("Chart").GetValue<bool>())
                {
  
                    if (!Config.Item("Small").GetValue<bool>())
                    {
                       //Background
                       Drawing.DrawLine(myTeamHpX + OffsetX, myTeamHpY + 30 + OffsetY, enemyTeamDmgX + OffsetX,
                            enemyTeamDmgY + OffsetY, 300, Color.FromArgb(200, 180, 180, 180));
                       //Background BORDERS
                       Drawing.DrawLine(myTeamHpX + OffsetX-1, myTeamHpY + 30 + OffsetY, enemyTeamDmgX + OffsetX-1,
                            enemyTeamDmgY + OffsetY, 1, white);
                       Drawing.DrawLine(myTeamHpX + OffsetX + 300, myTeamHpY + 30 + OffsetY, enemyTeamDmgX + OffsetX + 300,
                       enemyTeamDmgY + OffsetY, 1, white);
                       Drawing.DrawLine(myTeamHpX + OffsetX + 150, myTeamHpY + 30 + OffsetY, myTeamHpX + OffsetX + 150,
                        myTeamHpY + 30 + OffsetY, 300, white);
                       Drawing.DrawLine(myTeamHpX + OffsetX + 150, myTeamHpY + 30 + OffsetY-91, myTeamHpX + OffsetX + 150,
                        myTeamHpY + 30 + OffsetY-91, 302, white);
                       //Allies Health text
                       Drawing.DrawText(myTeamHpX + OffsetX, myTeamHpY + OffsetY - 2, white,
                            "Allies Health(" + myteamhpBar + ")");
                        Drawing.DrawLine(myTeamHpX + OffsetX, myTeamHpY + OffsetY, myTeamHpX + OffsetX,
                            myTeamHpY + OffsetY + 14, (int)(myteamhpBar * scale * -1), green);
                        //Enemies Health text
                        Drawing.DrawText(enemyTeamHpX + OffsetX, enemyTeamHpY + OffsetY - 2, white,
                            "Enemies Health(" + enemyteamhpBar + ")");
                        Drawing.DrawLine(enemyTeamHpX + OffsetX, enemyTeamHpY + OffsetY, enemyTeamHpX + OffsetX,
                            enemyTeamHpY + OffsetY + 14, (int)(-enemyteamhpBar * scale), green);
                        //Allies Damage
                        Drawing.DrawText(myTeamDmgX + OffsetX, myTeamDmgY + OffsetY - 2, white,
                            "Allies Damage(" + myteamdmgBar + ")");
                        Drawing.DrawLine(myTeamDmgX + OffsetX, myTeamDmgY + OffsetY, myTeamDmgX + OffsetX,
                            myTeamDmgY + OffsetY + 14, (int)(-myteamdmgBar * scale), red);
                        //Enemies Damage
                        Drawing.DrawText(enemyTeamDmgX + OffsetX, enemyTeamDmgY + OffsetY - 2, white,
                            "Enemies Damage(" + enemyteamdmgBar + ")");
                        Drawing.DrawLine(enemyTeamDmgX + OffsetX, enemyTeamDmgY + OffsetY, enemyTeamDmgX + OffsetX,
                            enemyTeamDmgY + OffsetY + 14, (int)(-enemyteamdmgBar * scale), red);
                    }
                    else
                    {
                        //Background
                        Drawing.DrawLine(myTeamHpX + OffsetX, myTeamHpY + 30 + OffsetY, enemyTeamDmgX + OffsetX,
                            enemyTeamDmgY + OffsetY, 200, Color.FromArgb(200, 180, 180, 180));
                        //Background BORDERS
                        Drawing.DrawLine(myTeamHpX + OffsetX-1, myTeamHpY + 30 + OffsetY, enemyTeamDmgX + OffsetX-1,
                            enemyTeamDmgY + OffsetY, 1, white);
                        Drawing.DrawLine(myTeamHpX + OffsetX +200, myTeamHpY + 30 + OffsetY, enemyTeamDmgX + OffsetX +200,
                            enemyTeamDmgY + OffsetY, 1, white);
                        Drawing.DrawLine(myTeamHpX + OffsetX+100, myTeamHpY + 30 + OffsetY, myTeamHpX + OffsetX+100,
                            myTeamHpY + 30 + OffsetY, 200, white);
                        Drawing.DrawLine(myTeamHpX + OffsetX+100, myTeamHpY + 30 + OffsetY - 91, myTeamHpX + OffsetX+100,
                            myTeamHpY + 30 + OffsetY - 91, 202, white);
                        //Allies Health text
                        aHealt.text = "Allies Health(" + myteamhpBar + ")";
                        aHealt.X = myTeamHpX + OffsetX;
                        aHealt.Y = myTeamHpY + OffsetY - 2;
                        Drawing.DrawLine(myTeamHpX + OffsetX, myTeamHpY + OffsetY, myTeamHpX + OffsetX,
                            myTeamHpY + OffsetY + 14, (int)(myteamhpBar * scale * -1), green);
                        //Enemies Health text
                        eHealt.text = "Enemies Health(" + enemyteamhpBar + ")";
                        eHealt.X = enemyTeamHpX + OffsetX;
                        eHealt.Y = enemyTeamHpY + OffsetY - 2;
                        Drawing.DrawLine(enemyTeamHpX + OffsetX, enemyTeamHpY + OffsetY, enemyTeamHpX + OffsetX,
                            enemyTeamHpY + OffsetY + 14, (int)(-enemyteamhpBar * scale), green);
                        //Allies Damage
                        aDmg.text = "Allies Damage(" + myteamdmgBar + ")";
                        aDmg.X = myTeamDmgX + OffsetX;
                        aDmg.Y = myTeamDmgY + OffsetY - 2;
                        Drawing.DrawLine(myTeamDmgX + OffsetX, myTeamDmgY + OffsetY, myTeamDmgX + OffsetX,
                            myTeamDmgY + OffsetY + 14, (int) (-myteamdmgBar*scale), red);
                        //Enemies Damage
                        eDmg.text = "Enemies Damage(" + enemyteamdmgBar + ")";
                        eDmg.X = enemyTeamDmgX + OffsetX;
                        eDmg.Y = enemyTeamDmgY + OffsetY - 2;
                        Drawing.DrawLine(enemyTeamDmgX + OffsetX, enemyTeamDmgY + OffsetY, enemyTeamDmgX + OffsetX,
                            enemyTeamDmgY + OffsetY + 14, (int) (-enemyteamdmgBar*scale), red);
                    }
                }
                if (!Config.Item("Small").GetValue<bool>())
                {
                    if (myteamhpBar - enemyteamdmgBar < enemyteamhpBar - myteamdmgBar)
                    {
                        Drawing.DrawText(myTeamHpX + 45 + OffsetX, myTeamHpY + OffsetY + 13, red,
                            "Enemy team is stronger");
                        Drawing.DrawText(myTeamHpX + 225 + OffsetX, myTeamHpY + OffsetY + 13, green,
                            teams.myTeamNum.ToString());
                        Drawing.DrawText(myTeamHpX + 235 + OffsetX, myTeamHpY + OffsetY + 13, white, "v");
                        Drawing.DrawText(myTeamHpX + 245 + OffsetX, myTeamHpY + OffsetY + 13, red,
                            teams.enemyTeamNum.ToString());
                    }
                    else
                    {
                        Drawing.DrawText(myTeamHpX + 55 + OffsetX, myTeamHpY + OffsetY + 13, green,
                            "Your team is stronger");
                        Drawing.DrawText(myTeamHpX + 220 + OffsetX, myTeamHpY + OffsetY + 13, green,
                            teams.myTeamNum.ToString());
                        Drawing.DrawText(myTeamHpX + 230 + OffsetX, myTeamHpY + OffsetY + 13, white, "v");
                        Drawing.DrawText(myTeamHpX + 240 + OffsetX, myTeamHpY + OffsetY + 13, red,
                            teams.enemyTeamNum.ToString());
                    }
                }
                else
                {
                    if (myteamhpBar - enemyteamdmgBar < enemyteamhpBar - myteamdmgBar)
                    {
                        gPower.text = "";
                        ePower.text = "Enemy team is stronger";
                        ePower.X = myTeamHpX + 20 + OffsetX;
                        ePower.Y = myTeamHpY + OffsetY + 14;
                        aNum.text = teams.myTeamNum.ToString();
                        aNum.X = myTeamHpX + 155 + OffsetX;
                        aNum.Y = myTeamHpY + OffsetY + 14;
                        versus.text = "v";
                        versus.X = myTeamHpX + 163 + OffsetX;
                        versus.Y = myTeamHpY + OffsetY + 14;
                        eNum.text = teams.enemyTeamNum.ToString();
                        eNum.X = myTeamHpX + 171 + OffsetX;
                        eNum.Y = myTeamHpY + OffsetY + 14;
                    }
                    else
                    {
                        ePower.text = "";
                        gPower.text = "Your team is stronger";
                        gPower.X = myTeamHpX + 30 + OffsetX;
                        gPower.Y = myTeamHpY + OffsetY + 14;
                        aNum.text = teams.myTeamNum.ToString();
                        aNum.X = myTeamHpX + 150 + OffsetX;
                        aNum.Y = myTeamHpY + OffsetY + 14;
                        versus.text = "v";
                        versus.X = myTeamHpX + 158 + OffsetX;
                        versus.Y = myTeamHpY + OffsetY + 14;
                        eNum.text = teams.enemyTeamNum.ToString();
                        eNum.X = myTeamHpX + 166 + OffsetX;
                        eNum.Y = myTeamHpY + OffsetY + 14;
                    }
                }

            }
            else
            {
                resetSmallText(null,null);
            } 
            DrawCircle("draw", range);
        }
        private static int countAllies(int range)
        {
            return ObjectManager.Get<Obj_AI_Hero>().Count(i => player.Distance(i) < range && !i.IsDead && !i.IsMinion && (i.IsAlly || i.IsMe) && !i.IsEnemy && i.IsValid);
        }
        private static Render.Sprite loadFrame()
        {

                var load = new Render.Sprite(Resources.tsframe, new SharpDX.Vector2(0, 0))
                {
                    Color = new SharpDX.ColorBGRA(255f, 255f, 255f, 20f)
                };
                load.Position = new SharpDX.Vector2((int)(Drawing.Width * 0.628), (int)(Drawing.Height - 102));
                load.Show();
                load.Add(0);
                return load;
        }
        private static void DrawCircle(string menuItem, float range)
        {
            Circle circle = Config.Item(menuItem).GetValue<Circle>();
            if (circle.Active) Render.Circle.DrawCircle(player.Position, range, circle.Color);
        }

        private static Render.Text loadText(string text, ColorBGRA color)
        {
            var load = new Render.Text(new Vector2(0,0), text, 15, color,
                "Calibri");
            load.Add(0);
            return load;
        }

    }
}
