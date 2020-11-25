using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSaliceResurrected.Base;

namespace xSaliceResurrected.Utilities
{
    class WardJumper : SpellBase
    {
        //items
        public static int LastPlaced;
        public static Vector3 LastWardPos;
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        public static void JumpKs(Obj_AI_Hero target)
        {
            foreach (Obj_AI_Minion ward in ObjectManager.Get<Obj_AI_Minion>().Where(ward =>
                E.IsReady() && Q.IsReady() && ward.Name.ToLower().Contains("ward") &&
                ward.Distance(target.ServerPosition) < Q.Range && ward.Distance(Player.Position) < E.Range))
            {
                E.Cast(ward);
                return;
            }

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero =>
                E.IsReady() && Q.IsReady() && hero.Distance(target.ServerPosition) < Q.Range &&
                hero.Distance(Player.Position) < E.Range && hero.IsValidTarget(E.Range)))
            {
                E.Cast(hero);
                return;
            }

            foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion =>
                E.IsReady() && Q.IsReady() && minion.Distance(target.ServerPosition) < Q.Range &&
                minion.Distance(Player.Position) < E.Range && minion.IsValidTarget(E.Range)))
            {
                E.Cast(minion);
                return;
            }

            if (Player.Distance(target.Position) < Q.Range)
            {
                Q.Cast(target);
                return;
            }

            if (E.IsReady() && Q.IsReady())
            {
                Vector3 position = Player.ServerPosition +
                                   Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * 590;

                if (target.Distance(position) < Q.Range)
                {
                    InventorySlot invSlot = FindBestWardItem();
                    if (invSlot == null) return;

                    Player.Spellbook.CastSpell(invSlot.SpellSlot, position);
                    LastWardPos = position;
                    LastPlaced = Utils.TickCount;
                }
            }

            if (Player.Distance(target.Position) < Q.Range)
            {
                Q.Cast(target);
            }
        }

        public static void WardJump()
        {
            //wardWalk(Game.CursorPos);

            foreach (Obj_AI_Minion ward in ObjectManager.Get<Obj_AI_Minion>().Where(ward =>
                ward.Name.ToLower().Contains("ward") && ward.Distance(Game.CursorPos) < 250))
            {
                if (E.IsReady())
                {
                    E.Cast(ward);
                    return;
                }
            }

            foreach (
                Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.Distance(Game.CursorPos) < 250 && !hero.IsDead))
            {
                if (E.IsReady())
                {
                    E.Cast(hero);
                    return;
                }
            }

            foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion =>
                minion.Distance(Game.CursorPos) < 250))
            {
                if (E.IsReady())
                {
                    E.Cast(minion);
                    return;
                }
            }

            if (Utils.TickCount <= LastPlaced + 3000 || !E.IsReady()) return;

            Vector3 cursorPos = Game.CursorPos;
            Vector3 myPos = Player.ServerPosition;

            Vector3 delta = cursorPos - myPos;
            delta.Normalize();

            Vector3 wardPosition = myPos + delta * (600 - 5);

            InventorySlot invSlot = FindBestWardItem();
            if (invSlot == null) return;

            Items.UseItem((int)invSlot.Id, wardPosition);
            LastWardPos = wardPosition;
            LastPlaced = Utils.TickCount;
        }

        private static InventorySlot FindBestWardItem()
        {
            InventorySlot slot = Items.GetWardSlot();
            if (slot == default(InventorySlot)) return null;
            return slot;
        }
    }
}
