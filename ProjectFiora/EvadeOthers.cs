using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using LeagueSharp.Common.Data;
using ItemData = LeagueSharp.Common.Data.ItemData;
using FioraProject.Evade;

namespace FioraProject
{
    using static Program;
    using static GetTargets;
    #region OtherSkill
    internal class OtherSkill
    {
        private static readonly List<SpellData> Spells = new List<SpellData>();

        // riven variables
        private static int RivenDashTick;
        private static int RivenQ3Tick;
        private static Vector2 RivenDashEnd = new Vector2();
        private static float RivenQ3Rad = 150;

        // fizz variables
        private static Vector2 FizzFishEndPos = new Vector2();
        private static GameObject FizzFishChum = null;
        private static int FizzFishChumStartTick;
        internal static void Init()
        {
            LoadSpellData();
            Spells.RemoveAll(i => !HeroManager.Enemies.Any(
                            a =>
                            string.Equals(
                                a.ChampionName,
                                i.ChampionName,
                                StringComparison.InvariantCultureIgnoreCase)));
            var evadeMenu = new Menu("Block Other skils", "EvadeOthers");
            {
                evadeMenu.Bool("W", "Use W");
                foreach (var hero in
                    HeroManager.Enemies.Where(
                        i =>
                        Spells.Any(
                            a =>
                            string.Equals(
                                a.ChampionName,
                                i.ChampionName,
                                StringComparison.InvariantCultureIgnoreCase))))
                {
                    evadeMenu.AddSubMenu(new Menu("-> " + hero.ChampionName, hero.ChampionName.ToLowerInvariant()));
                }
                foreach (var spell in
                    Spells.Where(
                        i =>
                        HeroManager.Enemies.Any(
                            a =>
                            string.Equals(
                                a.ChampionName,
                                i.ChampionName,
                                StringComparison.InvariantCultureIgnoreCase))))
                {
                    ((Menu)evadeMenu.SubMenu(spell.ChampionName.ToLowerInvariant())).Bool(
                        spell.ChampionName + spell.Slot,
                        spell.ChampionName + " (" + (spell.Slot == SpellSlot.Unknown ? "Passive" : spell.Slot.ToString()) + ")",
                        false);
                }
            }
            Menu.AddSubMenu(evadeMenu);
            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnPlayAnimation;
            CustomEvents.Unit.OnDash += Unit_OnDash;
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            var missile = sender as MissileClient;
            if (missile == null || !missile.IsValid)
                return;
            var caster = missile.SpellCaster as Obj_AI_Hero;
            if (!(caster is Obj_AI_Hero) || caster.Team == Player.Team)
                return;
            if (missile.SData.Name == "FizzMarinerDoomMissile")
            {
                FizzFishEndPos = missile.Position.To2D();
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Fizz_UltimateMissile_Orbit.troy" && FizzFishEndPos.IsValid()
                && sender.Position.To2D().Distance(FizzFishEndPos) <= 300)
            {
                FizzFishChum = sender;
                if (Utils.GameTimeTickCount >= FizzFishChumStartTick + 5000)
                    FizzFishChumStartTick = Utils.GameTimeTickCount;
            }
        }

        private static void Unit_OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            var caster = sender as Obj_AI_Hero;
            if (caster == null || !caster.IsValid || caster.Team == Player.Team)
            {
                return;
            }
            // riven dash
            if (caster.ChampionName == "Riven"
                && Menu.SubMenu("EvadeOthers").SubMenu(("Riven").ToLowerInvariant())
                .Item("Riven" + SpellSlot.Q).GetValue<bool>())
            {
                RivenDashTick = Utils.GameTimeTickCount;
                RivenDashEnd = args.EndPos;
            }
        }

