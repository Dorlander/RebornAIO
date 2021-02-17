using System;
using System.Linq;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SCommon;
using SCommon.PluginBase;
using SCommon.Prediction;
using SCommon.Maths;
using SCommon.Database;
using SUtility.Drawings;
using SAutoCarry.Champions.Helpers;
using SharpDX;

namespace SAutoCarry.Champions
{
    public class Riven : Champion
    {
        public bool IsDoingFastQ = false;
        public bool IsCrestcentReady
        {
            get { return (Items.HasItem(3077) && Items.CanUseItem(3077)) || (Items.HasItem(3074) && Items.CanUseItem(3074)); }
        }
        public SpellSlot SummonerFlash = ObjectManager.Player.GetSpellSlot("summonerflash");
        private Dictionary<string, StringList> ComboMethodBackup = new Dictionary<string, StringList>();

        public Riven()
            : base ("Riven", "Shy Riven")
        {
            OnUpdate += BeforeOrbwalk;
            OnDraw += BeforeDraw;
            OnCombo += Combo;
            OnHarass += Combo; //same function because harass mode is just same combo w/o flash & r (which already implemented in combo)
            OnLaneClear += LaneClear;
            OnLastHit += LastHit;

            SCommon.Orbwalking.Events.AfterAttack += Animation.AfterAttack;
           // Obj_AI_Hero.OnProcessSpellCast += Animation.OnPlay;
            Animation.OnAnimationCastable += Animation_OnAnimationCastable;
            Game.OnWndProc += Game_OnWndProc;
        }

        public override void CreateConfigMenu()
        {
            Menu combo = new Menu("Combo", "combo");
            combo.AddItem(new MenuItem("CDISABLER", "Disable R Usage").SetValue(false))
                    .ValueChanged += (s, ar) =>
                    {
                        ConfigMenu.Item("CR1MODE").Show(!ar.GetNewValue<bool>());
                        ConfigMenu.Item("CR2MODE").Show(!ar.GetNewValue<bool>());
                    };
            combo.AddItem(new MenuItem("CR1MODE", "R1 Mode").SetValue(new StringList(new string[] { "Always", "If Killable With R2", "Smart" }))).Show(!combo.Item("CDISABLER").GetValue<bool>());
            combo.AddItem(new MenuItem("CR2MODE", "R2 Mode").SetValue(new StringList(new string[] { "Always", "If Killable", "If Out of Range" }, 1))).Show(!combo.Item("CDISABLER").GetValue<bool>());
            combo.AddItem(new MenuItem("CEMODE", "E Mode").SetValue(new StringList(new string[] { "E to enemy", "E Cursor Pos", "E to back off", "Dont Use E" }, 0)));
            combo.AddItem(new MenuItem("CUSEF", "Use Flash In Combo").SetValue(new KeyBind('G', KeyBindType.Toggle))).Permashow();

            Menu comboType = new Menu("Combo Methods", "combomethod");
            foreach (var enemy in HeroManager.Enemies)
            {
                ComboMethodBackup.Add(String.Format("CMETHOD{0}", enemy.ChampionName), new StringList(new string[] { "Normal", "Shy Burst", "Flash Combo" }));
                comboType.AddItem(new MenuItem(String.Format("CMETHOD{0}", enemy.ChampionName), enemy.ChampionName).SetValue(new StringList(new string[] { "Normal", "Shy Burst", "Flash Combo" })))
                    .ValueChanged += (s, ar) =>
                    {
                        if (!comboType.Item("CSHYKEY").GetValue<KeyBind>().Active && !comboType.Item("CFLASHKEY").GetValue<KeyBind>().Active)
                            ComboMethodBackup[((MenuItem)s).Name] = ar.GetNewValue<StringList>();
                    };
            }
            comboType.AddItem(new MenuItem("CSHYKEY", "Set All Shy Burst While Pressing Key").SetValue(new KeyBind('T', KeyBindType.Press))).Permashow();
            comboType.AddItem(new MenuItem("CFLASHKEY", "Set All Flash Combo While Pressing Key").SetValue(new KeyBind('Z', KeyBindType.Press))).Permashow();
            combo.AddSubMenu(comboType);


            Menu harass = new Menu("Harass", "harass");
            harass.AddItem(new MenuItem("HEMODE", "E Mode").SetValue(new StringList(new string[] { "E to enemy", "E Cursor Pos", "E to back off", "Dont Use E" }, 0)));


            Menu laneclear = new Menu("LaneClear/JungleClear", "laneclear");
            laneclear.AddItem(new MenuItem("LUSEQ", "Use Q").SetValue(true));
            laneclear.AddItem(new MenuItem("LUSEW", "Use W").SetValue(true))
                .ValueChanged += (s, ar) =>
                {
                    laneclear.Item("LMINW").Show(ar.GetNewValue<bool>());
                };
            laneclear.AddItem(new MenuItem("LMINW", "Min. Minion To W").SetValue(new Slider(1, 1, 6))).Show(laneclear.Item("LUSEW").GetValue<bool>());
            laneclear.AddItem(new MenuItem("LUSETIAMAT", "Use Tiamat/Hydra").SetValue(true));
            laneclear.AddItem(new MenuItem("LSEMIQJUNG", "Semi-Q Jungle Clear").SetValue(true));
            laneclear.AddItem(new MenuItem("LASTUSETIAMAT", "Use Tiamat/Hydra for Last Hitting").SetValue(true));

            Menu misc = new Menu("Misc", "misc");
            misc.AddItem(new MenuItem("MFLEEKEY", "Flee Key").SetValue(new KeyBind('A', KeyBindType.Press)));
            misc.AddItem(new MenuItem("MFLEEWJ", "Use Wall Jump while flee").SetValue(true)).Permashow();
            misc.AddItem(new MenuItem("MKEEPQ", "Keep Q Alive (To Cursor Pos)").SetValue(false));
            misc.AddItem(new MenuItem("MMINDIST", "Min. Distance to gapclose").SetValue(new Slider(390, 250, 750)));
            misc.AddItem(new MenuItem("MAUTOINTRW", "Interrupt Spells With W").SetValue(true));
            misc.AddItem(new MenuItem("MAUTOINTRQ", "Try Interrupt Spells With Ward & Q3").SetValue(false));
            misc.AddItem(new MenuItem("MANTIGAPW", "Anti Gap Closer With W").SetValue(true));
            misc.AddItem(new MenuItem("MANTIGAPQ", "Try Anti Gap Closer With Ward & Q3").SetValue(false));
            misc.AddItem(new MenuItem("DDRAWCOMBOMODE", "Draw Combo Mode").SetValue(true));
            DamageIndicator.Initialize((t) => (float)CalculateComboDamage(t) + (float)CalculateDamageR2(t), misc);


            ConfigMenu.AddSubMenu(combo);
            ConfigMenu.AddSubMenu(harass);
            ConfigMenu.AddSubMenu(laneclear);
            ConfigMenu.AddSubMenu(misc);
            ConfigMenu.AddToMainMenu();

            ComboInstance.Initialize(this);
        }

