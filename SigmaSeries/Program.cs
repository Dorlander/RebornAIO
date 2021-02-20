using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using Version = System.Version;

namespace SigmaSeries
{
    // All Credits to H3H3 for the loader.
    internal class Program
    {
        public static Version Version;
        static void Main(string[] args)
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version;
            CustomEvents.Game.OnGameLoad += a =>
            {
                new PluginLoader();
            };
        }
    }
}
