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
                if (!req.Pawn.CapacityConditionsMet(capLimiters)) return false;
                if (!req.Pawn.AllNeedLevelsMet(needLevels)) return false;
                if (!req.Pawn.AllSkillLevelsMet(skillLimiters)) return false;
                if (!req.Pawn.CheckHediffTrio(anyOfHediffs, allOfHediffs, noneOfHediffs)) return false;
                if (!req.Pawn.CheckGeneTrio(anyOfGenes, allOfGenes, noneOfGenes)) return false;

                return true;
            }

            return false;
        }
    }
}
