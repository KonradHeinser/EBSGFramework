using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class GeneEffect
    {
        public GeneDef gene;
        public List<GeneDef> anyOfGene;
        public List<GeneDef> allOfGene;
        public float effect = 1;
        public StatDef statFactor;
    }
}
