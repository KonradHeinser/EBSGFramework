using Verse;
using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class EBSGUtilities
    {
        public static List<HediffDef> ApplyHediffs(Pawn pawn, HediffDef hediff = null, List<HediffDef> hediffs = null)
        {
            List<HediffDef> addedHediffs = null;
            if (hediff != null && pawn.health.hediffSet.HasHediff(hediff))
            {
                addedHediffs.Add(hediff);
                pawn.health.AddHediff(hediff);
            }

            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in hediffs)
                {
                    if (pawn.health.hediffSet.HasHediff(hediffDef))
                    {
                        addedHediffs.Add(hediffDef);
                        pawn.health.AddHediff(hediff);
                    }
                }
            }
            return addedHediffs;
        }

        public static void RemoveHediffs(Pawn pawn, HediffDef hediff = null, List<HediffDef> hediffs = null)
        {

            if (hediff != null && pawn.health.hediffSet.HasHediff(hediff))
            {
                Hediff hediffToRemove = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
                pawn.health.RemoveHediff(hediffToRemove);
            }

            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in hediffs)
                {
                    if (pawn.health.hediffSet.HasHediff(hediffDef)) pawn.health.AddHediff(hediff);
                }
            }
        }
    }
}
