using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using Collision = LeagueSharp.Common.Collision;

// using Beaving.s.Baseult;

namespace RandomUlt.Helpers
{
    internal class LastPositions
    {
        public static List<Positions> Enemies;
        private static Menu configMenu;
        public bool enabled = true;
        public static Spell R;
        private float LastUltTime;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public Vector3 SpawnPos;

        public static List<string> SupportedHeroes =
            new List<string>(new string[] { "Ezreal", "Jinx", "Ashe", "Draven", "Gangplank", "Ziggs", "Lux", "Xerath" });

        public List<Vector3> ShielderTurretsOrder =
            new List<Vector3>(
                new Vector3[] { new Vector3(6919.155f, 1483.599f, 43.32f), new Vector3(1512.892f, 6699.57f, 42.06392f) });

        public List<Vector3> ShielderTurretsChaos =
            new List<Vector3>(
                new Vector3[] { new Vector3(7943.15f, 13411.8f, 38), new Vector3(13327.4f, 8226.28f, 38), });

        public static List<string> BaseUltHeroes = new List<string>(new string[] { "Ezreal", "Jinx", "Ashe", "Draven" });

        public static int[] xerathUltRange = new[] { 3200, 4400, 5600, };
        public bool xerathUltActivated;
        public Obj_AI_Hero xerathUltTarget;

        public LastPositions(Menu config)
        {
            configMenu = config;
            R = new Spell(SpellSlot.R);
            if (player.ChampionName == "Ezreal")
            {
                R.SetSkillshot(1.2f, 160f, 2000f, false, SkillshotType.SkillshotLine);
            }
            if (player.ChampionName == "Jinx")
            {
                R.SetSkillshot(0.6f, 140f, 1700f, true, SkillshotType.SkillshotLine);
            }
            if (player.ChampionName == "Ashe")
            {
                R.SetSkillshot(0.25f, 130f, 1600f, true, SkillshotType.SkillshotLine);
            }
            if (player.ChampionName == "Draven")
            {
                R.SetSkillshot(0.4f, 160f, 2000f, true, SkillshotType.SkillshotLine);
            }
            if (player.ChampionName == "Lux")
            {
                R.SetSkillshot(0.5f, 190f, float.MaxValue, false, SkillshotType.SkillshotLine);
            }
            if (player.ChampionName == "Ziggs")
            {
                R.SetSkillshot(0.1f, 525f, 1750f, false, SkillshotType.SkillshotCircle);
            }
            if (player.ChampionName == "Gangplank")
            {
                R.SetSkillshot(0.1f, 600f, R.Speed, false, SkillshotType.SkillshotCircle);
            }
            if (player.ChampionName == "Xerath")
            {
                R.SetSkillshot(0.7f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            }
            SpawnPos = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy).Position;
            if (SupportedChamps())
            {
                config.AddItem(new MenuItem("UseR", "Use R")).SetValue(true);
                if (player.ChampionName == "Gangplank")
                {
                    config.AddItem(new MenuItem("gpWaves", "GP ult waves to damage")).SetValue(new Slider(2, 1, 7));
                }
                if (player.ChampionName == "Xerath")
                {
                    config.AddItem(new MenuItem("XerathUlts", "Xerath ults to damage")).SetValue(new Slider(2, 1, 3));
                }
                if (player.ChampionName == "Draven")
                {
                    config.AddItem(new MenuItem("Backdamage", "Count second hit")).SetValue(true);
                    config.AddItem(new MenuItem("CallBack", "Reduce time between hits")).SetValue(true);
                }
                config.AddItem(new MenuItem("Hitchance", "Hitchance")).SetValue(new Slider(2, 1, 5));
                Menu DontUlt = new Menu("Don't Ult", "DontUltRandomUlt");
                foreach (var e in HeroManager.Enemies)
                {
                    DontUlt.AddItem(new MenuItem(e.ChampionName + "DontUltRandomUlt", e.ChampionName)).SetValue(false);
                }
                config.AddSubMenu(DontUlt);
                config.AddItem(new MenuItem("Alliesrange", "Allies min range from the target"))
                    .SetValue(new Slider(1500, 500, 2000));
                config.AddItem(new MenuItem("EnemiesAroundYou", "Block if enemies around you"))
                    .SetValue(new Slider(600, 0, 2000));
                config.AddItem(new MenuItem("waitBeforeUlt", "Wait time before ults(ms)"))
                    .SetValue(new Slider(600, 0, 3000));
                config.AddItem(new MenuItem("BaseUltFirst", "BaseUlt has higher priority")).SetValue(false);
                config.AddItem(new MenuItem("Collision", "Calc damage reduction")).SetValue(true);
                config.AddItem(new MenuItem("drawNotification", "Draw notification")).SetValue(true);
            }
            config.AddItem(new MenuItem("RandomUltDrawings", "Draw possible place")).SetValue(false);
            Menu orbBlock = new Menu("Block keys", "BlockKeys");
            orbBlock.AddItem(new MenuItem("OrbBlock1", "Disabled by keypress"))
                .SetValue(new KeyBind(65, KeyBindType.Press));
            orbBlock.AddItem(new MenuItem("OrbBlock2", "Disabled by keypress"))
                .SetValue(new KeyBind(88, KeyBindType.Press));
            orbBlock.AddItem(new MenuItem("OrbBlock3", "Disabled by keypress"))
                .SetValue(new KeyBind(67, KeyBindType.Press));
            orbBlock.AddItem(new MenuItem("ComboBlock", "Disabled by Combo"))
                .SetValue(new KeyBind(32, KeyBindType.Press));
            orbBlock.AddItem(new MenuItem("OnlyCombo", "Only Combo key")).SetValue(true);
            config.AddSubMenu(orbBlock);
            Enemies = HeroManager.Enemies.Select(x => new Positions(x)).ToList();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnTeleport += Obj_AI_Base_OnTeleport;
        }


