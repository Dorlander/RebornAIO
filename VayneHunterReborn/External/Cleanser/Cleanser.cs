using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.External.Cleanser
{
    class Cleanser
    {
        #region Spells & Bufftypes
        private static readonly BuffType[] Buffs = { BuffType.Blind, BuffType.Charm, BuffType.CombatDehancer, BuffType.Fear, BuffType.Flee, BuffType.Knockback, BuffType.Knockup, BuffType.Polymorph, BuffType.Silence, BuffType.Sleep, BuffType.Snare, BuffType.Stun, BuffType.Suppression, BuffType.Taunt };

        private static readonly List<CleanserSpell> CleanserSpells = new List<CleanserSpell>
        {
            new CleanserSpell
            {
                ChampName = "Warwick",
                IsEnabled = true,
                SpellBuff = "InfiniteDuress",
                SpellName = "Warwick R",
                RealName = "warwickR",
                OnlyKill = false,
                Slot = SpellSlot.R,
                Delay = 100f
            },
            new CleanserSpell
            {
                ChampName = "Zed",
                IsEnabled = true,
                SpellBuff = "zedulttargetmark",
                SpellName = "Zed R",
                RealName = "zedultimate",
                OnlyKill = true,
                Slot = SpellSlot.R,
                Delay = 800f
            },
            new CleanserSpell
            {
                ChampName = "Rammus",
                IsEnabled = true,
                SpellBuff = "PuncturingTaunt",
                SpellName = "Rammus E",
                RealName = "rammusE",
                OnlyKill = false,
                Slot = SpellSlot.E,
                Delay = 100f                
            },
            /** Danger Level 4 Spells*/
            new CleanserSpell
            {
                ChampName = "Skarner",
                IsEnabled = true,
                SpellBuff = "SkarnerImpale",
                SpellName = "Skaner R",
                RealName = "skarnerR",
                OnlyKill = false,
                Slot = SpellSlot.R,
                Delay = 100f
            },
            new CleanserSpell
            {
                ChampName = "Fizz",
                IsEnabled = true,
                SpellBuff = "FizzMarinerDoom",
                SpellName = "Fizz R",
                RealName = "FizzR",
                OnlyKill = false,
                Slot = SpellSlot.R,
                Delay = 100f
            },
            new CleanserSpell
            {
                ChampName = "Galio",
                IsEnabled = true,
                SpellBuff = "GalioIdolOfDurand",
                SpellName = "Galio R",
                RealName = "GalioR",
                OnlyKill = false,
                Slot = SpellSlot.R,
                Delay = 100f
            },
            new CleanserSpell
            {
                ChampName = "Malzahar",
                IsEnabled = true,
                SpellBuff = "AlZaharNetherGrasp",
                SpellName = "Malzahar R",
                RealName = "MalzaharR",
                OnlyKill = false,
                Slot = SpellSlot.R,
                Delay = 200f
            },
            /** Danger Level 3 Spells*/
            new CleanserSpell
            {
                ChampName = "Zilean",
                IsEnabled = false,
                SpellBuff = "timebombenemybuff",
                SpellName = "Zilean Q",
                OnlyKill = true,
                Slot = SpellSlot.Q,
                Delay = 700f
            },
            new CleanserSpell
            {
                ChampName = "Vladimir",
                IsEnabled = false,
                SpellBuff = "VladimirHemoplague",
                SpellName = "Vladimir R",
                RealName = "VladimirR",
                OnlyKill = true,
                Slot = SpellSlot.R,
                Delay = 700f
            },
            new CleanserSpell
            {
                ChampName = "Mordekaiser",
                IsEnabled = true,
                SpellBuff = "MordekaiserChildrenOfTheGrave",
                SpellName = "Mordekaiser R",
                OnlyKill = true,
                 Slot = SpellSlot.R,
                Delay = 800f
            },
            /** Danger Level 2 Spells*/
            new CleanserSpell
            {
                ChampName = "Poppy",
                IsEnabled = true,
                SpellBuff = "PoppyDiplomaticImmunity",
                SpellName = "Poppy R",
                RealName = "PoppyR",
                OnlyKill = false,
                 Slot = SpellSlot.R,
                Delay = 100f
            }
        };
        #endregion

        private static float _lastCycle;

        private static readonly Items.Item QSS = new Items.Item(3140);

        private static readonly Items.Item Merc = new Items.Item(3139);

        private static readonly Items.Item Dervish = new Items.Item(3137);

        public static void LoadMenu(Menu RootMenu)
        {
            var cleanserMenu = new Menu("[VHR] Cleanser", "dz191.vhr.cleanser");
            {
                var spellCleanserMenu = new Menu("Cleanser - Spells", "dz191.vhr.cleanser.spells");
                {
                    foreach (var spell in CleanserSpells.Where(h => HeroManager.Enemies.Any(m => m.ChampionName.ToLower() == h.ChampName.ToLower())))
                    {
                        var sMenu = new Menu(spell.SpellName, spell.SpellBuff);
                        sMenu.AddItem(
                            new MenuItem("dz191.vhr.cleanser.spells." + spell.SpellBuff + ".A", "Always").SetValue(
                                !spell.OnlyKill));
                        sMenu.AddItem(
                            new MenuItem("dz191.vhr.cleanser.spells." + spell.SpellBuff + ".K", "Only if killed by it")
                                .SetValue(spell.OnlyKill));
                        spellCleanserMenu.AddSubMenu(sMenu);
                    }
                    cleanserMenu.AddSubMenu(spellCleanserMenu);
                }

                var buffCleanserMenu = new Menu("Cleanser - Bufftype Cleanser", "dz191.vhr.cleanser.bufftype");
                {
                    foreach (var buffType in Buffs)
                    {
                        buffCleanserMenu.AddItem(new MenuItem("dz191.vhr.cleanser.bufftype."+ buffType, buffType.ToString()).SetValue(true));
                    }

                    buffCleanserMenu.AddItem(new MenuItem("dz191.vhr.cleanser.bufftype.minbuffs", "Min Buffs").SetValue(new Slider(2, 1, 5)));
                    cleanserMenu.AddSubMenu(buffCleanserMenu);
                }

                cleanserMenu.AddKeybind("dz191.vhr.cleanser.use.combo", "Cleanser Key", new Tuple<uint, KeyBindType>(32, KeyBindType.Press));

                cleanserMenu.AddItem(new MenuItem("dz191.vhr.cleanser.use", "Use Always").SetValue(true));

                RootMenu.AddSubMenu(cleanserMenu);
            }
        }

        public static void OnUpdate()
        {
            if (Utils.GameTimeTickCount - _lastCycle < 100)
            {
                return;
            }

            _lastCycle = Utils.GameTimeTickCount;

            if (!MenuExtensions.GetItemValue<KeyBind>("dz191.vhr.cleanser.use.combo").Active &&
                !MenuExtensions.GetItemValue<bool>("dz191.vhr.cleanser.use"))
            {
                return;
            }

            var CleanseItem = GetCleanseItem();

            if (CleanseItem != null)
            {
                HandleOnKillCleanser(CleanseItem);
                HandleSpellCleanser(CleanseItem);
                HandleBuffTypeCleanser(CleanseItem);
            }
        }

        private static void HandleSpellCleanser(Items.Item CleanseItem)
        {
            var spellsOnMe =
                    CleanserSpells.Where(
                        spell =>
                            ObjectManager.Player.HasBuff(spell.SpellBuff) &&
                            (MenuExtensions.GetItemValue<bool>("dz191.vhr.cleanser.spells." + spell.SpellBuff + ".A")));
            if (spellsOnMe.Any())
            {
                CastCleanse(CleanseItem);
            }
        }

        private static void HandleOnKillCleanser(Items.Item CleanseItem)
        {
            var spellsOnMe =
                CleanserSpells.Where(
                    spell =>
                        ObjectManager.Player.HasBuff(spell.SpellBuff) &&
                        (MenuExtensions.GetItemValue<bool>("dz191.vhr.cleanser.spells." + spell.SpellBuff + ".K") ||
                         MenuExtensions.GetItemValue<bool>("dz191.vhr.cleanser.spells." + spell.SpellBuff + ".A")) &&
                        (HeroManager.Enemies.FirstOrDefault(m => m.ChampionName == spell.ChampName) != null
                            ? HeroManager.Enemies.FirstOrDefault(m => m.ChampionName == spell.ChampName)
                                .GetSpellDamage(ObjectManager.Player, spell.Slot) >= ObjectManager.Player.Health + 15
                            : false));

            if (spellsOnMe.Any())
            {
                CastCleanse(CleanseItem);
            }
        }

        private static void HandleBuffTypeCleanser(Items.Item CleanseItem)
        {
            var buffCount = Buffs.Count(buff => ObjectManager.Player.HasBuffOfType(buff) && MenuExtensions.GetItemValue<bool>(string.Format("dz191.vhr.cleanser.bufftype.{0}", buff)));
            if (buffCount >= MenuExtensions.GetItemValue<Slider>("dz191.vhr.cleanser.bufftype.minbuffs").Value)
            {
                CastCleanse(CleanseItem);
            }
        }

        private static void CastCleanse(Items.Item CleanseItem)
        {
            LeagueSharp.Common.Utility.DelayAction.Add((int)(250 + Game.Ping/2f), () =>
            {
                CleanseItem.Cast();
            });
        }

        private static Items.Item GetCleanseItem()
        {
            if (QSS.IsOwned() && QSS.IsReady())
            {
                return QSS;
            }

            if (Merc.IsOwned() && Merc.IsReady())
            {
                return Merc;
            }

            if (Dervish.IsOwned() && Dervish.IsReady())
            {
                return Dervish;
            }

            return null;
        }
    }
}
