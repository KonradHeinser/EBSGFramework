using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_PawnMulti : ConditionalStatAffecter
    {
        public List<CapCheck> capLimiters;

        public List<NeedLevel> needLevels;

        public List<SkillCheck> skillLimiters;

        public List<HediffDef> anyOfHediffs;

        public List<HediffDef> allOfHediffs;

        public List<HediffDef> noneOfHediffs;

        public List<GeneDef> anyOfGenes;

        public List<GeneDef> allOfGenes;

        public List<GeneDef> noneOfGenes;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            return "EBSG_CorrectConditions".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Pawn != null)
            {
                if (!EBSGUtilities.CapacityConditionsMet(req.Pawn, capLimiters)) return false;
                if (!EBSGUtilities.AllNeedLevelsMet(req.Pawn, needLevels)) return false;
                if (!EBSGUtilities.AllSkillLevelsMet(req.Pawn, skillLimiters)) return false;
                if (!EBSGUtilities.CheckHediffTrio(req.Pawn, anyOfHediffs, allOfHediffs, noneOfHediffs)) return false;
                if (!EBSGUtilities.CheckGeneTrio(req.Pawn, anyOfGenes, allOfGenes, noneOfGenes)) return false;

                return true;
            }

            return false;
        }
    }
}
