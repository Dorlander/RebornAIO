using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VayneHunter_Reborn.External.Translation.Languages
{
    /**
     * Portuguese translation by Sapporo
     * 
     * https://www.joduska.me/forum/user/99408-sapporo/
     */
    class Portuguese : IVHRLanguage
    {
        public string GetName()
        {
            return "Portuguese";
        }

        public Dictionary<string, string> GetTranslations()
        {
            var translations = new Dictionary<string, string>
            {
                {"dz191.vhr.combo.r.minenemies", "Min. de Inimigos para R"},

                {"dz191.vhr.combo.q.2wstacks", "Apenas use Q se 2 Stacks do W  estiver no Alvo"},

                {"dz191.vhr.mixed.q.2wstacks", "Apenas use Q se 2 Stacks do W estiver no Alvo"},

                {"dz191.vhr.mixed.ethird", "Usar o E para procar terceiro hit"},

                {"dz191.vhr.farm.condemnjungle", "Usar o E para condenar monstros da jungle"},

                {"dz191.vhr.farm.qjungle", "Usar o Q contra monstros da jungle"},

                {"dz191.vhr.misc.condemn.qlogic", "Q Lógico"},

                {"dz191.vhr.mixed.mirinQ", "Q para Parede sempre quando possível (Modo Mirin)"},

                {"dz191.vhr.misc.tumble.smartq", "Tentar QE quando possível"},

                {"dz191.vhr.misc.tumble.noaastealthex", "Não dar AA quando estiver invisível"},

                {"dz191.vhr.misc.tumble.noqenemies", "Não dar o Q em direção aos inimigos"},

                {"dz191.vhr.misc.tumble.dynamicqsafety", "Usar Q dinâmico para Distância Segura"},

                {"dz191.vhr.misc.tumble.qspam", "Ignorar verificações no Q"},

                {"dz191.vhr.misc.tumble.qinrange", "Q para dar KS"},

                {"dz191.vhr.misc.tumble.walltumble.warning", "Clique e segure Walltumble"},

                {
                    "dz191.vhr.misc.tumble.walltumble.warning.2",
                    "Ela irá andar para o lugar do Tumble mais próximo e vai usar o Tumble"
                },

                {"dz191.vhr.misc.tumble.walltumble", "Tumble através da Parede (WallTumble)"},

                {"dz191.vhr.misc.condemn.condemnmethod", "Método do Condenar"},

                {"dz191.vhr.misc.condemn.pushdistance", "Distância do E para empurrar"},

                {"dz191.vhr.misc.condemn.accuracy", "Precisão (Apenas para o Revolution)"},

                {"dz191.vhr.misc.condemn.enextauto", "E no próximo Auto"},

                {"dz191.vhr.misc.condemn.onlystuncurrent", "Apenas stunar o alvo atual"},

                {"dz191.vhr.misc.condemn.autoe", "E Automático"},

                {"dz191.vhr.misc.condemn.eks", "KS Inteligente com E"},

                {"dz191.vhr.misc.condemn.noeaa", "Não usar E se o Alvo pode ser morto em X AA"},

                {"dz191.vhr.misc.condemn.trinketbush", "Usar Trinket na Bush com Condenar"},

                {"dz191.vhr.misc.condemn.lowlifepeel", "Peel com E quando estiver vida baixa"},

                {"dz191.vhr.misc.condemn.noeturret", "Não usar E debaixo da torre inimiga"},

                {"dz191.vhr.misc.general.antigp", "Anti Gapcloser"},

                {"dz191.vhr.misc.general.interrupt", "Interrupter"},

                {"dz191.vhr.misc.general.antigpdelay", "Atraso no Anti Gapcloser (ms)"},

                {"dz191.vhr.misc.general.specialfocus", "Focar alvos com 2 marcas do W"},

                {"dz191.vhr.misc.general.reveal", "Revelar Invisível (Pink Ward / Lens)"},

                {"dz191.vhr.misc.general.disablemovement", "Desativar movimento do Orbwalker"},

                {"dz191.vhr.misc.general.disableattk", "Desativar Ataques do Orbwalker"},

                {"dz191.vhr.draw.spots", "Desenhar Locais"},

                {"dz191.vhr.draw.range", "Desenhar Ranges Inimigos"},

                {"dz191.vhr.draw.qpos", "Posição do Q Reborn (Debug)"},

                {"dz191.vhr.activator.onkey", "Tecla Ativadora"},

                {"dz191.vhr.activator.always", "Sempre Habilitada"},

                {"dz191.vhr.activator.spells.barrier.onhealth", "Na vida < %"},

                {"dz191.vhr.activator.spells.barrier.ls", "Integração do Evade/Damage Prediction"},

                {"dz191.vhr.activator.spells.heal.onhealth", "Na vida < %"},

                {"dz191.vhr.activator.spells.heal.ls", "Integração do Evade/Damage Prediction"},

            };

            return translations;
        }
    }
}
