using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using ShineCommon;
using SPrediction;
using SharpDX;

namespace MidlaneSharp
{
    public partial class Orianna : BaseChamp
    {
        private Action[] UltMethods = new Action[3];
        private string[] InitiatorsList = new string[] 
        {   
            "aatroxq", "akalishadowdance", "headbutt", "carpetbomb", "dianateleport", "elisespidereinitial", "fioraq",
            "fizzpiercingstrike", "gnarbige", "gnare", "gragase", "gravesmove", "hecarimult", "ireliagatotsu", "jarvanivdragonstrike",
            "jaxleapstrike", "jaycetotheskies", "riftwalk", "khazixe", "khazixelong", "leblancslide", "leblancslidem",
            "blindmonkqtwo", "leonazenithblade", "luciane", "ufslash", "monkeykingnimbus", "pantheon_leapbash", "poppyheroiccharge",
            "renektonsliceanddice", "riventricleave", "rivenfeint", "sejuaniarcticassault", "shenshadowdash", "shyvanatransformcast",
            "rocketjump", "slashcast", "viq", "xenzhaosweep", "yasuodashwrapper", "zace", "ziggswtoggle"
        };

        public Orianna()
            : base("Orianna")
        {
            Ball = new BallMgr();
            Ball.OnProcessCommand += Ball_OnProcessCommand;
        }

