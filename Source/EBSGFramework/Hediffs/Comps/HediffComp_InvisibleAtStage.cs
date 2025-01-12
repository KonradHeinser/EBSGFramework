using Verse;

namespace EBSGFramework
{
    public class HediffComp_InvisibleAtStage : HediffComp
    {
        HediffCompProperties_InvisibleAtStage Props => (HediffCompProperties_InvisibleAtStage)props;

        public override bool CompDisallowVisible()
        {
            return Props.invisibleStages.Contains(parent.CurStageIndex + 1);
        }
    }
}
