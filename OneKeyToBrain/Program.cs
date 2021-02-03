using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.IO;
using System.Diagnostics;
using SharpDX;
using Collision = LeagueSharp.Common.Collision;
using System.Threading;

namespace OneKeyToBrain
{
    class Program
    {

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public static float QMANA;
        public static float WMANA;
        public static float EMANA;
        public static float RMANA;

        public static float potionProtect=0;
        public static Vector2 WayPoint;
        public static float WayPointTime;
        public static Vector3 positionWard;
        public static int timer;
        public static float JungleTime;
        public static Obj_AI_Hero jungler = null;
        private static Obj_AI_Hero WardTarget;
        private static float WardTime = 0;

        public static Items.Item WardS = new Items.Item(2043, 600f);
        public static Items.Item WardN = new Items.Item(2044, 600f);
        public static Items.Item TrinketN = new Items.Item(3340, 600f);

        public static Items.Item SightStone = new Items.Item(2049, 600f);
        public static Items.Item Potion = new Items.Item(2003, 0);
        public static Items.Item ManaPotion = new Items.Item(2004, 0);
        public static Items.Item Youmuu = new Items.Item(3142, 0);

        public static Menu Config;

        private static Obj_AI_Hero Player;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            Q = new Spell(SpellSlot.Q);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            W = new Spell(SpellSlot.W, 1500f);
            W.SetSkillshot(0.6f, 75f, 3300f, false, SkillshotType.SkillshotLine);


            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (IsJungler(enemy) && enemy.IsEnemy)
                {
                    jungler = enemy;
                    Game.PrintChat("OKTW Brain enemy jungler: " + enemy.SkinName);
                }
            }

            Config = new Menu("OKTW Brain", "OKTW Brain", true);
            Config.AddToMainMenu();

            Config.SubMenu("Iteams").AddItem(new MenuItem("pots", "Use pots").SetValue(true));
            Config.SubMenu("Iteams").AddItem(new MenuItem("buyPots", "Buy 1 hp pot").SetValue(false));
            Config.AddItem(new MenuItem("click", "Show enemy click").SetValue(true));
            Config.AddItem(new MenuItem("infoCombo", "Show info combo").SetValue(true));
            Config.SubMenu("Wards").AddItem(new MenuItem("ward", "Auto ward enemy in Grass").SetValue(false));
            Config.SubMenu("Wards").AddItem(new MenuItem("wardC", "Only Combo").SetValue(false));
            Config.SubMenu("GankTimer").AddItem(new MenuItem("timer", "GankTimer").SetValue(true));

            Config.SubMenu("Dev option").AddItem(new MenuItem("OnCreate", "OnCreate / OnDelete").SetValue(false));
            Config.SubMenu("Dev option").AddItem(new MenuItem("Prediction", "ShowPredictionInfo").SetValue(false));
            Config.SubMenu("Dev option").AddItem(new MenuItem("debug", "Debug").SetValue(false));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
                Config.SubMenu("Dev option").SubMenu("Good Player").AddItem(new MenuItem("GoodPlayer" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));

            Config.SubMenu("Combo Key").AddItem(new MenuItem("Combo", "Combo").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnDelete += Obj_AI_Base_OnDelete;
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
        }

        private static void Obj_AI_Base_OnCreate(GameObject obj, EventArgs args)
        {
            if (!Config.Item("OnCreate").GetValue<bool>())
                return;
            if (obj.IsValid)
            {
                debug("OnCreate: " + obj.Name);
            }
        }

        private static void Obj_AI_Base_OnDelete(GameObject obj, EventArgs args)
        {
            if (!Config.Item("OnCreate").GetValue<bool>())
                return;
            if (obj.IsValid)
            {

                debug("OnDelete: " + obj.Name);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            PotionMenager();
            if (Config.Item("ward").GetValue<bool>() || (Config.Item("ward").GetValue<bool>() && Config.Item("Combo").GetValue<KeyBind>().Active && Config.Item("wardC").GetValue<bool>()))
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(2000)))
                {
                    bool WallOfGrass = NavMesh.IsWallOfGrass(Prediction.GetPrediction(enemy, 0.3f).CastPosition, 0);
                    if (WallOfGrass)
                    {
                        positionWard = Prediction.GetPrediction(enemy, 0.3f).CastPosition;
                        WardTarget = enemy;
                        WardTime = Game.Time;
                    }
                }

