namespace KoreanLucian
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using KoreanCommon;

    using LeagueSharp;
    using LeagueSharp.Common;
    using LeagueSharp.Common.Data;

    using ItemData = LeagueSharp.Common.Data.ItemData;

    using SharpDX;

    internal class Core : CommonCore
    {
        private readonly object lockObject;

        private readonly Lucian lucian;

        private bool hasPassive;

        public Core(Lucian lucian)
            : base(lucian)
        {
            this.lucian = lucian;

            lockObject = new object();

            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Spellbook.OnCastSpell += LockR;
            Spellbook.OnCastSpell += PassiveControl;
        }

        private void PassiveControl(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.Q || args.Slot == SpellSlot.W || args.Slot == SpellSlot.E)
            {
                ProcessSpell();
            }
        }

        private void ProcessSpell()
        {
            hasPassive = true;
            Utility.DelayAction.Add(450, Orbwalking.ResetAutoAttackTimer);
        }

        private void LockR(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot != SpellSlot.R)
            {
                return;
            }

            lock (lockObject)
            {
                Spellbook.OnCastSpell -= LockR;

                if (KoreanUtils.GetParamBool(champion.MainMenu, "useyoumuu")
                    && ItemData.Youmuus_Ghostblade.GetItem().IsReady())
                {
                    ItemData.Youmuus_Ghostblade.GetItem().Cast();
                }

                if (KoreanUtils.GetParamBool(champion.MainMenu, "lockr"))
                {
                    Obj_AI_Hero target = null;

                    if (TargetSelector.SelectedTarget != null
                        && TargetSelector.SelectedTarget.Distance(champion.Player.Position) < R.Range + 400F)
                    {
                        target = TargetSelector.SelectedTarget;
                    }
                    else
                    {
                        target = TargetSelector.GetTarget(
                            champion.Player,
                            R.Range + 400f,
                            TargetSelector.DamageType.Physical);
                    }

                    if (target != null)
                    {
                        args.Process = false;
                        if (R.Cast(target.Position))
                        {
                            ProcessSpell();
                        }
                    }
                }

                Spellbook.OnCastSpell += LockR;
            }
        }

        private bool CheckPassive()
        {
            return hasPassive;
        }

        private bool CheckHaras(Obj_AI_Hero target)
        {
            if (target == null)
            {
                return false;
            }
            return KoreanUtils.GetParamBool(lucian.MainMenu, target.ChampionName.ToLowerInvariant());
        }

        //Maybe next feature
        //private bool IsJungleMob(Obj_AI_Base target)
        //{
        //    return (target.SkinName.ToLowerInvariant() == "sru_blue"
        //            || target.SkinName.ToLowerInvariant() == "sru_gromp"
        //            || target.SkinName.ToLowerInvariant() == "sru_murkwolf"
        //            || target.SkinName.ToLowerInvariant() == "sru_razorbeak"
        //            || target.SkinName.ToLowerInvariant() == "sru_baron"
        //            || target.SkinName.ToLowerInvariant() == "sru_dragon"
        //            || target.SkinName.ToLowerInvariant() == "sru_red"
        //            || target.SkinName.ToLowerInvariant() == "sru_crab"
        //            || target.SkinName.ToLowerInvariant() == "sru_krug");
        //}

        public bool HaveManaToHaras()
        {
            return champion.Player.ManaPercent > KoreanUtils.GetParamSlider(champion.MainMenu, "manalimittoharas");
        }

        public bool HaveManaToLaneclear()
        {
            return champion.Player.ManaPercent > KoreanUtils.GetParamSlider(champion.MainMenu, "manalimittolaneclear");
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                hasPassive = false;
            }
        }

        public override void LastHitMode()
        {
            //no needed
        }

        public override void HarasMode()
        {
            lock (lockObject)
            {
                Obj_AI_Hero target;

                if (Q.UseOnHaras && Q.IsReady() && Q.CanCast() && HaveManaToHaras())
                {
                    if (!KoreanUtils.GetParamKeyBind(champion.MainMenu, "toggleextendedq"))
                    {
                        if (champion.CastExtendedQ(true))
                        {
                            ProcessSpell();
                        }
                    }

                    if (!CheckPassive())
                    {
                        target = TargetSelector.GetTarget(champion.Player, Q.Range, TargetSelector.DamageType.Physical);

                        if (!CheckHaras(target))
                        {
                            return;
                        }

                        if (target != null && Q.IsReadyToCastOn(target) && Q.CanCast(target))
                        {
                            if (Q.CastOnUnit(target))
                            {
                                ProcessSpell();
                            }
                        }
                    }
                }

                if (W.UseOnHaras && !CheckPassive() && HaveManaToHaras() && W.IsReady() && W.CanCast())
                {
                    target = TargetSelector.GetTarget(champion.Player, W.Range, TargetSelector.DamageType.Physical);

                    if (!CheckHaras(target))
                    {
                        return;
                    }

                    if (target != null
                        && target.Distance(champion.Player) <= Orbwalking.GetRealAutoAttackRange(champion.Player))
                    {
                        if (W.Cast(target.Position))
                        {
                            ProcessSpell();
                        }
                    }
                    else
                    {
                        PredictionOutput wPrediction = W.GetPrediction(target);

                        if (wPrediction != null && wPrediction.Hitchance >= HitChance.High
                            && wPrediction.CastPosition != Vector3.Zero)
                        {
                            if (W.Cast(wPrediction.CastPosition))
                            {
                                ProcessSpell();
                            }
                        }
                    }
                }

                if (E.UseOnHaras && lucian.semiAutomaticE.Holding && E.IsReady() && E.CanCast() && !CheckPassive()
                    && ((E.Instance.ManaCost > 0 && HaveManaToHaras()) || E.Instance.ManaCost.Equals(0f)))
                {
                    target = TargetSelector.GetTarget(
                        champion.Player,
                        E.Range + Orbwalking.GetRealAutoAttackRange(champion.Player),
                        TargetSelector.DamageType.Physical);

                    if (!CheckHaras(target))
                    {
                        return;
                    }

                    if (target != null && lucian.semiAutomaticE.Cast(target))
                    {
                        ProcessSpell();
                    }
                }
            }
        }

        public override void LaneClearMode()
        {
            if (CheckPassive())
            {
                return;
            }

            lock (lockObject)
            {
                if (Q.UseOnLaneClear && Q.IsReady() && Q.CanCast() && !CheckPassive() && HaveManaToLaneclear()
                    && champion.CastExtendedQToLaneClear())
                {
                    ProcessSpell();
                }

                if (W.UseOnLaneClear && W.IsReady() && W.CanCast() && !CheckPassive() && HaveManaToHaras())
                {
                    List<Obj_AI_Base> minions = MinionManager.GetMinions(W.Range).ToList();
                    MinionManager.FarmLocation farmLocation = W.GetCircularFarmLocation(minions);

                    int minMinions = KoreanUtils.GetParamSlider(champion.MainMenu, "wcounthit");

                    if (farmLocation.MinionsHit >= minMinions && minions.Count >= minMinions)
                    {
                        if (W.Cast(farmLocation.Position))
                        {
                            ProcessSpell();
                        }
                    }
                }

                if (E.UseOnLaneClear && lucian.semiAutomaticE.Holding && E.IsReady() && E.CanCast() && !CheckPassive()
                    && (E.Instance.ManaCost.Equals(0) || (E.Instance.ManaCost > 0 && HaveManaToHaras())))
                {
                    Obj_AI_Base target =
                        MinionManager.GetMinions(E.Range + Orbwalking.GetRealAutoAttackRange(champion.Player))
                            .FirstOrDefault(
                                minion =>
                                champion.Player.Distance(minion) > Orbwalking.GetRealAutoAttackRange(champion.Player)
                                && Game.CursorPos.Distance(minion.Position) < champion.Player.Distance(minion));

                    if (target != null && lucian.semiAutomaticE.Cast(target))
                    {
                        ProcessSpell();
                        champion.Orbwalker.ForceTarget(target);
                    }
                }
            }

            if (KoreanUtils.GetParamBool(champion.MainMenu, "harasonlaneclear"))
            {
                HarasMode();
            }
        }

        public override void ComboMode()
        {
            //IDK why but i need this code since the patch 5.17
            if (champion.Player.IsChannelingImportantSpell())
            {
                Orbwalking.MoveTo(Game.CursorPos);
                return;
            }

            Obj_AI_Hero target;

            if (W.UseOnCombo && !CheckPassive() && W.IsReady() && W.CanCast())
            {
                target = TargetSelector.GetTarget(champion.Player, W.Range, TargetSelector.DamageType.Physical);

                PredictionOutput wPrediction = W.GetPrediction(target);

                if (wPrediction != null && wPrediction.Hitchance >= HitChance.High
                    && wPrediction.CastPosition != Vector3.Zero)
                {
                    if (W.Cast(wPrediction.CastPosition))
                    {
                        ProcessSpell();
                    }
                }
                else if (target != null
                         && target.Distance(champion.Player) <= Orbwalking.GetRealAutoAttackRange(champion.Player))
                {
                    if (W.Cast(target.ServerPosition))
                    {
                        ProcessSpell();
                    }
                }
            }

            if (Q.UseOnCombo && Q.IsReady() && Q.CanCast())
            {
                if (!CheckPassive())
                {
                    target = TargetSelector.GetTarget(champion.Player, Q.Range, TargetSelector.DamageType.Physical);

                    if (target != null && Q.IsReadyToCastOn(target) && Q.CanCast(target))
                    {
                        if (Q.CastOnUnit(target))
                        {
                            ProcessSpell();
                        }
                    }
                }

                if (champion.CastExtendedQ())
                {
                    ProcessSpell();
                }
            }

            if (E.UseOnCombo && !CheckPassive() && E.IsReady() && E.CanCast()
                && (!KoreanUtils.GetParamBool(champion.MainMenu, "dashmode")
                    || (KoreanUtils.GetParamBool(champion.MainMenu, "dashmode") && lucian.semiAutomaticE.Holding)))
            {
                target = TargetSelector.GetTarget(
                    E.Range + Orbwalking.GetRealAutoAttackRange(champion.Player) - 25f,
                    TargetSelector.DamageType.Physical);

                if (target != null && target.IsEnemy && target.IsValid && !target.IsDead && lucian.semiAutomaticE.Cast(target))
                {
                    ProcessSpell();
                }
            }
        }

        public override void Ultimate(Obj_AI_Hero target)
        {
            if (target == null)
            {
                R.Cast(Game.CursorPos);
            }
            else if (KoreanUtils.GetParamBool(champion.MainMenu, "lockr"))
            {
                R.Cast(target.Position);
            }
        }
    }
}