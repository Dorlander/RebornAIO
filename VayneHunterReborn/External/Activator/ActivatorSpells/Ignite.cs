using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.External.Activator.ActivatorSpells
{
    class Ignite : IVHRSpell
    {
        private SpellSlot IgniteSlot
        {
            get { return ObjectManager.Player.GetSpellSlot("summonerdot"); }
        }

        public void OnLoad()
        {
        }

        public void BuildMenu(Menu RootMenu)
        {
            var spellMenu = new Menu("Ignite", "dz191.vhr.activator.spells.ignite");
            {
                spellMenu.AddItem(new MenuItem("dz191.vhr.activator.spells.ignite.sep11", ">>>         On Mode        <<<"));

                spellMenu.AddItem(new MenuItem("dz191.vhr.activator.spells.ignite.always", "Always")).SetValue(true);
                spellMenu.AddItem(new MenuItem("dz191.vhr.activator.spells.ignite.combo", "In Combo")).SetValue(true);
            }
            RootMenu.AddSubMenu(spellMenu);
        }

        public bool ShouldRun()
        {
            return MenuExtensions.GetItemValue<bool>("dz191.vhr.activator.spells.ignite.always")
                || (MenuExtensions.GetItemValue<bool>("dz191.vhr.activator.spells.ignite.combo") && Variables.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo);
        }

        public void Run()
        {
            if (ShouldRun())
            {
                var hero =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            h => h.IsValidTarget(465f)).OrderBy(m => m.Health);
                    var myHero = hero.FirstOrDefault();
                    if (myHero != null)
                    {
                        var healSlot = myHero.GetSpellSlot("summonerheal");
                        var health = myHero.Health + 10;

                        if (healSlot != SpellSlot.Unknown && myHero.Spellbook.GetSpell(healSlot).State == SpellState.Ready)
                        {
                            //Let's assume he'll use heal. 0.5f because of grevious wounds.
                            health += GetHealAmount(myHero) * 0.5f;
                        }

                        if (health <= ObjectManager.Player.GetSummonerSpellDamage(myHero, Damage.SummonerSpell.Ignite))
                        {
                            CastSpell(myHero);
                        }
                    }
            }
        }

        private int GetHealAmount(Obj_AI_Hero Hero)
        {
            return 90 + (Hero.Level - 1) * 15;
        }

        private void CastSpell(Obj_AI_Hero target)
        {
            if (IgniteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.Spellbook.GetSpell(IgniteSlot).State == SpellState.Ready)
            {
                ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, target);
            }
        }
    }
}
