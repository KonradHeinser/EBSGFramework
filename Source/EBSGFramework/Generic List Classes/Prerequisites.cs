using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class Prerequisites
    {
        public List<HediffDef> hasAnyOfHediffs;
        public List<HediffDef> hasAllOfHediffs;
        public List<HediffDef> hasNoneOfHediffs;
        public bool samePartPrerequisites = false;
        public List<GeneDef> hasAnyOfGenes;
        public List<GeneDef> hasAllOfGenes;
        public List<GeneDef> hasNoneOfGenes;
        public List<XenotypeDef> isAnyOfXenotype;
        public List<XenotypeDef> isNoneOfXenotype;
        public string notMetString = "EBSG_ConditionsNotMet";

        public bool ValidPawn(Pawn pawn, BodyPartRecord bodyPart = null)
        {
            if (samePartPrerequisites)
            {
                if (!pawn.CheckHediffTrio(hasAnyOfHediffs, hasAllOfHediffs, hasNoneOfHediffs, bodyPart)) return false;
            }
            else
                if (!pawn.CheckHediffTrio(hasAnyOfHediffs, hasAllOfHediffs, hasNoneOfHediffs)) return false;

            if (!pawn.CheckGeneTrio(hasAnyOfGenes, hasAllOfGenes, hasNoneOfGenes)) return false;
            
            if (pawn.genes?.Xenotype != null)
            {
                if (!isAnyOfXenotype.NullOrEmpty() && !isAnyOfXenotype.Contains(pawn.genes.Xenotype))
                    return false;
                if (!isNoneOfXenotype.NullOrEmpty() && isNoneOfXenotype.Contains(pawn.genes.Xenotype))
                    return false;
            }
            else if (!isAnyOfXenotype.NullOrEmpty()) 
                return false;

            return true;
        }
    }
}
