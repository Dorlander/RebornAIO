using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Evade
{
    public static class AddUI
    {
        public static void Notif(string msg, int time)
        {
            var x = new Notification("ProjectFiora:  " + msg, time);
            Notifications.AddNotification(x);
        }

        public static LeagueSharp.Common.MenuItem Separator(this LeagueSharp.Common.Menu subMenu, string name)
        {
            return subMenu.AddItem(new LeagueSharp.Common.MenuItem(name, name));
        }

        public static LeagueSharp.Common.MenuItem Bool(this LeagueSharp.Common.Menu subMenu, string name, string display, bool state = true)
        {
            return subMenu.AddItem(new LeagueSharp.Common.MenuItem(name, display).SetValue(state));
        }

        public static LeagueSharp.Common.MenuItem KeyBind(this LeagueSharp.Common.Menu subMenu,
            string name,
            string display,
            System.Windows.Forms.Keys key,
            KeyBindType type = KeyBindType.Press)
        {
            return subMenu.AddItem(new LeagueSharp.Common.MenuItem(name, display).SetValue<KeyBind>(new KeyBind((uint)key, type)));
        }

        public static LeagueSharp.Common.MenuItem List(this LeagueSharp.Common.Menu subMenu, string name, string display, string[] array)
        {
            return subMenu.AddItem(new LeagueSharp.Common.MenuItem(name, display).SetValue<StringList>(new StringList(array)));
        }

        public static LeagueSharp.Common.MenuItem Slider(this LeagueSharp.Common.Menu subMenu, string name, string display, int cur, int min = 0, int max = 100)
        {
            return subMenu.AddItem(new LeagueSharp.Common.MenuItem(name, display).SetValue<Slider>(new Slider(cur, min, max)));
        }
    }
    public static class TargetedSkillShots
    {
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Menu Menu { get { return Config.Menu; } }
        public class EvadeTarget
        {
            #region Static Fields

            private static readonly List<Targets> DetectedTargets = new List<Targets>();

            private static readonly List<SpellData> Spells = new List<SpellData>();

            #endregion

            #region Methods

            public static void Init()
            {
                LoadSpellData();

                Spells.RemoveAll(i => !HeroManager.Enemies.Any(
                a =>
                string.Equals(
                    a.ChampionName,
                    i.ChampionName,
                    StringComparison.InvariantCultureIgnoreCase)));

                var evadeMenu = new Menu("Evade Targeted SkillShot", "EvadeTarget");
                {
                    var aaMenu = new Menu("Auto Attack", "AA");
                    {
                        aaMenu.Bool("B", "Basic Attack", false);
                        aaMenu.Slider("BHpU", "-> If Hp < (%)", 35);
                        aaMenu.Bool("C", "Crit Attack", false);
                        aaMenu.Slider("CHpU", "-> If Hp < (%)", 40);
                        evadeMenu.AddSubMenu(aaMenu);
                    }
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
                            spell.MissileName,
                            spell.MissileName + " (" + spell.Slot + ")",
                            false);
                    }
                }
                Menu.AddSubMenu(evadeMenu);
                Game.OnUpdate += OnUpdateTarget;
                GameObject.OnCreate += ObjSpellMissileOnCreate;
                GameObject.OnDelete += ObjSpellMissileOnDelete;
            }

            private static void LoadSpellData()
            {
                Spells.Add(
                       new SpellData { ChampionName = "Akali", SpellNames = new[] { "akalimota" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData { ChampionName = "Anivia", SpellNames = new[] { "frostbite" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Annie", SpellNames = new[] { "disintegrate" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Brand",
                        SpellNames = new[] { "brandwildfire", "brandwildfiremissile" },
                        Slot = SpellSlot.R
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Caitlyn",
                        SpellNames = new[] { "caitlynaceintheholemissile" },
                        Slot = SpellSlot.R
                    });
                Spells.Add(
                    new SpellData
                    { ChampionName = "Cassiopeia", SpellNames = new[] { "cassiopeiatwinfang" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Elise", SpellNames = new[] { "elisehumanq" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "FiddleSticks",
                        SpellNames = new[] { "fiddlesticksdarkwind", "fiddlesticksdarkwindmissile" },
                        Slot = SpellSlot.E
                    });
                Spells.Add(
                    new SpellData { ChampionName = "Gangplank", SpellNames = new[] { "parley" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData { ChampionName = "Janna", SpellNames = new[] { "sowthewind" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData { ChampionName = "Kassadin", SpellNames = new[] { "nulllance" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Katarina",
                        SpellNames = new[] { "katarinaq", "katarinaqmis" },
                        Slot = SpellSlot.Q
                    });
                Spells.Add(
                    new SpellData
                    { ChampionName = "Kayle", SpellNames = new[] { "judicatorreckoning" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Leblanc",
                        SpellNames = new[] { "leblancchaosorb", "leblancchaosorbm" },
                        Slot = SpellSlot.Q
                    });
                Spells.Add(new SpellData { ChampionName = "Lulu", SpellNames = new[] { "luluw" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData
                    { ChampionName = "Malphite", SpellNames = new[] { "seismicshard" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "MissFortune",
                        SpellNames = new[] { "missfortunericochetshot", "missFortunershotextra" },
                        Slot = SpellSlot.Q
                    });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Nami",
                        SpellNames = new[] { "namiwenemy", "namiwmissileenemy" },
                        Slot = SpellSlot.W
                    });
                Spells.Add(
                    new SpellData { ChampionName = "Nunu", SpellNames = new[] { "iceblast" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Pantheon", SpellNames = new[] { "pantheonq" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                    {
                        ChampionName = "Ryze",
                        SpellNames = new[] { "spellflux", "spellfluxmissile" },
                        Slot = SpellSlot.E
                    });
                Spells.Add(
                    new SpellData { ChampionName = "Shaco", SpellNames = new[] { "twoshivpoison" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Sona", SpellNames = new[] { "sonaqmissile" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData { ChampionName = "Swain", SpellNames = new[] { "swaintorment" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Syndra", SpellNames = new[] { "syndrar" }, Slot = SpellSlot.R });
                Spells.Add(
                    new SpellData { ChampionName = "Teemo", SpellNames = new[] { "blindingdart" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                    { ChampionName = "Tristana", SpellNames = new[] { "detonatingshot" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData
                    { ChampionName = "TwistedFate", SpellNames = new[] { "bluecardattack" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData
                    { ChampionName = "TwistedFate", SpellNames = new[] { "goldcardattack" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData
                    { ChampionName = "TwistedFate", SpellNames = new[] { "redcardattack" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData
                    { ChampionName = "Vayne", SpellNames = new[] { "vaynecondemnmissile" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData
                    { ChampionName = "Veigar", SpellNames = new[] { "veigarprimordialburst" }, Slot = SpellSlot.R });
                Spells.Add(
                    new SpellData
                    { ChampionName = "Viktor", SpellNames = new[] { "viktorpowertransfer" }, Slot = SpellSlot.Q });
            }

            private static void ObjSpellMissileOnCreate(GameObject sender, EventArgs args)
            {
                var missile = sender as MissileClient;
                if (missile == null || !missile.IsValid)
                {
                    return;
                }
                var caster = missile.SpellCaster as Obj_AI_Hero;
                if (caster == null || !caster.IsValid || caster.Team == Player.Team || !(missile.Target != null && missile.Target.IsMe))
                {
                    return;
                }
                var spellData =
                    Spells.FirstOrDefault(
                        i =>
                        i.SpellNames.Contains(missile.SData.Name.ToLower())
                        && Menu.SubMenu("EvadeTarget").SubMenu(i.ChampionName.ToLowerInvariant()).Item(i.MissileName).GetValue<bool>());
                if (spellData == null && Orbwalking.IsAutoAttack(missile.SData.Name)
                    && (!missile.SData.Name.ToLower().Contains("crit")
                            ? Menu.SubMenu("EvadeTarget").SubMenu("AA").Item("B").GetValue<bool>()
                              && Player.HealthPercent < Menu.SubMenu("EvadeTarget").SubMenu("AA").Item("BHpU").GetValue<Slider>().Value
                            : Menu.SubMenu("EvadeTarget").SubMenu("AA").Item("C").GetValue<bool>()
                              && Player.HealthPercent < Menu.SubMenu("EvadeTarget").SubMenu("AA").Item("CHpU").GetValue<Slider>().Value))
                {
                    spellData = new SpellData { ChampionName = caster.ChampionName, SpellNames = new[] { missile.SData.Name } };
                }
                if (spellData == null)
                {
                    return;
                }
                DetectedTargets.Add(new Targets { Start = caster.ServerPosition, Obj = missile });
            }

            private static void ObjSpellMissileOnDelete(GameObject sender, EventArgs args)
            {
                var missile = sender as MissileClient;
                if (missile == null || !missile.IsValid)
                {
                    return;
                }
                var caster = missile.SpellCaster as Obj_AI_Hero;
                if (caster == null || !caster.IsValid || caster.Team == Player.Team)
                {
                    return;
                }
                DetectedTargets.RemoveAll(i => i.Obj.NetworkId == missile.NetworkId);
            }

            private static void OnUpdateTarget(EventArgs args)
            {
                if (Player.IsDead)
                {
                    return;
                }
                if (Player.HasBuffOfType(BuffType.SpellShield) || Player.HasBuffOfType(BuffType.SpellImmunity))
                {
                    return;
                }
                if (!Menu.Item("EnabledToggle").GetValue<KeyBind>().Active && !Menu.Item("EnablePress").GetValue<KeyBind>().Active)
                    return;
                foreach (var target in
                    DetectedTargets.Where(i => i.Obj.Position.Distance(Player.Position) <= (0.25f + Game.Ping/1000)* i.Obj.SData.MissileSpeed + 100 ).OrderBy(i => i.Obj.Position.Distance(Player.Position)))
                {
                    if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).IsReady())
                    {
                        var pos = ObjectManager.Player.Position.To2D().Extend(target.Start.To2D(), 250);
                        ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W, pos.To3D());
                    }
                }
            }

            #endregion

            private class SpellData
            {
                #region Fields

                public string ChampionName;

                public SpellSlot Slot;

                public string[] SpellNames = { };

                #endregion

                #region Public Properties

                public string MissileName
                {
                    get
                    {
                        return this.SpellNames.First();
                    }
                }

                #endregion
            }

            private class Targets
            {
                #region Fields

                public MissileClient Obj;

                public Vector3 Start;

                #endregion
            }
        }
    }
}
