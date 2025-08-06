using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityDestroyItem : CompProperties_AbilityEffect
    {
        public List<List<ThingLink>> options;

        public bool addCostToStatSummary = true;

        public CompProperties_AbilityDestroyItem()
        {
            compClass = typeof(CompAbilityEffect_DestroyItem);
        }

        public override IEnumerable<string> ExtraStatSummary()
        {
            if (!options.NullOrEmpty() && addCostToStatSummary)
            {
                yield return "";
                yield return "Cost".Translate().CapitalizeFirst() + ":";
                foreach (var option in options)
                {
                    string text = " - " + "AnyOf".Translate() + ": ";
                    foreach (var link in option)
                        text += link.ToString() + ", ";
                    yield return text.Remove(text.Length - 2); // Lops off the last ", "
                }
            }
        }
    }
}
