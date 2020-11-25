using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace xSaliceResurrected.Managers
{
    static class ManaManager
    {
        public static void AddManaManagertoMenu(Menu myMenu, String source, int standard)
        {
            myMenu.AddItem(new MenuItem(source + "_Manamanager", "Mana Manager", true).SetValue(new Slider(standard)));
        }

        public static bool FullManaCast()
        {
            if (ObjectManager.Player.Mana >= SpellManager.QSpell.ManaCost + SpellManager.WSpell.ManaCost + SpellManager.ESpell.ManaCost + SpellManager.RSpell.ManaCost)
                return true;
            return false;
        }

        public static bool HasMana(string source)
        {
            if (ObjectManager.Player.ManaPercent > Champion.menu.Item(source + "_Manamanager", true).GetValue<Slider>().Value)
                return true;
            return false;
        }
    }
}
