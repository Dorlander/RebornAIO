using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Version = System.Version;


namespace SigmaSeries.Plugins
{
    public class Tryndamere : PluginBase
    {
        public bool packetCast;

        public Tryndamere()
            : base(new Version(0, 1, 1))
        {
            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 0);

            E.SetSkillshot(0.5f, 225f, 700f, false, SkillshotType.SkillshotCircle);
        }


        public override void ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(false));
            config.AddItem(new MenuItem("minQHP", "Min Q HP").SetValue(new Slider(15, 1, 100)));
            config.AddItem(new MenuItem("smartSep", "- - Smart W Settings").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            config.AddItem(new MenuItem("wBack", "Use W if facing + Out of AA Range").SetValue(true));
            config.AddItem(new MenuItem("wFront", "Use W if in AA Range").SetValue(false));
            config.AddItem(new MenuItem("UseECombo", "Use E infront of unit").SetValue(false));
            config.AddItem(new MenuItem("UseRCombo", "Use R!").SetValue(true));
            config.AddItem(new MenuItem("minRHP", "Min R HP").SetValue(new Slider(5, 1, 100)));
        }

        public override void HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("extraPARRAMAMAMAM", "Use Mixed Mode, No Skills."));
        }

        public override void FarmMenu(Menu config)
        {
            config.AddItem(new MenuItem("useEWC", "Use E WC").SetValue(false));
            config.AddItem(
                new MenuItem("JungleActive", "Jungle Clear!").SetValue(new KeyBind("C".ToCharArray()[0],
                    KeyBindType.Press)));
            config.AddItem(new MenuItem("UseQJung", "Use Q").SetValue(false));
            config.AddItem(new MenuItem("minQHPJung", "Min Q HP").SetValue(new Slider(15, 1, 100)));
        }

        public override void BonusMenu(Menu config)
        {
            config.AddItem(new MenuItem("packetCast", "Packet Cast").SetValue(true));
        }

        public override void OnUpdate(EventArgs args)
        {
            packetCast = Config.Item("packetCast").GetValue<bool>();
            if (ComboActive)
            {
                combo();
            }
            if (HarassActive)
            {
                harass();
            }
            if (WaveClearActive)
            {
                waveClear();
            }
            if (FreezeActive)
            {
                freeze();
            }
            if (Config.Item("JungleActive").GetValue<KeyBind>().Active)
            {
                jungle();
            }
        }

        private void combo()
        {
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useWF = Config.Item("wFront").GetValue<bool>();
            var useWB = Config.Item("wBack").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();
            Obj_AI_Hero Target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);
            if (Target != null)
            {
                //castItems(Target);
                if (useW && W.IsReady())
                {
                    if (Player.Distance(Target) < Orbwalking.GetRealAutoAttackRange(Player) && useWF)
                    {
                        W.CastOnUnit(Player, packetCast);
                    }
                    else if (useWB)
                    {
                        useWSmart(Target);
                    }
                }
                if (Player.Distance(Target) > Orbwalking.GetRealAutoAttackRange(Player) &&
                    Player.Distance(Target) < E.Range && useE && E.IsReady())
                {
                    Vector2 pos1 = Player.Position.To2D();
                    Vector2 pos2 = (Prediction.GetPrediction(Target, 0.5f).CastPosition.To2D() - Player.Position.To2D());
                    pos2 = pos2.Normalized();

                    Vector2 pos = pos1 + (pos2*E.Range);

                    E.Cast(pos, packetCast);
                }
            }
            if (GetHpSliderEqual("minQHP", Player) && useQ && Q.IsReady())
            {
                Q.Cast(Player.Position, true);
            }
            if (GetHpSliderEqual("minRHP", Player) && useR && R.IsReady())
            {
                R.CastOnUnit(Player, packetCast);
            }
        }

        private void harass()
        {
        }

        // By DeTuKs
        private void useWSmart(Obj_AI_Hero target)
        {
            float trueAARange = Player.AttackRange + target.BoundingRadius;
            float trueERange = target.BoundingRadius + W.Range;

            float dist = Player.Distance(target);
            var dashPos = new Vector2();
            if (target.IsMoving)
            {
                Vector2 tpos = target.Position.To2D();
                Vector2 path = target.Path[0].To2D() - tpos;
                path.Normalize();
                dashPos = tpos + (path*100);
            }
            float targ_ms = (target.IsMoving && Player.Distance(dashPos) > dist) ? target.MoveSpeed : 0;
            float msDif = (Player.MoveSpeed - targ_ms) == 0 ? 0.0001f : (Player.MoveSpeed - targ_ms);
            float timeToReach = (dist - trueAARange)/msDif;
            if (dist > trueAARange && dist < trueERange && timeToReach > 1.7f ||
                dist > trueAARange && dist < trueERange && timeToReach < 0.0f)
            {
                W.CastOnUnit(Player, packetCast);;
            }
        }

        private void waveClear()
        {
            var useE = Config.Item("useEWC").GetValue<bool>();
            List<Obj_AI_Base> jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range,
                MinionTypes.All);

            if (jungleMinions.Count > 0)
            {
                foreach (Obj_AI_Base minion in jungleMinions)
                {
                    if (minion.IsValidTarget(E.Range) && E.IsReady() && useE)
                    {
                        MinionManager.FarmLocation ePoint = E.GetCircularFarmLocation(jungleMinions);
                        E.Cast(ePoint.Position, true);
                    }
                }
            }
        }

        private void freeze()
        {
        }

        private void jungle()
        {
            var useQ = Config.Item("UseQJung").GetValue<bool>();

            if (JungleMinions.Count > 0)
            {
                if (GetHpSliderEqual("minQHPJung", Player) && Q.IsReady())
                {
                    Q.Cast(Player.Position, true);
                }
            }
        }

        private bool GetHpSliderEqual(String s, Obj_AI_Base unit)
        {
            return Config.Item(s).GetValue<Slider>().Value >= ((unit.Health/unit.MaxHealth)*100);
        }
    }
}