        private void Obj_AI_Base_OnTeleport(Obj_AI_Base sender, GameObjectTeleportEventArgs args)
        {
            var unit = sender as Obj_AI_Hero;

            if (unit == null || !unit.IsValid || unit.IsAlly)
            {
                return;
            }

            var recall = Packet.S2C.Teleport.Decoded(unit, args);
            Enemies.Find(x => x.Player.NetworkId == recall.UnitNetworkId).RecallData.Update(recall);
        }

        private bool SupportedChamps()
        {
            return SupportedHeroes.Any(h => h.Contains(player.ChampionName));
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!configMenu.Item("RandomUltDrawings").GetValue<bool>() || !enabled ||
                configMenu.Item("ComboBlock").GetValue<KeyBind>().Active)
            {
                return;
            }
            foreach (var enemy in
                Enemies.Where(
                    x =>
                        x.Player.IsValid<Obj_AI_Hero>() && !x.Player.IsDead &&
                        x.RecallData.Recall.Status == Packet.S2C.Teleport.Status.Start &&
                        x.RecallData.Recall.Type == Packet.S2C.Teleport.Type.Recall)
                    .OrderBy(x => x.RecallData.GetRecallTime()))
            {
                var trueDist = Math.Abs(enemy.LastSeen - enemy.RecallData.RecallStartTime) / 1000 *
                               enemy.Player.MoveSpeed;
                var dist = (Math.Abs(enemy.LastSeen - enemy.RecallData.RecallStartTime) / 1000 * enemy.Player.MoveSpeed) -
                           enemy.Player.MoveSpeed / 3;
                if (dist > 1500)
                {
                    continue;
                }

                if (dist < 50)
                {
                    dist = 50;
                }
                var line = getpos(enemy, trueDist);
                Vector3 pos = line;
                if (enemy.Player.IsVisible)
                {
                    pos = enemy.Player.Position;
                }
                else if (line.Distance(enemy.Player.Position) < dist &&
                         (enemy.predictedpos.UnderTurret(true) ||
                          NavMesh.GetCollisionFlags(enemy.predictedpos).HasFlag(CollisionFlags.Grass)))
                {
                    pos = enemy.predictedpos;
                }
                else
                {
                    pos =
                        PointsAroundTheTarget(enemy.Player.Position, trueDist)
                            .Where(
                                p =>
                                    !p.IsWall() && line.Distance(p) < dist / 1.5f &&
                                    GetPath(enemy.Player, p) < trueDist)
                            .OrderByDescending(p => NavMesh.GetCollisionFlags(p).HasFlag(CollisionFlags.Grass))
                            .ThenBy(p => line.Distance(p))
                            .FirstOrDefault();
                }
                if (pos.IsValid())
                {
                    Render.Circle.DrawCircle(pos, 50, Color.Red, 8);
                }
                if (!enemy.Player.IsVisible)
                {
                    if (pos.IsValid())
                    {
                        Render.Circle.DrawCircle(line, dist / 1.5f, Color.LawnGreen, 7);
                    }
                }
            }
            if (SupportedChamps() && configMenu.Item("drawNotification").GetValue<bool>() && R.IsReady() &&
                !player.IsDead)
            {
                var possibleTargets =
                    Enemies.Where(
                        x =>
                            !x.Player.IsDead && checkdmg(x.Player, x.Player.Position) &&
                            (Environment.TickCount - x.LastSeen < 4000) && x.Player.CountAlliesInRange(1000) < 1 &&
                            UltTime(x.Player.Position) <
                            9500 - configMenu.Item("waitBeforeUlt").GetValue<Slider>().Value);
                if (possibleTargets.Any() && player.IsHPBarRendered)
                {
                    Drawing.DrawText(
                        player.HPBarPosition.X + 8, player.HPBarPosition.Y - 30, Color.Red, "Possible Randomult");
                }
            }
        }

