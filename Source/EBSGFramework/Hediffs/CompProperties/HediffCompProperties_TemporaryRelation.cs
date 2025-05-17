using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_TemporaryRelation : HediffCompProperties
    {
        public PawnRelationDef ownRelation;

        public PawnRelationDef otherRelation;

        public HediffCompProperties_TemporaryRelation()
        {
            compClass = typeof(HediffComp_TemporaryRelation);
        }
    }
}
