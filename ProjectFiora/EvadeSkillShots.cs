using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using LeagueSharp.Common.Data;
using ItemData = LeagueSharp.Common.Data.ItemData;
using FioraProject.Evade;

namespace FioraProject
{
    using static Program;
    using static GetTargets;
    class EvadeSkillShots
    {
        #region Evade
        public static void Evading()
        {
            var parry = Evade.EvadeSpellDatabase.Spells.FirstOrDefault(i => i.Enable && i.IsReady && i.Slot == SpellSlot.W);
            if (parry == null)
            {
                return;
            }
            var skillshot =
                Evade.Evade.SkillshotAboutToHit(Player, 0 + Game.Ping + Program.Menu.SubMenu("Evade").SubMenu("Spells").SubMenu(parry.Name).Item("WDelay").GetValue<Slider>().Value)
                    .Where(
                        i =>
                        parry.DangerLevel <= i.DangerLevel)
                    .MaxOrDefault(i => i.DangerLevel);
            if (skillshot != null)
            {
                var target = GetTarget(W.Range);
                if (target.IsValidTarget(W.Range))
                    Player.Spellbook.CastSpell(parry.Slot, target.Position);
                else
                {
                    var hero = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(W.Range));
                    if (hero != null)
                        Player.Spellbook.CastSpell(parry.Slot, hero.Position);
                    else
                        Player.Spellbook.CastSpell(parry.Slot, Player.ServerPosition.Extend(skillshot.Start.To3D(), 100));
                }
            }
        }
        #endregion Evade

    }
}