        public override void SetSpells()
        {
            Spells[Q] = new Spell(SpellSlot.Q, 260f);
            Spells[W] = new Spell(SpellSlot.W, 250f);
            Spells[E] = new Spell(SpellSlot.E, 270f);
            Spells[R] = new Spell(SpellSlot.R, 900f);
            Spells[R].SetSkillshot(0.25f, 225f, 1600f, false, SkillshotType.SkillshotCone);
        }

        public void BeforeOrbwalk()
        {
            if (!Spells[Q].IsReady(1000))
            {
                Animation.QStacks = 0;
                IsDoingFastQ = false;
            }

            if (!Spells[R].IsReady())
                Animation.UltActive = false;

            if (ConfigMenu.Item("MFLEEKEY").GetValue<KeyBind>().Active)
                Flee();

            if (ConfigMenu.Item("CSHYKEY").GetValue<KeyBind>().Active)
            {
                foreach (var enemy in HeroManager.Enemies)
                {
                    var typeVal = ConfigMenu.Item(String.Format("CMETHOD{0}", enemy.ChampionName)).GetValue<StringList>();
                    if (typeVal.SelectedIndex != 1)
                    {
                        typeVal.SelectedIndex = 1;
                        ConfigMenu.Item(String.Format("CMETHOD{0}", enemy.ChampionName)).SetValue(typeVal);
                    }
                }
                var target = LeagueSharp.Common.TargetSelector.GetTarget(-1, TargetSelector.DamageType.Physical);
                Orbwalker.Orbwalk(target, Game.CursorPos);
                Combo();
                return;
            }
            else if (ConfigMenu.Item("CFLASHKEY").GetValue<KeyBind>().Active)
            {
                foreach (var enemy in HeroManager.Enemies)
                {
                    var typeVal = ConfigMenu.Item(String.Format("CMETHOD{0}", enemy.ChampionName)).GetValue<StringList>();
                    if (typeVal.SelectedIndex != 2)
                    {
                        typeVal.SelectedIndex = 2;
                        ConfigMenu.Item(String.Format("CMETHOD{0}", enemy.ChampionName)).SetValue(typeVal);
                    }
                }
                var target = LeagueSharp.Common.TargetSelector.GetTarget(-1, TargetSelector.DamageType.Physical);
                Orbwalker.Orbwalk(target, Game.CursorPos);
                Combo();
                return;
            }
            else
            {
                foreach (var enemy in HeroManager.Enemies)
                {
                    var typeVal = ConfigMenu.Item(String.Format("CMETHOD{0}", enemy.ChampionName)).GetValue<StringList>();
                    if (typeVal.SelectedIndex != ComboMethodBackup[String.Format("CMETHOD{0}", enemy.ChampionName)].SelectedIndex)
                    {
                        typeVal.SelectedIndex = ComboMethodBackup[String.Format("CMETHOD{0}", enemy.ChampionName)].SelectedIndex;
                        ConfigMenu.Item(String.Format("CMETHOD{0}", enemy.ChampionName)).SetValue(typeVal);
                    }
                }
            }

            if (ConfigMenu.Item("MKEEPQ").GetValue<bool>() && Animation.QStacks != 0 && Utils.TickCount - Animation.LastQTick >= 3500)
                Spells[Q].Cast(Game.CursorPos);
        }

