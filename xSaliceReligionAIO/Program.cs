using System;
using LeagueSharp.Common;

namespace xSaliceReligionAIO
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += LoadReligion;
        }

        static void LoadReligion(EventArgs args)
        {
            Champion champs = new Champion(true);
        }
    }
}
