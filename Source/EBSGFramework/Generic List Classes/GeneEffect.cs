using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class GeneEffect
    {
        public GeneDef gene;
        public List<GeneDef> anyOfGene;
        public List<GeneDef> allOfGene;
        public float effect = 1;
        public StatDef statFactor;
        public float offset = 0;
    }
}
