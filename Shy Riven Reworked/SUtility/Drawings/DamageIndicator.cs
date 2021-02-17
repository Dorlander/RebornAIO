using System;
using System.Globalization;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SUtility.Drawings
{
    public static class DamageIndicator
    {
        public delegate float dCalculateDamage(Obj_AI_Hero target);
        public static dCalculateDamage CalculateDamage;

        private const int XOffset = 10;
        private const int YOffset = 20;
        private const int Width = 103;
        private const int Height = 8;

        private static Menu s_Menu;
        private static readonly Render.Text Text = new Render.Text(0, 0, "", 11, new ColorBGRA(255, 0, 0, 255), "monospace");


        public static void Initialize(dCalculateDamage fn, Menu menuToAttach)
        {
            s_Menu = new Menu("Damage Indicator", "SUtility.Drawings.DamageIndicator.Root");
            s_Menu.AddItem(new MenuItem("SUtility.Drawings.DamageIndicator.Root.Fill", "Fill HP Bar").SetValue(new Circle(true, Color.Goldenrod)));
            s_Menu.AddItem(new MenuItem("SUtility.Drawings.DamageIndicator.Root.Enabled", "Enabled").SetValue(true));
            menuToAttach.AddSubMenu(s_Menu);
            CalculateDamage = fn;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if(Enabled)
            {
                foreach (var unit in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValid && h.IsHPBarRendered && h.IsEnemy))
                {
                    var barPos = unit.HPBarPosition;
                    var damage = CalculateDamage(unit);
                    var percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
                    var yPos = barPos.Y + YOffset;
                    var xPosDamage = barPos.X + XOffset + Width * percentHealthAfterDamage;
                    var xPosCurrentHp = barPos.X + XOffset + Width * unit.Health / unit.MaxHealth;

                    if (damage > unit.Health)
                    {
                        Text.X = (int)barPos.X + XOffset;
                        Text.Y = (int)barPos.Y + YOffset - 13;
                        Text.text = ((int)(unit.Health - damage)).ToString(CultureInfo.InvariantCulture);
                        Text.OnEndScene();
                    }

                    Drawing.DrawLine(xPosDamage, yPos, xPosDamage, yPos + Height, 2, Color.Lime);

                    if (FillHPBar)
                    {
                        float differenceInHp = xPosCurrentHp - xPosDamage;
                        var pos1 = barPos.X + 9 + (107 * percentHealthAfterDamage);

                        for (int i = 0; i < differenceInHp; i++)
                        {
                            Drawing.DrawLine(pos1 + i, yPos, pos1 + i, yPos + Height, 1, FillColor);
                        }
                    }
                }
            }
        }

        public static bool Enabled
        {
            get { return s_Menu.Item("SUtility.Drawings.DamageIndicator.Root.Enabled").GetValue<bool>(); }
            set { s_Menu.Item("SUtility.Drawings.DamageIndicator.Root.Enabled").SetValue(value); }
        }

        public static bool FillHPBar
        {
            get { return s_Menu.Item("SUtility.Drawings.DamageIndicator.Root.Fill").GetValue<Circle>().Active; }
            set { s_Menu.Item("SUtility.Drawings.DamageIndicator.Root.Enabled").SetValue(new Circle(value, FillColor)); }
        }

        public static Color FillColor
        {
            get { return s_Menu.Item("SUtility.Drawings.DamageIndicator.Root.Fill").GetValue<Circle>().Color; }
            set { s_Menu.Item("SUtility.Drawings.DamageIndicator.Root.Enabled").SetValue(new Circle(FillHPBar, value)); }
        }
    }
}