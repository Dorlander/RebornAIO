using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace KurisuDarius
{
    internal class KurisuLib
    {
        internal static Obj_AI_Hero Player = ObjectManager.Player;
        internal static Dictionary<string, Spell> Spellbook = new Dictionary<string, Spell>
        {
            { "Q", new Spell(SpellSlot.Q, 425f) },
            { "W", new Spell(SpellSlot.W, 200f) },
            { "E", new Spell(SpellSlot.E, 490f) },
            { "R", new Spell(SpellSlot.R, 460f) }
        };

        internal static void HandleItems()
        {
            // hydra
            if (Items.HasItem(3077) && Items.CanUseItem(3077))
                Items.UseItem(3077);

            // tiamat
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
                Items.UseItem(3074);

            // titanic hydra
            if (Items.HasItem(3748) && Items.CanUseItem(3748))
                Items.UseItem(3748);
        }

        internal static float QDmg(Obj_AI_Base unit)
        {
            return
                (float)
                    Player.CalcDamage(unit, Damage.DamageType.Physical,
                        new[] {20, 20, 35, 50, 65, 80} [Spellbook["Q"].Level] +
                       (new[] {1.0, 1.0, 1.1, 1.2, 1.3, 1.4} [Spellbook["Q"].Level] * Player.FlatPhysicalDamageMod));
        }

        internal static float WDmg(Obj_AI_Base unit)
        {
            return
                (float)
                    Player.CalcDamage(unit, Damage.DamageType.Physical,
                       Player.TotalAttackDamage + (0.4 * Player.TotalAttackDamage));
        }

        internal static float RDmg(Obj_AI_Base unit, int stackcount)
        {
            var bonus = (new[] {20, 20, 40, 60} [Spellbook["R"].Level] +
                            (0.25 * Player.FlatPhysicalDamageMod) * stackcount);
            return
                (float) (bonus + (Player.CalcDamage(unit, Damage.DamageType.True,
                        new[] { 100, 100, 200, 300} [Spellbook["R"].Level] + (0.75 * Player.FlatPhysicalDamageMod))));
        }

        internal static float Hemorrhage(Obj_AI_Base unit, int stackcount)
        {
            if (stackcount < 1)
                stackcount = 1;

            return
                (float)
                    Player.CalcDamage(unit, Damage.DamageType.Physical,
                        (9 + Player.Level) + (0.3 * Player.FlatPhysicalDamageMod)) * stackcount;
        }
    }
}
