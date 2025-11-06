using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_GeneticBed : CompProperties_AssignableToPawn
    {
        public List<GeneDef> anyOfGenes;

        public List<GeneDef> allOfGenes;

        public List<GeneDef> noneOfGenes;

        public CompProperties_GeneticBed()
        {
            compClass = typeof(CompAssignableToPawn_GeneticBed);
        }
    }
}
