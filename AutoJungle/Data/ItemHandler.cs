using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoJungle.Data
{
    internal class ItemHandler
    {
        public static List<BuyingOrder> ItemList = new List<BuyingOrder>();

        public ItemHandler(BuildType type)
        {
            switch (type)
            {
                case BuildType.AD:
                    SetListAD();
                    break;
                case BuildType.AS:
                    SetListAS();
                    break;
                case BuildType.ASMANA:
                    SetListASMANA();
                    break;
                case BuildType.NOC:
                    SetListNOC();
                    break;
                case BuildType.EVE:
                    SetListEVE();
                    break;
                case BuildType.APADT:
                    SetListAPADT();
                    break;
                case BuildType.VB:
                    SetlistVB();
                    break;
                case BuildType.Manwang:
                    SetlistManwang();
                    break;
                case BuildType.YI:
                    SetlistYI();
                    break;
                case BuildType.Bro:
                    SetlistBro();
                    break;
                case BuildType.Nu:
                    SetlistNu();
                    break;
                case BuildType.UD:
                    SetlistUD();
                    break;
                case BuildType.KOG:
                    SetlistKOG();
                    break;    
                case BuildType.Kayle:
                	SetlistKayle();
                    break;
            }
        }

        private void SetlistKayle()
        {
            ItemList.Add(new BuyingOrder(1, 1039, "Hunter's Talisman", 350));
            ItemList.Add(new BuyingOrder(2, 2031, "Refillable Potion", 150));
            ItemList.Add(new BuyingOrder(3, 3340, "Trinket", 0));
            ItemList.Add(new BuyingOrder(4, 1041, "Hunter's Machete", 350));
            ItemList.Add(new BuyingOrder(5, 3715, "Skirmisher's Blade", 700));
            ItemList.Add(new BuyingOrder(6, 3101, "Stinger", 1300));
            ItemList.Add(new BuyingOrder(7, 3006, "BKG", 1100));
            ItemList.Add(new BuyingOrder(8, 3115, "NsT", 1900));
            ItemList.Add(new BuyingOrder(9, 1037, "十字", 875));
            ItemList.Add(new BuyingOrder(10, 3124, "怒", 2725));
            ItemList.Add(new BuyingOrder(11, 1419, "Bloodrazor", 1625));
            ItemList.Add(new BuyingOrder(12, 3144, "Cutlass", 1500));
            ItemList.Add(new BuyingOrder(12, 3153, "Blade_of_the_Ruined_King", 1900));
            ItemList.Add(new BuyingOrder(13, (int) ItemId.Giants_Belt, "Giants Belt", 1435));
            ItemList.Add(new BuyingOrder(14, 1052, "Blade_of_the_Ruined_King", 435));
            ItemList.Add(new BuyingOrder(15, 3116, "RCS", 1765));
            ItemList.Add(new BuyingOrder(18, 2139, "Elixir of Sorcery", 500));
        }

        private void SetlistKOG()
        {
            ItemList.Add(new BuyingOrder(1, 1041, "Hunter's Machete", 350));
            ItemList.Add(new BuyingOrder(2, 2031, "Refillable Potion", 150));
            ItemList.Add(new BuyingOrder(3, 3340, "Trinket", 0));
            ItemList.Add(new BuyingOrder(4, 1039, "Hunter's Talisman", 350));
            ItemList.Add(new BuyingOrder(5, 3715, "Skirmisher's Sabre", 700));
            ItemList.Add(new BuyingOrder(6, 1042, "Dagger", 350));
            ItemList.Add(new BuyingOrder(7, 1419, "Bloodrazor", 2125));
            ItemList.Add(new BuyingOrder(8, 3006, "攻速鞋", 1100));
            ItemList.Add(new BuyingOrder(9, 1037, "十字", 875));
            ItemList.Add(new BuyingOrder(10, 3124, "怒", 2725));
            ItemList.Add(new BuyingOrder(11, 3086, "叉子", 1300));
            ItemList.Add(new BuyingOrder(12, 3085, "hurrican", 1300));
            ItemList.Add(new BuyingOrder(13, (int) ItemId.Goredrinker, "弯刀", 1650));
            ItemList.Add(new BuyingOrder(14, (int) ItemId.Blade_of_the_Ruined_King, "破败", 1750));
            ItemList.Add(new BuyingOrder(15, 3044, "Phage", 950));
            ItemList.Add(new BuyingOrder(16, 3071, "tbc", 1950));
            ItemList.Add(new BuyingOrder(17, 2140, "嗑药", 500));
        }

        private void SetlistManwang()
        {
            ItemList.Add(new BuyingOrder(1, 1041, "打野刀", 350));
            ItemList.Add(new BuyingOrder(2, 2031, "药水", 150));
            ItemList.Add(new BuyingOrder(3, 3340, "饰品", 0));
            ItemList.Add(new BuyingOrder(4, 1039, "打野刀2", 350));
            ItemList.Add(new BuyingOrder(5, 3715, "合成打野", 700));
            ItemList.Add(new BuyingOrder(6, 1042, "攻速1", 350));
            ItemList.Add(new BuyingOrder(7, 1419, "攻速打野", 1350));
            ItemList.Add(new BuyingOrder(8, (int) ItemId.Mobility_Boots, "鞋", 475));
            ItemList.Add(new BuyingOrder(9, 3006, "攻速鞋", 800));
            ItemList.Add(new BuyingOrder(10, (int) ItemId.Goredrinker, "弯刀", 1650));
            ItemList.Add(new BuyingOrder(11, (int) ItemId.Blade_of_the_Ruined_King, "破败", 1750));
            ItemList.Add(new BuyingOrder(12, 1038, "男票<3刀", 1300));
            ItemList.Add(new BuyingOrder(13, 1037, "十字", 875));
            ItemList.Add(new BuyingOrder(14, 1018, "衣服", 800));
            ItemList.Add(new BuyingOrder(15, 3031, "暴击", 625));
            ItemList.Add(new BuyingOrder(16, (int) ItemId.Recurve_Bow, "弓", 1000));
            ItemList.Add(new BuyingOrder(17, 1026, "棒", 850));
            ItemList.Add(new BuyingOrder(18, 3124, "怒", 1750));
            ItemList.Add(new BuyingOrder(19, 3086, "叉子", 1300));
            ItemList.Add(new BuyingOrder(20, 3087, "电", 1000));
            ItemList.Add(new BuyingOrder(21, 2140, "嗑药", 500));
        }

        private void SetlistUD()
        {
            ItemList.Add(new BuyingOrder(1, 1039, "Hunter's Talisman", 350));
            ItemList.Add(new BuyingOrder(2, 2031, "Refillable Potion", 150));
            ItemList.Add(new BuyingOrder(3, 3340, "Trinket", 0));
            ItemList.Add(new BuyingOrder(4, 1041, "Hunter's Machete", 350));
            ItemList.Add(new BuyingOrder(5, 3706, "Stalker's Blade", 700));
            ItemList.Add(new BuyingOrder(6, 3113, "Aether Wisp", 850));
            ItemList.Add(new BuyingOrder(7, 1402, "Runic Echoes", 1350));
            ItemList.Add(new BuyingOrder(8, 3009, "BoS", 900));
            ItemList.Add(new BuyingOrder(9, 3025, "ICE", 2750));
            ItemList.Add(new BuyingOrder(10, 3124, "怒", 3700));
            ItemList.Add(new BuyingOrder(11, 3742, "Dead Man's Plate", 2600));
            ItemList.Add(new BuyingOrder(12, 3211, "Spectres Cow", 1200));
            ItemList.Add(new BuyingOrder(13, 3065, "Spirit Visage", 1600));
            ItemList.Add(new BuyingOrder(14, 2138, "嗑药", 500));
        }

        private void SetListASMANA()
        {
            ItemList.Add(new BuyingOrder(1, 1041, "Hunter's Machete", 350));
            ItemList.Add(new BuyingOrder(2, 2031, "Refillable Potion", 150));
            ItemList.Add(new BuyingOrder(3, 3340, "Trinket", 0));
            ItemList.Add(new BuyingOrder(4, 1039, "Hunter's Talisman", 350));
            ItemList.Add(new BuyingOrder(5, 3715, "Skirmisher's Sabre", 700));
            ItemList.Add(new BuyingOrder(6, 1042, "Dagger", 350));
            ItemList.Add(new BuyingOrder(7, 1419, "Bloodrazor", 1350));
            ItemList.Add(new BuyingOrder(8, (int) ItemId.Mobility_Boots, "Boots of Speed", 475));
            ItemList.Add(new BuyingOrder(9, (int) ItemId.Tear_of_the_Goddess, "Tear of the Goddess", 750));
            ItemList.Add(new BuyingOrder(10, (int) ItemId.Boots_of_Swiftness, "Boots of Swoftness", 500));
            ItemList.Add(new BuyingOrder(11, (int) ItemId.Goredrinker, "Bilgewater Cutlass", 1650));
            ItemList.Add(new BuyingOrder(12, (int) ItemId.Blade_of_the_Ruined_King, "BOTRK", 1750));
            ItemList.Add(new BuyingOrder(13, (int) ItemId.Tiamat, "Tiamat", 1250));
            ItemList.Add(new BuyingOrder(14, (int) ItemId.Vampiric_Scepter, "Vampiric_Screpter", 900));
            ItemList.Add(new BuyingOrder(15, (int) ItemId.Ravenous_Hydra, "Ravenous Hydra Melee Only", 1450));
            ItemList.Add(new BuyingOrder(16, (int) ItemId.Manamune, "Ravenous Hydra Melee Only", 1650));
            ItemList.Add(new BuyingOrder(17, (int) ItemId.Zeal, "Zeal", 1300));
            ItemList.Add(new BuyingOrder(18, (int) ItemId.Phantom_Dancer, "Phantom Dancer", 1500));
        }

        private void SetlistBro()
        {
            ItemList.Add(new BuyingOrder(1, 1041, "打野刀", 350));
            ItemList.Add(new BuyingOrder(2, 2031, "Refillable Potion", 150));
            ItemList.Add(new BuyingOrder(3, 3340, "Trinket", 0));
            ItemList.Add(new BuyingOrder(4, 1039, "Hunter's Talisman", 350));
            ItemList.Add(new BuyingOrder(5, 3706, "Stalker's Blade", 700));
            ItemList.Add(new BuyingOrder(6, 3751, "直接合成打野", 1100));
            ItemList.Add(new BuyingOrder(7, 1401, "Cinderhulk", 625));
            ItemList.Add(new BuyingOrder(8, 1001, "Boots of Speed", 600));
            ItemList.Add(new BuyingOrder(9, 1028, "Ruby Crystal", 400));
            ItemList.Add(new BuyingOrder(10, 3044, "Phage", 950));
            ItemList.Add(new BuyingOrder(11, 3133, "warhamm", 1100));
            ItemList.Add(new BuyingOrder(12, 3071, "tbc", 750));
            ItemList.Add(new BuyingOrder(13, 1031, "Chain Vest", 800));
            ItemList.Add(new BuyingOrder(14, 3742, "Dead Man's Plate", 1800));
            ItemList.Add(new BuyingOrder(15, 3009, "Boots of Swiftness", 600));
            ItemList.Add(new BuyingOrder(16, 3052, "Jaurims Fist", 1300));
            ItemList.Add(new BuyingOrder(17, 3077, "Tiamat", 1300));
            ItemList.Add(new BuyingOrder(18, 3748, "Titanic Hydra", 1100));
            ItemList.Add(new BuyingOrder(19, 3211, "Spectres Cow", 1200));
            ItemList.Add(new BuyingOrder(20, 3065, "Spirit Visage", 1600));
            ItemList.Add(new BuyingOrder(21, 2138, "Elixir of Iron", 500));
        }

        private void SetListAD()
        {
            //ItemList.Add(new BuyingOrder());
        }

        private void SetListAS()
        {
            ItemList.Add(new BuyingOrder(1, 1041, "Hunter's Machete", 350));
            ItemList.Add(new BuyingOrder(2, 2031, "Refillable Potion", 150));
            ItemList.Add(new BuyingOrder(3, 3340, "Trinket", 0));
            ItemList.Add(new BuyingOrder(4, 1039, "Hunter's Talisman", 350));
            ItemList.Add(new BuyingOrder(5, 3715, "Skirmisher's Sabre", 700));
            ItemList.Add(new BuyingOrder(6, 1042, "Dagger", 350));
            ItemList.Add(new BuyingOrder(7, 1419, "Bloodrazor", 1350));
            ItemList.Add(new BuyingOrder(8, (int) ItemId.Mobility_Boots, "Boots of Speed", 475));
            ItemList.Add(new BuyingOrder(9, (int) ItemId.Boots_of_Swiftness, "Boots of Swoftness", 500));
            ItemList.Add(new BuyingOrder(10, (int) ItemId.Goredrinker, "Bilgewater Cutlass", 1650));
            ItemList.Add(new BuyingOrder(11, (int) ItemId.Blade_of_the_Ruined_King, "BOTRK", 1750));
            ItemList.Add(new BuyingOrder(12, (int) ItemId.Recurve_Bow, "Recurve Bow", 1000));
            ItemList.Add(new BuyingOrder(13, (int) ItemId.Wits_End, "Wits End", 1800));
            ItemList.Add(new BuyingOrder(14, (int) ItemId.Tiamat, "Tiamat", 1250));
            ItemList.Add(new BuyingOrder(15, (int) ItemId.Vampiric_Scepter, "Vampiric_Screpter", 900));
            ItemList.Add(new BuyingOrder(16, (int) ItemId.Ravenous_Hydra, "Ravenous Hydra Melee Only", 1450));
            ItemList.Add(new BuyingOrder(17, (int) ItemId.Zeal, "Zeal", 1200));
            ItemList.Add(new BuyingOrder(18, (int) ItemId.Phantom_Dancer, "Phantom Dancer", 1500));
        }

        private void SetListNOC()
        {
            ItemList.Add(new BuyingOrder(1, 1041, "Hunter's Machete", 350));
            ItemList.Add(new BuyingOrder(2, 2031, "Refillable Potion", 150));
            ItemList.Add(new BuyingOrder(3, 3340, "Trinket", 0));
            ItemList.Add(new BuyingOrder(4, 1039, "Hunter's Talisman", 350));
            ItemList.Add(new BuyingOrder(5, 3715, "Skirmisher's Sabre", 700));
            ItemList.Add(new BuyingOrder(6, 1042, "Dagger", 350));
            ItemList.Add(new BuyingOrder(7, 1419, "Bloodrazor", 1350));
            ItemList.Add(new BuyingOrder(8, (int) ItemId.Mobility_Boots, "Boots of Speed", 475));
            ItemList.Add(new BuyingOrder(9, (int) ItemId.Boots_of_Swiftness, "Boots of Swoftness", 500));
            ItemList.Add(new BuyingOrder(10, (int) ItemId.Goredrinker, "Bilgewater Cutlass", 1650));
            ItemList.Add(new BuyingOrder(11, (int) ItemId.Blade_of_the_Ruined_King, "BOTRK", 1750));
            ItemList.Add(new BuyingOrder(12, (int) ItemId.Giants_Belt, "Giants Belt", 1000));
            ItemList.Add(new BuyingOrder(13, (int) ItemId.Chain_Vest, "Chain Vest", 800));
            ItemList.Add(new BuyingOrder(14, 3742, "Dead Mans Plate", 1800));
            ItemList.Add(new BuyingOrder(15, (int) ItemId.Tiamat, "Tiamat", 1250));
            ItemList.Add(new BuyingOrder(16, (int) ItemId.Vampiric_Scepter, "Vampiric_Screpter", 900));
            ItemList.Add(new BuyingOrder(17, (int) ItemId.Ravenous_Hydra, "Ravenous Hydra Melee Only", 1450));
            ItemList.Add(new BuyingOrder(18, (int) ItemId.Zeal, "Zeal", 1200));
            ItemList.Add(new BuyingOrder(19, (int) ItemId.Phantom_Dancer, "Phantom Dancer", 1350));
            ItemList.Add(new BuyingOrder(14, 2140, "Elixir of Wrath", 500));
        }

        private void SetListAPADT()
        {
            ItemList.Add(new BuyingOrder(1, 1041, "Hunter's Machete", 350));
            ItemList.Add(new BuyingOrder(2, 2031, "Refillable Potion", 150));
            ItemList.Add(new BuyingOrder(3, 3340, "Trinket", 0));
            ItemList.Add(new BuyingOrder(4, 1039, "Hunter's Talisman", 350));
            ItemList.Add(new BuyingOrder(5, 3706, "Stalker's Blade", 700));
            ItemList.Add(new BuyingOrder(6, 3113, "Aether Wisp", 850));
            ItemList.Add(new BuyingOrder(7, 1402, "Runic Echoes", 1350));
            ItemList.Add(new BuyingOrder(8, 1001, "Boots of Speed", 750));
            ItemList.Add(new BuyingOrder(9, 3111, "Mercurys Treads", 400));
            ItemList.Add(new BuyingOrder(10, 3052, "Jaurims Fist", 1300));
            ItemList.Add(new BuyingOrder(11, 3077, "Tiamat", 1300));
            ItemList.Add(new BuyingOrder(12, 3748, "Titanic Hydra", 1100));
            ItemList.Add(new BuyingOrder(13, (int) ItemId.Giants_Belt, "Giants Belt", 1000));
            ItemList.Add(new BuyingOrder(14, (int) ItemId.Chain_Vest, "Chain Vest", 800));
            ItemList.Add(new BuyingOrder(15, 3742, "Dead Mans Plate", 1800));
            ItemList.Add(new BuyingOrder(16, 3211, "Spectres Cow", 1200));
            ItemList.Add(new BuyingOrder(17, 3102, "Banshees Veil", 1250));
            ItemList.Add(new BuyingOrder(18, (int) ItemId.Goredrinker, "Bilgewater Cutlass", 1650));
            ItemList.Add(new BuyingOrder(19, (int) ItemId.Blade_of_the_Ruined_King, "BOTRK", 1750));
            ItemList.Add(new BuyingOrder(20, 2140, "Elixir of Wrath", 500));
            //add heal spell useage and keep buy Elixir of Wrath after full item
        }

        private void SetListEVE()
        {
            ItemList.Add(new BuyingOrder(1, 1041, "Hunter's Machete", 350));
            ItemList.Add(new BuyingOrder(2, 2031, "Refillable Potion", 150));
            ItemList.Add(new BuyingOrder(3, 3340, "Trinket", 0));
            ItemList.Add(new BuyingOrder(4, 1039, "Hunter's Talisman", 350));
            ItemList.Add(new BuyingOrder(5, 3706, "Stalker's Blade", 700));
            ItemList.Add(new BuyingOrder(6, 3113, "Aether Wisp", 850));
            ItemList.Add(new BuyingOrder(7, 1402, "Runic Echoes", 1350));
            ItemList.Add(new BuyingOrder(8, 1001, "Boots of Speed", 400));
            ItemList.Add(new BuyingOrder(9, 3158, "Ionian Boots of Lucidity", 600));
            ItemList.Add(new BuyingOrder(10, (int) ItemId.Giants_Belt, "Giants Belt", 1000));
            ItemList.Add(new BuyingOrder(11, 3116, "Rylais Crystal Scepter", 2250));
            ItemList.Add(new BuyingOrder(12, 1052, "宝典", 435));
            ItemList.Add(new BuyingOrder(13, 3285, "Ludens Echo", 2765));
            ItemList.Add(new BuyingOrder(14, 1026, "Blasting Wand", 850));
            ItemList.Add(new BuyingOrder(15, 3135, "Void Staf", 1800));
            ItemList.Add(new BuyingOrder(16, 1058, "Needlessly Large Rod", 1250));
            ItemList.Add(new BuyingOrder(17, 3089, "Rabadons Deathcap", 2500));
            ItemList.Add(new BuyingOrder(18, 2139, "Elixir of Sorcery", 500));
            //出装顺序不好，下次再改
        }

        private void SetlistVB()
        {
            ItemList.Add(new BuyingOrder(1, 1041, "打野刀", 350));
            ItemList.Add(new BuyingOrder(2, 2031, "Refillable Potion", 150));
            ItemList.Add(new BuyingOrder(3, 3340, "Trinket", 0));
            ItemList.Add(new BuyingOrder(4, 1039, "Hunter's Talisman", 350));
            ItemList.Add(new BuyingOrder(5, 3706, "Stalker's Blade", 700));
            ItemList.Add(new BuyingOrder(6, 3751, "直接合成打野", 1100));
            ItemList.Add(new BuyingOrder(7, 1401, "Cinderhulk", 625));
            ItemList.Add(new BuyingOrder(8, 1001, "Boots of Speed", 600));
            ItemList.Add(new BuyingOrder(9, 1028, "Ruby Crystal", 400));
            ItemList.Add(new BuyingOrder(10, 1031, "Chain Vest", 800));
            ItemList.Add(new BuyingOrder(11, 3742, "Dead Man's Plate", 1800));
            ItemList.Add(new BuyingOrder(12, 3009, "Boots of Swiftness", 600));
            ItemList.Add(new BuyingOrder(13, 3211, "Spectres Cow", 1200));
            ItemList.Add(new BuyingOrder(14, 3065, "Spirit Visage", 1600));
            ItemList.Add(new BuyingOrder(15, 3052, "Jaurims Fist", 1300));
            ItemList.Add(new BuyingOrder(16, 3077, "Tiamat", 1300));
            ItemList.Add(new BuyingOrder(17, 3748, "Titanic Hydra", 1100));
            ItemList.Add(new BuyingOrder(18, (int) ItemId.Giants_Belt, "Giants Belt", 1000));
            ItemList.Add(new BuyingOrder(19, 3143, "Randuin's Omen", 1900));
            ItemList.Add(new BuyingOrder(20, 2138, "Elixir of Iron", 500));
        }

        private void SetlistYI()
        {
            ItemList.Add(new BuyingOrder(1, 1041, "Hunter's Machete", 350));
            ItemList.Add(new BuyingOrder(2, 2031, "Refillable Potion", 150));
            ItemList.Add(new BuyingOrder(3, 3340, "Trinket", 0));
            ItemList.Add(new BuyingOrder(4, 1039, "Hunter's Talisman", 350));
            ItemList.Add(new BuyingOrder(5, 3715, "Skirmisher's Blade", 700));
            ItemList.Add(new BuyingOrder(6, 1042, "Dagger", 1050));
            ItemList.Add(new BuyingOrder(7, (int) ItemId.Recurve_Bow, "弓", 700));
            ItemList.Add(new BuyingOrder(8, 1419, "Bloodrazor", 1600));
            ItemList.Add(new BuyingOrder(9, (int) ItemId.Mobility_Boots, "Boots of Speed", 1500));
            ItemList.Add(new BuyingOrder(10, 3117, "BoM", 600));
            ItemList.Add(new BuyingOrder(11, (int) ItemId.Tiamat, "Tiamat", 1250));
            ItemList.Add(new BuyingOrder(12, (int) ItemId.Ravenous_Hydra, "Ravenous Hydra Melee Only", 2350));
            ItemList.Add(new BuyingOrder(13, (int) ItemId.Goredrinker, "Bilgewater Cutlass", 1650));
            ItemList.Add(new BuyingOrder(14, (int) ItemId.Blade_of_the_Ruined_King, "BOTRK", 1750));
            ItemList.Add(new BuyingOrder(15, 3086, "zeal", 1300));
            ItemList.Add(new BuyingOrder(16, 3046, "PD", 1250));
            ItemList.Add(new BuyingOrder(17, (int) ItemId.Giants_Belt, "Giants Belt", 1000));
            ItemList.Add(new BuyingOrder(18, 3742, "Dead Mans Plate", 2600));
            ItemList.Add(new BuyingOrder(19, 2140, "Elixir of Wrath", 500));
        }

        private void SetlistNu()
        {
            ItemList.Add(new BuyingOrder(1, 1041, "Hunter's Machete", 350));
            ItemList.Add(new BuyingOrder(2, 2031, "Refillable Potion", 150));
            ItemList.Add(new BuyingOrder(3, 3340, "Trinket", 0));
            ItemList.Add(new BuyingOrder(4, 1039, "Hunter's Talisman", 350));
            ItemList.Add(new BuyingOrder(5, 3706, "Stalker's Blade", 700));
            ItemList.Add(new BuyingOrder(6, 3113, "Aether Wisp", 850));
            ItemList.Add(new BuyingOrder(7, 1402, "Runic Echoes", 1350));
            ItemList.Add(new BuyingOrder(8, 1001, "Boots of Speed", 1600));
            ItemList.Add(new BuyingOrder(9, 3158, "Ionian Boots of Lucidity", 600));
            ItemList.Add(new BuyingOrder(10, 3024, "Glacial Shroud", 1000));
            ItemList.Add(new BuyingOrder(11, 3110, "Frozen Heart", 2000));
            ItemList.Add(new BuyingOrder(12, 1052, "宝典", 435));
            ItemList.Add(new BuyingOrder(13, 3285, "Ludens Echo", 2765));
            ItemList.Add(new BuyingOrder(14, 1026, "Blasting Wand", 850));
            ItemList.Add(new BuyingOrder(15, 3135, "Void Staf", 1800));
            ItemList.Add(new BuyingOrder(16, 1058, "Needlessly Large Rod", 1250));
            ItemList.Add(new BuyingOrder(17, 3089, "Rabadons Deathcap", 2500));
            ItemList.Add(new BuyingOrder(18, 2139, "Elixir of Sorcery", 500));
        }

        public static void UseItemsJungle()
        {
            if (Items.HasItem((int) ItemId.Tiamat) && Items.CanUseItem((int) ItemId.Tiamat) &&
                Helpers.getMobs(Program.player.Position, 400).Count > 2)
            {
                Items.UseItem((int) ItemId.Tiamat);
            }
            if (Items.HasItem((int) ItemId.Ravenous_Hydra) &&
                Items.CanUseItem((int) ItemId.Ravenous_Hydra) &&
                Helpers.getMobs(Program.player.Position, 400).Count > 2)
            {
                Items.UseItem((int) ItemId.Ravenous_Hydra);
            }
            var muramana = LeagueSharp.Common.Data.ItemData.Muramana.GetItem().Id;
            if (Items.HasItem(muramana) && Items.CanUseItem(muramana) && Program.player.HasBuff("Muramana"))
            {
                Items.UseItem(muramana);
            }
        }

        public static void UseItemsCombo(Obj_AI_Base target, bool use)
        {
            if (Items.HasItem((int) ItemId.Tiamat) && Items.CanUseItem((int) ItemId.Tiamat) &&
                target.Distance(Program.player) < 400)
            {
                Items.UseItem((int) ItemId.Tiamat);
            }
            if (Items.HasItem((int) ItemId.Ravenous_Hydra) &&
                Items.CanUseItem((int) ItemId.Ravenous_Hydra) && target.Distance(Program.player) < 400)
            {
                Items.UseItem((int) ItemId.Ravenous_Hydra);
            }
            if (!use)
            {
                return;
            }
            if (Items.HasItem((int) ItemId.Goredrinker) && Items.CanUseItem((int) ItemId.Goredrinker) &&
                (target.Distance(Program.player) > Orbwalking.GetRealAutoAttackRange(target) ||
                 (target.HealthPercent < 35 && Program.player.HealthPercent < 35)))
            {
                Items.UseItem((int) ItemId.Goredrinker, target);
            }
            if (Items.HasItem((int) ItemId.Blade_of_the_Ruined_King) &&
                Items.CanUseItem((int) ItemId.Blade_of_the_Ruined_King) &&
                (target.Distance(Program.player) > Orbwalking.GetRealAutoAttackRange(target) ||
                 (target.HealthPercent < 35 && Program.player.HealthPercent < 35)))
            {
                Items.UseItem((int) ItemId.Blade_of_the_Ruined_King, target);
            }
            var muramana = LeagueSharp.Common.Data.ItemData.Muramana.GetItem().Id;
            if (Items.HasItem(muramana) && Items.CanUseItem(muramana))
            {
                if (!Program.player.HasBuff("Muramana") && Program.player.Mana > 250)
                {
                    Items.UseItem(muramana);
                }
                else if (Program.player.HasBuff("Muramana") && Program.player.Mana < 260)
                {
                    Items.UseItem(muramana);
                }
            }
        }
    }

    internal class BuyingOrder
    {
        public int Index;
        public int ItemId;
        public string Name;
        public int Price;

        public BuyingOrder(int idx, int itemid, string name, int price)
        {
            Index = idx;
            ItemId = itemid;
            Name = name;
            Price = price;
        }
    }

    internal enum BuildType
    {
        AS,
        AD,
        ASMANA,
        NOC,
        EVE,
        APADT,
        VB,
        Manwang,
        YI,
        Bro,
        Nu,
        UD,
        Kayle,
        KOG
    }
}
