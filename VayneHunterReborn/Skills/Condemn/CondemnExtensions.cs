using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Skills.Condemn
{
    static class CondemnExtensions
    {
        public static bool IsCondemnable(this Obj_AI_Base target, Vector3 fromPosition)
        {
            var pushDistance = MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.pushdistance").Value;
            var targetPosition = target.ServerPosition;
            float checkDistance = pushDistance / 40f;
            var pushDirection = (targetPosition - ObjectManager.Player.ServerPosition).Normalized();
            for (int i = 0; i < 40; i++)
            {
                Vector3 finalPosition = targetPosition + (pushDirection * checkDistance * i);
                if (finalPosition.IsWall())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
