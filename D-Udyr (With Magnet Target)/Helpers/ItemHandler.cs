using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;

namespace D_Udyr.Helpers
{
    public class ItemHandler
    {
        public static Obj_AI_Hero player = ObjectManager.Player;
        public static Items.Item tiamat = LeagueSharp.Common.Data.ItemData.Ironspike_Whip.GetItem();
        public static Items.Item randuins = LeagueSharp.Common.Data.ItemData.Randuins_Omen.GetItem();
        public static Items.Item Dfg = new Items.Item(3128, 750);
        public static Items.Item Bft = new Items.Item(3188, 750);
        public static Items.Item Muramana = LeagueSharp.Common.Data.ItemData.Muramana.GetItem();
        public static Items.Item Muramana2 = LeagueSharp.Common.Data.ItemData.Muramana2.GetItem();
        public static Items.Item sheen = new Items.Item(3057, player.AttackRange);
        public static Items.Item gaunlet = new Items.Item(3025, player.AttackRange);
        public static Items.Item trinity = new Items.Item(3078, player.AttackRange);
        public static Items.Item lich = new Items.Item(3100, player.AttackRange);
        public static Items.Item youmuu = new Items.Item(3142, player.AttackRange);

        public static Items.Item solari = LeagueSharp.Common.Data.ItemData.Locket_of_the_Iron_Solari.GetItem();

        public static Items.Item Qss = new Items.Item(3140, 0);
        public static Items.Item Mercurial = new Items.Item(3139, 0);
        public static Items.Item Dervish = new Items.Item(3137, 0);
        public static Items.Item Zhonya = new Items.Item(3157, 0);
        public static Items.Item Wooglet = new Items.Item(3090, 0);
        public static Items.Item Seraph = new Items.Item(3040, 0);
        public static Items.Item SeraphDom = new Items.Item(3048, 0);

        public static Items.Item ProtoBelt = new Items.Item(3152, 0);
        public static Items.Item GLP = new Items.Item(3030, 0);

        public static bool QssUsed, useHydra = false;
        public static float MuramanaTime;
        public static Obj_AI_Hero hydraTarget;

        private static readonly Random _random = new Random(DateTime.Now.Millisecond);

        public static bool IsURF;

        public static Spell Q, W, E, R;

