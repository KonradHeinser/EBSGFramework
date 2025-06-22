using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByNearbyPawns : HediffCompProperties
    {
        // Overrides severity to be based on the number of nearby pawns. Default behaviour makes a severity of 1 mean that there is no nearby pawn

        public float range;
        public StatDef rangeStat;
        public bool onlyEnemies; // Only counts pawns that are hostile to the pawn
        public bool onlyDifferentFaction; // Only counts pawns that aren't in the player's faction
        public bool onlySameFaction; // Only counts pawns in the same faction
        public bool onlyHumanlikes; // Only count pawns that are considered humanlike by Rimworld
        public bool includeSelf = true; // If false, the hediff will be gone when no other pawns are nearby

        public HediffCompProperties_SeverityByNearbyPawns()
        {
            compClass = typeof(HediffComp_SeverityByNearbyPawns);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in ConfigErrors(parentDef))
                yield return error;

            if (onlyDifferentFaction && onlySameFaction)
                yield return "Both onlySameFaction and onlyDifferentFaction, which makes no sense for obvious reasons";
        }
    }
}
