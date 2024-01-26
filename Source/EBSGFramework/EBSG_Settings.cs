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

        public static bool ageLimitedAgeless = ModsConfig.BiotechActive;

        public EBSG_Settings() { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ageLimitedAgeless, "ageLimitedAgeless");
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

            // Setting up scroll stuff now in case the list grows considerably in the future
            int numberOfOptions = 1; // One for each section
            if (showMainOptions) numberOfOptions += 1;
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

            optionsMenu.End();
            Widgets.EndScrollView();
            base.Write();
        }
    }
}
