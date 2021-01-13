using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.External.Translation.Languages;
using VayneHunter_Reborn.Modules;
using VayneHunter_Reborn.Modules.ModuleList.Condemn;
using VayneHunter_Reborn.Modules.ModuleList.Misc;
using VayneHunter_Reborn.Modules.ModuleList.Tumble;
using VayneHunter_Reborn.Utility.Helpers;

namespace VayneHunter_Reborn.Utility
{
    class Variables
    {
        private const float Range = 1200f;

        public static Menu Menu { get; set; }

        public static Orbwalking.Orbwalker Orbwalker { get; set; }

        public static Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q) },
            { SpellSlot.W, new Spell(SpellSlot.W) },
            { SpellSlot.E, new Spell(SpellSlot.E, 590f) },
            { SpellSlot.R, new Spell(SpellSlot.R) }
        };

        public static List<IModule> moduleList = new List<IModule>()
        {
            new AutoE(),
            new EKS(),
            new LowLifePeel(),
            new NoAAStealth(),
            new QKS(),
            new WallTumble(),
            new Focus2WStacks(),
            new Reveal(),
            new DisableMovement(),
            new CondemnJungleMobs()
        };

        public static List<IVHRLanguage> languageList = new List<IVHRLanguage>()
        {
            new English(),
            new Chinese(),
            new French(),
            new German(),
            new Portuguese(),
            new Korean(),
            new Italian()
        };

        public static IEnumerable<Obj_AI_Hero> MeleeEnemiesTowardsMe
        {
            get
            {
                return
                    HeroManager.Enemies.FindAll(
                        m => m.IsMelee() && m.Distance(ObjectManager.Player) <= PlayerHelper.GetRealAutoAttackRange(m, ObjectManager.Player)
                            && (m.ServerPosition.To2D() + (m.BoundingRadius + 25f) * m.Direction.To2D().Perpendicular()).Distance(ObjectManager.Player.ServerPosition.To2D()) <= m.ServerPosition.Distance(ObjectManager.Player.ServerPosition)
                            && m.IsValidTarget(Range, false));
            }
        }

        public static IEnumerable<Obj_AI_Hero> EnemiesClose
        {
            get
            {
                return
                    HeroManager.Enemies.Where(
                        m =>
                            m.Distance(ObjectManager.Player, true) <= Math.Pow(1000, 2) && m.IsValidTarget(1500, false) &&
                            m.CountEnemiesInRange(m.IsMelee() ? m.AttackRange * 1.5f : m.AttackRange + 20 * 1.5f) > 0);
            }
        }

    }
}
