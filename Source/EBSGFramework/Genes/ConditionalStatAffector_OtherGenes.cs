﻿using RimWorld;
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
            return EBSGUtilities.CheckGeneTrio(req.Pawn, anyOfGenes, allOfGenes, noneOfGenes);
        }
    }
}
