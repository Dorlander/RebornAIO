using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VayneHunter_Reborn.External.Translation.Languages
{
    /**
    * German Translation by suicidecarl & blacky
    * 
    * https://www.joduska.me/forum/user/103503-suicidecarl/
    * https://www.joduska.me/forum/user/38-blacky/
    */

    class German : IVHRLanguage
    {
        public string GetName()
        {
            return "German";
        }

        public Dictionary<string, string> GetTranslations()
        {
            var translations = new Dictionary<string, string>
            {
                { "dz191.vhr.combo.r.minenemies", "Mindest Anzahl Gegner für R" },
                { "dz191.vhr.combo.q.2wstacks", "Nur Q, wenn 2x W auf dem Gegner" },
                { "dz191.vhr.mixed.q.2wstacks", "Nur Q, wenn 2x W auf dem Gegner" },
                { "dz191.vhr.mixed.ethird", "Nutze E fuer 3. W" },
                { "dz191.vhr.farm.condemnjungle", "E nutzen fuer Jungle Mobs" },
                { "dz191.vhr.farm.qjungle", "Q nutzen für Jungle Mobs" },
                { "dz191.vhr.misc.condemn.qlogic", "Q Logik" },
                { "dz191.vhr.mixed.mirinQ", "Q - wenn moeglich - zur Wand (Mirin Modus)" },
                { "dz191.vhr.misc.tumble.smartq", "Probiere QE, wenn moeglich" },
                { "dz191.vhr.misc.tumble.noaastealthex", "Kein AA wenn Unsichtbar" },
                { "dz191.vhr.misc.tumble.noqenemies", "Kein Q in die Gegner" },
                { "dz191.vhr.misc.tumble.dynamicqsafety", "Nutze dynamischen Q-Sicherheitsabstand" },
                { "dz191.vhr.misc.tumble.qspam", "Ignoriere Q check" },
                { "dz191.vhr.misc.tumble.qinrange", "Q fuer KS" },
                { "dz191.vhr.misc.tumble.walltumble.warning", "Druecke und halte fuer Walltumble" },
                {
                    "dz191.vhr.misc.tumble.walltumble.warning.2",
                    "Er laeuft zum naehsten Walltumblepunkt und fuehrt Hechtrolle aus"
                },
                { "dz191.vhr.misc.tumble.walltumble", "Hechtrolle ueber Wand (WallTumble)" },
                { "dz191.vhr.misc.condemn.condemnmethod", "Condemn Methode" },
                { "dz191.vhr.misc.condemn.pushdistance", "E Pushdistanz" },
                { "dz191.vhr.misc.condemn.accuracy", "Genauigkeit (Nur Revolution)" },
                { "dz191.vhr.misc.condemn.enextauto", "Beim nächsten Autoattack E nutzen" },
                { "dz191.vhr.misc.condemn.onlystuncurrent", "Nur jetziges Ziel stunnen" },
                { "dz191.vhr.misc.condemn.autoe", "Automatisch E" },
                { "dz191.vhr.misc.condemn.eks", "Schlauer E KS" },
                { "dz191.vhr.misc.condemn.noeaa", "Kein E nutzen, wenn Ziel mit X AA's getoetet werden kann" },
                { "dz191.vhr.misc.condemn.trinketbush", "Busch warden nach E" },
                { "dz191.vhr.misc.condemn.lowlifepeel", "... mit E wenn wenig Leben" },
                { "dz191.vhr.misc.condemn.noeturret", "Kein E nutzen, wenn unter gegnerischem Tower" },
                { "dz191.vhr.misc.general.antigp", "Gegner wegschießen" },
                { "dz191.vhr.misc.general.interrupt", "Unterbrechen" },
                { "dz191.vhr.misc.general.antigpdelay", "Verzoegerung von Gegner wegschießen (ms)" },
                { "dz191.vhr.misc.general.specialfocus", "Fokussiere Gegner mit 2x W auf ihnen" },
                { "dz191.vhr.misc.general.reveal", "Unsichtbare aufdecken (Pink Ward / Spaeherlinse)" },
                { "dz191.vhr.misc.general.disablemovement", "Orbwalkerbewegungen ausschalten" },
                { "dz191.vhr.misc.general.disableattk", "Orbwalkerattacken ausschalten" },
                { "dz191.vhr.draw.spots", "Spots anzeigen lassen" },
                { "dz191.vhr.draw.range", "Gegnerische Reichweite anzeigen lassen" },
                { "dz191.vhr.draw.qpos", "Reborn Q Position (Debug)" },
                { "dz191.vhr.activator.onkey", "Activatortaste" },
                { "dz191.vhr.activator.always", "Immer aktiviert" },
                { "dz191.vhr.activator.spells.barrier.onhealth", "Bei Leben < %" },
                { "dz191.vhr.activator.spells.barrier.ls", "Evade/Schadenprediction integrierung" },
                { "dz191.vhr.activator.spells.heal.onhealth", "Bei Leben < %" },
                { "dz191.vhr.activator.spells.heal.ls", "Evade/Schadenprediction integrierung" },
            };

            return translations;
        }
    }
}
