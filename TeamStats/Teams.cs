using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamStats
{
    internal class Teams
    {
        public List<Obj_AI_Hero> myTeam = new List<Obj_AI_Hero>();
        public List<Obj_AI_Hero> EnemyTeam = new List<Obj_AI_Hero>();
        public int myTeamNum;
        public int enemyTeamNum;
        public int myTeamDmg;
        public int enemyTeamDmg;
        public int myTeamHP { get; set; }
        public int enemyTeamHP { get; set; }

        public Teams()
        {
            SetNums();
        }
        public void SetNums()
        {
            foreach (var player in ObjectManager.Get<Obj_AI_Hero>().Where(i => !i.IsDead && !i.IsMinion && Program.player.Distance(i) < Program.range))
            {
                //list
                if (player.IsEnemy) EnemyTeam.Add(player);
                if (player.IsAlly || player.IsMe) myTeam.Add(player);
                //num
                if (player.IsEnemy) enemyTeamNum++;
                if (player.IsAlly || player.IsMe) myTeamNum++;
            }
            myTeam = myTeam.OrderByDescending(i => i.CharacterState).ToList();
            EnemyTeam = EnemyTeam.OrderByDescending(i => i.CharacterState).ToList();
            var e = 0;
            var eHP = EnemyTeam[0].Health;
            foreach (var enemy in EnemyTeam)
            {
                if (myTeam.Count - 1 < e)
                {
                    break;
                }
                if (eHP < 0 && myTeam.Count-1>=e+1)
                {
                    e++;
                    eHP = myTeam[e].Health + eHP;
                }
                enemyTeamDmg += (int)ComboDamage(enemy, myTeam[e]);
                eHP -= (int)ComboDamage(enemy, myTeam[e]);
                enemyTeamHP += (int)(enemy.Health);

            }
            var t = 0;
            var tHP = EnemyTeam[0].Health;
            foreach (var teammate in myTeam)
            {
                if (EnemyTeam.Count-1<t)
                {
                    break;
                }
                if (tHP < 0 && EnemyTeam.Count-1>=t+1)
                {                    
                    t++; 
                    tHP = EnemyTeam[t].Health + tHP;

                }
                myTeamDmg += (int)ComboDamage(teammate, EnemyTeam[t]);
                tHP -= (int)ComboDamage(teammate, EnemyTeam[t]);
                myTeamHP += (int)(teammate.Health);
            }
        }
        private static float ComboDamage(Obj_AI_Hero src, Obj_AI_Hero dsc)
        {
            if (!src.IsValid || !dsc.IsValid) return 0f;
            float basicDmg = 0;
            int attacks = (int)Math.Floor(src.AttackSpeedMod*5);
            if (src.Crit>0)
            {
                    
                basicDmg += (float)src.GetAutoAttackDamage(dsc) * attacks * (1 + src.Crit);
            }
            else
            {

                basicDmg += (float)src.GetAutoAttackDamage(dsc) * attacks;
            }
            float damage = basicDmg;
            var spells = src.Spellbook.Spells;
            
            foreach (var spell in spells)
            {
                var t = spell.CooldownExpires - Game.Time;
                if (spell.Level > 0 && t < 0.5 && Damage.GetSpellDamage(src, dsc, spell.Slot) > 0)
                {
                    switch (src.SkinName)
                    {
                                case "Ahri":
                                    if (spell.Slot == SpellSlot.Q)
                                    {
                                        damage += (float)(Damage.GetSpellDamage(src, dsc, spell.Slot));
                                        damage += (float)(Damage.GetSpellDamage(src, dsc, spell.Slot,1));
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "Akali":
                                    if (spell.Slot == SpellSlot.R)
                                    {
                                        damage += (float) (Damage.GetSpellDamage(src, dsc, spell.Slot)*spell.Ammo);
                                    }
                                    else if (spell.Slot == SpellSlot.Q)
                                    {
                                        damage += (float)(Damage.GetSpellDamage(src, dsc, spell.Slot));
                                        damage += (float)(Damage.GetSpellDamage(src, dsc, spell.Slot, 1));
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "Amumu":
                                    if (spell.Slot == SpellSlot.W)
                                    {
                                        damage += (float)(Damage.GetSpellDamage(src, dsc, spell.Slot) * 5);
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "Cassiopeia":
                                    if (spell.Slot == SpellSlot.Q || spell.Slot == SpellSlot.E || spell.Slot == SpellSlot.W)
                                    {
                                        damage += (float)(Damage.GetSpellDamage(src, dsc, spell.Slot) * 2);
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "Fiddlesticks":
                                    if (spell.Slot == SpellSlot.W || spell.Slot == SpellSlot.E)
                                    {
                                        damage += (float)(Damage.GetSpellDamage(src, dsc, spell.Slot)*5); 
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "Fiora":
                                    if (spell.Slot == SpellSlot.R)
                                    {
                                        damage += (float)FioraPassiveDamage(src, dsc, 4);
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "Garen":
                                    if (spell.Slot == SpellSlot.E)
                                    {
                                        damage += (float)GarenEDamage(src, dsc);
                                    }
                                    else if (spell.Slot == SpellSlot.R)
                                    {
                                        damage += (float)GarenR(src, dsc);
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "Irelia":
                                    if (spell.Slot == SpellSlot.W)
                                    {
                                        damage += (float)(Damage.GetSpellDamage(src, dsc, spell.Slot) * attacks);
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "Karthus":
                                    if (spell.Slot == SpellSlot.Q)
                                    {
                                        damage += (float)(Damage.GetSpellDamage(src, dsc, spell.Slot) * 4);
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "KogMaw":
                                    if (spell.Slot == SpellSlot.W)
                                    {
                                        damage += (float)(Damage.GetSpellDamage(src, dsc, spell.Slot) * attacks);
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "LeeSin":
                                    if (spell.Slot == SpellSlot.Q)
                                    {
                                        damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                        damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot,1);
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "Lucian":
                                    if (spell.Slot == SpellSlot.R)
                                    {
                                        damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot)*4;
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "Nunu":
                                    if (spell.Slot != SpellSlot.R && spell.Slot != SpellSlot.Q)
                                    {
                                        damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "MasterYi":
                                    if (spell.Slot != SpellSlot.E)
                                    {
                                        damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot)*attacks;
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "MonkeyKing":
                                    if (spell.Slot != SpellSlot.R)
                                    {
                                        damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot) * 4;
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "Pantheon":
                                    if (spell.Slot == SpellSlot.E)
                                    {
                                        damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot)*3;
                                    }
                                    else if (spell.Slot == SpellSlot.R)
                                    {
                                        damage +=0;
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    
                                    break;
                                case "Rammus":
                                    if (spell.Slot == SpellSlot.R)
                                    {
                                        damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot)*6;
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "Riven":
                                    if (spell.Slot == SpellSlot.Q)
                                    {
                                        damage += RivenDamageQ(spell, src, dsc);
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "Viktor":
                                    if (spell.Slot == SpellSlot.R)
                                    {
                                        damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                        damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot,1) * 5;
                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                case "Vladimir":
                                    if (spell.Slot == SpellSlot.E)
                                    {
                                        damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot) * 2;

                                    }
                                    else damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                    break;
                                default:
                                damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot);
                                break;
                    }
                                     
                }
            }
            
            if (src.Spellbook.CanUseSpell(src.GetSpellSlot("summonerdot")) == SpellState.Ready)
            {
                damage += (float)src.GetSummonerSpellDamage(dsc, Damage.SummonerSpell.Ignite);
            }
            return damage;
        }

        private static float RivenDamageQ(SpellDataInst spell,Obj_AI_Hero src, Obj_AI_Hero dsc)
        {
            double dmg = 0;
            if (spell.IsReady())
            {
                dmg += src.CalcDamage(dsc, Damage.DamageType.Physical,
                (-10 + (spell.Level * 20) +
                (0.35 + (spell.Level * 0.05)) * (src.FlatPhysicalDamageMod + src.BaseAttackDamage))*3);
            }
            return (float)dmg;
        }
        public static double FioraPassiveDamage(Obj_AI_Hero source, Obj_AI_Base target, int passives)
        {
            return passives * (0.03f + 0.027 + 0.001f * source.Level * source.FlatPhysicalDamageMod / 100f) *
                   target.MaxHealth;
        }
        private static double GarenR(Obj_AI_Hero source, Obj_AI_Hero target)
        {
            var R = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R);
            var dmg = new double[] { 175, 350, 525 }[R.Level - 1] +
                      new[] { 28.57, 33.33, 40 }[R.Level - 1] / 100 * (target.MaxHealth - target.Health);
            if (target.HasBuff("garenpassiveenemytarget"))
            {
                return Damage.CalcDamage(source, target, Damage.DamageType.True, dmg);
            }
            else
            {
                return Damage.CalcDamage(source, target, Damage.DamageType.Magical, dmg);
            }
        }

        public static int[] spins = new int[] { 5, 6, 7, 8, 9, 10 };
        public static double[] baseEDamage = new double[] { 15, 18.8, 22.5, 26.3, 30 };
        public static double[] bonusEDamage = new double[] { 34.5, 35.3, 36, 36.8, 37.5 };
        private static double GarenEDamage(Obj_AI_Hero src, Obj_AI_Hero target)
        {
            var E = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E);
            var dmg = (baseEDamage[E.Level - 1] +
                      bonusEDamage[E.Level - 1] / 100 * src.TotalAttackDamage) * GetSpins(src);
            var bonus = target.HasBuff("garenpassiveenemytarget") ? target.MaxHealth / 100f * GetSpins(src) : 0;
            if (ObjectManager.Get<Obj_AI_Base>().Count(o => o.IsValidTarget() && o.Distance(target) < 650) == 0)
            {
                return Damage.CalcDamage(src, target, Damage.DamageType.Physical, dmg) * 1.33 + bonus;
            }
            else
            {
                return Damage.CalcDamage(src, target, Damage.DamageType.Physical, dmg) + bonus;
            }
        }

        private static double GetSpins(Obj_AI_Hero src)
        {
            if (src.Level < 4)
            {
                return 5;
            }
            if (src.Level < 7)
            {
                return 6;
            }
            if (src.Level < 10)
            {
                return 7;
            }
            if (src.Level < 13)
            {
                return 8;
            }
            if (src.Level < 16)
            {
                return 9;
            }
            if (src.Level < 18)
            {
                return 10;
            }
            return 5;
        }
    }
 }
