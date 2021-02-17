// Copyright 2014 - 2014 Esk0r
// Program.cs is part of Evade.
// 
// Evade is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Evade is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Evade. If not, see <http://www.gnu.org/licenses/>.

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using GamePath = System.Collections.Generic.List<SharpDX.Vector2>;

#endregion

namespace Evade
{
    internal class Program
    {
        public static SpellList<Skillshot> DetectedSkillshots = new SpellList<Skillshot>();

        private static bool _evading;

        private static Vector2 _evadePoint;

        public static bool NoSolutionFound = false;

        public static Vector2 EvadeToPoint = new Vector2();

        public static int LastWardJumpAttempt = 0;

        public static Vector2 PreviousTickPosition = new Vector2();
        public static Vector2 PlayerPosition = new Vector2();

        public static string PlayerChampionName;

        private static readonly Random RandomN = new Random();
        private static int LastSentMovePacketT = 0;
        private static int LastSentMovePacketT2 = 0;

        private static int LastSMovePacketT = 0;

        private static bool ForcePathFollowing = false;
        public static bool Evading
        {
            get { return _evading; } //
            set
            {
                if (value == true)
                {
                    ForcePathFollowing = true;
                    LastSentMovePacketT = 0;
                    ObjectManager.Player.SendMovePacket(EvadePoint);
                }

                _evading = value;
            }
        }

        public static Vector2 EvadePoint
        {
            get { return _evadePoint; }
            set
            {
                _evadePoint = value;
            }
        }

        
        public static void Init()
        {
            PlayerChampionName = ObjectManager.Player.ChampionName;

            //Create the menu to allow the user to change the config.
            Config.CreateMenu();
            
            //Add the game events.
            Game.OnUpdate += Game_OnOnGameUpdate;
            //Obj_AI_Hero.OnIssueOrder += ObjAiHeroOnOnIssueOrder;
            //Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            //Set up the OnDetectSkillshot Event.
            SkillshotDetector.OnDetectSkillshot += OnDetectSkillshot;
            SkillshotDetector.OnDeleteMissile += SkillshotDetectorOnOnDeleteMissile;

            //Ondash event.
            //CustomEvents.Unit.OnDash += UnitOnOnDash;

            DetectedSkillshots.OnAdd += DetectedSkillshots_OnAdd;

            //Initialze the collision
            Collision.Init();
        }
        private static void DetectedSkillshots_OnAdd(object sender, EventArgs e)
        {
            Evading = false;
        }

