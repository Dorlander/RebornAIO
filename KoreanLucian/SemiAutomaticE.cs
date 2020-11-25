namespace KoreanLucian
{
    using System;

    using KoreanCommon;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    internal class SemiAutomaticE
    {
        private readonly Lucian lucian;

        private readonly Render.Text Text = new Render.Text(
            0,
            0,
            "Tap E again to DASH\nTap CTRL to CANCEL",
            23,
            new ColorBGRA(255, 255, 255, 200));

        private bool _holding;

        private CommonSpell E;

        public SemiAutomaticE(Lucian lucian)
        {
            this.lucian = lucian;
            E = lucian.Spells.E;

            if (KoreanUtils.GetParamBool(lucian.MainMenu, "dashmode"))
            {
                Game.OnWndProc += Game_OnWndProc;
            }
        }

        public bool Holding
        {
            get
            {
                return _holding;
            }
        }

        public bool Cast(Obj_AI_Base target)
        {
            if (target.IsDead)
            {
                return false;
            }

            Process(false);
            return E.Cast(Game.CursorPos);
        }

        private void Process(bool b)
        {
            if (b)
            {
                _holding = true;
                LeagueSharp.Drawing.OnDraw += Drawing_OnDraw;
                Spellbook.OnCastSpell += HoldE;
            }
            else
            {
                _holding = false;
                LeagueSharp.Drawing.OnDraw -= Drawing_OnDraw;
                Spellbook.OnCastSpell -= HoldE;
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (KoreanUtils.GetParamBool(lucian.MainMenu, "drawingetext"))
            {
                Text.X = (int)lucian.Player.HPBarPosition.X + 5;
                Text.Y = (int)lucian.Player.HPBarPosition.Y + 180;
                Text.OnEndScene();
            }
        }

        public void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != (uint)WindowsMessages.WM_KEYDOWN || E.Instance.Level == 0)
            {
                return;
            }

            if (args.WParam == 69 && !_holding)
            {
                Process(true);
            }
            else if (args.WParam == 69 && _holding || (args.WParam == 17))
            {
                Process(false);
            }
        }

        private void HoldE(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot != E.Slot)
            {
                return;
            }

            if (_holding)
            {
                args.Process = false;
            }
        }
    }
}