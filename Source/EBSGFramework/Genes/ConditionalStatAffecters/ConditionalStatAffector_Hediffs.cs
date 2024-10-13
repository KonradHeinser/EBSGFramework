using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class ConditionalStatAffector_Hediffs : ConditionalStatAffecter
    {
        public List<HediffDef> anyOfHediffs;

        public List<HediffDef> allOfHediffs;

        public List<HediffDef> noneOfHediffs;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            return "EBSG_CorrectHediffs".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            return EBSGUtilities.CheckHediffTrio(req.Pawn, anyOfHediffs, allOfHediffs, noneOfHediffs);
        }
    }
}
