using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.Helpers;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Skills.Condemn.Methods
{
    class Shine
    {
        public static Obj_AI_Base GetTarget(Vector3 fromPosition)
        {
            foreach (var target in HeroManager.Enemies.Where(h => h.IsValidTarget(Variables.spells[SpellSlot.E].Range)))
            {
                var pushDistance = MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.pushdistance").Value;
                var targetPosition = Variables.spells[SpellSlot.E].GetPrediction(target).UnitPosition;
                var pushDirection = (targetPosition - ObjectManager.Player.ServerPosition).Normalized();
                float checkDistance = pushDistance / 40f;
                for (int i = 0; i < 40; i++)
                {
                    Vector3 finalPosition = targetPosition + (pushDirection * checkDistance * i);
                    var collFlags = NavMesh.GetCollisionFlags(finalPosition);
                    var j4Flag = MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.condemn.condemnflag") && (finalPosition.IsJ4Flag(target));
                    if (collFlags.HasFlag(CollisionFlags.Wall) || collFlags.HasFlag(CollisionFlags.Building) || j4Flag) //not sure about building, I think its turrets, nexus etc
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
            }

            return null;
        }
    }
}
