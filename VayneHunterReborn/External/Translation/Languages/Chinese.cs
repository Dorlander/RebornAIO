using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VayneHunter_Reborn.External.Translation.Languages
{
    /*
     * Chinese Translation by Southpaw and MasterLag
     * 
     * https://www.joduska.me/forum/user/2503-southpaw/
     * https://www.joduska.me/forum/user/3600-masterlag/
     */

    class Chinese : IVHRLanguage
    {
        public string GetName()
        {
            return "Chinese";
        }

        public Dictionary<string, string> GetTranslations()
        {
            var translations = new Dictionary<string, string>() {
                {"dz191.vhr.combo.r.minenemies", "對手小於多少人的時候使用R"},
                {"dz191.vhr.combo.q.2wstacks", "如果只有兩個目標 疊W"},
                {"dz191.vhr.mixed.q.2wstacks", "如果只有兩個目標 疊W"},
                {"dz191.vhr.mixed.ethird", "使用E 觸發第三個印記"},
                {"dz191.vhr.farm.condemnjungle", "使用对丛林怪物E"},
                {"dz191.vhr.farm.qjungle", "使用对丛林怪物Q"},
                {"dz191.vhr.misc.condemn.qlogic", "Q的判定"},
                {"dz191.vhr.mixed.mirinQ", "使用Q如果能推墙 (Mirin Mode)"},
                {"dz191.vhr.misc.tumble.smartq", "嘗試使用QE接濟"},
                {"dz191.vhr.misc.tumble.noaastealthex", "大招潛行時不要連A"},
                {"dz191.vhr.misc.tumble.noqenemies", "不要使用Q貼近對手"},
                {"dz191.vhr.misc.tumble.dynamicqsafety", "使用Q安全距離"},
                {"dz191.vhr.misc.tumble.qspam", "忽略檢查Q"},
                {"dz191.vhr.misc.tumble.qinrange", "使用Q偷頭"},
                {"dz191.vhr.misc.tumble.walltumble.warning", "確認按住並且翻牆"},
                {"dz191.vhr.misc.tumble.walltumble.warning.2", "It will walk to the nearest Tumble spot and Tumble"},
                {"dz191.vhr.misc.tumble.walltumble", "翻牆 (WallTumble)"},
                {"dz191.vhr.misc.condemn.condemnmethod", "模式選擇"},
                {"dz191.vhr.misc.condemn.pushdistance", "推牆距離"},
                {"dz191.vhr.misc.condemn.accuracy", "準確度 (Revolution Only)"},
                {"dz191.vhr.misc.condemn.enextauto", "下個目標自動推牆"},
                {"dz191.vhr.misc.condemn.onlystuncurrent", "只有击晕当前目标"},
                {"dz191.vhr.misc.condemn.autoe", "自动E"},
                {"dz191.vhr.misc.condemn.eks", "E杀偷"},
                {"dz191.vhr.misc.condemn.noeaa", "Don't E if Target can be killed in X AA"},
                {"dz191.vhr.misc.condemn.trinketbush", "饰品布什E上"},
                {"dz191.vhr.misc.condemn.lowlifepeel", "皮尔有E时，低温养生"},
                {"dz191.vhr.misc.condemn.noeturret", "没有E在敌方炮塔"},
                {"dz191.vhr.misc.general.antigp", "反Gapcloser"},
                {"dz191.vhr.misc.general.interrupt", "阻碍者"},
                {"dz191.vhr.misc.general.antigpdelay", "反Gapcloser延迟 (ms)"},
                {"dz191.vhr.misc.general.specialfocus", "注重目标与2 W"},
                {"dz191.vhr.misc.general.reveal", "隐形显示 (Pink Ward / Lens)"},
                {"dz191.vhr.misc.general.disablemovement", "禁用Orbwalker运动"},
                {"dz191.vhr.misc.general.disableattk", "禁用Orbwalker攻击"},
                {"dz191.vhr.draw.spots", "绘制景点"},
                {"dz191.vhr.draw.range", "绘制敌人范围"},
                {"dz191.vhr.draw.qpos", "Reborn Q Position (Debug)"},
                {"dz191.vhr.activator.onkey", "激活热键绑定"},
                {"dz191.vhr.activator.always", "始终启用"},
                {"dz191.vhr.activator.spells.barrier.onhealth", "On health < %"},
                {"dz191.vhr.activator.spells.barrier.ls", "Evade/Damage Prediction integration"},
                {"dz191.vhr.activator.spells.heal.onhealth", "On health < %"},
                {"dz191.vhr.activator.spells.heal.ls", "Evade/Damage Prediction integration"},
            };

            return translations;
        }

    }
}
