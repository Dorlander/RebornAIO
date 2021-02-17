using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Support.Util;
using ActiveGapcloser = Support.Util.ActiveGapcloser;
using AntiGapcloser = Support.Util.AntiGapcloser;
using Version = System.Version;

namespace Support
{
    /// <summary>
    ///     PluginBase class
    /// </summary>
    public abstract class PluginBase
    {
        /// <summary>
        ///     Init BaseClass
        /// </summary>
        protected PluginBase()
        {
            Author = "h3h3";
            ChampionName = Player.ChampionName;
            Version = Program.Version;

            InitConfig();
            InitOrbwalker();
            InitPluginEvents();
            InitPrivateEvents();

            Helpers.PrintMessage(string.Format("{0} by {1} v.{2} loaded!", ChampionName, Author, Version));
        }

        /// <summary>
        ///     ActiveMode
        /// </summary>
        public static Orbwalking.OrbwalkingMode ActiveMode { get; set; }

        /// <summary>
        ///     AttackMinion
        /// </summary>
        public bool AttackMinion
        {
            get
            {
                return Helpers.AllyInRange(1500).Count == 0 ||
                       Player.Buffs.Any(buff => buff.Name == "talentreaperdisplay" && buff.Count > 0);
            }
        }

        /// <summary>
        ///     AttackRange
        /// </summary>
        public float AttackRange
        {
            get { return Orbwalking.GetRealAutoAttackRange(Target); }
        }

        /// <summary>
        ///     Plugin display name
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        ///     Champion Author
        /// </summary>
        public string ChampionName { get; set; }

        /// <summary>
        ///     ComboConfig
        /// </summary>
        public Menu ComboConfig { get; set; }

        /// <summary>
        ///     ComboMode
        /// </summary>
        public bool ComboMode
        {
            get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && !Player.IsDead; }
        }

        /// <summary>
        ///     Config
        /// </summary>
        public static Menu Config { get; set; }

        /// <summary>
        ///     DrawingConfig
        /// </summary>
        public Menu DrawingConfig { get; set; }

        /// <summary>
        ///     E
        /// </summary>
        public Spell E { get; set; }

        /// <summary>
        ///     HarassConfig
        /// </summary>
        public Menu HarassConfig { get; set; }

        /// <summary>
        ///     HarassMana
        /// </summary>
        public bool HarassMana
        {
            get { return Player.Mana > Player.MaxMana*ConfigValue<Slider>("HarassMana").Value/100; }
        }

