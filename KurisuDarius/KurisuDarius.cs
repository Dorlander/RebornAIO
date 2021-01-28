using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using KL = KurisuDarius.KurisuLib;

namespace KurisuDarius
{
    internal class KurisuDarius
    {
        internal static Menu Config;
        internal static int LastGrabTimeStamp;
        internal static int LastDunkTimeStamp;
        internal static HpBarIndicator HPi = new HpBarIndicator();
        internal static Orbwalking.Orbwalker Orbwalker;

        public KurisuDarius()
        {
            if (ObjectManager.Player.ChampionName == "Darius")
            {
                Menu_OnLoad();

                // On Update Event
                Game.OnUpdate += Game_OnUpdate;

                // On Draw Event
                Drawing.OnDraw += Drawing_OnDraw;
                Drawing.OnEndScene += Drawing_OnEndScene;

                // After Attack Event
                Orbwalking.AfterAttack += Orbwalking_AfterAttack;

                // On Spell Cast Event
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
                CustomEvents.Unit.OnDash += Unit_OnDash;

                // Interrupter
                Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            }
        }

        internal static void Unit_OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            if (!Config.Item("useeflee").GetValue<bool>())
            {
                return;
            }

            var hero = sender as Obj_AI_Hero;
            if (hero != null && hero.IsEnemy && hero.Distance(args.EndPos) < KL.Player.Distance(args.EndPos))
            {
                if (hero.IsValidTarget(KL.Spellbook["E"].Range))
                {
                    if (KL.Spellbook["E"].IsReady())
                    {
                        KL.Spellbook["E"].Cast(args.EndPos);
                    }
                }
            }
        }

