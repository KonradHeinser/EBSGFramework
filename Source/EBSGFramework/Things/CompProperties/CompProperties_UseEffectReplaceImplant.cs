using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_UseEffectReplaceImplant: CompProperties_UseEffectInstallImplant
    {
        public List<HediffDef> replacedHediffs;
        
        public CompProperties_UseEffectReplaceImplant()
        {
            compClass = typeof(CompUseEffect_ReplaceImplant);
        }
    }
}