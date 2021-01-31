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
        public static bool loaded;

        public PluginLoader()
        {
            if (!loaded)
            {  

            switch (ObjectManager.Player.ChampionName.ToLower())
                {
                    case "ahri":
                        new Ahri();
                        loaded = true;
                        break;
                    case "akali":
                        new Akali();
                        loaded = true;
                        break;
                    case "anivia":
                        new Anivia();
                        break;
                    case "cassiopeia":
                        new Cassiopeia();
                        loaded = true;
                        break;
                    case "ashe":
                        loaded = true;
                        new Ashe();
                        break;
                    case "azir":
                        new Azir();
                        loaded = true;;
                        break;
                    case "chogath":
                        new Chogath();
                        loaded = true;
                        break;
                    case "corki":
                        new Corki();
                        loaded = true;
                        break;
                    case "ekko":
                        new Ekko();
                        loaded = true;
                        break;
                    case "ezreal":
                        new Ezreal();
                        loaded = true;
                        break;
                    case "fiora":
                        Game.PrintChat("xSalice Religion AIO: Lilith sux but so does this fiora so use trees");
                        loaded = true;
                        break;
                    case "irelia":
                        new Irelia();
                        loaded = true;
                        break;
                    case "jinx":
                        new Jinx();
                        loaded = true;
                        break;
                    case "karthus":
                        new Karthus();
                        loaded = true;
                        break;
                    case "katarina":
                        new Katarina();
                        loaded = true;
                        break;
                    case "kogmaw":
                        new KogMaw();
                        loaded = true;
                        break;
                    case "lissandra":
                        new Lissandra();
                        loaded = true;
                        break;
                    case "lucian":
                        new Lucian();
                        loaded = true;
                        break;
                    case "jayce":
                        new Jayce();
                        loaded = true;
                        break;
                    case "orianna":
                        new Orianna();
                        loaded = true;
                        break;
                    case "rumble":
                        new Rumble();
                        loaded = true;
                        break;
                    case "syndra":
                        new Syndra();
                        loaded = true;
                        break;
                    case "vayne":
                        new Vayne();
                        loaded = true;
                        break;
                    case "viktor":
                        new Viktor();
                        loaded = true;
                        break;
                    case "vladimir":
                        new Vladimir();
                        loaded = true;
                        break;
                    case "urgot":
                        new Urgot();
                        loaded = true;
                        break;
                    case "zyra":
                        new Zyra();
                        loaded = true;
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