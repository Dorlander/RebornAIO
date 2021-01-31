using System.Collections.Generic;

namespace xSaliceReligionAIO
{
    class Initiator
    {
        public string HeroName { get; set; }
        public string SpellName { get; set; }
        public string SDataName { get; set; }

        public static List<Initiator> InitatorList = new List<Initiator>();

        static Initiator()
        {
            //aatrox
            InitatorList.Add(new Initiator
            {
                HeroName = "Aatrox",
                SpellName = "Aatrox Q",
                SDataName = "AatroxQ"
            });

            //alistar
            InitatorList.Add(new Initiator
            {
                HeroName = "Alistar",
                SpellName = "Alistar W",
                SDataName = "Headbutt"
            });

            //amumu
            InitatorList.Add(new Initiator
            {
                HeroName = "Amumu",
                SpellName = "Amumu Q",
                SDataName = "BandageToss"
            });

            //elise
            InitatorList.Add(new Initiator
            {
                HeroName = "Elise",
                SpellName = "Elise Spider E",
                SDataName = "elisespideredescent"
            });

            //fid
            InitatorList.Add(new Initiator
            {
                HeroName = "FiddleSticks",
                SpellName = "FiddleSticks R",
                SDataName = "Crowstorm"
            });

            //fiora
            InitatorList.Add(new Initiator
            {
                HeroName = "Fiora",
                SpellName = "Fiora Q",
                SDataName = "FioraQ"
            });

            //gragas
            InitatorList.Add(new Initiator
            {
                HeroName = "Gragas",
                SpellName = "Gragas Q",
                SDataName = "GragasE"
            });

            //Hecarim
            InitatorList.Add(new Initiator
            {
                HeroName = "Hecarim",
                SpellName = "Hecarim R",
                SDataName = "HecarimUlt"
            });

            //Irelia
            InitatorList.Add(new Initiator
            {
                HeroName = "Irelia",
                SpellName = "Irelia Q",
                SDataName = "IreliaGatotsu"
            });

            //JarvanIV
            InitatorList.Add(new Initiator
            {
                HeroName = "JarvanIV",
                SpellName = "JarvanIV EQ",
                SDataName = "JarvanIVDragonStrike"
            });


            InitatorList.Add(new Initiator
            {
                HeroName = "JarvanIV",
                SpellName = "JarvanIV R",
                SDataName = "JarvanIVCataclysm"
            });

            //Jax
            InitatorList.Add(new Initiator
            {
                HeroName = "Jax",
                SpellName = "Jax Q",
                SDataName = "JaxLeapStrike"
            });

            //Jayce
            InitatorList.Add(new Initiator
            {
                HeroName = "Jayce",
                SpellName = "Jayce Q",
                SDataName = "JayceToTheSkies"
            });

            //Kassadin
            InitatorList.Add(new Initiator
            {
                HeroName = "Kassadin",
                SpellName = "Kassadin Q",
                SDataName = "RiftWalk"
            });

            //Katarina
            InitatorList.Add(new Initiator
            {
                HeroName = "Katarina",
                SpellName = "Katarina E",
                SDataName = "KatarinaE"
            });

            //Khazix
            InitatorList.Add(new Initiator
            {
                HeroName = "Khazix",
                SpellName = "Khazix E",
                SDataName = "KhazixE"
            });


            InitatorList.Add(new Initiator
            {
                HeroName = "Khazix",
                SpellName = "Khazix E(Evo)",
                SDataName = "khazixelong"
            });

            //Leblanc
            InitatorList.Add(new Initiator
            {
                HeroName = "Leblanc",
                SpellName = "Leblanc W",
                SDataName = "LeblancSlide"
            });

            //LeeSin
            InitatorList.Add(new Initiator
            {
                HeroName = "LeeSin",
                SpellName = "LeeSin 2nd Q",
                SDataName = "blindmonkqtwo"
            });

            //Leona
            InitatorList.Add(new Initiator
            {
                HeroName = "Leona",
                SpellName = "Leona E",
                SDataName = "LeonaZenithBladeMissle"
            });

            //Lissandra
            InitatorList.Add(new Initiator
            {
                HeroName = "Lissandra",
                SpellName = "Lissandra E",
                SDataName = "LissandraE"
            });

            //Malphite
            InitatorList.Add(new Initiator
            {
                HeroName = "Malphite",
                SpellName = "Malphite R",
                SDataName = "UFSlash"
            });

            //Maokai
            InitatorList.Add(new Initiator
            {
                HeroName = "Maokai",
                SpellName = "Maokai W",
                SDataName = "MaokaiUnstableGrowth"
            });

            //MonkeyKing
            InitatorList.Add(new Initiator
            {
                HeroName = "MonkeyKing",
                SpellName = "MonkeyKing E",
                SDataName = "MonkeyKingNimbus"
            });


            InitatorList.Add(new Initiator
            {
                HeroName = "MonkeyKing",
                SpellName = "MonkeyKing R",
                SDataName = "MonkeyKingSpinToWin"
            });

            //Nocturne
            InitatorList.Add(new Initiator
            {
                HeroName = "Nocturne",
                SpellName = "Nocturne R",
                SDataName = "NocturneParanoia"
            });

            //Renekton
            InitatorList.Add(new Initiator
            {
                HeroName = "Renekton",
                SpellName = "Renekton E",
                SDataName = "RenektonSliceAndDice"
            });

            //Rengar
            InitatorList.Add(new Initiator
            {
                HeroName = "Rengar",
                SpellName = "Rengar R",
                SDataName = "RengarR"
            });

            //Rengar
            InitatorList.Add(new Initiator
            {
                HeroName = "Rengar",
                SpellName = "Rengar R",
                SDataName = "RengarR"
            });

            //Sejuani
            InitatorList.Add(new Initiator
            {
                HeroName = "Sejuani",
                SpellName = "Sejuani Q",
                SDataName = "SejuaniArcticAssault"
            });

            //Shaco
            InitatorList.Add(new Initiator
            {
                HeroName = "Shaco",
                SpellName = "Shaco Q",
                SDataName = "Deceive"
            });

            //Shen
            InitatorList.Add(new Initiator
            {
                HeroName = "Shen",
                SpellName = "Shen E",
                SDataName = "ShenShadowDash"
            });

            //Shyvana
            InitatorList.Add(new Initiator
            {
                HeroName = "Shyvana",
                SpellName = "Shyvana R",
                SDataName = "ShyvanaTransformCast"
            });

            //Talon
            InitatorList.Add(new Initiator
            {
                HeroName = "Talon",
                SpellName = "Talon E",
                SDataName = "TalonCutthroat"
            });

            //Thresh
            InitatorList.Add(new Initiator
            {
                HeroName = "Thresh",
                SpellName = "Thresh Q",
                SDataName = "threshqleap"
            });

            //Tristana
            InitatorList.Add(new Initiator
            {
                HeroName = "Tristana",
                SpellName = "Tristana W",
                SDataName = "RocketJump"
            });

            //Tryndamere
            InitatorList.Add(new Initiator
            {
                HeroName = "Tryndamere",
                SpellName = "Tryndamere E",
                SDataName = "slashCast"
            });

            //Twitch
            InitatorList.Add(new Initiator
            {
                HeroName = "Twitch",
                SpellName = "Twitch Q",
                SDataName = "HideInShadows"
            });

            //vi
            InitatorList.Add(new Initiator
            {
                HeroName = "Vi",
                SpellName = "Vi Q",
                SDataName = "ViQ"
            });

            InitatorList.Add(new Initiator
            {
                HeroName = "Vi",
                SpellName = "Vi R",
                SDataName = "ViR"
            });

            //Volibear
            InitatorList.Add(new Initiator
            {
                HeroName = "Volibear",
                SpellName = "Volibear Q",
                SDataName = "VolibearQ"
            });

            //Xin Zhao
            InitatorList.Add(new Initiator
            {
                HeroName = "Xin Zhao",
                SpellName = "Xin Zhao E",
                SDataName = "XenZhaoSweep"
            });

            //Xin Zac
            InitatorList.Add(new Initiator
            {
                HeroName = "Zac",
                SpellName = "Zac E",
                SDataName = "ZacE"
            });

        }
    }
}
