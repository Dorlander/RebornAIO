﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SPrediction;
using SharpDX;

namespace MidlaneSharp
{
    public partial class Orianna
    {
        private BallMgr Ball;
        private class BallMgr
        {          
            public enum Command
            {
                Attack = 0,
                Dissonance = 1,
                Protect = 2,
                Shockwave = 3,
            }

            private ConcurrentQueue<Tuple<Command, Obj_AI_Hero>> WorkQueue;
            private Vector3 _position;

            public Vector3 Position
            {
                get { return _position; }
                set
                {
                    if (_position != value)
                    {
                        BallMgr_OnPositionChanged(_position, value);
                        _position = value;
                    }
                }
            }
            public bool IsBallReady;

            public delegate void dOnProcessCommand(Command cmd, Obj_AI_Hero target);
            public event dOnProcessCommand OnProcessCommand;
            public BallMgr()
            {
                WorkQueue = new ConcurrentQueue<Tuple<Command, Obj_AI_Hero>>();
                Position = ObjectManager.Player.ServerPosition;
                Game.OnUpdate += Game_OnUpdate;
                Obj_AI_Hero.OnCreate += Obj_AI_Hero_OnCreate;
                Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            }

            public void Post(Command cmd, Obj_AI_Hero t)
            {
                WorkQueue.Enqueue(new Tuple<Command, Obj_AI_Hero>(cmd, t));
            }

            public void Process(int count = 1)
            {
                Tuple<Command, Obj_AI_Hero> cmd;
                for (int i = 0; i < count; i++)
                {
                    if (WorkQueue.TryDequeue(out cmd))
                        OnProcessCommand(cmd.Item1, cmd.Item2);
                }
            }

            public void ClearWorkQueue()
            {
                WorkQueue = new ConcurrentQueue<Tuple<Command, Obj_AI_Hero>>();
            }

            public bool CheckHeroCollision(Vector3 to)
            {
                if (Position == to)
                    return false;

                return SPrediction.Collision.CheckEnemyHeroCollision(Position.To2D(), to.To2D(), 130f, 0.25f);
            }

            private void Obj_AI_Hero_OnCreate(GameObject sender, EventArgs args)
            {
                if (sender.IsAlly && sender.Name.Contains("DoomBall"))
                    Position = sender.Position;
            }

            private void Game_OnUpdate(EventArgs args)
            {
                if (ObjectManager.Player.HasBuff("OrianaGhostSelf"))
                    Position = ObjectManager.Player.ServerPosition;
                else
                {
                    foreach (var ally in HeroManager.Allies)
                    {
                        if (ally.HasBuff("OrianaGhost"))
                        {
                            Position = ally.ServerPosition;
                            break;
                        }
                    }
                }

                if (IsBallReady)
                    Process();
            }

            private void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
            {
                if (sender.IsMe && (args.SData.Name == "OrianaIzunaCommand" || args.SData.Name == "OrianaRedactCommand"))
                    Position = Vector3.Zero;
            }

            private void BallMgr_OnPositionChanged(Vector3 oldVal, Vector3 newVal)
            {
                IsBallReady = newVal != Vector3.Zero;
                for(int i = 0; i < 4; i++)
                {
                    if (Program.Champion.Spells[i] != null)
                        Program.Champion.Spells[i].From = newVal;
                }
            }
        }
    }
}
