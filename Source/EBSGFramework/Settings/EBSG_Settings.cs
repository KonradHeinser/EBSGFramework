using RimWorld;
using UnityEngine;
using Verse;

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
            settings.DoWindowContents(inRect);
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

        public static bool ageLimitedAgeless = ModsConfig.BiotechActive;

        public static bool psychicInsulationBondOpinion = true;
        public static bool psychicInsulationBondMood = true;

        public static bool superclottingArchite = true;

        public static bool architePsychicInfluencerBondTorn = false;


        public EBSG_Settings() { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ageLimitedAgeless, "ageLimitedAgeless", ModsConfig.BiotechActive);
            Scribe_Values.Look(ref psychicInsulationBondOpinion, "psychicInsulationBondOpinion", true);
            Scribe_Values.Look(ref psychicInsulationBondMood, "psychicInsulationBondMood", true);
            Scribe_Values.Look(ref superclottingArchite, "superclottingArchite", true);
            Scribe_Values.Look(ref architePsychicInfluencerBondTorn, "architePsychicInfluencerBondTorn", false);
        }

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard optionsMenu = new Listing_Standard();

            var scrollContainer = inRect.ContractedBy(10);
            scrollContainer.height -= optionsMenu.CurHeight;
            scrollContainer.y += optionsMenu.CurHeight;
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

            // Check for various mods

            bool EBSGAllInOneActive = ModsConfig.IsActive("EBSG.AiO");
            bool EAGActive = ModsConfig.IsActive("EBSG.Archite");
            bool EBSGBleedActive = ModsConfig.IsActive("EBSG.Bleeding");
            bool EBSGPsychicActive = ModsConfig.IsActive("EBSG.Psychic");

            // Find out how much room is needed
            int numberOfOptions = 1;
            if (showMainOptions) numberOfOptions += 1;
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
                }
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

            optionsMenu.End();
            Widgets.EndScrollView();
            base.Write();
        }
    }
}
