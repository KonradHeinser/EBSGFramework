using RimWorld;
using UnityEngine;
using Verse;
using System;
using System.Reflection;
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

        private static bool showMainAIOptions = true;

        public static bool ageLimitedAgeless = ModsConfig.BiotechActive;
        public static bool hideInactiveSkinGenes = false;
        public static bool hideInactiveHairGenes = false;
        public static bool defaultToRecipeIcon = true;

        public static bool psychicInsulationBondOpinion = true;
        public static bool psychicInsulationBondMood = true;

        public static bool superclottingArchite = true;

        public static bool architePsychicInfluencerBondTorn = false;

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

                        new TabRecord("EBSG_SettingMenuLabel_EBSGThinkTree".Translate(),
                        delegate () {
                            tabInt = 2;
                            tabsList.Clear();
                        }, tabInt == 2),
                        // new TabRecord("EBSG_SettingMenuLabel_Unbugger".Translate(), delegate () { tabInt = 5; }, tabInt == 5)
                    };
                }
                return tabsList;
            }
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
            if (asexualDaysSettings == null) asexualDaysSettings = new Dictionary<string, int>();

            Scribe_Values.Look(ref ageLimitedAgeless, "ageLimitedAgeless", ModsConfig.BiotechActive);
            Scribe_Values.Look(ref hideInactiveSkinGenes, "hideInactiveSkinGenes", false);
            Scribe_Values.Look(ref hideInactiveHairGenes, "hideInactiveHairGenes", false);
            Scribe_Values.Look(ref psychicInsulationBondOpinion, "psychicInsulationBondOpinion", true);
            Scribe_Values.Look(ref psychicInsulationBondMood, "psychicInsulationBondMood", true);
            Scribe_Values.Look(ref superclottingArchite, "superclottingArchite", true);
            Scribe_Values.Look(ref architePsychicInfluencerBondTorn, "architePsychicInfluencerBondTorn", false);
            Scribe_Values.Look(ref defaultToRecipeIcon, "defaultToRecipeIcon", true);

            //Scribe_Collections.Look(ref asexualDaysSettings, "EBSG_asexualDaysSettings", LookMode.Value, LookMode.Value);
        }

        public void DoWindowContents(Rect inRect, out bool activeSettings)
        {
            Listing_Standard optionsMenu = new Listing_Standard();

            Rect tabs = new Rect(inRect)
            {
                yMin = Mathf.CeilToInt((float)TabsList.Count / 5f) * 80
            };
            TabDrawer.DrawTabs<TabRecord>(tabs, TabsList, Mathf.CeilToInt(TabsList.Count / 5), Mathf.FloorToInt(tabs.width / 5));

            activeSettings = false;
            var scrollContainer = inRect.ContractedBy(10);
            scrollContainer.height -= optionsMenu.CurHeight;
            scrollContainer.y += tabs.yMin;
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

                    Widgets.BeginScrollView(frameRect, ref scrollPosition, contentRect, true);

                    optionsMenu.Begin(contentRect.AtZero());

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

                    int numberOfAIOptions = 4;

                    contentRect.height = numberOfAIOptions * 35; // To avoid weird white space, height is based off of option count of present mods

                    Widgets.BeginScrollView(frameRect, ref scrollPosition, contentRect, true);

                    optionsMenu.Begin(contentRect.AtZero());

                    optionsMenu.CheckboxLabeled("EBSG_SettingMenuLabel_EBSGThinkTree".Translate(), ref showMainAIOptions, "EBSG_SettingMenuLabel_EBSGThinkTreeDescription".Translate());
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
