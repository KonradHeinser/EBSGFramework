using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class GenesAtSeverity
    {
        public float minSeverity = 0;

        public float maxSeverity = 999;

        public bool xenogenes = true;

        public List<GeneDef> genes;

        public FloatRange validSeverity = FloatRange.Zero;
    }
}
