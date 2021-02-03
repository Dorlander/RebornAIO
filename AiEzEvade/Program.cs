namespace AiEzEvade
{
    using System;
    using LeagueSharp;
    using LeagueSharp.Common;

    internal class Program
    {
        public static Menu Config;
        public static Obj_AI_Hero Player;
        public static int TickLimit;
        public static int Mode = 3;

        public static ezEvadeSettings set = new ezEvadeSettings(2, true, true, true, false, false, true, 100, 100, 100,
            false);

        private static Menu ezEvadeMenu => Menu.GetMenu("EzEvade", "ezEvade");

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            Config = new Menu("AiEzEvade", "AiEzEvade", true);

            var lowDanger = Config.AddSubMenu(new Menu("LOW danger", "LOW danger"));
            {
                lowDanger.AddItem(new MenuItem("LowHp", "Activation above % HP").SetValue(new Slider(70)));
                lowDanger.AddItem(new MenuItem("LowEvadeMode", "Evade Mode").SetValue(new Slider(2, 0, 2)));
                lowDanger.AddItem(new MenuItem("LowDodgeDangerous", "Dodge only Dangerous").SetValue(true));
                lowDanger.AddItem(new MenuItem("LowDodgeCircularSpells", "Dodge Circular Spells").SetValue(false));
                lowDanger.AddItem(new MenuItem("LowDodgeFOWSpells", "Dodge FOW Spells").SetValue(false));
                lowDanger.AddItem(new MenuItem("LowCheckSpellCollision", "Check Spell Collision").SetValue(true));
                lowDanger.AddItem(new MenuItem("LowClickOnlyOnce", "Click Only Once").SetValue(true));
                lowDanger.AddItem(new MenuItem("LowTickLimiter", "Tick Limiter").SetValue(new Slider(100, 0, 1000)));
                lowDanger.AddItem(new MenuItem("LowReactionTime", "Reaction Time").SetValue(new Slider(250, 0, 1000)));
                lowDanger.AddItem(
                    new MenuItem("LowSpellDetectionTime", "Spell Detection Time").SetValue(new Slider(100, 0, 1000)));
                lowDanger.AddItem(new MenuItem("LowFastMovementBlock", "Fast Movement Block").SetValue(false));
            }

            var mediumDanger = Config.AddSubMenu(new Menu("MEDIUM danger", "MEDIUM danger"));
            {
                mediumDanger.AddItem(new MenuItem("0", "Activation between LOW and HIGH"));
                mediumDanger.AddItem(new MenuItem("MediumEvadeMode", "Evade Mode").SetValue(new Slider(2, 2, 0)));
                mediumDanger.AddItem(new MenuItem("MediumDodgeDangerous", "Dodge only Dangerous").SetValue(false));
                mediumDanger.AddItem(new MenuItem("MediumDodgeCircularSpells", "Dodge Circular Spells").SetValue(false));
                mediumDanger.AddItem(new MenuItem("MediumDodgeFOWSpells", "Dodge FOW Spells").SetValue(true));
                mediumDanger.AddItem(new MenuItem("MediumCheckSpellCollision", "Check Spell Collision").SetValue(true));
                mediumDanger.AddItem(new MenuItem("MediumClickOnlyOnce", "Click Only Once").SetValue(true));
                mediumDanger.AddItem(new MenuItem("MediumTickLimiter", "Tick Limiter").SetValue(new Slider(100, 0, 1000)));
                mediumDanger.AddItem(new MenuItem("MediumReactionTime", "Reaction Time").SetValue(new Slider(200, 0, 1000)));
                mediumDanger.AddItem(
                    new MenuItem("MediumSpellDetectionTime", "Spell Detection Time").SetValue(new Slider(200, 0, 1000)));
                mediumDanger.AddItem(new MenuItem("MediumFastMovementBlock", "Fast Movement Block").SetValue(false));
            }

            var highDanger = Config.AddSubMenu(new Menu("HIGH danger", "HIGH danger"));
            {
                highDanger.AddItem(new MenuItem("HighHp", "Activation under % HP").SetValue(new Slider(35)));
                highDanger.AddItem(new MenuItem("HighEvadeMode", "Evade Mode").SetValue(new Slider(1, 0, 2)));
                highDanger.AddItem(new MenuItem("HighDodgeDangerous", "Dodge only Dangerous").SetValue(false));
                highDanger.AddItem(new MenuItem("HighDodgeCircularSpells", "Dodge Circular Spells").SetValue(true));
                highDanger.AddItem(new MenuItem("HighDodgeFOWSpells", "Dodge FOW Spells").SetValue(true));
                highDanger.AddItem(new MenuItem("HighCheckSpellCollision", "Check Spell Collision").SetValue(true));
                highDanger.AddItem(new MenuItem("HighClickOnlyOnce", "Click Only Once").SetValue(false));
                highDanger.AddItem(new MenuItem("HighTickLimiter", "Tick Limiter").SetValue(new Slider(70, 0, 1000)));
                highDanger.AddItem(new MenuItem("HighReactionTime", "Reaction Time").SetValue(new Slider(50, 0, 1000)));
                highDanger.AddItem(
                    new MenuItem("HighSpellDetectionTime", "Spell Detection Time").SetValue(new Slider(50, 0, 1000)));
                highDanger.AddItem(new MenuItem("HighFastMovementBlock", "Fast Movement Block").SetValue(true));
            }

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //foreach (var menu in Menu.RootMenus)
            //{
            //    Console.WriteLine(menu.Key);
            //}

            if (ezEvadeMenu != null)
            {
                if (Utils.TickCount - TickLimit > 200)
                {
                    TickLimit = Utils.TickCount;

                    var newMode = 0;

                    if (Player.HealthPercent < Config.Item("LowHp").GetValue<Slider>().Value)
                    {
                        newMode = 2;
                    }
                    else if (Player.HealthPercent > Config.Item("HighHp").GetValue<Slider>().Value)
                    {
                        newMode = 0;
                    }
                    else
                    {
                        newMode = 1;
                    }

                    if (newMode != Mode)
                    {
                        switch (newMode)
                        {
                            case 0:
                                set = new ezEvadeSettings(
                                    Config.Item("LowEvadeMode").GetValue<Slider>().Value,
                                    Config.Item("LowDodgeDangerous").GetValue<bool>(),
                                    Config.Item("LowDodgeCircularSpells").GetValue<bool>(),
                                    Config.Item("LowDodgeFOWSpells").GetValue<bool>(),
                                    Config.Item("LowCheckSpellCollision").GetValue<bool>(),
                                    false,
                                    Config.Item("LowClickOnlyOnce").GetValue<bool>(),
                                    Config.Item("LowTickLimiter").GetValue<Slider>().Value,
                                    Config.Item("LowReactionTime").GetValue<Slider>().Value,
                                    Config.Item("LowSpellDetectionTime").GetValue<Slider>().Value,
                                    Config.Item("LowFastMovementBlock").GetValue<bool>());

                                SetezEvade(set);
                                Mode = newMode;
                                return;
                            case 1:
                                set = new ezEvadeSettings(
                                    Config.Item("MediumEvadeMode").GetValue<Slider>().Value,
                                    Config.Item("MediumDodgeDangerous").GetValue<bool>(),
                                    Config.Item("MediumDodgeCircularSpells").GetValue<bool>(),
                                    Config.Item("MediumDodgeFOWSpells").GetValue<bool>(),
                                    Config.Item("MediumCheckSpellCollision").GetValue<bool>(),
                                    false,
                                    Config.Item("MediumClickOnlyOnce").GetValue<bool>(),
                                    Config.Item("MediumTickLimiter").GetValue<Slider>().Value,
                                    Config.Item("MediumReactionTime").GetValue<Slider>().Value,
                                    Config.Item("MediumSpellDetectionTime").GetValue<Slider>().Value,
                                    Config.Item("MediumFastMovementBlock").GetValue<bool>());

                                SetezEvade(set);
                                Mode = newMode;
                                return;
                            case 2:
                                set = new ezEvadeSettings(
                                    Config.Item("HighEvadeMode").GetValue<Slider>().Value,
                                    Config.Item("HighDodgeDangerous").GetValue<bool>(),
                                    Config.Item("HighDodgeCircularSpells").GetValue<bool>(),
                                    Config.Item("HighDodgeFOWSpells").GetValue<bool>(),
                                    Config.Item("HighCheckSpellCollision").GetValue<bool>(),
                                    false,
                                    Config.Item("HighClickOnlyOnce").GetValue<bool>(),
                                    Config.Item("HighTickLimiter").GetValue<Slider>().Value,
                                    Config.Item("HighReactionTime").GetValue<Slider>().Value,
                                    Config.Item("HighSpellDetectionTime").GetValue<Slider>().Value,
                                    Config.Item("HighFastMovementBlock").GetValue<bool>());

                                SetezEvade(set);
                                Mode = newMode;
                                break;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("i Cant Found ezEvade");
            }
        }

        private static void SetezEvade(ezEvadeSettings sets)
        {
            ezEvadeMenu.Item("EvadeMode").SetValue(
                new StringList(new[] {"Smooth", "Fastest", "Very Smooth"}, sets.EvadeMode));
            ezEvadeMenu.Item("DodgeDangerous").SetValue(sets.DodgeDangerous);
            ezEvadeMenu.Item("DodgeCircularSpells").SetValue(sets.DodgeCircularSpells);
            ezEvadeMenu.Item("DodgeFOWSpells").SetValue(sets.DodgeFOWSpells);
            ezEvadeMenu.Item("CheckSpellCollision").SetValue(sets.CheckSpellCollision);
            ezEvadeMenu.Item("ContinueMovement").SetValue(sets.ContinueMovement);
            ezEvadeMenu.Item("ClickOnlyOnce").SetValue(sets.ClickOnlyOnce);
            ezEvadeMenu.Item("TickLimiter").SetValue(new Slider(sets.TickLimiter, 0, 500));
            ezEvadeMenu.Item("ReactionTime").SetValue(new Slider(sets.ReactionTime, 0, 500));
            ezEvadeMenu.Item("SpellDetectionTime").SetValue(new Slider(sets.SpellDetectionTime, 0, 1000));
            //ezEvadeMenu.Item("FastMovementBlock").SetValue(sets.FastMovementBlock);
        }
    }
}
