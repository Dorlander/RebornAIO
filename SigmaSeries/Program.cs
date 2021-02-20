using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SigmaSeries
{
    // All Credits to H3H3 for the loader.
    internal class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += a =>
            {
                try
                {
                    var type = Type.GetType("SigmaSeries.Plugins." + ObjectManager.Player.ChampionName);

                    if (type != null)
                    {
                        Activator.CreateInstance(type);
                        return;
                    }

                    Game.PrintChat(ObjectManager.Player.ChampionName + " Not Supported");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            };
        }
    }
}
