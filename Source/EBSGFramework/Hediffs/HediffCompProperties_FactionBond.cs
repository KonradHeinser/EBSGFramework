using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_FactionBond : HediffCompProperties
    {
        // This sets the severity of the hediff to be equal to the number of pawns in the faction with the hediff. Not compatible with severity/day or any other severity changers
        public HediffCompProperties_FactionBond()
        {
            compClass = typeof(HediffComp_FactionBond);
        }
    }
}
