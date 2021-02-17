using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ShineCommon.Activator
{
    public abstract class Summoner
    {
        public Menu summoners;
        public SpellSlot Slot;
        public float Range;

        public Summoner()
        {
            Slot = SpellSlot.Unknown;
            Game.OnUpdate += Game_OnUpdate;
        }

        public virtual void Game_OnUpdate(EventArgs args)
        {
            //
        }

        public virtual void SetSlot()
        {
            //
        }

        public virtual int GetDamage()
        {
            //
            return 0;
        }

        public bool IsReady()
        {
            return ObjectManager.Player.Spellbook.CanUseSpell(Slot) == SpellState.Ready;
        }

        public void Cast()
        {
            if (Slot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(Slot) == SpellState.Ready)
                ObjectManager.Player.Spellbook.CastSpell(Slot);
        }

        public void Cast(Obj_AI_Base t)
        {
            if (Slot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(Slot) == SpellState.Ready && t.Distance(ObjectManager.Player.ServerPosition) < Range)
                ObjectManager.Player.Spellbook.CastSpell(Slot, t);
        }
    }
}
