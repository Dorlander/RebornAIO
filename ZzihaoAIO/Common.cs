using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ZzihaoAIO.Yasuo
{
    public static class Common
    {
        public static float DistanceToPlayer(this Obj_AI_Base source)
        {
            return ObjectManager.Player.Distance(source);
        }

        public static float DistanceToPlayer(this Vector3 position)
        {
            return position.To2D().DistanceToPlayer();
        }

        public static float DistanceToPlayer(this Vector2 position)
        {
            return ObjectManager.Player.Distance(position);
        }
    }
}
