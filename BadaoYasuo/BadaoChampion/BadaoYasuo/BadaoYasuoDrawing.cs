using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace BadaoKingdom.BadaoChampion.BadaoYasuo
{
    public static class BadaoYasuoDrawing
    {
        public static void BadaoActivate()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            // dive turret
            if(BadaoYasuoVariables.DrawDiveTurret.GetValue<bool>())
            {
                Drawing.DrawText(50,  100, Color.Lime, "Dive Turret" + "(" + (Keys)BadaoYasuoVariables.DiveTurretKey.GetValue<KeyBind>().Key + ")" + " : " 
                    + (BadaoYasuoVariables.DiveTurretKey.GetValue<KeyBind>().Active ? "On" : "Off"));
            }
            // autoQ
            if (BadaoYasuoVariables.DrawAutoQ.GetValue<bool>())
            {
                Drawing.DrawText(50,  115, Color.Lime, "Auto Q " + "(" + (Keys)BadaoYasuoVariables.AutoQ.GetValue<KeyBind>().Key + ")" + " : "
                    + (BadaoYasuoVariables.AutoQ.GetValue<KeyBind>().Active ? "On" : "Off"));
            }
            // E
            if (BadaoYasuoVariables.DrawE.GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, BadaoMainVariables.E.Range, Color.Lime);
            }
            // R
            if (BadaoYasuoVariables.DrawR.GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, BadaoMainVariables.R.Range, Color.Lime);
            }
            // Assassinate
            if (BadaoYasuoVariables.DrawAssasinate.GetValue<bool>())
            {
                Drawing.DrawText(50, 130, Color.Lime, "Assasinate " + "(" + (Keys)BadaoYasuoVariables.AssassinateKey.GetValue<KeyBind>().Key + ")" + " : "
                    + (BadaoYasuoVariables.AssassinateKey.GetValue<KeyBind>().Active ? "On" : "Off"));
                if (BadaoYasuoVariables.AssassinateKey.GetValue<KeyBind>().Active)
                {
                    var x = Drawing.WorldToScreen(ObjectManager.Player.Position);
                    var head = Drawing.WorldToScreen((ObjectManager.Player.Position.To2D() + new Vector2(0, 250)).To3D());
                    var selected = TargetSelector.GetSelectedTarget();
                    if (selected.BadaoIsValidTarget())
                    {
                        var y = Drawing.WorldToScreen(selected.Position);
                        Drawing.DrawLine(x, y, 2, Color.Pink);
                        Drawing.DrawText(head[0], head[1], Color.Yellow, "selected target is: " + selected.ChampionName);
                    }
                    else
                    {
                        Drawing.DrawText(head[0], head[1], Color.Yellow, "please select target");
                    }
                }
            }
            // Combomdoe
            if (BadaoYasuoVariables.DrawComboMode.GetValue<bool>())
            {
                Drawing.DrawText(50, 145, Color.Lime, "Combo Mode " + "(" + (Keys)BadaoYasuoVariables.TargetModeKey.GetValue<KeyBind>().Key + ")" + " : "
                    + (BadaoYasuoVariables.TargetMode.GetValue<StringList>().SelectedValue));
            }
        }
    }
}