        private Vector3 getpos(Positions enemy, float dist)
        {
            var time = (enemy.LastSeen - enemy.RecallData.RecallStartTime) / 1000;
            var line = enemy.Player.Position.Extend(enemy.predictedpos, dist);
            if (enemy.Player.Position.Distance(enemy.predictedpos) < dist &&
                ((time < 2 ||
                  enemy.Player.Position.Distance(enemy.predictedpos) > enemy.Player.Position.Distance(line) * 0.70f)))
            {
                line = enemy.predictedpos;
            }
            return line;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            float time = System.Environment.TickCount;
            foreach (Positions enemyInfo in
                Enemies.Where(x => x.Player.IsVisible && !x.Player.IsDead && x.Player.IsValidTarget()))
            {
                enemyInfo.LastSeen = time;
                var prediction = Prediction.GetPrediction(enemyInfo.Player, 10);
                if (prediction != null)
                {
                    enemyInfo.predictedpos = prediction.UnitPosition;
                }
            }
            if (xerathUltActivated && R.IsReady() && !configMenu.Item("ComboBlock").GetValue<KeyBind>().Active &&
                player.HasBuff("xerathrshots"))
            {
                var enemy =
                    Enemies.Where(x => x.Player.NetworkId == xerathUltTarget.NetworkId && !x.Player.IsDead)
                        .FirstOrDefault();
                if (enemy != null)
                {
                    R.Cast(enemy.Player.Position);
                }
                else
                {
                    var target =
                        HeroManager.Enemies.Where(h => player.Distance(h) < xerathUltRange[R.Level - 1] && h.IsVisible)
                            .OrderBy(h => h.Health)
                            .FirstOrDefault();
                    if (target != null)
                    {
                        R.Cast(target);
                    }
                }
            }
            if (!SupportedChamps() || !configMenu.Item("UseR").GetValue<bool>() || !R.IsReady() || !enabled ||
                configMenu.Item("ComboBlock").GetValue<KeyBind>().Active)
            {
                return;
            }
            if (player.CountEnemiesInRange(configMenu.Item("EnemiesAroundYou").GetValue<Slider>().Value) >= 1)
            {
                return;
            }
            if (!configMenu.Item("OnlyCombo").GetValue<bool>() &&
                (configMenu.Item("OrbBlock1").GetValue<KeyBind>().Active ||
                 configMenu.Item("OrbBlock2").GetValue<KeyBind>().Active ||
                 configMenu.Item("OrbBlock3").GetValue<KeyBind>().Active))
            {
                return;
            }
            if (player.ChampionName == "Draven" && player.Spellbook.GetSpell(SpellSlot.R).Name != "DravenRCast")
            {
                return;
            }
            var HitChance = configMenu.Item("Hitchance").GetValue<Slider>().Value;
            foreach (Positions enemy in
                Enemies.Where(
                    x =>
                        x.Player.IsValid<Obj_AI_Hero>() && !x.Player.IsDead &&
                        !configMenu.Item(x.Player.ChampionName + "DontUltRandomUlt").GetValue<bool>() &&
                        x.RecallData.Recall.Status == Packet.S2C.Teleport.Status.Start &&
                        x.RecallData.Recall.Type == Packet.S2C.Teleport.Type.Recall)
                    .OrderBy(x => x.RecallData.GetRecallTime()))
            {
                if (CheckBuffs(enemy.Player) || CheckBaseUlt(enemy.RecallData.GetRecallCountdown()) ||
                    !(Environment.TickCount - enemy.RecallData.RecallStartTime >
                      configMenu.Item("waitBeforeUlt").GetValue<Slider>().Value))
                {
                    continue;
                }
                var dist = (Math.Abs(enemy.LastSeen - enemy.RecallData.RecallStartTime) / 1000 * enemy.Player.MoveSpeed) -
                           enemy.Player.MoveSpeed / 3;
                var trueDist = Math.Abs(enemy.LastSeen - enemy.RecallData.RecallStartTime) / 1000 *
                               enemy.Player.MoveSpeed;
                var line = getpos(enemy, dist);
                switch (HitChance)
                {
                    case 1:
                        break;
                    case 2:
                        if (trueDist > 1000 && !enemy.Player.IsVisible)
                        {
                            continue;
                        }
                        break;
                    case 3:
                        if (trueDist > 850 && !enemy.Player.IsVisible)
                        {
                            continue;
                        }
                        break;
                    case 4:
                        if (trueDist > 700 && !enemy.Player.IsVisible)
                        {
                            continue;
                        }
                        break;
                    case 5:
                        if (trueDist > 500 && !enemy.Player.IsVisible)
                        {
                            continue;
                        }
                        break;
                }
                Vector3 pos = line;
                if (enemy.Player.IsVisible)
                {
                    pos = enemy.Player.Position;
                }
                else if (line.Distance(enemy.Player.Position) < dist &&
                         (enemy.predictedpos.UnderTurret(true) ||
                          NavMesh.GetCollisionFlags(enemy.predictedpos).HasFlag(CollisionFlags.Grass)))
                {
                    pos = enemy.predictedpos;
                }
                {
                    if (dist > 1500)
                    {
                        continue;
                    }
                    pos =
                        PointsAroundTheTarget(enemy.Player.Position, trueDist)
                            .Where(
                                p =>
                                    !p.IsWall() && line.Distance(p) < dist / 1.2f && GetPath(enemy.Player, p) < trueDist)
                            .OrderByDescending(p => NavMesh.GetCollisionFlags(p).HasFlag(CollisionFlags.Grass))
                            .ThenBy(p => line.Distance(p))
                            .FirstOrDefault();
                }
                if (pos != null)
                {
                    if (player.ChampionName == "Ziggs" && player.Distance(pos) > 5000f)
                    {
                        continue;
                    }
                    if (player.ChampionName == "Lux" && player.Distance(pos) > 3000f)
                    {
                        continue;
                    }
                    if (player.ChampionName == "Xerath" && player.Distance(pos) > xerathUltRange[R.Level - 1] - 500)
                    {
                        continue;
                    }
                    kill(enemy, new Vector3(pos.X, pos.Y, 0));
                }
            }
        }

