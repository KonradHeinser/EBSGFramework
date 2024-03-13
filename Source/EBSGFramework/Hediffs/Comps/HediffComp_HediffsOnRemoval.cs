using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffComp_HediffsOnRemoval : HediffComp
    {
        public HediffCompProperties_HediffsOnRemoval Props => (HediffCompProperties_HediffsOnRemoval)props;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (!Props.hediffsToGive.NullOrEmpty())
                foreach (HediffToGive hediffToGive in Props.hediffsToGive)
                    PostRemoveInner(Pawn, hediffToGive);
        }

        protected void PostRemoveInner(Pawn pawn, HediffToGive hediffToGive)
        {
            if (pawn == null) return;

            if (hediffToGive.bodyParts.NullOrEmpty())
            {
                if (hediffToGive.replaceExisting)
                {
                    if (hediffToGive.hediffDef != null)
                    {
                        Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediffToGive.hediffDef);
                        if (firstHediffOfDef != null)
                            pawn.health.RemoveHediff(firstHediffOfDef);
                    }

                    if (!hediffToGive.hediffDefs.NullOrEmpty() && EBSGUtilities.PawnHasAnyOfHediffs(pawn, hediffToGive.hediffDefs))
                        foreach (HediffDef hediff in hediffToGive.hediffDefs)
                        {
                            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediffToGive.hediffDef);
                            if (firstHediffOfDef != null)
                                pawn.health.RemoveHediff(firstHediffOfDef);
                        }
                }

                EBSGUtilities.AddOrAppendHediffs(pawn, hediffToGive.severity, hediffToGive.severity, hediffToGive.hediffDef, hediffToGive.hediffDefs);
            }
            else
            {
                Dictionary<BodyPartDef, int> foundParts = new Dictionary<BodyPartDef, int>();

                foreach (BodyPartDef bodyPartDef in hediffToGive.bodyParts)
                {
                    if (pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).NullOrEmpty()) continue;
                    if (foundParts.NullOrEmpty() || !foundParts.ContainsKey(bodyPartDef))
                        foundParts.Add(bodyPartDef, 0);

                    if (hediffToGive.hediffDef != null)
                        EBSGUtilities.AddHediffToPart(pawn, pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).ToArray()[foundParts[bodyPartDef]], hediffToGive.hediffDef, hediffToGive.severity, hediffToGive.severity, hediffToGive.replaceExisting);
                    if (!hediffToGive.hediffDefs.NullOrEmpty())
                        foreach (HediffDef hediff in hediffToGive.hediffDefs)
                            EBSGUtilities.AddHediffToPart(pawn, pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).ToArray()[foundParts[bodyPartDef]], hediff, hediffToGive.severity, hediffToGive.severity, hediffToGive.replaceExisting);
                    foundParts[bodyPartDef]++;
                }
            }
        }
    }
}
