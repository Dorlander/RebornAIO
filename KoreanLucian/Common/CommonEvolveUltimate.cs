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
<<<<<<< HEAD
                sender.Spellbook.CanUseSpell(SpellSlot.R);
=======
                sender.Spellbook.CastSpell(SpellSlot.R);
>>>>>>> ec4686f6e389b77e9ccdee06b6c6e31e8a0e9431
            }
        }
    }
}