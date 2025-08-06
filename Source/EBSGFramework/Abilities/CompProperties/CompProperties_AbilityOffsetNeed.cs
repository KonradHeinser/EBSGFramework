using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityOffsetNeed : CompProperties_AbilityEffect
    {
        public List<NeedOffset> needOffsets;
        public List<NeedOffset> casterNeedOffsets;
        public bool preventRepeats = true; // Only used for randoms to prevent repeats

        public CompProperties_AbilityOffsetNeed()
        {
            compClass = typeof(CompAbilityEffect_OffsetNeed);
        }


        public override IEnumerable<string> ExtraStatSummary()
        {
            if (!needOffsets.NullOrEmpty())
            {
                yield return "";
                yield return "EBSG_NeedOffsets".Translate();
                foreach (NeedOffset needOffset in needOffsets)
                {
                    string buildString = " - ";
                    if (needOffset.need == null) buildString += "EBSG_Random".Translate();
                    else buildString += needOffset.need.LabelCap;
                    buildString += " ";

                    if (needOffset.offset < 0) buildString += needOffset.offset;
                    else buildString += "+" + needOffset.offset;
                    yield return buildString;
                }
            }

            if (!casterNeedOffsets.NullOrEmpty())
            {
                yield return "";
                yield return "EBSG_CasterNeedOffsets".Translate();
                foreach (NeedOffset needOffset in casterNeedOffsets)
                {
                    string buildString = " - ";
                    if (needOffset.need == null) buildString += "EBSG_Random".Translate();
                    else buildString += needOffset.need.LabelCap;
                    buildString += " ";

                    if (needOffset.offset < 0) buildString += needOffset.offset;
                    else buildString += "+" + needOffset.offset;
                    yield return buildString;
                }
            }
        }
    }
}
