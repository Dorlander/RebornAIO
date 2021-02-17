using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace BadaoKingdom.BadaoChampion.BadaoYasuo
{
    public static class BadaoYasuoVariables
    {
        // Combo
        public static MenuItem TargetMode;
        public static MenuItem TargetModeKey;
        public static MenuItem ComboStackQ;
        public static MenuItem ComboRHits;
        public static MenuItem ComboIgnite;
        public static MenuItem DiveTurretKey;

        // Harass
        public static MenuItem HarassQ;
        public static MenuItem HarassQ3;

        // LaneClear
        public static MenuItem LaneQ;
        public static MenuItem LaneE;

        //Jung
        public static MenuItem JungQ;
        public static MenuItem JungE;

        //Lasthit
        public static MenuItem LastHitQ;
        public static MenuItem LastHitE;

        // Auto
        public static MenuItem AutoQ;
        public static MenuItem AlsoAutoQ3;
        public static MenuItem AutoStackQ3;
        public static MenuItem AutoKSQ;
        public static MenuItem AutoKSE;

        //Flee
        public static MenuItem FleeKey;

        //Assassinate
        public static MenuItem AssassinateKey;

        // Draw
        public static MenuItem DrawDiveTurret;
        public static MenuItem DrawAutoQ;
        public static MenuItem DrawAssasinate;
        public static MenuItem DrawR;
        public static MenuItem DrawE;
        public static MenuItem DrawComboMode;
    }
}
