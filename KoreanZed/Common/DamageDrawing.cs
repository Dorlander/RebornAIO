namespace KoreanZed.Common
{
    using System;
    using System.Linq;

    using KoreanZed;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;

    class CommonDamageDrawing
    {
        public delegate float DrawDamageDelegate(Obj_AI_Hero hero);

        private const int Width = 103;

        private const int Height = 8;

        private readonly Render.Text killableText = new Render.Text(0, 0, "KILLABLE", 20, new ColorBGRA(255, 0, 0, 255));

        private DrawDamageDelegate amountOfDamage;

        public bool Active = true;

        private readonly ZedMenu zedMenu;

        public CommonDamageDrawing(ZedMenu zedMenu)
        {
            this.zedMenu = zedMenu;
        }

        public DrawDamageDelegate AmountOfDamage
        {
            get
            {
                return amountOfDamage;
            }

            set
            {
                if (amountOfDamage == null)
                {
                    Drawing.OnDraw += DrawDamage;
                }
                amountOfDamage = value;
            }
        }

        private bool Enabled()
        {
            return ((Active) && (amountOfDamage != null)
                    && ((zedMenu.GetParamBool("koreanzed.drawing.damageindicator"))
                        || zedMenu.GetParamBool("koreanzed.drawing.killableindicator")));
        }

        private void DrawDamage(EventArgs args)
        {
            Color color = Color.Gray;
            Color barColor = Color.White;

            if (zedMenu.GetParamStringList("koreanzed.drawing.damageindicatorcolor") == 1)
            {
                color = Color.Gold;
                barColor = Color.Olive;
            }
            else if (zedMenu.GetParamStringList("koreanzed.drawing.damageindicatorcolor") == 2)
            {
                color = Color.FromArgb(100, Color.Black);
                barColor = Color.Lime;
            }

            if (Enabled())
            {
                foreach (
                    Obj_AI_Hero champ in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(h => h.IsVisible && h.IsEnemy && h.IsValid && h.IsHPBarRendered))
                {
                    float damage = amountOfDamage(champ);

                    if (damage > 0)
                    {
                        Vector2 pos = champ.HPBarPosition;

                        if (zedMenu.GetParamBool("koreanzed.drawing.killableindicator")
                            && (damage > champ.Health + 50f))
                        {
                            Render.Circle.DrawCircle(champ.Position, 100, Color.Red);
                            Render.Circle.DrawCircle(champ.Position, 75, Color.Red);
                            Render.Circle.DrawCircle(champ.Position, 50, Color.Red);
                            killableText.X = (int)pos.X + 40;
                            killableText.Y = (int)pos.Y - 20;
                            killableText.OnEndScene();
                        }

                        if (zedMenu.GetParamBool("koreanzed.drawing.damageindicator"))
                        {
                            float healthAfterDamage = Math.Max(0, champ.Health - damage) / champ.MaxHealth;
                            float posY = pos.Y + 20f;
                            float posDamageX = pos.X + 12f + Width * healthAfterDamage;
                            float posCurrHealthX = pos.X + 12f + Width * champ.Health / champ.MaxHealth;

                            float diff = (posCurrHealthX - posDamageX) + 3;

                            float pos1 = pos.X + 8 + (107 * healthAfterDamage);

                            for (int i = 0; i < diff-3; i++)
                            {
                                Drawing.DrawLine(pos1 + i, posY, pos1 + i, posY + Height, 1, color);
                            }

                            Drawing.DrawLine(posDamageX, posY, posDamageX, posY + Height, 2, barColor);
                        }
                    }
                }
            }
        }
    }
}