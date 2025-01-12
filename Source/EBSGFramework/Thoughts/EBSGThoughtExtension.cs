using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class EBSGThoughtExtension : DefModExtension
    {
        public bool compoundingHatred = false; // When true, each gene that is found in checked genes increases the stage
        public bool opinionOfAllOthers = false; // Makes the thought apply to all others instead of just those with checked genes. Intended for xenophobe/xenophile
        public bool xenophilobic = true;
        public bool checkNotPresent = false;

        public List<GeneDef> relatedGenes;
        public List<GeneDef> nullifyingGenes; // Genes checked for early nullification
        public List<GeneDef> requiredGenes; // The observer must have one of these genes to feel anything. Acts as a reverse nullifyingGenes

        public float maxWaterDistance = 0; // If above 0, this extends the search radius
        public List<int> thresholds;
        public int waterTilesNeeded = 1; // Normally just stops after the first tile

        public SimpleCurve curve = null;
        public HediffDef hediff = null;
    }
}
