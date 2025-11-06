using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityAlterGenes : CompProperties_AbilityEffect
    {
        public List<RandomXenoGenes> geneSets; // Named as such because it was originally solely to create entire xenotypes

        public bool removeGenesFromOtherLists = true; // This being true means that while activating, the comp will remove any gene that exists on the other list(s), even if they are from the xenotype

        public bool inheritable = true; // The default behaviour is to make the genes inheritable(germline)

        public List<GeneDef> alwaysAddedGenes; // These genes will be added regardless of the set picked

        public List<GeneDef> alwaysRemovedGenes; // These genes will be removed regardless of set picked

        public bool showMessage = false;

        public CompProperties_AbilityAlterGenes()
        {
            compClass = typeof(CompAbilityEffect_AlterGenes);
        }
    }
}
