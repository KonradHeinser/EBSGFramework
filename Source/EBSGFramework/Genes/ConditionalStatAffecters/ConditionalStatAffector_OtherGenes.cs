using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class ConditionalStatAffector_OtherGenes : ConditionalStatAffecter
    {
        public List<GeneDef> anyOfGenes;

        public List<GeneDef> allOfGenes;

        public List<GeneDef> noneOfGenes;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            return "EBSG_CorrectGenes".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Pawn != null)
                return req.Pawn.CheckGeneTrio(anyOfGenes, allOfGenes, noneOfGenes);
            return false;
        }
    }
}
