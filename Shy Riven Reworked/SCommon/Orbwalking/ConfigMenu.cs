using System;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace SCommon.Orbwalking
{
    public class ConfigMenu
    {
        private Menu m_Menu;
        private Orbwalker m_orbInstance;

        public ConfigMenu(Orbwalker instance, Menu menuToAttach)
        {
            m_orbInstance = instance;
            m_Menu = new Menu("Orbwalking", "Orbwalking.Root");
            m_Menu.AddItem(new MenuItem("Orbwalking.Root.iExtraWindup", "Extra Windup Time").SetValue(new Slider(40, 0, 100)));
            m_Menu.AddItem(new MenuItem("Orbwalking.Root.iMovementDelay", "Movement Delay").SetValue(new Slider(30, 0, 1000)));
            m_Menu.AddItem(new MenuItem("Orbwalking.Root.iHoldPosition", "Hold Area Radius").SetValue(new Slider(30, 30, 250)));
            m_Menu.AddItem(new MenuItem("Orbwalking.Root.blLastHit", "Last Hit").SetValue(new KeyBind('X', KeyBindType.Press)));
            m_Menu.AddItem(new MenuItem("Orbwalking.Root.blHarass", "Harass").SetValue(new KeyBind('C', KeyBindType.Press)));
            m_Menu.AddItem(new MenuItem("Orbwalking.Root.blLaneClear", "Lane Clear").SetValue(new KeyBind('V', KeyBindType.Press)));
            m_Menu.AddItem(new MenuItem("Orbwalking.Root.blCombo", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            Menu misc = new Menu("Misc", "Orbwalking.Misc");
            misc.AddItem(new MenuItem("Orbwalking.Misc.blAttackStructures", "Attack Structures").SetValue(true));
            misc.AddItem(new MenuItem("Orbwalking.Misc.blSupportMode", "Support Mode").SetValue(false));
            misc.AddItem(new MenuItem("Orbwalking.Misc.blDontAttackChampWhileLaneClear", "Dont attack champions while Lane Clear").SetValue(false));
            misc.AddItem(new MenuItem("Orbwalking.Misc.blDontMoveMouseOver", "Mouse over hero to stop move").SetValue(false));
            misc.AddItem(new MenuItem("Orbwalking.Misc.blMagnetMelee", "Magnet Target (Only Melee)").SetValue(false)).ValueChanged +=
                (s, ar) =>
                {
                    var it = misc.Item("Orbwalking.Misc.iStickRange");
                    if (it != null)
                        it.Show(ar.GetNewValue<bool>());
                };
            misc.AddItem(new MenuItem("Orbwalking.Misc.iStickRange", "Stick Range").SetValue(new Slider(300, 0, 500))).Show(misc.Item("Orbwalking.Misc.blMagnetMelee").GetValue<bool>());
            misc.AddItem(new MenuItem("Orbwalking.Misc.blDontMoveInRange", "Dont move if enemy in AA range").SetValue(false));
            misc.AddItem(new MenuItem("Orbwalking.Misc.blLegitMode", "Legit Mode").SetValue(false)).ValueChanged +=
                (s, ar) =>
                {
                    var it = misc.Item("Orbwalking.Misc.iLegitPercent");
                    if (it != null)
                        it.Show(ar.GetNewValue<bool>());
                };
            misc.AddItem(new MenuItem("Orbwalking.Misc.iLegitPercent", "Make Me Legit %").SetValue(new Slider(20, 0, 100))).Show(misc.Item("Orbwalking.Misc.blLegitMode").GetValue<bool>());

            Menu drawings = new Menu("Drawings", "Orbwalking.Drawings");
            drawings.AddItem(new MenuItem("Orbwalking.Drawings.SelfAACircle", "Self AA Circle").SetValue(new Circle(true, Color.FromArgb(155, 255, 255, 0))));
            drawings.AddItem(new MenuItem("Orbwalking.Drawings.EnemyAACircle", "Enemy AA Circle").SetValue(new Circle(false, Color.FromArgb(155, 255, 255, 0))));
            drawings.AddItem(new MenuItem("Orbwalking.Drawings.LastHitMinion", "Last Hitable Minion").SetValue(new Circle(true, Color.FromArgb(155, 255, 255, 0))));
            drawings.AddItem(new MenuItem("Orbwalking.Drawings.HoldZone", "Hold Zone").SetValue(new Circle(false, Color.FromArgb(155, 255, 255, 0))));
            drawings.AddItem(new MenuItem("Orbwalking.Drawings.iLineWidth", "Line Width").SetValue(new Slider(2, 1, 6)));

            m_Menu.AddSubMenu(drawings);
            m_Menu.AddSubMenu(misc);
            menuToAttach.AddSubMenu(m_Menu);

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            if (ExtraWindup == 40 || ExtraWindup == 70)
            {
                if (ObjectManager.Player.IsMelee)
                    ExtraWindup = 70;
                else
                    ExtraWindup = 40;
            }
        }

        /// <summary>
        /// Gets combo key is pressed
        /// </summary>
        public bool Combo
        {
            get { return m_Menu.Item("Orbwalking.Root.blCombo").GetValue<KeyBind>().Active; }
        }

        /// <summary>
        /// Gets harass key is pressed
        /// </summary>
        public bool Harass
        {
            get { return m_Menu.Item("Orbwalking.Root.blHarass").GetValue<KeyBind>().Active; }
        }

        /// <summary>
        /// Gets lane clear key is pressed
        /// </summary>
        public bool LaneClear
        {
            get { return m_Menu.Item("Orbwalking.Root.blLaneClear").GetValue<KeyBind>().Active; }
        }

        /// <summary>
        /// Gets last hit key is pressed
        /// </summary>
        public bool LastHit
        {
            get { return m_Menu.Item("Orbwalking.Root.blLastHit").GetValue<KeyBind>().Active; }
        }

        /// <summary>
        /// Gets or sets extra windup time value
        /// </summary>
        public int ExtraWindup
        {
            get { return m_Menu.Item("Orbwalking.Root.iExtraWindup").GetValue<Slider>().Value; }
            set { m_Menu.Item("Orbwalking.Root.iExtraWindup").SetValue(new Slider(value, 0, 100)); }
        }

        /// <summary>
        /// Gets or sets movement delay value
        /// </summary>
        public int MovementDelay
        {
            get { return m_Menu.Item("Orbwalking.Root.iMovementDelay").GetValue<Slider>().Value; }
            set { m_Menu.Item("Orbwalking.Root.iMovementDelay").SetValue(new Slider(value, 0, 1000)); }
        }

        /// <summary>
        /// Gets or sets hold area radius value
        /// </summary>
        public int HoldAreaRadius
        {
            get { return m_Menu.Item("Orbwalking.Root.iHoldPosition").GetValue<Slider>().Value; }
            set { m_Menu.Item("Orbwalking.Root.iHoldPosition").SetValue(new Slider(value, 30, 250)); }
        }

        /// <summary>
        /// Gets or sets attack structures value
        /// </summary>
        public bool AttackStructures
        {
            get { return m_Menu.Item("Orbwalking.Misc.blAttackStructures").GetValue<bool>(); }
            set { m_Menu.Item("Orbwalking.Misc.blAttackStructures").SetValue(value); }
        }

        /// <summary>
        /// Gets or sets support mode value
        /// </summary>
        public bool SupportMode
        {
            get { return m_Menu.Item("Orbwalking.Misc.blSupportMode").GetValue<bool>(); }
            set { m_Menu.Item("Orbwalking.Misc.blSupportMode").SetValue(value); }
        }
        
        /// <summary>
        /// Gets or sets Dont attack champions while laneclear mode value
        /// </summary>
        public bool DontAttackChampWhileLaneClear
        {
            get { return m_Menu.Item("Orbwalking.Misc.blDontAttackChampWhileLaneClear").GetValue<bool>(); }
            set { m_Menu.Item("Orbwalking.Misc.blDontAttackChampWhileLaneClear").SetValue(value); }
        }

        /// <summary>
        /// Gets or sets Dont move over value
        /// </summary>
        public bool DontMoveMouseOver
        {
            get { return m_Menu.Item("Orbwalking.Misc.blDontMoveMouseOver").GetValue<bool>(); }
            set { m_Menu.Item("Orbwalking.Misc.blDontMoveMouseOver").SetValue(value); }
        }

        /// <summary>
        /// Gets or set magnet melee value
        /// </summary>
        public bool MagnetMelee
        {
            get { return m_Menu.Item("Orbwalking.Misc.blMagnetMelee").GetValue<bool>(); }
            set { m_Menu.Item("Orbwalking.Misc.blMagnetMelee").SetValue(value); }
        }

        /// <summary>
        /// Gets or sets stick range value
        /// </summary>
        public int StickRange
        {
            get { return m_Menu.Item("Orbwalking.Misc.iStickRange").GetValue<Slider>().Value; }
            set { m_Menu.Item("Orbwalking.Misc.iStickRange").SetValue(new Slider(value, 0, 500)); }
        }

        /// <summary>
        /// Gets or sets dont move in aa range value
        /// </summary>
        public bool DontMoveInRange
        {
            get { return m_Menu.Item("Orbwalking.Misc.blDontMoveInRange").GetValue<bool>(); }
            set { m_Menu.Item("Orbwalking.Misc.blDontMoveInRange").SetValue(value); }
        }

        /// <summary>
        /// Gets or set legit mode value
        /// </summary>
        public bool LegitMode
        {
            get { return m_Menu.Item("Orbwalking.Misc.blLegitMode").GetValue<bool>() && ObjectManager.Player.GetAttackSpeed() > 1.40f && m_orbInstance.ActiveMode == Orbwalker.Mode.Combo; }
            set { m_Menu.Item("Orbwalking.Misc.blLegitMode").SetValue(value); }
        }

        /// <summary>
        /// Gets or set legit percent value
        /// </summary>
        public int LegitPercent
        {
            get { return m_Menu.Item("Orbwalking.Misc.iLegitPercent").GetValue<Slider>().Value; }
            set { m_Menu.Item("Orbwalking.Misc.iLegitPercent").SetValue(new Slider(value, 0, 100)); }
        }

        /// <summary>
        /// Gets Self aa circle value
        /// </summary>
        public Circle SelfAACircle
        {
            get { return m_Menu.Item("Orbwalking.Drawings.SelfAACircle").GetValue<Circle>(); }
        }

        /// <summary>
        /// Gets enemy aa circle value
        /// </summary>
        public Circle EnemyAACircle
        {
            get { return m_Menu.Item("Orbwalking.Drawings.EnemyAACircle").GetValue<Circle>(); }
        }

        /// <summary>
        /// Gets last hit minion value
        /// </summary>
        public Circle LastHitMinion
        {
            get { return m_Menu.Item("Orbwalking.Drawings.LastHitMinion").GetValue<Circle>(); }
        }

        /// <summary>
        /// Gets hold zone value
        /// </summary>
        public Circle HoldZone
        {
            get { return m_Menu.Item("Orbwalking.Drawings.HoldZone").GetValue<Circle>(); }
        }

        /// <summary>
        /// Gets line width value
        /// </summary>
        public int LineWidth
        {
            get { return m_Menu.Item("Orbwalking.Drawings.iLineWidth").GetValue<Slider>().Value; }
        }
    }
}
