using System;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;
using SCommon.Orbwalking;
using SCommon.Evade;
using SCommon.Prediction;
using SharpDX;
using SharpDX.Direct3D9;
//typedefs
using Prediction = SCommon.Prediction.Prediction;
using Geometry = SCommon.Maths.Geometry;
using Color = System.Drawing.Color;
using TargetSelector = SCommon.TS.TargetSelector;

namespace SCommon.PluginBase
{
    public abstract class Champion : IChampion
    {
        public const int Q = 0, W = 1, E = 2, R = 3;

        public Menu ConfigMenu, DrawingMenu;
        public Orbwalking.Orbwalker Orbwalker;
        public Spell[] Spells = new Spell[4];
        public Font Text;

        public delegate void dVoidDelegate();
        public dVoidDelegate OnUpdate, OnDraw, OnCombo, OnHarass, OnLaneClear, OnLastHit;

        public Champion(string szChampName, string szMenuName, bool enableRangeDrawings = true, bool enableEvader = true)
        {
            Text = new Font(Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Malgun Gothic",
                    Height = 15,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearTypeNatural
                });

            ConfigMenu = new Menu(szMenuName, szChampName, true);

            TargetSelector.Initialize(ConfigMenu);
            Orbwalker = new Orbwalking.Orbwalker(ConfigMenu);

            SetSpells();

            DrawingMenu = new Menu("Drawings", "drawings");
            if (enableRangeDrawings)
            {
                if (this.Spells[0] != null && this.Spells[0].Range > 0)
                    this.DrawingMenu.AddItem(new MenuItem("DDRAWQ", "Draw Q").SetValue(new Circle(true, Color.Red, this.Spells[0].Range)));

                if (this.Spells[1] != null && this.Spells[1].Range > 0)
                    this.DrawingMenu.AddItem(new MenuItem("DDRAWW", "Draw W").SetValue(new Circle(true, Color.Aqua, this.Spells[1].Range)));

                if (this.Spells[2] != null && this.Spells[2].Range > 0)
                    this.DrawingMenu.AddItem(new MenuItem("DDRAWE", "Draw E").SetValue(new Circle(true, Color.Bisque, this.Spells[2].Range)));

                if (this.Spells[3] != null && this.Spells[3].Range > 0 && this.Spells[3].Range < 3000) //global ult ?
                    this.DrawingMenu.AddItem(new MenuItem("DDRAWR", "Draw R").SetValue(new Circle(true, Color.Chartreuse, this.Spells[3].Range)));
            }
            ConfigMenu.AddSubMenu(DrawingMenu);

            if (enableEvader)
            {
                Menu evaderMenu = null;
                Evader evader;
                switch (szChampName.ToLower())
                {
                    case "ezreal":
                        evader = new Evader(out evaderMenu, Database.EvadeMethods.Blink, Spells[E]);
                        break;
                    case "sivir":
                    case "morgana":
                        evader = new Evader(out evaderMenu, Database.EvadeMethods.SpellShield, Spells[E]);
                        break;
                    case "fizz":
                        evader = new Evader(out evaderMenu, Database.EvadeMethods.Dash, Spells[E]);
                        break;
                    case "lissandra":
                        evader = new Evader(out evaderMenu, Database.EvadeMethods.Invulnerability, Spells[R]);
                        break;
                    case "nocturne":
                        evader = new Evader(out evaderMenu, Database.EvadeMethods.SpellShield, Spells[W]);
                        break;
                    case "vladimir":
                        evader = new Evader(out evaderMenu, Database.EvadeMethods.Invulnerability, Spells[W]);
                        break;
                    case "graves":
                    case "gnar":
                    case "lucian":
                    case "riven":
                    case "shen":
                        evader = new Evader(out evaderMenu, Database.EvadeMethods.Dash, Spells[E]);
                        break;
                    case "zed":
                    case "leblanc":
                        evader = new Evader(out evaderMenu, Database.EvadeMethods.Dash, Spells[W]);
                        break;
                    case "vayne":
                        evader = new Evader(out evaderMenu, Database.EvadeMethods.Dash, Spells[Q]);
                        break;
                }
                if (evaderMenu != null)
                    ConfigMenu.AddSubMenu(evaderMenu);
            }
            CreateConfigMenu();

            #region Events
            Game.OnUpdate += this.Game_OnUpdate;
            Drawing.OnDraw += this.Drawing_OnDraw;
            Orbwalking.Events.BeforeAttack += this.Orbwalking_BeforeAttack;
            Orbwalking.Events.AfterAttack += this.Orbwalking_AfterAttack;
            AntiGapcloser.OnEnemyGapcloser += this.AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += this.Interrupter_OnPossibleToInterrupt;
            Obj_AI_Base.OnBuffAdd += this.Obj_AI_Base_OnBuffAdd;
            Obj_AI_Base.OnProcessSpellCast += this.Obj_AI_Base_OnProcessSpellCast;
            CustomEvents.Unit.OnDash += Unit_OnDash;
            #endregion

            Prediction.Prediction.Initialize(ConfigMenu);
        }

        public virtual void CreateConfigMenu()
        {
            ConfigMenu.AddToMainMenu();
        }

        public virtual void SetSpells()
        {
            Spells[Q] = new Spell(SpellSlot.Q);
            Spells[W] = new Spell(SpellSlot.W);
            Spells[E] = new Spell(SpellSlot.E);
            Spells[R] = new Spell(SpellSlot.R);
        }

