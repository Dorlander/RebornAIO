using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ShineCommon
{
    [Flags]
    public enum EvadeMethods
    {
        Default = 0,
        EzrealE = 1,
        SivirE = 2,
        MorganaE = 4,
        KayleR = 16,
        FizzE = 32,
        LissandraR = 64,
        NocturneW = 128,
        VladimirW = 256,
        ZedW = 512,
        QSS = 1024,
        None = 2048,
    }

    [Flags]
    public enum Collisions
    {
        None = 0,
        Minions = 1,
        Champions = 2,
        YasuoWall = 4,
    }

    public struct ArcData
    {
        public float Width;
        public float Height;
        public float Radius;
        public Vector2 Pos;
        public float Angle;
    }

    public class SpellData
    {
        public string ChampionName;
        public string SpellName;
        public SpellSlot Slot;
        public bool IsArc;
        public ArcData ArcData;
        public bool IsSkillshot;
        public SkillshotType Type;
        public int Delay;
        public int Range;
        public int Radius;
        public int MissileSpeed;
        public bool IsDangerous;
        public string MissileSpellName;
        public EvadeMethods EvadeMethods;
        public Collisions Collisionable;
    }

    public class MovementBuffSpellData : SpellData
    {
        public float[] Percent;
        public float[] Extra;
        public bool IsDecaying;
        public float DecaysTo;
        public float DecayTime;
    }

    public class EscapeSpellData : SpellData
    {

    }

    public class SpellDatabase
    {
        public static List<SpellData> EvadeableSpells;
        public static List<MovementBuffSpellData> MovementBuffers;

        public static void InitalizeSpellDatabase()
        {
            EvadeableSpells = new List<SpellData>();
            #region Dangreous Spell Database
            //diana q x axis eliptic radius 315
            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Ahri",
                    SpellName = "AhriSeduce",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    Radius = 60,
                    MissileSpeed = 1550,
                    IsDangerous = true,
                    MissileSpellName = "AhriSeduceMissile",
                    EvadeMethods =  EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS | EvadeMethods.ZedW,
                    Collisionable = Collisions.Champions | Collisions.Minions | Collisions.YasuoWall 
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Amumu",
                    SpellName = "BandageToss",
                    Slot = SpellSlot.Q,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 90,
                    MissileSpeed = 2000,
                    IsDangerous = true,
                    MissileSpellName = "SadMummyBandageToss",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS | EvadeMethods.ZedW,
                    Collisionable = Collisions.Champions | Collisions.Minions | Collisions.YasuoWall

                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Amumu",
                    SpellName = "CurseoftheSadMummy",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 0,
                    Radius = 550,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Annie",
                    SpellName = "InfernalGuardian",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 600,
                    Radius = 251,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Ashe",
                    SpellName = "EnchantedCrystalArrow",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 20000,
                    Radius = 130,
                    MissileSpeed = 1600,
                    IsDangerous = true,
                    MissileSpellName = "EnchantedCrystalArrow",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS | EvadeMethods.ZedW,
                    Collisionable = Collisions.Champions | Collisions.YasuoWall
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Bard",
                    SpellName = "BardR",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 500,
                    Range = 3400,
                    Radius = 350,
                    MissileSpeed = 2100,
                    IsDangerous = false,
                    MissileSpellName = "BardR",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Blitzcrank",
                    SpellName = "RocketGrab",
                    Slot = SpellSlot.Q,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1050,
                    Radius = 70,
                    MissileSpeed = 1800,
                    IsDangerous = true,
                    MissileSpellName = "RocketGrabMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.ZedW,
                    Collisionable = Collisions.Champions | Collisions.YasuoWall
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Braum",
                    SpellName = "BraumRWrapper",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 1200,
                    Radius = 115,
                    MissileSpeed = 1400,
                    IsDangerous = true,
                    MissileSpellName = "braumrmissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE,
                    Collisionable = Collisions.YasuoWall
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Diana",
                    SpellName = "DianaArc",
                    Slot = SpellSlot.Q,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 895,
                    Radius = 195,
                    IsArc = true,
                    ArcData = new ArcData
                        {
                            Pos = new Vector2(875 / 2f, 20),
                            Angle = (float)Math.PI,
                            Width = 410,
                            Height = 200,
                            Radius = 120
                        },
                    MissileSpeed = 1600,
                    IsDangerous = true,
                    MissileSpellName = "DianaArc",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE,
                    Collisionable = Collisions.YasuoWall
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Draven",
                    SpellName = "DravenDoubleShot",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 130,
                    MissileSpeed = 1400,
                    IsDangerous = true,
                    MissileSpellName = "DravenDoubleShotMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE,
                    Collisionable = Collisions.YasuoWall
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Elise",
                    SpellName = "EliseHumanE",
                    IsSkillshot = true,
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 55,
                    MissileSpeed = 1600,
                    IsDangerous = true,
                    MissileSpellName = "EliseHumanE",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.ZedW,
                    Collisionable = Collisions.YasuoWall
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Evelynn",
                    SpellName = "EvelynnR",
                    IsSkillshot = true,
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 650,
                    Radius = 350,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "EvelynnR",
                    EvadeMethods =  EvadeMethods.EzrealE | EvadeMethods.SivirE
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Cassiopeia",
                    SpellName = "CassiopeiaPetrifyingGaze",
                    IsSkillshot = true,
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCone,
                    Delay = 600,
                    Range = 825,
                    Radius = 80,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "CassiopeiaPetrifyingGaze",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.ZedW
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Fizz",
                    SpellName = "FizzMarinerDoom",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1300,
                    Radius = 120,
                    MissileSpeed = 1350,
                    IsDangerous = true,
                    MissileSpellName = "FizzMarinerDoomMissile",
                    Collisionable = Collisions.Champions | Collisions.YasuoWall,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.ZedW
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Galio",
                    SpellName = "GalioIdolOfDurand",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 0,
                    Radius = 550,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Gnar",
                    SpellName = "GnarR",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 0,
                    Radius = 500,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Gragas",
                    SpellName = "GragasE",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 0,
                    Range = 950,
                    Radius = 200,
                    MissileSpeed = 1200,
                    IsDangerous = false,
                    MissileSpellName = "GragasE",
                    Collisionable = Collisions.Champions | Collisions.Minions ,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE
                });



            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Gragas",
                    SpellName = "GragasR",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 1050,
                    Radius = 375,
                    MissileSpeed = 1800,
                    IsDangerous = true,
                    MissileSpellName = "GragasRBoom",
                    Collisionable = Collisions.YasuoWall,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Graves",
                    SpellName = "GravesChargeShot",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 100,
                    MissileSpeed = 2100,
                    IsDangerous = true,
                    MissileSpellName = "GravesChargeShotShot",
                    Collisionable = Collisions.YasuoWall,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.ZedW
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Leona",
                    SpellName = "LeonaSolarFlare",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 1000,
                    Range = 1200,
                    Radius = 300,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "LeonaSolarFlare",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Leona",
                    SpellName = "LeonaZenithBlade",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 905,
                    Radius = 70,
                    MissileSpeed = 2000,
                    IsDangerous = true,
                    MissileSpellName = "LeonaZenithBladeMissile",
                    Collisionable = Collisions.YasuoWall,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS | EvadeMethods.ZedW
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Malphite",
                    SpellName = "UFSlash",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 0,
                    Range = 1000,
                    Radius = 270,
                    MissileSpeed = 1500,
                    IsDangerous = true,
                    MissileSpellName = "UFSlash",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.ZedW
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Morgana",
                    SpellName = "DarkBindingMissile",
                    Slot = SpellSlot.Q,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1300,
                    Radius = 80,
                    MissileSpeed = 1200,
                    IsDangerous = true,
                    MissileSpellName = "DarkBindingMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS | EvadeMethods.ZedW,
                    Collisionable = Collisions.Champions | Collisions.Minions | Collisions.YasuoWall
                });


            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Nami",
                    SpellName = "NamiQ",
                    Slot = SpellSlot.Q,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 950,
                    Range = 1625,
                    Radius = 150,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "namiqmissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS
                });


            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Nautilus",
                    SpellName = "NautilusAnchorDrag",
                    IsSkillshot = true,
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 90,
                    MissileSpeed = 2000,
                    IsDangerous = true,
                    MissileSpellName = "NautilusAnchorDragMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.ZedW,
                    Collisionable = Collisions.Champions | Collisions.Minions | Collisions.YasuoWall
                });


            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Rengar",
                    SpellName = "RengarE",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    Radius = 70,
                    MissileSpeed = 1500,
                    IsDangerous = true,
                    MissileSpellName = "RengarEFinal",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS | EvadeMethods.ZedW,
                    Collisionable = Collisions.Champions | Collisions.Minions | Collisions.YasuoWall
                });


            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Sona",
                    SpellName = "SonaR",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    Radius = 140,
                    MissileSpeed = 2400,
                    IsDangerous = true,
                    MissileSpellName = "SonaR",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS | EvadeMethods.ZedW,
                    Collisionable = Collisions.YasuoWall
                });



            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Swain",
                    SpellName = "SwainShadowGrasp",
                    Slot = SpellSlot.W,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 1100,
                    Range = 900,
                    Radius = 180,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "SwainShadowGrasp",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Syndra",
                    SpellName = "syndrae5",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 300,
                    Range = 950,
                    Radius = 90,
                    MissileSpeed = 1601,
                    IsDangerous = false,
                    MissileSpellName = "syndrae5",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS | EvadeMethods.ZedW
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Syndra",
                    SpellName = "SyndraE",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 300,
                    Range = 950,
                    Radius = 90,
                    MissileSpeed = 1601,
                    IsDangerous = false,
                    MissileSpellName = "SyndraE",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS | EvadeMethods.ZedW,
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Thresh",
                    SpellName = "ThreshQ",
                    Slot = SpellSlot.Q,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 1100,
                    Radius = 70,
                    MissileSpeed = 1900,
                    IsDangerous = true,
                    MissileSpellName = "ThreshQMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS | EvadeMethods.ZedW,
                    Collisionable = Collisions.Champions | Collisions.Minions | Collisions.YasuoWall
                });


            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Varus",
                    SpellName = "VarusQMissilee",
                    Slot = SpellSlot.Q,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1800,
                    Radius = 70,
                    MissileSpeed = 1900,
                    IsDangerous = false,
                    MissileSpellName = "VarusQMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.ZedW,
                    Collisionable = Collisions.YasuoWall
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Varus",
                    SpellName = "VarusR",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1200,
                    Radius = 120,
                    MissileSpeed = 1950,
                    IsDangerous = true,
                    MissileSpellName = "VarusRMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS | EvadeMethods.ZedW,
                    Collisionable = Collisions.Champions | Collisions.YasuoWall
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Velkoz",
                    SpellName = "VelkozE",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 500,
                    Range = 800,
                    Radius = 225,
                    MissileSpeed = 1500,
                    IsDangerous = false,
                    MissileSpellName = "VelkozEMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Yasuo",
                    SpellName = "yasuoq3w",
                    Slot = SpellSlot.Q,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 1150,
                    Radius = 90,
                    MissileSpeed = 1500,
                    IsDangerous = true,
                    MissileSpellName = "yasuoq3w",
                    Collisionable = Collisions.YasuoWall,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS | EvadeMethods.ZedW,
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Zyra",
                    SpellName = "ZyraGraspingRoots",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1150,
                    Radius = 70,
                    MissileSpeed = 1150,
                    IsDangerous = true,
                    MissileSpellName = "ZyraGraspingRoots",
                    Collisionable = Collisions.YasuoWall,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Zyra",
                    SpellName = "zyrapassivedeathmanager",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 1474,
                    Radius = 70,
                    MissileSpeed = 2000,
                    IsDangerous = true,
                    MissileSpellName = "zyrapassivedeathmanager",
                    Collisionable = Collisions.YasuoWall,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                });
            #endregion
            #region Escape Spell Data

            #endregion
            #region Movement Buffers
            /*
                Ahri Q
                Ekko Passive
                Gangplank Passive
                Hecarim E
                Karma E
                Karma R + E
                Kennen E
                Olaf R
                Lulu W
             */
            MovementBuffers = new List<MovementBuffSpellData>();
            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Ahri",
                SpellName = "Orb of Deception",
                Slot = SpellSlot.Q,
                Extra = new float[] { 215 },
                IsDecaying = true,
                DecayTime = 0.5f,
                DecaysTo = 80,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Bard",
                SpellName = "bardwspeedboost",
                Slot = SpellSlot.W,
                Percent = new float[] { 50 },
                IsDecaying = true,
                DecayTime = 1.5f
            });
            
            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Blitzcrank",
                SpellName = "Overdrive",
                Slot = SpellSlot.W,
                Percent = new float[] { 70, 75, 80, 85, 90 },
                IsDecaying = true,
                DecayTime = 0.5f,
                DecaysTo = 10,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Ekko",
                SpellName = "Z-Drive Resonance",
                Slot = SpellSlot.Unknown,
                Percent = new float[] { 40, 50, 60, 70, 80 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Evelynn",
                SpellName = "EvelynnW",
                Slot = SpellSlot.W,
                Percent = new float[] { 30, 40, 50, 60, 70 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Gangplank",
                SpellName = "Trial by Fire",
                Slot = SpellSlot.Unknown,
                Percent = new float[] { 30 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Garen",
                SpellName = "garenqhaste",
                Slot = SpellSlot.Q,
                Percent = new float[] { 35 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Hecarim",
                SpellName = "Devastating Charge",
                Slot = SpellSlot.E,
                Percent = new float[] { 25 },
                IsDecaying = true,
                DecayTime = 4,
                DecaysTo = 75,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Karma",
                SpellName = "Inspire",
                Slot = SpellSlot.E,
                Percent = new float[] { 40, 45, 50, 55, 60 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Karma",
                SpellName = "Defiance",
                Slot = SpellSlot.E,
                Percent = new float[] { 60 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Kennen",
                SpellName = "Lightning Rush",
                Slot = SpellSlot.E,
                Percent = new float[] { 100 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Lucian",
                SpellName = "lucianwbuff",
                Slot = SpellSlot.W,
                Percent = new float[] { 40, 45, 50, 55, 60 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Lulu",
                SpellName = "Whimsy",
                Slot = SpellSlot.W,
                Percent = new float[] { 30 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Nami",
                SpellName = "namipassivedebuff",
                Slot = SpellSlot.Unknown,
                Extra = new float[] { 40 }, // +10% ap
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Olaf",
                SpellName = "Ragnarok",
                Slot = SpellSlot.R,
                Percent = new float[] { 50, 60, 70 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Poppy",
                SpellName = "poppyparagonspeed",
                Slot = SpellSlot.W,
                Percent = new float[] { 17, 19, 21, 23, 25 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Quinn",
                SpellName = "quinnpassiveammo",
                Slot = SpellSlot.W,
                Percent = new float[] { 20, 30, 40, 50, 60 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Sona",
                SpellName = "sonaehaste",
                Slot = SpellSlot.E,
                Extra = new float[] { 13, 14, 15, 16, 17 }, //-3% for ally && (+7.5% for 100 ap 3.5% for ally) + (%2 * ultlevel)
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Viktor",
                SpellName = "haste",
                Slot = SpellSlot.Q,
                Percent = new float[] { 30 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "Zilean",
                SpellName = "TimeWarp",
                Slot = SpellSlot.E,
                Percent = new float[] { 40, 55, 70, 85, 99 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "ITEM_PASSIVE_RAGE_HIT",
                SpellName = "itemphageminispeed",
                Slot = SpellSlot.Unknown,
                Extra = new float[] { 20 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "ITEM_PASSIVE_RAGE_KILL",
                SpellName = "itemphagespeed",
                Slot = SpellSlot.Unknown,
                Extra = new float[] { 60 },
                IsDecaying = false,
            });

            MovementBuffers.Add(new MovementBuffSpellData
            {
                ChampionName = "ITEM_PASSIVE_FUROR",
                SpellName = "bootsdeathmarchspeed",
                Slot = SpellSlot.Unknown,
                Percent = new float[] { 12 },
                IsDecaying = true,
                DecayTime = 2,
            });
            #endregion
        }

    }

    public class DetectedSpellData
    {
        public SpellData Spell;
        public Vector2 StartPosition;
        public Vector2 EndPosition;
        public Obj_AI_Base Sender;
        public GameObjectProcessSpellCastEventArgs Args;

        public void Set(SpellData s, Vector2 sp, Vector2 ep, Obj_AI_Base snd, GameObjectProcessSpellCastEventArgs ar)
        {
            Spell = s;
            StartPosition = sp;
            EndPosition = ep;
            Sender = snd;
            Args = ar;
        }
    }
}
