#region

#endregion

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Support.Util;

namespace Support.Plugins
{
    public class Annie : PluginBase
    {
        public Annie()
        {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 625);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 600);

            Q.SetTargetted(250, 1400);
            W.SetSkillshot(600, (float) (50*Math.PI/180), float.MaxValue, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(250, 200, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.CastCheck(Target, "ComboQ"))
                {
                    Q.Cast(Target, false);
                }
                if (W.CastCheck(Target, "ComboW"))
                {
                    W.Cast(Target, true);
                }

                if (R.CastCheck(Target, "ComboR"))
                {
                    R.Cast(Target, true);
                }
                CastE();
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }
            if (GetPassiveStacks() >= 4)
            {
                if (Q.CastCheck(target, "Interrupt.Q"))
                {
                    Q.Cast(target);
                    return;
                }
                if (W.CastCheck(target, "Interrupt.W"))
                {
                    W.CastOnUnit(target);
                    return;
                }
            }
            if (GetPassiveStacks() == 3)
            {
                if (E.IsReady())
                {
                    E.Cast();
                    if (Q.CastCheck(target, "Interrupt.Q"))
                    {
                        Q.Cast(target);
                        return;
                    }
                    if (W.CastCheck(target, "Interrupt.W"))
                    {
                        W.CastOnUnit(target);
                        return;
                    }
                }
                if (Q.CastCheck(target, "Interrupt.Q") && W.CastCheck(target, "Interrupt.W"))
                {
                    Q.Cast(target);
                    W.CastOnUnit(target);
                }
            }
        }

        private void CastE()
        {
            if (GetPassiveStacks() < 4 && !ObjectManager.Player.IsRecalling())
            {
                E.Cast();
            }
        }

        //sosharp love xSalice
        private int GetPassiveStacks()
        {
            var buffs =
                ObjectManager.Player.Buffs.Where(
                    buff => (buff.Name.ToLower() == "pyromania" || buff.Name.ToLower() == "pyromania_particle"));
            var buffInstances = buffs as BuffInstance[] ?? buffs.ToArray();
            if (!buffInstances.Any())
            {
                return 0;
            }
            var buf = buffInstances.First();
            var count = buf.Count >= 4 ? 4 : buf.Count;
            return buf.Name.ToLower() == "pyromania_particle" ? 4 : count;
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
            config.AddBool("ComboR", "Use R", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Interrupt.Q", "Use Q to Interrupt Spells", true);
            config.AddBool("Interrupt.W", "Use W to Interrupt Spells", true);
        }
    }
}