using System.Collections.Generic;
using RimWorld;
using Verse;

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

        public override bool Active
        {
            get
            {
                if (!base.Active) return false;

                if (Extension != null)
                {
                    // Make it active for the remove past age to force the removal on tick
                    if (Extension.removePastAge > 0 && pawn.ageTracker.AgeBiologicalYearsFloat > Extension.removePastAge)
                        return true;
                    if (Extension.maxAgeActive > 0 && pawn.ageTracker.AgeBiologicalYearsFloat > Extension.maxAgeActive)
                        return false;
                }

                return true;
            }
        }

        public override void PostAdd()
        {
            if (!Active || Overridden) return;
            base.PostAdd();
            HediffAdder.HediffAdding(pawn, this);
            if (Extension != null)
            {
                if (addedAbilities == null) addedAbilities = new List<AbilityDef>();
                SpawnAgeLimiter.GetGender(pawn, Extension);
                SpawnAgeLimiter.LimitAge(pawn, Extension.expectedAges, Extension.ageRange, Extension.sameBioAndChrono);
                if (!Extension.mutationGeneSets.NullOrEmpty() && delayTicks >= 5) delayTicks = Extension.delayTicks;

                if (!Extension.hediffsToApplyAtAges.NullOrEmpty())
                    pawn.AddHediffToParts(Extension.hediffsToApplyAtAges, null, true);
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (pawn.IsHashIntervalTick(200))
            {
                if (Extension != null && !Extension.geneAbilities.NullOrEmpty() && pawn.genes.GenesListForReading.Count != cachedGeneCount)
                {
                    if (addedAbilities == null) addedAbilities = new List<AbilityDef>();
                    addedAbilities = SpawnAgeLimiter.AbilitiesWithCertainGenes(pawn, Extension.geneAbilities, addedAbilities);
                    cachedGeneCount = pawn.genes.GenesListForReading.Count;
                }
            }

            if (pawn.IsHashIntervalTick(2500))
            {
                if (Extension != null)
                {
                    Dictionary<BodyPartDef, int> foundParts = new Dictionary<BodyPartDef, int>();

                    if (Extension.removePastAge > 0 && pawn.ageTracker.AgeBiologicalYearsFloat > Extension.removePastAge)
                    {
                        pawn.genes.RemoveGene(this);
                        return;
                    }

                    if (!Extension.hediffsToApplyAtAges.NullOrEmpty())
                        foreach (HediffToParts hediffToParts in Extension.hediffsToApplyAtAges)
                            if (hediffToParts.validAges.ValidValue(pawn.ageTracker.AgeBiologicalYearsFloat))
                                if (hediffToParts.bodyParts.NullOrEmpty())
                                    if (pawn.HasHediff(hediffToParts.hediff))
                                    {
                                        if (hediffToParts.onlyIfNew) continue;
                                        Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffToParts.hediff);
                                        hediff.Severity += hediffToParts.severity;
                                    }
                                    else
                                        pawn.AddOrAppendHediffs(hediffToParts.severity, 0, hediffToParts.hediff);
                                else
                                {
                                    foundParts.Clear();
                                    if (!hediffToParts.bodyParts.NullOrEmpty())
                                        foreach (BodyPartDef bodyPartDef in hediffToParts.bodyParts)
                                        {
                                            if (pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).NullOrEmpty()) continue;
                                            if (foundParts.NullOrEmpty() || !foundParts.ContainsKey(bodyPartDef))
                                            {
                                                foundParts.Add(bodyPartDef, 0);
                                            }
                                            pawn.AddHediffToPart(pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).ToArray()[foundParts[bodyPartDef]], hediffToParts.hediff, hediffToParts.severity, hediffToParts.severity, hediffToParts.onlyIfNew);
                                            foundParts[bodyPartDef]++;
                                        }
                                }
                            else
                                pawn.RemoveHediffsFromParts(null, hediffToParts);

                    if (!Extension.genderByAge.NullOrEmpty() && (Extension.genderByAge.Count > 1 || Extension.genderByAge[0].range != GenderByAge.defaultRange))
                        SpawnAgeLimiter.GetGender(pawn, Extension);
                }
            }

            // Mutation should always be the last thing processed, along with anything else attached to the timer
            if (delayTicks < 0) return;
            if (delayTicks == 0 && Extension?.mutationGeneSets.NullOrEmpty() == false)
                pawn.GainRandomGeneSet(Extension.inheritable, Extension.removeGenesFromOtherLists, Extension.mutationGeneSets);
            delayTicks--;
        }

        public override void PostRemove()
        {
            base.PostRemove();
            HediffAdder.HediffRemoving(pawn, this);

            if (Extension != null && !Extension.hediffsToApplyAtAges.NullOrEmpty())
                pawn.RemoveHediffsFromParts(Extension.hediffsToApplyAtAges);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref delayTicks, "EBSG_HemoMutationDelayTicks", 5);
            Scribe_Collections.Look(ref addedAbilities, "EBSG_HemoAddedAbilities");
        }
    }
}
