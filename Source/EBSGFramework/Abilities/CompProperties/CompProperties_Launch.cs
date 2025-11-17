using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_Launch : CompProperties_AbilityEffect
    {
        public int maxDistance = 10;

        public StatDef distanceFactorStat;
        
        public ThingDef skyfallerLeaving;

        public ThingDef pawnTransporter;

        public WorldObjectDef worldObject;

        public bool checkJammer = true;

        // This checks if the caravan is immobilized due to mass. Only matters if noMapTravelWhenTooMuchMass is false
        public bool noMapTravelWhileImmobilized = true;

        // This compares the total mass of everything, including animals, with the caster's max carry. Usually results in the pawn only being able to transport themselves and non-pawns
        public bool noMapTravelWhenTooMuchMass = true;

        public CompProperties_Launch()
        {
            compClass = typeof(CompAbilityEffect_Launch);
        }
    }
}
