#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace QuickTeleport
{
    internal class Program
    {
        private static Menu menu;
        private static SpellDataInst teleport;
        private static Obj_AI_Hero player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            player = ObjectManager.Player;
            teleport = player.Spellbook.GetSpell(player.GetSpellSlot("SummonerTeleport"));
            if (teleport == null || teleport.Slot == SpellSlot.Unknown)
            {
                return;
            }

            menu = new Menu("QuickTeleport", "QuickTeleport", true);
            menu.AddItem(new MenuItem("Hotkey", "Hotkey").SetValue(new KeyBind(16, KeyBindType.Press)));
            menu.AddItem(new MenuItem("Turret", "QT to Turrets Only").SetValue(true));
            menu.AddToMainMenu();
            Game.OnUpdate += Game_OnGameUpdate;
            Game.PrintChat("QuickTeleport by Trees loaded.");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!CanTeleport() || !menu.Item("Hotkey").GetValue<KeyBind>().Active)
            {
                return;
            }

            Obj_AI_Base closestObject = player;
            float d = 2000;

            foreach (var obj in
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(
                        obj =>
                            obj != null && obj.IsValid && obj.IsVisible && !obj.IsDead && obj.Team == player.Team &&
                            obj.Type != player.Type && (obj is Obj_AI_Turret || !menu.Item("Turret").GetValue<bool>()) &&
                            obj.ServerPosition.Distance(Game.CursorPos) < d))
            {
                closestObject = obj;
                d = obj.ServerPosition.Distance(Game.CursorPos);
            }

            if (closestObject != player && closestObject != null)
            {
                CastTeleport(closestObject);
            }
        }

        private static bool CanTeleport()
        {
            return teleport != null && teleport.Slot != SpellSlot.Unknown && teleport.State == SpellState.Ready &&
                   player.CanCast;
        }

        private static void CastTeleport(Obj_AI_Base unit)
        {
            if (CanTeleport())
            {
                player.Spellbook.CastSpell(teleport.Slot, unit);
            }
        }
    }
}