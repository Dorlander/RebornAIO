using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSaliceResurrected.Base;
using xSaliceResurrected.Mid;
using Color = System.Drawing.Color;

namespace xSaliceResurrected.Managers
{
    internal class AoeSpellManager : SpellBase
    {
        private static Menu _menu;
        private static Boolean _qEnabled, _wEnabled, _eEnabled, _rEnabled, _qeEnabled;

        public static Menu AddHitChanceMenuCombo(Boolean q, Boolean w, Boolean e, Boolean r, Boolean qe = false)
        {
            _menu = new Menu("AOE Spells", "AOE Spells");

            _menu.AddItem(new MenuItem("enabledAOE", "Enabled", true).SetValue(true));

            if (q)
            {
                _menu.AddItem(new MenuItem("qAutoLaunch", "Auto Q if hit >= Enemies", true).SetValue(new Slider(3, 1, 5)));
                _qEnabled = true;
            }
            if (w)
            {
                _menu.AddItem(new MenuItem("wAutoLaunch", "Auto W if hit >= Enemies", true).SetValue(new Slider(3, 1, 5)));
                _wEnabled = true;
            }
            if (e)
            {
                _menu.AddItem(new MenuItem("eAutoLaunch", "Auto E if hit >= Enemies", true).SetValue(new Slider(3, 1, 5)));
                _eEnabled = true;
            }
            if (r)
            {
                _menu.AddItem(new MenuItem("rAutoLaunch", "Auto R if hit >= Enemies", true).SetValue(new Slider(3, 1, 5)));
                _rEnabled = true;
            }
            if (qe)
            {
                _menu.AddItem(new MenuItem("qeAutoLaunch", "Auto QE if hit >= Enemies", true).SetValue(new Slider(3, 1, 5)));
                _qeEnabled = true;
            }

            Game.OnUpdate += Mec;
            return _menu;
        }

        private static void Mec(EventArgs arg)
        {
            /*
            if (LagManager.Enabled)
                if (!LagManager.ReadyState)
                    return;
            */

            if (!_menu.Item("enabledAOE", true).GetValue<bool>())
                return;

            if (_qeEnabled)
                CastComboMec(QExtend, _menu.Item("qeAutoLaunch", true).GetValue<Slider>().Value);
            if (_qEnabled)
                CastMec(Q, _menu.Item("qAutoLaunch", true).GetValue<Slider>().Value);
            if (_wEnabled)
                CastMec(W, _menu.Item("wAutoLaunch", true).GetValue<Slider>().Value);
            if (_eEnabled)
                CastMec(E, _menu.Item("eAutoLaunch", true).GetValue<Slider>().Value);
            if (_rEnabled)
                CastMec(R, _menu.Item("rAutoLaunch", true).GetValue<Slider>().Value);

        }

        private static void CastMec(Spell spell, int minHit)
        {
            if (!spell.IsReady() || ObjectManager.Player.HealthPercent <= 10)
                return;

            foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(spell.Range)))
            {
                var pred = spell.GetPrediction(target, true);

                var nearByEnemies = 1;

                if (spell.Type == SkillshotType.SkillshotLine && spell.Collision)
                {
                    var poly = new Geometry.Polygon.Circle(pred.UnitPosition, spell.Width);

                    nearByEnemies +=
                        HeroManager.Enemies.Where(x => x.NetworkId != target.NetworkId)
                            .Count(enemy => poly.IsInside(enemy.ServerPosition));

                }
                else
                {
                    nearByEnemies = pred.AoeTargetsHitCount;
                }

                if (nearByEnemies >= minHit)
                {
                    
                    spell.Cast(target);
                    return;
                }
            }
        }

        private static void CastComboMec(Spell spell, int minHit)
        {
            if (!spell.IsReady() || !E.IsReady() || ObjectManager.Player.HealthPercent <= 10)
                return;

            const int gateDis = 200;

            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(spell.Range)))
            {
                var tarPred = spell.GetPrediction(target, true);
                
                Vector3 gateVector = ObjectManager.Player.Position + Vector3.Normalize(target.ServerPosition - ObjectManager.Player.Position)*gateDis;

                var nearByEnemies = 1;

                var poly = new Geometry.Polygon.Circle(tarPred.UnitPosition, spell.Width);

                nearByEnemies += HeroManager.Enemies.Where(x => x.NetworkId != target.NetworkId).Count(enemy => poly.IsInside(enemy.ServerPosition));

                if (ObjectManager.Player.Distance(tarPred.CastPosition) < spell.Range + 100 && nearByEnemies >= minHit)
                {
                    if (Jayce.HammerTime && R.IsReady() && Jayce.CanQcd == 0 && Jayce.CanEcd == 0)
                        R.Cast();
                    else if(Jayce.HammerTime)
                        return;

                    Console.WriteLine("Hit Combo: " + nearByEnemies);
                    E.Cast(gateVector);
                    spell.Cast(tarPred.CastPosition);
                    return;
                }
            }
        }
    }
}