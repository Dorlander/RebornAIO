/*
 * Warning: This file has been generated automatically.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using xSaliceReligionAIO.Champions;

namespace xSaliceReligionAIO
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
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "akali":
                        new Akali();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
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
                    case "azir":
                        new Azir();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "blitzcrank":
                        new Blitzcrank();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "corki":
                        new Corki();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "ezreal":
                        new Ezreal();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "fiora":
                        new Fiora();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "fizz":
                        new Fizz();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "irelia":
                        new Irelia();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "jayce":
                        new Jayce();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "karthus":
                        new Karthus();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "katarina":
                        new Katarina();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "lissandra":
                        new Lissandra();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "lucian":
                        new Lucian();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "orianna":
                        new Orianna();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "rumble":
                        new Rumble();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    case "syndra":
                        new Syndra();
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
                    case "vladimir":
                        new Vladimir();
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
                    case "zyra":
                        new Zyra();
                        loaded = true;
                        Game.PrintChat("<font color = \"#FFB6C1\">xSalice's " + ObjectManager.Player.ChampionName + " Loaded!</font>");
                        break;
                    default:
                        Game.PrintChat("xSalice's Religion => {0} Not Supported!", ObjectManager.Player.ChampionName);
                        break;
                }
            }
        }
    }
}
