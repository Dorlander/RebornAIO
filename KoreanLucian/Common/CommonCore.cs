namespace KoreanCommon
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    public abstract class CommonCore
    {
        protected readonly CommonSpell E;

        protected readonly CommonSpell Q;

        protected readonly CommonSpell R;

        protected readonly CommonSpell RFlash;

        protected readonly CommonSpells spells;

        protected readonly CommonSpell W;

        public CommonCore(CommonChampion champion)
        {
            this.champion = champion;
            spells = champion.Spells;
            Q = champion.Spells.Q;
            W = champion.Spells.W;
            E = champion.Spells.E;
            R = champion.Spells.R;
            RFlash = champion.Spells.RFlash;

            Game.OnUpdate += UseSkills;
        }

        protected CommonChampion champion { get; set; }

        public abstract void LastHitMode();

        public abstract void HarasMode();

        public abstract void LaneClearMode();

        public abstract void ComboMode();

        public abstract void Ultimate(Obj_AI_Hero target);

        public void UseSkills(EventArgs args)
        {
            if (champion != null)
            {
                switch (champion.MainMenu.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.LastHit:
                        LastHitMode();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        HarasMode();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        LaneClearMode();
                        break;
                    case Orbwalking.OrbwalkingMode.Combo:
                        ComboMode();
                        break;
                }
            }
        }
    }
}