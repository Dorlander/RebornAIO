using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace SCommon.Orbwalking
{
    public class Drawings
    {
        private Orbwalker m_Instance;

        public Drawings(Orbwalker instance)
        {
            m_Instance = instance;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (m_Instance.Configuration.SelfAACircle.Active)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Utility.GetAARange(), m_Instance.Configuration.SelfAACircle.Color, m_Instance.Configuration.LineWidth);

            if(m_Instance.Configuration.EnemyAACircle.Active)
            {
                foreach (var target in HeroManager.Enemies.FindAll(target => target.IsValidTarget(1200)))
                    Render.Circle.DrawCircle(target.Position, Utility.GetAARange(target), m_Instance.Configuration.EnemyAACircle.Color, m_Instance.Configuration.LineWidth);
            }

            if (m_Instance.Configuration.HoldZone.Active)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, m_Instance.Configuration.HoldAreaRadius, m_Instance.Configuration.HoldZone.Color, m_Instance.Configuration.LineWidth);

            if(m_Instance.Configuration.LastHitMinion.Active)
            {
                foreach(var minion in MinionManager.GetMinions(1200))
                {
                    if (Damage.Prediction.IsLastHitable(minion))
                        Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius * 2, m_Instance.Configuration.LastHitMinion.Color, m_Instance.Configuration.LineWidth);
                }
            }
        }
    }
}
