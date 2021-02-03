namespace KoreanZed
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    class ZedSpellDodge
    {
        private readonly ZedSpell r;

        private readonly ZedMenu zedMenu;

        private readonly Obj_AI_Hero player;

        public ZedSpellDodge(ZedSpells spells, ZedMenu mainMenu)
        {
            r = spells.R;
            zedMenu = mainMenu;
            player = ObjectManager.Player;

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null || args == null || !r.IsReady() || r.Instance.ToggleState != 0
                || !player.GetEnemiesInRange(r.Range).Any() || args.Slot != SpellSlot.R || !sender.IsChampion()
                || !sender.IsEnemy || !zedMenu.GetParamBool("koreanzed.miscmenu.rdodge.user")
                || !zedMenu.CheckMenuItem("koreanzed.miscmenu.rdodge." + args.SData.Name.ToLowerInvariant()))
            {
                return;
            }

            if (((args.Target != null && args.Target.IsMe)
                    || player.Distance(args.End) < Math.Max(args.SData.CastRadius, args.SData.LineWidth))
                && zedMenu.GetParamBool("koreanzed.miscmenu.rdodge." + args.SData.Name.ToLowerInvariant()))
            {
                int delay = (int) Math.Truncate((double)(player.Distance(sender) / args.SData.MissileSpeed)) - 1;
                Utility.DelayAction.Add(delay, () => { r.Cast(TargetSelector.GetTarget(r.Range, r.DamageType)); });
                //Console.WriteLine("r dodge");
            }
        }
    }
}
