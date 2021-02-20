using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Version = System.Version;

namespace SigmaSeries
{

    public abstract class PluginBase
    {
        public Orbwalking.Orbwalker Orbwalker { set; get; }
        public string ChampName { get; set; }
        public Version Version { get; set; }
        public bool ComboActive { get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo; } }
        public bool HarassActive { get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed; } }
        public bool WaveClearActive { get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear; } }
        public bool FleeActive { get { return false; } }
        public bool FreezeActive { get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit; } }
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public Spell Q { get; set; }
        public Spell W { get; set; }
        public Spell E { get; set; }
        public Spell R { get; set; }

        public List<Obj_AI_Base> JungleMinions = new List<Obj_AI_Base>();

        public Menu Config { get; set; }
        public Menu ComboConfig { get; set; }
        public Menu HarassConfig { get; set; }
        public Menu DrawConfig { get; set; }
        public Menu FarmConfig { get; set; }
        public Menu BonusConfig { get; set; }
        public Menu ItemConfig { get; set; }
        public Menu SummonerConfig { get; set; }

        public List<Spell> SpellList = new List<Spell>();

        public static Items.Item Hydra;
        public static Items.Item Tiamat;
        public static Items.Item DFG;

        public void castItems(Obj_AI_Base target, bool isMinion = false)
        {
            return;
            if (isMinion)
            {
                if (Player.Distance(target) <= Hydra.Range && Config.Item("hdr").GetValue<bool>())
                {
                    Hydra.Cast(target);
                }
                if (Player.Distance(target) <= Tiamat.Range && Config.Item("tia").GetValue<bool>())
                {
                    Tiamat.Cast(target);
                }
            }
            else
            {
                if (Player.Distance(target) <= Hydra.Range && Config.Item("hdr").GetValue<bool>())
                {
                    Hydra.Cast(target);
                }
                if (Player.Distance(target) <= Tiamat.Range && Config.Item("tia").GetValue<bool>())
                {
                    Tiamat.Cast(target);
                }
                if (target.IsValidTarget(DFG.Range) && Config.Item("dfg").GetValue<bool>())
                {
                    DFG.Cast(target);
                }
            }
        }


        public static SpellSlot IgniteSlot;

        protected PluginBase(Version version)
        {
            ChampName = Player.ChampionName;
            Version = version;

            Game.PrintChat("Sigma" + ChampName + " Loaded. Version: " + Version.ToString() + " - By Fluxy");

            Utility.DelayAction.Add(250, () =>
            {
                SpellList.Add(Q);
                SpellList.Add(W);
                SpellList.Add(E);
                SpellList.Add(R);
            });

            Hydra = new Items.Item(3074, 175f);
            Tiamat = new Items.Item(3077, 175f);
            DFG = new Items.Item(3128, 750f);


            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            createConfigs();
            eventsLoad();
            extraEvents();
            addOW();
        }

        private void eventsLoad()
        {
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalking.BeforeAttack += BeforeAttack;
            Orbwalking.AfterAttack += AfterAttack;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt +=Interrupter_OnPossibleToInterrupt;
        }

        private void extraEvents()
        {
            Game.OnUpdate += args =>
            {
                JungleMinions = MinionManager.GetMinions(Player.Position, 800, MinionTypes.All, MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth).ToList();
                var Target = TargetSelector.GetTarget(600, TargetSelector.DamageType.Magical);
                if (Target != null && IgniteSlot != SpellSlot.Unknown 
                    && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready 
                    && ObjectManager.Player.GetSummonerSpellDamage(Target, Damage.SummonerSpell.Ignite) > Target.Health
                    && Config.Item("IGNks").GetValue<bool>())
                {
                    Player.Spellbook.CastSpell(IgniteSlot, Target);
                }
            };

            Drawing.OnDraw += args =>
            {
                foreach (var spell in SpellList.Where(s => s != null))
                {
                    var menuItem = Config.Item(spell.Slot + "Range" + ChampName).GetValue<Circle>();
                    if (menuItem.Active && spell.Level > 0 && spell.IsReady())
                        Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            };
        }

        private void createConfigs()
        {
            Config = new Menu("SigmaSeries - " + Player.ChampionName, "SigmaSeries - " + Player.ChampionName, true);
            var tsMenu = Config.AddSubMenu(new Menu("TargetSelector", "TargetSelector"));
            TargetSelector.AddToMenu(tsMenu);
            ComboConfig = Config.AddSubMenu(new Menu("Combo", "Combo"));
            HarassConfig = Config.AddSubMenu(new Menu("Harass", "Harass"));
            FarmConfig = Config.AddSubMenu(new Menu("Farm", "Farm"));
            BonusConfig = Config.AddSubMenu(new Menu("Extra", "Extra"));

            ItemConfig = Config.AddSubMenu(new Menu("Item Configs", "Item Configs"));

            SummonerConfig = Config.AddSubMenu(new Menu("Summoner Configs", "Summoner Configs"));
           

            DrawConfig = Config.AddSubMenu(new Menu("Draw", "Draw"));
            DrawConfig.AddItem(new MenuItem("QRange" + ChampName, "Q Range").SetValue(new Circle(false, System.Drawing.Color.Green)));
            DrawConfig.AddItem(new MenuItem("WRange" + ChampName, "W Range").SetValue(new Circle(false, System.Drawing.Color.Green)));
            DrawConfig.AddItem(new MenuItem("ERange" + ChampName, "E Range").SetValue(new Circle(false, System.Drawing.Color.Green)));
            DrawConfig.AddItem(new MenuItem("RRange" + ChampName, "R Range").SetValue(new Circle(false, System.Drawing.Color.Green)));

            ComboMenu(ComboConfig);
            HarassMenu(HarassConfig);
            FarmMenu(FarmConfig);
            BonusMenu(BonusConfig);
            DrawingMenu(DrawConfig);
            ItemMenu(ItemConfig);
            SummonerMenu(SummonerConfig);
            
            Config.AddToMainMenu();
        }

        public virtual void SummonerMenu(Menu SummonerMenu)
        {
            var subIGNITE = SummonerConfig.AddSubMenu(new Menu("Ignite", "Ignite"));
            subIGNITE.AddItem(new MenuItem("IGNks", "Use Ignite to KS").SetValue(true));
        }

        public void addOW()
        {
            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking")));
        }
        public virtual void ComboMenu(Menu config)
        {

        }
        public virtual void ItemMenu(Menu config)
        {
            var subAD = ItemConfig.AddSubMenu(new Menu("AD Items", "AD Items"));

            subAD.AddItem(new MenuItem("hdr", "Use Hydra").SetValue(true));
            subAD.AddItem(new MenuItem("tia", "Use Tiamat").SetValue(true));

            var subAP = ItemConfig.AddSubMenu(new Menu("AP Items", "AP Items"));

            subAP.AddItem(new MenuItem("dfg", "Use Deathfire Grasp").SetValue(true));

            ItemConfig.AddItem(new MenuItem("UseItems", "Use Items").SetValue(true));
        }
        public virtual void HarassMenu(Menu config)
        {
        }
        public virtual void FarmMenu(Menu config)
        {
        }
        public virtual void BonusMenu(Menu config)
        {
        }
        public virtual void DrawingMenu(Menu config)
        {
        }
        public virtual void OnDraw(EventArgs args)
        {
        }
        public virtual void OnUpdate(EventArgs args)
        {
        }
        public virtual void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
        }
        public virtual void AfterAttack(AttackableUnit unit, AttackableUnit target) 
        {
        }
        public virtual void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
        }
        public virtual void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
        }

    }
}