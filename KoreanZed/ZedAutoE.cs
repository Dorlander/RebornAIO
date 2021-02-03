namespace KoreanZed
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;
    using System.Collections.Generic;

    class ZedAutoE
    {
        private readonly ZedMenu zedMenu;

        private readonly ZedShadows zedShadows;

        private readonly ZedSpell e;

        public ZedAutoE(ZedMenu zedMenu, ZedShadows zedShadows, ZedSpells zedSpells)
        {
            this.zedMenu = zedMenu;
            this.zedShadows = zedShadows;
            e = zedSpells.E;

            Game.OnUpdate += Game_OnUpdate;
        }

        public List<Obj_AI_Base> GetShadows2()
        {
            List<Obj_AI_Base> resultList = new List<Obj_AI_Base>();

            foreach (Obj_AI_Base objAiBase in
                ObjectManager.Get<Obj_AI_Base>().Where(obj => obj.SkinName.ToLowerInvariant().Contains("Shadow") && !obj.IsDead))
            {
                resultList.Add(objAiBase);
            }
            return resultList;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!e.IsReady() || ObjectManager.Player.Mana < e.ManaCost || !zedMenu.GetParamBool("koreanzed.miscmenu.autoe"))
            {
                return;
            }

            List<Obj_AI_Base> shadows = GetShadows2();

            if (
                HeroManager.Enemies.Any(
                    enemy =>
                    !enemy.IsDead && !enemy.IsZombie && enemy.Distance(ObjectManager.Player) < e.Range
                    && enemy.IsValidTarget())
                || GetShadows2()
                       .Any(
                           shadow =>
                           HeroManager.Enemies.Any(
                               enemy =>
                               !enemy.IsDead && !enemy.IsZombie && enemy.Distance(shadow) < e.Range
                               && enemy.IsValidTarget())))
            {
                e.Cast();
                //Console.WriteLine("auto e1");
            }
        }
    }
}