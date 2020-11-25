namespace KoreanCommon
{
    using System;

    using LeagueSharp;

    public class CommonEvolveUltimate
    {
        public CommonEvolveUltimate()
        {
            Obj_AI_Base.OnLevelUp += EvolveUltimate;
        }

        private static void EvolveUltimate(Obj_AI_Base sender, EventArgs args)
        {
            if (sender.IsMe)
            {
                sender.Spellbook.CanUseSpell(SpellSlot.R);
            }
        }
    }
}