        public virtual void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling() || args == null)
                return;

            if (OnUpdate != null)
                OnUpdate();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.Orbwalker.Mode.Combo:
                    {
                        if (OnCombo != null)
                            OnCombo();
                    }
                    break;
                case Orbwalking.Orbwalker.Mode.Mixed:
                    {
                        if (OnHarass != null)
                            OnHarass();
                    }
                    break;
                case Orbwalking.Orbwalker.Mode.LaneClear:
                    {
                        if (OnLaneClear != null)
                            OnLaneClear();
                    }
                    break;
                case Orbwalking.Orbwalker.Mode.LastHit:
                    {
                        if (OnLastHit != null)
                            OnLastHit();
                    }
                    break;

            }
        }

        public virtual void Drawing_OnDraw(EventArgs args)
        {
            if (OnDraw != null)
                OnDraw();

            foreach (MenuItem it in DrawingMenu.Items)
            {
                Circle c = it.GetValue<Circle>();
                if (c.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, c.Radius, c.Color, 2);
            }
        }

        protected virtual void Orbwalking_BeforeAttack(BeforeAttackArgs args)
        {
            //
        }

        protected virtual void Orbwalking_AfterAttack(AfterAttackArgs args)
        {
            //
        }

        protected virtual void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            //
        }

        protected virtual void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            //
        }

        protected virtual void Obj_AI_Base_OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            //
        }

        protected virtual void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            //
        }

        protected virtual void Unit_OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            //
        }

        /// <summary>
        /// Checks if combo is ready
        /// </summary>
        /// <returns>true if combo is ready</returns>
        public bool ComboReady()
        {
            return Spells[Q].IsReady() && Spells[W].IsReady() && Spells[E].IsReady() && Spells[R].IsReady();
        }

        #region Damage Calculation Funcitons
        /// <summary>
        /// Calculates combo damage to given target
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="aacount">Auto Attack Count</param>
        /// <returns>Combo damage</returns>
        public double CalculateComboDamage(Obj_AI_Hero target, int aacount = 2)
        {
            return CalculateSpellDamage(target) + CalculateSummonersDamage(target) + CalculateItemsDamage(target) + CalculateAADamage(target, aacount);
        }

        /// <summary>
        /// Calculates Spell Q damage to given target
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>Spell Q Damage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateDamageQ(Obj_AI_Hero target)
        {
            if (Spells[Q].IsReady())
                return ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);

            return 0.0;
        }

        /// <summary>
        /// Calculates Spell W damage to given target
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>Spell W Damage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateDamageW(Obj_AI_Hero target)
        {
            if (Spells[W].IsReady())
                return ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);

            return 0.0;
        }

        /// <summary>
        /// Calculates Spell E damage to given target
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>Spell E Damage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateDamageE(Obj_AI_Hero target)
        {
            if (Spells[E].IsReady())
                return ObjectManager.Player.GetSpellDamage(target, SpellSlot.E);

            return 0.0;
        }

        /// <summary>
        /// Calculates Spell R damage to given target
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>Spell R Damage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateDamageR(Obj_AI_Hero target)
        {
            if (Spells[R].IsReady())
                return ObjectManager.Player.GetSpellDamage(target, SpellSlot.R);

            return 0.0;
        }

        /// <summary>
        /// Calculates all spell's damage to given target
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>All spell's damage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CalculateSpellDamage(Obj_AI_Hero target)
        {
            return CalculateDamageQ(target) + CalculateDamageW(target) + CalculateDamageE(target) + CalculateDamageR(target);
        }

        /// <summary>
        /// Calculates summoner spell damages to given target
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>Summoner spell damage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CalculateSummonersDamage(Obj_AI_Hero target)
        {
            var ignite = ObjectManager.Player.GetSpellSlot("summonerdot");
            if (ignite != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(ignite) == SpellState.Ready && ObjectManager.Player.Distance(target, false) < 550)
                return ObjectManager.Player.GetSummonerSpellDamage(target, LeagueSharp.Common.Damage.SummonerSpell.Ignite); //ignite

            return 0.0;
        }

        /// <summary>
        /// Calculates Item's active damages to given target
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>Item's damage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateItemsDamage(Obj_AI_Hero target)
        {
            double dmg = 0.0;

            if (Items.CanUseItem(3144) && ObjectManager.Player.Distance(target, false) < 550)
                dmg += ObjectManager.Player.GetItemDamage(target, LeagueSharp.Common.Damage.DamageItems.Bilgewater); //bilgewater cutlass

            if (Items.CanUseItem(3153) && ObjectManager.Player.Distance(target, false) < 550)
                dmg += ObjectManager.Player.GetItemDamage(target, LeagueSharp.Common.Damage.DamageItems.Botrk); //botrk

            if (Items.HasItem(3057))
                dmg += ObjectManager.Player.CalcDamage(target, LeagueSharp.Common.Damage.DamageType.Magical, ObjectManager.Player.BaseAttackDamage); //sheen

            if (Items.HasItem(3100))
                dmg += ObjectManager.Player.CalcDamage(target, LeagueSharp.Common.Damage.DamageType.Magical, (0.75 * ObjectManager.Player.BaseAttackDamage) + (0.50 * ObjectManager.Player.FlatMagicDamageMod)); //lich bane

            if (Items.HasItem(3285))
                dmg += ObjectManager.Player.CalcDamage(target, LeagueSharp.Common.Damage.DamageType.Magical, 100 + (0.1 * ObjectManager.Player.FlatMagicDamageMod)); //luden

            return dmg;

        }

        /// <summary>
        /// Calculates Auto Attack damage to given target
        /// </summary>
        /// <param name="target">Targetparam>
        /// <param name="aacount">Auto Attack count</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateAADamage(Obj_AI_Hero target, int aacount = 2)
        {
            return Damage.AutoAttack.GetDamage(target) * aacount;
        }
        #endregion
    }
}
