using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Skills.Condemn.Methods
{
    class VHRevolution
    {
        public static Obj_AI_Base GetTarget(Vector3 fromPosition)
        {
            var HeroList = HeroManager.Enemies.Where(
                                    h =>
                                        h.IsValidTarget(Variables.spells[SpellSlot.E].Range) &&
                                        !h.HasBuffOfType(BuffType.SpellShield) &&
                                        !h.HasBuffOfType(BuffType.SpellImmunity));
                    //dz191.vhr.misc.condemn.rev.accuracy
                    //dz191.vhr.misc.condemn.rev.nextprediction
           var MinChecksPercent = MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.accuracy").Value;
           var PushDistance = MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.pushdistance").Value;

           if (ObjectManager.Player.ServerPosition.UnderTurret(true))
           {
                 return null;
           }

            foreach (var Hero in HeroList)
            {
                var prediction = Variables.spells[SpellSlot.E].GetPrediction(Hero);

                if (MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.condemn.onlystuncurrent") &&
                    Hero.NetworkId != Variables.Orbwalker.GetTarget().NetworkId)
                {
                    continue;
                }

                if (Hero.Health + 10 <=
                    ObjectManager.Player.GetAutoAttackDamage(Hero)*
                    MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.noeaa").Value)
                {
                    continue;
                }

                var PredictionsList = new List<Vector3>
                {
                    Hero.ServerPosition,
                    Hero.Position,
                    prediction.CastPosition,
                    prediction.UnitPosition
                };

                if (Hero.IsDashing())
                {
                    PredictionsList.Add(Hero.GetDashInfo().EndPos.To3D());
                }

                var wallsFound = 0;
                foreach (var position in PredictionsList)
                {
                    for (var i = 0; i < PushDistance; i += (int) Hero.BoundingRadius)
                    {
                        var cPos = position.Extend(fromPosition, -i);
                        if (cPos.IsWall())
                        {
                            wallsFound++;
                            break;
                        }
                    }
                }

                if ((wallsFound/PredictionsList.Count) >= MinChecksPercent/100f)
                {
                    return Hero;
                }
            }
            return null;
        }
    }
}
