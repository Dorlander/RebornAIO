namespace KoreanLucian
{
    using KoreanCommon;

    internal class Lucian : CommonChampion
    {
        public readonly Core core;

        private readonly Drawing drawing;

        public readonly KillSteal killSteal;

        public readonly SemiAutomaticE semiAutomaticE;

        public Lucian()
            : base("Korean Lucian")
        {
            KoreanLucian.Spells.Load(this);
            core = new Core(this);
            CustomMenu.Load(this);
            ForceUltimate.ForceUltimate = core.Ultimate;
            drawing = new Drawing(this);
            killSteal = new KillSteal(this);
            DrawDamage.AmountOfDamage = KoreanLucian.Spells.MaxComboDamage;
            semiAutomaticE = new SemiAutomaticE(this);
            ExtendedQ.Load(this);
        }
    }
}