using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace SCommon.TS
{
    internal static class ConfigMenu
    {
        private static Menu s_Config;
        public static void Create(Menu menuToAttach)
        {
            s_Config = new Menu("Target Selector", "TargetSelector.Root");
            s_Config.AddItem(new MenuItem("TargetSelector.Root.blFocusSelected", "Focus Selected Target").SetValue(true));
            s_Config.AddItem(new MenuItem("TargetSelector.Root.blOnlyAttackSelected", "Only Attack Selected Target").SetValue(false));
            s_Config.AddItem(new MenuItem("TargetSelector.Root.SelectedTargetColor", "Selected Target Color").SetValue(new Circle(true, System.Drawing.Color.Red)));

            Menu champPrio = new Menu("Champion Priority", "TargetSelector.Priority");
            foreach (var enemy in HeroManager.Enemies)
                champPrio.AddItem(new MenuItem(String.Format("TargetSelector.Priority.{0}", enemy.CharData.BaseSkinName), enemy.CharData.BaseSkinName).SetValue(new Slider(1, 1, 5)));

            s_Config.AddSubMenu(champPrio);

            s_Config.AddItem(new MenuItem("TargetSelector.Root.iTargettingMode", "Targetting Mode").SetValue(new StringList(new[] { "Auto", "Low HP", "Most AD", "Most AP", "Closest", "Near Mouse", "Less Attack", "Less Cast" }, 0)));

            menuToAttach.AddSubMenu(s_Config);
        }

        /// <summary>
        /// Gets or sets focus selected target value
        /// </summary>
        public static bool FocusSelected
        {
            get { return s_Config.Item("TargetSelector.Root.blFocusSelected").GetValue<bool>(); }
            set { s_Config.Item("TargetSelector.Root.blFocusSelected").SetValue(value); }
        }

        /// <summary>
        /// Gets or sets only attack selected value
        /// </summary>
        public static bool OnlyAttackSelected
        {
            get { return s_Config.Item("TargetSelector.Root.blOnlyAttackSelected").GetValue<bool>(); }
            set { s_Config.Item("TargetSelector.Root.blOnlyAttackSelected").SetValue(value); }
        }

        /// <summary>
        /// Gets or sets selected target color value
        /// </summary>
        public static Circle SelectedTargetColor
        {
            get { return s_Config.Item("TargetSelector.Root.SelectedTargetColor").GetValue<Circle>(); }
            set { s_Config.Item("TargetSelector.Root.SelectedTargetColor").SetValue(value); }
        }

        /// <summary>
        /// Gets targetting mode
        /// </summary>
        public static int TargettingMode
        {
            get { return s_Config.Item("TargetSelector.Root.iTargettingMode").GetValue<StringList>().SelectedIndex; }
        }

        /// <summary>
        /// Gets priority of given enemy
        /// </summary>
        /// <param name="enemy">Enemy</param>
        /// <returns>Given enemy's priority which set by user</returns>
        public static int GetChampionPriority(Obj_AI_Hero enemy)
        {
            return s_Config.Item(String.Format("TargetSelector.Priority.{0}", enemy.CharData.BaseSkinName)).GetValue<Slider>().Value;
        }
    }
}
