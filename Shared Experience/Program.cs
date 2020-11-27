using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SharedExperience
{
    class Program
    {
        static Menu menu;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        public static void OnGameLoad(EventArgs args)
        {
            LoadMenu();
            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat("<font color=\"#00BFFF\">Shared Experience</font> <font color=\"#FFFFFF\"> - Loaded</font>");
        }

        public static float[] Exp = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        public static int[] SharingCount = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        public static int[] VisibleCount = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        public static int[] InvisibleCount = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        public static int[] TimeSharingChange = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        public static int[] TimeChangedExp = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static int[] TimeUpdateVisible = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        public static int[] TimeMissing = { Environment.TickCount, Environment.TickCount, Environment.TickCount, Environment.TickCount, Environment.TickCount, Environment.TickCount, Environment.TickCount, Environment.TickCount, Environment.TickCount, Environment.TickCount };
        public static int[] IsNearMe = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static int VisibleTotal = 0;
        public static int AliveTotal = 0;
        public static int Invisible = 0;
        public static Color[] MissingColor = { Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White };
        public static Color[] VisibleColor = { Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White };

        public static Vector3[] LastMinionPosition = { Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero };
        public static Vector3 MyPos = Vector3.Zero;
        public static float RangedMinonExp = 29.44f;
        public static float MeleeMiniobExp = 58.88f;
        public static float SiegeMinionExp = 92.00f;
        public static Color[] Cor = { Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White};

        public static void OnGameUpdate(EventArgs args)
        {
            int i = -1;
            float expReceived = 0;

            Invisible = -1;
            VisibleTotal = 0;
            AliveTotal = 0;

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                i += 1;

                if (hero.IsMe)
                {
                    MyPos = hero.Position;
                }

                if (hero.IsAlly) continue;

                if (hero.IsEnemy && !hero.IsDead)
                {
                    AliveTotal += 1;
                    if (hero.IsVisible)
                    {
                        VisibleTotal += 1;
                    }
                    else
                    {
                        Invisible = i;
                    }

                }

                if (!hero.IsVisible)
                {
                    Exp[i] = (hero.Level);
                    SharingCount[i] = 0;
                    VisibleCount[i] = 0;
                    InvisibleCount[i] = 0;
                    TimeChangedExp[i] = 0;

                    
                    float t = ((Vector3.Distance(hero.Position, MyPos) * 1000 / hero.MoveSpeed) - (Environment.TickCount - TimeMissing[i]));

                    if (t <= 4000) MissingColor[i] = Color.FromArgb(255, 255, 80, 0);
                    else if (t <= 6000) MissingColor[i] = Color.FromArgb(255, 255, 120, 0);
                    else if (t <= 8000) MissingColor[i] = Color.FromArgb(255, 255, 150, 0);
                    else if (t <= 10000) MissingColor[i] = Color.FromArgb(255, 255, 180, 0);
                    else if (t <= 12000) MissingColor[i] = Color.FromArgb(255, 255, 210, 0);
                    else if (t <= 14000) MissingColor[i] = Color.FromArgb(255, 225, 200, 0);
                    else if (t <= 16000) MissingColor[i] = Color.FromArgb(255, 200, 190, 0);
                    else if (t <= 18000) MissingColor[i] = Color.FromArgb(255, 180, 180, 0);
                    else if (t <= 20000) MissingColor[i] = Color.FromArgb(255, 150, 190, 0);
                    else if (t > 22000) MissingColor[i] = Color.FromArgb(255, 100, 200, 0);

                    if (Environment.TickCount - TimeMissing[i] >= 5000) 
                    {
                        if (IsNearMe[i] == 1 && (Vector3.Distance(hero.Position, MyPos) > 5500)) TimeMissing[i] = Environment.TickCount + 30000;
                        IsNearMe[i] = 0;
                    }

                    if (hero.IsDead)
                    {
                        MissingColor[i] = Color.FromArgb(255, 200, 200, 200);

                        IsNearMe[i] = 0;
                    }

                    continue;
                }
                else
                {

                    if (Vector3.Distance(hero.Position, MyPos) <= 5500)
                    {
                        IsNearMe[i] = 1;
                    }
                    else if (Vector3.Distance(hero.Position, MyPos) > 5500)
                    {
                        IsNearMe[i] = 0;
                    }

                    float t = (Vector3.Distance(hero.Position, MyPos) * 1000 / hero.MoveSpeed);

                    if (IsNearMe[i] == 1)
                    {
                        
                        if (t <= 4000) MissingColor[i] = Color.FromArgb(255, 255, 80, 0);
                        else if (t <= 6000) MissingColor[i] = Color.FromArgb(255, 255, 120, 0);
                        else if (t <= 8000) MissingColor[i] = Color.FromArgb(255, 255, 150, 0);
                        else if (t <= 10000) MissingColor[i] = Color.FromArgb(255, 255, 180, 0);
                        else if (t <= 12000) MissingColor[i] = Color.FromArgb(255, 255, 210, 0);
                        else if (t <= 14000) MissingColor[i] = Color.FromArgb(255, 225, 200, 0);
                        else if (t <= 16000) MissingColor[i] = Color.FromArgb(255, 200, 190, 0);
                        else if (t <= 18000) MissingColor[i] = Color.FromArgb(255, 180, 180, 0);
                        else if (t <= 20000) MissingColor[i] = Color.FromArgb(255, 150, 190, 0);
                        else if (t > 22000) MissingColor[i] = Color.FromArgb(255, 100, 200, 0);

                        if ((Environment.TickCount - TimeChangedExp[i]) < 5000 && InvisibleCount[i] > 0)
                        {
                            if (AliveTotal - VisibleTotal == 1)
                            {
                                if (Vector3.Distance(hero.Position, MyPos) <= 3000)
                                {
                                    VisibleColor[Invisible] = Color.FromArgb(255, 255, 0, 0);
                                    MissingColor[Invisible] = Color.FromArgb(255, 255, 0, 0);
                                }
                            }
                        }
                    }
                    else
                    {
                        MissingColor[i] = Color.FromArgb(255, 0, 255, 0);
                    }

                    if (t <= 4000) VisibleColor[i] = Color.FromArgb(255, 255, 80, 0);
                    else if (t <= 6000) VisibleColor[i] = Color.FromArgb(255, 255, 120, 0);
                    else if (t <= 8000) VisibleColor[i] = Color.FromArgb(255, 255, 150, 0);
                    else if (t <= 10000) VisibleColor[i] = Color.FromArgb(255, 255, 180, 0);
                    else if (t <= 12000) VisibleColor[i] = Color.FromArgb(255, 255, 210, 0);
                    else if (t <= 14000) VisibleColor[i] = Color.FromArgb(255, 225, 200, 0);
                    else if (t <= 16000) VisibleColor[i] = Color.FromArgb(255, 200, 190, 0);
                    else if (t <= 18000) VisibleColor[i] = Color.FromArgb(255, 180, 180, 0);
                    else if (t <= 20000) VisibleColor[i] = Color.FromArgb(255, 150, 190, 0);
                    else if (t > 22000) VisibleColor[i] = Color.FromArgb(255, 100, 200, 0);

                    if (hero.IsDead)
                    {
                        MissingColor[i] = Color.FromArgb(255, 200, 200, 200);
                    }
                }

                

                if (hero.IsDead || hero.IsMe || (hero.Level == 18) || hero.IsInvulnerable) continue;

                TimeMissing[i] = Environment.TickCount;

                foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>())
                {
                    if (minion.IsAlly && Vector3.Distance(hero.Position, minion.Position) <= 1400)
                    {
                        if (minion.IsDead)
                        {
                            LastMinionPosition[i] = minion.Position;
                            continue;
                        }
                        string MinionName = minion.BaseSkinName;
                        //Console.WriteLine(MinionName);
                    }
                }
                //Console.WriteLine("Minions next to " + (hero.ChampionName) + " -- " + aliveMeleeMinionCount + " Melee -- " + aliveRangedMinionCount + " Ranged -- " + aliveSiegeMinionCount + " Siege");

                if (Exp[i] != hero.Level)
                {
                    int rangedMinion = 0;   // melee == 2*ranged
                    int siegeMinion = 0;

                    TimeChangedExp[i] = Environment.TickCount; 

                    expReceived = (float)Math.Round(hero.Level - Exp[i], 2);

                    //Console.WriteLine((i) + " " + (hero.ChampionName) + "  Last exp received: " + (expReceived) + " exp ");
                    //Console.WriteLine("Got Experience from -- " + killedRangedMinionCount + " Ranged -- " + killedMeleeMinionCount + " Melee -- " + killedSiegeMinionCount + " Siege");

                    int found = 0;

                    for (int expSharingCount = 1; expSharingCount <= 5; expSharingCount++)
                    {
                        if (expSharingCount == 1)
                        {
                            for (float increasedExp = 1.00f; increasedExp <= 1.14f; increasedExp += 0.02f)
                            {
                                for (rangedMinion = 0; rangedMinion <= 20; rangedMinion += 1)
                                {
                                    if (expReceived == (Math.Round((RangedMinonExp * rangedMinion * increasedExp), 2)))
                                    {
                                        SharingCount[i] = expSharingCount;
                                        TimeSharingChange[i] = Environment.TickCount;
                                        TimeUpdateVisible[i] = 0;
                                        found = 1;
                                        break;
                                    }
                                    for (siegeMinion = 1; siegeMinion <= 3; siegeMinion += 1)
                                    {
                                        if (expReceived == (Math.Round((RangedMinonExp * rangedMinion * increasedExp), 2) + Math.Round((SiegeMinionExp * siegeMinion * increasedExp), 2)))
                                        {
                                            SharingCount[i] = expSharingCount;
                                            TimeSharingChange[i] = Environment.TickCount;
                                            TimeUpdateVisible[i] = 0;
                                            found = 1;
                                            break;
                                        }
                                    }
                                    if (found == 1) break;
                                }
                                if (found == 1) break;
                            }
                        }
                        else
                        {
                            for (float increasedExp = 1.00f; increasedExp <= 1.14f; increasedExp += 0.02f)
                            {
                                for (rangedMinion = 0; rangedMinion <= 27; rangedMinion += 1)
                                {
                                    if (expReceived == (Math.Round(((RangedMinonExp * rangedMinion * 1.30435f / expSharingCount) * increasedExp), 2)))
                                    {
                                        SharingCount[i] = expSharingCount;
                                        TimeSharingChange[i] = Environment.TickCount;
                                        TimeUpdateVisible[i] = 0;
                                        found = 1;
                                        break;
                                    }
                                    for (siegeMinion = 1; siegeMinion <= 3; siegeMinion += 1)
                                    {
                                        if (expReceived == (Math.Round(((RangedMinonExp * rangedMinion * 1.30435f / expSharingCount) * increasedExp), 2) + Math.Round(((SiegeMinionExp * siegeMinion * 1.30435f / expSharingCount) * increasedExp), 2)))
                                        {
                                            SharingCount[i] = expSharingCount;
                                            TimeSharingChange[i] = Environment.TickCount;
                                            TimeUpdateVisible[i] = 0;
                                            found = 1;
                                            break;
                                        }
                                    }
                                    if (found == 1) break;
                                }
                                if (found == 1) break;
                            }
                        }
                        if (found == 1)
                        {
                            //Console.WriteLine("Shared Experience between " + expSharingCount + " champions ");
                            break;
                        }
                    }
                    Exp[i] = (hero.Level);
                }

                int deadCount = 0;
                VisibleCount[i] = 0;
                foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (enemy.IsEnemy && Vector3.Distance(hero.Position, enemy.Position) <= (3000))
                    {
                        if (enemy.IsVisible) VisibleCount[i] += 1;
                        if (enemy.IsDead)
                        {
                            VisibleCount[i] -= 1;
                            deadCount += 1;
                        }
                    }

                    
                }

                //Console.WriteLine((i) + " " + (hero.ChampionName) + "  deadCount: " + deadCount + " VisibleCount[i] -- " + VisibleCount[i] + " SharingCount[i] -- " + SharingCount[i] + " InvisibleCount[i] -- " + InvisibleCount[i]);
                    
                if (SharingCount[i] < VisibleCount[i])
                {
                    SharingCount[i] = VisibleCount[i];
                    InvisibleCount[i] = 0;
                    TimeSharingChange[i] = Environment.TickCount;
                }
                else
                {
                    InvisibleCount[i] = (SharingCount[i] - VisibleCount[i] - deadCount);

                    if (deadCount > 0 && InvisibleCount[i] == 0)
                    {
                        SharingCount[i] = VisibleCount[i];
                        TimeSharingChange[i] = Environment.TickCount;
                    }

                    if (InvisibleCount[i] < 0) InvisibleCount[i] = 0;

                }

                TimeUpdateVisible[i] = Environment.TickCount;

                if (InvisibleCount[i] > SharingCount[i] - 1 && SharingCount[i] > 0) InvisibleCount[i] = SharingCount[i] - 1;

                if ((Environment.TickCount - TimeSharingChange[i]) >= 0)
                {
                    Cor[i] = Color.White;
                }

                if ((Environment.TickCount - TimeSharingChange[i]) >= 20000)
                {
                    SharingCount[i] = VisibleCount[i];
                    TimeSharingChange[i] = Environment.TickCount;
                }

                if (InvisibleCount[i] > 0)
                {
                    Cor[i] = menu.Item("invColor").GetValue<Color>();
                    //Console.WriteLine(Cor[i]);

                    if (VisibleTotal == 4)
                    {

                    }
                }
            }
        }

        public static void Drawing_OnDraw(EventArgs args)
        {
            int i = -1;
            int c = -1;

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                i += 1;

                if (hero.IsMe)
                {
                    MyPos = hero.Position;
                    continue;
                }

                if (hero.IsAlly) continue;

                c += 1;

                int x = menu.Item("posX").GetValue<Slider>().Value;
                int y = menu.Item("posY").GetValue<Slider>().Value + c * 30;

                if ((Environment.TickCount - TimeChangedExp[i]) < 5000 && InvisibleCount[i] > 0)
                {
                    if (menu.Item("drawPredictionCircle").GetValue<bool>()) Render.Circle.DrawCircle(LastMinionPosition[i], 1500, Color.Red);

                    if (AliveTotal - VisibleTotal == 1)
                    {
                        if (Vector3.Distance(hero.Position, MyPos) <= 3000)
                        {
                            IsNearMe[Invisible] = 1;
                            VisibleColor[Invisible] = Color.FromArgb(255, 255, 0, 0);
                            MissingColor[Invisible] = Color.FromArgb(255, 255, 0, 0);
                            TimeMissing[Invisible] = Environment.TickCount;
                        }
                    }
                }

                if (IsNearMe[i] == 1 && !hero.IsDead)
                {
                    if (menu.Item("drawchampionlist").GetValue<bool>())
                    {
                        Drawing.DrawLine(
                            new Vector2(x - 4.5f, y - 5),
                            new Vector2(x + 85, y - 5), 3, VisibleColor[i]);

                        Drawing.DrawLine(
                            new Vector2(x + 85, y - 4.5f),
                            new Vector2(x + 85, y + 21), 3, VisibleColor[i]);

                        Drawing.DrawLine(
                            new Vector2(x + 85, y + 21),
                            new Vector2(x - 3, y + 21), 3, VisibleColor[i]);

                        Drawing.DrawLine(
                            new Vector2(x - 5, y + 21),
                            new Vector2(x - 5, y - 4.5f), 3, VisibleColor[i]);


                        Drawing.DrawText(x+5, y, MissingColor[i], hero.ChampionName);
                    }
                }
                else
                {
                    if (menu.Item("drawchampionlist").GetValue<bool>())
                    {
                        Drawing.DrawText(x+5, y, MissingColor[i], hero.ChampionName);
                    }
                }

                if (!menu.Item("showEnemies").GetValue<bool>()) continue;

                int textXOffset = menu.Item("positionX").GetValue<Slider>().Value;
                int textYOffset = menu.Item("positionY").GetValue<Slider>().Value;

                

                if (SharingCount[i] > 0)
                {
                    if (InvisibleCount[i] > 0)
                    {
                        Drawing.DrawText(hero.HPBarPosition.X + textXOffset, hero.HPBarPosition.Y + textYOffset, Cor[i], "+" + (SharingCount[i] - 1) + " (" + InvisibleCount[i] + " Inv)");
                    }
                    if (!menu.Item("onlyShowInv").GetValue<bool>() && InvisibleCount[i] == 0)
                    {
                        Drawing.DrawText(hero.HPBarPosition.X + textXOffset, hero.HPBarPosition.Y + textYOffset, Cor[i], "+" + (SharingCount[i] - 1));
                    }
                }
            }
        }

        static void LoadMenu()
        {
            menu = new Menu("Shared Experience", "Shared Experience", true);
            menu.AddItem(new MenuItem("showEnemies", "Draw Text On Enemy").SetValue(true));
            menu.AddItem(new MenuItem("onlyShowInv", "Only Draw Text When Not Visible Enemies").SetValue(false));
            menu.AddItem(new MenuItem("drawPredictionCircle", "Draw Prediction Circle").SetValue(true));
            menu.AddItem(new MenuItem("invColor", "Text Color When Not Visible Enemies").SetValue(Color.FromArgb(255, 245,25,25)));
            menu.AddItem(new MenuItem("positionX", "OnEnemy Text Position X").SetValue(new Slider(142, -100, 200)));
            menu.AddItem(new MenuItem("positionY", "OnEnemy Text Position Y").SetValue(new Slider(21, -100, 100)));


            //Champion List Menu
            menu.AddSubMenu(new Menu("Champion List", "Champion List"));
            menu.SubMenu("Champion List").AddItem(new MenuItem("drawchampionlist", "Draw Champion List").SetValue(false));
            menu.SubMenu("Champion List").AddItem(new MenuItem("posX", "Champions List Pos X").SetValue(new Slider(Drawing.Width / 2, 0, Drawing.Width)));
            menu.SubMenu("Champion List").AddItem(new MenuItem("posY", "Champions List Pos Y").SetValue(new Slider(Drawing.Height / 2, 0, Drawing.Height)));

            menu.AddToMainMenu();
        }
    }
}
