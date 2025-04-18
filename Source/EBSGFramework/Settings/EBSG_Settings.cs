﻿using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

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
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }
    }

    public class EBSG_Settings : ModSettings
    {
        private static Vector2 scrollPosition = Vector2.zero;

        public static bool ageLimitedAgeless = ModsConfig.BiotechActive;
        public static bool hideInactiveSkinGenes = false;
        public static bool hideInactiveHairGenes = false;
        public static bool defaultToRecipeIcon = true;

        public static bool noInnateMechlinkPrereq = false;
        public static bool noInnateRemotePrereqs = false;
        public static bool noInnatePsylinkPrereq = false;
        public static bool psychicInsulationBondOpinion = true;
        public static bool psychicInsulationBondMood = true;

        public static bool superclottingArchite = true;

        public static bool architePsychicInfluencerBondTorn = false;

        public static Dictionary<string, bool> thinkTreeSettings;
        public static List<string> thinkTreeSettingKeys;
        public static List<bool> thinkTreeSettingBools;

        private static bool alreadyCheckThinkSettings = false;

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
            EBSGRecorder recorder = EBSGDefOf.EBSG_Recorder;

            if (recorder?.thinkTreeSettings.NullOrEmpty() == false)
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
                EBSGRecorder recorder = EBSGDefOf.EBSG_Recorder;

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
        private Dictionary<string, List<string>> treeLabelsAndDescs; // The key is the label, and this relies on the counts of this and treeSettings remaining the same
        private List<string> treeSettingIDs = new List<string>(); // Gives a list that can be iterated through
        private bool builtTreeSettingMenuOptions = false;
        private List<FloatMenuOption> thinkMenus;
        private string currentThinkMenu = "EBSGFramework";

        private void BuildTreeSettingMenuOptions()
        {
            treeSettings = new Dictionary<string, List<ThinkBranchSetting>>();
            treeLabelsAndDescs = new Dictionary<string, List<string>>();
            thinkMenus = new List<FloatMenuOption>();

            if (Recorder != null)
                foreach (ThinkTreeSetting setting in Recorder.thinkTreeSettings)
                {
                    if (setting.individualSettings.NullOrEmpty())
                        continue;

                    thinkMenus.Add(new FloatMenuOption(setting.label, delegate
                    {
                        currentThinkMenu = setting.uniqueID;
                    }));

                    if (treeSettings.NullOrEmpty() || !treeSettings.ContainsKey(setting.uniqueID))
                    {
                        treeSettings.Add(setting.uniqueID, setting.individualSettings);
                        treeLabelsAndDescs.Add(setting.uniqueID, new List<string> { setting.label, setting.description });
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

        private EBSGRecorder Recorder => EBSGDefOf.EBSG_Recorder;

        public EBSG_Settings()
        { }

        public override void ExposeData()
        {
            base.ExposeData();
            if (thinkTreeSettings == null) thinkTreeSettings = new Dictionary<string, bool>();

            Scribe_Values.Look(ref ageLimitedAgeless, "ageLimitedAgeless", ModsConfig.BiotechActive);
            Scribe_Values.Look(ref hideInactiveSkinGenes, "hideInactiveSkinGenes", false);
            Scribe_Values.Look(ref hideInactiveHairGenes, "hideInactiveHairGenes", false);
            Scribe_Values.Look(ref noInnateMechlinkPrereq, "noInnateMechlinkPrereq", false);
            Scribe_Values.Look(ref noInnateRemotePrereqs, "noInnateRemotePrereqs", false);
            Scribe_Values.Look(ref noInnatePsylinkPrereq, "noInnatePsylinkPrereq", false);
            Scribe_Values.Look(ref psychicInsulationBondOpinion, "psychicInsulationBondOpinion", true);
            Scribe_Values.Look(ref psychicInsulationBondMood, "psychicInsulationBondMood", true);
            Scribe_Values.Look(ref superclottingArchite, "superclottingArchite", true);
            Scribe_Values.Look(ref architePsychicInfluencerBondTorn, "architePsychicInfluencerBondTorn", false);
            Scribe_Values.Look(ref defaultToRecipeIcon, "defaultToRecipeIcon", true);

        Scribe_Collections.Look(ref thinkTreeSettings, "EBSG_ThinkTreeSettings", LookMode.Value, LookMode.Value);
        }

        private List<FloatMenuOption> mainMenuOptions;
        private Dictionary<string, string> mainMenuLabels;
        private string mainMenu = "EBSG.Framework";

        public List<FloatMenuOption> MainMenuOptions
        {
            get
            {
                if (mainMenuOptions.NullOrEmpty())
                {
                    mainMenuOptions = new List<FloatMenuOption>()
                    {
                        new FloatMenuOption("EBSG_ModName".Translate(), delegate
                        {
                            mainMenu = "EBSG.Framework";
                        })
                    };
                    mainMenuLabels = new Dictionary<string, string>
                    {
                        { "EBSG.Framework", "EBSG_ModName".Translate() }
                    };

                    if (ModsConfig.IsActive("EBSG.AiO") || ModsConfig.IsActive("EBSG.Lite"))
                    {
                        mainMenuOptions.Add(new FloatMenuOption("EBSGAiO_ModName".Translate(), delegate
                        {
                            mainMenu = "EBSG.AiO";
                        }));
                        mainMenuLabels.Add("EBSG.AiO", "EBSGAiO_ModName".Translate());

                        if (ModsConfig.IsActive("EBSG.Archite"))
                        {
                            mainMenuOptions.Add(new FloatMenuOption("EAG_ModName".Translate(), delegate
                            {
                                mainMenu = "EBSG.Archite";
                            }));
                            mainMenuLabels.Add("EBSG.Archite", "EAG_ModName".Translate());
                        }
                    }
                    else
                    {
                        if (ModsConfig.IsActive("EBSG.Bleeding"))
                        {
                            mainMenuOptions.Add(new FloatMenuOption("EBSGBleed_ModName".Translate(), delegate
                            {
                                mainMenu = "EBSG.Bleeding";
                            }));
                            mainMenuLabels.Add("EBSG.Bleeding", "EBSGBleed_ModName".Translate());
                        }

                        if (ModsConfig.IsActive("EBSG.Psychic"))
                        {
                            mainMenuOptions.Add(new FloatMenuOption("EBSGPsychic_ModName".Translate(), delegate
                            {
                                mainMenu = "EBSG.Psychic";
                            }));
                            mainMenuLabels.Add("EBSG.Psychic", "EBSGPsychic_ModName".Translate());

                            if (ModsConfig.IsActive("EBSG.Archite"))
                            {
                                mainMenuOptions.Add(new FloatMenuOption("EAG_ModName".Translate(), delegate
                                {
                                    mainMenu = "EBSG.Archite";
                                }));
                                mainMenuLabels.Add("EBSG.Archite", "EAG_ModName".Translate());
                            }
                        }

                        if (ModsConfig.IsActive("EBSG.Mechanitor"))
                        {
                            mainMenuOptions.Add(new FloatMenuOption("EBSGMechanitor_ModName".Translate(), delegate
                            {
                                mainMenu = "EBSG.Mechanitor";
                            }));
                            mainMenuLabels.Add("EBSG.Mechanitor", "EBSGMechanitor_ModName".Translate());
                        }
                    }
                }
                
                return mainMenuOptions;
            }
            
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
            var contentRect = frameRect.ContractedBy(5);
            contentRect.x = -5;
            contentRect.y = 0;

            Widgets.BeginScrollView(frameRect, ref scrollPosition, contentRect, true);
            optionsMenu.Begin(contentRect.AtZero());

            switch (tabInt)
            {
                case 1: // Main EBSG settings
                    contentRect.height = 350;
                    if (MainMenuOptions.Count > 1)
                        if (optionsMenu.ButtonTextLabeledPct("", mainMenuLabels[mainMenu], 0.65f))
                        {
                            Find.WindowStack.Add(new FloatMenu(mainMenuOptions));
                        }

                    optionsMenu.Label(mainMenuLabels[mainMenu]);
                    optionsMenu.Gap(7f);

                    switch (mainMenu)
                    {
                        case "EBSG.Framework":
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
                            break;
                        case "EBSG.AiO":
                            optionsMenu.CheckboxLabeled("SuperclottingArchite".Translate(), ref superclottingArchite);
                            optionsMenu.Gap(10f);
                            optionsMenu.CheckboxLabeled("NoInnateMechlinkPrereq".Translate(), ref noInnateMechlinkPrereq, "NoInnateMechlinkPrereqDescription".Translate());
                            optionsMenu.Gap(10f);
                            if (noInnateMechlinkPrereq)
                            {
                                optionsMenu.CheckboxLabeled("NoInnateRemotePrereq".Translate(), ref noInnateRemotePrereqs, "NoInnateRemotePrereqDescription".Translate());
                                optionsMenu.Gap(10f);
                            }
                            optionsMenu.CheckboxLabeled("NoInnatePsylinkPrereq".Translate(), ref noInnatePsylinkPrereq, "NoInnatePsylinkPrereqDescription".Translate());
                            optionsMenu.Gap(10f);
                            optionsMenu.CheckboxLabeled("PsychicInsulationMood".Translate(), ref psychicInsulationBondMood);
                            optionsMenu.Gap(10f);
                            optionsMenu.CheckboxLabeled("PsychicInsulationOpinion".Translate(), ref psychicInsulationBondOpinion);
                            optionsMenu.Gap(10f);
                            break;
                        case "EBSG.Archite":
                            optionsMenu.CheckboxLabeled("ArchitePsychicInfluencer".Translate(), ref architePsychicInfluencerBondTorn);
                            optionsMenu.Gap(10f);
                            break;
                        case "EBSG.Bleeding":
                            optionsMenu.CheckboxLabeled("SuperclottingArchite".Translate(), ref superclottingArchite);
                            optionsMenu.Gap(10f);
                            break;
                        case "EBSG.Psychic":
                            optionsMenu.CheckboxLabeled("NoInnatePsylinkPrereq".Translate(), ref noInnatePsylinkPrereq, "NoInnatePsylinkPrereqDescription".Translate());
                            optionsMenu.Gap(10f);
                            optionsMenu.CheckboxLabeled("PsychicInsulationMood".Translate(), ref psychicInsulationBondMood);
                            optionsMenu.Gap(10f);
                            optionsMenu.CheckboxLabeled("PsychicInsulationOpinion".Translate(), ref psychicInsulationBondOpinion);
                            optionsMenu.Gap(10f);
                            break;
                        case "EBSG.Mechanitor":
                            optionsMenu.CheckboxLabeled("NoInnateMechlinkPrereq".Translate(), ref noInnateMechlinkPrereq, "NoInnateMechlinkPrereqDescription".Translate());
                            optionsMenu.Gap(10f);
                            if (noInnateMechlinkPrereq)
                            {
                                optionsMenu.CheckboxLabeled("NoInnateRemotePrereq".Translate(), ref noInnateRemotePrereqs, "NoInnateRemotePrereqDescription".Translate());
                                optionsMenu.Gap(10f);
                            }
                            break;
                    }
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
                            var settings = treeSettings[currentThinkMenu];
                            contentRect.height = (settings.Count + 1) * 35;
                            if (optionsMenu.ButtonTextLabeledPct("EBSG_ChooseCategory".Translate(), treeLabelsAndDescs[currentThinkMenu][0], 0.25f)){
                                Find.WindowStack.Add(new FloatMenu(thinkMenus));
                            }
                            optionsMenu.Label(treeLabelsAndDescs[currentThinkMenu][0], -1, treeLabelsAndDescs[currentThinkMenu][1]);
                            optionsMenu.Gap(7f);

                            foreach (ThinkBranchSetting branchSetting in settings)
                            {
                                bool settingFlag = thinkTreeSettings[currentThinkMenu + branchSetting.settingID];

                                optionsMenu.CheckboxLabeled("    " + branchSetting.label.CapitalizeFirst(), ref settingFlag, branchSetting.description);
                                optionsMenu.Gap(5f);

                                thinkTreeSettings[currentThinkMenu + branchSetting.settingID] = settingFlag;
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
