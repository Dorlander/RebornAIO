using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace TreeCritter
{
    internal class Program
    {
        public static Menu Menu;
        public static AttackableUnit PriorityTarget;
        public static AttackableUnit CurrentTarget;

        public static Obj_AI_Hero Player
        {
            get { return ObjectHandler.Player; }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("TreeCritter", "TreeCritter", true);
            Menu.AddItem(new MenuItem("FocusMode", "Focus Mode").SetValue(new KeyBind(32, KeyBindType.Press)));
            Menu.AddItem(new MenuItem("DPSMode", "DPS Mode").SetValue(new KeyBind('C', KeyBindType.Press)));
            Menu.AddToMainMenu();

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Menu.Item("FocusMode").IsActive() && !Menu.Item("DPSMode").IsActive())
            {
                return;
            }

            var unit = sender as Obj_AI_Hero;
            var target = args.Target as Obj_AI_Base;
            var spell = args.SData;

            if (unit == null || !unit.IsValid || !unit.IsMe || target == null || !target.IsValid || spell == null ||
                spell.Name.Contains("Crit") || !spell.IsAutoAttack())
            {
                return;
            }

            var selectedTarget = Hud.SelectedUnit;

            if (selectedTarget != null && selectedTarget.Equals(args.Target) && Menu.Item("FocusMode").IsActive())
            {
                return;
            }

            if (target is Obj_AI_Turret || target.Health <= Player.GetAutoAttackDamage(target))
            {
                return;
            }

            Player.IssueOrder(GameObjectOrder.AttackUnit, GetOtherUnit());
        }

        private static AttackableUnit GetOtherUnit()
        {
            return GetAttackableUnit<Obj_AI_Hero>() ?? GetAttackableUnit<Obj_AI_Minion>();
        }

        private static AttackableUnit GetAttackableUnit<T>() where T : Obj_AI_Base, new()
        {
            var aaRange = GetAARange();
            return
                ObjectHandler.Get<T>()
                    .Enemies.OrderBy(h => Player.Distance(h as AttackableUnit))
                    .FirstOrDefault(h => h.IsValidTarget(aaRange));
        }

        private static float GetAARange()
        {
            return Orbwalking.GetRealAutoAttackRange(Player);
        }
    }
}