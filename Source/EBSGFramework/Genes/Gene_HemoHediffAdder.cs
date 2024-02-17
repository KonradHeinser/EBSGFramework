using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class Gene_HemoHediffAdder : Gene_HemogenDrain
    {
        public int delayTicks = 5;

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
            HediffAdder.HediffAdding(pawn, this);
            if (Extension != null)
            {
                if (addedAbilities == null) addedAbilities = new List<AbilityDef>();
                SpawnAgeLimiter.LimitAge(pawn, Extension.expectedAges, Extension.ageRange, Extension.sameBioAndChrono);
                if (!Extension.mutationGeneSets.NullOrEmpty() && delayTicks >= 5) delayTicks = Extension.delayTicks;
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
                    addedAbilities = SpawnAgeLimiter.AbilitiesWithCertainGenes(pawn, Extension.geneAbilities, addedAbilities);
                    cachedGeneCount = pawn.genes.GenesListForReading.Count;
                }
            }

            // Mutation should always be the last thing processed, along with anything else attached to the timer
            if (delayTicks < 0) return;
            if (delayTicks == 0)
            {
                if (Extension != null)
                {
                    if (!Extension.mutationGeneSets.NullOrEmpty()) EBSGUtilities.GainRandomGeneSet(pawn, Extension.inheritable, Extension.removeGenesFromOtherLists, Extension.mutationGeneSets);
                }
            }
            delayTicks--;
        }

        public override void PostRemove()
        {
            base.PostRemove();
            HediffAdder.HediffRemoving(pawn, this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref delayTicks, "EBSG_HemoMutationDelayTicks", 5);
            Scribe_Collections.Look(ref addedAbilities, "EBSG_HemoAddedAbilities");
        }
    }
}
