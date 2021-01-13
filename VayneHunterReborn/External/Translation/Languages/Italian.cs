using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VayneHunter_Reborn.External.Translation.Languages
{
    /// <summary>
    /// Italian by Exory.
    /// https://www.joduska.me/forum/user/98704-exory/
    /// </summary>
    class Italian : IVHRLanguage
    {
        public string GetName()
        {
            return "Italian";
        }

        public Dictionary<string, string> GetTranslations()
        {
            var translations = new Dictionary<string, string>
            {
                {"dz191.vhr.combo.r.minenemies", "Min. numero di nemici per uso R"},
                {"dz191.vhr.combo.q.2wstacks", "Usa Q solo se il bersaglio ha 2 stacks di W"},
                {"dz191.vhr.mixed.q.2wstacks", "Usa Q solo se il bersaglio ha 2 stacks di W"},
                {"dz191.vhr.mixed.ethird", "Usa Condanna per proccare la W"},
                {"dz191.vhr.farm.condemnjungle", "Condanna i mostri in giungla"},
                {"dz191.vhr.farm.qjungle", "Usa Capriola contro mostri in giungla"},
                {"dz191.vhr.misc.condemn.qlogic", "Logica della Q"},
                {"dz191.vhr.mixed.mirinQ", "Capriola sui Muri quando possibile (Mirin Mode)"},
                {"dz191.vhr.misc.tumble.smartq", "Prova Capriola->Condanna quando possibile"},
                {"dz191.vhr.misc.tumble.noaastealthex", "Non attaccare da invisibile"},
                {"dz191.vhr.misc.tumble.noqenemies", "Non usare Capriola addosso ai nemici"},
                {"dz191.vhr.misc.tumble.dynamicqsafety", "Usa distanza dinamica di sicurezza per Capriola"},
                {"dz191.vhr.misc.tumble.qspam", "Ignora i controlli per Capriola (Spam mode)"},
                {"dz191.vhr.misc.tumble.qinrange", "Usa Capriola quando puoi uccidere"},
                {"dz191.vhr.misc.tumble.walltumble.warning", "Clicca e tieni premuto WallTumble"},
                {"dz191.vhr.misc.tumble.walltumble.warning.2", "Camminera' verso il punto da WallTumble piu' vicino e lo attraversera'"},
                {"dz191.vhr.misc.tumble.walltumble", "Attraversa i Muri (WallTumble)"},
                {"dz191.vhr.misc.condemn.condemnmethod", "Metodo di Condanna"},
                {"dz191.vhr.misc.condemn.pushdistance", "Distanza di respinzione Condanna"},
                {"dz191.vhr.misc.condemn.accuracy", "Precisione (Solo Metodo Revolution)"},
                {"dz191.vhr.misc.condemn.enextauto", "Usa condanna al prossimo attacco base"},
                {"dz191.vhr.misc.condemn.onlystuncurrent", "Condanna per stunnare solo il target"},
                {"dz191.vhr.misc.condemn.autoe", "Usa Condanna in automatico"},
                {"dz191.vhr.misc.condemn.eks", "Condanna per kill intelligente"},
                {"dz191.vhr.misc.condemn.noeaa", "Non condannare se il bersaglio puo' essere ucciso in X Attacchi"},
                {"dz191.vhr.misc.condemn.trinketbush", "Usa il Trinket nei bush dopo condanna"},
                {"dz191.vhr.misc.condemn.lowlifepeel", "AutoPeel con Condanna quando hai poca vita"},
                {"dz191.vhr.misc.condemn.noeturret", "Non usare Condanna sotto la torre nemica"},
                {"dz191.vhr.misc.general.antigp", "Anti-Gapcloser"},
                {"dz191.vhr.misc.general.interrupt", "Interruttore"},
                {"dz191.vhr.misc.general.antigpdelay", "Anti-Gapcloser (ms)"},
                {"dz191.vhr.misc.general.specialfocus", "Punta i bersagli che hanno 2 stacks di W"},
                {"dz191.vhr.misc.general.reveal", "Rivela Invisibili (Ward Rosa / Trinket Rosso)"},
                {"dz191.vhr.misc.general.disablemovement", "Disabilita Movimenti dell'Orbwalker"},
                {"dz191.vhr.misc.general.disableattk", "Disabilita Attacchi dell'Orbwalker"},
                {"dz191.vhr.draw.spots", "Disegna punti di WallTumble"},
                {"dz191.vhr.draw.range", "Disegna i range dei nemici."},
                {"dz191.vhr.draw.qpos", "Posizione Logica Q Reborn (Debug)"},
                {"dz191.vhr.activator.onkey", "Tasto Activator"},
                {"dz191.vhr.activator.always", "Sempre abilitato"},
                {"dz191.vhr.activator.spells.barrier.onhealth", "Se la vita e' < %"},
                {"dz191.vhr.activator.spells.barrier.ls", "Integrazione di predizione Evade/Danno"},
                {"dz191.vhr.activator.spells.heal.onhealth", "Se la vita e' < %"},
                {"dz191.vhr.activator.spells.heal.ls", "Integrazione di predizione Evade/Danno"},
                {" ", " "},
            };

            return translations;
        }
    }
}
