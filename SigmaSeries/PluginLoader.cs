using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SigmaSeries.Plugins;

namespace SigmaSeries
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
                    case "darius":
                        new Darius();
                        loaded = true;
                        break;
                    case "cassiopeia":
                        new Cassiopeia();
                        loaded = true;
                        break;
                    case "fizz":
                        new Fizz();
                        loaded = true;
                        break;
                    case "hecarim":
                        new Hecarim();
                        loaded = true;
                        break;
                    case "mordekaiser":
                        new Mordekaiser();
                        loaded = true;
                        break;
                    case "rengar":
                        new Rengar();
                        loaded = true;
                        break;
                    case "tryndamere":
                        new Tryndamere();
                        loaded = true;
                        break;
                    case "varus":
                        new Varus();
                        loaded = true;
                        break;
                    case "singed":
                        new Singed();
                        loaded = true;
                        break;

                }
            }
        }
    }
}
