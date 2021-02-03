namespace KoreanZed
{
    using LeagueSharp;
    using LeagueSharp.Common;

    public class ZedSpell : Spell
    {
        public bool UseOnCombo { get; set; }

        public bool UseOnHarass { get; set; }

        public bool UseOnLastHit { get; set; }

        public bool UseOnLaneClear { get; set; }

        public ZedSpell(SpellSlot slot, float range, TargetSelector.DamageType damageType)
            : base (slot, range, damageType)
        {
            UseOnCombo = false;
            UseOnHarass = false;
            UseOnLastHit = false;
            UseOnLaneClear = false;
        }
    }
}
