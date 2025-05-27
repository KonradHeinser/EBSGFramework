using Verse;

namespace EBSGFramework
{
    public class Hediff_ModularImplant : Hediff_Implant
    {
        HediffComp_Modular cachedComp;
        public override HediffStage CurStage
        {
            get
            {
                HediffStage stage = base.CurStage ?? new HediffStage();

                if (cachedComp == null && !comps.NullOrEmpty())
                    foreach (HediffComp c in comps)
                        if (c is HediffComp_Modular comp)
                        {
                            cachedComp = comp;
                            break;
                        }

                return cachedComp?.GetStage(stage) ?? base.CurStage;
            }
        }
    }
}
