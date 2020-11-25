namespace KoreanLucian
{
    using System.Collections.Generic;
    using System.Linq;

    using KoreanCommon;

    using LeagueSharp;
    using LeagueSharp.Common;

    public static class Spells
    {
        private static CommonChampion lucian;

        public static void Load(CommonChampion champion)
        {
            lucian = champion;

            CommonSpell Q = new CommonSpell(SpellSlot.Q, 675, TargetSelector.DamageType.Physical);
            CommonSpell W = new CommonSpell(SpellSlot.W, 1000, TargetSelector.DamageType.Magical);
            CommonSpell E = new CommonSpell(SpellSlot.E, 425, TargetSelector.DamageType.Physical);
            CommonSpell R = new CommonSpell(SpellSlot.R, 1400, TargetSelector.DamageType.Physical);

            Q.SetTargetted(0.25f, float.MaxValue);
            W.SetSkillshot(0.4f, 150f, 1600, true, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 1f, float.MaxValue, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.2f, 110f, 2500, true, SkillshotType.SkillshotLine);

            champion.Spells.AddSpell(Q);
            champion.Spells.AddSpell(W);
            champion.Spells.AddSpell(E);
            champion.Spells.AddSpell(R);
        }

        public static float MaxComboDamage(Obj_AI_Hero target)
        {
            float damage = 0f;

            if (lucian == null)
            {
                return damage;
            }

            if (lucian.Spells != null && lucian.Spells.SpellList.Count > 0)
            {
                List<CommonSpell> Spells =
                    lucian.Spells.SpellList.Where(x => x.UseOnCombo && x.IsReady() && x.CanCast()).ToList();
                if (Spells.Count > 0)
                {
                    foreach (CommonSpell spell in Spells)
                    {
                        damage += spell.GetDamage(target);
                        if (spell.Slot == SpellSlot.E)
                        {
                            damage += (float)lucian.Player.GetAutoAttackDamage(target) * 1.4f;
                        }
                    }
                }
            }

            return damage;
        }
    }
}