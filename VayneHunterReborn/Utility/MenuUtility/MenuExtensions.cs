using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace VayneHunter_Reborn.Utility.MenuUtility
{
    static class MenuExtensions
    {
        public static MenuItem AddBool(this Menu menu, string name, string displayName, bool defaultValue = false)
        {
            return menu.AddItem(new MenuItem(name, displayName).SetValue(defaultValue));
        }

        public static MenuItem AddSlider(this Menu menu, string name, string displayName, Tuple<int, int, int> values)
        {
            return menu.AddItem(new MenuItem(name, displayName).SetValue(new Slider(values.Item1, values.Item2, values.Item3)));
        }

        public static MenuItem AddKeybind(this Menu menu, string name, string displayName, Tuple<uint, KeyBindType> value)
        {
            return menu.AddItem(new MenuItem(name, displayName).SetValue(new KeyBind(value.Item1, value.Item2)));
        }

        public static MenuItem AddText(this Menu menu, string name, string displayName)
        {
            return menu.AddItem(new MenuItem(name, displayName));
        }

        public static MenuItem AddStringList(this Menu menu, string name, string displayName, string[] value, int index = 0)
        {
            return menu.AddItem(new MenuItem(name, displayName).SetValue(new StringList(value, index)));
        }

        public static MenuItem AddSkill(this Menu menu, Enumerations.Skills skill, Orbwalking.OrbwalkingMode mode, bool defValue = true)
        {
            var name = string.Format("dz191.vhr.{0}.use{1}", mode.ToString().ToLower(), skill.ToString().ToLower());
            var displayName = string.Format("Use {0}", skill);
            return menu.AddItem(new MenuItem(name, displayName).SetValue(defValue));
        }

        public static MenuItem AddManaLimiter(this Menu menu, Enumerations.Skills skill, Orbwalking.OrbwalkingMode mode, int defMana = 0, bool displayMode = false)
        {
            var name = string.Format("dz191.vhr.{0}.mm.{1}.mana", mode.ToString().ToLower(), skill.ToString().ToLower());
            var displayName = displayMode ? string.Format("{0} Mana {1}", skill, mode) : string.Format("{0} Mana", skill);
            return menu.AddItem(new MenuItem(name, displayName).SetValue(new Slider(defMana)));
        }

        public static bool IsEnabledAndReady(this Spell spell, Orbwalking.OrbwalkingMode mode, bool checkMana = true)
        {
            var name = string.Format("dz191.vhr.{0}.use{1}", mode.ToString().ToLower(), spell.Slot.ToString().ToLower());
            var mana = string.Format("dz191.vhr.{0}.mm.{1}.mana", mode.ToString().ToLower(), spell.Slot.ToString().ToLower());
            
            if(Variables.Menu.Item(name) != null && Variables.Menu.Item(mana) != null)
            {
                return spell.IsReady()
                    && GetItemValue<bool>(name)
                    && (!checkMana || (ObjectManager.Player.Mana >= GetItemValue<Slider>(mana).Value));
            }
            return false;
        }

        public static T GetItemValue<T>(string item)
        {
            if (Variables.Menu.Item(item) == null)
            {
                return default(T);
            }

            return Variables.Menu.Item(item).GetValue<T>();
        }
    }
}
