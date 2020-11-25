namespace KoreanLucian
{
    using System;
    using System.Drawing;

    using KoreanCommon;

    using LeagueSharp.Common;

    internal class Drawing
    {
        public Drawing(CommonChampion champion)
        {
            this.champion = champion;
            spells = champion.Spells;

            LeagueSharp.Drawing.OnDraw += DrawRanges;
        }

        private CommonChampion champion { get; set; }

        private CommonSpells spells { get; set; }

        private void DrawRanges(EventArgs args)
        {
            if (!KoreanUtils.GetParamBool(champion.MainMenu, "drawskillranges"))
            {
                return;
            }

            Render.Circle.DrawCircle(
                champion.Player.Position,
                spells.MaxRangeCombo,
                Color.FromArgb(150, Color.DarkGreen),
                10);
        }
    }
}