using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace YasuoSharpV2
{
        internal class YasuoSharp
        {


            public const string CharName = "Yasuo";
            //Orbwalker
            public static Orbwalking.Orbwalker Orbwalker;


            public static Menu Config;

            public static Menu skillShotMenu;


            public static string lastSpell = "";

            public static int afterDash = 0;

            public static bool canSave = true;
            public static bool canExport = true;
            public static bool canDelete = true;

            public static bool wasStream = false;


            public static List<Skillshot> DetectedSkillshots = new List<Skillshot>();

            public YasuoSharp()
            {

                // map = new Map();
                /* CallBAcks */
                CustomEvents.Game.OnGameLoad += onLoad;

            }

            private static void onLoad(EventArgs args)
            {
                if (ObjectManager.Player.ChampionName != CharName)
                    return;

                Yasuo.setSkillShots();
                Yasuo.setDashes();
                Yasuo.point1 = Yasuo.Player.Position;
                Game.PrintChat("YasuoSharpV2 by DeTuKs");

                Console.WriteLine("YasuoSharpV2 by DeTuKs");

                try
                {

                    Config = new Menu("YasuoSharp", "Yasuo", true);
                    //Orbwalker
                    Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
                    Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));
                    //TS
                    var TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
                    TargetSelector.AddToMenu(TargetSelectorMenu);
                    Config.AddSubMenu(TargetSelectorMenu);
                    //Combo
                    Config.AddSubMenu(new Menu("Combo Sharp", "combo"));
                    Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items")).SetValue(true);
                    //SmartR
                    Config.SubMenu("combo").AddItem(new MenuItem("smartR", "Smart R")).SetValue(true);
                    Config.SubMenu("combo").AddItem(new MenuItem("useRHit", "Use R if hit")).SetValue(new Slider(3, 5, 1));
                    Config.SubMenu("combo").AddItem(new MenuItem("useRHitTime", "Use R when they land")).SetValue(true);
                    Config.SubMenu("combo").AddItem(new MenuItem("useEWall", "use E to safe")).SetValue(true);
                    //Flee away
                    Config.SubMenu("combo").AddItem(new MenuItem("flee", "E away")).SetValue(new KeyBind('Z', KeyBindType.Press, false));
                    Config.SubMenu("combo").AddItem(new MenuItem("fleeStack", "Stack Q while flee")).SetValue(true);


                    //LastHit
                    Config.AddSubMenu(new Menu("LastHit Sharp", "lHit"));
                    Config.SubMenu("lHit").AddItem(new MenuItem("useQlh", "Use Q")).SetValue(true);
                    Config.SubMenu("lHit").AddItem(new MenuItem("useElh", "Use E")).SetValue(true);
                    //LaneClear
                    Config.AddSubMenu(new Menu("LaneClear Sharp", "lClear"));
                    Config.SubMenu("lClear").AddItem(new MenuItem("useQlc", "Use Q")).SetValue(true);
                    Config.SubMenu("lClear").AddItem(new MenuItem("useEmpQHit", "Emp Q Min hit")).SetValue(new Slider(3, 6, 1));
                    Config.SubMenu("lClear").AddItem(new MenuItem("useElc", "Use E")).SetValue(true);
                    //Harass
                    Config.AddSubMenu(new Menu("Harass Sharp", "harass"));
                    Config.SubMenu("harass").AddItem(new MenuItem("harassTower", "Harass under tower")).SetValue(false);
                    Config.SubMenu("harass").AddItem(new MenuItem("harassOn", "Harass enemies")).SetValue(true);
                    Config.SubMenu("harass").AddItem(new MenuItem("harQ3Only", "Use only Q3")).SetValue(false);
                    //Drawings
                    Config.AddSubMenu(new Menu("Drawing Sharp", "drawing"));
                    Config.SubMenu("drawing").AddItem(new MenuItem("disDraw", "Dissabel drawing")).SetValue(false);
                    Config.SubMenu("drawing").AddItem(new MenuItem("drawQ", "Draw Q range")).SetValue(true);
                    Config.SubMenu("drawing").AddItem(new MenuItem("drawE", "Draw E range")).SetValue(true);
                    Config.SubMenu("drawing").AddItem(new MenuItem("drawR", "Draw R range")).SetValue(true);
                    Config.SubMenu("drawing").AddItem(new MenuItem("drawWJ", "Draw Wall Jumps")).SetValue(true);

                    //Extra
                    Config.AddSubMenu(new Menu("Extra Sharp", "extra"));
                    Config.SubMenu("extra").AddItem(new MenuItem("djTur", "Dont Jump turrets")).SetValue(true);
                    Config.SubMenu("extra").AddItem(new MenuItem("autoLevel", "Auto Level")).SetValue(true);
                    Config.SubMenu("extra").AddItem(new MenuItem("levUpSeq", "")).SetValue(new StringList(new string[2] { "Q E W Q start", "Q E Q W start" }));

                    //LastHit
                    Config.AddSubMenu(new Menu("Wall Usage", "aShots"));
                    //SmartW
                    Config.SubMenu("aShots").AddItem(new MenuItem("smartW", "Smart WW")).SetValue(true);
                    Config.SubMenu("aShots").AddItem(new MenuItem("smartEDogue", "E use dogue")).SetValue(true);
                    Config.SubMenu("aShots").AddItem(new MenuItem("wwDanger", "WW only dangerous")).SetValue(false);
                    Config.SubMenu("aShots").AddItem(new MenuItem("wwDmg", "WW if does proc HP")).SetValue(new Slider(0, 100, 1));
                    skillShotMenu = getSkilshotMenu();
                    Config.SubMenu("aShots").AddSubMenu(skillShotMenu);

                    Config.SubMenu("aShots").AddSubMenu(TargetedSpellManager.setUp());
                    //Streaming
                    Config.AddSubMenu(new Menu("Stream Sharp", "stream"));
                    Config.SubMenu("stream").AddItem(new MenuItem("streamMouse", "SimulateMouse")).SetValue(false);
                   
                    //Debug
                    Config.AddSubMenu(new Menu("Debug", "debug"));
                    Config.SubMenu("debug").AddItem(new MenuItem("WWLast", "Print last ww blocked")).SetValue(new KeyBind('T', KeyBindType.Press, false));
                    Config.SubMenu("debug").AddItem(new MenuItem("saveDash", "saveDashd")).SetValue(new KeyBind('O', KeyBindType.Press, false));
                    Config.SubMenu("debug").AddItem(new MenuItem("exportDash", "export dashes")).SetValue(new KeyBind('P', KeyBindType.Press, false));
                    Config.SubMenu("debug").AddItem(new MenuItem("deleteDash", "deleteLastDash")).SetValue(new KeyBind('I', KeyBindType.Press, false));

                    Config.AddToMainMenu();

                    TargetSpellDetector.init();

                    Drawing.OnDraw += onDraw;
                    Game.OnUpdate += OnGameUpdate;

                    GameObject.OnCreate += OnCreateObject;
                    GameObject.OnDelete += OnDeleteObject;
                    Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
                    Spellbook.OnStopCast += onStopCast;
                    
                    CustomEvents.Unit.OnLevelUp += OnLevelUp;

                    

                    SkillshotDetector.OnDetectSkillshot += OnDetectSkillshot;
                    SkillshotDetector.OnDeleteMissile += OnDeleteMissile;


                    Orbwalking.BeforeAttack += beforeAttack;
                    SmoothMouse.start();
                }
                catch
                {
                    Game.PrintChat("Oops. Something went wrong with Yasuo - Sharpino");
                }

            }


           

            private static void beforeAttack(Orbwalking.BeforeAttackEventArgs args)
            {
                SmoothMouse.addMouseEvent(args.Target.Position);
                
            }


            public static Menu getSkilshotMenu()
            {
                //Create the skillshots submenus.
                var skillShots = new Menu("Enemy Skillshots", "aShotsSkills");

                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (hero.Team != ObjectManager.Player.Team)
                    {
                        foreach (var spell in SpellDatabase.Spells)
                        {
                            if (spell.ChampionName == hero.ChampionName)
                            {
                                var subMenu = new Menu(spell.MenuItemName, spell.MenuItemName);

                                subMenu.AddItem(
                                    new MenuItem("DangerLevel" + spell.MenuItemName, "Danger level").SetValue(
                                        new Slider(spell.DangerValue, 5, 1)));

                                subMenu.AddItem(
                                    new MenuItem("IsDangerous" + spell.MenuItemName, "Is Dangerous").SetValue(
                                        spell.IsDangerous));

                                subMenu.AddItem(new MenuItem("Draw" + spell.MenuItemName, "Draw").SetValue(true));
                                subMenu.AddItem(new MenuItem("Enabled" + spell.MenuItemName, "Enabled").SetValue(true));

                                skillShots.AddSubMenu(subMenu);
                            }
                        }
                    }
                }
                return skillShots;
            }

            public static bool skillShotIsDangerous(string Name)
            {
                if (skillShotMenu.Item("IsDangerous" + Name) != null)
                {
                    return skillShotMenu.Item("IsDangerous" + Name).GetValue<bool>();
                }
                return true;
            }

            public static bool EvadeSpellEnabled(string Name)
            {
                if (skillShotMenu.Item("Enabled" + Name) != null)
                {
                    return skillShotMenu.Item("Enabled" + Name).GetValue<bool>();
                }
                return true;
            }

            public static void updateSkillshots()
            {
                foreach (var ss in DetectedSkillshots)
                {
                    ss.Game_OnGameUpdate();
                }
            }

            private static void OnGameUpdate(EventArgs args)
            {
                try
                {

                    if (!wasStream && Config.Item("streamMouse").GetValue<bool>())
                    {
                        SmoothMouse.start();
                        wasStream = true;
                    }
                    else if (!Config.Item("streamMouse").GetValue<bool>())
                    {
                        wasStream = false;
                    }


                    Yasuo.Q.SetSkillshot(Yasuo.getNewQSpeed(), 50f, float.MaxValue, false, SkillshotType.SkillshotLine);

                    if (Yasuo.startDash + 470000/((700 + Yasuo.Player.MoveSpeed)) < Environment.TickCount && Yasuo.isDashigPro)
                    {
                        Yasuo.isDashigPro = false;
                    }

                    //updateSkillshots();
                    //Remove the detected skillshots that have expired.
                    DetectedSkillshots.RemoveAll(skillshot => !skillshot.IsActive());

                    Obj_AI_Hero target = TargetSelector.GetTarget((Yasuo.E.IsReady()) ? 1500 : 475, TargetSelector.DamageType.Physical);
                    if (Orbwalker.ActiveMode.ToString() == "Combo")
                    {
                        Yasuo.doCombo(target);
                    }

                    if (Orbwalker.ActiveMode.ToString() == "LastHit")
                    {
                        Yasuo.doLastHit(target);
                        Yasuo.useQSmart(target);
                    }

                    if (Orbwalker.ActiveMode.ToString() == "Mixed")
                    {
                        Yasuo.doLastHit(target);
                        Yasuo.useQSmart(target);
                    }

                    if (Orbwalker.ActiveMode.ToString() == "LaneClear")
                    {
                        Yasuo.doLaneClear(target);
                    }

                    if (Config.Item("flee").GetValue<KeyBind>().Active)
                    {
                        Yasuo.fleeToMouse();
                        Yasuo.stackQ();
                    }

                    if (Config.Item("saveDash").GetValue<KeyBind>().Active && canSave)
                    {
                        Yasuo.saveLastDash();
                        canSave = false;
                    }
                    else
                    {
                        canSave = true;
                    }

                    if (Config.Item("deleteDash").GetValue<KeyBind>().Active && canDelete)
                    {
                        if (Yasuo.dashes.Count > 0)
                            Yasuo.dashes.RemoveAt(Yasuo.dashes.Count - 1);
                        canDelete = false;
                    }
                    else
                    {
                        canDelete = true;
                    }
                    if (Config.Item("exportDash").GetValue<KeyBind>().Active && canExport)
                    {
                        using (var file = new System.IO.StreamWriter(@"C:\YasuoDashes.txt"))
                        {

                            foreach (var dash in Yasuo.dashes)
                            {
                                string dashS = "dashes.Add(new YasDash(new Vector3(" +
                                               dash.from.X.ToString("0.00").Replace(',', '.') + "f," +
                                               dash.from.Y.ToString("0.00").Replace(',', '.') + "f," +
                                               dash.from.Z.ToString("0.00").Replace(',', '.') +
                                               "f),new Vector3(" + dash.to.X.ToString("0.00").Replace(',', '.') + "f," +
                                               dash.to.Y.ToString("0.00").Replace(',', '.') + "f," +
                                               dash.to.Z.ToString("0.00").Replace(',', '.') + "f)));";
                                //new YasDash(new Vector3(X,Y,Z),new Vector3(X,Y,Z))

                                file.WriteLine(dashS);
                            }
                            file.Close();
                        }

                        canExport = false;
                    }
                    else
                    {
                        canExport = true;
                    }

                    if (Config.Item("WWLast").GetValue<KeyBind>().Active)
                    {
                        Console.WriteLine("Last WW skill blocked: " + lastSpell);
                        Game.PrintChat("Last WW skill blocked: " + lastSpell);
                    }

                    if (Config.Item("harassOn").GetValue<bool>() && Orbwalker.ActiveMode.ToString() == "None")
                    {
                        if (target!= null)
                            Yasuo.useQSmart(target, Config.Item("harQ3Only").GetValue<bool>());
                    }

                    foreach (var mis in DetectedSkillshots)
                    {
                        Yasuo.useWSmart(mis);

                        if (Config.Item("smartEDogue").GetValue<bool>() && !Yasuo.isSafePoint(Yasuo.Player.Position.To2D(),true).IsSafe)
                            Yasuo.useEtoSafe(mis);
                    }

                    if (Config.Item("smartR").GetValue<bool>() && Yasuo.R.IsReady())
                        Yasuo.useRSmart();

                    Yasuo.processTargetedSpells();



                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            private static void onDraw(EventArgs args)
            {
                if (Config.Item("disDraw").GetValue<bool>())
                    return;



                Drawing.DrawText(100,100,Color.Red,"targ Spells: "+TargetSpellDetector.ActiveTargeted.Count);

                foreach (Obj_AI_Base jun in MinionManager.GetMinions(Yasuo.Player.ServerPosition, 700, MinionTypes.All, MinionTeam.Neutral))
                {
                Render.Circle.DrawCircle(jun.Position, 70, Color.Green);
                    Vector2 posAfterE = Yasuo.Player.ServerPosition.To2D() + (Vector2.Normalize(jun.ServerPosition.To2D() - Yasuo.Player.ServerPosition.To2D()) * 475);
                // Vector2 posAfterE = Yasuo.Player.Position.To2D().Extend(jun.Position.To2D(), 475);//jun.ServerPosition.To2D().Extend() + (Vector2.Normalize(Yasuo.Player.Position.To2D() - jun.ServerPosition.To2D()) * 475);
                Render.Circle.DrawCircle(posAfterE.To3D(), 50, Color.Violet);
                    Vector3 posAfterDash = Yasuo.Player.GetPath(posAfterE.To3D()).Last();
                    Render.Circle.DrawCircle(posAfterDash, 50, Color.DarkRed);

                }

                if (Config.Item("drawQ").GetValue<bool>())
                    Utility.DrawCircle(Yasuo.Player.Position, 475, (Yasuo.isDashigPro) ? Color.Red : Color.Blue, 10, 10);
                if (Config.Item("drawR").GetValue<bool>())
                    Utility.DrawCircle(Yasuo.Player.Position, 1200, Color.Blue);

                if (Config.Item("flee").GetValue<KeyBind>().Active && Config.Item("drawWJ").GetValue<bool>())
                {
                    Utility.DrawCircle(Game.CursorPos, 350, Color.Cyan);

                    Utility.DrawCircle(Yasuo.lastDash.from, 60, Color.BlueViolet);
                    Utility.DrawCircle(Yasuo.lastDash.to, 60, Color.BlueViolet);

                    foreach (Yasuo.YasDash dash in Yasuo.dashes)
                    {
                        if (dash.from.Distance(Game.CursorPos) < 1200)
                        {
                            var SA = Drawing.WorldToScreen(dash.from);
                            var SB = Drawing.WorldToScreen(dash.to);
                            Drawing.DrawLine(SA.X, SA.Y, SB.X, SB.Y, 3, Color.Green);
                        }
                    }

                }
                
            
                /*   if ((int)NavMesh.GetCollisionFlags(Game.CursorPos) == 2 || (int)NavMesh.GetCollisionFlags(Game.CursorPos) == 64)
                    Drawing.DrawCircle(Game.CursorPos, 70, Color.Green);
                if (map.isWall(Game.CursorPos.To2D()))
                    Drawing.DrawCircle(Game.CursorPos, 100, Color.Red);

                foreach (Polygon pol in map.poligs)
                {
                    pol.Draw(Color.BlueViolet, 3);
                }

                foreach(Obj_AI_Base jun in MinionManager.GetMinions(Yasuo.Player.ServerPosition,700,MinionTypes.All,MinionTeam.Neutral))
                {
                    Drawing.DrawCircle(jun.Position, 70, Color.Green);
                     SharpDX.Vector2 proj = map.getClosestPolygonProj(jun.ServerPosition.To2D());
                     SharpDX.Vector2 posAfterE = jun.ServerPosition.To2D() + (SharpDX.Vector2.Normalize(proj - jun.ServerPosition.To2D() ) * 475);
                     Drawing.DrawCircle(posAfterE.To3D(), 50, Color.Violet);
                }
            
                foreach (MissileClient mis in skillShots)
                {
                    Drawing.DrawCircle(mis.Position, 47, Color.Orange);
                    Drawing.DrawCircle(mis.EndPosition, 100, Color.BlueViolet);
                   Drawing.DrawCircle(mis.SpellCaster.Position, Yasuo.Player.BoundingRadius + mis.SData.LineWidth, Color.DarkSalmon);
                    Drawing.DrawCircle(mis.StartPosition, 70, Color.Green);
                }*/

            }

            private static void OnCreateObject(GameObject sender, EventArgs args)
            {
                //wall
                if (sender is MissileClient)
                {
                MissileClient missle = (MissileClient)sender;
                    if (missle.SData.Name == "yasuowmovingwallmisl")
                    {
                        Yasuo.wall.setL(missle);
                    }

                    if (missle.SData.Name == "yasuowmovingwallmisr")
                    {
                        Yasuo.wall.setR(missle);
                    }
                }

                if (sender is MissileClient && ((MissileClient)sender).Target.IsMe)
                    TargetSpellDetector.setParticle((MissileClient)sender);
            }

            private static void OnDeleteObject(GameObject sender, EventArgs args)
            {
                /* int i = 0;
                 foreach (var lho in skillShots)
                 {
                     if (lho.NetworkId == sender.NetworkId)
                     {
                         skillShots.RemoveAt(i);
                         return;
                     }
                     i++;
                 }*/
            }


            private static void onStopCast(Spellbook obj, SpellbookStopCastEventArgs args)
            {
                if (obj.Owner.IsMe)
                {
                    if (obj.Owner.IsValid && args.DestroyMissile && args.StopAnimation)
                    {
                        Yasuo.isDashigPro = false;
                    }
                }
            }

            public static void OnProcessSpell(Obj_AI_Base obj, GameObjectProcessSpellCastEventArgs arg)
            {
                if (obj.IsMe)
                {
                    if (arg.SData.Name == "YasuoDashWrapper")//start dash
                    {
                        Console.WriteLine("--- DAhs started---");
                        Yasuo.lastDash.from = Yasuo.Player.Position;
                        Yasuo.isDashigPro = true;
                        Yasuo.castFrom = Yasuo.Player.Position;
                        Yasuo.startDash = Environment.TickCount;
                    }
                }
            }

            public static void OnLevelUp(LeagueSharp.Obj_AI_Base sender, LeagueSharp.Common.CustomEvents.Unit.OnLevelUpEventArgs args)
            {
                if (sender.NetworkId == Yasuo.Player.NetworkId)
                {
                    if (!Config.Item("autoLevel").GetValue<bool>())
                        return;
                    if (Config.Item("levUpSeq").GetValue<StringList>().SelectedIndex == 0)
                        Yasuo.sBook.LevelUpSpell(Yasuo.levelUpSeq[args.NewLevel - 1].Slot);
                    else if (Config.Item("levUpSeq").GetValue<StringList>().SelectedIndex == 1)
                        Yasuo.sBook.LevelUpSpell(Yasuo.levelUpSeq2[args.NewLevel - 1].Slot);
                }
            }



            private static void OnGameProcessPacket(GamePacketEventArgs args)
            {//28 16 176 ??184
                if (args.PacketData[0] == 41)//135no 100no 183no 34no 101 133 56yesss? 127 41yess
                {
                    GamePacket gp = new GamePacket(args.PacketData);
                    //Console.WriteLine(Encoding.UTF8.GetString(args.PacketData, 0, args.PacketData.Length));
                    gp.Position = 1;
                    if (gp.ReadInteger() == Yasuo.Player.NetworkId /*&&  Encoding.UTF8.GetString(args.PacketData, 0, args.PacketData.Length).Contains("Spell3")*/)
                    {
                        Console.WriteLine("----");
                        Yasuo.lastDash.to = Yasuo.Player.Position;
                        Yasuo.isDashigPro = false;
                        Yasuo.time = Game.Time - Yasuo.startDash;
                    }
                    /* for (int i = 1; i < gp.Size() - 4; i++)
                     {
                         gp.Position = i;
                         if (gp.ReadInteger() == Yasuo.Player.NetworkId)
                         {
                             Console.WriteLine("Found: "+i);
                         }
                     }

                     Console.WriteLine("End dash");
                     Yasuo.Q.Cast(Yasuo.Player.Position);*/
                }

                /*if (args.PacketData[0] == 176) //135no 100no 183no 34no 101 133 56yesss? 127 41yess
                {
                    GamePacket gp = new GamePacket(args.PacketData);
                    //Console.WriteLine(Encoding.UTF8.GetString(args.PacketData, 0, args.PacketData.Length));
                    gp.Position = 1;
                    if (gp.ReadInteger() == Yasuo.Player.NetworkId)
                    {
                        Console.WriteLine("--- DAhs started Packets---");
                        Yasuo.lastDash.from = Yasuo.Player.Position;
                        Yasuo.isDashigPro = true;
                        Yasuo.castFrom = Yasuo.Player.Position;
                        Yasuo.startDash = Game.Time;
                    }
                }*/
            }

            private static void OnGameSendPacket(GamePacketEventArgs args)
            {
                /*if (args.PacketData[0] == 154) //135no 100no 183no 34no 101 133 56yesss? 127 41yess
                {
                    var spell = Packet.C2S.Cast.Decoded(args.PacketData);
                    if (spell.Slot == Yasuo.E.Slot)
                    {
                        Console.WriteLine("--- DAhs started Packets---");
                        Yasuo.lastDash.from = Yasuo.Player.Position;
                        Yasuo.isDashigPro = true;
                        Yasuo.castFrom = Yasuo.Player.Position;
                        Yasuo.startDash = Game.Time;
                    }
                }*/
            }



            private static void OnDeleteMissile(Skillshot skillshot, MissileClient missile)
            {
                if (skillshot.SpellData.SpellName == "VelkozQ")
                {
                    var spellData = SpellDatabase.GetByName("VelkozQSplit");
                    var direction = skillshot.Direction.Perpendicular();
                    if (DetectedSkillshots.Count(s => s.SpellData.SpellName == "VelkozQSplit") == 0)
                    {
                        for (var i = -1; i <= 1; i = i + 2)
                        {
                            var skillshotToAdd = new Skillshot(
                                DetectionType.ProcessSpell, spellData, Environment.TickCount, missile.Position.To2D(),
                                missile.Position.To2D() + i * direction * spellData.Range, skillshot.Unit);
                            DetectedSkillshots.Add(skillshotToAdd);
                        }
                    }
                }
            }

            private static void OnDetectSkillshot(Skillshot skillshot)
            {
                var alreadyAdded = false;

                foreach (var item in DetectedSkillshots)
                {
                    if (item.SpellData.SpellName == skillshot.SpellData.SpellName &&
                        (item.Unit.NetworkId == skillshot.Unit.NetworkId &&
                         (skillshot.Direction).AngleBetween(item.Direction) < 5 &&
                         (skillshot.Start.Distance(item.Start) < 100 || skillshot.SpellData.FromObjects.Length == 0)))
                    {
                        alreadyAdded = true;
                    }
                }

                //Check if the skillshot is from an ally.
                if (skillshot.Unit.Team == ObjectManager.Player.Team)
                {
                    return;
                }

                //Check if the skillshot is too far away.
                if (skillshot.Start.Distance(ObjectManager.Player.ServerPosition.To2D()) >
                    (skillshot.SpellData.Range + skillshot.SpellData.Radius + 1000) * 1.5)
                {
                    return;
                }

                //Add the skillshot to the detected skillshot list.
                if (!alreadyAdded)
                {
                    //Multiple skillshots like twisted fate Q.
                    if (skillshot.DetectionType == DetectionType.ProcessSpell)
                    {
                        if (skillshot.SpellData.MultipleNumber != -1)
                        {
                            var originalDirection = skillshot.Direction;

                            for (var i = -(skillshot.SpellData.MultipleNumber - 1) / 2;
                                i <= (skillshot.SpellData.MultipleNumber - 1) / 2;
                                i++)
                            {
                                var end = skillshot.Start +
                                          skillshot.SpellData.Range *
                                          originalDirection.Rotated(skillshot.SpellData.MultipleAngle * i);
                                var skillshotToAdd = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                                    skillshot.Unit);

                                DetectedSkillshots.Add(skillshotToAdd);
                            }
                            return;
                        }

                        if (skillshot.SpellData.SpellName == "UFSlash")
                        {
                            skillshot.SpellData.MissileSpeed = 1600 + (int)skillshot.Unit.MoveSpeed;
                        }

                        if (skillshot.SpellData.Invert)
                        {
                            var newDirection = -(skillshot.End - skillshot.Start).Normalized();
                            var end = skillshot.Start + newDirection * skillshot.Start.Distance(skillshot.End);
                            var skillshotToAdd = new Skillshot(
                                skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                                skillshot.Unit);
                            DetectedSkillshots.Add(skillshotToAdd);
                            return;
                        }

                        if (skillshot.SpellData.Centered)
                        {
                            var start = skillshot.Start - skillshot.Direction * skillshot.SpellData.Range;
                            var end = skillshot.Start + skillshot.Direction * skillshot.SpellData.Range;
                            var skillshotToAdd = new Skillshot(
                                skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                                skillshot.Unit);
                            DetectedSkillshots.Add(skillshotToAdd);
                            return;
                        }

                        if (skillshot.SpellData.SpellName == "SyndraE" || skillshot.SpellData.SpellName == "syndrae5")
                        {
                            var angle = 60;
                            var edge1 =
                                (skillshot.End - skillshot.Unit.ServerPosition.To2D()).Rotated(
                                    -angle / 2 * (float)Math.PI / 180);
                            var edge2 = edge1.Rotated(angle * (float)Math.PI / 180);

                            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
                            {
                                var v = minion.ServerPosition.To2D() - skillshot.Unit.ServerPosition.To2D();
                                if (minion.Name == "Seed" && edge1.CrossProduct(v) > 0 && v.CrossProduct(edge2) > 0 &&
                                    minion.Distance(skillshot.Unit) < 800 &&
                                    (minion.Team != ObjectManager.Player.Team))
                                {
                                    var start = minion.ServerPosition.To2D();
                                    var end = skillshot.Unit.ServerPosition.To2D()
                                        .Extend(
                                            minion.ServerPosition.To2D(),
                                            skillshot.Unit.Distance(minion) > 200 ? 1300 : 1000);

                                    var skillshotToAdd = new Skillshot(
                                        skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                                        skillshot.Unit);
                                    DetectedSkillshots.Add(skillshotToAdd);
                                }
                            }
                            return;
                        }

                        if (skillshot.SpellData.SpellName == "AlZaharCalloftheVoid")
                        {
                            var start = skillshot.End - skillshot.Direction.Perpendicular() * 400;
                            var end = skillshot.End + skillshot.Direction.Perpendicular() * 400;
                            var skillshotToAdd = new Skillshot(
                                skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                                skillshot.Unit);
                            DetectedSkillshots.Add(skillshotToAdd);
                            return;
                        }

                        if (skillshot.SpellData.SpellName == "ZiggsQ")
                        {
                            var d1 = skillshot.Start.Distance(skillshot.End);
                            var d2 = d1 * 0.4f;
                            var d3 = d2 * 0.69f;


                            var bounce1SpellData = SpellDatabase.GetByName("ZiggsQBounce1");
                            var bounce2SpellData = SpellDatabase.GetByName("ZiggsQBounce2");

                            var bounce1Pos = skillshot.End + skillshot.Direction * d2;
                            var bounce2Pos = bounce1Pos + skillshot.Direction * d3;

                            bounce1SpellData.Delay =
                                (int)(skillshot.SpellData.Delay + d1 * 1000f / skillshot.SpellData.MissileSpeed + 500);
                            bounce2SpellData.Delay =
                                (int)(bounce1SpellData.Delay + d2 * 1000f / bounce1SpellData.MissileSpeed + 500);

                            var bounce1 = new Skillshot(
                                skillshot.DetectionType, bounce1SpellData, skillshot.StartTick, skillshot.End, bounce1Pos,
                                skillshot.Unit);
                            var bounce2 = new Skillshot(
                                skillshot.DetectionType, bounce2SpellData, skillshot.StartTick, bounce1Pos, bounce2Pos,
                                skillshot.Unit);

                            DetectedSkillshots.Add(bounce1);
                            DetectedSkillshots.Add(bounce2);
                        }

                        if (skillshot.SpellData.SpellName == "ZiggsR")
                        {
                            skillshot.SpellData.Delay =
                                (int)(1500 + 1500 * skillshot.End.Distance(skillshot.Start) / skillshot.SpellData.Range);
                        }

                        if (skillshot.SpellData.SpellName == "JarvanIVDragonStrike")
                        {
                            var endPos = new Vector2();

                            foreach (var s in DetectedSkillshots)
                            {
                                if (s.Unit.NetworkId == skillshot.Unit.NetworkId && s.SpellData.Slot == SpellSlot.E)
                                {
                                    endPos = s.End;
                                }
                            }

                            foreach (var m in ObjectManager.Get<Obj_AI_Minion>())
                            {
                                if (m.BaseSkinName == "jarvanivstandard" && m.Team == skillshot.Unit.Team &&
                                    skillshot.IsDanger(m.Position.To2D()))
                                {
                                    endPos = m.Position.To2D();
                                }
                            }

                            if (!endPos.IsValid())
                            {
                                return;
                            }

                            skillshot.End = endPos + 200 * (endPos - skillshot.Start).Normalized();
                            skillshot.Direction = (skillshot.End - skillshot.Start).Normalized();
                        }
                    }

                    if (skillshot.SpellData.SpellName == "OriannasQ")
                    {
                        var endCSpellData = SpellDatabase.GetByName("OriannaQend");

                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, endCSpellData, skillshot.StartTick, skillshot.Start, skillshot.End,
                            skillshot.Unit);

                        DetectedSkillshots.Add(skillshotToAdd);
                    }


                    //Dont allow fow detection.
                    if (skillshot.SpellData.DisableFowDetection && skillshot.DetectionType == DetectionType.RecvPacket)
                    {
                        return;
                    }
#if DEBUG
                    Console.WriteLine(Environment.TickCount + "Adding new skillshot: " + skillshot.SpellData.SpellName);
#endif

                    DetectedSkillshots.Add(skillshot);
                }
            }

        }
}
