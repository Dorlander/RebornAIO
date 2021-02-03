namespace KoreanZed
{
    using System;

    using KoreanZed.Common;

    using LeagueSharp.Common;

    class Zed
    {
        private readonly ZedMenu zedMenu;

        private readonly ZedCore zedCore;

        private readonly ZedAntiGapCloser zedAntiGapCloser;

        private readonly Orbwalking.Orbwalker zedOrbwalker;

        private readonly ZedSpells zedSpells;

        private readonly ZedShadows zedShadows;

        private readonly ZedPotions zedPotions;

        private readonly ZedKS zedKs;

        private readonly ZedSpellDodge zedSpellDodge;

        private readonly ZedDrawingSpells zedDrawingSpells;

        private readonly CommonDamageDrawing damageDrawing;

        private readonly CommonForceUltimate forceUltimate;

        private readonly ZedUnderTurretFarm zedUnderTurretFarm;

        private readonly ZedEnergyChecker energy;

        private readonly ZedAutoE zedAutoE;

        private readonly ZedFlee zedFlee;

        public Zed()
        {
            zedSpells = new ZedSpells();
            zedMenu = new ZedMenu(zedSpells, out zedOrbwalker);
            energy = new ZedEnergyChecker(zedMenu);
            zedShadows = new ZedShadows(zedMenu, zedSpells, energy);
            zedCore = new ZedCore(zedSpells, zedOrbwalker, zedMenu, zedShadows, energy);
            zedAntiGapCloser = new ZedAntiGapCloser(zedMenu, zedSpells, zedShadows);
            zedPotions = new ZedPotions(zedMenu);
            zedKs = new ZedKS(zedSpells, zedOrbwalker, zedShadows);
            zedSpellDodge = new ZedSpellDodge(zedSpells, zedMenu);
            zedDrawingSpells = new ZedDrawingSpells(zedMenu, zedSpells);
            zedUnderTurretFarm = new ZedUnderTurretFarm(zedSpells, zedOrbwalker);
            damageDrawing = new CommonDamageDrawing(zedMenu) { AmountOfDamage = zedCore.ComboDamage };
            forceUltimate = new CommonForceUltimate(zedMenu, zedSpells, zedOrbwalker) { ForceUltimate = zedCore.ForceUltimate };
            zedAutoE = new ZedAutoE(zedMenu, zedShadows, zedSpells);
            zedFlee = new ZedFlee(zedMenu, zedShadows);
        }
    }
}