        private static void Obj_AI_Hero_OnPlayAnimation(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var caster = sender as Obj_AI_Hero;
            if (caster == null || !caster.IsValid || caster.Team == Player.Team)
            {
                return;
            }
            // riven Q3
            if (caster.ChampionName == "Riven"
                && Menu.SubMenu("EvadeOthers").SubMenu(("Riven").ToLowerInvariant())
                .Item("Riven" + SpellSlot.Q).GetValue<bool>()
                )
            {
                RivenQ3Tick = Utils.GameTimeTickCount;
                if (caster.HasBuff("RivenFengShuiEngine"))
                    RivenQ3Rad = 150;
                else
                    RivenQ3Rad = 225;
            }
            // others
            var spellDatas =
               Spells.Where(
                   i =>
                   caster.ChampionName.ToLowerInvariant() == i.ChampionName.ToLowerInvariant()
                   && Menu.SubMenu("EvadeOthers").SubMenu(i.ChampionName.ToLowerInvariant())
                   .Item(i.ChampionName + i.Slot).GetValue<bool>());
            if (!spellDatas.Any())
            {
                return;
            }
            foreach (var spellData in spellDatas)
            {
                //reksaj W
                if (!Player.HasBuff("reksaiknockupimmune") && spellData.ChampionName == "Reksai"
                    && spellData.Slot == SpellSlot.W)// chua test
                {
                    if (Player.Position.To2D().Distance(caster.Position.To2D())
                        <= Player.BoundingRadius + caster.BoundingRadius + caster.AttackRange)
                        SolveInstantBlock();
                    return;
                }
            }
        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var caster = sender as Obj_AI_Hero;
            if (caster == null || !caster.IsValid || caster.Team == Player.Team)
            {
                return;
            }
            var spellDatas =
               Spells.Where(
                   i =>
                   caster.ChampionName.ToLowerInvariant() == i.ChampionName.ToLowerInvariant()
                   && Menu.SubMenu("EvadeOthers").SubMenu(i.ChampionName.ToLowerInvariant())
                   .Item(i.ChampionName + i.Slot).GetValue<bool>());
            if (!spellDatas.Any())
            {
                return;
            }
            foreach (var spellData in spellDatas)
            {
                // auto attack
                if (args.SData.IsAutoAttack() && args.Target != null && args.Target.IsMe)
                {
                    if (spellData.ChampionName == "Jax" && spellData.Slot == SpellSlot.W && caster.HasBuff("JaxEmpowerTwo"))
                    {
                        SolveInstantBlock();
                        return;
                    }
                    if (spellData.ChampionName == "Yorick" && spellData.Slot == SpellSlot.Q && caster.HasBuff("YorickSpectral"))
                    {
                        SolveInstantBlock();
                        return;
                    }
                    if (spellData.ChampionName == "Poppy" && spellData.Slot == SpellSlot.Q && caster.HasBuff("PoppyDevastatingBlow"))
                    {
                        SolveInstantBlock();
                        return;
                    }
                    if (spellData.ChampionName == "Rengar" && spellData.Slot == SpellSlot.Q
                        && (caster.HasBuff("rengarqbase") || caster.HasBuff("rengarqemp")))
                    {
                        SolveInstantBlock();
                        return;
                    }
                    if (spellData.ChampionName == "Nautilus" && spellData.Slot == SpellSlot.Unknown
                        && (!Player.HasBuff("nautiluspassivecheck")))
                    {
                        SolveInstantBlock();
                        return;
                    }
                    if (spellData.ChampionName == "Udyr" && spellData.Slot == SpellSlot.E && caster.HasBuff("UdyrBearStance")
                        && (Player.HasBuff("udyrbearstuncheck")))
                    {
                        SolveInstantBlock();
                        return;
                    }
                    return;
                }
                // aoe
                if (spellData.ChampionName == "Riven" && spellData.Slot == SpellSlot.W && args.Slot == SpellSlot.W)// chua test
                {
                    if (Player.Position.To2D().Distance(caster.Position.To2D())
                        <= Player.BoundingRadius + caster.BoundingRadius + caster.AttackRange)
                        SolveInstantBlock();
                    return;
                }
                if (spellData.ChampionName == "Diana" && spellData.Slot == SpellSlot.E && args.Slot == SpellSlot.E)// chua test
                {
                    if (Player.Position.To2D().Distance(caster.Position.To2D())
                        <= Player.BoundingRadius + 450)
                        SolveInstantBlock();
                    return;
                }
                if (spellData.ChampionName == "Maokai" && spellData.Slot == SpellSlot.R && args.SData.Name == "maokaidrain3toggle")
                {
                    if (Player.Position.To2D().Distance(caster.Position.To2D())
                        <= Player.BoundingRadius + 575)
                        SolveInstantBlock();
                    return;
                }
                if (spellData.ChampionName == "Kalista" && spellData.Slot == SpellSlot.E && args.Slot == SpellSlot.E)
                {
                    if (Player.Position.To2D().Distance(caster.Position.To2D())
                        <= 950
                        && Player.HasBuff("kalistaexpungemarker"))
                        SolveInstantBlock();
                    return;
                }
                if (spellData.ChampionName == "Kennen" && spellData.Slot == SpellSlot.W && args.Slot == SpellSlot.W)// chua test
                {
                    if (Player.Position.To2D().Distance(caster.Position.To2D())
                        <= 800
                        && Player.HasBuff("kennenmarkofstorm") && Player.GetBuffCount("kennenmarkofstorm") == 2)
                        SolveInstantBlock();
                    return;
                }
                if (spellData.ChampionName == "Azir" && spellData.Slot == SpellSlot.R && args.Slot == SpellSlot.R)// chua test
                {
                    Vector2 start = caster.Position.To2D().Extend(args.End.To2D(), -300);
                    Vector2 end = start.Extend(caster.Position.To2D(), 750);
                    Render.Circle.DrawCircle(start.To3D(), 50, Color.Red);
                    Render.Circle.DrawCircle(end.To3D(), 50, Color.Red);
                    float width = caster.Level >= 16 ? 125 * 6 / 2 :
                                caster.Level >= 11 ? 125 * 5 / 2 :
                                125 * 4 / 2;
                    FioraProject.Evade.Geometry.Rectangle Rect = new FioraProject.Evade.Geometry.Rectangle(start, end, width);
                    var Poly = Rect.ToPolygon();
                    if (!Poly.IsOutside(Player.Position.To2D()))
                    {
                        SolveInstantBlock();
                    }
                    return;
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.HasBuff("vladimirhemoplaguedebuff") && HeroManager.Enemies.Any(x => x.ChampionName == "Vladimir")
                && Menu.SubMenu("EvadeOthers").SubMenu(("Vladimir").ToLowerInvariant())
                .Item("Vladimir" + SpellSlot.R).GetValue<bool>())
            {
                var buff = Player.GetBuff("vladimirhemoplaguedebuff");
                if (buff == null)
                    return;
                SolveBuffBlock(buff);
            }

            if (Player.HasBuff("zedrdeathmark") && HeroManager.Enemies.Any(x => x.ChampionName == "Zed")
                && Menu.SubMenu("EvadeOthers").SubMenu(("Zed").ToLowerInvariant())
                .Item("Zed" + SpellSlot.R).GetValue<bool>())
            {
                var buff = Player.GetBuff("zedrdeathmark");
                if (buff == null)
                    return;
                SolveBuffBlock(buff);
            }

            if (Player.HasBuff("tristanaechargesound") && HeroManager.Enemies.Any(x => x.ChampionName == "Tristana")
                && Menu.SubMenu("EvadeOthers").SubMenu(("Tristana").ToLowerInvariant())
                .Item("Tristana" + SpellSlot.E).GetValue<bool>())
            {
                var buff = Player.GetBuff("tristanaechargesound ");
                if (buff == null)
                    return;
                SolveBuffBlock(buff);
            }

            if (Player.HasBuff("SoulShackles") && HeroManager.Enemies.Any(x => x.ChampionName == "Morgana")
                && Menu.SubMenu("EvadeOthers").SubMenu(("Morgana").ToLowerInvariant())
                .Item("Morgana" + SpellSlot.R).GetValue<bool>())
            {
                var buff = Player.GetBuff("SoulShackles");
                if (buff == null)
                    return;
                SolveBuffBlock(buff);
            }

            if (Player.HasBuff("NocturneUnspeakableHorror") && HeroManager.Enemies.Any(x => x.ChampionName == "Nocturne")
                && Menu.SubMenu("EvadeOthers").SubMenu(("Nocturne").ToLowerInvariant())
                .Item("Nocturne" + SpellSlot.E).GetValue<bool>())
            {
                var buff = Player.GetBuff("NocturneUnspeakableHorror");
                if (buff == null)
                    return;
                SolveBuffBlock(buff);
            }

            if (Player.HasBuff("karthusfallenonetarget") && HeroManager.Enemies.Any(x => x.ChampionName == "Karthus")
                && Menu.SubMenu("EvadeOthers").SubMenu(("Karthus").ToLowerInvariant())
                .Item("Karthus" + SpellSlot.R).GetValue<bool>())
            {
                var buff = Player.GetBuff("karthusfallenonetarget");
                if (buff == null)
                    return;
                SolveBuffBlock(buff);
            }

            if (Player.HasBuff("KarmaSpiritBind") && HeroManager.Enemies.Any(x => x.ChampionName == "Karma")
                && Menu.SubMenu("EvadeOthers").SubMenu(("Karma").ToLowerInvariant())
                .Item("Karma" + SpellSlot.R).GetValue<bool>())
            {
                var buff = Player.GetBuff("KarmaSpiritBind");
                if (buff == null)
                    return;
                SolveBuffBlock(buff);
            }

            if ((Player.HasBuff("LeblancSoulShackle") || (Player.HasBuff("LeblancShoulShackleM")))
                && HeroManager.Enemies.Any(x => x.ChampionName == "Karma")
                && Menu.SubMenu("EvadeOthers").SubMenu(("Karma").ToLowerInvariant())
                .Item("Karma" + SpellSlot.R).GetValue<bool>())
            {
                var buff = Player.GetBuff("LeblancSoulShackle");
                if (buff != null)
                {
                    SolveBuffBlock(buff);
                    return;
                }
                var buff2 = Player.GetBuff("LeblancShoulShackleM");
                if (buff2 != null)
                {
                    SolveBuffBlock(buff2);
                    return;
                }
            }

            // jax E
            var jax = HeroManager.Enemies.FirstOrDefault(x => x.ChampionName == "Jax" && x.IsValidTarget());

            if (jax != null && jax.HasBuff("JaxCounterStrike")
                && Menu.SubMenu("EvadeOthers").SubMenu(("Jax").ToLowerInvariant())
                .Item("Jax" + SpellSlot.E).GetValue<bool>())
            {
                var buff = jax.GetBuff("JaxCounterStrike");
                if (buff != null)
                {
                    if ((buff.EndTime - Game.Time) * 1000 <= 650 + Game.Ping && Player.Position.To2D().Distance(jax.Position.To2D())
                        <= Player.BoundingRadius + jax.BoundingRadius + jax.AttackRange + 100)
                    {
                        SolveInstantBlock();
                        return;
                    }
                }
            }

            //maokai R
            var maokai = HeroManager.Enemies.FirstOrDefault(x => x.ChampionName == "Maokai" && x.IsValidTarget());
            if (maokai != null && maokai.HasBuff("MaokaiDrain3")
                && Menu.SubMenu("EvadeOthers").SubMenu(("Maokai").ToLowerInvariant())
                .Item("Maokai" + SpellSlot.R).GetValue<bool>())
            {
                var buff = maokai.GetBuff("MaokaiDrain3");
                if (buff != null)
                {
                    if (Player.Position.To2D().Distance(maokai.Position.To2D())
                        <= Player.BoundingRadius + 475)
                        SolveBuffBlock(buff);
                }
            }

            // nautilus R
            if (Player.HasBuff("nautilusgrandlinetarget") && HeroManager.Enemies.Any(x => x.ChampionName == "Nautilus")
                && Menu.SubMenu("EvadeOthers").SubMenu(("Nautilus").ToLowerInvariant())
                .Item("Nautilus" + SpellSlot.R).GetValue<bool>())
            {
                var buff = Player.GetBuff("nautilusgrandlinetarget");
                if (buff == null)
                    return;
                var obj = ObjectManager.Get<GameObject>().Where(x => x.Name == "GrandLineSeeker").FirstOrDefault();
                if (obj == null)
                    return;
                if (obj.Position.To2D().Distance(Player.Position.To2D()) <= 300 + 700 * Game.Ping / 1000)
                {
                    SolveInstantBlock();
                    return;
                }
            }

            //rammus Q
            var ramus = HeroManager.Enemies.FirstOrDefault(x => x.ChampionName == "Rammus" && x.IsValidTarget());
            if (ramus != null
                && Menu.SubMenu("EvadeOthers").SubMenu(("Rammus").ToLowerInvariant())
                .Item("Rammus" + SpellSlot.Q).GetValue<bool>())
            {
                var buff = ramus.GetBuff("PowerBall");
                if (buff != null)
                {
                    var waypoints = ramus.GetWaypoints();
                    if (waypoints.Count == 1)
                    {
                        if (Player.Position.To2D().Distance(ramus.Position.To2D())
                            <= Player.BoundingRadius + ramus.AttackRange + ramus.BoundingRadius)
                        {
                            SolveInstantBlock();
                            return;
                        }
                    }
                    else
                    {
                        if (Player.Position.To2D().Distance(ramus.Position.To2D())
                            <= Player.BoundingRadius + ramus.AttackRange + ramus.BoundingRadius
                            + ramus.MoveSpeed * (0.5f + Game.Ping / 1000))
                        {
                            if (waypoints.Any(x => x.Distance(Player.Position.To2D())
                                <= Player.BoundingRadius + ramus.AttackRange + ramus.BoundingRadius + 70))
                            {
                                SolveInstantBlock();
                                return;
                            }
                            for (int i = 0; i < waypoints.Count() - 2; i++)
                            {
                                if (Player.Position.To2D().Distance(waypoints[i], waypoints[i + 1], true)
                                    <= Player.BoundingRadius + ramus.BoundingRadius + ramus.AttackRange + 70)
                                {
                                    SolveInstantBlock();
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            //fizzR
            if (HeroManager.Enemies.Any(x => x.ChampionName == "Fizz")
                && Menu.SubMenu("EvadeOthers").SubMenu(("Fizz").ToLowerInvariant())
                .Item("Fizz" + SpellSlot.R).GetValue<bool>())
            {
                if (FizzFishChum != null && FizzFishChum.IsValid
                    && Utils.GameTimeTickCount - FizzFishChumStartTick >= 1500 - 250 - Game.Ping
                    && Player.Position.To2D().Distance(FizzFishChum.Position.To2D()) <= 250 + Player.BoundingRadius)
                {
                    SolveInstantBlock();
                    return;
                }
            }

            //nocturne R
            var nocturne = HeroManager.Enemies.FirstOrDefault(x => x.ChampionName == "Nocturne" && x.IsValidTarget());
            if (nocturne != null
                && Menu.SubMenu("EvadeOthers").SubMenu(("Nocturne").ToLowerInvariant())
                .Item("Nocturne" + SpellSlot.R).GetValue<bool>())
            {
                var buff = Player.GetBuff("nocturneparanoiadash");
                if (buff != null && Player.Position.To2D().Distance(nocturne.Position.To2D()) <= 300 + 1200 * Game.Ping / 1000)
                {
                    SolveInstantBlock();
                    return;
                }
            }


            // rivenQ3
            var riven = HeroManager.Enemies.FirstOrDefault(x => x.ChampionName == "Riven" && x.IsValidTarget());
            if (riven != null && Menu.SubMenu("EvadeOthers").SubMenu(("Riven").ToLowerInvariant())
                .Item("Riven" + SpellSlot.Q).GetValue<bool>() && RivenDashEnd.IsValid())
            {
                if (Utils.GameTimeTickCount - RivenDashTick <= 100 && Utils.GameTimeTickCount - RivenQ3Tick <= 100
                    && Math.Abs(RivenDashTick - RivenQ3Tick) <= 100 && Player.Position.To2D().Distance(RivenDashEnd) <= RivenQ3Rad)
                {
                    SolveInstantBlock();
                    return;
                }
            }

        }
        private static void SolveBuffBlock(BuffInstance buff)
        {
            if (Player.IsDead || Player.HasBuffOfType(BuffType.SpellShield) || Player.HasBuffOfType(BuffType.SpellImmunity)
                || !Menu.SubMenu("EvadeOthers").Item("W").GetValue<bool>() || !W.IsReady())
                return;
            if (buff == null)
                return;
            if ((buff.EndTime - Game.Time) * 1000 <= 250 + Game.Ping)
            {
                var tar = GetTarget(W.Range);
                if (tar.IsValidTarget(W.Range))
                    Player.Spellbook.CastSpell(SpellSlot.W, tar.Position);
                else
                {
                    var hero = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(W.Range));
                    if (hero != null)
                        Player.Spellbook.CastSpell(SpellSlot.W, hero.Position);
                    else
                        Player.Spellbook.CastSpell(SpellSlot.W, Player.ServerPosition.Extend(Game.CursorPos, 100));
                }
            }
        }
        private static void SolveInstantBlock()
        {
            if (Player.IsDead || Player.HasBuffOfType(BuffType.SpellShield) || Player.HasBuffOfType(BuffType.SpellImmunity)
                || !Menu.SubMenu("EvadeOthers").Item("W").GetValue<bool>() || !W.IsReady())
                return;
            var tar = GetTarget(W.Range);
            if (tar.IsValidTarget(W.Range))
                Player.Spellbook.CastSpell(SpellSlot.W, tar.Position);
            else
            {
                var hero = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(W.Range));
                if (hero != null)
                    Player.Spellbook.CastSpell(SpellSlot.W, hero.Position);
                else
                    Player.Spellbook.CastSpell(SpellSlot.W, Player.ServerPosition.Extend(Game.CursorPos, 100));
            }
        }
        private static void LoadSpellData()
        {
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Azir",
                    Slot = SpellSlot.R,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Fizz",
                    Slot = SpellSlot.R,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Jax",
                    Slot = SpellSlot.W,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Jax",
                    Slot = SpellSlot.E,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Riven",
                    Slot = SpellSlot.Q,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Riven",
                    Slot = SpellSlot.W,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Diana",
                    Slot = SpellSlot.E,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Kalista",
                    Slot = SpellSlot.E,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Karma",
                    Slot = SpellSlot.W,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Karthus",
                    Slot = SpellSlot.R,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Kennen",
                    Slot = SpellSlot.W,
                });
            //Spells.Add(
            //    new SpellData
            //    {
            //        ChampionName = "Leesin",
            //        Slot = SpellSlot.Q,
            //    });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Leblanc",
                    Slot = SpellSlot.E,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Maokai",
                    Slot = SpellSlot.R,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Morgana",
                    Slot = SpellSlot.R,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Nautilus",
                    Slot = SpellSlot.Unknown,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Nautilus",
                    Slot = SpellSlot.R,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Nocturne",
                    Slot = SpellSlot.E,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Nocturne",
                    Slot = SpellSlot.R,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Nocturne",
                    Slot = SpellSlot.R,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Rammus",
                    Slot = SpellSlot.Q,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Rengar",
                    Slot = SpellSlot.Q,
                });
            Spells.Add(
            new SpellData
            {
                ChampionName = "Reksai",
                Slot = SpellSlot.W,
            });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Vladimir",
                    Slot = SpellSlot.R,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Zed",
                    Slot = SpellSlot.R,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Tristana",
                    Slot = SpellSlot.E,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Udyr",
                    Slot = SpellSlot.E,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Yorick",
                    Slot = SpellSlot.Q,
                });
        }
        private class SpellData
        {
            #region Fields

            public string ChampionName;

            public SpellSlot Slot;

            #endregion

            #region Public Properties


            #endregion
        }
    }
    #endregion OtherSkill
}
