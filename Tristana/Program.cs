using System;
using System.Linq;
using LeagueSharp.Common;
using LeagueSharp;

namespace Tristana
{
    class Program
    {

        internal static Menu Root;
        internal static Spell Q, W, E, R;
        internal static Orbwalking.Orbwalker Orbwalker;

        internal static float TargetRange;
        internal static float PlayerRange;
        internal static Obj_AI_Hero Player => ObjectManager.Player;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Tristana")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);
            W = new Spell(SpellSlot.W, 900f);

            Root = new Menu("Tristana", "tristana", true);

            var ormenu = new Menu("Orbwalk", "ormenu");
            Orbwalker = new Orbwalking.Orbwalker(ormenu);
            Root.AddSubMenu(ormenu);

            var kemenu = new Menu("Keys", "kemenu");
            kemenu.AddItem(new MenuItem("usecombo", "Combo [active]")).SetValue(new KeyBind(32, KeyBindType.Press));
            kemenu.AddItem(new MenuItem("useflee", "Flee [active]")).SetValue(new KeyBind('Z', KeyBindType.Press));
            Root.AddSubMenu(kemenu);

            var comenu = new Menu("Combo", "cmenu");

            var tcmenu = new Menu("Config", "tcmenu");

            var whemenu = new Menu("E Whitelist", "whemenu");
            foreach (var hero in HeroManager.Enemies)
                whemenu.AddItem(new MenuItem("whe" + hero.ChampionName, hero.ChampionName))
                    .SetValue(true).SetTooltip("E on " + hero.ChampionName);

            tcmenu.AddItem(new MenuItem("autor", "R Killsteal")).SetValue(true);
            tcmenu.AddItem(new MenuItem("efocus", "Focus E Target")).SetValue(true);
            tcmenu.AddItem(new MenuItem("efinish", "E + R Finisher")).SetValue(true);
            tcmenu.AddItem(new MenuItem("mine", "-> Min E Stacks"))
                .SetValue(new Slider(2, 1, 3)).SetTooltip("The Min E Stacks Before Pushing the Target Away");
            tcmenu.AddSubMenu(whemenu);

            comenu.AddSubMenu(tcmenu);

            comenu.AddItem(new MenuItem("useqcombo", "Use Q")).SetValue(true);
            comenu.AddItem(new MenuItem("useecombo", "Use E")).SetValue(true);
            comenu.AddItem(new MenuItem("usercombo", "Use R")).SetValue(true);
            Root.AddSubMenu(comenu);

            var fmenu = new Menu("Flee", "fmenu");
            Root.AddSubMenu(fmenu);

            var exmenu = new Menu("Extra", "exmenu");
            exmenu.AddItem(new MenuItem("interrupt", "Interrupter")).SetValue(false);
            exmenu.AddItem(new MenuItem("gap", "Anti-Gapcloser")).SetValue(false);
            Root.AddSubMenu(exmenu);

          /*  var skmenu = new Menu("Skins", "skmenu");
            var skinitem = new MenuItem("useskin", "Enabled");
            skmenu.AddItem(skinitem).SetValue(false);

            skinitem.ValueChanged += (sender, eventArgs) =>
            {
                if (!eventArgs.GetNewValue<bool>())
                {
                    ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, ObjectManager.Player.BaseSkinId);
                }
            };

            skmenu.AddItem(new MenuItem("skinid", "Skin Id")).SetValue(new Slider(10, 0, 12));
            Root.AddSubMenu(skmenu); */

            var drmenu = new Menu("Drawings", "drmenu");
            drmenu.AddItem(new MenuItem("drawe", "Draw E")).SetValue(false);
            drmenu.AddItem(new MenuItem("draww", "Draw W")).SetValue(false);
            Root.AddSubMenu(drmenu);

            Root.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            Game.PrintChat("<b>Tristana#</b> - Loaded!");

            if (Menu.GetMenu("Activator", "activator") == null &&
                Menu.GetMenu("ElUtilitySuite", "ElUtilitySuite") == null &&
                Menu.GetMenu("adcUtility", "adcUtility") == null &&
                Menu.GetMenu("NabbActivator", "nabbactivator.menu") == null &&
                Menu.GetMenu("Slutty Utility", "Slutty Utility") == null &&
                Menu.GetMenu("MActivator", "masterActivator") == null)
            {
                Game.PrintChat("<font color=\"#FFF280\">Wooa</font>! you aren't using any activator. " +
                               "How about trying <b>Activator#</b> :^)");
            }
        }

        private static bool ekappa()
        {
            return Root.SubMenu("cmenu").SubMenu("tcmenu").SubMenu("whemenu").Items.Any(i => i.GetValue<bool>()) &&
                 Player.GetEnemiesInRange(1250).Any(ez => Root.Item("whe" + ez.ChampionName).GetValue<bool>());
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            PlayerRange = Player.AttackRange + 65;
            TargetRange = PlayerRange + 200;

            if (Player.IsDead || !Orbwalking.CanMove(100))
            {
                return;
            }

            if (Root.Item("useflee").GetValue<KeyBind>().Active)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
            }

            Secure(Root.Item("autor").GetValue<bool>());

