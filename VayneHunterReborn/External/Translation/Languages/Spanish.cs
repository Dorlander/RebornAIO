using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VayneHunter_Reborn.External.Translation.Languages
{
    /// <summary>
    /// Spanish by Dakkros.
    /// https://www.joduska.me/forum/user/90711-dakkros/
    /// </summary>
    class Spanish : IVHRLanguage
    {
        public string GetName()
        {
            return "Spanish";
        }

        public Dictionary<string, string> GetTranslations()
        {
            var translations = new Dictionary<string, string>
            {
                { "dz191.vhr.combo.r.minenemies", "Enemigos mínimos para el uso de R" },
                { "dz191.vhr.combo.q.2wstacks", "Usar solo Q si hay 2 stacks de la W en el enemigo" },
                { "dz191.vhr.mixed.q.2wstacks", "Usar solo Q si hay 2 stacks de la W en el enemigo" },
                { "dz191.vhr.mixed.ethird", "Usar la E en la tercera pasiva" },
                { "dz191.vhr.farm.condemnjungle", "Usar la E en los monstruos de la jungla" },
                { "dz191.vhr.farm.qjungle", "Usar la Q contra los minions" },
                { "dz191.vhr.misc.condemn.qlogic", "Lógica de la Q" },
                { "dz191.vhr.mixed.mirinQ", "Usar la Q hacia el muro cuando sea posible (Mirin Mode)" },
                { "dz191.vhr.misc.tumble.smartq", "Intentar usar QE cuando sea posible" },
                { "dz191.vhr.misc.tumble.noaastealthex", "No usar la Q mientras estar invisible" },
                { "dz191.vhr.misc.tumble.noqenemies", "No usar la Q contra los enemigos" },
                { "dz191.vhr.misc.tumble.dynamicqsafety", "Usar la Q en distancia segura" },
                { "dz191.vhr.misc.tumble.qspam", "Ignorar las comprobaciones de las Q" },
                { "dz191.vhr.misc.tumble.qinrange", "Usar la Q para KS" },
                { "dz191.vhr.misc.tumble.walltumble.warning", "Clickear y aguantar para transpasar el muro" },
                {
                    "dz191.vhr.misc.tumble.walltumble.warning.2",
                    "Va a andar hacia el muro mas cercano para saltarlo con la Q "
                },
                { "dz191.vhr.misc.tumble.walltumble", "Usar la Q sobre el muro (WallTumble)" },
                { "dz191.vhr.misc.condemn.condemnmethod", "El Método del Stun" },
                { "dz191.vhr.misc.condemn.pushdistance", "La distancia de empuje de la E" },
                { "dz191.vhr.misc.condemn.accuracy", "Exactitud de la E (Revolution Only)" },
                { "dz191.vhr.misc.condemn.enextauto", "Usar la E después del autoataque" },
                { "dz191.vhr.misc.condemn.onlystuncurrent", "Usar el stun solamente contra el enemigo seleccionado" },
                { "dz191.vhr.misc.condemn.autoe", "E Automática" },
                { "dz191.vhr.misc.condemn.eks", "Mecánica de la E KS" },
                { "dz191.vhr.misc.condemn.noeaa", "No usar la E en el enemigo si esta a X autoataques" },
                { "dz191.vhr.misc.condemn.trinketbush", "Usar el trinket en el arbusto para usar la E" },
                { "dz191.vhr.misc.condemn.lowlifepeel", "Ayudar con la E cuando estas a poca vida" },
                { "dz191.vhr.misc.condemn.noeturret", "No usar la E bajo la torreta enemiga" },
                { "dz191.vhr.misc.general.antigp", "Usar la E contra las habilidades que van en ti" },
                { "dz191.vhr.misc.general.interrupt", "Interruptor" },
                {
                    "dz191.vhr.misc.general.antigpdelay", "Tiempo de usar la E contra las habilidades que van en ti (ms)"
                },
                { "dz191.vhr.misc.general.specialfocus", "Focusear a enemigos con dos marcas de la W" },
                { "dz191.vhr.misc.general.reveal", "Revelar la invisibilidad (Guardián Rosa / Lentes de visión)" },
                { "dz191.vhr.misc.general.disablemovement", "Deshabilitar el movimiento del OrbWalker" },
                { "dz191.vhr.misc.general.disableattk", "Deshabilitar el ataque del Orbwalker" },
                { "dz191.vhr.draw.spots", "Dibujar los puntos" },
                { "dz191.vhr.draw.range", "Dibujar el rango de los enemigos" },
                { "dz191.vhr.draw.qpos", "Posicionamiento de la Q Reborn (Debug)" },
                { "dz191.vhr.activator.onkey", "Clave del Activador" },
                { "dz191.vhr.activator.always", "Siempre habilitado" },
                { "dz191.vhr.activator.spells.barrier.onhealth", "En el porcentaje de la vida < %" },
                { "dz191.vhr.activator.spells.barrier.ls", "Esquivación/Predicción del daño integrada" },
                { "dz191.vhr.activator.spells.heal.onhealth", "En el porcentaje de la vida < %" },
                { "dz191.vhr.activator.spells.heal.ls", "Esquivación/Predicción del daño integrada" },
                { " ", " " }
            };

            return translations;
        }
    }
}
