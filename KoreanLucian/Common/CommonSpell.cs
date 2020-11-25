namespace KoreanCommon
{
    using LeagueSharp;
    using LeagueSharp.Common;

    public class CommonSpell : Spell
    {
        public CommonSpell(
            SpellSlot slot,
            float range = 0,
            TargetSelector.DamageType damageType = TargetSelector.DamageType.Physical)
            : base(slot, range, damageType)
        {
        }

        public MenuItem UseOnComboMenu { get; set; }

        public MenuItem UseOnHarasMenu { get; set; }

        public MenuItem UseOnLaneClearMenu { get; set; }

        public bool UseOnCombo
        {
            get
            {
                bool b;

                if (UseOnComboMenu != null)
                {
                    b = UseOnComboMenu.GetValue<bool>();
                }
                else
                {
                    b = false;
                }
                return b;
            }
        }

        public bool UseOnHaras
        {
            get
            {
                bool b;

                if (UseOnHarasMenu != null)
                {
                    b = UseOnHarasMenu.GetValue<bool>();
                }
                else
                {
                    b = false;
                }
                return b;
            }
        }

        public bool UseOnLaneClear
        {
            get
            {
                bool b;

                if (UseOnComboMenu != null)
                {
                    b = UseOnLaneClearMenu.GetValue<bool>();
                }
                else
                {
                    b = false;
                }
                return b;
            }
        }

        public float LastTimeUsed
        {
            get
            {
                return Instance.CooldownExpires - Instance.Cooldown;
            }
        }

        public float UsedforXSecAgo
        {
            get
            {
                return Game.Time - LastTimeUsed;
            }
        }

        public bool CanCast(int maxToggleState = 1)
        {
            if (ObjectManager.Player.ChampionName.ToLowerInvariant() == "vladimir")
            {
                return true;
            }
            else
            {
                return ObjectManager.Player.Mana > Instance.ManaCost && Instance.ToggleState <= maxToggleState;
            }
        }

        public bool IsReadyToCastOn(Obj_AI_Hero target, int maxToggleState = 1)
        {
            return this.IsReady() && CanCast(maxToggleState) && CanCast(target) && target.IsValidTarget(Range) && !target.IsDead;
        }

    }
}