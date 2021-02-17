using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SCommon.Database;
using SharpDX;

namespace SCommon.TS
{
    public static class TargetSelector
    {
        public enum Mode
        {
            Auto,
            LowHP,
            MostAD,
            MostAP,
            Closest,
            NearMouse,
            LessAttack,
            LessCast
        }

        private static Obj_AI_Hero s_SelectedTarget = null;
        private static Obj_AI_Hero s_LastTarget = null;
        private static int s_LastTargetSent;

        public static Obj_AI_Hero SelectedTarget
        {
            get { return s_SelectedTarget; }
        }

        static TargetSelector()
        {
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        public static void Initialize(Menu menuToAttach)
        {
            ConfigMenu.Create(menuToAttach);
        }

        public static Obj_AI_Hero GetTarget(float range, LeagueSharp.Common.TargetSelector.DamageType dmgType = LeagueSharp.Common.TargetSelector.DamageType.Physical, Vector3? _from = null)
        {
            Vector3 from = _from.HasValue ? _from.Value : ObjectManager.Player.ServerPosition;
            if (s_LastTarget == null)
            {
                var t = GetNewTarget(range, dmgType, from);
                s_LastTarget = t;
            }
            else
            {
                if (s_LastTarget.IsValidTarget(range) && Utils.TickCount - s_LastTargetSent > 250)
                    s_LastTarget = GetNewTarget(range, dmgType, from);
                else if (!s_LastTarget.IsValidTarget(range))
                    s_LastTarget = GetNewTarget(range, dmgType, from);

            }
            s_LastTargetSent = Utils.TickCount;
            return s_LastTarget;
        }

        private static Obj_AI_Hero GetNewTarget(float range, LeagueSharp.Common.TargetSelector.DamageType dmgType = LeagueSharp.Common.TargetSelector.DamageType.Physical, Vector3? _from = null)
        {
            if (range < 0)
                range = Orbwalking.Utility.GetAARange();

            if (ConfigMenu.OnlyAttackSelected)
            {
                if (s_SelectedTarget != null)
                {
                    if (s_SelectedTarget.IsValidTarget(range))
                        return s_SelectedTarget;
                    else
                        return null;
                }
            }

            if(ConfigMenu.FocusSelected)
            {
                if(s_SelectedTarget != null)
                {
                    if (s_SelectedTarget.IsValidTarget(range))
                        return s_SelectedTarget;
                }
            }

            var enemies = HeroManager.Enemies.FindAll(p => p.IsValidTarget(range + p.BoundingRadius) && !LeagueSharp.Common.TargetSelector.IsInvulnerable(p, dmgType));
            if (enemies.Count == 0)
                return null;

            Vector3 from = _from.HasValue ? _from.Value : ObjectManager.Player.ServerPosition;

            switch ((Mode)ConfigMenu.TargettingMode)
            {
                case Mode.LowHP:
                    return enemies.MinOrDefault(hero => hero.Health);

                case Mode.MostAD:
                    return enemies.MaxOrDefault(hero => hero.BaseAttackDamage + hero.FlatPhysicalDamageMod);

                case Mode.MostAP:
                    return enemies.MaxOrDefault(hero => hero.BaseAbilityDamage + hero.FlatMagicDamageMod);

                case Mode.Closest:
                    return
                        enemies.MinOrDefault(
                            hero =>
                                (_from.HasValue ? _from.Value : ObjectManager.Player.ServerPosition).Distance(
                                    hero.ServerPosition, true));

                case Mode.NearMouse:
                    return enemies.Find(hero => hero.Distance(Game.CursorPos, true) < 22500); // 150 * 150

                case Mode.Auto:
                    {
                        var possibleTargets = HeroManager.Enemies.Where(p => p.IsValidTarget(range, true, from)).OrderByDescending(q => GetTotalMultipler(q)).ToList();
                        if (possibleTargets.Count == 1)
                            return possibleTargets.First();
                        else if (possibleTargets.Count > 1)
                        {
                            var killableTarget = possibleTargets.OrderByDescending(p => GetTotalADAPMultipler(p)).FirstOrDefault(q => GetHealthMultipler(q) >= 10);
                            if (killableTarget != null)
                                return killableTarget;

                            var targets = possibleTargets.OrderByDescending(p => ObjectManager.Player.Distance(p.ServerPosition));
                            Obj_AI_Hero mostImportant = null;
                            double mostImportantsDamage = 0;
                            foreach (var target in targets)
                            {
                                double dmg = target.CalcDamage(ObjectManager.Player, LeagueSharp.Common.Damage.DamageType.Physical, 100) + target.CalcDamage(ObjectManager.Player, LeagueSharp.Common.Damage.DamageType.Magical, 100);
                                if (mostImportant == null)
                                {
                                    mostImportant = target;
                                    mostImportantsDamage = dmg;
                                }
                                else
                                {
                                    if (Orbwalking.Utility.InAARange(ObjectManager.Player, target) && !Orbwalking.Utility.InAARange(ObjectManager.Player, mostImportant))
                                    {
                                        mostImportant = target;
                                        mostImportantsDamage = dmg;
                                        continue;
                                    }
                                    else if (Orbwalking.Utility.InAARange(ObjectManager.Player, target) && Orbwalking.Utility.InAARange(ObjectManager.Player, mostImportant))
                                    {
                                        if (mostImportantsDamage < dmg / 2f)
                                        {
                                            mostImportant = target;
                                            mostImportantsDamage = dmg;
                                            continue;
                                        }

                                        if ((mostImportant.IsMelee && !target.IsMelee) || (!mostImportant.IsMelee && target.IsMelee))
                                        {
                                            float targetMultp = GetHealthMultipler(target);
                                            float mostImportantsMultp = GetHealthMultipler(mostImportant);
                                            if (mostImportantsMultp < targetMultp)
                                            {
                                                mostImportant = target;
                                                mostImportantsDamage = dmg;
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }
                            return mostImportant;
                        }
                        return null;
                    }
            }

            return null;
        }

        private static float GetTotalMultipler(Obj_AI_Hero target)
        {
            return (GetHealthMultipler(target) + GetTotalADAPMultipler(target) + (s_SelectedTarget == target && ConfigMenu.FocusSelected ? 10 : 0)) * GetRoleMultipler(target) + GetCustomMultipler(target);
        }

        private static float GetTotalADAPMultipler(Obj_AI_Hero target)
        {
            return HeroManager.Enemies.OrderByDescending(p => p.TotalMagicalDamage() + p.TotalAttackDamage).ToList().FindIndex(q => q.NetworkId == target.NetworkId) * 2;
        }

        private static float GetCustomMultipler(Obj_AI_Hero target)
        {
            return ConfigMenu.GetChampionPriority(target) * 2;
        }

        private static float GetRoleMultipler(Obj_AI_Hero target)
        {
            return (5 - target.GetPriority());
        }

        private static float GetHealthMultipler(Obj_AI_Hero target)
        {
            if (target.Health <= Damage.AutoAttack.GetDamage(target) * 2f)
                return 20;

            int spellCount = 0;
            if (IsKillableWithSpells(target, ref spellCount))
            {
                if (spellCount == 1)
                    return 10;
                else if (spellCount > 0)
                {
                    return 10 / (float)spellCount;
                }
            }

            if (target.HealthPercent <= 50 && target.GetRole() != ChampionRole.Tank)
                return 10 / (target.HealthPercent + 1);

            return 0;
        }

        private static bool IsKillableWithSpells(Obj_AI_Hero target, ref int count)
        {
            float dmg = 0.0f;
            foreach (var spell in ObjectManager.Player.Spellbook.Spells)
            {
                if (spell.IsReady() && target.IsValidTarget(spell.SData.CastRange))
                {
                    dmg += (float)ObjectManager.Player.GetSpellDamage(target, spell.Slot);
                    count++;
                }
            }
            return target.Health <= dmg;
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN)
            {
                return;
            }
            s_SelectedTarget =
                HeroManager.Enemies
                    .FindAll(hero => hero.IsValidTarget() && hero.Distance(Game.CursorPos, true) < 40000) // 200 * 200
                    .OrderBy(h => h.Distance(Game.CursorPos, true)).FirstOrDefault();

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (ConfigMenu.FocusSelected && ConfigMenu.SelectedTargetColor.Active)
            {
                if (s_SelectedTarget != null && s_SelectedTarget.IsValidTarget())
                    Render.Circle.DrawCircle(s_SelectedTarget.Position, 150, ConfigMenu.SelectedTargetColor.Color, 7, true);
            }
        }
    }
}
