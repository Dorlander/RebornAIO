namespace KoreanZed.QueueActions
{
    using System;

    struct ActionQueueItem
    {
        public float Time;
        public Func<bool> PreConditionFunc;
        public Func<bool> ConditionToRemoveFunc;
        public Action ComboAction;
    }
}
