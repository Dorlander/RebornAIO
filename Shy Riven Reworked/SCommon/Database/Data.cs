using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace SCommon.Database
{
    public static class Data
    {
        private static Dictionary<string, float> s_HeroHitBoxes;
        private static Dictionary<string, int> s_JunglePrio;
        private static List<WardData> s_WardData;
        private static List<ChampionData> s_ChampionData;

        static Data()
        {
            GenerateHitBoxData();
            GenerateJungleData();
            GenerateWardData();
            GenerateChampionData();
        }

        public static float GetOrginalHitBox(this Obj_AI_Hero hero)
        {
            return s_HeroHitBoxes[hero.ChampionName];
        }

        public static int GetJunglePriority(this Obj_AI_Base mob)
        {
            if (!s_JunglePrio.ContainsKey(mob.Name))
                return 0;
            return s_JunglePrio[mob.Name];
        }

        public static bool IsWard(this Obj_AI_Base unit)
        {
            return s_WardData.Exists(p => p.Name == unit.Name);
        }

        public static int GetPriority(this Obj_AI_Hero hero)
        {
            return (int)s_ChampionData.Find(p => p.Name == hero.ChampionName).Role;
        }

        public static ChampionRole GetRole(this Obj_AI_Hero hero)
        {
            return s_ChampionData.Find(p => p.Name == hero.ChampionName).Role;
        }

        public static int GetID(this Obj_AI_Hero hero)
        {
            return s_ChampionData.Find(p => p.Name == hero.ChampionName).ID;
        }

        public static int GetID(string name)
        {
            return s_ChampionData.Find(p => p.Name == name).ID;
        }

        public static int GetMaxHeroID()
        {
            return s_ChampionData.MaxOrDefault(p => p.ID).ID;
        }

        private static void GenerateHitBoxData()
        {
            if (s_HeroHitBoxes != null)
                return;
            s_HeroHitBoxes = new Dictionary<string, float>();
            foreach (var hero in HeroManager.AllHeroes)
                if (!s_HeroHitBoxes.ContainsKey(hero.ChampionName))
                    s_HeroHitBoxes.Add(hero.ChampionName, hero.BBox.Minimum.Distance(hero.BBox.Maximum));
        }

        private static void GenerateJungleData()
        {
            if (s_JunglePrio != null)
                return;
            s_JunglePrio = new Dictionary<string, int>();
            s_JunglePrio.Add("SRU_Razorbeak3.1.1", 1);
            s_JunglePrio.Add("SRU_RazorbeakMini3.1.4", 2);
            s_JunglePrio.Add("SRU_RazorbeakMini3.1.3", 2);
            s_JunglePrio.Add("SRU_RazorbeakMini3.1.2", 2);
            s_JunglePrio.Add("SRU_Red4.1.1", 1);
            s_JunglePrio.Add("SRU_RedMini4.1.2", 2);
            s_JunglePrio.Add("SRU_RedMini4.1.3", 2);
            s_JunglePrio.Add("SRU_Krug5.1.2", 1);
            s_JunglePrio.Add("SRU_KrugMini5.1.1", 2);
            s_JunglePrio.Add("SRU_Murkwolf2.1.1", 1);
            s_JunglePrio.Add("SRU_MurkwolfMini2.1.3", 2);
            s_JunglePrio.Add("SRU_MurkwolfMini2.1.2", 2);
            s_JunglePrio.Add("SRU_Blue1.1.1", 1);
            s_JunglePrio.Add("SRU_BlueMini21.1.3", 2);
            s_JunglePrio.Add("SRU_BlueMini1.1.2", 2);
            s_JunglePrio.Add("SRU_Gromp13.1.1", 1);
            s_JunglePrio.Add("SRU_Razorbeak9.1.1", 1);
            s_JunglePrio.Add("SRU_RazorbeakMini9.1.4", 2);
            s_JunglePrio.Add("SRU_RazorbeakMini9.1.3", 2);
            s_JunglePrio.Add("SRU_RazorbeakMini9.1.2", 2);
            s_JunglePrio.Add("SRU_Red10.1.1", 1);
            s_JunglePrio.Add("SRU_RedMini10.1.2", 2);
            s_JunglePrio.Add("SRU_RedMini10.1.3", 2);
            s_JunglePrio.Add("SRU_Krug11.1.2", 1);
            s_JunglePrio.Add("SRU_KrugMini11.1.1", 2);
            s_JunglePrio.Add("SRU_Murkwolf8.1.1", 1);
            s_JunglePrio.Add("SRU_MurkwolfMini8.1.3", 2);
            s_JunglePrio.Add("SRU_MurkwolfMini8.1.2", 2);
            s_JunglePrio.Add("SRU_Blue7.1.1", 1);
            s_JunglePrio.Add("SRU_BlueMini27.1.3", 2);
            s_JunglePrio.Add("SRU_BlueMini7.1.2", 2);
            s_JunglePrio.Add("SRU_Gromp14.1.1", 1);
            s_JunglePrio.Add("SRU_Dragon6.1.1", 1);
            s_JunglePrio.Add("SRU_Baron12.1.1", 1);
            s_JunglePrio.Add("Sru_Crab15.1.1", 2);
            s_JunglePrio.Add("Sru_Crab16.1.1", 2);
            s_JunglePrio.Add("TT_NGolem2.1.1", 1);
            s_JunglePrio.Add("TT_NGolem22.1.2", 2);
            s_JunglePrio.Add("TT_NWraith21.1.3", 2);
            s_JunglePrio.Add("TT_NWraith21.1.2", 2);
            s_JunglePrio.Add("TT_NWraith1.1.1", 1);
            s_JunglePrio.Add("TT_NWolf23.1.3", 2);
            s_JunglePrio.Add("TT_NWolf23.1.2", 2);
            s_JunglePrio.Add("TT_NWolf3.1.1", 2);
            s_JunglePrio.Add("TT_NWolf26.1.3", 2);
            s_JunglePrio.Add("TT_NWolf26.1.2", 2);
            s_JunglePrio.Add("TT_NWolf6.1.1", 1);
            s_JunglePrio.Add("TT_NWraith4.1.1", 1);
            s_JunglePrio.Add("TT_NWraith24.1.3", 1);
            s_JunglePrio.Add("TT_NWraith24.1.2", 1);
            s_JunglePrio.Add("TT_NGolem25.1.2", 2);
            s_JunglePrio.Add("TT_NGolem5.1.1", 1);
            s_JunglePrio.Add("AscXerath", 1);
        }

        private static void GenerateWardData()
        {
            if (s_WardData != null)
                return;
            s_WardData = new List<WardData>();
            s_WardData.Add(
                new WardData
                {
                    Name = "VisionWard",
                    DisplayName = "VisionWard",
                    ObjectName = "visionward",
                    Type = ObjectType.Ward,
                    CastRange = 1450,
                    Duration = 180000,
                }
                );

            s_WardData.Add(
                new WardData
                {
                    Name = "SightWard",
                    DisplayName = "SightWard",
                    ObjectName = "sightward",
                    Type = ObjectType.Ward,
                    CastRange = 1450,
                    Duration = 180000,
                }
                );

            s_WardData.Add(
                new WardData
                {
                    Name = "YellowTrinket",
                    DisplayName = "SightWard",
                    ObjectName = "sightward",
                    Type = ObjectType.Ward,
                    CastRange = 1450,
                    Duration = 180000,
                }
                );

            s_WardData.Add(
                new WardData
                {
                    Name = "SightWard",
                    DisplayName = "SightWard",
                    ObjectName = "itemghostward",
                    Type = ObjectType.Ward,
                    CastRange = 1450,
                    Duration = 180000,
                }
                );

            s_WardData.Add(
                new WardData
                {
                    Name = "SightWard",
                    DisplayName = "SightWard",
                    ObjectName = "wrigglelantern",
                    Type = ObjectType.Ward,
                    CastRange = 1450,
                    Duration = 60000,
                }
                );

            s_WardData.Add(
                new WardData
                {
                    Name = "ShacoBox",
                    DisplayName = "Jack In The Box",
                    ObjectName = "jackinthebox",
                    Type = ObjectType.Ward,
                    CastRange = 1450,
                    Duration = 60000,
                }
                );
        }

        private static void GenerateChampionData()
        {
            if (s_ChampionData != null)
                return;

            s_ChampionData = new List<ChampionData>();
            int id = 0;
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Aatrox", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Ahri", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Akali", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Alistar", Role = ChampionRole.Support });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Amumu", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Anivia", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Annie", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Ashe", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Azir", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Bard", Role = ChampionRole.Support });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Blitzcrank", Role = ChampionRole.Support });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Brand", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Braum", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Caitlyn", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Cassiopeia", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Chogath", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Corki", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Darius", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Diana", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "DrMundo", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Draven", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Ekko", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Elise", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Evelynn", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Ezreal", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "FiddleSticks", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Fiora", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Fizz", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Galio", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Gangplank", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Garen", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Gnar", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Gragas", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Graves", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Hecarim", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Heimerdinger", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Irelia", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Janna", Role = ChampionRole.Support });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "JarvanIV", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Jax", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Jayce", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Jinx", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Karma", Role = ChampionRole.Support });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Kalista", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Karthus", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Kassadin", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Katarina", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Kayle", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Kennen", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Khazix", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "KogMaw", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Leblanc", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "LeeSin", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Leona", Role = ChampionRole.Support });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Lissandra", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Lucian", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Lulu", Role = ChampionRole.Support });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Lux", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Malphite", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Malzahar", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Maokai", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "MasterYi", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "MissFortune", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "MonkeyKing", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Mordekaiser", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Morgana", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Nami", Role = ChampionRole.Support });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Nasus", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Nautilus", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Nidalee", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Nocturne", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Nunu", Role = ChampionRole.Support });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Olaf", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Orianna", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Pantheon", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Poppy", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Quinn", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Rammus", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "RekSai", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Renekton", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Rengar", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Riven", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Rumble", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Ryze", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Sejuani", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Shaco", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Shen", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Shyvana", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Singed", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Sion", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Sivir", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Skarner", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Sona", Role = ChampionRole.Support });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Soraka", Role = ChampionRole.Support });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Swain", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Syndra", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "TahmKench", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Talon", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Taric", Role = ChampionRole.Support });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Teemo", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Thresh", Role = ChampionRole.Support });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Tristana", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Trundle", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Tryndamere", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "TwistedFate", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Twitch", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Udyr", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Urgot", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Varus", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Vayne", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Veigar", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Velkoz", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Vi", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Viktor", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Vladimir", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Volibear", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Warwick", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Xerath", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "XinZhao", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Yasuo", Role = ChampionRole.Bruiser });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Yorick", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Zac", Role = ChampionRole.Tank });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Zed", Role = ChampionRole.ADC });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Ziggs", Role = ChampionRole.AP });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Zilean", Role = ChampionRole.Support });
            s_ChampionData.Add(new ChampionData { ID = id++, Name = "Zyra", Role = ChampionRole.AP });
        }
    }
}
