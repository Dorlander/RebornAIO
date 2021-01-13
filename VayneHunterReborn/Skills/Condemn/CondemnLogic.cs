using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using VayneHunter_Reborn.Skills.Condemn.Methods;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Skills.Condemn
{
    class CondemnLogic
    {
        private static Spell E
        {
            get { return Variables.spells[SpellSlot.E]; }
        }

        private static readonly Spell TrinketSpell = new Spell(SpellSlot.Trinket);

        public static void OnLoad()
        {
            InterrupterGapcloser.OnLoad();
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        public static void Execute(EventArgs args)
        {

            if (!E.IsEnabledAndReady(Variables.Orbwalker.ActiveMode))
            {
                return;
            }

            var CondemnTarget = GetCondemnTarget(ObjectManager.Player.ServerPosition);
            if (CondemnTarget.IsValidTarget())
            {
               // var AAForE = MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.noeaa").Value;

               // if (CondemnTarget.Health / ObjectManager.Player.GetAutoAttackDamage(CondemnTarget, true) < AAForE)
               // {
               //     return;
               // }

                E.Cast(CondemnTarget);
                TrinketBush(CondemnTarget.ServerPosition.Extend(ObjectManager.Player.ServerPosition, -450f));
            }
        }

        private static void TrinketBush(Vector3 endPosition)
        {
            if (TrinketSpell.IsReady())
            {
                var extended = ObjectManager.Player.ServerPosition.Extend(endPosition, 400f);
                if (NavMesh.IsWallOfGrass(extended, 130f) && !NavMesh.IsWallOfGrass(ObjectManager.Player.ServerPosition, 65f))
                {
                    LeagueSharp.Common.Utility.DelayAction.Add((int)(Game.Ping / 2f + 250), () =>
                    {
                        TrinketSpell.Cast(extended);
                    });
                }
            }
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            return;
            if (sender != null && sender.Owner != null && sender.Owner.IsMe && args.Slot == SpellSlot.E && (Variables.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo))
            {
                if (!(args.Target is Obj_AI_Hero))
                {
                    args.Process = false;
                    return;
                }

                if (GetCondemnTarget(ObjectManager.Player.ServerPosition).IsValidTarget())
                {
                    if (!Shine.GetTarget(ObjectManager.Player.ServerPosition).IsValidTarget())
                    {
                        args.Process = false;
                    }
                }
            }
        }

        public static Obj_AI_Base GetCondemnTarget(Vector3 fromPosition)
        {
            switch (MenuExtensions.GetItemValue<StringList>("dz191.vhr.misc.condemn.condemnmethod").SelectedIndex)
            {
                case 0:
                    //VH Revolution
                    return VHRevolution.GetTarget(fromPosition);
                case 1:
                    //VH Reborn
                    return VHReborn.GetTarget(fromPosition);
                case 2:
                    //Marksman / Gosu
                    return Marksman.GetTarget(fromPosition);
                case 3:
                    //Shine#
                    return Shine.GetTarget(fromPosition);
            }
            return null;
        }
    }
}
