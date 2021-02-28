using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using AutoJungle.Data;
using System.IO;
using LeagueSharp;
using System.Text;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Resources;
using System.Threading.Tasks;

namespace AutoJungle
{
    internal class Program
    {
        public static GameInfo _GameInfo = null;

        public static Menu menu;

        public static float UpdateLimiter, ResetTimer, GameStateChanging;

        public static Obj_AI_Hero player;

        public static Random Random = new Random(Environment.TickCount);

        public static ItemHandler ItemHandler;

        public static Vector3 pos;

        public static OrbwalkingForBots.Orbwalker orbwalker;

        public static ResourceManager resourceM;
        public static string culture;
        public static String[] languages = new String[] { "English", "Chinese (Simplified)", "Chinese (Traditional)" };
        public static String[] languagesShort = new String[] { "en", "cn", "tw" };
        public static string fileName, path;

        #region Main

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_GameInfo.SmiteableMob != null)
            {
                Jungle.CastSmite(_GameInfo.SmiteableMob);
            }
            CastHighPrioritySpells();
            if (ShouldSkipUpdate())
            {
                return;
            }
            SetGameInfo();
            if (_GameInfo.WaitOnFountain)
            {
                return;
            }

            if (menu.Item("WaitAtLvlTWO", true).GetValue<bool>() && Jungle.smite != null &&
                Jungle.smite.Instance.Ammo == 0 && player.Level == 2 && _GameInfo.GameState == State.Positioning &&
                player.HealthPercent < 55 && player.Distance(_GameInfo.MonsterList.First().Position) < 700)
            {
                _GameInfo.afk = 0;
                return;
            }
            //Checking Afk
            if (CheckAfk())
            {
                return;
            }
            if (HighPriorityPositioning())
            {
                MoveToPos();
                return;
            }

            //Check the camp, maybe its cleared
            CheckCamp();
            if (Debug)
            {
                /* Console.WriteLine("Items: ");
                foreach (var i in player.InventoryItems)
                {
                    Console.WriteLine("\t Name: {0}, ID: {1}({2})", i.IData.TranslatedDisplayName, i.Id, (int) i.Id);
                }*/
                _GameInfo.Show();
            }
            //Shopping
            if (Shopping())
            {
                return;
            }

            //Recalling
            if (RecallHander())
            {
                return;
            }
            if (menu.Item("UseTrinket").GetValue<bool>())
            {
                PlaceWard();
            }
            MoveToPos();

