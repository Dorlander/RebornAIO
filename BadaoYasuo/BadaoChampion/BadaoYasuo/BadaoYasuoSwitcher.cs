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
    public static class BadaoYasuoSwitcher
    {
        public static int LastSwitch;
        public static void BadaoActivate()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            // target mode 
            if (BadaoYasuoVariables.TargetModeKey.GetValue<KeyBind>().Active)
                ModeSwitch(ref BadaoYasuoVariables.TargetMode,ref  BadaoYasuoVariables.TargetModeKey);
        }

        private static void ModeSwitch(ref MenuItem Mode,ref MenuItem Key)
        {
            var Index = Mode.GetValue<StringList>().SelectedIndex;
            var Slist = Mode.GetValue<StringList>().SList;
            var Count = Slist.Count();
            var lasttime = Environment.TickCount - LastSwitch;
            if (!Key.GetValue<KeyBind>().Active ||
                lasttime <= Game.Ping)
            {
                return;
            }
            LastSwitch = Environment.TickCount + 300;
            int NewIndex = Index == Count - 1 ? 0 : Index + 1;
            Mode.SetValue(new StringList(Slist, NewIndex));
        }
    }
}
