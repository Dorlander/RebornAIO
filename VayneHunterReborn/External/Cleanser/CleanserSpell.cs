using System;
using LeagueSharp;

namespace VayneHunter_Reborn.External.Cleanser
{
    class CleanserSpell
    {
        public String ChampName { get; set; }

        public String SpellName { get; set; }

        public String RealName { get; set; }

        public String SpellBuff { get; set; }

        public bool IsEnabled { get; set; }

        public bool OnlyKill { get; set; }

        public SpellSlot Slot { get; set; }

        public float Delay { get; set; }
    }
}
