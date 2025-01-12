using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ThoughtWorker_HediffSeverity : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            EBSGThoughtExtension thoughtExtension = def.GetModExtension<EBSGThoughtExtension>();

            if (thoughtExtension?.hediff != null && thoughtExtension.curve != null)
            {
                if (p.HasHediff(thoughtExtension.hediff))
                    return ThoughtState.ActiveDefault;
            }

            return ThoughtState.Inactive;
        }

        public override string PostProcessLabel(Pawn p, string label)
        {
            EBSGThoughtExtension thoughtExtension = def.GetModExtension<EBSGThoughtExtension>();
            if (p.health.hediffSet.TryGetHediff(thoughtExtension.hediff, out var hediff))
            {
                if (hediff is Hediff_Dependency dependency)
                    return label.Formatted(p.Named("PAWN"), dependency.Label.Named("HEDIFF"), dependency.GetLabel().Named("DEPENDENCY"));
                return label.Formatted(p.Named("PAWN"), hediff.Label.Named("HEDIFF"));
            }
            return base.PostProcessLabel(p, label);
        }
    }
}
