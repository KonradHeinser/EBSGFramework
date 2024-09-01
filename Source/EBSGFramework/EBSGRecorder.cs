using System.Collections.Generic;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class EBSGRecorder : Def
    {
        public List<GeneDef> bloodfeederGenes;
        public List<GeneEvent> geneEvents;
        public List<GeneDef> hiddenGenes;
        public List<GeneTemplateDef> hiddenTemplates;
        public List<ThinkTreeSetting> thinkTreeSettings;
        public List<PawnKindDef> pawnKindsWithoutIntialRelationships;
    }
}
