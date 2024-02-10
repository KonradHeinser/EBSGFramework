using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_CreateOtherHediffs : HediffComp
    {
        private HediffCompProperties_CreateOtherHediffs Props => (HediffCompProperties_CreateOtherHediffs)props;
        public bool removeHediff = false; // flag for removing hediff after removing a part to avoid killing the pawn in one hediff
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn.IsHashIntervalTick(200))
            {
                foreach (HediffsAtSeverities hediffSet in Props.hediffSets)
                {
                    if (parent.Severity >= hediffSet.minSeverity && parent.Severity <= hediffSet.maxSeverity)
                    {
                        if (hediffSet.hediffDef != null)
                        {
                            DoHediffStuff(hediffSet.hediffDef, hediffSet);
                        }
                        if (!hediffSet.hediffDefs.NullOrEmpty())
                        {
                            int ticker = 1;
                            foreach (HediffDef hediffDef in hediffSet.hediffDefs)
                            {
                                ticker++;
                                DoHediffStuff(hediffDef, hediffSet);
                            }
                        }
                    }
                }
                if (removeHediff) Pawn.health.RemoveHediff(parent);
            }
        }

        public void DoHediffStuff(HediffDef hediffDef, HediffsAtSeverities hediffSet)
        {

            float severityPerDay = hediffSet.severityPerDay * 0.003333334f;
            Hediff firstHediffOfDef = null;
            BodyPartRecord bodyPart = null;
            if (hediffSet.bodyPart != null) // If you need to target a specific body part, find it
            {
                foreach (BodyPartRecord notMissingPart in Pawn.health.hediffSet.GetNotMissingParts())
                {
                    if (notMissingPart.def == hediffSet.bodyPart)
                    {
                        bodyPart = notMissingPart;
                        break;
                    }
                }
                if (bodyPart == null) return; // If the part doesn't exist, then don't try to add hediffs to it for obvious reasons
                foreach (Hediff hediff in Pawn.health.hediffSet.hediffs) // Go through all the hediffs to try to find the hediff on the specified part
                {
                    if (hediff.Part == bodyPart && hediff.def == hediffDef) firstHediffOfDef = hediff;
                    break;
                }

            }
            else firstHediffOfDef = Pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef); // Just get the first hediff

            if (firstHediffOfDef != null) firstHediffOfDef.Severity += severityPerDay; // If the hediff exists, alter severity
            else
            {
                if (hediffDef == HediffDefOf.MissingBodyPart)
                {
                    Pawn.TakeDamage(new DamageInfo(EBSGDefOf.EBSG_GeneticDamage, 99999f, 999f, -1f, null, bodyPart));
                    removeHediff = true;
                }
                else
                {
                    firstHediffOfDef = Pawn.health.AddHediff(hediffDef, bodyPart);
                    firstHediffOfDef.Severity = hediffSet.initialSeverity; // Set the hediff severity
                }
            }
        }
    }
}