        public void BeforeDraw()
        {
            if (ConfigMenu.Item("DDRAWCOMBOMODE").GetValue<bool>())
            {
                foreach (var enemy in HeroManager.Enemies)
                {
                    if (!enemy.IsDead && enemy.IsVisible)
                    {
                        var text_pos = Drawing.WorldToScreen(enemy.Position);
                        Drawing.DrawText((int)text_pos.X - 20, (int)text_pos.Y + 35, System.Drawing.Color.Aqua, ConfigMenu.Item(String.Format("CMETHOD{0}", enemy.ChampionName)).GetValue<StringList>().SelectedValue);
                    }
                }
            }
        }

        public void Combo()
        {
            var t = Target.Get(600, true);
            if (t != null)
                ComboInstance.MethodsOnUpdate[ConfigMenu.Item(String.Format("CMETHOD{0}", t.ChampionName)).GetValue<StringList>().SelectedIndex](t);
        }

        public void LaneClear()
        {
            var minion = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 400, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).FirstOrDefault();
            if (minion != null)
            {
                if (ConfigMenu.Item("LUSEQ").GetValue<bool>() && Spells[Q].IsReady())
                {
                    Animation.SetAttack(true);
                    if (!IsDoingFastQ && !SCommon.Orbwalking.Utility.InAARange(minion))
                        Spells[Q].Cast(minion.ServerPosition);
                    IsDoingFastQ = true;
                }

                if (ConfigMenu.Item("LUSEW").GetValue<bool>() && Spells[W].IsReady() && (ObjectManager.Get<Obj_AI_Minion>().Count(p => MinionManager.IsMinion(p) && p.IsValidTarget(Spells[W].Range)) >= ConfigMenu.Item("LMINW").GetValue<Slider>().Value || minion.IsJungleMinion()))
                {
                    if (ConfigMenu.Item("LUSETIAMAT").GetValue<bool>())
                        CastCrescent();
                    Spells[W].Cast();
                }
            }
        }