        internal static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender.IsValidTarget(KL.Spellbook["E"].Range) && KL.Spellbook["E"].IsReady())
            {
                if (Config.Item("useeint").GetValue<bool>())
                {
                    KL.Spellbook["E"].Cast(sender.ServerPosition);
                }
            }
        }

        internal static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            switch (args.SData.Name.ToLower())
            {
                case "dariuscleave":
                    Utility.DelayAction.Add(Game.Ping + 800, Orbwalking.ResetAutoAttackTimer);
                    break;

                case "dariusaxegrabcone":
                    LastGrabTimeStamp = Utils.GameTimeTickCount;
                    Utility.DelayAction.Add(Game.Ping + 100, Orbwalking.ResetAutoAttackTimer);
                    break;

                case "dariusexecute":
                    LastDunkTimeStamp = Utils.GameTimeTickCount;
                    Utility.DelayAction.Add(Game.Ping + 350, Orbwalking.ResetAutoAttackTimer);
                    break;
            }
        }

        internal static float RModifier => Config.Item("rmodi").GetValue<Slider>().Value;

        internal static float MordeShield(Obj_AI_Hero unit)
        {
            if (unit.ChampionName != "Mordekaiser")
            {
                return 0f;
            }

            return unit.Mana;
        }

        internal static int PassiveCount(Obj_AI_Base unit)
        {
            return unit.GetBuffCount("dariushemo") > 0 ? unit.GetBuffCount("dariushemo") : 0;
        }

        internal static void Drawing_OnEndScene(EventArgs args)
        {
            if (!Config.Item("drawfill").GetValue<bool>() || KL.Player.IsDead)
            {
                return;
            }

            foreach (var enemy in HeroManager.Enemies.Where(ene => ene.IsValidTarget() && ene.IsHPBarRendered))
            {
                HPi.unit = enemy;
                HPi.drawDmg(KL.RDmg(enemy, PassiveCount(enemy)), new ColorBGRA(255, 255, 0, 90));
            }
        }

        internal static void Game_OnUpdate(EventArgs args)
        {
            if (KL.Spellbook["R"].IsReady() && Config.Item("ksr").GetValue<bool>())
            {
                foreach (var unit in HeroManager.Enemies.Where(ene => ene.IsValidTarget(KL.Spellbook["R"].Range) && !ene.IsZombie))
                {
                    if (unit.CountEnemiesInRange(1200) <= 1 && Config.Item("ksr1").GetValue<bool>())
                    {
                        if (KL.RDmg(unit, PassiveCount(unit)) + RModifier + 
                            KL.Hemorrhage(unit, PassiveCount(unit) - 1) >= unit.Health + MordeShield(unit))
                        {
                            if (!TargetSelector.IsInvulnerable(unit, TargetSelector.DamageType.True))
                            {
                                if (!unit.HasBuff("kindredrnodeathbuff"))
                                    KL.Spellbook["R"].CastOnUnit(unit);
                            }
                        }
                    }

                    if (KL.RDmg(unit, PassiveCount(unit)) + RModifier >= unit.Health +
                        KL.Hemorrhage(unit, 1) + MordeShield(unit))
                    {
                        if (!TargetSelector.IsInvulnerable(unit, TargetSelector.DamageType.True))
                        {
                            if (!unit.HasBuff("kindredrnodeathbuff"))
                                KL.Spellbook["R"].CastOnUnit(unit);
                        }
                    }
                }
            }

            if (Config.Item("useca").GetValue<KeyBind>().Active)
            {
                Combo(Config.Item("useq").GetValue<bool>(), Config.Item("usew").GetValue<bool>(),
                    Config.Item("usee").GetValue<bool>(), Config.Item("user").GetValue<bool>());
            }

            if (Config.Item("useha").GetValue<KeyBind>().Active)
            {
                Harass();
            }

            if (Config.Item("caste").GetValue<KeyBind>().Active)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                Combo(false, false, true, false);
            }
        }

        internal static void Drawing_OnDraw(EventArgs args)
        {
            if (KL.Player.IsDead)
            {
                return;
            }

            var acircle = Config.Item("drawe").GetValue<Circle>();
            if (acircle.Active)
                Render.Circle.DrawCircle(KL.Player.Position, KL.Spellbook["E"].Range, acircle.Color, 3);

            var rcircle = Config.Item("drawr").GetValue<Circle>();
            if (rcircle.Active)
                Render.Circle.DrawCircle(KL.Player.Position, KL.Spellbook["R"].Range, rcircle.Color, 3);

            var qcircle = Config.Item("drawq").GetValue<Circle>();
            if (qcircle.Active)
                Render.Circle.DrawCircle(KL.Player.Position, KL.Spellbook["Q"].Range, qcircle.Color, 3);

            if (!Config.Item("drawstack").GetValue<bool>())
            {
                return;
            }

            var plaz = Drawing.WorldToScreen(KL.Player.Position); // player z axis
            if (KL.Player.GetBuffCount("dariusexecutemulticast") > 0)
            {
                var executetime = KL.Player.GetBuff("dariusexecutemulticast").EndTime - Game.Time;
                Drawing.DrawText(plaz[0] - 15, plaz[1] + 55, System.Drawing.Color.OrangeRed, executetime.ToString("0.0"));
            }

            foreach (var enemy in HeroManager.Enemies.Where(ene => ene.IsValidTarget() && !ene.IsZombie))
            {
                var enez = Drawing.WorldToScreen(enemy.Position); // enemy z axis
                if (enemy.GetBuffCount("dariushemo") > 0)
                {
                    var endtime = enemy.GetBuff("dariushemo").EndTime - Game.Time;
                    Drawing.DrawText(enez[0] - 50, enez[1], System.Drawing.Color.OrangeRed,  "Stack Count: " + enemy.GetBuffCount("dariushemo"));
                    Drawing.DrawText(enez[0] - 25, enez[1] + 20, System.Drawing.Color.OrangeRed, endtime.ToString("0.0"));
                }
            }
        }


        internal static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var hero = unit as Obj_AI_Hero;
            if (hero == null || !hero.IsValid<Obj_AI_Hero>() || hero.Type != GameObjectType.obj_AI_Hero ||
                Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
            {
                return;
            }

            if (KL.Spellbook["R"].IsReady() && KL.Player.Mana - KL.Spellbook["W"].ManaCost > 
                KL.Spellbook["R"].ManaCost || !KL.Spellbook["R"].IsReady())
            {
                if (!hero.HasBuffOfType(BuffType.Slow) || Config.Item("wwww").GetValue<bool>())
                    KL.Spellbook["W"].Cast();
            }

            if (!KL.Spellbook["W"].IsReady() && Config.Item("iiii").GetValue<bool>())
            {
                KL.HandleItems();
            }
        }

        internal static bool CanQ(Obj_AI_Base unit)
        {
            if (!unit.IsValidTarget() || unit.IsZombie ||
                TargetSelector.IsInvulnerable(unit, TargetSelector.DamageType.Physical))
            {
                return false;
            }

            if (KL.Player.Distance(unit.ServerPosition) < 175 ||
                Utils.GameTimeTickCount - LastGrabTimeStamp < 350)
            {
                return false;
            }

            if (KL.Spellbook["R"].IsReady() &&
                KL.Player.Mana - KL.Spellbook["Q"].ManaCost < KL.Spellbook["R"].ManaCost)
            {
                return false;
            }

            if (KL.Spellbook["W"].IsReady() && KL.WDmg(unit) >= unit.Health &&
                unit.Distance(KL.Player.ServerPosition) <= 200)
            {
                return false;
            }

            if (KL.Spellbook["W"].IsReady() && KL.Player.HasBuff("DariusNoxonTactictsONH") &&
                unit.Distance(KL.Player.ServerPosition) <= 225)
            {
                return false;
            }

            if (KL.Player.Distance(unit.ServerPosition) > KL.Spellbook["Q"].Range)
            {
                return false;
            }

            if (KL.Spellbook["R"].IsReady() && KL.Spellbook["R"].IsInRange(unit) &&
                KL.RDmg(unit, PassiveCount(unit)) - KL.Hemorrhage(unit, 1) >= unit.Health)
            {
                return false;
            }

            if (KL.Player.GetAutoAttackDamage(unit) * 2 + KL.Hemorrhage(unit, PassiveCount(unit)) >= unit.Health &&
                KL.Player.Distance(unit.ServerPosition) <= 180)
            {
                return false;
            }

            return true;
        }

        internal static void Harass()
        {
            if (Config.Item("harassq").GetValue<bool>() && KL.Spellbook["Q"].IsReady())
            {
                if (KL.Player.Mana / KL.Player.MaxMana * 100 > 60)
                {
                    if (CanQ(TargetSelector.GetTarget(KL.Spellbook["E"].Range, TargetSelector.DamageType.Physical)))
                    {
                        KL.Spellbook["Q"].Cast();
                    }
                }
            }   
        }

        internal static void Combo(bool useq, bool usew, bool usee, bool user)
        {
            if (useq && KL.Spellbook["Q"].IsReady())
            {
                if (CanQ(TargetSelector.GetTarget(KL.Spellbook["E"].Range, TargetSelector.DamageType.Physical)))
                {
                    KL.Spellbook["Q"].Cast();
                }
            }

            if (usew && KL.Spellbook["W"].IsReady())
            {
                var wtarget = TargetSelector.GetTarget(KL.Spellbook["E"].Range, TargetSelector.DamageType.Physical);
                if (wtarget.IsValidTarget(KL.Spellbook["W"].Range) && !wtarget.IsZombie)
                {
                    if (wtarget.Distance(KL.Player.ServerPosition) <= 200 && KL.WDmg(wtarget) >= wtarget.Health)
                    {
                        if (Utils.GameTimeTickCount - LastDunkTimeStamp >= 500)
                        {
                            KL.Spellbook["W"].Cast();
                        }
                    }
                }
            }

            if (usee && KL.Spellbook["E"].IsReady())
            {
                var etarget = TargetSelector.GetTarget(KL.Spellbook["E"].Range, TargetSelector.DamageType.Physical);
                if (etarget.IsValidTarget())
                {
                    if (etarget.Distance(KL.Player.ServerPosition) > 250)
                    {
                        if (KL.Player.CountAlliesInRange(1000) >= 1)
                            KL.Spellbook["E"].Cast(etarget.ServerPosition);

                        if (KL.RDmg(etarget, PassiveCount(etarget)) - KL.Hemorrhage(etarget, 1) >= etarget.Health)
                            KL.Spellbook["E"].Cast(etarget.ServerPosition);

                        if (KL.Spellbook["Q"].IsReady() || KL.Spellbook["W"].IsReady())
                            KL.Spellbook["E"].Cast(etarget.ServerPosition);

                        if (KL.Player.GetAutoAttackDamage(etarget) + KL.Hemorrhage(etarget, 3) * 3 >= etarget.Health)
                            KL.Spellbook["E"].Cast(etarget.ServerPosition);
                    }           
                }
            }

            if (user && KL.Spellbook["R"].IsReady())
            {
                var unit = TargetSelector.GetTarget(KL.Spellbook["E"].Range, TargetSelector.DamageType.Physical);

                if (unit.IsValidTarget(KL.Spellbook["R"].Range) && !unit.IsZombie)
                {
                    if (!unit.HasBuffOfType(BuffType.Invulnerability) && !unit.HasBuffOfType(BuffType.SpellShield))
                    {
                        if (KL.RDmg(unit, PassiveCount(unit)) + RModifier >= unit.Health +
                            KL.Hemorrhage(unit, 1) + MordeShield(unit))
                        {
                            if (!TargetSelector.IsInvulnerable(unit, TargetSelector.DamageType.True))
                            {
                                if (!unit.HasBuff("kindredrnodeathbuff"))
                                    KL.Spellbook["R"].CastOnUnit(unit);
                            }
                        }
                    }
                }
            }
        }

        internal static void Menu_OnLoad()
        {
            Config = new Menu("Kurisu's Darius", "darius", true);

            var omenu = new Menu(":: Orbwalker", "omenu");
            Orbwalker = new Orbwalking.Orbwalker(omenu);
            Config.AddSubMenu(omenu);

            var drmenu = new Menu(":: Draw Settings", "drawings");
            drmenu.AddItem(new MenuItem("drawe", ":: Draw E"))
                .SetValue(new Circle(false, System.Drawing.Color.FromArgb(150, System.Drawing.Color.Red)));
            drmenu.AddItem(new MenuItem("drawq", ":: Draw Q"))
                .SetValue(new Circle(false, System.Drawing.Color.FromArgb(150, System.Drawing.Color.Red)));
            drmenu.AddItem(new MenuItem("drawr", ":: Draw R"))
                .SetValue(new Circle(false, System.Drawing.Color.FromArgb(150, System.Drawing.Color.White)));
            drmenu.AddItem(new MenuItem("drawfill", ":: Draw R Damage Fill")).SetValue(true);
            drmenu.AddItem(new MenuItem("drawstack", ":: Draw Stack Count")).SetValue(true);
            Config.AddSubMenu(drmenu);

            var rmenu = new Menu(":: Ultimate Settings ", "rmenu");
            rmenu.AddItem(new MenuItem("user", ":: Use R in combo")).SetValue(true);
            rmenu.AddItem(new MenuItem("ksr", ":: Use R auto (no key)")).SetValue(true);
            rmenu.AddItem(new MenuItem("ksr1", ":: Use R early (1v1)"))
                .SetValue(false)
                .SetTooltip("Use early if target will bleed to death");
            //rmenu.AddItem(new MenuItem("userlast", "Use R before buff expiry"))
            //    .SetValue(true)
            //    .SetTooltip("After a successful ult, will not waste R if buff will Expire");
            rmenu.AddItem(new MenuItem("rmodi", ":: Adjust R damage"))
                .SetValue(new Slider(0, -250, 250))
                .SetTooltip("Lower it if the target is living with a slither of health.");
            Config.AddSubMenu(rmenu);   

            var cmenu = new Menu(":: Darius Settings", "cmenu");
            cmenu.AddItem(new MenuItem("iiii", ":: Hydra/Tiamat/Titanic")).SetValue(true);
            cmenu.AddItem(new MenuItem("usee", ":: Use E in combo")).SetValue(true);
            cmenu.AddItem(new MenuItem("useq", ":: Use Q in combo")).SetValue(true);
            cmenu.AddItem(new MenuItem("harassq", ":: Use Q in harass")).SetValue(true);
            cmenu.AddItem(new MenuItem("usew", ":: Use W after attack")).SetValue(true);
            cmenu.AddItem(new MenuItem("wwww", ":: Use W on slowed targets")).SetValue(true);
            cmenu.AddItem(new MenuItem("useeint", ":: Auto E interrupt")).SetValue(true);
            cmenu.AddItem(new MenuItem("useeflee", ":: Auto E fleeing targets"))
                .SetValue(true)
                .SetTooltip("Will pull targets in who use a spell to flee.");
            Config.AddSubMenu(cmenu);

            Config.AddItem(new MenuItem("useca", ":: Combo [active]")).SetValue(new KeyBind(32, KeyBindType.Press));
            Config.AddItem(new MenuItem("useha", ":: Harass [active]")).SetValue(new KeyBind('C', KeyBindType.Press));
            Config.AddItem(new MenuItem("caste", ":: Cast Assisted E [semi]")).SetValue(new KeyBind('T', KeyBindType.Press));

            Config.AddToMainMenu();
        }
    }
}
