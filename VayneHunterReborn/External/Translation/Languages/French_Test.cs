using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VayneHunter_Reborn.External.Translation.Languages
{
    /*
     * French Translation by Koala974
     * https://www.joduska.me/forum/user/19647-koala974/
     */

    class French : IVHRLanguage
    {
        public string GetName()
        {
            return "French";
        }

        public Dictionary<string, string> GetTranslations()
        {
            var translations = new Dictionary<string, string>
            {
                {"dz191.vhr.combo.r.minenemies", "Nombre min. ennemies pour R"},
                {"dz191.vhr.combo.q.2wstacks", "A seulement si 2 stacks Z sur la cible"},
                {"dz191.vhr.mixed.q.2wstacks", "A seulement si 2 stacks Z sur la cible"},
                {"dz191.vhr.mixed.ethird", "Utiliser le E pour le 3 eme stack"},
                {"dz191.vhr.farm.condemnjungle", "Utiliser le E contre les monstres de la jungle"},
                {"dz191.vhr.farm.qjungle", "Utiliser le A contre les monstres de la jungle"},
                {"dz191.vhr.misc.condemn.qlogic", "Logique A"},
                {"dz191.vhr.mixed.mirinQ", "A contre le mur quand possible (Mirin Mode)"},
                {"dz191.vhr.misc.tumble.smartq", "AE quand possible"},
                {"dz191.vhr.misc.tumble.noaastealthex", "Ne pas AA quand invisible"},
                {"dz191.vhr.misc.tumble.noqenemies", "Ne pas A vers les ennemis"},
                {"dz191.vhr.misc.tumble.dynamicqsafety", "Utiliser la vérification de distance de sécurité avant de roulade"},
                {"dz191.vhr.misc.tumble.qspam", "Ignorer les vérifications du A"},
                {"dz191.vhr.misc.tumble.qinrange", "A pour KS"},
                {"dz191.vhr.misc.tumble.walltumble.warning", "Restez appuyer pour dash à travers le mur"},
                {"dz191.vhr.misc.tumble.walltumble.warning.2", "Il ira vers le spot le plus près et roulade à travers"},
                {"dz191.vhr.misc.tumble.walltumble", "Roulade à travers le mur (RouladeMur)"},
                {"dz191.vhr.misc.condemn.condemnmethod", "Méthode Condamn"},
                {"dz191.vhr.misc.condemn.pushdistance", "Distance Poussée E"},
                {"dz191.vhr.misc.condemn.accuracy", "Precision(Revolution seulement)"},
                {"dz191.vhr.misc.condemn.enextauto", "E prochaine auto"},
                {"dz191.vhr.misc.condemn.onlystuncurrent", "Stun uniquement la cible actuelle"},
                {"dz191.vhr.misc.condemn.autoe", "Auto E"},
                {"dz191.vhr.misc.condemn.eks", "KS au E intelligent"},
                {"dz191.vhr.misc.condemn.noeaa", "Ne pas E si la cible peut etre tuée en X AA"},
                {"dz191.vhr.misc.condemn.trinketbush", "Trinket le bush quand condamn"},
                {"dz191.vhr.misc.condemn.lowlifepeel", "Se sauver avec le E quand low hp"},
                {"dz191.vhr.misc.condemn.noeturret", "Ne pas utiliser le E sous la tour ennemie"},
                {"dz191.vhr.misc.general.antigp", "Anti Gapcloser"},
                {"dz191.vhr.misc.general.interrupt", "Interrupteur"},
                {"dz191.vhr.misc.general.antigpdelay", "Anti Gapcloser Delai (ms)"},
                {"dz191.vhr.misc.general.specialfocus", "Focus la cible avec 2 stacks de Z"},
                {"dz191.vhr.misc.general.reveal", "Réveler les invisibles (Pink Ward / Lens)"},
                {"dz191.vhr.misc.general.disablemovement", "Désactiver les mouvements de l'orbwalker"},
                {"dz191.vhr.misc.general.disableattk", "Désactiver les attaques de l'orbwalker"},
                {"dz191.vhr.draw.spots", "Montrer les spots"},
                {"dz191.vhr.draw.range", "Montrer la portée des ennemis"},
                {"dz191.vhr.draw.qpos", "Positionnement du A, Reborn (Debug)"},
                {"dz191.vhr.activator.onkey", "Touche Activateur Items"},
                {"dz191.vhr.activator.always", "Toujours activé"},
                {"dz191.vhr.activator.spells.barrier.onhealth", "Quand HP < %"},
                {"dz191.vhr.activator.spells.barrier.ls", "Intégrer les prédictions de dégats/Evade"},
                {"dz191.vhr.activator.spells.heal.onhealth", "Quand HP < %"},
                {"dz191.vhr.activator.spells.heal.ls", "Intégrer les prédictions de dégats/Evade"},
            };

            return translations;
        }
    }
}

