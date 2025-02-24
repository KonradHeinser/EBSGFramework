using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_UseEffectAlterGenes : CompProperties_UseEffect
    {
        public List<GeneDef> alwaysAddedGenes;

        public bool inheritable = true;

        public List<GeneDef> alwaysRemovedGenes;

        public List<RandomXenoGenes> geneSets;

        public bool removeGenesFromOtherLists = true;

        public bool showMessage = true;

        public Prerequisites prerequisites;

        public CompProperties_UseEffectAlterGenes()
        {
            compClass = typeof(CompUseEffect_AlterGenes);
        }
    }
}
