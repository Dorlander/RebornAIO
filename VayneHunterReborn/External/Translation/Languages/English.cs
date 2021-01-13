using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VayneHunter_Reborn.External.Translation.Languages
{
    class English : IVHRLanguage
    {
        public string GetName()
        {
            return "English";
        }

        public Dictionary<string, string> GetTranslations()
        {
            var translations = new Dictionary<string, string>
            {
                {"dz191.vhr.combo.r.minenemies", "Min. R Enemies"},
                {"dz191.vhr.combo.q.2wstacks", "Only Q if 2W Stacks on Target"},
                {"dz191.vhr.mixed.q.2wstacks", "Only Q if 2W Stacks on Target"},
                {"dz191.vhr.mixed.ethird", "Use E for Third Proc"},
                {"dz191.vhr.farm.condemnjungle", "Use E to condemn jungle mobs"},
                {"dz191.vhr.farm.qjungle", "Use Q against jungle mobs"},
                {"dz191.vhr.misc.condemn.qlogic", "Q Logic"},
                {"dz191.vhr.mixed.mirinQ", "Q to Wall when Possible (Mirin Mode)"},
                {"dz191.vhr.misc.tumble.smartq", "Try to QE when possible"},
                {"dz191.vhr.misc.tumble.noaastealthex", "Don't AA while stealthed"},
                {"dz191.vhr.misc.tumble.noqenemies", "Don't Q into enemies"},
                {"dz191.vhr.misc.tumble.dynamicqsafety", "Use dynamic Q Safety Distance"},
                {"dz191.vhr.misc.tumble.qspam", "Ignore Q checks"},
                {"dz191.vhr.misc.tumble.qinrange", "Q For KS"},
                {"dz191.vhr.misc.tumble.walltumble.warning", "Click and hold Walltumble"},
                {"dz191.vhr.misc.tumble.walltumble.warning.2", "It will walk to the nearest Tumble spot and Tumble"},
                {"dz191.vhr.misc.tumble.walltumble", "Tumble Over Wall (WallTumble)"},
                {"dz191.vhr.misc.condemn.condemnmethod", "Condemn Method"},
                {"dz191.vhr.misc.condemn.pushdistance", "E Push Distance"},
                {"dz191.vhr.misc.condemn.accuracy", "Accuracy (Revolution Only)"},
                {"dz191.vhr.misc.condemn.enextauto", "E Next Auto"},
                {"dz191.vhr.misc.condemn.onlystuncurrent", "Only stun current target"},
                {"dz191.vhr.misc.condemn.autoe", "Auto E"},
                {"dz191.vhr.misc.condemn.eks", "Smart E KS"},
                {"dz191.vhr.misc.condemn.noeaa", "Don't E if Target can be killed in X AA"},
                {"dz191.vhr.misc.condemn.trinketbush", "Trinket Bush on Condemn"},
                {"dz191.vhr.misc.condemn.lowlifepeel", "Peel with E when low health"},
                {"dz191.vhr.misc.condemn.noeturret", "No E Under enemy turret"},
                {"dz191.vhr.misc.general.antigp", "Anti Gapcloser"},
                {"dz191.vhr.misc.general.interrupt", "Interrupter"},
                {"dz191.vhr.misc.general.antigpdelay", "Anti Gapcloser Delay (ms)"},
                {"dz191.vhr.misc.general.specialfocus", "Focus targets with 2 W marks"},
                {"dz191.vhr.misc.general.reveal", "Stealth Reveal (Pink Ward / Lens)"},
                {"dz191.vhr.misc.general.disablemovement", "Disable Orbwalker Movement"},
                {"dz191.vhr.misc.general.disableattk", "Disable Orbwalker Attack"},
                {"dz191.vhr.draw.spots", "Draw Spots"},
                {"dz191.vhr.draw.range", "Draw Enemy Ranges"},
                {"dz191.vhr.draw.qpos", "Reborn Q Position (Debug)"},
                {"dz191.vhr.activator.onkey", "Activator Key"},
                {"dz191.vhr.activator.always", "Always Enabled"},
                {"dz191.vhr.activator.spells.barrier.onhealth", "On health < %"},
                {"dz191.vhr.activator.spells.barrier.ls", "Evade/Damage Prediction integration"},
                {"dz191.vhr.activator.spells.heal.onhealth", "On health < %"},
                {"dz191.vhr.activator.spells.heal.ls", "Evade/Damage Prediction integration"},
                {" ", " "},

            };

            return translations;
        }
    }
}
