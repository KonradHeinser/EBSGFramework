using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    // Named as such because this was originally solely for spawn age limiting, it now serves as a universal base for various things
    public class SpawnAgeLimiter : Gene
    {
        public int mutationDelayTicks = 5;

        public int cachedGeneCount = 0;

        public List<AbilityDef> addedAbilities;

        public EBSGExtension cachedExtension;

        public EBSGExtension Extension
        {
            get
            {
                if (cachedExtension == null)
                {
                    cachedExtension = def.GetModExtension<EBSGExtension>();
                }
                return cachedExtension;
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            if (Extension != null)
            {
                if (addedAbilities == null) addedAbilities = new List<AbilityDef>();
                LimitAge(pawn, Extension.expectedAges, Extension.ageRange, Extension.sameBioAndChrono);
                if (!Extension.mutationGeneSets.NullOrEmpty() && mutationDelayTicks >= 5) mutationDelayTicks = Extension.delayTicks;
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (pawn.IsHashIntervalTick(200))
            {
                if (Extension != null && !Extension.geneAbilities.NullOrEmpty() && pawn.genes != null && pawn.genes.GenesListForReading.Count != cachedGeneCount)
                {
                    if (addedAbilities == null) addedAbilities = new List<AbilityDef>();
                    addedAbilities = AbilitiesWithCertainGenes(pawn, Extension.geneAbilities, addedAbilities);
                    cachedGeneCount = pawn.genes.GenesListForReading.Count;
                }
            }

            // Mutation should always be the last thing processed, along with anything else attached to the timer
            if (mutationDelayTicks < 0) return;
            if (mutationDelayTicks == 0)
            {
                if (Extension != null)
                {
                    if (!Extension.mutationGeneSets.NullOrEmpty()) EBSGUtilities.GainRandomGeneSet(pawn, Extension.inheritable, Extension.removeGenesFromOtherLists, Extension.mutationGeneSets);
                }
            }
            mutationDelayTicks--;
        }

        public static void LimitAge(Pawn pawn, FloatRange expectedAges, FloatRange ageRange, bool sameBioAndChrono = false, bool removeChronic = true)
        {
            if (ageRange.max > 0)
            {
                float currentBioAge = pawn.ageTracker.AgeBiologicalYearsFloat;
                float currentChronoAge = pawn.ageTracker.AgeChronologicalYearsFloat;
                if (!expectedAges.Includes(currentBioAge))
                {
                    currentBioAge = ageRange.RandomInRange;
                    pawn.ageTracker.AgeBiologicalTicks = (long)(currentBioAge * 3600000f);
                }
                if (sameBioAndChrono && currentBioAge != currentChronoAge)
                {
                    pawn.ageTracker.AgeChronologicalTicks = (long)(currentBioAge * 3600000f);
                }
                if (removeChronic) EBSGUtilities.RemoveChronicHediffs(pawn);
            }
        }

        public static List<AbilityDef> AbilitiesWithCertainGenes(Pawn pawn, List<AbilityAndGeneLink> geneAbilities, List<AbilityDef> addedAbilities)
        {
            List<AbilityDef> abilitiesToAdd = new List<AbilityDef>();

            EBSGUtilities.RemovePawnAbilities(pawn, addedAbilities);

            foreach (AbilityAndGeneLink link in geneAbilities)
            {
                if (link.abilities.NullOrEmpty()) continue;
                if (EBSGUtilities.PawnHasAnyOfGenes(pawn, link.requireOneOfGenes) && (link.forbiddenGenes.NullOrEmpty() || !EBSGUtilities.PawnHasAnyOfGenes(pawn, link.forbiddenGenes)) && EBSGUtilities.PawnHasAllOfGenes(pawn, link.requiredGenes))
                {
                    foreach (AbilityDef ability in link.abilities) abilitiesToAdd.Add(ability);
                }
            }

            return EBSGUtilities.GivePawnAbilities(pawn, abilitiesToAdd);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref mutationDelayTicks, "EBSG_MutationDelayTicks", 5);
            Scribe_Collections.Look(ref addedAbilities, "EBSG_AddedAbilities");
        }
    }
}
