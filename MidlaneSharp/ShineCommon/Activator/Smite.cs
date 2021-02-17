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
    public class Smite : Summoner
    {
        private readonly int[] Purple = { 3713, 3726, 3725, 3726, 3723 };
        private readonly int[] Grey = { 3711, 3722, 3721, 3720, 3719 };
        private readonly int[] Red = { 3715, 3718, 3717, 3716, 3714 };
        private readonly int[] Blue = { 3706, 3710, 3709, 3708, 3707 };

        private string[] JungleMinions;
        private TargetSelector.DamageType DamageType;
        private bool CanCastOnEnemy;

        public Smite(TargetSelector.DamageType dmgtype, Menu activator)
        {
            DamageType = dmgtype;
            SetSlot();
            if (Slot != SpellSlot.Unknown)
            {
                Range = 700f;
                CanCastOnEnemy = Blue.Any(i => Items.HasItem(i)) || Red.Any(i => Items.HasItem(i));
                #region jungle minions
                if (LeagueSharp.Common.Utility.Map.GetMap().Type.Equals(LeagueSharp.Common.Utility.Map.MapType.TwistedTreeline))
                {
                    JungleMinions = new string[] { "TT_Spiderboss", "TT_NWraith", "TT_NGolem", "TT_NWolf" };
                }
                else
                {
                    JungleMinions = new string[]
                    {
                        "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Red", "SRU_Krug", "SRU_Dragon",
                        "SRU_Baron", "Sru_Crab"
                    };
                }
                #endregion

                summoners = new Menu("Smite", "summonersmite");
                summoners.AddItem(new MenuItem("SMITEENEMY", "Auto Smite Enemy")).SetValue(true)
                            .ValueChanged += (s, ar) =>
                            {
                                bool newVal = ar.GetNewValue<bool>();
                                ((MenuItem)s).Permashow(ar.GetNewValue<bool>(), newVal ? "Auto Smite Enemy" : null);
                            };
                summoners.AddItem(new MenuItem("SMITEJUNGLE", "Auto Smite Monster")).SetValue(true)
                            .ValueChanged += (s, ar) =>
                            {
                                bool newVal = ar.GetNewValue<bool>();
                                ((MenuItem)s).Permashow(ar.GetNewValue<bool>(), newVal ? "Auto Smite Monster" : null);
                            };
                summoners.AddItem(new MenuItem("SMITEENABLE", "Enabled").SetValue<KeyBind>(new KeyBind(32, KeyBindType.Press)));

                summoners.Item("SMITEENEMY").Permashow(true, "Auto Smite Enemy");
                summoners.Item("SMITEJUNGLE").Permashow(true, "Auto Smite Monster");

                activator.AddSubMenu(summoners);
            }
        }

        public override void SetSlot()
        {
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => string.Equals(spell.Name, this.Name, StringComparison.CurrentCultureIgnoreCase)))
                Slot = spell.Slot;
        }

        public override int GetDamage()
        {
            int level = ObjectManager.Player.Level;
            int index = ObjectManager.Player.Level / 5;
            float[] dmgs = { 370 + 20 * level, 330 + 30 * level, 240 + 40 * level, 100 + 50 * level };
            return (int) dmgs[index];
        }

        public bool IsLargeMonster(string name)
        {
            return JungleMinions.Contains(name);
        }

        public override void Game_OnUpdate(EventArgs args)
        {
            if (Slot != SpellSlot.Unknown)
            {
                if (summoners.Item("SMITEJUNGLE").GetValue<bool>())
                {
                    var minions = MinionManager.GetMinions(this.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.Health);
                    foreach (var minion in minions)
                    {
                        if (IsLargeMonster(minion.Name) && minion.Health <= GetDamage())
                        {
                            Cast(minion);
                            break;
                        }
                    }
                }

                if (CanCastOnEnemy && summoners.Item("SMITEENABLE").GetValue<KeyBind>().Active && summoners.Item("SMITEENEMY").GetValue<bool>() && IsReady())
                {
                    var t = TargetSelector.GetTarget(this.Range, DamageType);
                    if (t != null && t.IsValidTarget())
                        Cast(t);

                }
            }
        }

        public string Name
        {
            get
            {
                if (Blue.Any(i => Items.HasItem(i)))
                    return "s5_summonersmiteplayerganker";

                if (Red.Any(i => Items.HasItem(i)))
                    return "s5_summonersmiteduel";

                if (Grey.Any(i => Items.HasItem(i)))
                    return "s5_summonersmitequick";

                if (Purple.Any(i => Items.HasItem(i)))
                    return "itemsmiteaoe";

                return "summonersmite";
            }
        }
    }
}