                if (Player.Distance(positionWard) < 600 && !WardTarget.IsValidTarget() && Game.Time - WardTime < 5)
                {
                    WardTime = Game.Time - 6;
                    if (TrinketN.IsReady())
                        TrinketN.Cast(positionWard);
                    else if (SightStone.IsReady())
                        SightStone.Cast(positionWard);
                    else if (WardS.IsReady())
                        WardS.Cast(positionWard);
                    else if (WardN.IsReady())
                        WardN.Cast(positionWard);
                }
            }
            Obj_SpawnPoint allySpawn = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsAlly);
            if (ObjectManager.Player.Distance(allySpawn.Position) > 600 &&!Items.HasItem(Potion.Id) && Config.Item("buyPots").GetValue<bool>() && ObjectManager.Player.InFountain() && Game.Time - potionProtect >= 0.2)
            {
                
                
                Potion.Buy();
                potionProtect = Game.Time;
            }

        }
        public static void drawText(string msg, Obj_AI_Hero Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero.Position);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1], color, msg);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("click").GetValue<bool>() )
            {
                foreach (var champion in ObjectManager.Get<Obj_AI_Hero>().Where(champion => Config.Item("GoodPlayer" + champion.BaseSkinName).GetValue<bool>() && champion.IsVisible && champion.IsValid))
                {
                    List<Vector2> waypoints = champion.GetWaypoints();

                    if (WayPoint != waypoints.Last<Vector2>())
                    {
                        if (WayPointTime - Game.Time > -0.11)
                            Render.Circle.DrawCircle(champion.Position, 50, System.Drawing.Color.Cyan);

                        WayPointTime = Game.Time;

                    }
                    WayPoint = waypoints.Last<Vector2>();

                }
            }
            var tw = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
                if (Config.Item("click").GetValue<bool>() && tw.IsValidTarget())
                {
                    List<Vector2> waypoints = tw.GetWaypoints();
                    WayPoint = waypoints.Last<Vector2>();
                    Render.Circle.DrawCircle(waypoints.Last<Vector2>().To3D(), 50, System.Drawing.Color.Red);
                }
            
            if (Config.Item("Prediction").GetValue<bool>())
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(2000)))
                {
                    var poutput = W.GetPrediction(enemy);
                    drawText("HitChance: " + (int)poutput.Hitchance, enemy, System.Drawing.Color.GreenYellow);
                }

            }
            else if (Config.Item("infoCombo").GetValue<bool>())
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(2000)))
                {
                    string combo;
                    var hpCombo = Q.GetDamage(enemy) + W.GetDamage(enemy) + E.GetDamage(enemy);
                    var hpLeft = enemy.Health - Q.GetDamage(enemy) + W.GetDamage(enemy) + E.GetDamage(enemy) + R.GetDamage(enemy);
                    if (Q.GetDamage(enemy) > enemy.Health)
                        combo = "Q";
                    else if (Q.GetDamage(enemy) + W.GetDamage(enemy) > enemy.Health)
                        combo = "QW";
                    else if (Q.GetDamage(enemy) + W.GetDamage(enemy) + E.GetDamage(enemy) > enemy.Health)
                        combo = "QWE";
                    else if (Q.GetDamage(enemy) + W.GetDamage(enemy) + E.GetDamage(enemy) + R.GetDamage(enemy) > enemy.Health)
                        combo = "QWER";
                    else
                    {
                        if (Player.FlatPhysicalDamageMod > Player.FlatMagicDamageMod)
                            combo = "QWER+" + (int)(hpLeft / (Player.Crit * Player.GetAutoAttackDamage(enemy) + Player.GetAutoAttackDamage(enemy))) + " AA";
                        else
                            combo = "QWER+" + (int)(hpLeft / hpCombo) + "QWE";
                    }

                    
                    if (hpLeft > hpCombo)
                        drawText(combo, enemy, System.Drawing.Color.Red);
                    else if (hpLeft < 0)
                        drawText(combo, enemy, System.Drawing.Color.Red);
                    else if (hpLeft > 0)
                        drawText(combo, enemy, System.Drawing.Color.Yellow);
                }
            }

            if (Config.Item("timer").GetValue<bool>() && jungler != null)
            {
                if (jungler.IsDead)
                {
                    Obj_SpawnPoint enemySpawn = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy);
                    timer = (int)(enemySpawn.Position.Distance(ObjectManager.Player.Position) / 370);
                    drawText(" " + timer, ObjectManager.Player, System.Drawing.Color.Cyan);
                }
                else if (jungler.IsVisible)
                {
                    float Way = 0;
                    var JunglerPath = ObjectManager.Player.GetPath(jungler.Position);
                    var PointStart = ObjectManager.Player.Position;
                    foreach (var point in JunglerPath)
                    {
                        if (PointStart.Distance(point) > 0)
                        {
                            Way += PointStart.Distance(point);
                            PointStart = point;
                        }
                    }
                    timer = (int)(Way / jungler.MoveSpeed);
                    drawText(" " + timer, ObjectManager.Player, System.Drawing.Color.GreenYellow);
                }
                else
                {

                    if (timer > 0)
                        drawText(" " + timer, ObjectManager.Player, System.Drawing.Color.Orange);
                    else
                        drawText(" " + timer, ObjectManager.Player, System.Drawing.Color.Red);
                    if (Game.Time - JungleTime >= 1)
                    {
                        timer = timer - 1;
                        JungleTime = Game.Time;
                    }
                }
            }
        }

        private static bool IsJungler(Obj_AI_Hero hero)
        {
            return hero.Spellbook.Spells.Any(spell => spell.Name.ToLower().Contains("smite"));
        }

        public static void debug(string msg)
        {
            if (Config.Item("debug").GetValue<bool>())

                Game.PrintChat(msg);
        }
        public static void PotionMenager()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;
            if (!R.IsReady())
                RMANA = QMANA - ObjectManager.Player.Level * 2;
            else
                RMANA = R.Instance.ManaCost; ;
            if (Config.Item("pots").GetValue<bool>() && !ObjectManager.Player.InFountain() && !ObjectManager.Player.HasBuff("Recall"))
            {
                if (Potion.IsReady() && !ObjectManager.Player.HasBuffIn("RegenerationPotion",0.75f, true))
                {
                    if (ObjectManager.Player.CountEnemiesInRange(700) > 0 && ObjectManager.Player.Health + 200 < ObjectManager.Player.MaxHealth)
                        Potion.Cast();
                    else if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.6)
                        Potion.Cast();
                }
                if (ManaPotion.IsReady() && !ObjectManager.Player.HasBuffIn("FlaskOfCrystalWater", 0.75f, true))
                {
                    if (ObjectManager.Player.CountEnemiesInRange(1200) > 0 && ObjectManager.Player.Mana < RMANA)
                        ManaPotion.Cast();
                }
            }
        }

    }
}