        public void LastHit()
        {
            if (ConfigMenu.Item("LASTUSETIAMAT").GetValue<bool>() && IsCrestcentReady)
            {
                var minion = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 400, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).FirstOrDefault();
                if (minion != null)
                {
                    float dist = minion.Distance(ObjectManager.Player.ServerPosition);
                    double dmg = (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod) * (1 - dist * 0.001);
                    if (minion.Health <= dmg)
                        CastCrescent();
                }
            }
        }

        public void Flee()
        {
            if (Spells[Q].IsReady() && Animation.QStacks != 2)
                Spells[Q].Cast(Game.CursorPos);

            if (ConfigMenu.Item("MFLEEWJ").GetValue<bool>())
            {
                if (Spells[Q].IsReady())
                {
                    var curSpot = WallJump.GetSpot(ObjectManager.Player.ServerPosition);
                    if (curSpot.Start != Vector3.Zero && Animation.QStacks == 2)
                    {
                        if (Spells[E].IsReady())
                            Spells[E].Cast(curSpot.End);
                        else
                            if (Items.GetWardSlot() != null)
                                Items.UseItem((int)Items.GetWardSlot().Id, curSpot.End);
                        Spells[Q].Cast(curSpot.End);
                        return;
                    }
                    var spot = WallJump.GetNearest(Game.CursorPos);
                    if (spot.Start != Vector3.Zero)
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, spot.Start);
                        return;
                    }
                    else
                        Spells[E].Cast(Game.CursorPos);
                }
            }
            else
            {
                if (Spells[Q].IsReady() && Animation.QStacks == 2)
                    Spells[Q].Cast(Game.CursorPos);

                if (Spells[E].IsReady())
                    Spells[E].Cast(Game.CursorPos);
            }

            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
        }

        public void FastQCombo()
        {
            if (Spells[Q].IsReady())
            {
                var t = Target.Get(Spells[Q].Range);
                if (t != null)
                {
                    Target.Set(t);
                    Orbwalker.ForcedTarget = t;
                    Animation.SetAttack(true);
                    if (!IsDoingFastQ && !SCommon.Orbwalking.Utility.InAARange(t))
                        Spells[Q].Cast(t.ServerPosition);
                    IsDoingFastQ = true;
                }
            }
        }

        public bool CheckR1(Obj_AI_Hero t)
        {
            if (!ObjectManager.Player.HasBuff("RivenFengShuiEngine") && !ConfigMenu.Item("CDISABLER").GetValue<bool>() && Spells[R].IsReady() && t.Distance(ObjectManager.Player.ServerPosition) < 500 && Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Combo)
            {
                if (ObjectManager.Player.GetSpellDamage(t, SpellSlot.Q) * 2 + ObjectManager.Player.GetSpellDamage(t, SpellSlot.W) + CalculateAADamage(t, 2) >= t.Health)
                    return false;

                if (ObjectManager.Player.ServerPosition.CountEnemiesInRange(500) > 1)
                    return true;

                switch (ConfigMenu.Item("CR1MODE").GetValue<StringList>().SelectedIndex)
                {
                    case 1: if (!(t.Health - CalculateComboDamage(t) - CalculateDamageR2(t) <= 0)) return false;
                        break;
                    case 2: if (!(t.Health - CalculateComboDamage(t) < 1000 && t.Health >= 1000)) return false;
                        break;
                }
                return true;
            }
            return false;
        }

        public bool CheckR2(Obj_AI_Hero t)
        {
            if (ObjectManager.Player.HasBuff("RivenFengShuiEngine") && !ConfigMenu.Item("CDISABLER").GetValue<bool>() && Spells[R].IsReady() && t.Distance(ObjectManager.Player.ServerPosition) < 900 && Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Combo)
            {
                switch (ConfigMenu.Item("CR2MODE").GetValue<StringList>().SelectedIndex)
                {
                    case 1: if (t.Health - CalculateDamageR2(t) > 0 || t.Distance(ObjectManager.Player.ServerPosition) > 650f) return false;
                        break;
                    case 2: if (t.Distance(ObjectManager.Player.ServerPosition) < 600) return false;
                        break;
                }
                return true;
            }
            return false;
        }

        public void CastCrescent()
        {
            if (ObjectManager.Player.CountEnemiesInRange(500) > 0 || Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.LaneClear)
            {
                if (Items.HasItem(3077) && Items.CanUseItem(3077)) //tiamat
                    Items.UseItem(3077);
                else if (Items.HasItem(3074) && Items.CanUseItem(3074)) //hydra
                    Items.UseItem(3074);

                Animation.CanCastAnimation = true;
            }
        }

        public override double CalculateAADamage(Obj_AI_Hero target, int aacount = 3)
        {
            double dmg = base.CalculateAADamage(target, aacount);                                                                                                                                                                                                                                                               /*          PBE            */
            dmg += ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical, new[] { 0.2, 0.2, 0.25, 0.25, 0.25, 0.3, 0.3, 0.3, 0.35, 0.35, 0.35, 0.4, 0.4, 0.4, 0.45, 0.45, 0.45, 0.5 }[ObjectManager.Player.Level - 1] * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod) * 5) /** (1 + EdgeCount * 0.001)*/;
            return dmg;
        }

        public override double CalculateDamageQ(Obj_AI_Hero target)
        {
            if (!Spells[Q].IsReady())
                return 0.0d;

            return base.CalculateDamageQ(target) * (3 - Animation.QStacks);
        }

        public override double CalculateDamageR(Obj_AI_Hero target)
        {
            if (!Spells[R].IsReady())
                return 0.0d;
            return ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical, ObjectManager.Player.FlatPhysicalDamageMod * 0.2 * 3);
        }

        public double CalculateDamageR2(Obj_AI_Hero target)
        {
            if (Spells[R].IsReady())
                return ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical, (new[] { 80, 120, 160 }[Spells[R].Level - 1] + ObjectManager.Player.FlatPhysicalDamageMod * 0.6) * (1 + ((100 - target.HealthPercent) > 75 ? 75 : (100 - target.HealthPercent)) * 0.0267d));
            return 0.0d;
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.IsAutoAttack())
                    Animation.SetLastAATick(Utils.TickCount);
                else if (args.SData.Name == "RivenTriCleave")
                    Orbwalker.ResetAATimer();
            }
            else if (Target.Get(1000, true) != null)
            {
                if (args.SData.Name == "summonerflash")
                {
                    if (args.End.Distance(ObjectManager.Player.ServerPosition) > 300 && args.End.Distance(ObjectManager.Player.ServerPosition) < 500 && !Spells[E].IsReady())
                        Target.SetFlashed();
                }
            }
        }

        protected override void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Spells[W].IsReady() && sender.IsEnemy && sender.ServerPosition.Distance(ObjectManager.Player.ServerPosition) <= Spells[W].Range && ConfigMenu.Item("MAUTOINTRW").GetValue<bool>())
                Spells[W].Cast();
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!gapcloser.Sender.IsEnemy)
                return;

            if (Spells[W].IsReady() && gapcloser.End.Distance(ObjectManager.Player.ServerPosition) <= Spells[W].Range && ConfigMenu.Item("MANTIGAPW").GetValue<bool>())
                LeagueSharp.Common.Utility.DelayAction.Add(100 + Game.Ping, () => Spells[W].Cast());              
            
            if (ConfigMenu.Item("MANTIGAPQ").GetValue<bool>() && Animation.QStacks == 2)
            {
                if (gapcloser.Sender.Spellbook.GetSpell(gapcloser.Slot).SData.MissileSpeed != 0)
                {
                    LeagueSharp.Common.Utility.DelayAction.Add((int)(gapcloser.End.Distance(gapcloser.Start) / gapcloser.Sender.Spellbook.GetSpell(gapcloser.Slot).SData.MissileSpeed * 1000f) - Game.Ping, () =>
                    {
                        if (Items.GetWardSlot() != null)
                            Items.UseItem((int)Items.GetWardSlot().Id, ObjectManager.Player.ServerPosition + (gapcloser.End - gapcloser.Start).Normalized() * 40);
                        Spells[Q].Cast(ObjectManager.Player.ServerPosition);
                    });
                }
            }
        }

        public void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == (uint)WindowsMessages.WM_LBUTTONDBLCLK)
            {
                var clickedTarget = HeroManager.Enemies
                    .FindAll(hero => hero.IsValidTarget() && hero.Distance(Game.CursorPos, true) < 40000) // 200 * 200
                    .OrderBy(h => h.Distance(Game.CursorPos, true)).FirstOrDefault();

                if (clickedTarget != null)
                {
                    var typeVal = ConfigMenu.Item(String.Format("CMETHOD{0}", clickedTarget.ChampionName)).GetValue<StringList>();
                    typeVal.SelectedIndex = (typeVal.SelectedIndex + 1) % 3;
                    ConfigMenu.Item(String.Format("CMETHOD{0}", clickedTarget.ChampionName)).SetValue(typeVal);
                }
            }
        }

        private void Animation_OnAnimationCastable(string animname)
        {
            if (Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Combo || Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Mixed || ConfigMenu.Item("CSHYKEY").GetValue<KeyBind>().Active || ConfigMenu.Item("CFLASHKEY").GetValue<KeyBind>().Active)
            {
                var t = Target.Get(1000);
                if (t != null)
                    ComboInstance.MethodsOnAnimation[ConfigMenu.Item(String.Format("CMETHOD{0}", t.ChampionName)).GetValue<StringList>().SelectedIndex](t, animname);
            }
        }
    }
}
