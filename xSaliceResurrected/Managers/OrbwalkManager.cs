using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSaliceResurrected.Utilities;

namespace xSaliceResurrected.Managers
{
    static class OrbwalkManager
    {
        public static void Orbwalk(Obj_AI_Base target, Vector3 pos)
        {
            Orbwalking.Orbwalk(target, pos);
        }

        public static bool InAutoAttackRange(Obj_AI_Base target)
        {
            return Champion.Orbwalker.InAutoAttackRange(target);
        }

        public static void ResetAutoAttackTimer()
        {
            Orbwalking.ResetAutoAttackTimer();
        }

        public static void SetAttack(bool f)
        {
            Champion.Orbwalker.SetAttack(f);
        }

        public static void SetMovement(bool f)
        {
            Champion.Orbwalker.SetMovement(f);
        }

        public static bool CanMove(float delay)
        {
            return Orbwalking.CanMove(delay);
        }

        public static bool IsAutoAttack(string name)
        {
            return Orbwalking.IsAutoAttack(name);
        }
    }
}
