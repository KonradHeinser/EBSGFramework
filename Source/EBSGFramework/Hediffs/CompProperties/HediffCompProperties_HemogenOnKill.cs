using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_HemogenOnKill : HediffCompProperties
    {
        // Only works on things that can bleed to avoid weird situations

        public FloatRange validSeverity = FloatRange.Zero;

        public float maxDistance = 4.9f;

        public float staticHemogenGain = 0f; // If not 0, this is used instead of a portion of the remaining blood in a corpse

        public float hemogenEfficiency = 1f; // % efficiency. At 1, for every 10% of bloodloss added to the corpse, gain 0.1 hemogen, or 10 in game, while at 2, that would result in a gain of 0.2

        public float maxGainableHemogen = 1f; // Max hemogen gainable from one corpse. Max bloodloss is this divided by efficiency

        public bool allowHumanoids = true;

        public bool allowDryads = true;

        public bool allowInsects = true;

        public bool allowAnimals = true;

        public bool allowEntities = true;

        public List<GeneDef> forbiddenTargetGenes;

        public bool useRelativeBodySize = true; // Causes the body size of the target and the killer to impact how much can be gained

        public HediffCompProperties_HemogenOnKill()
        {
            compClass = typeof(HediffComp_HemogenOnKill);
        }
    }
}
