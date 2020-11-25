using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using xSaliceResurrected.Managers;

namespace xSaliceResurrected.Base
{
    abstract class SpellBase
    {

        protected static List<Spell> SpellList
        {
            get { return SpellManager.SpellList; }
        }

        protected static Spell P
        {
            get { return SpellManager.P; }
        }

        protected static Spell Q
        {
            get { return SpellManager.Q; }
        }

        protected static Spell Q2
        {
            get { return SpellManager.Q2; }
        }

        protected static Spell QExtend
        {
            get { return SpellManager.QExtend;  }
        }
        protected static Spell W
        {
            get { return SpellManager.W; }
        }

        protected static Spell W2
        {
            get { return SpellManager.W2; }
        }

        protected static Spell E
        {
            get { return SpellManager.E; }
        }

        protected static Spell E2
        {
            get { return SpellManager.E2; }
        }

        protected static Spell R
        {
            get { return SpellManager.R; }
        }
        protected static Spell R2
        {
            get { return SpellManager.R2; }
        }

        protected static SpellDataInst QSpell
        {
            get { return SpellManager.QSpell; }
        }

        protected static SpellDataInst WSpell
        {
            get { return SpellManager.WSpell; }
        }

        protected static SpellDataInst ESpell
        {
            get { return SpellManager.ESpell; }
        }

        protected static SpellDataInst RSpell
        {
            get { return SpellManager.RSpell; }
        }
    }
}
