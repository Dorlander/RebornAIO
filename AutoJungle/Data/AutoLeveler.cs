using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoJungle.Data
{
    internal class AutoLeveler
    {
        public int[] LevelingOrder = new int[] { 0, 1, 2, 0, 0, 3, 0, 1, 0, 1, 3, 1, 1, 2, 2, 3, 2, 2 };
        public int lastLevel = -1;

        public AutoLeveler(int[] tree)
        {
            if (ObjectManager.Player.Level >= 18)
            {
                return;
            }
            LevelingOrder = tree;
        }

        public void LevelSpells()
        {
            try
            {
                var current = GetTotalPoints();
                if (lastLevel < current + Program.player.Level - GetTotalPoints())
                {
                    switch (LevelingOrder[current])
                    {
                        case 0:
                            Program.player.Spellbook.LevelUpSpell(SpellSlot.Q);
                            break;
                        case 1:
                            Program.player.Spellbook.LevelUpSpell(SpellSlot.W);
                            break;
                        case 2:
                            Program.player.Spellbook.LevelUpSpell(SpellSlot.E);
                            break;
                        case 3:
                            Program.player.Spellbook.LevelUpSpell(SpellSlot.R);
                            break;
                    }
                    lastLevel = current;
                }
            }
            catch (Exception) {}
        }

        private static int GetTotalPoints()
        {
            var spell = Program.player.Spellbook;
            var q = spell.GetSpell(SpellSlot.Q).Level;
            var w = spell.GetSpell(SpellSlot.W).Level;
            var e = spell.GetSpell(SpellSlot.E).Level;
            var r = spell.GetSpell(SpellSlot.R).Level;

            return q + w + e + r;
        }
    }
}