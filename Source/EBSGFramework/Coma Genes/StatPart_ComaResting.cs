using System.Collections.Generic;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class StatPart_ComaResting : StatPart
    {
        public List<GeneDef> relatedGenes;

        public float factor;

        public override void TransformValue(StatRequest req, ref float val)
        {
            Need_ComaGene comaNeed = req.Pawn?.needs?.TryGetNeed<Need_ComaGene>();
            if (comaNeed != null && EBSGUtilities.HasAnyOfRelatedGene(req.Pawn, relatedGenes))
            {
                if (relatedGenes.Contains(comaNeed.ComaGene.def) && comaNeed.Comatose)
                {
                    val *= factor;
                    return;
                }
                foreach (Need need in req.Pawn.needs.AllNeeds)
                    if (need is Need_ComaGene coma && relatedGenes.Contains(coma.ComaGene.def) && coma.Comatose)
                    {
                        val *= factor;
                        return;
                    }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            Need_ComaGene comaNeed = req.Pawn?.needs?.TryGetNeed<Need_ComaGene>();
            if (comaNeed != null && EBSGUtilities.HasAnyOfRelatedGene(req.Pawn, relatedGenes))
            {
                if (relatedGenes.Contains(comaNeed.ComaGene.def) && comaNeed.Comatose)
                    return "EBSG_Coma".Translate().CapitalizeFirst() + ": x" + factor.ToStringPercent();

                foreach (Need need in req.Pawn.needs.AllNeeds)
                    if (need is Need_ComaGene coma && relatedGenes.Contains(coma.ComaGene.def) && coma.Comatose)
                        return "EBSG_Coma".Translate().CapitalizeFirst() + ": x" + factor.ToStringPercent();
            }
            return null;
        }
    }
}

