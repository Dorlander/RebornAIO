using System;
using System.Collections.Generic;
using LeagueSharp.Common;
using VayneHunter_Reborn.External.ProfileSelector.ProfileValues;

namespace VayneHunter_Reborn.External.ProfileSelector.DefaultProfiles
{
    class Asuna : IDefaultProfile
    {
        public ProfileSettings GetProfileSettings()
        {
            var baseName = "Asuna";

            ProfileSettings p = new ProfileSettings
            {
                ProfileName = baseName,
                ProfileDescription = "Asuna's profile. This profile contains the settings the developer of VHR uses in game.",
                IsDefault = true,
                Options = GetOptions()
            };
            return p;
        }

        public static List<ProfileOption> GetOptions()
        {
            return new List<ProfileOption>
            {
                #region Q Settings Save
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Tumble,
                    MinorCategory = MinorCategories.QLogic,
                    ValueType = ValueTypes.Stringlist,
                    StringListValue =
                        new Tuple<string[], int>(
                            new[] { "Reborn", "Normal", "Kite melees", "Kurisu" },0)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Tumble,
                    MinorCategory = MinorCategories.SmartQ,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = true
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Tumble,
                    MinorCategory = MinorCategories.NoAAStealthEx,
                    ValueType = ValueTypes.Keybind,
                    KeybindValue = new Tuple<uint, KeyBindType>('K', KeyBindType.Toggle)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Tumble,
                    MinorCategory = MinorCategories.NoQEnemies,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = false
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Tumble,
                    MinorCategory = MinorCategories.DynamicQSafety,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = false
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Tumble,
                    MinorCategory = MinorCategories.QSpam,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = false
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Tumble,
                    MinorCategory = MinorCategories.QInRange,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = true
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Tumble,
                    MinorCategory = MinorCategories.WallTumble,
                    ValueType = ValueTypes.Keybind,
                    KeybindValue =
                        new Tuple<uint, KeyBindType>(
                            'Y',
                            KeyBindType.Press)
                },

                #endregion

                #region Condemn Bullshit
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.CondemnMethod,
                    ValueType = ValueTypes.Stringlist,
                    StringListValue =
                        new Tuple<string[], int>(
                            new[] { "VH Revolution", "VH Reborn", "Marksman/Gosu", "Shine#" },
                           0)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.PushDistance,
                    ValueType = ValueTypes.Slider,
                    SliderValue =
                        new Tuple<int, int, int>(
                            395, 350, 470)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.Accuracy,
                    ValueType = ValueTypes.Slider,
                    SliderValue =
                        new Tuple<int, int, int>(
                            33, 1, 100)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.ENextAuto,
                    ValueType = ValueTypes.Keybind,
                    KeybindValue =
                        new Tuple<uint, KeyBindType>(
                            'T',
                            KeyBindType.Press)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.OnlyStunCurrent,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = false
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.AutoE,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = true
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.EKS,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = false
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.NoEAA,
                    ValueType = ValueTypes.Slider,
                    SliderValue =
                        new Tuple<int, int, int>(2, 0, 5)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.TrinketBush,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = false
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.EThird,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = false
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.LowLifePeel,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = false
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.CondemnTurret,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = false
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.CondemnFlag,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = false
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.NoETurret,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = true
                },

                #endregion

                #region General
                new ProfileOption
                {
                    MajorCategory = MajorCategories.General,
                    MinorCategory = MinorCategories.AntiGP,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = false
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.General,
                    MinorCategory = MinorCategories.Interrupt,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = true
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.General,
                    MinorCategory = MinorCategories.AntiGPDelay,
                    ValueType = ValueTypes.Slider,
                    SliderValue =
                        new Tuple<int, int, int>(
                            250, 0, 1000)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.General,
                    MinorCategory = MinorCategories.SpecialFocus,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = true
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.General,
                    MinorCategory = MinorCategories.Reveal,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = false
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.General,
                    MinorCategory = MinorCategories.DisableMovement,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = false
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.General,
                    MinorCategory = MinorCategories.Permashow,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = false
                },

                #endregion
            };
        }
    }
}
