using System;

namespace SCommon.Database
{
    public struct WardData
    {
        public string Name;
        public string DisplayName;
        public string ObjectName;
        public ObjectType Type;
        public int CastRange;
        public int Duration;
    }
    
    public struct ChampionData
    {
        public int ID;
        public string Name;
        public ChampionRole Role;
    }
}