        private static void SkillshotDetectorOnOnDeleteMissile(Skillshot skillshot, MissileClient missile)
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
                            DetectionType.ProcessSpell, spellData, Utils.TickCount, missile.Position.To2D(),
                            missile.Position.To2D() + i * direction * spellData.Range, skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                    }
                }
            }
        }

        private static void OnDetectSkillshot(Skillshot skillshot)
        {
            //Check if the skillshot is already added.
            var alreadyAdded = false;

            if (Config.Menu.Item("DisableFow").GetValue<bool>() && !skillshot.Unit.IsVisible)
            {
                return;
            }

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
            if (skillshot.Unit.Team == ObjectManager.Player.Team && !Config.TestOnAllies)
            {
                return;
            }

            //Check if the skillshot is too far away.
            //if (skillshot.Start.Distance(PlayerPosition) >
            //    (skillshot.SpellData.Range + skillshot.SpellData.Radius + 1000) * 1.5)
            //{
            //    return;
            //}
            //Add the skillshot to the detected skillshot list.
            if (!alreadyAdded || skillshot.SpellData.DontCheckForDuplicates)
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

                    if (skillshot.SpellData.SpellName == "SionR")
                    {
                        skillshot.SpellData.MissileSpeed = (int)skillshot.Unit.MoveSpeed;
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

                    if (skillshot.SpellData.SpellName == "TaricE" && (skillshot.Unit as Obj_AI_Hero).ChampionName == "Taric")
                    {
                        var target = HeroManager.AllHeroes.FirstOrDefault(h => h.Team == skillshot.Unit.Team && h.IsVisible && h.HasBuff("taricwleashactive"));
                        if (target != null)
                        {
                            var start = target.ServerPosition.To2D();
                            var direction = (skillshot.OriginalEnd - start).Normalized();
                            var end = start + direction * skillshot.SpellData.Range;
                            var skillshotToAdd = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick,
                                    start, end, target)
                            {
                                OriginalEnd = skillshot.OriginalEnd
                            };
                            DetectedSkillshots.Add(skillshotToAdd);
                        }
                    }

                    if (skillshot.SpellData.SpellName == "SyndraE" || skillshot.SpellData.SpellName == "syndrae5")
                    {
                        var angle = 60;
                        var edge1 =
                            (skillshot.End - skillshot.Unit.ServerPosition.To2D()).Rotated(
                                -angle / 2 * (float)Math.PI / 180);
                        var edge2 = edge1.Rotated(angle * (float)Math.PI / 180);

                        var positions = new List<Vector2>();

                        var explodingQ = DetectedSkillshots.FirstOrDefault(s => s.SpellData.SpellName == "SyndraQ");

                        if (explodingQ != null)
                        {
                            positions.Add(explodingQ.End);
                        }

                        foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
                        {
                            if (minion.Name == "Seed" && !minion.IsDead && (minion.Team != ObjectManager.Player.Team || Config.TestOnAllies))
                            {
                                positions.Add(minion.ServerPosition.To2D());
                            }
                        }

                        foreach (var position in positions)
                        {
                            var v = position - skillshot.Unit.ServerPosition.To2D();
                            if (edge1.CrossProduct(v) > 0 && v.CrossProduct(edge2) > 0 &&
                                position.Distance(skillshot.Unit) < 800)
                            {
                                var start = position;
                                var end = skillshot.Unit.ServerPosition.To2D()
                                    .Extend(
                                        position,
                                        skillshot.Unit.Distance(position) > 200 ? 1300 : 1000);

                                var startTime = skillshot.StartTick;

                                startTime += (int)(150 + skillshot.Unit.Distance(position) / 2.5f);
                                var skillshotToAdd = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, startTime, start, end,
                                    skillshot.Unit);
                                DetectedSkillshots.Add(skillshotToAdd);
                            }
                        }
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "MalzaharQ")
                    {
                        var start = skillshot.End - skillshot.Direction.Perpendicular() * 400;
                        var end = skillshot.End + skillshot.Direction.Perpendicular() * 400;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "ZyraQ")
                    {
                        var start = skillshot.End - skillshot.Direction.Perpendicular() * 450;
                        var end = skillshot.End + skillshot.Direction.Perpendicular() * 450;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "DianaArc")
                    {
                        var skillshotToAdd = new Skillshot(
                        skillshot.DetectionType, SpellDatabase.GetByName("DianaArcArc"), skillshot.StartTick, skillshot.Start, skillshot.End,
                        skillshot.Unit);

                        DetectedSkillshots.Add(skillshotToAdd);
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
                                var extendedE = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start,
                                    skillshot.End + skillshot.Direction * 100, skillshot.Unit);
                                if (!extendedE.IsSafe(s.End))
                                {
                                    endPos = s.End;
                                }
                                break;
                            }
                        }

                        foreach (var m in ObjectManager.Get<Obj_AI_Minion>())
                        {
                            if (m.BaseSkinName == "jarvanivstandard" && m.Team == skillshot.Unit.Team)
                            {

                                var extendedE = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start,
                                    skillshot.End + skillshot.Direction * 100, skillshot.Unit);
                                if (!extendedE.IsSafe(m.Position.To2D()))
                                {
                                    endPos = m.Position.To2D();
                                }
                                break;
                            }
                        }

                        if (endPos.IsValid())
                        {
                            skillshot = new Skillshot(DetectionType.ProcessSpell, SpellDatabase.GetByName("JarvanIVEQ"), Utils.TickCount, skillshot.Start, endPos, skillshot.Unit);
                            skillshot.End = endPos + 200 * (endPos - skillshot.Start).Normalized();
                            skillshot.Direction = (skillshot.End - skillshot.Start).Normalized();
                        }
                    }
                }

                //if (skillshot.SpellData.SpellName == "OriannasQ")
                //{
                //    var skillshotToAdd = new Skillshot(
                //        skillshot.DetectionType, SpellDatabase.GetByName("OriannaQend"), skillshot.StartTick, skillshot.Start, skillshot.End,
                //        skillshot.Unit);

                //    DetectedSkillshots.Add(skillshotToAdd);
                //}


                ////Dont allow fow detection.
                //if (skillshot.SpellData.DisableFowDetection && skillshot.DetectionType == DetectionType.RecvPacket)
                //{
                //    return;
                //}
                DetectedSkillshots.Add(skillshot);
            }
        }

        private static void Game_OnOnGameUpdate(EventArgs args)
        {
            
            //Remove the detected skillshots that have expired.
            DetectedSkillshots.RemoveAll(skillshot => !skillshot.IsActive());

            //Trigger OnGameUpdate on each skillshot.
            foreach (var skillshot in DetectedSkillshots)
            {
                skillshot.Game_OnGameUpdate();
            }

        }


        /// <summary>
        /// Returns true if the point is not inside the detected skillshots.
        /// </summary>
        public static IsSafeResult IsSafe(Vector2 point)
        {
            var result = new IsSafeResult();
            result.SkillshotList = new List<Skillshot>();

            foreach (var skillshot in DetectedSkillshots)
            {
                    result.SkillshotList.Add(skillshot);
            }

            result.IsSafe = (result.SkillshotList.Count == 0);

            return result;
        }


        /// <summary>
        /// Returns true if some detected skillshot is about to hit the unit.
        /// </summary>
        public static bool IsAboutToHit(Obj_AI_Base unit, int time)
        {
            time += 150;
            foreach (var skillshot in DetectedSkillshots)
            {
                if (skillshot.IsAboutToHit(time, unit))
                {
                    return true;
                }
            }
            return false;
        }


        public struct IsSafeResult
        {
            public bool IsSafe;
            public List<Skillshot> SkillshotList;
        }
    }
}
