using Verse;

namespace EBSGFramework
{
    public class Hediff_ModularAddedPart : Hediff_AddedPart
    {
        HediffComp_Modular cachedComp;
        public override HediffStage CurStage
        {
            get
            {
                var stage = base.CurStage ?? new HediffStage();

                if (cachedComp == null && !comps.NullOrEmpty())
                    cachedComp = comps.FirstOrDefault(c => c is HediffComp_Modular) as HediffComp_Modular;

                return cachedComp?.GetStage(stage) ?? base.CurStage;
            }
        }
    }
}
