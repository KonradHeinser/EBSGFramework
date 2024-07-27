using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class EBSG_Mod : Mod
    {
        internal static EBSG_Settings settings;

        public EBSG_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<EBSG_Settings>();
        }

        public override string SettingsCategory()
        {
            return "EBSG_SettingMenuLabel".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoWindowContents(inRect, out bool activeSettings);
            if (activeSettings)
            {
                if (!settings.AsexualHediffs.NullOrEmpty())
                    VFECompatabilityUtilities.SetAsexualRates(settings.AsexualHediffs, settings.asexualDaysSettings);
            }

        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }
    }

    public class EBSG_Settings : ModSettings
    {
        private static Vector2 scrollPosition = Vector2.zero;

        private static bool showMainOptions = true;
        private static bool showEBSGAiOOptions = true;
        private static bool showEBSGBleedOptions = true;
        private static bool showEBSGPsychicOptions = true;
        private static bool showEAGOptions = true;
        //private static bool showCustomizableOptions = true;

        public static bool ageLimitedAgeless = ModsConfig.BiotechActive;
        public static bool hideInactiveSkinGenes = false;
        public static bool hideInactiveHairGenes = false;
        public static bool defaultToRecipeIcon = true;

        public static bool psychicInsulationBondOpinion = true;
        public static bool psychicInsulationBondMood = true;

        public static bool superclottingArchite = true;

        public static bool architePsychicInfluencerBondTorn = false;

        public static Dictionary<string, bool> thinkTreeSettings;
        public static List<string> thinkTreeSettingKeys;
        public static List<bool> thinkTreeSettingBools;

        private static bool alreadyCheckThinkSettings = false;

        public Dictionary<string, int> asexualDaysSettings;

        private List<HediffDef> cachedAsexualHediffs = new List<HediffDef>();
        public bool checkedAsexual = false;

        private List<TabRecord> tabsList;

        public List<TabRecord> TabsList
        {
            get
            {
                if (tabsList.NullOrEmpty())
                {
                    tabsList = new List<TabRecord>
                    {
                        new TabRecord("EBSG_SettingMenuLabel_Main".Translate(), delegate () {
                            tabInt = 1;
                            tabsList.Clear(); // Resets list to get new tab, and potentially allow for more dynamic tab creation in the future
                        }, tabInt == 1),

                        // new TabRecord("EBSG_SettingMenuLabel_Unbugger".Translate(), delegate () { tabInt = 5; }, tabInt == 5)
                    };

                    if (NeedEBSGThinkTree())
                        tabsList.Insert(1, new TabRecord("EBSG_SettingMenuLabel_EBSGThinkTree".Translate(),
                                delegate ()
                                {
                                    tabInt = 2;
                                    tabsList.Clear();
                                }, tabInt == 2)
                            );
                }
                return tabsList;
            }
        }

        public static void BuildThinkTreeSettings()
        {
            EBSGRecorder recorder = DefDatabase<EBSGRecorder>.GetNamedSilentFail("EBSG_Recorder");

            if (recorder != null && !recorder.thinkTreeSettings.NullOrEmpty())
            {
                if (thinkTreeSettings.NullOrEmpty())
                {
                    thinkTreeSettings = new Dictionary<string, bool>();
                    thinkTreeSettingKeys = new List<string>();
                    thinkTreeSettingBools = new List<bool>();
                }

                foreach (ThinkTreeSetting treeSetting in recorder.thinkTreeSettings)
                {
                    if (treeSetting.individualSettings.NullOrEmpty())
                        continue;

                    foreach (ThinkBranchSetting setting in treeSetting.individualSettings)
                        if (thinkTreeSettings.NullOrEmpty() || !thinkTreeSettings.ContainsKey(treeSetting.uniqueID + setting.settingID))
                        {
                            thinkTreeSettings.Add(treeSetting.uniqueID + setting.settingID, setting.defaultSetting);
                        }

                }
            }

            alreadyCheckThinkSettings = true;
        }

        public static bool FetchThinkTreeSetting(string uniqueID, string settingID)
        {
            if (!thinkTreeSettings.NullOrEmpty() && thinkTreeSettings.ContainsKey(uniqueID + settingID))
                return thinkTreeSettings[uniqueID + settingID];
            return false;
        }

        private static bool needThinkTree = false;
        private static bool needTreeChecked = false;

        public static bool NeedEBSGThinkTree()
        {
            if (!needTreeChecked)
            {
                EBSGRecorder recorder = DefDatabase<EBSGRecorder>.GetNamedSilentFail("EBSG_Recorder");

                if (recorder != null && !recorder.thinkTreeSettings.NullOrEmpty())
                    foreach (ThinkTreeSetting setting in recorder.thinkTreeSettings)
                        if (setting.uniqueID != "EBSGFramework") // If it is not in the framework's group, then something else added it, and we need everything active
                        {
                            needThinkTree = true;
                            break;
                        }
                needTreeChecked = true;
            }

            return needThinkTree;
        }

        private Dictionary<string, List<ThinkBranchSetting>> treeSettings; // All the settings that need to show up in the menu. This doesn't mess with settings themselves
        private Dictionary<string, List<string>> treeSettingLabelsAndDescriptions; // The key is the label, and this relies on the counts of this and treeSettings remaining the same
        private Dictionary<string, bool> hiddenTreeSettings; // Gives ability to hide sections
        private List<string> treeSettingIDs = new List<string>(); // Gives a list that can be iterated through
        private bool builtTreeSettingMenuOptions = false;

        private void BuildTreeSettingMenuOptions()
        {
            treeSettings = new Dictionary<string, List<ThinkBranchSetting>>();
            treeSettingLabelsAndDescriptions = new Dictionary<string, List<string>>();
            hiddenTreeSettings = new Dictionary<string, bool>();

            if (Recorder != null)
                foreach (ThinkTreeSetting setting in Recorder.thinkTreeSettings)
                {
                    if (setting.individualSettings.NullOrEmpty())
                        continue;
                    if (treeSettings.NullOrEmpty() || !treeSettings.ContainsKey(setting.uniqueID))
                    {
                        treeSettings.Add(setting.uniqueID, setting.individualSettings);
                        treeSettingLabelsAndDescriptions.Add(setting.uniqueID, new List<string> { setting.label, setting.description });
                        hiddenTreeSettings.Add(setting.uniqueID, true);
                        treeSettingIDs.Add(setting.uniqueID);
                    }
                    else
                        foreach (ThinkBranchSetting branch in setting.individualSettings)
                            if (!treeSettings[setting.uniqueID].Contains(branch))
                                treeSettings[setting.uniqueID].Add(branch);
                }

            builtTreeSettingMenuOptions = true;
        }

        private int tabInt = 1;

        public List<HediffDef> AsexualHediffs
        {
            get
            {
                if (!checkedAsexual && cachedAsexualHediffs.NullOrEmpty())
                {
                    if (asexualDaysSettings == null) asexualDaysSettings = new Dictionary<string, int>();
                    if (Recorder != null && !Recorder.asexualHediffs.NullOrEmpty())
                        foreach (HediffDef hediff in Recorder.asexualHediffs)
                            if (!hediff.comps.NullOrEmpty())
                                foreach (HediffCompProperties comp in hediff.comps)
                                    if (comp.compClass.FullName == "AnimalBehaviours.HediffCompProperties_AsexualReproduction")
                                    {
                                        cachedAsexualHediffs.Add(hediff);
                                        if (!asexualDaysSettings.ContainsKey(hediff.defName))
                                            asexualDaysSettings.Add(hediff.defName, VFECompatabilityUtilities.GetDefaultAsexualRate(comp));
                                        break;
                                    }
                    checkedAsexual = true;
                }

                return cachedAsexualHediffs;
            }
        }

        public bool NeedCustomizationSection
        {
            get
            {
                if (!AsexualHediffs.NullOrEmpty()) return true;
                return false;
            }
        }

        private EBSGRecorder Recorder => DefDatabase<EBSGRecorder>.GetNamedSilentFail("EBSG_Recorder");

        public EBSG_Settings()
        { }

        public override void ExposeData()
        {
            base.ExposeData();
            if (thinkTreeSettings == null) thinkTreeSettings = new Dictionary<string, bool>();

            //if (asexualDaysSettings == null) asexualDaysSettings = new Dictionary<string, int>();

            Scribe_Values.Look(ref ageLimitedAgeless, "ageLimitedAgeless", ModsConfig.BiotechActive);
            Scribe_Values.Look(ref hideInactiveSkinGenes, "hideInactiveSkinGenes", false);
            Scribe_Values.Look(ref hideInactiveHairGenes, "hideInactiveHairGenes", false);
            Scribe_Values.Look(ref psychicInsulationBondOpinion, "psychicInsulationBondOpinion", true);
            Scribe_Values.Look(ref psychicInsulationBondMood, "psychicInsulationBondMood", true);
            Scribe_Values.Look(ref superclottingArchite, "superclottingArchite", true);
            Scribe_Values.Look(ref architePsychicInfluencerBondTorn, "architePsychicInfluencerBondTorn", false);
            Scribe_Values.Look(ref defaultToRecipeIcon, "defaultToRecipeIcon", true);


            Scribe_Collections.Look(ref thinkTreeSettings, "EBSG_ThinkTreeSettings", LookMode.Value, LookMode.Value);

            //Scribe_Collections.Look(ref asexualDaysSettings, "EBSG_asexualDaysSettings", LookMode.Value, LookMode.Value);
        }

        public void DoWindowContents(Rect inRect, out bool activeSettings)
        {
            Listing_Standard optionsMenu = new Listing_Standard();

            Rect tabs = new Rect(inRect)
            {
                yMin = 80,
                height = Mathf.CeilToInt((float)TabsList.Count / 5f) * 40
            };
            TabDrawer.DrawTabs<TabRecord>(tabs, TabsList, Mathf.CeilToInt(TabsList.Count / 5), Mathf.FloorToInt(tabs.width / 5));

            activeSettings = false;
            var scrollContainer = new Rect(inRect);
            scrollContainer.height -= optionsMenu.CurHeight + tabs.height;
            scrollContainer.y += tabs.height;
            Widgets.DrawBoxSolid(scrollContainer, Color.grey);
            var innerContainer = scrollContainer.ContractedBy(1);
            Widgets.DrawBoxSolid(scrollContainer, new ColorInt(37, 37, 37).ToColor);
            var frameRect = innerContainer.ContractedBy(5);
            frameRect.y += 15;
            frameRect.height -= 15;
            var contentRect = frameRect;
            contentRect.x = 0;
            contentRect.y = 0;
            contentRect.width -= 20;

            Widgets.BeginScrollView(frameRect, ref scrollPosition, contentRect, true);
            optionsMenu.Begin(contentRect.AtZero());

            switch (tabInt)
            {
                case 1: // Main EBSG settings
                        // Check for various mods

                    bool EBSGAllInOneActive = ModsConfig.IsActive("EBSG.AiO");
                    bool EAGActive = ModsConfig.IsActive("EBSG.Archite");
                    bool EBSGBleedActive = ModsConfig.IsActive("EBSG.Bleeding");
                    bool EBSGPsychicActive = ModsConfig.IsActive("EBSG.Psychic");

                    // Find out how much room is needed
                    int numberOfOptions = 1;
                    if (showMainOptions)
                    {
                        if (ModsConfig.BiotechActive)
                            numberOfOptions += 3;
                        numberOfOptions += 1;
                    }

                    if (EBSGAllInOneActive)
                    {
                        numberOfOptions += 1;
                        if (showEBSGAiOOptions) numberOfOptions += 3;
                        if (EAGActive)
                        {
                            numberOfOptions += 1;
                            if (showEAGOptions)
                                numberOfOptions += 1;
                        }
                    }
                    else
                    {
                        if (EBSGBleedActive)
                        {
                            numberOfOptions += 1;
                            if (showEBSGBleedOptions) numberOfOptions += 1;
                        }
                        if (EBSGPsychicActive)
                        {
                            numberOfOptions += 1;
                            if (showEBSGPsychicOptions)
                                numberOfOptions += 2;
                        }

                        if (EAGActive)
                        {
                            numberOfOptions += 1;
                            if (showEAGOptions)
                            {
                                if (EBSGPsychicActive) numberOfOptions += 1;
                            }
                        }
                    }

                    contentRect.height = numberOfOptions * 35; // To avoid weird white space, height is based off of option count of present mods

                    optionsMenu.CheckboxLabeled("EBSG_ModName".Translate(), ref showMainOptions, "EBSG_ModDescription".Translate());
                    optionsMenu.Gap(7f);
                    if (showMainOptions)
                    {
                        if (ModsConfig.BiotechActive)
                        {
                            optionsMenu.CheckboxLabeled("EBSG_AgeLimitedAgeless".Translate(), ref ageLimitedAgeless, "EBSG_AgeLimitedAgelessDescription".Translate());
                            optionsMenu.Gap(10f);
                            optionsMenu.CheckboxLabeled("EBSG_HideSkinGenes".Translate(), ref hideInactiveSkinGenes, "EBSG_HideSkinGenesDescription".Translate());
                            optionsMenu.Gap(10f);
                            optionsMenu.CheckboxLabeled("EBSG_HideHairGenes".Translate(), ref hideInactiveHairGenes, "EBSG_HideHairGenesDescription".Translate());
                            optionsMenu.Gap(10f);
                        }
                        optionsMenu.CheckboxLabeled("EBSG_DefaultToRecipeIcon".Translate(), ref defaultToRecipeIcon, "EBSG_DefaultToRecipeIconDescription".Translate());
                        optionsMenu.Gap(10f);
                    }

                    optionsMenu.Gap(10f);

                    if (EBSGAllInOneActive)
                    {
                        optionsMenu.CheckboxLabeled("EBSGAiO_ModName".Translate(), ref showEBSGAiOOptions, "EBSGAiO_ModDescription".Translate());
                        optionsMenu.Gap(7f);

                        if (showEBSGAiOOptions)
                        {
                            optionsMenu.CheckboxLabeled("SuperclottingArchite".Translate(), ref superclottingArchite);
                            optionsMenu.Gap(10f);
                            optionsMenu.CheckboxLabeled("PsychicInsulationMood".Translate(), ref psychicInsulationBondMood);
                            optionsMenu.Gap(10f);
                            optionsMenu.CheckboxLabeled("PsychicInsulationOpinion".Translate(), ref psychicInsulationBondOpinion);
                            optionsMenu.Gap(10f);
                        }
                    }
                    else
                    {
                        if (EBSGBleedActive)
                        {
                            optionsMenu.CheckboxLabeled("EBSGBleed_ModName".Translate(), ref showEBSGBleedOptions);
                            optionsMenu.Gap(7f);
                            if (showEBSGBleedOptions)
                            {
                                optionsMenu.CheckboxLabeled("SuperclottingArchite".Translate(), ref superclottingArchite);
                                optionsMenu.Gap(10f);
                            }
                        }
                        if (EBSGPsychicActive)
                        {
                            optionsMenu.CheckboxLabeled("EBSGPsychic_ModName".Translate(), ref showEBSGPsychicOptions);
                            optionsMenu.Gap(7f);
                            if (showEBSGPsychicOptions)
                            {
                                optionsMenu.CheckboxLabeled("PsychicInsulationMood".Translate(), ref psychicInsulationBondMood);
                                optionsMenu.Gap(10f);
                                optionsMenu.CheckboxLabeled("PsychicInsulationOpinion".Translate(), ref psychicInsulationBondOpinion);
                                optionsMenu.Gap(10f);
                            }
                        }
                    }

                    optionsMenu.Gap(10f);

                    if (EAGActive)
                    {
                        bool needArchiteDisplay = EBSGAllInOneActive || EBSGPsychicActive;

                        if (needArchiteDisplay)
                        {
                            optionsMenu.CheckboxLabeled("EAG_ModName".Translate(), ref showEAGOptions, "EAG_ModDescription".Translate());
                            optionsMenu.Gap(7f);

                            if (showEAGOptions)
                            {
                                if (EBSGAllInOneActive)
                                {
                                    optionsMenu.CheckboxLabeled("ArchitePsychicInfluencer".Translate(), ref architePsychicInfluencerBondTorn);
                                    optionsMenu.Gap(10f);
                                }
                                else
                                {
                                    if (EBSGPsychicActive)
                                    {
                                        optionsMenu.CheckboxLabeled("ArchitePsychicInfluencer".Translate(), ref architePsychicInfluencerBondTorn);
                                        optionsMenu.Gap(10f);
                                    }
                                }
                            }
                        }
                    }

                    optionsMenu.Gap(10f);

                    break;

                case 2: // Think Tree
                    if (!alreadyCheckThinkSettings) // Ensures that files have been checked for new settings
                        BuildThinkTreeSettings();

                    if (!builtTreeSettingMenuOptions)
                        BuildTreeSettingMenuOptions();

                    if (!thinkTreeSettings.NullOrEmpty())
                    {
                        if (Recorder != null && !treeSettingIDs.NullOrEmpty())
                        {
                            contentRect.height = treeSettingIDs.Count * 35; // To avoid weird white space, height is based off of option count of present mods

                            foreach (ThinkTreeSetting setting in Recorder.thinkTreeSettings)
                                if (hiddenTreeSettings.ContainsKey(setting.uniqueID) && hiddenTreeSettings[setting.uniqueID] && !setting.individualSettings.NullOrEmpty())
                                    contentRect.height += setting.individualSettings.Count * 35;

                            foreach (string id in treeSettingIDs)
                            {
                                bool showFlag = hiddenTreeSettings[id];
                                optionsMenu.CheckboxLabeled(treeSettingLabelsAndDescriptions[id][0], ref showFlag, treeSettingLabelsAndDescriptions[id][1]);
                                optionsMenu.Gap(7f);

                                if (showFlag)
                                    foreach (ThinkBranchSetting branchSetting in treeSettings[id])
                                    {
                                        bool settingFlag = thinkTreeSettings[id + branchSetting.settingID];

                                        optionsMenu.CheckboxLabeled("    " + branchSetting.label, ref settingFlag, branchSetting.description);
                                        optionsMenu.Gap(3f);

                                        thinkTreeSettings[id + branchSetting.settingID] = settingFlag;
                                    }



                                hiddenTreeSettings[id] = showFlag;
                                optionsMenu.Gap(10f);
                            }
                        }
                    }
                    else
                    {
                        // This shouldn't ever come to pass, but this is just in case something or someone really fucks with things
                        Log.Error("What in the fuck is going on in here. Sincerely, Alite. Let the author know you got this error so they can look at it. This isn't exactly a phrase Alite uses a lot in \"official\" code, so if you're seeing this, something went really really wrong.");

                        contentRect.height = 35;
                        optionsMenu.Label("EBSG_SettingMenuLabel_EBSGThinkTreeEmpty".Translate());
                        optionsMenu.Gap(10f);
                    }

                    break;
                case 4: // Customization settings
                        /*
                        if (NeedCustomizationSection)
                        {
                            numberOfOptions += 1;
                            if (showCustomizableOptions)
                            {
                                // This is where the individual customization categories belong
                                if (!AsexualHediffs.NullOrEmpty())
                                    numberOfOptions += AsexualHediffs.Count;
                            }
                        }

                        // Checks all the lists to see if any make this worth it.
                        if (NeedCustomizationSection)
                        {
                            optionsMenu.CheckboxLabeled("EBSG_CustomizableThings".Translate(), ref showCustomizableOptions, "EBSG_CustomizableThingsDescription".Translate());

                            if (showCustomizableOptions)
                            {
                                // Column 1
                                if ()
                            }
                        }
                        */

                    break;

                case 5: // Unbugger
                    break;
            }

            optionsMenu.End();
            Widgets.EndScrollView();
            Write();
        }
    }
}
