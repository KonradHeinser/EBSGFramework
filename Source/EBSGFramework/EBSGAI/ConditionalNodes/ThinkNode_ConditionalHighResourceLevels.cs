using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalHighResourceLevels : ThinkNode_Conditional
    {
        private float minLevel = 0.9f;
        private bool useTargetValue = true;
        private GeneDef gene = null;
        protected override bool Satisfied(Pawn pawn)
        {
            if (!pawn.HasRelatedGene(gene)) return false;

            if (pawn.genes.GetGene(gene) is Gene_Resource resourceGene)
            {
                if (useTargetValue) 
                    return resourceGene.Value >= resourceGene.targetValue;
                return resourceGene.Value >= minLevel;
            }
            Log.Error(gene + " doesn't appear to be a resource gene");
            return false;
        }
    }
}
