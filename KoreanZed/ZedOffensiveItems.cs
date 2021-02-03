namespace KoreanZed
{
    using LeagueSharp;
    using LeagueSharp.Common;

    using ItemData = LeagueSharp.Common.Data.ItemData;

    class ZedOffensiveItems
    {
        private readonly ZedMenu zedMenu;

        public ZedOffensiveItems(ZedMenu menu)
        {
            zedMenu = menu;
        }

        public void UseItems(Obj_AI_Hero target)
        {
            UseYomuusBlade();
           
            UseBOTRK(target);
            UseHydra();
            UseTiamat();
        }

        public void UseItemsLaneClear()
        {
            UseHydraLaneClear();
            UseTiamatLaneClear();
        }

        public void UseHarasItems()
        {
            UseHydraHaras();
            UseTiamatHaras();
        }

       

        private void UseBOTRK(Obj_AI_Hero target)
        {
            if (zedMenu.GetParamBool("koreanzed.combomenu.items.botrk")
                && ItemData.Blade_of_The_Ruined_King.GetItem().IsInRange(target)
                && ItemData.Blade_of_The_Ruined_King.GetItem().IsReady())
            {
                ItemData.Blade_of_The_Ruined_King.GetItem().Cast(target);
            }
        }

        private void UseYomuusBlade()
        {
            if (zedMenu.GetParamBool("koreanzed.combomenu.items.yomuus")
                && ItemData.Youmuus_Ghostblade.GetItem().IsReady())
            {
                ItemData.Youmuus_Ghostblade.GetItem().Cast();
            }
        }

        private void UseHydra()
        {
            if (zedMenu.GetParamBool("koreanzed.combomenu.items.hydra")
                && ItemData.Ravenous_Hydra.GetItem().IsReady())
            {
                ItemData.Ravenous_Hydra.GetItem().Cast();
            }
        }

        private void UseTiamat()
        {
            if (zedMenu.GetParamBool("koreanzed.combomenu.items.tiamat")
                && ItemData.Tiamat.GetItem().IsReady())
            {
                ItemData.Tiamat.GetItem().Cast();
            }
        }

        private void UseHydraLaneClear()
        {
            if (zedMenu.GetParamBool("koreanzed.laneclearmenu.items.hydra")
                && ItemData.Ravenous_Hydra.GetItem().IsReady())
            {
                ItemData.Ravenous_Hydra.GetItem().Cast();
            }
        }

        private void UseTiamatLaneClear()
        {
            if (zedMenu.GetParamBool("koreanzed.laneclearmenu.items.tiamat")
                && ItemData.Tiamat.GetItem().IsReady())
            {
                ItemData.Tiamat.GetItem().Cast();
            }
        }

        private void UseHydraHaras()
        {
            if (zedMenu.GetParamBool("koreanzed.harasmenu.items.hydra")
                && ItemData.Ravenous_Hydra.GetItem().IsReady())
            {
                ItemData.Ravenous_Hydra.GetItem().Cast();
            }
        }

        private void UseTiamatHaras()
        {
            if (zedMenu.GetParamBool("koreanzed.harasmenu.items.tiamat")
                && ItemData.Tiamat.GetItem().IsReady())
            {
                ItemData.Tiamat.GetItem().Cast();
            }
        }
    }
}
