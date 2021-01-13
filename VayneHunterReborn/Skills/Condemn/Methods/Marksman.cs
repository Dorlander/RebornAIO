using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.Helpers;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Skills.Condemn.Methods
{
    class Marksman
    {
        public static Obj_AI_Hero GetTarget(Vector3 fromPosition)
        {
            foreach (var target in HeroManager.Enemies.Where(h => h.IsValidTarget(Variables.spells[SpellSlot.E].Range)))
            {
                var pushDistance = MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.pushdistance").Value;
                var targetPosition = Variables.spells[SpellSlot.E].GetPrediction(target).UnitPosition;
                var finalPosition = targetPosition.Extend(fromPosition, -pushDistance);
                var finalPosition2 = targetPosition.Extend(fromPosition, -(pushDistance / 2f));
                var underTurret = MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.condemn.condemnturret") && (finalPosition.UnderTurret(true));
                var j4Flag = MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.condemn.condemnflag") && (finalPosition.IsJ4Flag(target) || finalPosition2.IsJ4Flag(target));
                if (finalPosition.IsWall() || finalPosition2.IsWall() || underTurret || j4Flag)
                {
                    if (MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.condemn.onlystuncurrent") && Variables.Orbwalker.GetTarget() != null &&
                            !target.NetworkId.Equals(Variables.Orbwalker.GetTarget().NetworkId))
                    {
                        return null;
                    }

                    if (target.Health + 10 <=
                            ObjectManager.Player.GetAutoAttackDamage(target) *
                            MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.noeaa").Value)
                    {
                        return null;
                    }

                    return target;
                }
            }
            return null;
        }
    }
}