            Combo(Root.Item("useqcombo").GetValue<bool>(), Root.Item("useecombo").GetValue<bool>(),
                  Root.Item("usercombo").GetValue<bool>());

          /*  if (Root.Item("useskin").GetValue<bool>())
            {
                Player.SetSkin(Player.CharData.BaseSkinName, Root.Item("skinid").GetValue<Slider>().Value);
            }*/

            if (Root.Item("efocus").GetValue<bool>())
            {
                var ET =
                    HeroManager.Enemies.FirstOrDefault(
                        x => x.HasBuff("tristanaechargesound") || x.HasBuff("tristanaecharge"));

                if (ET.IsValidTarget() && !ET.IsZombie)
                {
                    if (Root.Item("usecombo").GetValue<KeyBind>().Active)
                    {
                        TargetSelector.SetTarget(ET);
                        Orbwalker.ForceTarget(ET);
                    }
                }
            }
        }

        private static float EDmg(Obj_AI_Base hero)
        {
            if (!hero.HasBuff("tristanaechargesound") || !Root.Item("efinish").GetValue<bool>())
            {
                return 0f;
            }

            var b = hero.HasBuff("tristanaecharge") ? hero.GetBuff("tristanaecharge").Count + 1 : 1;
            if (b < Root.Item("mine").GetValue<Slider>().Value)
            {
                return 0f;
            }

            var physdmg = Player.CalcDamage(hero, Damage.DamageType.Physical,
                new[] { 47, 57, 70, 80, 90 } [E.Level - 1]  +
               (new[] { 0.5, 0.65, 0.80, 0.95, 1.1 } [E.Level - 1] * Player.FlatPhysicalDamageMod));

            var physbonus = Player.CalcDamage(hero, Damage.DamageType.Physical,
                new[] { 14.1, 17.1, 21, 24, 27 } [E.Level - 1] +
               (new[] { 0.15, 0.195, 0.24, 0.285, 0.33 } [E.Level - 1] * Player.FlatPhysicalDamageMod));


            return (float) (physdmg + (physbonus * b));
        }

        private static void Secure(bool user)
        {
            if (user && R.IsReady())
            {
                var RT =
                    HeroManager.Enemies.FirstOrDefault(
                        x => R.GetDamage(x) > x.Health || EDmg(x) + R.GetDamage(x) > x.Health);

                if (RT.IsValidTarget() && !RT.IsZombie && Orbwalking.InAutoAttackRange(RT))
                {
                    if (EDmg(RT) + R.GetDamage(RT) > RT.Health)
                    {
                        R.CastOnUnit(RT);
                    }

                    if (R.GetDamage(RT) > RT.Health)
                    {
                        R.CastOnUnit(RT);
                    }
                }
            }
        }

        private static void Combo(bool useq, bool usee, bool user)
        {
            if (!Root.Item("usecombo").GetValue<KeyBind>().Active)
            {
                return;
            }

            if (useq && Q.IsReady())
            {
                var QT = TargetSelector.GetTarget(TargetRange, TargetSelector.DamageType.Physical);
                if (QT.IsValidTarget() && !QT.IsZombie && Orbwalking.InAutoAttackRange(QT))
                {
                    Q.Cast();
                }
            }

            if (usee && E.IsReady())
            {
                var ET = TargetSelector.GetTarget(TargetRange, TargetSelector.DamageType.Physical);
                if (ET.IsValidTarget() && !ET.IsZombie && Orbwalking.InAutoAttackRange(ET))
                {
                    if (Root.Item("whe" + ET.ChampionName).GetValue<bool>() || !ekappa())
                    {
                        E.CastOnUnit(ET);
                    }
                }
            }

            if (user && R.IsReady())
            {
                var RT = TargetSelector.GetTarget(TargetRange, TargetSelector.DamageType.Physical);
                if (RT.IsValidTarget() && !RT.IsZombie && Orbwalking.InAutoAttackRange(RT))
                {
                    if (EDmg(RT) + R.GetDamage(RT) > RT.Health)
                    {
                        R.CastOnUnit(RT);
                    }

                    if (R.GetDamage(RT) > RT.Health)
                    {
                        R.CastOnUnit(RT);
                    }
                }
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var sender = gapcloser.Sender;
            if (sender.IsEnemy && sender.IsValidTarget(250f))
            {
                if (Root.Item("gap").GetValue<bool>())
                {
                    if (R.IsReady() && !sender.HasBuffOfType(BuffType.SpellImmunity))
                    {
                        R.CastOnUnit(sender);
                    }
                }
            }
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender.IsEnemy && sender.IsValidTarget(R.Range))
            {
                if (Root.Item("interrupt").GetValue<bool>())
                {
                    if (R.IsReady())
                    {
                        R.CastOnUnit(sender);
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (Root.Item("drawe").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, Player.AttackRange + 65, E.IsReady() ? System.Drawing.Color.LawnGreen : System.Drawing.Color.Red, 2);
            }

            if (Root.Item("draww").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? System.Drawing.Color.LawnGreen : System.Drawing.Color.Red, 2);
            }
        }
    }
}