        /// <summary>
        ///     HarassMode
        /// </summary>
        public bool HarassMode
        {
            get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && HarassMana && !Player.IsDead; }
        }

        /// <summary>
        ///     InterruptConfig
        /// </summary>
        public Menu InterruptConfig { get; set; }

        /// <summary>
        ///     ManaConfig
        /// </summary>
        public Menu ManaConfig { get; set; }

        /// <summary>
        ///     MiscConfig
        /// </summary>
        public Menu MiscConfig { get; set; }

        /// <summary>
        ///     Orbwalker
        /// </summary>
        public Orbwalking.Orbwalker Orbwalker { get; set; }

        /// <summary>
        ///     OrbwalkerTarget
        /// </summary>
        public AttackableUnit OrbwalkerTarget
        {
            get { return Orbwalker.GetTarget(); }
        }

        /// <summary>
        ///     Player Object
        /// </summary>
        public Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        /// <summary>
        ///     Q
        /// </summary>
        public Spell Q { get; set; }

        /// <summary>
        ///     R
        /// </summary>
        public Spell R { get; set; }

        /// <summary>
        ///     Target
        /// </summary>
        public Obj_AI_Hero Target
        {
            get { return TargetSelector.GetTarget(2000, TargetSelector.DamageType.Magical); }
        }

        /// <summary>
        ///     SupportTargetSelector
        /// </summary>
        public TargetSelector TargetSelector { get; set; }

        /// <summary>
        ///     Plugin Version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        ///     W
        /// </summary>
        public Spell W { get; set; }

        /// <summary>
        ///     ConfigValue
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="item">string</param>
        /// <remarks>
        ///     Helper for
        /// </remarks>
        /// <returns></returns>
        public T ConfigValue<T>(string item)
        {
            return Config.Item(item + ChampionName).GetValue<T>();
        }

        /// <summary>
        ///     OnProcessPacket
        /// </summary>
        /// <remarks>
        ///     override to Implement OnProcessPacket logic
        /// </remarks>
        /// <param name="args"></param>
        public virtual void OnProcessPacket(GamePacketEventArgs args)
        {
        }

        /// <summary>
        ///     OnSendPacket
        /// </summary>
        /// <remarks>
        ///     override to Implement OnSendPacket logic
        /// </remarks>
        /// <param name="args"></param>
        public virtual void OnSendPacket(GamePacketEventArgs args)
        {
        }

        /// <summary>
        ///     OnPossibleToInterrupt
        /// </summary>
        /// <remarks>
        ///     override to Implement SpellsInterrupt logic
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public virtual void OnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
        }

        /// <summary>
        ///     OnEnemyGapcloser
        /// </summary>
        /// <remarks>
        ///     override to Implement AntiGapcloser logic
        /// </remarks>
        /// <param name="gapcloser">ActiveGapcloser</param>
        public virtual void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
        }

        /// <summary>
        ///     OnUpdate
        /// </summary>
        /// <remarks>
        ///     override to Implement Update logic
        /// </remarks>
        /// <param name="args">EventArgs</param>
        public virtual void OnUpdate(EventArgs args)
        {
        }

        /// <summary>
        ///     OnBeforeAttack
        /// </summary>
        /// <remarks>
        ///     override to Implement OnBeforeAttack logic
        /// </remarks>
        /// <param name="args">Orbwalking.BeforeAttackEventArgs</param>
        public virtual void OnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
        }

        /// <summary>
        ///     OnAfterAttack
        /// </summary>
        /// <remarks>
        ///     override to Implement OnAfterAttack logic
        /// </remarks>
        /// <param name="unit">unit</param>
        /// <param name="target">target</param>
        public virtual void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
        }

        /// <summary>
        ///     OnLoad
        /// </summary>
        /// <remarks>
        ///     override to Implement class Initialization
        /// </remarks>
        /// <param name="args">EventArgs</param>
        public virtual void OnLoad(EventArgs args)
        {
        }

        /// <summary>
        ///     OnDraw
        /// </summary>
        /// <remarks>
        ///     override to Implement Drawing
        /// </remarks>
        /// <param name="args">EventArgs</param>
        public virtual void OnDraw(EventArgs args)
        {
        }

        /// <summary>
        ///     ComboMenu
        /// </summary>
        /// <remarks>
        ///     override to Implement ComboMenu Config
        /// </remarks>
        /// <param name="config">Menu</param>
        public virtual void ComboMenu(Menu config)
        {
        }

        /// <summary>
        ///     HarassMenu
        /// </summary>
        /// <remarks>
        ///     override to Implement HarassMenu Config
        /// </remarks>
        /// <param name="config">Menu</param>
        public virtual void HarassMenu(Menu config)
        {
        }

        /// <summary>
        ///     ManaMenu
        /// </summary>
        /// <remarks>
        ///     override to Implement ManaMenu Config
        /// </remarks>
        /// <param name="config">Menu</param>
        public virtual void ManaMenu(Menu config)
        {
        }

        /// <summary>
        ///     MiscMenu
        /// </summary>
        /// <remarks>
        ///     override to Implement MiscMenu Config
        /// </remarks>
        /// <param name="config">Menu</param>
        public virtual void MiscMenu(Menu config)
        {
        }

        /// <summary>
        ///     MiscMenu
        /// </summary>
        /// <remarks>
        ///     override to Implement Interrupt Config
        /// </remarks>
        /// <param name="config">Menu</param>
        public virtual void InterruptMenu(Menu config)
        {
        }

        /// <summary>
        ///     DrawingMenu
        /// </summary>
        /// <remarks>
        ///     override to Implement DrawingMenu Config
        /// </remarks>
        /// <param name="config">Menu</param>
        public virtual void DrawingMenu(Menu config)
        {
        }

        #region Private Stuff

        /// <summary>
        ///     PluginEvents Initialization
        /// </summary>
        private void InitPluginEvents()
        {
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalking.BeforeAttack += OnBeforeAttack;
            Orbwalking.AfterAttack += OnAfterAttack;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += OnPossibleToInterrupt;
            //Game.OnGameSendPacket += OnSendPacket;
            //Game.OnGameProcessPacket += OnProcessPacket;
            OnLoad(new EventArgs());
        }

        private void DrawSpell(Spell spell)
        {
            if (spell == null)
            {
                return;
            }

            var menu = ConfigValue<Circle>(spell.Slot + "Range");
            if (menu.Active && spell.Level > 0)
            {
                Render.Circle.DrawCircle(
                    Player.Position, spell.Range, spell.IsReady() ? menu.Color : Color.FromArgb(150, Color.Red));
            }
        }

        /// <summary>
        ///     PrivateEvents Initialization
        /// </summary>
        private void InitPrivateEvents()
        {
            Orbwalking.BeforeAttack += args =>
            {
                try
                {
                    if (args.Target.IsValid<Obj_AI_Minion>() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                    {
                        switch (ConfigValue<StringList>("AttackMinions").SelectedIndex)
                        {
                            case 0: // Smart
                                args.Process = AttackMinion;
                                break;

                            case 1: // Never
                                args.Process = false;
                                break;
                        }
                    }

                    if (args.Target.IsValid<Obj_AI_Hero>() && !ConfigValue<bool>("AttackChampions") &&
                        Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None)
                    {
                        args.Process = false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            };

            Drawing.OnDraw += args =>
            {
                try
                {
                    if (Player.IsDead)
                    {
                        return;
                    }

                    if (Target != null && ConfigValue<Circle>("Target").Active)
                    {
                        Render.Circle.DrawCircle(Target.Position, 125, ConfigValue<Circle>("Target").Color);
                    }

                    DrawSpell(Q);
                    DrawSpell(W);
                    DrawSpell(E);
                    DrawSpell(R);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            };
        }

        /// <summary>
        ///     Config Initialization
        /// </summary>
        private void InitConfig()
        {
            Config = new Menu("Support: " + Player.ChampionName, Player.ChampionName, true);
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("Target Selector", "Target Selector")));

            ComboConfig = Config.AddSubMenu(new Menu("Combo", "Combo"));
            HarassConfig = Config.AddSubMenu(new Menu("Harass", "Harass"));
            ManaConfig = Config.AddSubMenu(new Menu("Mana Limiter", "Mana Limiter"));
            MiscConfig = Config.AddSubMenu(new Menu("Misc", "Misc"));
            InterruptConfig = Config.AddSubMenu(new Menu("Interrupt", "Interrupt"));
            DrawingConfig = Config.AddSubMenu(new Menu("Drawings", "Drawings"));

            // mana
            ManaConfig.AddSlider("HarassMana", "Harass Mana %", 1, 1, 100);

            // misc
            MiscConfig.AddList("AttackMinions", "Attack Minions?", new[] {"Smart", "Never", "Always"});
            MiscConfig.AddBool("AttackChampions", "Attack Champions?", true);

            // drawing
            DrawingConfig.AddItem(
                new MenuItem("Target" + ChampionName, "Target").SetValue(new Circle(true, Color.DodgerBlue)));
            DrawingConfig.AddItem(
                new MenuItem("QRange" + ChampionName, "Q Range").SetValue(
                    new Circle(false, Color.FromArgb(150, Color.DodgerBlue))));
            DrawingConfig.AddItem(
                new MenuItem("WRange" + ChampionName, "W Range").SetValue(
                    new Circle(false, Color.FromArgb(150, Color.DodgerBlue))));
            DrawingConfig.AddItem(
                new MenuItem("ERange" + ChampionName, "E Range").SetValue(
                    new Circle(false, Color.FromArgb(150, Color.DodgerBlue))));
            DrawingConfig.AddItem(
                new MenuItem("RRange" + ChampionName, "R Range").SetValue(
                    new Circle(false, Color.FromArgb(150, Color.DodgerBlue))));

            // plugins
            ComboMenu(ComboConfig);
            HarassMenu(HarassConfig);
            ManaMenu(ManaConfig);
            MiscMenu(MiscConfig);
            InterruptMenu(InterruptConfig);
            DrawingMenu(DrawingConfig);

            Config.AddToMainMenu();
        }

        /// <summary>
        ///     Orbwalker Initialization
        /// </summary>
        private void InitOrbwalker()
        {
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
        }

        #endregion
    }
}