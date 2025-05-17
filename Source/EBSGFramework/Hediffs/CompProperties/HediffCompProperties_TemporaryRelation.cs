using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_TemporaryRelation : HediffCompProperties
    {
        public PawnRelationDef ownRelation; // This pawn views the other pawn

        public PawnRelationDef otherRelation; // How the other pawn views this pawn

        public HediffCompProperties_TemporaryRelation()
        {
            compClass = typeof(HediffComp_TemporaryRelation);
        }
    }
}
