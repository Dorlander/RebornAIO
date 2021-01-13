using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using LeagueSharp;
using LeagueSharp.Common;
using Newtonsoft.Json;
using VayneHunter_Reborn.External.ProfileSelector.DefaultProfiles;
using VayneHunter_Reborn.External.ProfileSelector.ProfileValues;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.External.ProfileSelector
{
    internal class ProfileSelector
    {
        #region Fields and Operators
        public static string LeagueSharpAppData
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "LS" + Environment.UserName.GetHashCode().ToString("X"));
            }
        }

        public static String FileName = "VHRProfiles.json";

        public static String WorkingDir
        {
            get { return Path.Combine(LeagueSharpAppData, "VHR"); }
        }

        public static String WorkingPath
        {
            get { return Path.Combine(WorkingDir, FileName); }
        }

        public static int currentIndex = 0;

        public static String CurrentlySelected = "Asuna";
        #endregion

        #region Profiles

        public static List<IDefaultProfile> DefaultProfiles = new List<IDefaultProfile> { new Asuna(), new Eirik() };
        public static List<ProfileSettings> Profiles = new List<ProfileSettings>()
        {
        };


        #endregion

        public static void OnLoad(Menu VHRMenu)
        {
            if (!File.Exists(@WorkingPath))
            {
                SaveDefaultProfiles();
            }

            Load();
            LoadMenu(VHRMenu);
            LoadAssociations();
            LoadCurrentlySelected();
        }

        #region Menu
        public static void LoadMenu(Menu VHRMenu)
        {
            var profilerMenu = new Menu("[VHR] Profile Selector","dz191.vhr.ps");
            {
                var stringListItems = Profiles.Select(p => p.ProfileName).ToArray();

                profilerMenu.AddItem(
                    new MenuItem("dz191.vhr.ps.profile", "Profile: ").SetValue(
                        new StringList(stringListItems, 0))).ValueChanged += (sender, args) =>
                        {
                           CurrentlySelected = args.GetNewValue<StringList>().SelectedValue;
                           SaveCurrentlySelected();
                        };

                profilerMenu.AddItem(
                    new MenuItem("dz191.vhr.ps.load", "Load").SetValue(false)).ValueChanged += (sender, args) =>
                    {
                        if (args.GetNewValue<bool>())
                        {
                            LoadProfile(GetCurrentProfile());
                            ResetItem(
                                       "dz191.vhr.ps.load", string.Format("Loaded Profile: {0}", GetCurrentProfile().ProfileName));
                        }
                    };

                profilerMenu.AddItem(
                    new MenuItem("dz191.vhr.ps.savenew", "Save to New Profile").SetValue(false)).ValueChanged += (sender, args) =>
                    {
                        if (args.GetNewValue<bool>())
                        {
                           // try
                           // {
                                var index = Profiles.Count(m => !m.IsDefault) + 1;
                                if (index < 6)
                                {
                                    var t = GetSettingsToProfile();
                                    Profiles.Add(t);
                                    Console.WriteLine(t.ProfileName + " Count: " + t.Options.Count);

                                    var sList = Profiles.Select(s => s.ProfileName).ToArray();
                                    Variables.Menu.Item("dz191.vhr.ps.profile")
                                        .SetValue(new StringList(sList, Profiles.Count - 1));
                                    ResetItem(
                                        "dz191.vhr.ps.savenew", string.Format("Saved new Profile: {0}", t.ProfileName));
                                    CurrentlySelected = t.ProfileName;
                                    Save();
                                    SaveCurrentlySelected();
                                    SaveAssociations();
                                }
                                else
                                {
                                    ResetItem(
                                       "dz191.vhr.ps.savenew", "5 Profiles limit reached!");
                                }
                                
                            }
                           // catch (Exception e)
                          //  {
                          //      Console.WriteLine("[VHR] Error Saving! " + e.Message);
                          //  }
                            
                       // }
                    };
                profilerMenu.AddItem(
                   new MenuItem("dz191.vhr.ps.savecurrent", "Save to Current Profile").SetValue(false)).ValueChanged += (sender, args) =>
                   {
                       if (args.GetNewValue<bool>())
                       {
                           var t = GetOptions();
                           var c = GetCurrentProfile();
                           if (!c.IsDefault)
                           {
                               c.Options = t;
                               ResetItem(
                                   "dz191.vhr.ps.savecurrent",
                                   string.Format("Saved to Current Profile: {0}", c.ProfileName));
                               SaveCurrentlySelected();
                               SaveAssociations();
                               Save();
                           }
                           else
                           {
                               ResetItem(
                                       "dz191.vhr.ps.savecurrent", "Cannot save to default profiles!");
                           }
                           
                       }
                   };

                profilerMenu.AddItem(
                    new MenuItem("dz191.vhr.ps.showdesc", "Show Description").SetValue(false)).ValueChanged +=
                    (sender, args) =>
                    {
                        if (args.GetNewValue<bool>())
                        {
                            ShowNotification(GetCurrentProfile().ProfileDescription, Color.AliceBlue, 10000);
                            LeagueSharp.Common.Utility.DelayAction.Add(
                                (int) (Game.Ping / 2f + 250f), () =>
                                {
                                    Variables.Menu.Item("dz191.vhr.ps.showdesc").SetValue(false);
                                });
                        }
                    };

                profilerMenu.AddItem(
                    new MenuItem("dz191.vhr.ps.delete", "Delete current Profile").SetValue(false)).ValueChanged +=
                    (sender, args) =>
                    {
                        if (args.GetNewValue<bool>())
                        {
                            var profile = GetCurrentProfile();
                            if (!profile.IsDefault)
                            {

                                Profiles.RemoveAll(p => p.ProfileName == profile.ProfileName);
                                var sList = Profiles.Select(s => s.ProfileName).ToArray();
                                Variables.Menu.Item("dz191.vhr.ps.profile")
                                    .SetValue(new StringList(sList, Profiles.Count - 1));
                                ResetItem("dz191.vhr.ps.delete", string.Format("Deleted Profile: {0}", profile.ProfileName));
                                SaveCurrentlySelected();
                                SaveAssociations();
                                Save();
                            }
                            else
                            {
                                ShowNotification("This profile cannot be deleted!", Color.AliceBlue, 6000);
                                LeagueSharp.Common.Utility.DelayAction.Add(
                               (int)(Game.Ping / 2f + 250f), () =>
                               {
                                   Variables.Menu.Item("dz191.vhr.ps.delete").SetValue(false);
                               });
                            }
                        }
                    };
                profilerMenu.AddItem(new MenuItem("dz191.vhr.ps.space1", " "));
                profilerMenu.AddItem(new MenuItem("dz191.vhr.ps.desc1", "Write .rename <newname> in chat to rename current profile").SetFontStyle(FontStyle.Bold, SharpDX.Color.Yellow));
            }
            VHRMenu.AddSubMenu(profilerMenu);

          //  Game.onInput += Game_OnInput;
        }

      /*  static void Game_OnInput(GameInputEventArgs args)
        {
            if (args.Input.StartsWith("."))
            {
                args.Process = false;
            }
            if (args.Input.StartsWith(".rename"))
            {
                var array = args.Input.Split(' ');
                var profileName = array[1];
                var c = GetCurrentProfile();
                if (!c.IsDefault)
                {
                    var oldName = c.ProfileName;
                    c.ProfileName = profileName;
                    ShowNotification(string.Format("Renamed {0} to {1}", oldName, c.ProfileName), Color.AliceBlue, 6000);
                    var currentlySel = GetItemValue<StringList>("dz191.vhr.ps.profile").SelectedIndex;
                    var sList = Profiles.Select(s => s.ProfileName).ToArray();
                    Variables.Menu.Item("dz191.vhr.ps.profile").SetValue(new StringList(sList, currentlySel));
                    SaveCurrentlySelected();
                    SaveAssociations();
                    Save();
                }
                else
                {
                    ShowNotification("Cannot Rename Default Profiles", Color.AliceBlue, 6000);
                }
            }
        } */

        #endregion

        #region Profile Loading

        public static void LoadProfile(ProfileSettings P)
        {
            if (P != null)
            {
                foreach (var option in P.Options)
                {
                    //Console.WriteLine(option.getMenuName());
                   // continue;
                    var menuItem = Variables.Menu.Item(option.getMenuName());
                    if (menuItem != null)
                    {
                        switch (option.ValueType)
                        {
                            case ValueTypes.Boolean:
                                if (!option.getMenuName().ToLower().Contains("dz191.vhr.misc.tumble.noaastealth"))
                                {
                                    menuItem.SetValue(option.BoolValue);
                                }
                                break;
                            case ValueTypes.Keybind:
                                if (option.KeybindValue != null)
                                {
                                    menuItem.SetValue(new KeyBind(option.KeybindValue.Item1, option.KeybindValue.Item2));
                                }
                                break;
                            case ValueTypes.Slider:
                                if (option.SliderValue != null)
                                {
                                    menuItem.SetValue(new Slider(option.SliderValue.Item1, option.SliderValue.Item2, option.SliderValue.Item3));
                                }
                                break;
                            case ValueTypes.Stringlist:
                                if (option.StringListValue != null)
                                {
                                    menuItem.SetValue(
                                        new StringList(option.StringListValue.Item1, option.StringListValue.Item2));
                                }
                                break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Profile Modification/Creation

        public static ProfileSettings GetSettingsToProfile()
        {
            var baseName = "VHR_Custom";
            var index = Profiles.Count(m => !m.IsDefault) + 1;

            ProfileSettings p = new ProfileSettings
            {
                ProfileName = baseName+index,
                ProfileDescription = "Custom VHR User-Created Profile #"+index,
                IsDefault = false,
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
                            GetItemValue<StringList>("dz191.vhr.misc.condemn.qlogic").SList,
                            GetItemValue<StringList>("dz191.vhr.misc.condemn.qlogic").SelectedIndex)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Tumble,
                    MinorCategory = MinorCategories.SmartQ,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.tumble.smartq")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Tumble,
                    MinorCategory = MinorCategories.NoAAStealthEx,
                    ValueType = ValueTypes.Keybind,
                    KeybindValue =
                        new Tuple<uint, KeyBindType>(
                            GetItemValue<KeyBind>("dz191.vhr.misc.tumble.noaastealthex").Key,
                            GetItemValue<KeyBind>("dz191.vhr.misc.tumble.noaastealthex").Type)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Tumble,
                    MinorCategory = MinorCategories.NoQEnemies,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.tumble.noqenemies")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Tumble,
                    MinorCategory = MinorCategories.DynamicQSafety,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.tumble.dynamicqsafety")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Tumble,
                    MinorCategory = MinorCategories.QSpam,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.tumble.qspam")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Tumble,
                    MinorCategory = MinorCategories.QInRange,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.tumble.qinrange")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Tumble,
                    MinorCategory = MinorCategories.WallTumble,
                    ValueType = ValueTypes.Keybind,
                    KeybindValue =
                        new Tuple<uint, KeyBindType>(
                            GetItemValue<KeyBind>("dz191.vhr.misc.tumble.walltumble").Key,
                            GetItemValue<KeyBind>("dz191.vhr.misc.tumble.walltumble").Type)
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
                            GetItemValue<StringList>("dz191.vhr.misc.condemn.condemnmethod").SList,
                            GetItemValue<StringList>("dz191.vhr.misc.condemn.condemnmethod").SelectedIndex)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.PushDistance,
                    ValueType = ValueTypes.Slider,
                    SliderValue =
                        new Tuple<int, int, int>(
                            GetItemValue<Slider>("dz191.vhr.misc.condemn.pushdistance").Value, 350, 470)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.Accuracy,
                    ValueType = ValueTypes.Slider,
                    SliderValue =
                        new Tuple<int, int, int>(
                            GetItemValue<Slider>("dz191.vhr.misc.condemn.accuracy").Value, 1, 100)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.ENextAuto,
                    ValueType = ValueTypes.Keybind,
                    KeybindValue =
                        new Tuple<uint, KeyBindType>(
                            GetItemValue<KeyBind>("dz191.vhr.misc.condemn.enextauto").Key,
                            GetItemValue<KeyBind>("dz191.vhr.misc.condemn.enextauto").Type)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.OnlyStunCurrent,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.condemn.onlystuncurrent")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.AutoE,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.condemn.autoe")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.EKS,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.condemn.eks")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.NoEAA,
                    ValueType = ValueTypes.Slider,
                    SliderValue =
                        new Tuple<int, int, int>(GetItemValue<Slider>("dz191.vhr.misc.condemn.noeaa").Value, 0, 5)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.TrinketBush,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.condemn.trinketbush")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.EThird,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.condemn.ethird")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.LowLifePeel,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.condemn.lowlifepeel")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.CondemnTurret,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.condemn.condemnturret")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.CondemnFlag,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.condemn.condemnflag")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.Condemn,
                    MinorCategory = MinorCategories.NoETurret,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.condemn.noeturret")
                },

                #endregion

                #region General
                new ProfileOption
                {
                    MajorCategory = MajorCategories.General,
                    MinorCategory = MinorCategories.AntiGP,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.general.antigp")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.General,
                    MinorCategory = MinorCategories.Interrupt,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.general.interrupt")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.General,
                    MinorCategory = MinorCategories.AntiGPDelay,
                    ValueType = ValueTypes.Slider,
                    SliderValue =
                        new Tuple<int, int, int>(
                            GetItemValue<Slider>("dz191.vhr.misc.general.antigpdelay").Value, 0, 1000)
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.General,
                    MinorCategory = MinorCategories.SpecialFocus,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.general.specialfocus")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.General,
                    MinorCategory = MinorCategories.Reveal,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.general.reveal")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.General,
                    MinorCategory = MinorCategories.DisableMovement,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.general.disablemovement")
                },
                new ProfileOption
                {
                    MajorCategory = MajorCategories.General,
                    MinorCategory = MinorCategories.Permashow,
                    ValueType = ValueTypes.Boolean,
                    BoolValue = GetItemValue<bool>("dz191.vhr.misc.general.permashow")
                },

                #endregion
            };
        }
        #endregion

        #region Utility methods
        public static ProfileSettings GetProfileOnName(String Name)
        {
            return Profiles.FirstOrDefault(s => s.ProfileName.ToLower() == Name.ToLower());
        }

        public static ProfileSettings GetCurrentProfile()
        {
            //CurrentlySelected = GetItemValue<StringList>("dz191.vhr.ps.profile").SelectedValue;
            return GetProfileOnName(GetItemValue<StringList>("dz191.vhr.ps.profile").SelectedValue) ?? GetProfileOnName("Asuna");
        }

        private static void ResetItem(String Item, String Message)
        {
            ShowNotification(Message, Color.AliceBlue, 5000);

            LeagueSharp.Common.Utility.DelayAction.Add((int)(Game.Ping / 2f + 250f), () =>
            {
                Variables.Menu.Item(Item).SetValue(false);
            });
        }
        public static Notification ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            var notif = new Notification(message).SetTextColor(color);
            Notifications.AddNotification(notif);
            if (dispose)
            {
                LeagueSharp.Common.Utility.DelayAction.Add(duration, () => notif.Dispose());
            }
            return notif;
        }

        private static T GetItemValue<T>(string item)
        {
            if (Variables.Menu.Item(item) == null)
            {
                return default(T);
            }

            return Variables.Menu.Item(item).GetValue<T>();
        }

        //TODO Not working
        public static void LoadProfileDesc(ProfileSettings current, Menu menu)
        {

            var currentDescription = current.ProfileDescription;
            List<string> array = new List<string>();
            for (int i = 0; i < currentDescription.Length; i += 16)
            {
                array.Add(currentDescription.Substring(i, 16));
            }
            var counter = 0;
            foreach (var h in array)
            {
                menu.AddItem(new MenuItem(string.Format("dz191.vhr.ps.{0}.{1}", current.ProfileName.ToLower(), counter), h));
                counter++;
            }
        }
        #endregion

        #region Load/Save
        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public static void SaveDefaultProfiles()
        {
            if (!Directory.Exists(@WorkingDir))
            {
                Directory.CreateDirectory(@WorkingDir);
            }

            var serializedObject = JsonConvert.SerializeObject(DefaultProfiles.Select(s => s.GetProfileSettings()), Formatting.Indented);

            using (StreamWriter sw = new StreamWriter(@WorkingPath))
            {
                sw.Write(serializedObject);
            }

            Console.WriteLine("[VHR] Saved Default Profiles!");
        }


        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public static void LoadAssociations()
        {
        }

        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public static void SaveAssociations()
        {
        }

        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public static void LoadCurrentlySelected()
        {
            if (!File.Exists(Path.Combine(WorkingDir, "CS.txt")))
            {
                CurrentlySelected = "Asuna";
                SaveCurrentlySelected();
                return;
            }

            var text = File.ReadAllText(Path.Combine(WorkingDir, "CS.txt"));
            
            CurrentlySelected = text;
        }

        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public static void SaveCurrentlySelected()
        {
            if (!Directory.Exists(@WorkingDir))
            {
                Directory.CreateDirectory(@WorkingDir);
            }

            using (StreamWriter sw = new StreamWriter(Path.Combine(WorkingDir, "CS.txt")))
            {
                sw.Write(GetItemValue<StringList>("dz191.vhr.ps.profile").SelectedValue);
            }
        }

        //Le Epik fromBehind();
        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public static void Load()
        {
            if (!File.Exists(@WorkingPath))
            {
                SaveDefaultProfiles();
                Load();
                return;
            }

            var text = File.ReadAllText(@WorkingPath);
            var DSProfiles = JsonConvert.DeserializeObject<List<ProfileSettings>>(text);
            foreach (var p in DSProfiles.Where(ps => Profiles.All(pd => pd.ProfileName != ps.ProfileName)))
            {
               Profiles.Add(p);
            }     
        }

        //Very l33t such wow doge
        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public static void Save()
        {
            if (!Directory.Exists(@WorkingDir))
            {
                Directory.CreateDirectory(@WorkingDir);
            }

            var serializedObject = JsonConvert.SerializeObject(Profiles, Formatting.Indented);

            using (StreamWriter sw = new StreamWriter(@WorkingPath))
            {
                sw.Write(serializedObject);
            }

        }
        #endregion

    }

    #region Profiles class
    internal class ProfileSettings
    {

        [JsonProperty(PropertyName = "ProfileName")]
        public String ProfileName { get; set; }

        [JsonProperty(PropertyName = "ProfileDesc")]
        public String ProfileDescription { get; set; }

        [JsonProperty(PropertyName = "IsDefault")]
        public bool IsDefault { get; set; }

        [JsonProperty(PropertyName = "ProfileOptions")]
        public List<ProfileOption> Options { get; set; }
    }

    internal class ProfileOption
    {
        [JsonProperty(PropertyName = "MenuCategory")]
        public MenuCategory MCategory  = MenuCategory.Misc;

        [JsonProperty(PropertyName = "MajorCategory")]
        public MajorCategories MajorCategory { get; set; }

        [JsonProperty(PropertyName = "MinorCategory")]
        public MinorCategories MinorCategory { get; set; }

        [JsonProperty(PropertyName = "ExtraOptions")]
        public String ExtraOptions { get; set; }

        [JsonProperty(PropertyName = "ValueType")]
        public ValueTypes ValueType { get; set; }

        [JsonProperty(PropertyName = "BoolValue")]
        public bool BoolValue { get; set; }

        [JsonProperty(PropertyName = "SliderValue")]
        public Tuple<int, int, int> SliderValue { get; set; }

        [JsonProperty(PropertyName = "KeybindValue")]
        public Tuple<uint, KeyBindType> KeybindValue { get; set; }

        [JsonProperty(PropertyName = "StringListValue")]
        public Tuple<string[], int> StringListValue { get; set; }

        public String getMenuName()
        {
            return string.Format("dz191.vhr.{0}.{1}.{2}{3}",
                MCategory.ToString().ToLower(),
                MajorCategory.ToString().ToLower(),
                MinorCategory.ToString().ToLower(),
                ExtraOptions != null?ExtraOptions.ToLower():"");
        }
    }
#endregion

}
