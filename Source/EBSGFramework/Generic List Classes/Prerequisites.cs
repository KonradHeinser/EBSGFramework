using System.Collections.Generic;
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
            
            return true;
        }
    }
}
