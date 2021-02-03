namespace AiEzEvade
{
    internal class ezEvadeSettings
    {
        public int EvadeMode;
        public int TickLimiter;
        public int ReactionTime;
        public int SpellDetectionTime;

        public bool DodgeDangerous;
        public bool DodgeCircularSpells;
        public bool DodgeFOWSpells;
        public bool CheckSpellCollision;
        public bool ContinueMovement;
        public bool ClickOnlyOnce;
        public bool FastMovementBlock;

        public ezEvadeSettings(int evadeMode, bool dodgeDangerous, bool dodgeCircularSpells, bool dodgeFOWSpells,
            bool checkSpellCollision, bool continueMovement, bool clickOnlyOnce, int tickLimiter, int reactionTime,
            int spellDetectionTime, bool fastMovementBlock)
        {
            EvadeMode = evadeMode;
            DodgeDangerous = dodgeDangerous;
            DodgeCircularSpells = dodgeCircularSpells;
            DodgeFOWSpells = dodgeFOWSpells;
            CheckSpellCollision = checkSpellCollision;
            ContinueMovement = continueMovement;
            ClickOnlyOnce = clickOnlyOnce;
            TickLimiter = tickLimiter;
            ReactionTime = reactionTime;
            SpellDetectionTime = spellDetectionTime;
            FastMovementBlock = fastMovementBlock;
        }
    }

}
