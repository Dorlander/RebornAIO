namespace KoreanZed.QueueActions
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    class ActionQueueCheckAutoAttack
    {
        private bool status;

        public bool Status
        {
            get
            {
                if (status)
                {
                    status = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public ActionQueueCheckAutoAttack()
        {
            status = false;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
            {
                return;
            }

            status = true;
            Utility.DelayAction.Add(100, () => status = false);
        }

        private void Game_OnUpdate(EventArgs args)
        {

        }
    }
}
