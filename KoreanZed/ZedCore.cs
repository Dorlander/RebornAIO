namespace KoreanZed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using KoreanZed.QueueActions;
    using KoreanZed.Enumerators;

    using SharpDX;
    using SharpDX.Direct3D9;

    using Color = System.Drawing.Color;

    class ZedCore
    {
        private Orbwalking.Orbwalker ZedOrbwalker { get; set; }

        private readonly ZedMenu zedMenu;

        private readonly ZedSpell q;

        private readonly ZedSpell w;

        private readonly ZedSpell e;

        private readonly ZedSpell r;

        private readonly Obj_AI_Hero player;

        private readonly ZedOffensiveItems zedItems;

        private readonly ZedEnergyChecker energy;

        private readonly ActionQueueList harasQueue;

        private readonly ActionQueueList comboQueue;

        private readonly ActionQueueList laneClearQueue;

        private readonly ActionQueueList lastHitQueue;

        private readonly ActionQueueCheckAutoAttack checkAutoAttack;

        private readonly ZedShadows shadows;

        private readonly ActionQueue actionQueue;

        private readonly ZedComboSelector zedComboSelector;

        public ZedCore(ZedSpells zedSpells, Orbwalking.Orbwalker zedOrbwalker, ZedMenu zedMenu, ZedShadows zedShadows, ZedEnergyChecker zedEnergy)
        {
            q = zedSpells.Q;
            w = zedSpells.W;
            e = zedSpells.E;
            r = zedSpells.R;

            player = ObjectManager.Player;
            ZedOrbwalker = zedOrbwalker;
            this.zedMenu = zedMenu;
            energy = zedEnergy;

            actionQueue = new ActionQueue();
            harasQueue = new ActionQueueList();
            comboQueue = new ActionQueueList();
            laneClearQueue = new ActionQueueList();
            lastHitQueue = new ActionQueueList();
            checkAutoAttack = new ActionQueueCheckAutoAttack();
            zedItems = new ZedOffensiveItems(zedMenu);
            shadows = zedShadows;
            zedComboSelector = new ZedComboSelector(zedMenu);

            Game.OnUpdate += Game_OnUpdate;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            switch (ZedOrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    LastHit();
                    break;
                default:
                    return;
            }
        }

        private void Combo()
        {
            if (actionQueue.ExecuteNextAction(comboQueue))
            {
                return;
            }

            Obj_AI_Hero itemsTarget = TargetSelector.GetTarget(player.AttackRange, TargetSelector.DamageType.Physical);
            if (itemsTarget != null)
            {
                zedItems.UseItems(itemsTarget);
            }

            shadows.Combo();

            if (w.UseOnCombo && shadows.CanCast && player.HasBuff("zedr2"))
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(w.Range + e.Range, TargetSelector.DamageType.Physical);

                if (target != null)
                {
                    actionQueue.EnqueueAction(comboQueue,
                        () => true,
                        () => shadows.Cast(w.GetPrediction(target).CastPosition),
                        () => true);
                    return;
                }
            }

            float maxRange = float.MaxValue;

            if (r.UseOnCombo && r.IsReady() && r.Instance.ToggleState == 0)
            {
                Obj_AI_Hero target = null;

                maxRange = Math.Min(maxRange, r.Range);

                if (zedMenu.GetParamBool("koreanzed.combo.ronselected"))
                {
                    if (TargetSelector.SelectedTarget != null && TargetSelector.SelectedTarget.IsValidTarget(maxRange))
                    {
                        target = TargetSelector.SelectedTarget;
                    }
                }
                else
                {
                    List<Obj_AI_Hero> ignoredChamps = zedMenu.GetBlockList(BlockListType.Ultimate);
                    target = TargetSelector.GetTarget(maxRange, r.DamageType, true, ignoredChamps);
                }

                if (target != null)
                {
                    switch (zedMenu.GetCombo())
                    {
                        case ComboType.AllStar:
                            AllStarCombo(target);
                            //Console.WriteLine("start combo 1");
                            break;
                        case ComboType.TheLine:
                            TheLineCombo(target);
                            //Console.WriteLine("line combo1");
                            break;
                    }
                    return;
                }
            }
            else if (w.UseOnCombo && shadows.CanCast && (!r.UseOnCombo || (r.UseOnCombo && !r.IsReady()))
                && (player.Mana > w.ManaCost + (q.UseOnCombo && q.IsReady() ? q.ManaCost : 0F) + (e.UseOnCombo && e.IsReady() ? e.ManaCost : 0F)))
            {
                maxRange = Math.Min(maxRange, w.Range + e.Range);
                Obj_AI_Hero target = TargetSelector.GetTarget(maxRange, TargetSelector.DamageType.Physical);
                if (target != null)
                {
                    actionQueue.EnqueueAction(
                        comboQueue,
                        () => shadows.CanCast,
                        () => shadows.Cast(w.GetPrediction(target).CastPosition),
                        () => !shadows.CanCast);
                    actionQueue.EnqueueAction(
                        comboQueue,
                        () => w.Instance.ToggleState != 0,
                        () => shadows.Combo(),
                        () => true);
                    actionQueue.EnqueueAction(
                        comboQueue,
                        () => shadows.CanSwitch && target.Distance(shadows.Instance.Position) <= player.AttackRange,
                        () => shadows.Switch(),
                        () => !shadows.CanSwitch || target.Distance(shadows.Instance.Position) > player.AttackRange || !w.IsReady());
                    actionQueue.EnqueueAction(
                        comboQueue,
                        () => player.Distance(target) <= Orbwalking.GetRealAutoAttackRange(target),
                        () => player.IssueOrder(GameObjectOrder.AttackUnit, target),
                        () => target.IsDead || target.IsZombie || player.Distance(target) > Orbwalking.GetRealAutoAttackRange(target) || checkAutoAttack.Status);
                    //Console.WriteLine("combo3 else");
                    return;
                }
            }

            if (q.UseOnCombo && q.IsReady() && player.Mana > q.ManaCost)
            {
                maxRange = Math.Min(maxRange, q.Range);
                Obj_AI_Hero target = TargetSelector.GetTarget(maxRange, q.DamageType);

                PredictionOutput predictionOutput = q.GetPrediction(target);

                if (predictionOutput.Hitchance >= HitChance.Medium)
                {
                    q.Cast(predictionOutput.CastPosition);
                    //Console.WriteLine("combo q");
                }
            }

            if (e.UseOnCombo && e.IsReady() && player.Mana > e.ManaCost)
            {
                maxRange = Math.Min(maxRange, e.Range);
                Obj_AI_Hero target = TargetSelector.GetTarget(maxRange, e.DamageType);
                if (target != null)
                {
                    actionQueue.EnqueueAction(comboQueue,
                        () => e.IsReady(),
                        () => e.Cast(),
                        () => true);
                    //Console.WriteLine("combo e");
                    return;
                }
            }

            if (w.UseOnCombo && w.IsReady() && shadows.CanSwitch)
            {
                List<Obj_AI_Base> shadowList = shadows.GetShadows();

                foreach (Obj_AI_Base objAiBase in shadowList)
                {
                    Obj_AI_Hero target = TargetSelector.GetTarget(2000F, TargetSelector.DamageType.Physical);

                    if (target != null && player.Distance(target) > Orbwalking.GetRealAutoAttackRange(target) + 50F &&
                        objAiBase.Distance(target) < player.Distance(target))
                    {
                        shadows.Switch();
                        //Console.WriteLine("combo w");
                    }
                }
            }
        }

        private void AllStarCombo(Obj_AI_Hero target)
        {
            actionQueue.EnqueueAction(
                comboQueue,
                () => r.IsReady() && r.Instance.ToggleState == 0 && player.IsVisible,
                () =>
                    {
                        zedComboSelector.AllStarAnimation();
                        r.Cast(target);
                    },
                () => r.IsReady() && r.Instance.ToggleState != 0 && player.IsVisible);
            actionQueue.EnqueueAction(
                comboQueue, 
                () => true, 
                () => zedItems.UseItems(target), 
                () => true);
            actionQueue.EnqueueAction(
                comboQueue,
                () => w.UseOnCombo && shadows.CanCast && player.Mana > w.ManaCost,
                () => shadows.Cast(target.ServerPosition),
                () => target.IsDead || target.IsZombie || w.Instance.ToggleState != 0 || !w.UseOnCombo || player.Mana <= w.ManaCost);
            actionQueue.EnqueueAction(
                comboQueue,
                () => w.Instance.ToggleState != 0 && q.UseOnCombo && q.IsReady(),
                () => q.Cast(q.GetPrediction(target).CastPosition),
                () => target.IsDead || target.IsZombie || !q.IsReady() || !q.UseOnCombo || player.Mana <= q.ManaCost);
            actionQueue.EnqueueAction(
                comboQueue,
                () => w.Instance.ToggleState != 0 && e.UseOnCombo && e.IsReady() && e.CanCast(target),
                () => e.Cast(),
                () => target.IsDead || target.IsZombie || w.Instance.ToggleState == 0 || !e.IsReady() || !e.UseOnCombo || !e.CanCast(target));
            //Console.WriteLine("star combo2");
        }

        private void TheLineCombo(Obj_AI_Hero target)
        {
            actionQueue.EnqueueAction(
                comboQueue,
                () => r.IsReady() && r.Instance.ToggleState == 0 && player.IsVisible,
                () =>
                {
                    zedComboSelector.TheLineAnimation();
                    r.Cast(target);
                },
                () => r.IsReady() && r.Instance.ToggleState != 0 && player.IsVisible);
            actionQueue.EnqueueAction(
                comboQueue, 
                () => true, 
                () => zedItems.UseItems(target), 
                () => true);
            actionQueue.EnqueueAction(
                comboQueue,
                () => w.UseOnCombo && shadows.CanCast && player.Mana > w.ManaCost,
                () => shadows.Cast(target.Position.Extend(shadows.Instance.Position, -1000F)),
                () => target.IsDead || target.IsZombie || w.Instance.ToggleState != 0 || !w.UseOnCombo || player.Mana <= w.ManaCost);
            actionQueue.EnqueueAction(
                comboQueue,
                () => e.UseOnCombo && e.IsReady() && e.CanCast(target),
                () => e.Cast(),
                () => target.IsDead || target.IsZombie || !e.IsReady() || !e.UseOnCombo || !e.CanCast(target));
            actionQueue.EnqueueAction(
                comboQueue,
                () => q.UseOnCombo && q.IsReady() && q.CanCast(target),
                () => q.Cast(q.GetPrediction(target).CastPosition),
                () => target.IsDead || target.IsZombie || !q.IsReady() || !q.UseOnCombo || !q.CanCast(target) || player.Mana <= q.ManaCost);
            //Console.WriteLine("line combo2");
        }

        private void Harass()
        {
            if (actionQueue.ExecuteNextAction(harasQueue))
            {
                return;
            }

            shadows.Harass();

            float maxRange = float.MaxValue;

            if (!energy.ReadyToHaras)
            {
                return;
            }

            List<Obj_AI_Hero> blackList = zedMenu.GetBlockList(BlockListType.Harass);

            if ((w.UseOnHarass && w.IsReady() && w.Instance.ToggleState == 0)
                && (player.HealthPercent
                    > zedMenu.GetParamSlider("koreanzed.harasmenu.wusage.dontuselowlife")
                    && HeroManager.Enemies.Count(
                        hero => !hero.IsDead && !hero.IsZombie && player.Distance(hero) < 2000F)
                    < zedMenu.GetParamSlider("koreanzed.harasmenu.wusage.dontuseagainst")))
            {
                if (q.UseOnHarass && q.IsReady() && (player.Mana > q.ManaCost + w.ManaCost))
                {
                    switch ((ShadowHarassTrigger)zedMenu.GetParamStringList("koreanzed.harasmenu.wusage.trigger"))
                    {
                        case ShadowHarassTrigger.MaxRange:
                            maxRange = Math.Min(maxRange, q.Range + w.Range);
                            //Console.WriteLine("haras 1");
                            break;

                        case ShadowHarassTrigger.MaxDamage:
                            maxRange = Math.Min(maxRange, w.Range + e.Range);
                            //Console.WriteLine("haras 2");
                            break;
                    }

                    Obj_AI_Hero target = TargetSelector.GetTarget(maxRange, q.DamageType, true, blackList);

                    if (target != null)
                    {
                        actionQueue.EnqueueAction(
                            harasQueue,
                            () => shadows.CanCast,
                            () => shadows.Cast(target),
                            () => !shadows.CanCast);
                        actionQueue.EnqueueAction(
                            harasQueue,
                            () => true,
                            () => shadows.Harass(),
                            () => true);
                        //Console.WriteLine("haras balck list");
                        return;
                    }
                }

                else if (e.UseOnHarass && e.IsReady() && (player.Mana > e.ManaCost + w.ManaCost))
                {
                    maxRange = Math.Min(maxRange, e.Range + w.Range);
                    Obj_AI_Hero target = TargetSelector.GetTarget(maxRange, e.DamageType, true, blackList);

                    if (target != null)
                    {
                        actionQueue.EnqueueAction(
                            harasQueue,
                            () => shadows.CanCast,
                            () => shadows.Cast(target),
                            () => !shadows.CanCast);
                        actionQueue.EnqueueAction(
                            harasQueue,
                            () => true,
                            () => shadows.Harass(),
                            () => true);
                        //Console.WriteLine("haras black list else");
                        return;
                    }
                }
            }

            if (q.UseOnHarass && energy.ReadyToHaras)
            {
                maxRange = Math.Min(maxRange, q.Range);
                Obj_AI_Hero target = TargetSelector.GetTarget(maxRange, q.DamageType, true, blackList);

                if (target != null)
                {
                    PredictionOutput predictionOutput = q.GetPrediction(target);

                    bool checkColision = zedMenu.GetParamBool("koreanzed.harasmenu.checkcollisiononq");

                    if (predictionOutput.Hitchance >= HitChance.High
                        && (!checkColision
                            || !q.GetCollision(
                                player.Position.To2D(),
                                new List<Vector2>() { predictionOutput.CastPosition.To2D() }).Any()))
                    {
                        q.Cast(predictionOutput.CastPosition);
                        //Console.WriteLine("has q");
                    }
                }
            }

            if (e.UseOnHarass && energy.ReadyToHaras)
            {
                if (TargetSelector.GetTarget(e.Range, e.DamageType) != null)
                {
                    e.Cast();
                    //Console.WriteLine("has e");
                }
            }

            if ((zedMenu.GetParamBool("koreanzed.harasmenu.items.hydra")
                || zedMenu.GetParamBool("koreanzed.harasmenu.items.tiamat")) &&
                (HeroManager.Enemies.Any(enemy => player.Distance(enemy) <= player.AttackRange)))
            {
                actionQueue.EnqueueAction(harasQueue, () => true, () => zedItems.UseHarasItems(), () => true);
                //Console.WriteLine("hydra tiamat haras");
            }

            LastHit();
        }

        private void JungleClear()
        {
            if (q.UseOnLaneClear && q.IsReady() && energy.ReadyToLaneClear)
            {
                Obj_AI_Base jungleMob =
                    MinionManager.GetMinions(
                        q.Range / 1.5F,
                        MinionTypes.All,
                        MinionTeam.Neutral,
                        MinionOrderTypes.MaxHealth).FirstOrDefault();

                if (jungleMob != null)
                {
                    q.Cast(q.GetPrediction(jungleMob).CastPosition);
                    //Console.WriteLine("jg clear q");
                }
            }

            if (e.UseOnLaneClear && e.IsReady() && energy.ReadyToLaneClear)
            {
                if (
                    MinionManager.GetMinions(e.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
                        .Any())
                {
                    e.Cast();
                    //Console.WriteLine("jg clear e");
                }
            }
        }

        private void LaneClear()
        {
            if (actionQueue.ExecuteNextAction(laneClearQueue))
            {
                return;
            }

            LastHit();

            JungleClear();

            if ((zedMenu.GetParamBool("koreanzed.laneclearmenu.items.hydra")
                || zedMenu.GetParamBool("koreanzed.laneclearmenu.items.tiamat")) &&
                (MinionManager.GetMinions(300F).Count() >= zedMenu.GetParamSlider("koreanzed.laneclearmenu.items.when")
                || MinionManager.GetMinions(300F, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).Any()))
            {
                zedItems.UseItemsLaneClear();
                //Console.WriteLine("lane clear item");
            }

            if (shadows.GetShadows().Any())
            {
                shadows.LaneClear(actionQueue, laneClearQueue);
                //Console.WriteLine("lane clear item");
                return;
            }

            if (w.UseOnLaneClear)
            {
                WlaneClear();
                //Console.WriteLine("w lane clear");
                return;
            }
            else
            {
                if (e.UseOnLaneClear && e.IsReady())
                {
                    int willHit = MinionManager.GetMinions(e.Range).Count;
                    int param = zedMenu.GetParamSlider("koreanzed.laneclearmenu.useeif");

                    if (willHit >= param)
                    {
                        actionQueue.EnqueueAction(
                            laneClearQueue,
                            () => true,
                            () => e.Cast(),
                            () => !e.IsReady());
                        //Console.WriteLine("lane clear e");
                        return;
                    }
                }

                if (q.UseOnLaneClear && q.IsReady())
                {
                    var farmLocation = q.GetLineFarmLocation(MinionManager.GetMinions(q.Range));

                    int willHit = farmLocation.MinionsHit;
                    int param = zedMenu.GetParamSlider("koreanzed.laneclearmenu.useqif");

                    if (willHit >= param)
                    {
                        actionQueue.EnqueueAction(
                            laneClearQueue,
                            () => q.IsReady(),
                            () => q.Cast(farmLocation.Position),
                            () => !q.IsReady());
                        //Console.WriteLine("lane clear q");
                        return;
                    }
                }
            }
        }

        private void WlaneClear()
        {
            if (!energy.ReadyToLaneClear)
            {
                return;
            }

            var minionsLong = MinionManager.GetMinions(w.Range + q.Range);
            var minionsShort = minionsLong.Where(minion => player.Distance(minion) <= w.Range + e.Range).ToList();
            bool attackingMinion = minionsShort.Any(minion => player.Distance(minion) <= player.AttackRange);

            if (!attackingMinion)
            {
                return;
            }

            var theChosen =
                MinionManager.GetMinions(e.Range + w.Range)
                    .OrderByDescending(
                        minion =>
                        MinionManager.GetMinions(player.Position.Extend(minion.Position, e.Range + 130F), e.Range)
                            .Count())
                    .FirstOrDefault();
            if (theChosen == null)
            {
                return;
            }

            Vector3 shadowPosition = player.Position.Extend(theChosen.Position, e.Range + 130F);

            if (player.Distance(shadowPosition) <= w.Range - 100F)
            {
                shadowPosition = Vector3.Zero;
            }

            bool canUse = HeroManager.Enemies.Count(enemy => !enemy.IsDead && !enemy.IsZombie && enemy.Distance(player) < 2500F)
                          <= zedMenu.GetParamSlider("koreanzed.laneclearmenu.dontuseeif");

            if (e.UseOnLaneClear && e.IsReady())
            {
                int extendedWillHit = MinionManager.GetMinions(shadowPosition, e.Range).Count();
                int shortenWillHit = MinionManager.GetMinions(e.Range).Count;
                int param = zedMenu.GetParamSlider("koreanzed.laneclearmenu.useeif");

                if (canUse && shadows.CanCast && shadowPosition != Vector3.Zero && extendedWillHit >= param
                    && player.Mana > w.ManaCost + e.ManaCost)
                {
                    actionQueue.EnqueueAction(
                        laneClearQueue,
                        () => shadows.CanCast,
                        () => shadows.Cast(shadowPosition),
                        () => !shadows.CanCast);
                    actionQueue.EnqueueAction(
                        laneClearQueue,
                        () => w.Instance.ToggleState != 0,
                        () => e.Cast(),
                        () => !e.IsReady());
                    //Console.WriteLine("w laneclear e 1");
                    return;
                }
                else if (shortenWillHit >= param)
                {
                    actionQueue.EnqueueAction(
                        laneClearQueue,
                        () => e.IsReady(),
                        () => e.Cast(),
                        () => !e.IsReady());
                    //Console.WriteLine("w laneclear e 2");
                    return;
                }
            }

            if (q.UseOnLaneClear && q.IsReady())
            {
                int extendedWillHit = 0;
                Vector3 extendedFarmLocation = Vector3.Zero;
                foreach (Obj_AI_Base objAiBase in MinionManager.GetMinions(shadowPosition, q.Range))
                {
                    var colisionList = q.GetCollision(
                        shadowPosition.To2D(),
                        new List<Vector2>() { objAiBase.Position.To2D() },
                        w.Delay);

                    if (colisionList.Count > extendedWillHit)
                    {
                        extendedFarmLocation =
                            colisionList.OrderByDescending(c => c.Distance(shadowPosition)).FirstOrDefault().Position;
                        extendedWillHit = colisionList.Count;
                    }
                }

                var shortenFarmLocation = q.GetLineFarmLocation(MinionManager.GetMinions(q.Range));

                int shortenWillHit = shortenFarmLocation.MinionsHit;
                int param = zedMenu.GetParamSlider("koreanzed.laneclearmenu.useqif");

                if (canUse && shadows.CanCast && shadowPosition != Vector3.Zero && extendedWillHit >= param
                    && player.Mana > w.ManaCost + q.ManaCost)
                {
                    actionQueue.EnqueueAction(
                        laneClearQueue,
                        () => shadows.CanCast,
                        () => shadows.Cast(shadowPosition),
                        () => !shadows.CanCast);
                    actionQueue.EnqueueAction(
                        laneClearQueue,
                        () => w.Instance.ToggleState != 0,
                        () => q.Cast(extendedFarmLocation),
                        () => !q.IsReady());
                    //Console.WriteLine("w lancelear q 1");
                    return;
                }
                else if (shortenWillHit >= param)
                {
                    actionQueue.EnqueueAction(
                        laneClearQueue,
                        () => q.IsReady(),
                        () => q.Cast(shortenFarmLocation.Position),
                        () => !q.IsReady());
                    //Console.WriteLine("w laneclear q 2");
                    return;
                }
            }
        }

        private void LastHit()
        {
            if (actionQueue.ExecuteNextAction(lastHitQueue))
            {
                return;
            }

            if (q.UseOnLastHit && q.IsReady() && energy.ReadyToLastHit)
            {
                Obj_AI_Base target =
                    MinionManager.GetMinions(q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth)
                        .FirstOrDefault(
                            minion =>
                            minion.Distance(player) > Orbwalking.GetRealAutoAttackRange(minion) + 10F && !minion.IsDead
                            && q.GetDamage(minion) / 2
                            >= HealthPrediction.GetHealthPrediction(
                                minion,
                                (int)(player.Distance(minion) / q.Speed) * 1000,
                                (int)q.Delay * 1000));

                if (target != null)
                {
                    PredictionOutput predictionOutput = q.GetPrediction(target);
                    actionQueue.EnqueueAction(lastHitQueue, () => q.IsReady(), () => q.Cast(predictionOutput.CastPosition), () => !q.IsReady());
                    //Console.WriteLine("lasthit q 1");
                    return;
                }
            }

            if (e.UseOnLastHit && e.IsReady() && energy.ReadyToLastHit)
            {
                if (MinionManager.GetMinions(e.Range).Count(minion => e.IsKillable(minion))
                    >= zedMenu.GetParamSlider("koreanzed.lasthitmenu.useeif"))
                {
                    actionQueue.EnqueueAction(lastHitQueue, () => e.IsReady(), () => e.Cast(), () => !e.IsReady());
                    //Console.WriteLine("lasthit q 2");
                    return;
                }
            }
        }

        public float ComboDamage(Obj_AI_Hero target)
        {
            float result = q.UseOnCombo && q.IsReady()
                               ? (q.GetCollision(player.Position.To2D(), new List<Vector2>() { target.Position.To2D() })
                                      .Any()
                                      ? q.GetDamage(target) / 2
                                      : q.GetDamage(target))
                               : 0F;

            result += e.UseOnCombo && e.IsReady() ? e.GetDamage(target) : 0F;

            result += w.UseOnCombo && w.IsReady() && player.Distance(target) < w.Range + Orbwalking.GetRealAutoAttackRange(target)
                          ? (float)player.GetAutoAttackDamage(target, true)
                          : 0F;

            float multiplier = 0.3F;
            if (r.Instance.Level == 2) multiplier = 0.4F;
            else if (r.Instance.Level == 3) multiplier = 0.5F;

            result += r.UseOnCombo && r.IsReady()
                          ? (float)
                            (r.GetDamage(target) + player.GetAutoAttackDamage(target, true)
                             + (q.IsReady() ? q.GetDamage(target) * multiplier : 0F)
                             + (e.IsReady() ? e.GetDamage(target) * multiplier : 0F))
                          : 0F;

            return result;
        }

        public void ForceUltimate(Obj_AI_Hero target = null)
        {
            if (target != null && r.CanCast(target))
            {
                r.Cast(target);
                //Console.WriteLine("force r 1");
            }
            else
            {
                r.Cast(TargetSelector.GetTarget(r.Range, r.DamageType));
                //Console.WriteLine("force r 2");
            }
        }
    }
}
