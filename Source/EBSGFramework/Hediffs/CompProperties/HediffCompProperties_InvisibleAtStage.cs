using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_InvisibleAtStage : HediffCompProperties
    {
        public List<int> invisibleStages;
        public HediffCompProperties_InvisibleAtStage()
        {
            compClass = typeof(HediffComp_InvisibleAtStage);
        }
    }
}