        private bool CheckBaseUlt(float recallCooldown)
        {
            if (configMenu.Item("BaseUltFirst").GetValue<bool>() &&
                BaseUltHeroes.Any(h => h.Contains(player.ChampionName)) && recallCooldown > UltTime(SpawnPos))
            {
                return true;
            }
            return false;
        }

        private bool CheckBuffs(Obj_AI_Hero enemy)
        {
            if (enemy.ChampionName == "Anivia")
            {
                if (enemy.HasBuff("rebirthcooldown"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            if (enemy.ChampionName == "Aatrox")
            {
                if (enemy.HasBuff("aatroxpassiveready"))
                {
                    return true;
                }
            }
            return false;
        }

        public static float GetPath(Obj_AI_Hero hero, Vector3 b)
        {
            var path = hero.GetPath(b);
            var lastPoint = path[0];
            var distance = 0f;
            foreach (var point in path.Where(point => !point.Equals(lastPoint)))
            {
                distance += lastPoint.Distance(point);
                lastPoint = point;
            }
            return distance;
        }

        public static Vector3 GetPointAfterTimeFromPath(Obj_AI_Hero hero, Vector3 b, float timeInSec)
        {
            var path = hero.GetPath(b);
            var lastPoint = path[0];
            var distance = 0f;
            var maxDist = hero.MoveSpeed * timeInSec;
            foreach (var point in path.Where(point => !point.Equals(lastPoint)))
            {
                if (distance > maxDist)
                {
                    break;
                }
                distance += lastPoint.Distance(point);
                lastPoint = point;
            }
            return lastPoint;
        }

        private bool CheckShieldTower(Vector3 pos)
        {
            if (Game.MapId != GameMapId.SummonersRift)
            {
                return false;
            }
            if (player.Team == GameObjectTeam.Chaos)
            {
                return ShielderTurretsOrder.Any(s => s.Distance(pos) < 1150f);
            }
            else if (player.Team == GameObjectTeam.Order)
            {
                return ShielderTurretsChaos.Any(s => s.Distance(pos) < 1150f);
            }
            else
            {
                return false;
            }
        }

        private void kill(Positions positions, Vector3 pos)
        {
            if (R.IsReady() && pos.Distance(positions.Player.Position) < 1200 &&
                ObjectManager.Get<Obj_AI_Hero>()
                    .Count(o => o.IsAlly && o.Distance(pos) < configMenu.Item("Alliesrange").GetValue<Slider>().Value) <
                1)
            {
                if (checkdmg(positions.Player, pos) && UltTime(pos) < positions.RecallData.GetRecallTime() &&
                    !isColliding(pos))
                {
                    if (player.ChampionName == "Xerath")
                    {
                        xerathUlt(positions, pos);
                    }
                    R.Cast(pos);
                    if (player.ChampionName == "Draven" && configMenu.Item("CallBack").GetValue<bool>())
                    {
                        Utility.DelayAction.Add((int) (UltTime(pos) - 300), () => R.Cast());
                    }
                }
            }
        }

        private void xerathUlt(Positions positions, Vector3 pos = default(Vector3))
        {
            if (pos != Vector3.Zero)
            {
                xerathUltActivated = true;
                xerathUltTarget = positions.Player;
                Utility.DelayAction.Add(5000, () => xerathUltActivated = false);
                R.Cast(pos);
            }
            else
            {
                if (positions.Player.IsVisible)
                {
                    xerathUltActivated = true;
                    xerathUltTarget = positions.Player;
                    Utility.DelayAction.Add(5000, () => xerathUltActivated = false);
                    R.Cast(positions.Player);
                }
                else
                {
                    xerathUltActivated = true;
                    xerathUltTarget = positions.Player;
                    Utility.DelayAction.Add(5000, () => xerathUltActivated = false);
                    R.Cast(
                        positions.Player.Position.Extend(
                            positions.predictedpos, (float) (positions.Player.MoveSpeed * 0.3)));
                }
            }
        }

        private bool isColliding(Vector3 pos)
        {
            if (player.ChampionName == "Draven" && player.ChampionName == "Ashe" && player.ChampionName == "Jinx")
            {
                var input = new PredictionInput { Radius = R.Width, Unit = player, };

                input.CollisionObjects[0] = CollisionableObjects.Heroes;

                return Collision.GetCollision(new List<Vector3> { pos }, input).Any();
            }
            return false;
        }

        private float UltTime(Vector3 pos)
        {
            var dist = player.ServerPosition.Distance(pos);
            if (player.ChampionName == "Ezreal")
            {
                return (dist / 2000) * 1000 + 1000;
            }
            //Beaving's calculations
            if (player.ChampionName == "Jinx" && dist > 1350)
            {
                const float accelerationrate = 0.3f;

                var acceldifference = dist - 1350f;

                if (acceldifference > 150f)
                {
                    acceldifference = 150f;
                }

                var difference = dist - 1500f;
                return (dist /
                        ((1350f * 1700f + acceldifference * (1700f + accelerationrate * acceldifference) +
                          difference * 1700f) / dist)) * 1000 + 250;
            }
            if (player.ChampionName == "Ashe")
            {
                return (dist / 1600) * 1000 + 250;
            }
            if (player.ChampionName == "Draven")
            {
                return (dist / 2000) * 1000 + 400;
            }
            if (player.ChampionName == "Ziggs")
            {
                return (dist / 1750f) * 1000 + 1000;
            }
            if (player.ChampionName == "Lux")
            {
                return 500f;
            }
            if (player.ChampionName == "Xerath")
            {
                return 500f;
            }
            return 0;
        }

        public static List<Vector3> PointsAroundTheTarget(Vector3 pos, float dist, float prec = 15, float prec2 = 5)
        {
            if (!pos.IsValid())
            {
                return new List<Vector3>();
            }
            List<Vector3> list = new List<Vector3>();
            if (dist > 500)
            {
                prec = 20;
                prec2 = 6;
            }
            if (dist > 805)
            {
                prec = 35;
                prec2 = 8;
            }
            var angle = 360 / prec * Math.PI / 180.0f;
            var step = dist / prec2;
            for (int i = 0; i < prec; i++)
            {
                for (int j = 0; j < prec2; j++)
                {
                    list.Add(
                        new Vector3(
                            pos.X + (float) (Math.Cos(angle * i) * (j * step)),
                            pos.Y + (float) (Math.Sin(angle * i) * (j * step)), pos.Z));
                }
            }

            return list;
        }

        private bool checkdmg(Obj_AI_Hero target, Vector3 pos)
        {
            var dmg = R.GetDamage(target);
            float bonuShieldNearTowers = 0f;
            var collision = configMenu.Item("Collision").GetValue<bool>();
            if (CheckShieldTower(pos))
            {
                bonuShieldNearTowers = 300f;
            }
            if (player.ChampionName == "Ezreal")
            {
                if (dmg * (collision ? 0.7f : 1f) - 10 - bonuShieldNearTowers > target.Health)
                {
                    return true;
                }
            }
            if (player.ChampionName == "Draven")
            {
                if (configMenu.Item("Backdamage").GetValue<bool>())
                {
                    dmg = dmg * 2;
                }
                if (dmg * (collision ? 0.8f : 1f) - 10 - bonuShieldNearTowers > target.Health)
                {
                    return true;
                }
            }
            if (player.ChampionName == "Jinx")
            {
                if (R.GetDamage(target, 1) - 10 - bonuShieldNearTowers > target.Health)
                {
                    return true;
                }
            }
            if (player.ChampionName == "Gangplank")
            {
                if (configMenu.Item("gpWaves").GetValue<Slider>().Value * dmg - bonuShieldNearTowers > target.Health)
                {
                    return true;
                }
            }
            if (player.ChampionName == "Xerath")
            {
                if (configMenu.Item("XerathUlts").GetValue<Slider>().Value * dmg - bonuShieldNearTowers > target.Health)
                {
                    return true;
                }
            }
            if (player.ChampionName == "Ashe" || player.ChampionName == "Lux" || player.ChampionName == "Ziggs")
            {
                if (dmg - 10 - bonuShieldNearTowers > target.Health)
                {
                    return true;
                }
            }
            return false;
        }
    }

    internal class Positions
    {
        public Obj_AI_Hero Player;
        public float LastSeen;
        public Vector3 predictedpos;

        public RecallData RecallData;

        public Positions(Obj_AI_Hero player)
        {
            Player = player;
            RecallData = new RecallData(this);
        }
    }

    internal class RecallData
    {
        public Positions Positions;
        public Packet.S2C.Teleport.Struct Recall;
        public Packet.S2C.Teleport.Struct Aborted;
        public float AbortTime;
        public float RecallStartTime;
        public bool started;
        public int FADEOUT_TIME = 3000;

        public RecallData(Positions positions)
        {
            Positions = positions;
            Recall = new Packet.S2C.Teleport.Struct(
                Positions.Player.NetworkId, Packet.S2C.Teleport.Status.Unknown, Packet.S2C.Teleport.Type.Unknown, 0);
        }

        public float GetRecallTime()
        {
            float time = System.Environment.TickCount;
            float countdown = 0;

            if (time - AbortTime < 2000)
            {
                countdown = Aborted.Duration - (AbortTime - Aborted.Start);
            }
            else if (AbortTime > 0)
            {
                countdown = 0;
            }
            else
            {
                countdown = Recall.Start + Recall.Duration - time;
            }

            return countdown < 0 ? 0 : countdown;
        }

        public float GetRecallCountdown()
        {
            float time = Environment.TickCount;
            float countdown = 0;

            if (time - AbortTime < FADEOUT_TIME)
            {
                countdown = Aborted.Duration - (AbortTime - Aborted.Start);
            }
            else if (AbortTime > 0)
            {
                countdown = 0;
            }
            else
            {
                countdown = Recall.Start + Recall.Duration - time;
            }

            return countdown < 0 ? 0 : countdown;
        }

        public Positions Update(Packet.S2C.Teleport.Struct newData)
        {
            if (newData.Type == Packet.S2C.Teleport.Type.Recall && newData.Status == Packet.S2C.Teleport.Status.Abort)
            {
                Aborted = Recall;
                AbortTime = System.Environment.TickCount;
                started = false;
            }
            else
            {
                AbortTime = 0;
            }
            if (newData.Type == Packet.S2C.Teleport.Type.Recall && newData.Status == Packet.S2C.Teleport.Status.Finish)
            {
                started = false;
            }
            if (newData.Type == Packet.S2C.Teleport.Type.Recall && newData.Status == Packet.S2C.Teleport.Status.Start)
            {
                if (!started)
                {
                    RecallStartTime = System.Environment.TickCount;
                }
                started = true;
            }
            Recall = newData;
            return Positions;
        }
    }
}