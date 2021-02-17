using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace BadaoKingdom.BadaoChampion.BadaoYasuo
{
    public static class BadaoYasuoCombo
    {
        public static void BadaoActivate()
        {
            Game.OnUpdate += Game_OnUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (BadaoMainVariables.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;
            if (!unit.IsMe)
                return;
            if (BadaoChecker.BadaoHasTiamat())
                BadaoChecker.BadaoUseTiamat();
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (BadaoMainVariables.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;
            // freestyle
            if (BadaoYasuoVariables.TargetMode.GetValue<StringList>().SelectedIndex == 0 && BadaoYasuoHelper.CanCastSpell())
            {
                if (BadaoMainVariables.Q.IsReady() && !BadaoYasuoHelper.IsDashing())
                {
                    BadaoYasuoHelper.CastQ();
                }
                if (BadaoMainVariables.E.IsReady())
                {
                    BadaoYasuoHelper.CastE();
                }
                if (BadaoMainVariables.Q.IsReady() && ObjectManager.Player.IsDashing())
                {
                    var data = ObjectManager.Player.GetDashInfo();
                    if (BadaoYasuoHelper.Qstate() == 3 || (Utility.CountEnemiesInRange(data.EndPos.To3D(), 270) >= 2))
                    {
                        foreach (var hero in HeroManager.Enemies.Where(x => x.IsValidTarget()))
                        {
                            BadaoYasuoHelper.CastQCone(hero);
                        }
                    }
                }

            }
            // selected
            if (BadaoYasuoVariables.TargetMode.GetValue<StringList>().SelectedIndex == 1 && BadaoYasuoHelper.CanCastSpell())
            {
                if (BadaoMainVariables.Q.IsReady() && !BadaoYasuoHelper.IsDashing())
                {
                    BadaoYasuoHelper.CastQ();
                }
                if (BadaoMainVariables.E.IsReady())
                {
                    var target = BadaoYasuoHelper.GetESelector();
                    if (target.IsValidTarget())
                        BadaoYasuoHelper.CastE(target);
                }
                if ( BadaoMainVariables.Q.IsReady() && ObjectManager.Player.IsDashing())
                {
                    var data = ObjectManager.Player.GetDashInfo();
                    if (BadaoYasuoHelper.Qstate() == 3 || (Utility.CountEnemiesInRange(data.EndPos.To3D(), 270) >= 2))
                    {
                        foreach (var hero in HeroManager.Enemies.Where(x => x.IsValidTarget()))
                        {
                            BadaoYasuoHelper.CastQCone(hero);
                        }
                    }
                }
            }

            // R if hit s
            if (BadaoMainVariables.R.IsReady())
            {
                foreach (var hero in HeroManager.Enemies.Where(x => x.IsValidTarget(BadaoMainVariables.R.Range) && BadaoYasuoHelper.IsOnAir(x)))
                {
                    var count = BadaoYasuoHelper.GetRCount(hero);
                    if (BadaoYasuoVariables.ComboRHits.GetValue<Slider>().Value <= count)
                        BadaoMainVariables.R.Cast(hero);
                }
            }

            // R always
            if (BadaoMainVariables.R.IsReady())
            {
                foreach (var hero in HeroManager.Enemies.Where(x => x.IsValidTarget(BadaoMainVariables.R.Range) && BadaoYasuoHelper.IsOnAir(x)))
                {
                    if (BadaoYasuoConfig.config.Item("ComboRAlways" + hero.NetworkId).GetValue<bool>()
                        && hero.Health* 100 / hero.MaxHealth <= BadaoYasuoConfig.config.Item("ComboRAlwaysHp" + hero.NetworkId).GetValue<Slider>().Value)
                    {
                        if (hero.HasBuffOfType(BuffType.Knockback))
                            BadaoMainVariables.R.Cast(hero);
                        var buff = hero.Buffs.Where(x => x.Type == BuffType.Knockup).MaxOrDefault(x => x.EndTime);
                        if (buff != null && (buff.EndTime - Game.Time )* 1000 <= 200 + Game.Ping)
                        {
                            BadaoMainVariables.R.Cast(hero);
                        }
                    }
                }
            }
            //Ignite
            if(BadaoYasuoVariables.ComboIgnite.GetValue<bool>() && BadaoMainVariables.Ignite != SpellSlot.Unknown && BadaoMainVariables.Ignite.IsReady())
            {
                foreach (var hero in HeroManager.Enemies.Where(x => x.BadaoIsValidTarget(600) && ObjectManager.Player.GetSummonerSpellDamage(x,Damage.SummonerSpell.Ignite) >= x.Health 
                && (!Orbwalking.InAutoAttackRange(x) || HeroManager.Player.HealthPercent <= 20)))
                {
                    ObjectManager.Player.Spellbook.CastSpell(BadaoMainVariables.Ignite, hero);
                }
            }

            //stack Q
            if (BadaoYasuoVariables.ComboStackQ.GetValue<bool>() && BadaoMainVariables.Q.IsReady() && BadaoYasuoHelper.Qstate() != 3
                && !HeroManager.Enemies.Any(x => x.IsValidTarget() && BadaoMainVariables.E.IsInRange(x)))
            {
                var target = BadaoYasuoHelper.GetETargets().FirstOrDefault();
                if (target.IsValidTarget())
                {
                    BadaoMainVariables.Q.Cast(target);
                }
            }

            //gap close E
            if (BadaoMainVariables.E.IsReady() && !HeroManager.Enemies.Any(x => x.IsValidTarget()
               && x.Distance(ObjectManager.Player) <= 475))
            {
                var Etargets = BadaoYasuoHelper.GetETargets();
                var Etarget = Etargets.Where(x => x != null && BadaoYasuoHelper.GetEDashEnd(x).Distance(Game.CursorPos.To2D()) + 150 < ObjectManager.Player.Distance(Game.CursorPos))
                    .MinOrDefault(x => BadaoYasuoHelper.GetEDashEnd(x).Distance(Game.CursorPos.To2D()));
                if (Etarget != null)
                {
                    BadaoMainVariables.E.Cast(Etarget);
                }
            }
        }
    }
}
