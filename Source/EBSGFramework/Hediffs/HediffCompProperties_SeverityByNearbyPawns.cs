using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByNearbyPawns : HediffCompProperties
    {
        // Overrides severity to be based on the number of nearby pawns. Default behaviour makes a severity of 1 mean that there is no nearby pawn

        public float range;
        public bool onlyNonPlayer; // Only counts pawns that aren't in the player's faction
        public bool onlyHumanlikes; // Only count pawns that are considered humanlike by Rimworld
        public bool onlyPlayer; // Only counts pawns in the player's faction
        public bool includeSelf = true; // If false, the hediff will be gone when no other pawns are nearby

        public HediffCompProperties_SeverityByNearbyPawns()
        {
            compClass = typeof(HediffComp_SeverityByNearbyPawns);
        }
    }
}
