using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace YasuoSharpV2
{
    class SmoothMouse
    {

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

        public static bool doMouse = true;

        /// <summary>
        /// Not sure if we're just supposed to create our own point class.
        /// </summary>
        struct Point
        {
            public int x;
            public int y;
        }

        struct MouseAction
        {
            public bool click;
            public bool back;
            public Vector3 pos;
            public List<Vector2> jumps;

            public MouseAction(Vector3 p, bool c,bool b)
            {
                click = c;
                pos = p;
                back = b;
                jumps = new List<Vector2>();
            }
        }

        private static Queue<MouseAction> queuePos = new Queue<MouseAction>();

        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        private static Vector2 difOfMouse;

        private static int lastAdd = Environment.TickCount;


        private static void doMouseClick()
        {
            Point pos = new Point();
            GetCursorPos(ref pos);
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, pos.x, pos.y, 0, 0);
        }

        private static void doMouseClick(long x, long y)
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, x, y, 0, 0);
        }

        private static Vector2 getMousePos()
        {
            Point posScreen = new Point();
            if (ScreenToClient(Process.GetCurrentProcess().MainWindowHandle, ref posScreen))
            {
                Point pos = new Point();
                if (GetCursorPos(ref pos))
                {
                    var vec = new Vector2(pos.x + posScreen.x, pos.y + posScreen.y);
                    difOfMouse = vec - Drawing.WorldToScreen(Game.CursorPos);
                    return vec;
                }
            }
            return new Vector2();
        }


        private static void MoveMouse(Vector2 pos)
        {
            Point posScreen = new Point();
            if (ScreenToClient(Process.GetCurrentProcess().MainWindowHandle, ref posScreen))
            {
                SetCursorPos((int)(pos.X - posScreen.x), (int)(pos.Y - posScreen.y));
            }
        }

        public static void addMouseEvent(Vector3 pos, bool click = false, bool back = true)
        {
            if (lastAdd + 200 < Environment.TickCount)
            {
                Console.WriteLine("new ouse action");
                lastAdd = Environment.TickCount;
                //if (queuePos.First().back)
                 //   queuePos.Dequeue();

                queuePos.Enqueue(new MouseAction(pos, click, false));
                if (back)
                    queuePos.Enqueue(new MouseAction(Game.CursorPos, click, back));
            }
        }


        public static void start()
        {
            new Thread(() =>
            {
                while (YasuoSharp.Config.Item("streamMouse").GetValue<bool>())
                {
                    if (queuePos.Count > 0)
                    {
                        var posNow = getMousePos();
                        var first = queuePos.First();
                        bool back = first.back;
                        if (!first.pos.IsOnScreen())
                        {
                            queuePos.Dequeue();
                        }
                        else
                        {
                            var firstOnScreen = first.pos.toScreen();
                            if (firstOnScreen.Distance(posNow, true) <= ((!back)?35*35 : 300*300))
                            {
                                MoveMouse(firstOnScreen);
                                if (first.click)
                                    doMouseClick();
                                queuePos.Dequeue();
                            }
                            else
                            {

                                var moveTo = posNow.Extend(firstOnScreen, 25);
                                if (first.jumps.Count > 1 && first.jumps[first.jumps.Count - 2].Distance(moveTo,true) < 15*15)
                                {
                                    queuePos.Dequeue();
                                }
                                else
                                {
                                    first.jumps.Add(moveTo);
                                    MoveMouse(moveTo);
                                }
                            }
                        }
                    }
                    Thread.Sleep(1000 / 50);
                }
            }).Start();
        }
    }


    public static class easierShit
    {
        public static Vector2 toScreen(this Vector3 pos)
        {
            return Drawing.WorldToScreen(pos);
        }
    }
}
