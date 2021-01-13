using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.Helpers;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Skills.Condemn.Methods
{
    class VHReborn
    {
        public static Obj_AI_Hero GetTarget(Vector3 fromPosition)
        {
            if (ObjectManager.Player.ServerPosition.UnderTurret(true))
            {
                return null;
            }

            var pushDistance = MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.pushdistance").Value;

            foreach (var target in HeroManager.Enemies.Where(h => h.IsValidTarget(Variables.spells[SpellSlot.E].Range) && !h.HasBuffOfType(BuffType.SpellShield) && !h.HasBuffOfType(BuffType.SpellImmunity)))
            {
                var targetPosition = target.ServerPosition;
                var finalPosition = targetPosition.Extend(fromPosition, -pushDistance);
                var numberOfChecks = (float)Math.Ceiling(pushDistance / 30f);


                if (MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.condemn.onlystuncurrent") && Variables.Orbwalker.GetTarget() != null &&
                            !target.NetworkId.Equals(Variables.Orbwalker.GetTarget().NetworkId))
                {
                    continue;
                }

                for (var i = 1; i <= 30; i++)
                {
                    var v3 = (targetPosition - fromPosition).Normalized();
                    var extendedPosition = targetPosition + v3 * (numberOfChecks * i);
                    //var underTurret = MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.condemn.condemnturret") && (Helpers.UnderAllyTurret(finalPosition) || Helpers.IsFountain(finalPosition));
                    var j4Flag = MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.condemn.condemnflag") && (extendedPosition.IsJ4Flag(target));
                    if ((extendedPosition.IsWall() || j4Flag) && (target.Path.Count() < 2) && !target.IsDashing())
                    {

                        if (target.Health + 10 <=
                            ObjectManager.Player.GetAutoAttackDamage(target) *
                            MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.noeaa").Value)
                        {
                            return null;
                        }
                        
                        return target;
                    }
                }
            }
            return null;
        }
    }
}
