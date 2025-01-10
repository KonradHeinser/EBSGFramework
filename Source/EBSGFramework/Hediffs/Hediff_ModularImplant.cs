using Verse;

namespace EBSGFramework
{
    public class Hediff_ModularImplant : Hediff_Implant
    {
        public override HediffStage CurStage
        {
            get
            {
                HediffStage stage = base.CurStage ?? new HediffStage();

                if (!comps.NullOrEmpty())
                    foreach (HediffComp c in comps)
                        if (c is HediffComp_Modular comp)
                            stage = comp.GetStage(stage);

                return stage;
            }
        }
    }
}
