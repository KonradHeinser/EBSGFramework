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
    }
}
