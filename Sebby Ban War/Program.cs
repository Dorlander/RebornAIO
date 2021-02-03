using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace Sebby_Ban_War
{
    class Program
    {
        static void Main(string[] args) { CustomEvents.Game.OnGameLoad += Game_OnGameLoad; }

        public static Font Tahoma13;
        public static Menu Config;
        public static int LastMouseTime = Utils.TickCount;
        public static Vector2 LastMousePos = Game.CursorPos.To2D();
        public static int NewPathTime = Utils.TickCount;
        public static int LastType = 0; // 0 Move , 1 Attack, 2 Cast spell
        public static bool LastUserClickTime = false;
        public static int PathPerSecInfo;
        public static int PacketCast = Utils.TickCount;

        private static void Game_OnGameLoad(EventArgs args)
        {
            Tahoma13 = new Font(Drawing.Direct3DDevice, new FontDescription
            { FaceName = "Tahoma", Height = 14, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });

            Config = new Menu("SBW - Sebby Ban War", "SBW - Sebby Ban War", true);
            Config.AddToMainMenu();
            Config.AddItem(new MenuItem("enable", "ENABLE").SetValue(true));
            Config.AddItem(new MenuItem("ClickTime", "Minimum Click Time (100)").SetValue(new Slider(100, 300, 0))).SetTooltip("0 - 100 scripter, 100 - 200 pro player, 200+ normal player");
            Config.AddItem(new MenuItem("showCPS", "Show action per sec").SetValue(true));

            Config.SubMenu("Advance").AddItem(new MenuItem("blockOut", "Block targeted action out screen").SetValue(true));
            Config.SubMenu("Advance").AddItem(new MenuItem("cut", "CUT SKILLSHOTS").SetValue(true));
            Config.SubMenu("Advance").AddItem(new MenuItem("skill", "BLOCK inhuman skill cast").SetValue(true));
            Obj_AI_Base.OnNewPath += Obj_AI_Base_OnNewPath;
            Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Game.OnWndProc += Game_OnWndProc;
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
           // Game.OnSendPacket += Game_OnSendPacket;
        }

      /*  private static void Game_OnSendPacket(GamePacketEventArgs args)
        {
            if(args.GetPacketId() == 270)
            {
                PathPerSecCounter++;
            }
        }*/

        public static int PathPerSecCounter = 0;
        public static int PathTimer = Utils.TickCount;

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Utils.TickCount - PathTimer > 1000)
            {
                PathPerSecInfo = PathPerSecCounter;
                PathTimer = Utils.TickCount;
                PathPerSecCounter = 0;
            }
        }

        private static void Obj_AI_Base_OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (!sender.IsMe)
                return;

            PathPerSecCounter++;
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == 123)
            {
                LastUserClickTime = true;
            }
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            
            if (!Config.Item("enable").GetValue<bool>())
                return;
            
            var spellPosition = args.EndPosition;
            if (args.Target != null)
            {
                if (args.Target.IsMe)
                    return;

                if (Config.Item("blockOut").GetValue<bool>() && !Render.OnScreen(Drawing.WorldToScreen(args.Target.Position)))
                {
                    Console.WriteLine("BLOCK SPELL OUT SCREEN");
                    args.Process = false;
                    return;
                }
                spellPosition = args.Target.Position;
            }
            // IGNORE TARGETED SPELLS
            if (spellPosition.IsZero)
                return;

            if (args.Slot != SpellSlot.Q && args.Slot != SpellSlot.W && args.Slot != SpellSlot.E && args.Slot != SpellSlot.R)
                return;

            var spell = ObjectManager.Player.Spellbook.Spells.FirstOrDefault(x => x.Slot == args.Slot);

            // LINE CUT SPELL RANGE
            if (Config.Item("cut").GetValue<bool>() && spell != null && spell.SData.LineWidth != 0 && spellPosition.Distance(args.StartPosition) > 700)
            {
                Random rnd = new Random();
                ObjectManager.Player.Spellbook.CastSpell(args.Slot, args.StartPosition.Extend(spellPosition, rnd.Next(400, 600)));
                Console.WriteLine("CUT SPELL");
                args.Process = false;
                return;
            }
            
            var screenPos = Drawing.WorldToScreen(spellPosition);    
            if (Config.Item("skill").GetValue<bool>() && Utils.TickCount - LastMouseTime < LastMousePos.Distance(screenPos) / 20)
            {
                Console.WriteLine("BLOCK SPELL");
                args.Process = false;
                return;
            }

            LastMouseTime = Utils.TickCount;
            LastMousePos = screenPos;
        }

        private static void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (!Config.Item("enable").GetValue<bool>())
                return;

            var screenPos = Drawing.WorldToScreen(args.TargetPosition);
            var mouseDis = LastMousePos.Distance(screenPos);
            if (LastUserClickTime)
            {
                LastUserClickTime = false;
                return;
            }

            if (args.Order == GameObjectOrder.AttackUnit && args.Target is Obj_AI_Minion && LastType == 0 && Utils.TickCount - LastMouseTime > mouseDis / 15)
            {
                //Console.WriteLine("SBW farm protection");
                LastType = 1;
                LastMouseTime = Utils.TickCount;
                LastMousePos = screenPos;
                return;
            }
          
            //Console.WriteLine(args.Order);
            if (Utils.TickCount - LastMouseTime < Config.Item("ClickTime").GetValue<Slider>().Value + (mouseDis / 15))
            {
                //Console.WriteLine("BLOCK " + args.Order);
                args.Process = false;
                return;
            }

            //Console.WriteLine("DIS " + LastMousePos.Distance(screenPos) + " TIME " + (Utils.TickCount - LastMouseTime));
            if (args.Order == GameObjectOrder.AttackUnit)
            {
                if (Config.Item("blockOut").GetValue<bool>() && !Render.OnScreen(screenPos))
                {
                    args.Process = false;
                    Console.WriteLine("SBW BLOCK AA OUT SCREEN");
                }
                if (args.Target is Obj_AI_Minion && LastType == 0)
                {
                    LastType = 1;
                    return;
                }
                LastType = 1;
            }
            else
                LastType = 0;

            LastMouseTime = Utils.TickCount;
            LastMousePos = screenPos;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("showCPS").GetValue<bool>())
            {
                var h = Drawing.Height * 0.7f;
                var w = Drawing.Width * 0.15f;
                var color = Color.Yellow;
                if (PathPerSecInfo < 5)
                    color = Color.GreenYellow;
                else if (PathPerSecInfo > 8)
                    color = Color.OrangeRed;

                DrawFontTextScreen(Tahoma13, "SBW Server action per sec: " + PathPerSecInfo, h, w, color);
            }
        }

        public static void DrawFontTextScreen(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }

        public static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
        }
    }
}
