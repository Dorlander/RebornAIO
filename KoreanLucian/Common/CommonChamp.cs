namespace KoreanCommon
{
    using LeagueSharp;
    using LeagueSharp.Common;

    public abstract class CommonChampion
    {
        protected CommonChampion(string menuDisplay)
        {
            Player = ObjectManager.Player;

            MainMenu = new CommonMenu(menuDisplay, true);
            Orbwalker = MainMenu.Orbwalker;
            Spells = new CommonSpells(this);
            ForceUltimate = new CommonForceUltimate(this);
            DrawDamage = new CommonDamageDrawing(this);
            DrawDamage.AmountOfDamage = Spells.MaxComboDamage;
            DrawDamage.Active = true;
            commonEvolveUltimate = new CommonEvolveUltimate();
        }

        private CommonEvolveUltimate commonEvolveUltimate;

        protected CommonDamageDrawing DrawDamage { get; set; }

        protected CommonForceUltimate ForceUltimate { get; set; }

        public CommonSpells Spells { get; set; }

        public Orbwalking.Orbwalker Orbwalker { get; set; }

        public Obj_AI_Hero Player { get; set; }

        public CommonMenu MainMenu { get; set; }
    }
}