        public static void UseItems(Obj_AI_Hero target, Menu config, float comboDmg = 0f, bool cleanseSpell = false)
        {
            if (config.Item("hyd").GetValue<bool>() && player.ChampionName != "Renekton")
            {
                castHydra(target);
            }
            if (config.Item("hyd").GetValue<bool>())
            {
                hydraTarget = target;
                useHydra = true;
            }
            else
            {
                useHydra = false;
            }
            if (config.Item("ran").GetValue<bool>() && Items.HasItem(randuins.Id) && Items.CanUseItem(randuins.Id))
            {
                if (target != null && player.Distance(target) < randuins.Range &&
                    player.CountEnemiesInRange(randuins.Range) >= config.Item("ranmin").GetValue<Slider>().Value)
                {
                    Items.UseItem(randuins.Id);
                }
            }
            /*
            if (config.Item("Muramana").GetValue<bool>() &&
                ((!MuramanaEnabled && config.Item("MuramanaMinmana").GetValue<Slider>().Value < player.ManaPercent) ||
                 (MuramanaEnabled && config.Item("MuramanaMinmana").GetValue<Slider>().Value > player.ManaPercent)))
            {
                if (Muramana.IsOwned() && Muramana.IsReady())
                {
                    Muramana.Cast();
                }
                if (Muramana2.IsOwned() && Muramana2.IsReady())
                {
                    Muramana2.Cast();
                }
            }
            MuramanaTime = System.Environment.TickCount;
             */
            if (config.Item("you").GetValue<bool>() && Items.HasItem(youmuu.Id) && Items.CanUseItem(youmuu.Id) &&
                target != null && player.Distance(target) < player.AttackRange + 50 && target.HealthPercent < 65)
            {
                youmuu.Cast();
            }

            if (config.Item("Zhonya").GetValue<bool>())
            {
                if (((config.Item("Zhonyadmg").GetValue<Slider>().Value / 100f * player.Health <=
                      Program.IncDamages.GetAllyData(player.NetworkId).DamageTaken ||
                      Program.IncDamages.GetAllyData(player.NetworkId).DamageTaken > player.Health) ||
                     (Danger() && player.HealthPercent < 30)) && player.IsValidTarget(10, false))
                {
                    if (Items.HasItem(Zhonya.Id) && Items.CanUseItem(Zhonya.Id))
                    {
                        Zhonya.Cast();
                        return;
                    }
                    if (Items.HasItem(Wooglet.Id) && Items.CanUseItem(Wooglet.Id))
                    {
                        Wooglet.Cast();
                        return;
                    }
                }
            }

            if (config.Item("Seraph").GetValue<bool>())
            {
                if (((config.Item("SeraphdmgHP").GetValue<Slider>().Value / 100f * player.Health <=
                      Program.IncDamages.GetAllyData(player.NetworkId).DamageTaken ||
                      Program.IncDamages.GetAllyData(player.NetworkId).DamageTaken > player.Health) ||
                     (config.Item("SeraphdmgSh").GetValue<Slider>().Value / 100f * (150 + player.Mana * 0.2f) <=
                      Program.IncDamages.GetAllyData(player.NetworkId).DamageTaken)) || Danger())
                {
                    if (Items.HasItem(Seraph.Id) && Items.CanUseItem(Seraph.Id))
                    {
                        Seraph.Cast();
                    }
                    if (Items.HasItem(SeraphDom.Id) && Items.CanUseItem(SeraphDom.Id))
                    {
                        SeraphDom.Cast();
                    }
                }
            }
            if (Items.HasItem(solari.Id) && Items.CanUseItem(solari.Id) && config.Item("solari").GetValue<bool>())
            {
                if ((config.Item("solariminally").GetValue<Slider>().Value <= player.CountAlliesInRange(solari.Range) &&
                     config.Item("solariminenemy").GetValue<Slider>().Value <= player.CountEnemiesInRange(solari.Range)) ||
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            h => h.IsAlly && !h.IsDead && solari.IsInRange(h) && CombatHelper.CheckCriticalBuffs(h)) !=
                    null ||
                    (Program.IncDamages.IncomingDamagesAlly.Any(
                        a => a.Hero.HealthPercent < 50 && (a.DamageTaken > 150 || a.DamageTaken > a.Hero.Health))))
                {
                    solari.Cast();
                }
            }
            if (config.Item("protoBelt").GetValue<bool>() && target != null && player.Distance(target) < 750)
            {
                if (config.Item("protoBeltEHealth").GetValue<Slider>().Value > target.HealthPercent &&
                    (player.Distance(target) > 150 ||
                     player.Distance(target) > Orbwalking.GetRealAutoAttackRange(target)))
                {
                    if (Items.HasItem(ProtoBelt.Id) && Items.CanUseItem(ProtoBelt.Id))
                    {
                        var pred = Prediction.GetPrediction(
                            target, 0.25f, 100, 2000,
                            new[]
                            {
                                CollisionableObjects.Heroes, CollisionableObjects.Minions, CollisionableObjects.Walls,
                                CollisionableObjects.YasuoWall
                            });
                        if (pred.Hitchance >= HitChance.High)
                        {
                            ProtoBelt.Cast(pred.CastPosition);
                        }
                    }
                }
            }
            if (config.Item("glp").GetValue<bool>() && target != null && player.Distance(target) < 650)
            {
                if (config.Item("glpEHealth").GetValue<Slider>().Value > target.HealthPercent)
                {
                    if (Items.HasItem(GLP.Id) && Items.CanUseItem(GLP.Id))
                    {
                        var pred = Prediction.GetPrediction(
                            target, 0.25f, 100, 1500,
                            new[] { CollisionableObjects.Heroes, CollisionableObjects.YasuoWall });
                        if (pred.Hitchance >= HitChance.High)
                        {
                            GLP.Cast(pred.CastPosition);
                        }
                    }
                }
            }
            if (config.Item("QSSEnabled").GetValue<bool>())
            {
                UseCleanse(config, cleanseSpell);
            }
        }

