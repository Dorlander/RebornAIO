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
    public static class BadaoYasuo
    {
        public static void BadaoActivate()
        {
            BadaoYasuoConfig.BadaoActivate();
            BadaoYasuoHelper.BadaoActivate();
            BadaoYasuoDrawing.BadaoActivate();
            BadaoYasuoCombo.BadaoActivate();
            BadaoYasuoAssasinate.BadaoActivate();
            BadaoYasuoHarass.BadaoActivate();
            BadaoYasuoClear.BadaoActivate();
            BadaoYasuoLastHit.BadaoActivate();
            BadaoYasuoAuto.BadaoActivate();
            BadaoYasuoFlee.BadaoActivate();
            BadaoYasuoSwitcher.BadaoActivate();
            Evade.Program.Init();
        }
    }
}
