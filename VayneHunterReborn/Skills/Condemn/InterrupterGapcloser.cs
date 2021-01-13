using System;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.External;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Skills.Condemn
{
    class InterrupterGapcloser
    {
        public static void OnLoad()
        {
            Interrupter2.OnInterruptableTarget += OnInterruptableTarget;
            CustomAntigapcloser.OnEnemyGapcloser += CustomAntigapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnCreate += GameObject_OnCreate;
        }

        private static void CustomAntigapcloser_OnEnemyGapcloser(External.ActiveGapcloser gapcloser)
        {
            if (MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.general.antigp") && Variables.spells[SpellSlot.E].IsReady())
            {
                LeagueSharp.Common.Utility.DelayAction.Add(MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.general.antigpdelay").Value,
                    () =>
                    {
                        if (gapcloser.Sender.IsValidTarget(Variables.spells[SpellSlot.E].Range)
                            && gapcloser.End.Distance(ObjectManager.Player.ServerPosition) <= 400f
                            && MenuExtensions.GetItemValue<bool>(
                            string.Format("dz191.vhr.agplist.{0}.{1}", gapcloser.Sender.ChampionName.ToLowerInvariant(), gapcloser.SpellName)
                            ))
                        {
                            Variables.spells[SpellSlot.E].Cast(gapcloser.Sender);
                        }
                    });
            }
        }

        private static void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.general.interrupt"))
            {
                if (args.DangerLevel == Interrupter2.DangerLevel.High && sender.IsValidTarget(Variables.spells[SpellSlot.E].Range))
                {
                    Variables.spells[SpellSlot.E].Cast(sender);
                }
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {

            if (sender is Obj_AI_Hero)
            {
                var s2 = (Obj_AI_Hero)sender;
                if (s2.IsValidTarget() && s2.ChampionName == "Pantheon" && s2.GetSpellSlot(args.SData.Name) == SpellSlot.W)
                {
                    if (MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.general.antigp") && args.Target.IsMe && Variables.spells[SpellSlot.E].IsReady())
                    {
                        if (s2.IsValidTarget(Variables.spells[SpellSlot.E].Range))
                        {
                            Variables.spells[SpellSlot.E].Cast(s2);
                        }
                    }
                }
            }

        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.general.antigp") && Variables.spells[SpellSlot.E].IsReady())
            {
                if (sender.IsEnemy && sender.Name == "Rengar_LeapSound.troy")
                {
                    var rengarEntity = HeroManager.Enemies.Find(h => h.ChampionName.Equals("Rengar") && h.IsValidTarget(Variables.spells[SpellSlot.E].Range));
                    if (rengarEntity != null)
                    {
                        Variables.spells[SpellSlot.E].Cast(rengarEntity);
                    }
                }
            }
        }
    }
}