        private static bool Danger()
        {
            return
                player.Buffs.Any(
                    b => CombatHelper.GetBuffTime(b) < 1f && CombatHelper.dotsHighDmg.Any(a => a == b.Name)) &&
                player.PhysicalShield < 100;
        }

        public static bool IsHeRunAway(Obj_AI_Hero target)
        {
            return (!target.IsFacing(player) &&
                    Prediction.GetPrediction(target, 600, 100f).CastPosition.Distance(player.Position) >
                    target.Position.Distance(player.Position));
        }

        public static void castHydra(Obj_AI_Hero target)
        {
            if (target != null && player.Distance(target) < tiamat.Range)
            {
                if (Items.HasItem((int)ItemId.Ironspike_Whip) && Items.CanUseItem((int)ItemId.Ironspike_Whip) && !Orbwalking.CanAttack())
                {
                    Items.UseItem((int)ItemId.Ironspike_Whip);
                }
                if (Items.HasItem((int)ItemId.Goredrinker) && Items.CanUseItem((int)ItemId.Goredrinker) && !Orbwalking.CanAttack())
                {
                    Items.UseItem((int)ItemId.Goredrinker);
                }
            }
        }

        public static Menu addItemOptons(Menu config)
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);
            var mConfig = config;
            Menu menuI = new Menu("Items ", "Itemsettings");
            menuI = ItemHandler.addCleanseOptions(menuI);
            menuI.AddItem(new MenuItem("hyd", "Hydra/Tiamat")).SetValue(true);
            Menu menuRan = new Menu("Randuin's Omen", "Rands ");
            menuRan.AddItem(new MenuItem("ran", "Enabled")).SetValue(true);
            menuRan.AddItem(new MenuItem("ranmin", "Min enemy")).SetValue(new Slider(2, 1, 6));
            menuI.AddSubMenu(menuRan);

            Menu menuOdin = new Menu("Odyn's Veil ", "Odyns");
            menuOdin.AddItem(new MenuItem("odin", "Enabled")).SetValue(true);
            menuOdin.AddItem(new MenuItem("odinonlyks", "KS only")).SetValue(false);
            menuOdin.AddItem(new MenuItem("odinmin", "Min enemy")).SetValue(new Slider(2, 1, 6));
            menuI.AddSubMenu(menuOdin);
            /*
            Menu menuMura = new Menu("Muramana ", "Mura");
            menuMura.AddItem(new MenuItem("Muramana", "Enabled")).SetValue(true);
            menuMura.AddItem(new MenuItem("MuramanaMinmana", "Min mana")).SetValue(new Slider(40, 0, 100));
            menuI.AddSubMenu(menuMura);
            */
            Menu menuFrost = new Menu("Frost Queen's Claim ", "Frost");
            menuFrost.AddItem(new MenuItem("frost", "Enabled")).SetValue(true);
            menuFrost.AddItem(new MenuItem("frostlow", "Use on low HP")).SetValue(true);
            menuFrost.AddItem(new MenuItem("frostmin", "Min enemy")).SetValue(new Slider(2, 1, 2));
            menuI.AddSubMenu(menuFrost);

            Menu menuZhonya = new Menu("Zhonya's Hourglass ", "Zhonya");
            menuZhonya.AddItem(new MenuItem("Zhonya", "Enabled")).SetValue(true);
            menuZhonya.AddItem(new MenuItem("Zhonyadmg", "Damage in health %")).SetValue(new Slider(100, 0, 100));
            menuI.AddSubMenu(menuZhonya);

            Menu menuSeraph = new Menu("Seraph's Embrace ", "Seraph");
            menuSeraph.AddItem(new MenuItem("Seraph", "Enabled")).SetValue(true);
            menuSeraph.AddItem(new MenuItem("SeraphdmgHP", "Damage in health %")).SetValue(new Slider(100, 0, 100));
            menuSeraph.AddItem(new MenuItem("SeraphdmgSh", "Damage in shield %")).SetValue(new Slider(60, 0, 100));
            menuI.AddSubMenu(menuSeraph);

            Menu menuMountain = new Menu("Face of the Mountain ", "Mountain");
            menuMountain.AddItem(new MenuItem("mountain", "Enabled")).SetValue(true);
            menuMountain.AddItem(new MenuItem("castonme", "SelfCast")).SetValue(true);
            menuMountain.AddItem(new MenuItem("mountainmin", "Under x % health")).SetValue(new Slider(20, 0, 100));
            Menu menuMountainprior = new Menu("Target priority", "MountainPriorityMenu");
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe))
            {
                menuMountainprior.AddItem(new MenuItem("mountainpriority" + ally.ChampionName, ally.ChampionName))
                    .SetValue(new Slider(5, 0, 5));
            }
            menuMountainprior.AddItem(new MenuItem("off", "0 is off"));
            menuMountain.AddSubMenu(menuMountainprior);
            menuI.AddSubMenu(menuMountain);

            Menu menuSolari = new Menu("Locket of the Iron Solari ", "Solari");
            menuSolari.AddItem(new MenuItem("solari", "Enabled")).SetValue(true);
            menuSolari.AddItem(new MenuItem("solariminally", "Min ally")).SetValue(new Slider(2, 1, 6));
            menuSolari.AddItem(new MenuItem("solariminenemy", "Min enemy")).SetValue(new Slider(2, 1, 6));
            menuI.AddSubMenu(menuSolari);

            Menu menuProtoBelt = new Menu("Hextech ProtoBelt-01", "ProtoBelt");
            menuProtoBelt.AddItem(new MenuItem("protoBelt", "Enabled")).SetValue(true);
            menuProtoBelt.AddItem(new MenuItem("protoBeltEHealth", "Under enemy health %"))
                .SetValue(new Slider(60, 0, 100));
            menuI.AddSubMenu(menuProtoBelt);

            Menu menuGLP = new Menu("Hextech GLP-800", "GLP");
            menuGLP.AddItem(new MenuItem("glp", "Enabled")).SetValue(true);
            menuGLP.AddItem(new MenuItem("glpEHealth", "Under enemy health %")).SetValue(new Slider(60, 0, 100));
            menuI.AddSubMenu(menuGLP);

            menuI.AddItem(new MenuItem("you", "Youmuu's Ghostblade")).SetValue(true);
            menuI.AddItem(new MenuItem("useItems", "Use Items")).SetValue(true);
            mConfig.AddSubMenu(menuI);
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            if (player.HasBuff("awesomehealindicator"))
            {
                IsURF = true;
            }

            return mConfig;
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            //if (useHydra && unit.IsMe && hydraTarget != null && target.NetworkId == hydraTarget.NetworkId)
            //{
            //    if (Items.HasItem(titanic.Id) && Items.CanUseItem(titanic.Id))
            //    {
            //        titanic.Cast();
            //        Orbwalking.ResetAutoAttackTimer();
            //    }
            //}
        }

        public static float GetItemsDamage(Obj_AI_Hero target)
        {
            double damage = 0;
            if (Items.HasItem(lich.Id) && Items.CanUseItem(lich.Id))
            {
                damage += player.CalcDamage(
                    target, Damage.DamageType.Magical, player.BaseAttackDamage * 0.75 + player.FlatMagicDamageMod * 0.5);
            }
            if (Items.HasItem(Dfg.Id) && Items.CanUseItem(Dfg.Id))
            {
                damage = damage * 1.2;
                damage += Damage.GetItemDamage(player, target, Damage.DamageItems.Dfg);
            }
            if (Items.HasItem(Bft.Id) && Items.CanUseItem(Bft.Id))
            {
                damage = damage * 1.2;
                damage += Damage.GetItemDamage(player, target, Damage.DamageItems.BlackFireTorch);
            }
            if (Items.HasItem(tiamat.Id) && Items.CanUseItem(tiamat.Id))
            {
                damage += Damage.GetItemDamage(player, target, Damage.DamageItems.Tiamat);
            }
            damage += GetSheenDmg(target);
            return (float)damage;
        }

        public static double GetSheenDmg(Obj_AI_Base target)
        {
            double damage = 0;
            if (Items.HasItem(sheen.Id) && (Items.CanUseItem(sheen.Id) || player.HasBuff("sheen")))
            {
                damage += player.CalcDamage(target, Damage.DamageType.Physical, player.BaseAttackDamage);
            }
            if (Items.HasItem(gaunlet.Id) && Items.CanUseItem(gaunlet.Id))
            {
                damage += player.CalcDamage(target, Damage.DamageType.Physical, player.BaseAttackDamage * 1.25);
            }
            if (Items.HasItem(trinity.Id) && Items.CanUseItem(trinity.Id))
            {
                damage += player.CalcDamage(target, Damage.DamageType.Physical, player.BaseAttackDamage * 2);
            }
            return damage;
        }


        public static Menu addCleanseOptions(Menu config)
        {
            var mConfig = config;
            Menu menuQ = new Menu("QSS", "QSSsettings");
            menuQ.AddItem(new MenuItem("slow", "Slow")).SetValue(false);
            menuQ.AddItem(new MenuItem("blind", "Blind")).SetValue(false);
            menuQ.AddItem(new MenuItem("silence", "Silence")).SetValue(false);
            menuQ.AddItem(new MenuItem("snare", "Snare")).SetValue(false);
            menuQ.AddItem(new MenuItem("stun", "Stun")).SetValue(false);
            menuQ.AddItem(new MenuItem("charm", "Charm")).SetValue(true);
            menuQ.AddItem(new MenuItem("taunt", "Taunt")).SetValue(true);
            menuQ.AddItem(new MenuItem("fear", "Fear")).SetValue(true);
            menuQ.AddItem(new MenuItem("suppression", "Suppression")).SetValue(true);
            menuQ.AddItem(new MenuItem("polymorph", "Polymorph")).SetValue(true);
            menuQ.AddItem(new MenuItem("damager", "Vlad/Zed ult")).SetValue(true);
            menuQ.AddItem(new MenuItem("QSSdelay", "Delay in ms")).SetValue(new Slider(600, 0, 1500));
            menuQ.AddItem(new MenuItem("QSSEnabled", "Enabled")).SetValue(true);
            mConfig.AddSubMenu(menuQ);
            return mConfig;
        }

        public static void UseCleanse(Menu config, bool cleanseSpell)
        {
            if (QssUsed)
            {
                return;
            }
            if (player.ChampionName == "Gangplank" && W.IsReady() && cleanseSpell)
            {
                Cleanse(0, config, cleanseSpell);
            }
            if (Items.CanUseItem((int)ItemId.Quicksilver_Sash) && Items.HasItem((int)ItemId.Quicksilver_Sash))
            {
                Cleanse((int)ItemId.Quicksilver_Sash, config, cleanseSpell);
            }
            if (Items.CanUseItem((int)ItemId.Mercurial_Scimitar) && Items.HasItem((int)ItemId.Mercurial_Scimitar))
            {
                Cleanse((int)ItemId.Mercurial_Scimitar, config, cleanseSpell);
            }
            if (Items.CanUseItem((int)ItemId.Silvermere_Dawn) && Items.HasItem((int)ItemId.Silvermere_Dawn))
            {
                Cleanse((int)ItemId.Silvermere_Dawn, config, cleanseSpell);
            }
        }

        private static void Cleanse(int Item, Menu config, bool useSpell = false)
        {
            var delay = config.Item("QSSdelay").GetValue<Slider>().Value + _random.Next(0, 120);
            foreach (var buff in player.Buffs)
            {
                if (config.Item("slow").GetValue<bool>() && buff.Type == BuffType.Slow)
                {
                    CastQSS(delay, Item);
                    return;
                }
                if (config.Item("blind").GetValue<bool>() && buff.Type == BuffType.Blind)
                {
                    CastQSS(delay, Item);
                    return;
                }
                if (config.Item("silence").GetValue<bool>() && buff.Type == BuffType.Silence)
                {
                    CastQSS(delay, Item);
                    return;
                }
                if (config.Item("snare").GetValue<bool>() && buff.Type == BuffType.Snare)
                {
                    CastQSS(delay, Item);
                    return;
                }
                if (config.Item("stun").GetValue<bool>() && buff.Type == BuffType.Stun)
                {
                    CastQSS(delay, Item);
                    return;
                }
                if (config.Item("charm").GetValue<bool>() && buff.Type == BuffType.Charm)
                {
                    CastQSS(delay, Item);
                    return;
                }
                if (config.Item("taunt").GetValue<bool>() && buff.Type == BuffType.Taunt)
                {
                    CastQSS(delay, Item);
                    return;
                }
                if (config.Item("fear").GetValue<bool>() && (buff.Type == BuffType.Fear || buff.Type == BuffType.Flee))
                {
                    CastQSS(delay, Item);
                    return;
                }
                if (config.Item("suppression").GetValue<bool>() && buff.Type == BuffType.Suppression)
                {
                    CastQSS(delay, Item);
                    return;
                }
                if (config.Item("polymorph").GetValue<bool>() && buff.Type == BuffType.Polymorph)
                {
                    CastQSS(delay, Item);
                    return;
                }
                if (config.Item("damager").GetValue<bool>())
                {
                    switch (buff.Name)
                    {
                        case "zedulttargetmark":
                            CastQSS(2900, Item);
                            break;
                        case "VladimirHemoplague":
                            CastQSS(4900, Item);
                            break;
                        case "MordekaiserChildrenOfTheGrave":
                            CastQSS(delay, Item);
                            break;
                        case "urgotswap2":
                            CastQSS(delay, Item);
                            break;
                        case "skarnerimpale":
                            CastQSS(delay, Item);
                            break;
                    }
                }
            }
        }

        private static void CastQSS(int delay, int item)
        {
            QssUsed = true;
            if (player.ChampionName == "Gangplank" && W.IsReady())
            {
                Utility.DelayAction.Add(
                    delay, () =>
                    {
                        W.Cast();
                        QssUsed = false;
                    });
                return;
            }
            else
            {
                Utility.DelayAction.Add(
                    delay, () =>
                    {
                        Items.UseItem(item, player);
                        QssUsed = false;
                    });
                return;
            }
        }


        private static void Game_OnGameUpdate(EventArgs args)
        {
            /*
                        var deltaT = System.Environment.TickCount - MuramanaTime;
                        if (MuramanaEnabled && ((deltaT > 500 && deltaT < 1000) || player.InFountain()))
                        {
                            if (Muramana.IsOwned() && Muramana.IsReady())
                            {
                                Muramana.Cast();
                            }
                            if (Muramana2.IsOwned() && Muramana2.IsReady())
                            {
                                Muramana2.Cast();
                            }
                        }*/
        }

        public static bool MuramanaEnabled
        {
            get { return player.HasBuff("Muramana"); }
        }
    }
}
