using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_Launch : CompProperties_AbilityEffect
    {
        public int maxDistance = 10;

        public ThingDef skyfallerLeaving;

        public WorldObjectDef worldObject;

        // This checks if the caravan is immobilized due to mass. Only matters if noMapTravelWhenTooMuchMass is false
        public bool noMapTravelWhileImobilized = true;

        // This compares the total mass of everything, including animals, with the caster's max carry. Usually results in the pawn only being able to transport themselves and non-pawns
        public bool noMapTravelWhenTooMuchMass = true;

        public CompProperties_Launch()
        {
            compClass = typeof(CompAbilityEffect_Launch);
        }
    }
}
