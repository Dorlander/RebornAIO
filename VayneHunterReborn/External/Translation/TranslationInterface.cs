using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Utility;

namespace VayneHunter_Reborn.External.Translation
{
    class TranslationInterface
    {
        public static void OnLoad(Menu RootMenu)
        {
            var TranslationsMenu = new Menu("[VHR] Translations", "dz191.vhr.translations");
            {
                var LanguageList = Variables.languageList.Select(language => language.GetName()).ToArray();
                TranslationsMenu.AddItem(
                    new MenuItem("dz191.vhr.translations.language", "Language: ").SetValue(
                        new StringList(LanguageList, 0)));
                TranslationsMenu.AddItem(
                    new MenuItem("dz191.vhr.translations.select", "Translate!").SetValue(false)).ValueChanged +=
                    (sender, args) =>
                    {
                        if (args.GetNewValue<bool>())
                        {
                            ChangeLanguage(Variables.Menu.Item("dz191.vhr.translations.language").GetValue<StringList>().SelectedValue);
                            LeagueSharp.Common.Utility.DelayAction.Add(Game.Ping, () =>
                            { Variables.Menu.Item("dz191.vhr.translations.select").SetValue(false); });
                        } 
                    };
                RootMenu.AddSubMenu(TranslationsMenu);
            }
        }

        private static void ChangeLanguage(string Language)
        {
            if (String.IsNullOrEmpty(Language))
            {
                return;
            }
            try
            {
                var foundLanguage =
                    Variables.languageList.FirstOrDefault(m => m.GetName().ToLower() == Language.ToLower());
                if (foundLanguage != null)
                {
                    foreach (var l in foundLanguage.GetTranslations().Where(l => !string.IsNullOrWhiteSpace(l.Key) && !string.IsNullOrWhiteSpace(l.Value)))
                    {
                        var foundItem = Variables.Menu.Item(l.Key);
                        if (foundItem != null)
                        {
                            foundItem.DisplayName = l.Value;
                        }
                    }

                    Game.PrintChat(string.Format("<font color='#FF0000'><b>[VHR - Rewrite!]</b></font> Loaded language: <font color='#FF0000'><b>{0}</b></font> successfully!", foundLanguage.GetName()));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}",e);
            }
        }
    }
}
