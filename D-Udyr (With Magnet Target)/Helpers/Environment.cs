using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace D_Udyr.Helpers
{
    public class Environment
    {
        public static Obj_AI_Hero player = ObjectManager.Player;

        public class Minion
        {
            public static int countMinionsInrange(Vector3 l, float p)
            {
                return
                    MinionManager.GetMinions(l, p, MinionTypes.All, MinionTeam.NotAlly)
                        .Count(i => !i.IsDead && i.IsEnemy && i.Distance(l) < p);
            }

            public static Vector3 bestVectorToAoeFarm(Vector3 center, float spellrange, float spellWidth, int hit = 0)
            {
                var minions = MinionManager.GetMinions(center, spellrange, MinionTypes.All, MinionTeam.NotAlly);
                Vector3 bestPos = new Vector3();
                int hits = hit;
                foreach (var minion in minions)
                {
                    if (countMinionsInrange(minion.Position, spellWidth) > hits)
                    {
                        bestPos = minion.Position;
                        hits = countMinionsInrange(minion.Position, spellWidth);
                    }
                    Vector3 newPos = new Vector3(minion.Position.X + 80, minion.Position.Y + 80, minion.Position.Z);
                    for (int i = 1; i < 4; i++)
                    {
                        var rotated = newPos.To2D().RotateAroundPoint(newPos.To2D(), 90 * i).To3D();
                        if (countMinionsInrange(rotated, spellWidth) > hits && player.Distance(rotated) <= spellrange)
                        {
                            bestPos = newPos;
                            hits = countMinionsInrange(rotated, spellWidth);
                        }
                    }
                }

                return bestPos;
            }

            public static bool KillableMinion(float range)
            {
                return
                    MinionManager.GetMinions(
                        player.Position, range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None)
                        .Any(
                            minion =>
                                HealthPrediction.GetHealthPrediction(minion, 3000) <=
                                Damage.GetAutoAttackDamage(player, minion, true));
            }
        }

        public class Hero
        {
            public static Obj_AI_Hero mostEnemyAtFriend(Obj_AI_Hero player,
                float spellRange,
                float spellWidth,
                int min = 0)
            {
                return
                    HeroManager.Allies.Where(
                        i => !i.IsDead && i.CountEnemiesInRange(spellWidth) >= min && i.Distance(player) < spellRange)
                        .OrderBy(i => i.IsMe)
                        .ThenByDescending(i => i.CountEnemiesInRange(spellWidth))
                        .FirstOrDefault();
            }

            public static Vector3 bestVectorToAoeSpell(IEnumerable<Obj_AI_Hero> heroes,
                float spellrange,
                float spellwidth)
            {
                Vector3 bestPos = new Vector3();
                int hits = 0;
                foreach (var hero in heroes)
                {
                    if (hero.Position.CountEnemiesInRange(spellwidth) > hits)
                    {
                        bestPos = hero.Position;
                        hits = hero.Position.CountEnemiesInRange(spellwidth);
                    }
                    Vector3 newPos = new Vector3(hero.Position.X + 80, hero.Position.Y + 80, hero.Position.Z);
                    for (int i = 1; i < 4; i++)
                    {
                        var rotated = newPos.To2D().RotateAroundPoint(newPos.To2D(), 90 * i).To3D();
                        if (rotated.CountEnemiesInRange(spellwidth) > hits && player.Distance(rotated) <= spellrange)
                        {
                            bestPos = newPos;
                            hits = rotated.CountEnemiesInRange(spellwidth);
                        }
                    }
                }

                return bestPos;
            }

            public static float GetAdOverTime(Obj_AI_Hero source, Obj_AI_Hero target, int times)
            {
                double basicDmg = 0;
                int attacks = (int)Math.Floor(source.AttackSpeedMod * times);
                for (int i = 0; i < attacks; i++)
                {
                    if (source.Crit > 0)
                    {
                        basicDmg += source.GetAutoAttackDamage(target, true) - source.GetAutoAttackDamage(target) +
                                    source.GetAutoAttackDamage(target) * (1 + source.Crit / attacks);
                    }
                    else
                    {
                        basicDmg += source.GetAutoAttackDamage(target, true);
                    }
                }
                return (float)basicDmg;
            }

            public static int getSpellDelay(Spell spell, Vector3 pos)
            {
                return (int)(spell.Delay * 1000 + player.Distance(pos) / spell.Speed);
            }

            public static int GetPriority(string championName)
            {
                string[] p1 =
                {
                    "Alistar", "Amumu", "Bard", "Blitzcrank", "Braum", "Cho'Gath", "Dr. Mundo", "Garen",
                    "Gnar", "Hecarim", "Janna", "Jarvan IV", "Leona", "Lulu", "Malphite", "Nami", "Nasus", "Nautilus",
                    "Nunu", "Olaf", "Rammus", "Renekton", "Sejuani", "Shen", "Shyvana", "Singed", "Sion", "Skarner",
                    "Sona", "Soraka", "Taric", "TahmKench", "Thresh", "Volibear", "Warwick", "MonkeyKing", "Yorick",
                    "Zac", "Zyra"
                };

                string[] p2 =
                {
                    "Aatrox", "Darius", "Elise", "Evelynn", "Galio", "Gangplank", "Gragas", "Irelia", "Jax",
                    "Lee Sin", "Maokai", "Morgana", "Nocturne", "Pantheon", "Poppy", "Rengar", "Rumble", "Ryze", "Swain",
                    "Trundle", "Tryndamere", "Udyr", "Urgot", "Vi", "XinZhao", "RekSai"
                };

                string[] p3 =
                {
                    "Akali", "Diana", "Ekko", "Fiddlesticks", "Fiora", "Fizz", "Heimerdinger", "Jayce",
                    "Kassadin", "Kayle", "Kha'Zix", "Lissandra", "Mordekaiser", "Nidalee", "Riven", "Shaco", "Vladimir",
                    "Yasuo", "Zilean"
                };

                string[] p4 =
                {
                    "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki",
                    "Draven", "Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw",
                    "Kindred", "Leblanc", "Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn",
                    "Sivir", "Syndra", "Talon", "Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar",
                    "Velkoz", "Viktor", "Xerath", "Zed", "Ziggs"
                };

                if (p1.Contains(championName))
                {
                    return 1;
                }
                if (p2.Contains(championName))
                {
                    return 2;
                }
                if (p3.Contains(championName))
                {
                    return 3;
                }
                return p4.Contains(championName) ? 4 : 1;
            }
        }

        public class Turret
        {
            public static int countTurretsInRange(Obj_AI_Hero l)
            {
                return ObjectManager.Get<Obj_AI_Turret>().Count(i => !i.IsDead && i.IsEnemy && l.Distance(i) < 750f);
            }
        }

        public class Map
        {
            public static bool CheckWalls(Vector3 player, Vector3 enemy)
            {
                var distance = player.Distance(enemy);
                for (int i = 1; i < 6; i++)
                {
                    if (player.Extend(enemy, distance + 55 * i).IsWall())
                    {
                        return true;
                    }
                }
                return false;
            }

            public static Vector3 ClosestWall(Vector3 StartPos, Vector3 EndPos)
            {
                var distance = StartPos.Distance(EndPos);
                for (int i = 1; i < 8; i++)
                {
                    if (StartPos.Extend(EndPos, distance + 55 * i).IsWall())
                    {
                        return StartPos.Extend(EndPos, distance + 55 * i);
                    }
                }
                return EndPos;
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
        }
    }
}