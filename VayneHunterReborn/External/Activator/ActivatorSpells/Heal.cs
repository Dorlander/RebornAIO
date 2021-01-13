using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.External.Activator.ActivatorSpells
{
    class Heal : IVHRSpell
    {
        private SpellSlot HealSlot
        {
            get { return ObjectManager.Player.GetSpellSlot("summonerheal"); }
        }

        public void OnLoad()
        {
            DamagePrediction.OnSpellWillKill += DamagePrediction_OnSpellWillKill;
        }

        public void BuildMenu(Menu RootMenu)
        {
            var spellMenu = new Menu("Heal", "dz191.vhr.activator.spells.heal");
            {
                spellMenu.AddItem(new MenuItem("dz191.vhr.activator.spells.heal.onhealth", "On Health < %")).SetValue(new Slider(10,1));
                spellMenu.AddItem(new MenuItem("dz191.vhr.activator.spells.heal.ls", "Evade/Damage Prediction integration")).SetValue(true);
                
                spellMenu.AddItem(new MenuItem("dz191.vhr.activator.spells.heal.sep11", ">>>         On Mode        <<<"));
                
                spellMenu.AddItem(new MenuItem("dz191.vhr.activator.spells.heal.always", "Always")).SetValue(true);
                spellMenu.AddItem(new MenuItem("dz191.vhr.activator.spells.heal.combo", "In Combo")).SetValue(true);
            }
            RootMenu.AddSubMenu(spellMenu);
        }

        public bool ShouldRun()
        {
            return MenuExtensions.GetItemValue<bool>("dz191.vhr.activator.spells.heal.always")
                || (MenuExtensions.GetItemValue<bool>("dz191.vhr.activator.spells.heal.combo") && Variables.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo);
        }

        public void Run()
        {
            if (ShouldRun())
            {
                var rSkills = EvadeHelper.EvadeDetectedSkillshots.Where(
                skillshot =>skillshot.IsAboutToHit(250, ObjectManager.Player.ServerPosition)).ToList();

                if (MenuExtensions.GetItemValue<bool>("dz191.vhr.activator.spells.heal.ls"))
                {
                    if (
                        rSkills.Any(
                            skillshot =>
                                skillshot.Caster.GetSpellDamage(ObjectManager.Player, skillshot.SpellData.SpellName) >=
                                ObjectManager.Player.Health + 15))
                    {
                        CastSpell();
                    }
                }

                if (ObjectManager.Player.HealthPercent <=
                    MenuExtensions.GetItemValue<Slider>("dz191.vhr.activator.spells.heal.onhealth").Value)
                {
                    CastSpell();
                }
            }
        }

        private void DamagePrediction_OnSpellWillKill(Obj_AI_Hero sender, Obj_AI_Hero target, SpellData sData)
        {
            if (target.IsMe && sender.IsEnemy)
            {
                if (ShouldRun() && MenuExtensions.GetItemValue<bool>("dz191.vhr.activator.spells.heal.ls"))
                {
                    CastSpell();
                }
            }
        }

        private void CastSpell()
        {
            if (HealSlot != SpellSlot.Unknown &&
                ObjectManager.Player.Spellbook.GetSpell(HealSlot).State == SpellState.Ready)
            {
                ObjectManager.Player.Spellbook.CastSpell(HealSlot);
            }
        }
    }
}
