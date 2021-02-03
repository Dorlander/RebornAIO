namespace KoreanZed
{
    using System.Collections.Generic;

    using LeagueSharp;

    class ZedEnergyChecker
    {
        private readonly ZedMenu zedMenu;

        public bool ReadyToHaras
        {
            get
            {
                return ObjectManager.Player.ManaPercent
                       > zedMenu.GetParamSlider("koreanzed.harasmenu.saveenergy");
            }
        }

        public bool ReadyToLaneClear
        {
            get
            {
                return ObjectManager.Player.ManaPercent
                       > zedMenu.GetParamSlider("koreanzed.laneclearmenu.saveenergy");
            }
        }

        public bool ReadyToLastHit
        {
            get
            {
                return ObjectManager.Player.ManaPercent
                       > zedMenu.GetParamSlider("koreanzed.lasthitmenu.saveenergy");
            }
        }

        public ZedEnergyChecker(ZedMenu menu)
        {
            zedMenu = menu;
        }

        public bool CanHarass(List<ZedSpell> spellList)
        {
            float total = 0F;

            foreach (ZedSpell zedSpell in spellList)
            {
                total += zedSpell.ManaCost;
            }

            return ((((ObjectManager.Player.Mana - total) / ObjectManager.Player.MaxMana) * 100)
                    > zedMenu.GetParamSlider("koreanzed.harasmenu.saveenergy"));
        }

    }
}
