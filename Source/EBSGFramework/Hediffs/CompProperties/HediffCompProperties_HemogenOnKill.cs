using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_HemogenOnKill : HediffCompProperties
    {
        public float staticHemogenGain = 0f; // If not 0, this is used instead of a portion of the remaining blood in a corpse

        public float hemogenEfficiency = 1f; // At 1, for every 10% of bloodloss added to the corpse, gain 0.1 hemogen, or 10 in game

        public float maxGainableHemogen = 1f; // Max amount drainable from one corpse

        public HediffCompProperties_HemogenOnKill()
        {
            compClass = typeof(HediffComp_HemogenOnKill);
        }
    }
}
