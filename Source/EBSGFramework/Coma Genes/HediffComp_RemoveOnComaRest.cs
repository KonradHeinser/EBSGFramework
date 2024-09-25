using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class HediffComp_RemoveOnComaRest : HediffComp
    {
        HediffCompProperties_RemoveOnComaRest Props => (HediffCompProperties_RemoveOnComaRest)props;

        public bool Valid(GeneDef gene)
        {
            if (Props.relatedGenes.Contains(gene))
                return true;
            return false;
        }
    }
}
