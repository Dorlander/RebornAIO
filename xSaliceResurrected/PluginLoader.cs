using System;
using System.IO;
using System.Net;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using xSaliceResurrected.ADC;
using xSaliceResurrected.Mid;
using xSaliceResurrected.Support;
using xSaliceResurrected.Top;

namespace xSaliceResurrected
{
    public class PluginLoader
    {
        private static bool _loaded;

        public PluginLoader()
        {
            if (!_loaded)
            {
                var webRequest = WebRequest.Create(@"https://github.com/Dorlander/RebornAIO/tree/basic/xSaliceResurrected/version.txt");

                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                    if (content != null)
                        using (var reader = new StreamReader(content))
                        {
                            var strContent = reader.ReadToEnd();

                            Notifications.AddNotification("Latest Version: " + strContent, 10000);
                            Notifications.AddNotification("Version Loaded: " + Assembly.GetExecutingAssembly().GetName().Version, 10000);
                            if (strContent != Assembly.GetExecutingAssembly().GetName().Version.ToString())
                                Notifications.AddNotification("Please Update Assembly!!!");

                        }

                switch (ObjectManager.Player.ChampionName.ToLower())
                {
                    case "ahri":
                        new Ahri();
                        _loaded = true;
                        break;
                    case "akali":
                        new Akali();
                        _loaded = true;
                        break;
                    case "anivia":
                        new Anivia();
                        break;
                    case "cassiopeia":
                        new Cassiopeia();
                        _loaded = true;
                        break;
                    case "ashe":
                        _loaded = true;
                        new Ashe();
                        break;
                    case "azir":
                        new Azir();
                        _loaded = true;;
                        break;
                    case "chogath":
                        new Chogath();
                        _loaded = true;
                        break;
                    case "corki":
                        new Corki();
                        _loaded = true;
                        break;
                    case "ekko":
                        new Ekko();
                        _loaded = true;
                        break;
                    case "ezreal":
                        new Ezreal();
                        _loaded = true;
                        break;
                    case "fiora":
                        Game.PrintChat("xSalice Religion AIO: Lilith sux but so does this fiora so use trees");
                        _loaded = true;
                        break;
                    case "irelia":
                        new Irelia();
                        _loaded = true;
                        break;
                    case "jinx":
                        new Jinx();
                        _loaded = true;
                        break;
                    case "karthus":
                        new Karthus();
                        _loaded = true;
                        break;
                    case "katarina":
                        new Katarina();
                        _loaded = true;
                        break;
                    case "kogmaw":
                        new KogMaw();
                        _loaded = true;
                        break;
                    case "lissandra":
                        new Lissandra();
                        _loaded = true;
                        break;
                    case "lucian":
                        new Lucian();
                        _loaded = true;
                        break;
                    case "jayce":
                        new Jayce();
                        _loaded = true;
                        break;
                    case "orianna":
                        new Orianna();
                        _loaded = true;
                        break;
                    case "rumble":
                        new Rumble();
                        _loaded = true;
                        break;
                    case "syndra":
                        new Syndra();
                        _loaded = true;
                        break;
                    case "vayne":
                        new Vayne();
                        _loaded = true;
                        break;
                    case "viktor":
                        new Viktor();
                        _loaded = true;
                        break;
                    case "vladimir":
                        new Vladimir();
                        _loaded = true;
                        break;
                    case "urgot":
                        new Urgot();
                        _loaded = true;
                        break;
                    case "zyra":
                        new Zyra();
                        _loaded = true;
                        break;
                    /*
                    case "anivia":
                        new Anivia();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "annie":
                        new Annie();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "blitzcrank":
                        new Blitzcrank();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "fizz":
                        new Fizz();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "veigar":
                        new Veigar();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "velkoz":
                        new Velkoz();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "yasuo":
                        new Yasuo();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "zed":
                        new Zed();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                     */
                    default:
                        Notifications.AddNotification(ObjectManager.Player.ChampionName + " not supported!!", 10000);
                        break;
                }
            }
        }
    }
}