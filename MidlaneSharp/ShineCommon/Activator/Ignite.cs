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
    public class Ignite : Summoner
    {
        private TargetSelector.DamageType DamageType;

        public Ignite(TargetSelector.DamageType dmgtype, Menu activator)
        {
            DamageType = dmgtype;
            SetSlot();
            if (Slot != SpellSlot.Unknown)
            {
                Range = 550f;
                summoners = new Menu("Ignite", "summonerignite");
                summoners.AddItem(new MenuItem("IGNITEENEMY", "Use Ignite")).SetValue(true);

                summoners.AddItem(new MenuItem("IGNITEKS", "Use Ignite for KS")).SetValue(true)
                            .ValueChanged += (s, ar) =>
                            {
                                bool newVal = ar.GetNewValue<bool>();
                                ((MenuItem)s).Permashow(ar.GetNewValue<bool>(), newVal ? "Ignite Kill Steal" : null);
                            };

                summoners.AddItem(new MenuItem("IGNITEENABLE", "Enabled").SetValue<KeyBind>(new KeyBind(32, KeyBindType.Press)));
                summoners.Item("IGNITEKS").Permashow(true, "Ignite Kill Steal");
                activator.AddSubMenu(summoners);
            }
        }

        public override void SetSlot()
        {
            Slot = ObjectManager.Player.GetSpellSlot("SummonerDot");
        }

        public override int GetDamage()
        {
            return 50 + (20 * ObjectManager.Player.Level);
        }

        public override void Game_OnUpdate(EventArgs args)
        {
            if (Slot != SpellSlot.Unknown)
            {
                if (summoners.Item("IGNITEKS").GetValue<bool>() && this.IsReady())
                {
                    var t = HeroManager.Enemies.FirstOrDefault(p => !p.IsDead && p.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 550f && HealthPrediction.GetHealthPrediction(p, 250) < this.GetDamage());
                    if (t != null)
                        this.Cast(t);
                }

                if (summoners.Item("IGNITEENABLE").GetValue<KeyBind>().Active && summoners.Item("IGNITEENEMY").GetValue<bool>() && this.IsReady())
                {
                    var t = TargetSelector.GetTarget(this.Range, DamageType);
                    if (t != null && t.IsValidTarget() && t.HealthPercent < 40)
                        Cast(t);
                }
            }
        }
    }
}