            CastSpells();
        }

        private static void CastHighPrioritySpells()
        {
            var target = _GameInfo.Target;
            switch (ObjectManager.Player.ChampionName)
            {
                case "Jax":
                    var eActive = player.HasBuff("JaxCounterStrike");
                    if (_GameInfo.GameState == State.Jungling || _GameInfo.GameState == State.LaneClear)
                    {
                        var targetMob = _GameInfo.Target;
                        if (Champdata.E.IsReady() && targetMob.IsValidTarget(350) &&
                            (player.ManaPercent > 40 || player.HealthPercent < 60 || player.Level == 1) && !eActive &&
                            _GameInfo.DamageCount >= 2 || _GameInfo.DamageTaken > player.Health * 0.2f)
                        {
                            Champdata.E.Cast();
                        }
                        return;
                    }
                    if (_GameInfo.GameState == State.FightIng)
                    {
                        if (Champdata.E.IsReady() && Champdata.R.IsReady() &&
                            ((Champdata.Q.CanCast(target) && !eActive) || (target.IsValidTarget(350)) ||
                             ((_GameInfo.DamageCount >= 2 || _GameInfo.DamageTaken > player.Health * 0.2f) || !eActive)))
                        {
                            Champdata.E.Cast();
                        }
                        return;
                    }
                    break;

                case "Kayle":
                    var rActive3 = player.HasBuff("JudicatorIntervention");
                    if (_GameInfo.GameState == State.FightIng)
                    {
                        var targetHero = _GameInfo.Target;
                        if (Champdata.R.IsReady() && targetHero.IsValidTarget(525) && player.HealthPercent <= 25 &&
                            !rActive3)
                        {
                            Champdata.R.Cast();
                        }
                        return;
                    }
                    if (_GameInfo.GameState == State.Jungling || _GameInfo.GameState == State.LaneClear)
                    {
                        var targetMob = _GameInfo.Target;
                        if (Champdata.R.IsReady() && targetMob.IsValidTarget(525) && player.HealthPercent <= 25 &&
                            !rActive3)
                        {
                            Champdata.R.Cast();
                        }
                        return;
                    }
                    break;
                /*
                                case "Nunu":
                                    var rActive = player.HasBuff("AbsoluteZero");
                                    if (_GameInfo.GameState == State.FightIng)
                                    {
                                        var targetHero = _GameInfo.Target;
                                        if (player.Spellbook.IsChanneling && (targetHero.IsValidTarget(650)))
                                        {
                                            return;
                                        }
                                    }
                                    break;
                */

                case "Udyr":
                    var rActive2 = !player.HasBuff("UdyrPhoenixStance");
                    if (_GameInfo.GameState == State.Jungling || _GameInfo.GameState == State.LaneClear)
                    {
                        var targetMob = _GameInfo.Target;
                        if (Champdata.R.IsReady() && targetMob.IsValidTarget(135) &&
                            (player.ManaPercent > 25 || player.Level == 1) && !rActive2)
                        {
                            Champdata.R.Cast();
                        }
                        return;
                    }
                    break;
            }
        }

        private static bool HighPriorityPositioning()
        {
            if (player.ChampionName == "Skarner")
            {
                var capturablePoints =
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(o => o.Distance(player) < 700 && !o.IsAlly && o.Name == "SkarnerPassiveCrystal")
                        .OrderBy(o => o.Distance(player))
                        .FirstOrDefault();
                if (capturablePoints != null)
                {
                    _GameInfo.MoveTo = capturablePoints.Position;
                    _GameInfo.GameState = State.Positioning;
                    return true;
                }
            }
            return false;
        }

        private static void PlaceWard()
        {
            if (_GameInfo.ClosestWardPos.IsValid() && Items.CanUseItem(3340))
            {
                Items.UseItem(3340, _GameInfo.ClosestWardPos);
            }
        }

        private static bool CheckAfk()
        {
            if (player.IsMoving || player.IsWindingUp || player.IsRecalling() || player.Level == 1)
            {
                _GameInfo.afk = 0;
            }
            else
            {
                _GameInfo.afk++;
            }
            if (_GameInfo.afk > 15 && !player.InFountain())
            {
                player.Spellbook.CastSpell(SpellSlot.Recall);
                return true;
            }
            return false;
        }

        private static void CheckCamp()
        {
            MonsterInfo nextMob = GetNextMob();
            if (nextMob != null && !nextMob.IsAlive())
            {
                //Console.WriteLine(nextMob.name + " skipped: " + (Environment.TickCount - nextMob.TimeAtDead / 1000f));
                _GameInfo.CurrentMonster++;
                _GameInfo.MoveTo = nextMob.Position;
                nextMob =
                    _GameInfo.MonsterList.OrderBy(m => m.Index).FirstOrDefault(m => m.Index == _GameInfo.CurrentMonster);
            }
            if (_GameInfo.GameState == State.Positioning)
            {
                if (Helpers.GetRealDistance(player, _GameInfo.MoveTo) < 500 && _GameInfo.MinionsAround == 0 &&
                    player.Level > 1)
                {
                    _GameInfo.CurrentMonster++;
                    if (nextMob != null)
                    {
                        _GameInfo.MoveTo = nextMob.Position;
                    }
                    //Console.WriteLine("CheckCamp - MoveTo: CurrentMonster++");
                }

                var probablySkippedMob = Helpers.GetNearest(player.Position, 1000);
                if (probablySkippedMob != null && probablySkippedMob.Distance(_GameInfo.MoveTo) > 200)
                {
                    var monster = _GameInfo.MonsterList.FirstOrDefault(m => probablySkippedMob.Name.Contains(m.name));
                    if (monster != null && monster.Index < 13)
                    {
                        _GameInfo.MoveTo = probablySkippedMob.Position;
                    }
                }
            }
        }

        private static void SetGameInfo()
        {
            _GameInfo.GroupWithoutTarget = false;
            ResetDamageTakenTimer();
            _GameInfo.WaitOnFountain = WaitOnFountain();
            _GameInfo.ShouldRecall = ShouldRecall();
            _GameInfo.GameState = SetGameState();
            _GameInfo.MoveTo = GetMovePosition();
            _GameInfo.Target = GetTarget();
            _GameInfo.MinionsAround = Helpers.getMobs(player.Position, 700).Count;
            _GameInfo.SmiteableMob = Helpers.GetNearest(player.Position);
            _GameInfo.AllyStructures = GetStructures(true, _GameInfo.SpawnPointEnemy);
            _GameInfo.EnemyStructures = GetStructures(false, _GameInfo.SpawnPoint);
            _GameInfo.ClosestWardPos = Helpers.GetClosestWard();
            _GameInfo.Champdata.Autolvl.LevelSpells();
        }

        private static IEnumerable<Vector3> GetStructures(bool ally, Vector3 basePos)
        {
            var turrets =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(t => t.IsAlly == ally && t.IsValid && t.Health > 0 && t.Position.Distance(basePos) > 700)
                    .OrderBy(t => t.Position.Distance(basePos))
                    .Select(t => t.Position);
            var inhibs =
                ObjectManager.Get<Obj_BarracksDampener>()
                    .Where(t => t.IsAlly == ally && t.IsValid && t.Health > 0)
                    .OrderBy(t => t.Position.Distance(basePos))
                    .Select(t => t.Position);
            var nexus =
                ObjectManager.Get<Obj_HQ>()
                    .Where(t => t.IsAlly == ally && t.IsValid && t.Health > 0)
                    .OrderBy(t => t.Position.Distance(basePos))
                    .Select(t => t.Position);

            return turrets.Concat(inhibs).Concat(nexus);
        }

        #region MainFunctions

        private static bool RecallHander()
        {
            if ((_GameInfo.GameState != State.Positioning && _GameInfo.GameState != State.Retreat) ||
                !_GameInfo.MonsterList.Any(m => m.Position.Distance(player.Position) < 800))
            {
                return false;
            }
            if (Helpers.getMobs(player.Position, 1300).Count > 0)
            {
                return false;
            }
            if (player.InFountain() || player.ServerPosition.Distance(_GameInfo.SpawnPoint) < 1000)
            {
                return false;
            }
            if ((_GameInfo.ShouldRecall && !player.IsRecalling() && !player.InFountain()) &&
                (_GameInfo.GameState == State.Positioning ||
                 (_GameInfo.GameState == State.Retreat &&
                  (_GameInfo.afk > 15 ||
                   ObjectManager.Get<Obj_AI_Base>().Count(o => o.IsEnemy && o.Distance(player) < 2000) == 0))))
            {
                if (player.Distance(_GameInfo.SpawnPoint) > 6000)
                {
                    Console.WriteLine(
                        "recalling" + Environment.TickCount + player.InFountain() +
                        (player.ServerPosition.Distance(_GameInfo.SpawnPoint) < 1000));
                    player.Spellbook.CastSpell(SpellSlot.Recall);
                }
                else
                {
                    player.IssueOrder(GameObjectOrder.MoveTo, _GameInfo.SpawnPoint);
                }
                return true;
            }

            if (player.IsRecalling())
            {
                return true;
            }

            return false;
        }

        private static void CastSpells()
        {
            if (_GameInfo.GameState == State.LaneClear || _GameInfo.GameState == State.Objective ||
                _GameInfo.GameState == State.Jungling || _GameInfo.GameState == State.Retreat)
            {
                Champdata.UseSpellsDef();
            }
            if (_GameInfo.Target == null)
            {
                return;
            }
            switch (_GameInfo.GameState)
            {
                case State.FightIng:
                    _GameInfo.Champdata.Combo();
                    Champdata.UseSpellsCombo();
                    break;
                case State.Ganking:
                    break;
                case State.Jungling:
                    _GameInfo.Champdata.JungleClear();
                    UsePotions();
                    break;
                case State.LaneClear:
                    _GameInfo.Champdata.JungleClear();
                    UsePotions();
                    break;
                case State.Objective:
                    if (_GameInfo.Target is Obj_AI_Hero)
                    {
                        _GameInfo.Champdata.Combo();
                    }
                    else
                    {
                        _GameInfo.Champdata.JungleClear();
                    }
                    break;
                case State.Retreat:
                    break;
                default:
                    break;
            }
        }

        private static void UsePotions()
        {
            if (Items.HasItem(2031) && Items.CanUseItem(2031) &&
                player.HealthPercent < menu.Item("HealthToPotion").GetValue<Slider>().Value &&
                !player.Buffs.Any(b => b.Name.Equals("ItemCrystalFlask")))
            {
                Items.UseItem(2031);
            }
        }

        private static void MoveToPos()
        {
            if ((_GameInfo.GameState != State.Positioning && _GameInfo.GameState != State.Ganking &&
                 _GameInfo.GameState != State.Retreat && _GameInfo.GameState != State.Grouping) ||
                !_GameInfo.MoveTo.IsValid())
            {
                return;
            }
            if (!Helpers.CheckPath(player.GetPath(_GameInfo.MoveTo)))
            {
                _GameInfo.CurrentMonster++;
                if (Debug)
                {
                    Console.WriteLine("MoveTo: CurrentMonster++2");
                }
            }
            if (_GameInfo.GameState == State.Retreat && _GameInfo.MoveTo.Distance(player.Position) < 100)
            {
                return;
            }
            if (_GameInfo.MoveTo.IsValid() &&
                (_GameInfo.MoveTo.Distance(_GameInfo.LastClick) > 150 || (!player.IsMoving && _GameInfo.afk > 10)))
            {
                if (player.IsMoving)
                {
                    int x, y;
                    x = (int) _GameInfo.MoveTo.X;
                    y = (int) _GameInfo.MoveTo.Y;
                    player.IssueOrder(
                        GameObjectOrder.MoveTo,
                        new Vector3(Random.Next(x, x + 100), Random.Next(y, y + 100), _GameInfo.MoveTo.Z));
                }
                else
                {
                    player.IssueOrder(GameObjectOrder.MoveTo, _GameInfo.MoveTo);
                }
            }
        }

        private static bool Shopping()
        {
            if (!player.InFountain())
            {
                if (Debug)
                {
                    Console.WriteLine("Shopping: Not in shop - false");
                }
                return false;
            }

            if (ObjectManager.Player.HasBuff("ElixirOfWrath") || ObjectManager.Player.HasBuff("ElixirOfIron") ||
                ObjectManager.Player.HasBuff("ElixirOfSorcery"))
            {
                return false;
            }

            var current =
                ItemHandler.ItemList.Where(i => Items.HasItem(i.ItemId))
                    .OrderByDescending(i => i.Index)
                    .FirstOrDefault();

            if (current != null)
            {
                var currentIndex = current.Index;
                var orderedList =
                    ItemHandler.ItemList.Where(i => !Items.HasItem(i.ItemId) && i.Index > currentIndex)
                        .OrderBy(i => i.Index);
                var itemToBuy = orderedList.FirstOrDefault();
                if (itemToBuy == null)
                {
                    if (Debug)
                    {
                        Console.WriteLine("Shopping: No next Item - false");
                    }
                    return false;
                }
               /* if (itemToBuy.Price <= player.Level)
                {
                    player.BuyItem((ItemId) itemToBuy.ItemId);
                    UpdateLimiter += new Random().Next(1000, 1800);
                    if (itemToBuy.Index > 9 && Items.HasItem(2031))
                    {
                        player.BuyItem(ItemId.Refillable_Potion);
                    }
                    var nextItem = orderedList.FirstOrDefault(i => i.Index == itemToBuy.Index + 1);
                    if (nextItem != null)
                    {
                        _GameInfo.NextItemPrice = nextItem.Price;
                    }
                    if (Debug)
                    {
                        Console.WriteLine("Shopping: Shopping- " + itemToBuy.Name + " - true");
                    }
                    return true;
                }*/
            }
            else
            {
                player.BuyItem((ItemId) ItemHandler.ItemList.FirstOrDefault(i => i.Index == 1).ItemId);
                var nextItem = ItemHandler.ItemList.FirstOrDefault(i => i.Index == 2);
                if (nextItem != null)
                {
                    _GameInfo.NextItemPrice = nextItem.Price;
                }
                return true;
            }


            if (Debug)
            {
                Console.WriteLine("Shopping: End - false");
            }
            return false;
        }

        private static Obj_AI_Base GetTarget()
        {
            var enemyTurret =
                ObjectManager.Get<Obj_AI_Turret>()
                    .FirstOrDefault(
                        t =>
                            t.IsEnemy && !t.IsDead && t.Distance(player) < 2000 &&
                            Helpers.getAllyMobs(t.Position, 1000).Count(m => m.UnderTurret(true)) > 1);
            switch (_GameInfo.GameState)
            {
                case State.Objective:
                    var obj = Helpers.GetNearest(player.Position, GameInfo.ChampionRange);
                    if (obj != null && (obj.Name.Contains("Dragon") || obj.Name.Contains("Baron")) &&
                        (HealthPrediction.GetHealthPrediction(obj, 3000) + 500 < Jungle.smiteDamage(obj) ||
                         (_GameInfo.EnemiesAround == 0 && player.Level > 8 &&
                          MinionManager.GetMinions(
                              player.Position, GameInfo.ChampionRange, MinionTypes.All, MinionTeam.NotAlly)
                              .Take(5)
                              .FirstOrDefault(m => m.Name.Contains("Sru_Crab") && m.Health < m.MaxHealth) == null)))
                    {
                        return obj;
                    }
                    else
                    {
                        return _GameInfo.EnemiesAround > 0 ? Helpers.GetTargetEnemy() : null;
                    }
                    break;
                case State.FightIng:
                    return Helpers.GetTargetEnemy();
                    break;
                case State.Ganking:
                    return null;
                    break;
                case State.Jungling:
                    return Helpers.getMobs(player.Position, 1000).OrderByDescending(m => m.MaxHealth).FirstOrDefault();
                    break;
                case State.LaneClear:
                    var moblc =
                        Helpers.getMobs(player.Position, GameInfo.ChampionRange)
                            .Where(m => !m.UnderTurret(true))
                            .OrderByDescending(m => player.GetAutoAttackDamage(m, true) > m.Health)
                            .ThenBy(m => m.Distance(player))
                            .FirstOrDefault();
                    if (moblc != null)
                    {
                        _GameInfo.Target = moblc;
                        return moblc;
                    }
                    if (enemyTurret != null)
                    {
                        return enemyTurret;
                    }

                    break;
                case State.Pushing:
                    var enemy = Helpers.GetTargetEnemy();
                    if (enemy != null)
                    {
                        _GameInfo.Target = enemy;
                        _GameInfo.Champdata.Combo();
                        return enemy;
                    }
                    var mob =
                        Helpers.getMobs(player.Position, GameInfo.ChampionRange)
                            .Where(
                                m =>
                                    (!m.UnderTurret(true) ||
                                     (enemyTurret != null &&
                                      Helpers.getAllyMobs(enemyTurret.Position, 1000).Count(o => o.UnderTurret(true)) >
                                      0)))
                            .OrderByDescending(m => player.GetAutoAttackDamage(m, true) > m.Health)
                            .ThenBy(m => m.Distance(player))
                            .FirstOrDefault();
                    if (mob != null)
                    {
                        _GameInfo.Target = mob;
                        _GameInfo.Champdata.JungleClear();
                        return mob;
                    }
                    if (enemyTurret != null)
                    {
                        _GameInfo.Champdata.JungleClear();
                        return enemyTurret;
                    }
                    break;
                case State.Defending:
                    var enemyDef = Helpers.GetTargetEnemy();
                    if (enemyDef != null && !_GameInfo.InDanger)
                    {
                        _GameInfo.Target = enemyDef;
                        _GameInfo.Champdata.Combo();
                        return enemyDef;
                    }
                    var mobDef =
                        Helpers.getMobs(player.Position, GameInfo.ChampionRange)
                            .OrderByDescending(m => m.CountEnemiesInRange(500) == 0)
                            .ThenByDescending(m => player.GetAutoAttackDamage(m, true) > m.Health)
                            .ThenBy(m => m.CountEnemiesInRange(500))
                            .FirstOrDefault();
                    if (mobDef != null)
                    {
                        _GameInfo.Target = mobDef;
                        _GameInfo.Champdata.JungleClear();
                        return mobDef;
                    }
                    break;
                default:
                    break;
            }

            if (Debug)
            {
                Console.WriteLine("GetTarget: Cant get target");
            }
            return null;
        }


        private static bool CheckObjective(Vector3 pos)
        {
            if ((pos.CountEnemiesInRange(800) > 0 || pos.CountAlliesInRange(800) > 0) && !CheckForRetreat(null, pos))
            {
                var obj = Helpers.GetNearest(pos);
                if (obj != null && obj.Health < obj.MaxHealth - 300)
                {
                    if (player.Distance(pos) > Jungle.smiteRange)
                    {
                        _GameInfo.MoveTo = pos;
                        return true;
                    }
                }
            }
            if ((Jungle.SmiteReady() || (player.Level >= 11 && player.HealthPercent > 80)) && player.Level >= 9 &&
                player.Distance(Camps.Dragon.Position) < GameInfo.ChampionRange)
            {
                var drake = Helpers.GetNearest(player.Position, GameInfo.ChampionRange);
                if (drake != null && drake.Name.Contains("Dragon"))
                {
                    _GameInfo.CurrentMonster = 13;
                    _GameInfo.MoveTo = drake.Position;
                    return true;
                }
            }
            return false;
        }

        private static bool CheckGanking()
        {
            Obj_AI_Hero gankTarget = null;
            if (player.Level >= menu.Item("GankLevel").GetValue<Slider>().Value &&
                ((player.Mana > Champdata.R.ManaCost && player.MaxMana > 100) || player.MaxMana <= 100))
            {
                var heroes =
                    HeroManager.Enemies.Where(
                        e =>
                            e.Distance(player) < menu.Item("GankRange").GetValue<Slider>().Value && e.IsValidTarget() &&
                            !e.UnderTurret(true) && !CheckForRetreat(e, e.Position)).OrderBy(e => player.Distance(e));
                foreach (var possibleTarget in heroes)
                {
                    var myDmg = Helpers.GetComboDMG(player, possibleTarget);
                    if (player.Level + 1 <= possibleTarget.Level)
                    {
                        continue;
                    }
                    if (Helpers.AlliesThere(possibleTarget.Position, 3000) + 1 <
                        possibleTarget.Position.CountEnemiesInRange(GameInfo.ChampionRange))
                    {
                        continue;
                    }
                    if (Helpers.GetComboDMG(possibleTarget, player) > player.Health)
                    {
                        continue;
                    }
                    var ally =
                        HeroManager.Allies.Where(a => !a.IsDead && a.Distance(possibleTarget) < 3000)
                            .OrderBy(a => a.Distance(possibleTarget))
                            .FirstOrDefault();
                    var hp = possibleTarget.Health - myDmg * menu.Item("GankFrequency").GetValue<Slider>().Value / 100f;
                    if (ally != null)
                    {
                        hp -= Helpers.GetComboDMG(ally, possibleTarget) *
                              menu.Item("GankFrequency").GetValue<Slider>().Value / 100;
                    }
                    if (hp < 0)
                    {
                        gankTarget = possibleTarget;
                        break;
                    }
                }
            }
            if (gankTarget != null)
            {
                if (Debug)
                {
                    Console.WriteLine("Gankable: " + gankTarget.Name);
                }
                var gankPosition =
                    Helpers.GankPos.Where(p => p.Distance(gankTarget.Position) < 2000)
                        .OrderBy(p => player.Distance(gankTarget.Position))
                        .FirstOrDefault();
                if (gankTarget.Distance(player) > 2000 && gankPosition.IsValid() &&
                    gankPosition.Distance(gankTarget.Position) < 2000 &&
                    player.Distance(gankTarget) > gankPosition.Distance(gankTarget.Position))
                {
                    _GameInfo.MoveTo = gankPosition;
                    return true;
                }
                else if (gankTarget.Distance(player) <= 2000)
                {
                    _GameInfo.MoveTo = gankTarget.Position;
                    return true;
                }
                else if (!gankPosition.IsValid())
                {
                    _GameInfo.MoveTo = gankTarget.Position;
                    return true;
                }
            }
            return false;
        }

        private static State SetGameState()
        {
            var enemy = Helpers.GetTargetEnemy();
            State tempstate = State.Null;
            if (CheckForRetreat(enemy, player.Position))
            {
                tempstate = State.Retreat;
            }
            if (tempstate == State.Null && _GameInfo.EnemiesAround == 0 &&
                (CheckObjective(Camps.Baron.Position) || CheckObjective(Camps.Dragon.Position)))
            {
                tempstate = State.Objective;
            }
            if (tempstate == State.Null && _GameInfo.GameState != State.Retreat && _GameInfo.GameState != State.Pushing &&
                _GameInfo.GameState != State.Defending &&
                ((enemy != null && !CheckForRetreat(enemy, enemy.Position) &&
                  Helpers.GetRealDistance(player, enemy.Position) < GameInfo.ChampionRange)) ||
                player.HasBuff("skarnerimpalevo"))
            {
                tempstate = State.FightIng;
            }
            if (tempstate == State.Null && player.Level >= 6 && CheckForGrouping())
            {
                if (_GameInfo.MoveTo.Distance(player.Position) <= GameInfo.ChampionRange)
                {
                    if (
                        ObjectManager.Get<Obj_AI_Turret>()
                            .FirstOrDefault(t => t.Distance(_GameInfo.MoveTo) < 2000 && t.IsAlly) != null &&
                        (_GameInfo.GameState == State.Grouping || _GameInfo.GameState == State.Defending))
                    {
                        tempstate = State.Defending;
                    }
                    else if (_GameInfo.GameState != State.Grouping && _GameInfo.GameState != State.Retreat &&
                             _GameInfo.GameState != State.Jungling)
                    {
                        tempstate = State.Pushing;
                    }
                }
                if (tempstate == State.Null &&
                    (_GameInfo.MoveTo.Distance(player.Position) > GameInfo.ChampionRange || _GameInfo.GroupWithoutTarget) &&
                    (_GameInfo.GameState == State.Positioning || _GameInfo.GameState == State.Grouping))
                {
                    tempstate = State.Grouping;
                }
            }
            if (tempstate == State.Null && _GameInfo.EnemiesAround == 0 &&
                (_GameInfo.GameState == State.Ganking || _GameInfo.GameState == State.Positioning) && CheckGanking())
            {
                tempstate = State.Ganking;
            }
            if (tempstate == State.Null && _GameInfo.MinionsAround > 0 &&
                (_GameInfo.MonsterList.Any(m => m.Position.Distance(player.Position) < 700) ||
                 _GameInfo.SmiteableMob != null) && _GameInfo.GameState != State.Retreat)
            {
                tempstate = State.Jungling;
            }
            if (tempstate == State.Null && CheckLaneClear(player.Position))
            {
                tempstate = State.LaneClear;
            }
            if (tempstate == State.Null)
            {
                tempstate = State.Positioning;
            }
            if (tempstate == _GameInfo.GameState)
            {
                return tempstate;
            }
            else if (Environment.TickCount - GameStateChanging > 1300 || _GameInfo.GameState == State.Retreat ||
                     tempstate == State.FightIng)
            {
                GameStateChanging = Environment.TickCount;
                return tempstate;
            }
            else
            {
                return _GameInfo.GameState;
            }
        }

        private static bool CheckLaneClear(Vector3 pos)
        {
            return (Helpers.AlliesThere(pos) == 0 || Helpers.AlliesThere(pos) >= 2 ||
                    player.Distance(_GameInfo.SpawnPoint) < 6000 || player.Distance(_GameInfo.SpawnPointEnemy) < 6000 ||
                    player.Level >= 10) && pos.CountEnemiesInRange(GameInfo.ChampionRange) == 0 &&
                   Helpers.getMobs(pos, GameInfo.ChampionRange).Count +
                   _GameInfo.EnemyStructures.Count(
                       p =>
                           p.Distance(pos) < GameInfo.ChampionRange &&
                           Helpers.getAllyMobs(p, 1000).Count(m => m.UnderTurret(true)) > 1) > 0 &&
                   !_GameInfo.MonsterList.Any(m => m.Position.Distance(pos) < 600) && _GameInfo.SmiteableMob == null &&
                   _GameInfo.GameState != State.Retreat;
        }

        private static bool CheckForRetreat(Obj_AI_Base enemy, Vector3 pos)
        {
            if (_GameInfo.GameState == State.Jungling)
            {
                return false;
            }
            if (enemy != null && !enemy.UnderTurret(true) && player.Distance(enemy) < 350 && !_GameInfo.AttackedByTurret)
            {
                return false;
            }
            var indanger = ((Helpers.GetHealth(true, pos) +
                             ((player.Distance(pos) < GameInfo.ChampionRange) ? 0 : player.Health)) * 1.3f <
                            Helpers.GetHealth(false, pos) && pos.CountEnemiesInRange(GameInfo.ChampionRange) > 1 &&
                            Helpers.AlliesThere(pos, 500) == 0) ||
                           player.HealthPercent < menu.Item("HealtToBack").GetValue<Slider>().Value;
            if (indanger || _GameInfo.AttackedByTurret)
            {
                if (((enemy != null && Helpers.AlliesThere(pos, 600) > 0) && player.HealthPercent > 25))
                {
                    return false;
                }
                if (_GameInfo.AttackedByTurret)
                {
                    if ((enemy != null &&
                         (enemy.Health > player.GetAutoAttackDamage(enemy, true) * 2 ||
                          enemy.Distance(player) > Orbwalking.GetRealAutoAttackRange(enemy) + 20) || enemy == null))
                    {
                        return true;
                    }
                }
                if (indanger)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CheckForGrouping()
        {
            if (Debug)
            {
                Console.WriteLine("---------------" + _GameInfo.GameState + "---------------");
            }
            //Checking grouping allies
            var ally =
                HeroManager.Allies.FirstOrDefault(
                    a => Helpers.AlliesThere(a.Position) >= 2 && a.Distance(_GameInfo.SpawnPointEnemy) < 7000);
            if (ally != null && CheckLaneClear(ally.Position) && !CheckForRetreat(null, ally.Position) &&
                Helpers.CheckPath(player.GetPath(ally.Position)))
            {
                _GameInfo.MoveTo = ally.Position.Extend(player.Position, 200);
                _GameInfo.GroupWithoutTarget = true;
                if (Debug)
                {
                    Console.WriteLine("True - CheckForGrouping() - Checking grouping allies");
                    Console.WriteLine("\t" + ally.Name);
                    Console.WriteLine("\t" + CheckForRetreat(null, ally.Position));
                    Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(ally.Position)));
                }
                return true;
            }
            if (Debug)
            {
                Console.WriteLine("False - CheckForGrouping() - Checking grouping allies");
                Console.WriteLine("\t" + ally != null);
                if (ally != null)
                {
                    Console.WriteLine("\t" + ally.Name);
                    Console.WriteLine("\t" + CheckForRetreat(null, ally.Position));
                    Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(ally.Position)));
                }
            }

            //Checknig base after recall
            if (player.Distance(_GameInfo.SpawnPoint) < 5000)
            {
                var mob =
                    Helpers.getMobs(_GameInfo.SpawnPoint, 5000)
                        .OrderByDescending(m => Helpers.getMobs(m.Position, 300).Count)
                        .FirstOrDefault();
                if (mob != null && Helpers.getMobs(mob.Position, 300).Count > 700 &&
                    Helpers.CheckPath(player.GetPath(mob.Position)) && !CheckForRetreat(null, mob.Position))
                {
                    _GameInfo.MoveTo = mob.Position;
                    if (Debug)
                    {
                        Console.WriteLine("True - CheckForGrouping() - Checknig base after recall");
                        Console.WriteLine("\t" + Helpers.getMobs(mob.Position, 300).Count);
                        Console.WriteLine("\t" + CheckForRetreat(null, mob.Position));
                        Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(mob.Position)));
                    }
                    return true;
                }
                if (Debug)
                {
                    Console.WriteLine("False - CheckForGrouping() - Checknig base after recall");
                    Console.WriteLine("\t" + mob != null);
                    if (mob != null)
                    {
                        Console.WriteLine("\t" + Helpers.getMobs(mob.Position, 300).Count);
                        Console.WriteLine("\t" + CheckForRetreat(null, mob.Position));
                        Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(mob.Position)));
                    }
                }
            }

            //Checknig enemy turrets
            foreach (var vector in
                _GameInfo.EnemyStructures.Where(
                    s =>
                        s.Distance(player.Position) < menu.Item("GankRange").GetValue<Slider>().Value &&
                        CheckLaneClear(s)))
            {
                var aMinis = Helpers.getAllyMobs(vector, GameInfo.ChampionRange);
                if (aMinis.Count > 1)
                {
                    var eMinis =
                        Helpers.getMobs(vector, GameInfo.ChampionRange)
                            .OrderByDescending(m => Helpers.getMobs(m.Position, 300).Count)
                            .FirstOrDefault();
                    if (eMinis != null)
                    {
                        var pos = eMinis.Position;
                        if (Helpers.CheckPath(player.GetPath(pos)) && !CheckForRetreat(null, pos))
                        {
                            _GameInfo.MoveTo = pos;
                            if (Debug)
                            {
                                Console.WriteLine("True - CheckForGrouping() - Checknig enemy turrets 1");
                                Console.WriteLine("\t" + CheckForRetreat(null, pos));
                                Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(pos)));
                            }
                            return true;
                        }
                        if (Debug)
                        {
                            Console.WriteLine("False - CheckForGrouping() - Checknig enemy turrets 1");
                            Console.WriteLine("\t" + CheckForRetreat(null, pos));
                            Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(pos)));
                        }
                    }
                    else
                    {
                        if (Helpers.CheckPath(player.GetPath(vector)) && !CheckForRetreat(null, vector))
                        {
                            _GameInfo.MoveTo = vector;
                            if (Debug)
                            {
                                Console.WriteLine("True - CheckForGrouping() - Checknig enemy turrets 2");
                                Console.WriteLine("\t" + CheckForRetreat(null, pos));
                                Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(pos)));
                            }
                            return true;
                        }
                        if (Debug)
                        {
                            Console.WriteLine("False - CheckForGrouping() - Checknig enemy turrets 2");
                            Console.WriteLine("\t" + CheckForRetreat(null, pos));
                            Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(pos)));
                        }
                    }
                }
            }
            //Checknig ally turrets
            foreach (var vector in
                _GameInfo.AllyStructures.Where(
                    s => s.Distance(player.Position) < menu.Item("GankRange").GetValue<Slider>().Value))
            {
                var eMinis = Helpers.getMobs(vector, GameInfo.ChampionRange);
                if (!CheckLaneClear(vector))
                {
                    continue;
                }
                if (eMinis.Count > 3)
                {
                    var temp = eMinis.OrderByDescending(m => Helpers.getMobs(m.Position, 300).Count).FirstOrDefault();
                    if (temp != null)
                    {
                        var pos = temp.Position;
                        if (Helpers.CheckPath(player.GetPath(pos)) && !CheckForRetreat(null, pos))
                        {
                            _GameInfo.MoveTo = pos;
                            if (Debug)
                            {
                                Console.WriteLine("True - CheckForGrouping() - Checknig ally turrets 1");
                                Console.WriteLine("\t" + CheckForRetreat(null, pos));
                                Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(pos)));
                            }
                            return true;
                        }
                        if (Debug)
                        {
                            Console.WriteLine("False - CheckForGrouping() - Checknig ally turrets 1");
                            Console.WriteLine("\t" + CheckForRetreat(null, pos));
                            Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(pos)));
                        }
                    }
                    else
                    {
                        if (Helpers.CheckPath(player.GetPath(vector)) && !CheckForRetreat(null, vector))
                        {
                            _GameInfo.MoveTo = vector;
                            if (Debug)
                            {
                                Console.WriteLine("True - CheckForGrouping() - Checknig ally turrets 2");
                                Console.WriteLine("\t" + CheckForRetreat(null, pos));
                                Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(pos)));
                            }
                            return true;
                        }
                        if (Debug)
                        {
                            Console.WriteLine("False - CheckForGrouping() - Checknig ally turrets 2");
                            Console.WriteLine("\t" + CheckForRetreat(null, pos));
                            Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(pos)));
                        }
                    }
                }
            }
            //follow minis
            var minis = Helpers.getAllyMobs(player.Position, 1000);
            if (minis.Count >= 5 && player.Level >= 8)
            {
                var objAiBase = minis.OrderBy(m => m.Distance(_GameInfo.SpawnPointEnemy)).FirstOrDefault();
                if (objAiBase != null &&
                    (objAiBase.CountAlliesInRange(GameInfo.ChampionRange) == 0 ||
                     objAiBase.CountAlliesInRange(GameInfo.ChampionRange) >= 2 || player.Level >= 10) &&
                    Helpers.getMobs(objAiBase.Position, 1000).Count == 0)
                {
                    _GameInfo.MoveTo = objAiBase.Position.Extend(_GameInfo.SpawnPoint, 100);
                    _GameInfo.GroupWithoutTarget = true;
                    if (Debug)
                    {
                        Console.WriteLine("True - CheckForGrouping() - follow minis");
                        Console.WriteLine("\t" + CheckForRetreat(null, objAiBase.Position));
                        Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(objAiBase.Position)));
                        Console.WriteLine("\t" + objAiBase.CountAlliesInRange(GameInfo.ChampionRange));
                        Console.WriteLine("\t" + Helpers.getMobs(objAiBase.Position, 1000).Count);
                    }
                    return true;
                }
                if (Debug)
                {
                    Console.WriteLine("False - CheckForGrouping() - follow minis");
                    Console.WriteLine("\t" + objAiBase != null);
                    if (objAiBase != null)
                    {
                        Console.WriteLine("\t" + CheckForRetreat(null, objAiBase.Position));
                        Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(objAiBase.Position)));
                        Console.WriteLine("\t" + objAiBase.CountAlliesInRange(GameInfo.ChampionRange));
                        Console.WriteLine("\t" + Helpers.getMobs(objAiBase.Position, 1000).Count);
                    }
                }
            }
            //Checking free enemy minionwaves
            if (player.Level > 8)
            {
                var miniwaves =
                    Helpers.getMobs(player.Position, menu.Item("GankRange").GetValue<Slider>().Value)
                        .Where(m => Helpers.getMobs(m.Position, 1200).Count > 6 && CheckLaneClear(m.Position))
                        .OrderByDescending(m => m.Distance(_GameInfo.SpawnPoint) < 7000)
                        .ThenByDescending(m => m.Distance(player) < 2000)
                        .ThenByDescending(m => Helpers.getMobs(m.Position, 1200).Count);
                foreach (var miniwave in
                    miniwaves.Where(miniwave => Helpers.getMobs(miniwave.Position, 1200).Count >= 6)
                        .Where(
                            miniwave =>
                                !CheckForRetreat(null, miniwave.Position) &&
                                Helpers.CheckPath(player.GetPath(miniwave.Position))))
                {
                    _GameInfo.MoveTo = miniwave.Position.Extend(player.Position, 200);
                    if (Debug)
                    {
                        Console.WriteLine("True - CheckForGrouping() - Checking free enemy minionwavess");
                        Console.WriteLine("\t" + CheckForRetreat(null, miniwave.Position));
                        Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(miniwave.Position)));
                        Console.WriteLine("\t" + miniwave.CountAlliesInRange(GameInfo.ChampionRange));
                        Console.WriteLine("\t" + Helpers.getMobs(miniwave.Position, 1000).Count);
                    }
                    return true;
                }
            }
            //Checking ally mobs, pushing
            if (player.Level > 8)
            {
                var miniwave =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            m =>
                                m.Distance(_GameInfo.SpawnPointEnemy) < 7000 &&
                                Helpers.getAllyMobs(m.Position, 1200).Count >= 7)
                        .OrderByDescending(m => m.Distance(_GameInfo.SpawnPointEnemy) < 7000)
                        .ThenBy(m => m.Distance(player))
                        .FirstOrDefault();
                if (miniwave != null && Helpers.CheckPath(player.GetPath(miniwave.Position)) &&
                    !CheckForRetreat(null, miniwave.Position) && CheckLaneClear(miniwave.Position))
                {
                    _GameInfo.MoveTo = miniwave.Position.Extend(player.Position, 200);
                    if (Debug)
                    {
                        Console.WriteLine("True - CheckForGrouping() - Checking ally mobs, pushing");
                        Console.WriteLine("\t" + CheckForRetreat(null, miniwave.Position));
                        Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(miniwave.Position)));
                        Console.WriteLine("\t" + miniwave.CountAlliesInRange(GameInfo.ChampionRange));
                        Console.WriteLine("\t" + Helpers.getMobs(miniwave.Position, 1000).Count);
                    }
                    return true;
                }
                if (Debug)
                {
                    Console.WriteLine("True - CheckForGrouping() - Checking ally mobs, pushing");
                    Console.WriteLine("\t" + miniwave != null);
                    if (miniwave != null)
                    {
                        Console.WriteLine("\t" + CheckForRetreat(null, miniwave.Position));
                        Console.WriteLine("\t" + Helpers.CheckPath(player.GetPath(miniwave.Position)));
                        Console.WriteLine("\t" + miniwave.CountAlliesInRange(GameInfo.ChampionRange));
                        Console.WriteLine("\t" + Helpers.getMobs(miniwave.Position, 1000).Count);
                    }
                }
            }
            if (Debug)
            {
                Console.WriteLine("------------------------------");
            }
            return false;
        }

        private static Vector3 GetMovePosition()
        {
            switch (_GameInfo.GameState)
            {
                case State.Retreat:
                    var enemyTurret =
                        ObjectManager.Get<Obj_AI_Turret>()
                            .FirstOrDefault(t => t.IsEnemy && !t.IsDead && t.Distance(player) < 2000);
                    var allyTurret =
                        ObjectManager.Get<Obj_AI_Turret>()
                            .OrderBy(t => t.Distance(player))
                            .FirstOrDefault(
                                t =>
                                    t.IsAlly && !t.IsDead && t.Distance(player) < 4000 &&
                                    t.CountEnemiesInRange(1200) == 0);
                    var enemy = _GameInfo.Target;
                    if (_GameInfo.AttackedByTurret && enemyTurret != null)
                    {
                        if (allyTurret != null)
                        {
                            return allyTurret.Position;
                        }
                        var nextPost = Prediction.GetPrediction(player, 1);
                        if (!nextPost.UnitPosition.UnderTurret(true))
                        {
                            return nextPost.CastPosition;
                        }
                        else
                        {
                            return _GameInfo.SpawnPoint;
                        }
                    }
                    if (allyTurret != null && player.Distance(_GameInfo.SpawnPoint) > player.Distance(allyTurret))
                    {
                        return allyTurret.Position.Extend(_GameInfo.SpawnPoint, 300);
                    }
                    return _GameInfo.SpawnPoint;
                    break;
                case State.Objective:
                    return _GameInfo.MoveTo;
                    break;
                case State.Grouping:
                    return _GameInfo.MoveTo;
                    break;
                case State.Defending:
                    return Vector3.Zero;
                    break;
                case State.Pushing:
                    return Vector3.Zero;
                    break;
                case State.Warding:
                    return _GameInfo.MoveTo;
                    break;
                case State.FightIng:
                    return Vector3.Zero;
                    break;
                case State.Ganking:
                    return _GameInfo.MoveTo;
                    break;
                case State.Jungling:
                    return Vector3.Zero;
                    break;
                case State.LaneClear:
                    return Vector3.Zero;
                    break;
                default:
                    MonsterInfo nextMob = GetNextMob();
                    if (nextMob != null)
                    {
                        return nextMob.Position;
                    }
                    var firstOrDefault = _GameInfo.MonsterList.FirstOrDefault(m => m.Index == 1);
                    if (firstOrDefault != null)
                    {
                        return firstOrDefault.Position;
                    }
                    break;
            }

            if (Debug)
            {
                Console.WriteLine("GetMovePosition: Can't get Position");
            }
            return Vector3.Zero;
        }

        private static MonsterInfo GetNextMob()
        {
            MonsterInfo nextMob = null;
            if (!menu.Item("EnemyJungle").GetValue<Boolean>())
            {
                if (player.Team == GameObjectTeam.Chaos)
                {
                    nextMob =
                        _GameInfo.MonsterList.OrderBy(m => m.Index)
                            .FirstOrDefault(m => m.Index == _GameInfo.CurrentMonster && !m.ID.Contains("bteam"));
                }
                else
                {
                    nextMob =
                        _GameInfo.MonsterList.OrderBy(m => m.Index)
                            .FirstOrDefault(m => m.Index == _GameInfo.CurrentMonster && !m.ID.Contains("pteam"));
                }
            }
            else
            {
                nextMob =
                    _GameInfo.MonsterList.OrderBy(m => m.Index).FirstOrDefault(m => m.Index == _GameInfo.CurrentMonster);
            }
            return nextMob;
        }

        private static void ResetDamageTakenTimer()
        {
            if (Environment.TickCount - ResetTimer > 1200)
            {
                ResetTimer = Environment.TickCount;
                _GameInfo.DamageTaken = 0f;
                _GameInfo.DamageCount = 0;
            }
            if (_GameInfo.CurrentMonster == 13 && player.Level <= 9)
            {
                _GameInfo.CurrentMonster++;
            }
            if (_GameInfo.CurrentMonster > 16)
            {
                _GameInfo.CurrentMonster = 1;
            }
        }

        private static bool ShouldRecall()
        {
            if (player.HealthPercent <= menu.Item("HealtToBack").GetValue<Slider>().Value)
            {
                if (Debug)
                {
                    Console.WriteLine("ShouldRecall: Low Health - true");
                }
                return true;
            }
           /* if (_GameInfo.CanBuyItem())
            {
                if (Debug)
                {
                    Console.WriteLine("ShouldRecall: Can buy item - true");
                }
                return true;
            }*/
            if (Helpers.getMobs(_GameInfo.SpawnPoint, 5000).Count > 6)
            {
                if (Debug)
                {
                    Console.WriteLine("ShouldRecall: Def base - true");
                }
                return true;
            }
            if (_GameInfo.GameState == State.Retreat && player.CountEnemiesInRange(GameInfo.ChampionRange) == 0)
            {
                if (Debug)
                {
                    Console.WriteLine("ShouldRecall: After retreat - true");
                }
                return true;
            }
            if (Debug)
            {
                Console.WriteLine("ShouldRecall: End - false");
            }
            return false;
        }

        private static bool WaitOnFountain()
        {
            if (!player.InFountain())
            {
                return false;
            }
            if (player.InFountain() && player.IsRecalling())
            {
                return false;
            }
            if (player.HealthPercent < 90 || (player.ManaPercent < 90 && player.MaxMana > 100))
            {
                if (player.IsMoving)
                {
                    player.IssueOrder(GameObjectOrder.HoldPosition, player.Position);
                }
                return true;
            }
            return false;
        }

        private static bool ShouldSkipUpdate()
        {
            if (!menu.Item("Enabled").GetValue<Boolean>())
            {
                return true;
            }
            if (Environment.TickCount - UpdateLimiter <= 400)
            {
                return true;
            }
            if (player.IsDead)
            {
                return true;
            }
            if (player.IsRecalling() && !player.InFountain())
            {
                return true;
            }
            UpdateLimiter = Environment.TickCount - Random.Next(0, 100);
            return false;
        }

        public static bool Debug
        {
            get { return menu.Item("debug").GetValue<KeyBind>().Active; }
        }

        #endregion

        #endregion

        #region Events

        private static void Game_ProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            Obj_AI_Hero target = args.Target as Obj_AI_Hero;
            if (target != null)
            {
                if (target.IsMe && sender.IsValid && !sender.IsDead && sender.IsEnemy && target.IsValid)
                {
                    if (Orbwalking.IsAutoAttack(args.SData.Name))
                    {
                        _GameInfo.DamageTaken += (float) sender.GetAutoAttackDamage(player, true);
                        _GameInfo.DamageCount++;
                    }
                    if (sender is Obj_AI_Turret && !_GameInfo.AttackedByTurret)
                    {
                        _GameInfo.AttackedByTurret = true;
                        Utility.DelayAction.Add(2000, () => _GameInfo.AttackedByTurret = false);
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Debug)
            {
                if (pos.IsValid())
                {
                    Render.Circle.DrawCircle(pos, 50, Color.Crimson, 7);
                }

                foreach (var m in Helpers.mod)
                {
                    Render.Circle.DrawCircle(m, 50, Color.Crimson, 7);
                }

                if (_GameInfo.LastClick.IsValid())
                {
                    Render.Circle.DrawCircle(_GameInfo.LastClick, 70, Color.Blue, 7);
                }
                if (_GameInfo.MoveTo.IsValid())
                {
                    Render.Circle.DrawCircle(_GameInfo.MoveTo, 77, Color.BlueViolet, 7);
                }
                foreach (var e in _GameInfo.EnemyStructures)
                {
                    Render.Circle.DrawCircle(e, 300, Color.Red, 7);
                }
                foreach (var a in _GameInfo.AllyStructures)
                {
                    Render.Circle.DrawCircle(a, 300, Color.DarkGreen, 7);
                }
                if (_GameInfo.ClosestWardPos.IsValid())
                {
                    Render.Circle.DrawCircle(_GameInfo.ClosestWardPos, 70, Color.LawnGreen, 7);
                }
            }
            if (menu.Item("State").GetValue<Boolean>())
            {
                Drawing.DrawText(150f, 200f, Color.Aqua, _GameInfo.GameState.ToString());
            }
        }

        private static void Obj_AI_Base_OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (sender.IsMe)
            {
                _GameInfo.LastClick = args.Path.Last();
            }
        }

        private static void OnValueChanged(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
            try
            {
                var index = languages.ToList().IndexOf(onValueChangeEventArgs.GetNewValue<StringList>().SelectedValue);
                File.WriteAllText(path + fileName, languagesShort[index], Encoding.Default);
                Console.WriteLine("Changed to " + languagesShort[index]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Init

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            player = ObjectManager.Player;
            _GameInfo = new GameInfo();
            orbwalker = new OrbwalkingForBots.Orbwalker();
            SetCulture();
            if (Game.MapId != GameMapId.SummonersRift)
            {
                Game.PrintChat(resourceM.GetString("MapNotSupported"));
                return;
            }
            _GameInfo.Champdata = new Champdata();
            if (_GameInfo.Champdata.Hero == null)
            {
                Game.PrintChat(resourceM.GetString("ChampNotSupported"));
                return;
            }
            Jungle.setSmiteSlot();
            if (Jungle.smiteSlot == SpellSlot.Unknown)
            {
                Game.PrintChat(resourceM.GetString("NoSmite"));
                return;
            }

          /*  Console.WriteLine("Items: ");
            foreach (var i in player.InventoryItems)
            {
                Console.WriteLine("\t Name: {0}, ID: {1}({2})", i.IData.TranslatedDisplayName, i.Id, (int) i.Id);
            }*/

            ItemHandler = new ItemHandler(_GameInfo.Champdata.Type);
            CreateMenu();
            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Game_ProcessSpell;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnNewPath += Obj_AI_Base_OnNewPath;
            Game.OnEnd += Game_OnEnd;
            Obj_AI_Base.OnDelete += Obj_AI_Base_OnDelete;
        }

        private static void SetCulture()
        {
            try
            {
                path = string.Format(@"{0}\AutoJ\", Config.AppDataDirectory);
                fileName = "Lang.txt";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (!File.Exists(path + fileName))
                {
                    File.AppendAllText(path + fileName, "en", Encoding.Default);
                    resourceM = new ResourceManager("AutoJungle.Resource.en", typeof(Program).Assembly);
                    Console.WriteLine("First start, lang is English");
                }
                else
                {
                    culture = File.ReadLines(path + fileName).First();
                    Console.WriteLine(culture);
                    resourceM = new ResourceManager("AutoJungle.Resource." + culture, typeof(Program).Assembly);
                    Console.WriteLine("Lang set to " + culture);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                resourceM = new ResourceManager("AutoJungle.Resource.en", typeof(Program).Assembly);
            }
        }

        private static void Obj_AI_Base_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Position.Distance(player.Position) < 600)
            {
                var closest = _GameInfo.MonsterList.FirstOrDefault(m => m.Position.Distance(sender.Position) < 600);
                if (closest != null && _GameInfo.GameState == State.Jungling &&
                    Helpers.getMobs(sender.Position, 600).Where(m => !m.IsDead).Count() == 0)
                {
                    if (Environment.TickCount - closest.TimeAtDead > closest.RespawnTime)
                    {
                        closest.TimeAtDead = Environment.TickCount;
                    }
                }
            }
        }

        private static void Game_OnEnd(GameEndEventArgs args)
        {
            if (menu.Item("AutoClose").GetValue<Boolean>())
            {
                Console.WriteLine("END");
                var delay = Random.Next(25000, 35000);
                Task.Run(
                    async () =>
                    {
                        await Task.Delay(delay);
                        Game.Quit();
                    });
            }
        }

        private static void CreateMenu()
        {
            menu = new Menu(resourceM.GetString("AutoJungle"), "AutoJungle", true);

            Menu menuD = new Menu(resourceM.GetString("dsettings"), "dsettings");
            menuD.AddItem(new MenuItem("debug", resourceM.GetString("debug")))
                .SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle))
                .SetFontStyle(FontStyle.Bold, SharpDX.Color.Orange);
            menuD.AddItem(new MenuItem("State", resourceM.GetString("State"))).SetValue(false);
            menu.AddSubMenu(menuD);
            Menu menuJ = new Menu(resourceM.GetString("jsettings"), "jsettings");
            menuJ.AddItem(
                new MenuItem("HealtToBack", resourceM.GetString("HealtToBack")).SetValue(new Slider(35, 0, 100)));
            menuJ.AddItem(
                new MenuItem("HealthToPotion", resourceM.GetString("HealthToPotion")).SetValue(new Slider(80, 0, 100)));
            menuJ.AddItem(new MenuItem("EnemyJungle", resourceM.GetString("EnemyJungle"))).SetValue(true);
            menuJ.AddItem(new MenuItem("UseTrinket", resourceM.GetString("UseTrinket"))).SetValue(true);
            menuJ.AddItem(new MenuItem("WaitAtLvlTWO", resourceM.GetString("WaitAtLvlTWO"), true)).SetValue(false);
            Menu menuJspells = new Menu(resourceM.GetString("sssettings"), "jssettings");
            menuJspells.AddItem(
                new MenuItem("UseBarrierJ", resourceM.GetString("UseBarrier")).SetValue(new Slider(0, -1, 100)));
            menuJspells.AddItem(
                new MenuItem("UseHealJ", resourceM.GetString("UseHeal")).SetValue(new Slider(0, -1, 100)));
            menuJ.AddSubMenu(menuJspells);
            menu.AddSubMenu(menuJ);
            Menu menuG = new Menu(resourceM.GetString("gsettings"), "gsettings");
            menuG.AddItem(new MenuItem("GankLevel", resourceM.GetString("GankLevel")).SetValue(new Slider(5, 1, 18)));
            menuG.AddItem(
                new MenuItem("GankFrequency", resourceM.GetString("GankFrequency")).SetValue(new Slider(100, 0, 100)));
            menuG.AddItem(
                new MenuItem("GankRange", resourceM.GetString("GankRange")).SetValue(new Slider(7000, 0, 20000)));
            menuG.AddItem(new MenuItem("ComboSmite", resourceM.GetString("ComboSmite"))).SetValue(true);

            Menu menuGspells = new Menu(resourceM.GetString("sssettings"), "gssettings");
            menuGspells.AddItem(
                new MenuItem("UseBarrierG", resourceM.GetString("UseBarrier")).SetValue(new Slider(0, -1, 100)));
            menuGspells.AddItem(
                new MenuItem("UseHealG", resourceM.GetString("UseHeal")).SetValue(new Slider(0, -1, 100)));
            menuGspells.AddItem(new MenuItem("UseIgniteG", resourceM.GetString("UseIgnite"))).SetValue(true);
            menuGspells.AddItem(new MenuItem("UseIgniteOpt", resourceM.GetString("UseIgniteOpt"))).SetValue(true);
            menuG.AddSubMenu(menuGspells);

            menu.AddSubMenu(menuG);
            menu.AddItem(new MenuItem("Enabled", resourceM.GetString("Enabled"))).SetValue(true);
            menu.AddItem(new MenuItem("AutoClose", resourceM.GetString("AutoClose"))).SetValue(true);
            Menu menuChamps = new Menu(resourceM.GetString("supported"), "supported");
            menuChamps.AddItem(new MenuItem("supportedYi", resourceM.GetString("supportedYi")));
            menuChamps.AddItem(new MenuItem("supportedWarwick", resourceM.GetString("supportedWarwick")));
            menuChamps.AddItem(new MenuItem("supportedShyvana", resourceM.GetString("supportedShyvana")));
            menuChamps.AddItem(new MenuItem("supportedJax", resourceM.GetString("supportedJax")));
            menuChamps.AddItem(new MenuItem("supportedXinZhao", resourceM.GetString("supportedXinZhao")));
            menuChamps.AddItem(new MenuItem("supportedNocturne", resourceM.GetString("supportedNocturne")));
            menuChamps.AddItem(new MenuItem("supportedEvelyn", resourceM.GetString("supportedEvelyn")));
            menuChamps.AddItem(new MenuItem("supportedVolibear", resourceM.GetString("supportedVolibear")));
            menuChamps.AddItem(new MenuItem("supportedTryndamere", resourceM.GetString("supportedTryndamere")));
            menuChamps.AddItem(new MenuItem("supportedOlaf", resourceM.GetString("supportedOlaf")));
            menuChamps.AddItem(new MenuItem("supportedNunu", resourceM.GetString("supportedNunu")));
            menuChamps.AddItem(new MenuItem("supportedUdyr", resourceM.GetString("supportedUdyr")));
            menuChamps.AddItem(new MenuItem("supportedKogMaw", resourceM.GetString("supportedKogMaw")));
            menuChamps.AddItem(new MenuItem("supportedKayle", resourceM.GetString("supportedKayle")));

            //menuChamps.AddItem(new MenuItem("supportedSkarner", "Skarner"));
            menu.AddSubMenu(menuChamps);

            Menu menuLang = new Menu(resourceM.GetString("lsetting"), "lsetting");
            menuLang.AddItem(
                new MenuItem("Language", resourceM.GetString("Language")).SetValue(new StringList(languages, 0)));
            menuLang.AddItem(
                new MenuItem("AutoJungleInfoReload", resourceM.GetString("AutoJungleInfoReload")).SetFontStyle(
                    FontStyle.Bold, SharpDX.Color.Pink));
            menu.AddSubMenu(menuLang);
            menu.AddItem(
                new MenuItem(
                    "AutoJungleInfo2",
                    resourceM.GetString("AutoJungleInfo2") +
                    Assembly.GetExecutingAssembly().GetName().Version.ToString().Replace(",", ".")).SetFontStyle(
                        FontStyle.Bold, SharpDX.Color.Red));
            /*menu.AddItem(
                new MenuItem("AutoJungleInfo3", resourceM.GetString("AutoJungleInfo3")).SetFontStyle(
                    FontStyle.Bold, SharpDX.Color.Purple));*/

            menu.AddToMainMenu();
            menu.Item("Language").ValueChanged += OnValueChanged;
        }

        #endregion
    }
}
