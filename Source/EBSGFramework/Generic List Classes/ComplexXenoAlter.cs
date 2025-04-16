using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ComplexXenoAlter
    {
        public FloatRange severity;

        public XenotypeDef xenotype;

        public List<GeneDef> addGenes;

        public List<GeneDef> removeGenes;

        public ThingDef filth;

        public IntRange filthCount = new IntRange(4, 7);

        public bool setXenotype = true; // If False, creates a blend of xenos instead of completely replacing genes
    }
}
