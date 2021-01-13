namespace VayneHunter_Reborn.External.ProfileSelector.ProfileValues
{
    class ValueBool : IValue
    {
        public static bool Value;
        public ValueBool(bool val)
        {
            Value = val;
        }

        public bool GetValue()
        {
            return Value;
        }
    }
}
