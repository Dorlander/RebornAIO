using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using LeagueSharp.Common.Data;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace FioraProject
{
    using static Program;
    using static Combos;
    using static GetTargets;
    public static class OrbwalkLastClick
    {
        private static Vector2 LastClickPoint = new Vector2();
        public static void Init()
        {
            Game.OnUpdate +=Game_OnUpdate;
            Game.OnWndProc +=Game_OnWndProc;
            Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
        }

        private static void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (!OrbwalkLastClickActive)
                return;
            if (!sender.IsMe)
                return;
            if (args.Order != GameObjectOrder.MoveTo)
                return;
            if (!Orbwalking.CanMove(90) || Player.IsCastingInterruptableSpell())
                args.Process = false;
        }

        public static void OrbwalkLRCLK_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            if (e.GetNewValue<KeyBind>().Active)
            {
                LastClickPoint = Game.CursorPos.To2D();
            }
        }
        private static void Game_OnUpdate(EventArgs args)
        {
            if (!OrbwalkLastClickActive)
                return;
            Combo();
            var target = GetTarget();
            Orbwalking.Orbwalk(
                        Orbwalking.InAutoAttackRange(target) ? target : null,
                        LastClickPoint.IsValid() ? LastClickPoint.To3D() : Game.CursorPos,
                        80,
                        50, false, false,false);
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == (uint)WindowsMessages.WM_RBUTTONDOWN)
            {
                LastClickPoint = Game.CursorPos.To2D();
            }
        }
    }
}
