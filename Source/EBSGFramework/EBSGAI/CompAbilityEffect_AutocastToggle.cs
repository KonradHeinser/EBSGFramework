using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_AutocastToggle : CompAbilityEffect
    {
        public new CompProperties_AbilityAutocastToggle Props => (CompProperties_AbilityAutocastToggle)props;

        public bool autocast = true;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref autocast, "autocast", true);
        }
    }
}