        public override void CreateConfigMenu()
        {
            combo = new Menu("Combo", "Combo");
            combo.AddItem(new MenuItem("CUSEQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("CUSEW", "Use W").SetValue(true));
            combo.AddItem(new MenuItem("CUSEE", "Use E For Damage Enemy").SetValue(true));
            //
            ult = new Menu("R Settings", "rsetting");
            ult.AddItem(new MenuItem("CUSER", "Use R").SetValue(true));
            ult.AddItem(new MenuItem("CUSERMETHOD", "R Method").SetValue<StringList>(new StringList(new string[] { "Only If Will Hit >= X Method", "If Will Hit Toggle Selected", "Midlane# Smart R" }, 2)))
                .ValueChanged += (s, ar) =>
                    {
                        ult.Item("CUSERHIT").Show(ar.GetNewValue<StringList>().SelectedIndex == 0);
                    };
            ult.AddItem(new MenuItem("CUSERHIT", "Use When Enemy Count >=").SetValue<Slider>(new Slider(3, 1, 5))).Show(ult.Item("CUSERMETHOD").GetValue<StringList>().SelectedIndex == 0);
            ult.AddItem(new MenuItem("DTOGGLER", "Draw Toggle R").SetValue(true));
            //
            combo.AddSubMenu(ult);

            harass = new Menu("Harass", "Harass");
            harass.AddItem(new MenuItem("HUSEQ", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("HUSEW", "Use W").SetValue(true));
            harass.AddItem(new MenuItem("HUSEE", "Use E For Damage Enemy").SetValue(true));
            harass.AddItem(new MenuItem("HMANA", "Min. Mana Percent").SetValue(new Slider(50, 0, 100)));

            laneclear = new Menu("Lane/Jungle Clear", "LaneClear");
            laneclear.AddItem(new MenuItem("LUSEQ", "Use Q").SetValue(true));
            laneclear.AddItem(new MenuItem("LUSEW", "Use W").SetValue(true));
            laneclear.AddItem(new MenuItem("LMINW", "Min. Minions To W In Range").SetValue(new Slider(3, 1, 12)));
            laneclear.AddItem(new MenuItem("LMANA", "Min. Mana Percent").SetValue(new Slider(50, 0, 100)));

            misc = new Menu("Misc", "Misc");
            misc.AddItem(new MenuItem("MANTIGAPEW", "Anti Gap Closer With E->W").SetValue(true));
            misc.AddItem(new MenuItem("MINTIMPORTANT", "Interrupt Important Spells With Q->R").SetValue(true));
            misc.AddItem(new MenuItem("MEINIT", "Cast E On Initiators").SetValue(true));
            misc.AddItem(new MenuItem("DDRAWBALL", "Draw Ball Position").SetValue(false));
            misc.AddItem(new MenuItem("DDRAWKILL", "Draw Killable Enemy").SetValue(true));

            Config.AddSubMenu(combo);
            Config.AddSubMenu(harass);
            Config.AddSubMenu(laneclear);
            Config.AddSubMenu(misc);
            Config.AddToMainMenu();

            BeforeDrawing += BeforeDraw;
            BeforeOrbWalking += BeforeOrbWalk;
            OrbwalkingFunctions[OrbwalkingComboMode] += Combo;
            OrbwalkingFunctions[OrbwalkingHarassMode] += Harass;
            OrbwalkingFunctions[OrbwalkingLaneClearMode] += LaneClear;
        }

        public override void SetSpells()
        {
            Spells[Q] = new Spell(SpellSlot.Q, 825f * 2f);
            Spells[Q].SetSkillshot(0f, 130f, 1400f, false, SkillshotType.SkillshotLine);

            Spells[W] = new Spell(SpellSlot.W, 220f);

            Spells[E] = new Spell(SpellSlot.E, 1100);

            Spells[R] = new Spell(SpellSlot.R, 330f);

            UltMethods[0] = () =>
            {
                {
                    if (Ball.Position.CountEnemiesInRange(Spells[R].Range) >= ult.Item("CUSERHIT").GetValue<Slider>().Value)
                    {
                        Ball.Post(BallMgr.Command.Shockwave, null);
                        return;
                    }
                }

                if (Spells[Q].IsReady() && Spells[R].IsReady())
                {
                    Vector3 bestQPos = Vector3.Zero;
                    int bestEnemyCount = 0;
                    foreach (var enemy in HeroManager.Enemies)
                    {
                        var enemies = enemy.GetEnemiesInRange(Spells[R].Range);
                        if (enemies.Count >= ult.Item("CUSERHIT").GetValue<Slider>().Value)
                        {
                            if (enemies.Count > bestEnemyCount)
                            {
                                bestEnemyCount = enemies.Count;
                                //find center of enemies
                                Vector3 pos = Vector3.Zero;
                                enemies.ForEach(p => pos += p.ServerPosition);
                                pos = pos / enemies.Count;
                                if (pos.Distance(ObjectManager.Player.ServerPosition) < Spells[Q].Range / 2f && pos.CountEnemiesInRange(Spells[R].Range) >= bestEnemyCount)
                                    bestQPos = pos;
                                else
                                    bestQPos = enemy.ServerPosition;
                            }
                        }
                    }

                    if (bestQPos != Vector3.Zero && bestQPos.Distance(ObjectManager.Player.ServerPosition) < Spells[Q].Range / 2f)
                    {
                        if (Ball.IsBallReady && Spells[Q].IsReady())
                        {
                            Spells[Q].Cast(bestQPos);
                            Ball.Post(BallMgr.Command.Shockwave, null);
                        }
                    }
                }
            };

            UltMethods[1] = () =>
            {
                if (TargetSelector.SelectedTarget != null)
                {
                    if (Spells[Q].IsReady() && Spells[R].IsReady())
                    {
                        if (TargetSelector.SelectedTarget.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < Spells[Q].Range / 2f)
                        {
                            Ball.Post(BallMgr.Command.Attack, TargetSelector.SelectedTarget);
                            Ball.Post(BallMgr.Command.Shockwave, null);
                        }
                    }
                    else if (Spells[R].IsReady())
                    {
                        if(TargetSelector.SelectedTarget.ServerPosition.Distance(Ball.Position) < Spells[R].Range)
                            Ball.Post(BallMgr.Command.Shockwave, TargetSelector.SelectedTarget);
                    }
                }
            };

            UltMethods[2] = () =>
            {
                if (Spells[Q].IsReady() && Spells[R].IsReady())
                {
                    Vector3 bestQPos = Vector3.Zero;
                    int bestPrioSum = 0;
                    foreach (var enemy in HeroManager.Enemies)
                    {
                        var enemies = enemy.GetEnemiesInRange(Spells[R].Range);
                        int prio_sum = 0;
                        foreach (var e in enemies)
                        {
                            prio_sum += ShineCommon.Utility.GetPriority(e.ChampionName);
                            if (e.HealthPercent < 50)
                                prio_sum += 1;
                        }

                        if (prio_sum >= 6)
                        {
                            if (prio_sum > bestPrioSum)
                            {
                                bestPrioSum = prio_sum;
                                //find center of enemies
                                Vector3 pos = Vector3.Zero;
                                enemies.ForEach(p => pos += p.ServerPosition);
                                pos = pos / enemies.Count;

                                var enemies2 = pos.GetEnemiesInRange(Spells[R].Range);
                                int prio_sum2 = 0;
                                foreach (var e in enemies2)
                                {
                                    prio_sum2 += ShineCommon.Utility.GetPriority(e.ChampionName);
                                    if (e.HealthPercent < 50)
                                        prio_sum2 += 1;
                                }

                                if (pos.Distance(ObjectManager.Player.ServerPosition) < Spells[Q].Range / 2f && prio_sum2 >= bestPrioSum)
                                    bestQPos = pos;
                                else
                                    bestQPos = enemy.ServerPosition;
                            }
                        }
                    }

                    if (bestQPos != Vector3.Zero && bestQPos.Distance(ObjectManager.Player.ServerPosition) < Spells[Q].Range / 2f)
                    {
                        if (Ball.IsBallReady && Spells[Q].IsReady())
                        {
                            Spells[Q].Cast(bestQPos);
                            Ball.Post(BallMgr.Command.Shockwave, null);
                        }
                    }
                }

                {
                    int prio_sum = 0;
                    var enemies = HeroManager.Enemies.Where(p => p.ServerPosition.Distance(Ball.Position) <= Spells[R].Range);
                    foreach (var enemy in enemies)
                    {
                        prio_sum += ShineCommon.Utility.GetPriority(enemy.ChampionName);
                        if (enemy.HealthPercent < 50)
                            prio_sum += 1;
                    }

                    if (prio_sum >= 6)
                    {
                        Ball.Post(BallMgr.Command.Shockwave, null);
                        return;
                    }

                    var t = TargetSelector.GetTarget(Spells[R].Range, TargetSelector.DamageType.Magical, true, null, Ball.Position);
                    if (t != null && ObjectManager.Player.CountEnemiesInRange(2000) <= 2)
                    {
                        Ball.Post(BallMgr.Command.Shockwave, t);
                        return;
                    }
                }
            };
        }

        public void BeforeDraw()
        {
            if (Config.Item("DDRAWKILL").GetValue<bool>())
            {
                foreach (var enemy in HeroManager.Enemies)
                {
                    if (!enemy.IsDead && enemy.Health < CalculateComboDamage(enemy))
                    {
                        var killable_pos = Drawing.WorldToScreen(enemy.Position);
                        Drawing.DrawText((int)killable_pos.X - 20, (int)killable_pos.Y + 35, System.Drawing.Color.Red, "Killable");
                    }
                }
            }

            if (Config.Item("DDRAWBALL").GetValue<bool>() && Ball.Position != Vector3.Zero)
                Render.Circle.DrawCircle(Ball.Position, 130f, System.Drawing.Color.Red, 1);
        }

        public void BeforeOrbWalk()
        {
            float predictedHealth = HealthPrediction.GetHealthPrediction(ObjectManager.Player, 500);
            if (ObjectManager.Player.Health - predictedHealth > 100 || predictedHealth <= 0)
                Spells[E].CastOnUnit(ObjectManager.Player);
            else if(ObjectManager.Player.HealthPercent <= 10 && OrbwalkingActiveMode != OrbwalkingNoneMode)
                Spells[E].CastOnUnit(ObjectManager.Player);

            if (OrbwalkingActiveMode == OrbwalkingNoneMode)
                Ball.ClearWorkQueue();
        }

        public void Combo()
        {
            //R
            if (Config.Item("CUSER").GetValue<bool>())
                UltMethods[ult.Item("CUSERMETHOD").GetValue<StringList>().SelectedIndex]();

            if (Spells[Q].IsReady() && Config.Item("CUSEQ").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(Spells[Q].Range / 2f, TargetSelector.DamageType.Magical);
                if(t != null)
                    Ball.Post(BallMgr.Command.Attack, t);
            }
            
            if (Spells[W].IsReady() && Config.Item("CUSEW").GetValue<bool>())
            {
                if (CountEnemiesInRangePredicted(Spells[W].Range - 50, 50, 0.25f) > 0)
                    Ball.Post(BallMgr.Command.Dissonance, null);
            }

            if (Spells[E].IsReady() && !Spells[W].IsReady() && Config.Item("CUSEE").GetValue<bool>())
            {
                if (Ball.CheckHeroCollision(ObjectManager.Player.ServerPosition))
                    Ball.Post(BallMgr.Command.Protect, ObjectManager.Player);
            }
        }

        public void Harass()
        {
            if (ObjectManager.Player.ManaPercent < Config.Item("HMANA").GetValue<Slider>().Value)
                return;

            if (Spells[Q].IsReady() && Config.Item("HUSEQ").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(Spells[Q].Range / 2f, TargetSelector.DamageType.Magical);
                if (t != null)
                    Ball.Post(BallMgr.Command.Attack, t);
            }

            if (Spells[W].IsReady() && Config.Item("HUSEW").GetValue<bool>())
            {
                if (CountEnemiesInRangePredicted(Spells[W].Range, 50, 0.25f) > 0)
                    Ball.Post(BallMgr.Command.Dissonance, null);
            }

            if (Spells[E].IsReady() && !Spells[W].IsReady() && Config.Item("HUSEE").GetValue<bool>())
            {
                if (Ball.CheckHeroCollision(ObjectManager.Player.ServerPosition))
                    Ball.Post(BallMgr.Command.Protect, ObjectManager.Player);
            }
        }

        public void LaneClear()
        {
            if (ObjectManager.Player.ManaPercent < Config.Item("LMANA").GetValue<Slider>().Value)
                return;

            if (Spells[Q].IsReady() && Config.Item("LUSEQ").GetValue<bool>())
            {
                var farm = MinionManager.GetBestCircularFarmLocation(MinionManager.GetMinions(Spells[Q].Range / 2f, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None).Select(p => p.ServerPosition.To2D()).ToList(), Spells[Q].Width, Spells[Q].Range);
                if (farm.MinionsHit > 0 && Ball.IsBallReady)
                    Spells[Q].Cast(farm.Position, true); 
            }

            if (Spells[W].IsReady() && Config.Item("LUSEW").GetValue<bool>())
            {
                if (ObjectManager.Get<Obj_AI_Minion>().Count(p => (p.IsEnemy || p.IsJungleMinion()) && p.ServerPosition.Distance(Ball.Position) <= Spells[W].Range) >= Config.Item("LMINW").GetValue<Slider>().Value)
                    Spells[W].Cast(true);
            }

        }

        private void Ball_OnProcessCommand(Orianna.BallMgr.Command cmd, Obj_AI_Hero target)
        {
            if (!Spells[(int)cmd].IsReady() || (OrbwalkingActiveMode != OrbwalkingHarassMode && OrbwalkingActiveMode != OrbwalkingComboMode))
                return;

            switch (cmd)
            {
                case BallMgr.Command.Attack:
                {
                    Spells[Q].SPredictionCast(target, HitChance.High, 0, 1, Ball.Position);
                }
                break;

                case BallMgr.Command.Dissonance:
                {
                    Spells[W].Cast();
                }
                break;

                case BallMgr.Command.Protect:
                {
                    Spells[E].CastOnUnit(target);
                }
                break;

                case BallMgr.Command.Shockwave:
                {
                    if (CountEnemiesInRangePredicted(Spells[R].Range, 100, 0.75f, target) > 0)
                        Spells[R].Cast();
                }
                break;
            }
        }

        private int CountEnemiesInRangePredicted(float range, float width, float time, Obj_AI_Hero t = null)
        {
            int cnt = 0;
            bool hasTarget = false;
            foreach (var enemy in HeroManager.Enemies)
            {
                var prediction = SPrediction.Prediction.GetPrediction(enemy, width, time, 0, range, false, SkillshotType.SkillshotCircle, enemy.GetWaypoints(), enemy.AvgMovChangeTime(), enemy.LastMovChangeTime(), enemy.AvgPathLenght(), Ball.Position.To2D(), Ball.Position.To2D());
                if (prediction.HitChance > HitChance.Low)
                {
                    if (prediction.UnitPosition.Distance(Ball.Position.To2D()) < range)
                    {
                        cnt++;
                        if (t != null && enemy.NetworkId == t.NetworkId)
                            hasTarget = true;
                    }
                }
            }

            if (t != null && cnt == 1 && !hasTarget)
                return 0;

            return cnt;
        }

        public override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Spells[E].IsReady() && gapcloser.Sender.IsEnemy)
            {
                Spells[E].CastOnUnit(ObjectManager.Player);
                Spells[W].Cast();
                if (OrbwalkingActiveMode == OrbwalkingComboMode) //combo anti gap closer self r
                    Ball.Post(BallMgr.Command.Shockwave, null);
            }
        }

        public override void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender.IsChannelingImportantSpell())
            {
                if (Spells[R].IsReady() && Ball.Position.Distance(sender.ServerPosition) < Spells[R].Range)
                    Spells[R].Cast(true);
                else if (Spells[Q].IsReady() && Spells[R].IsReady() && ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition) < Spells[Q].Range / 2f)
                {
                    Spells[Q].Cast(sender, true);
                    Spells[W].Cast(true);
                }
            }
        }

        public override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsAlly)
            {
                if (InitiatorsList.Contains(args.SData.Name.ToLower()) && sender.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < Spells[E].Range && Config.Item("MEINIT").GetValue<bool>())
                    Spells[E].CastOnUnit(sender, true);
            }
        }

        public override double CalculateDamageQ(Obj_AI_Hero target)
        {
            double dmg = 0.0;
            if (Config.Item("CUSEQ").GetValue<bool>() && Spells[Q].IsReady())
            {
                dmg = ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);
                int collCount = Spells[R].GetCollision(Ball.Position.To2D(), new List<Vector2>() { target.ServerPosition.To2D() }).Count();
                int percent = 10 - (collCount > 6 ? 6 : collCount);
                dmg = dmg * percent * 0.1;
            }
            return dmg;
        }
    }
}
