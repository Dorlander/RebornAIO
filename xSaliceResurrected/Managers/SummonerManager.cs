using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace xSaliceResurrected.Managers
{
    class SummonerManager
    {
        private static readonly SpellSlot IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
        private static readonly SpellSlot FlashSlot = ObjectManager.Player.GetSpellSlot("SummonerFlash");
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        public static bool Ignite_Ready()
        {
            return IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready;
        }

        public static bool Flash_Ready()
        {
            return FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready;
        }

        public static void UseFlash(Vector3 pos)
        {
            Player.Spellbook.CastSpell(FlashSlot, pos);
        }

    }
}
