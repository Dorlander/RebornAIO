namespace KoreanZed
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    class ZedSpells
    {
        public ZedSpell Q { get; set; }
        public ZedSpell W { get; set; }
        public ZedSpell E { get; set; }
        public ZedSpell R { get; set; }

        public ZedSpells()
        {
            Q = new ZedSpell(SpellSlot.Q, 925F, TargetSelector.DamageType.Physical);
            Q.SetSkillshot(0.25F, 50F, 1600F, false, SkillshotType.SkillshotLine);

            float wRange = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange;
            W = new ZedSpell(SpellSlot.W, wRange, TargetSelector.DamageType.Physical);
            W.SetSkillshot(0.75F, 75F, 1000F, false, SkillshotType.SkillshotLine);

            float eRange = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange - 10;
            E = new ZedSpell(SpellSlot.E, eRange, TargetSelector.DamageType.Physical);

            float rRange = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange;
            R = new ZedSpell(SpellSlot.R, rRange, TargetSelector.DamageType.Physical);
        }
    }
}
