using System;
using Verse;
using System.Collections.Generic;

// I made this because the VFE hediff adder makes the hediffs pop up again if a new gene is added through any other method, like a gene randomizing hediff as a wild example.
namespace EBSGFramework 
{
    public class HediffAdder : Gene
    {
        public Dictionary<BodyPartDef, int> foundParts = new Dictionary<BodyPartDef, int>();

        public override void PostAdd()
        {
            base.PostAdd();
            EBSGExtension extension = def.GetModExtension<EBSGExtension>();
            if (extension != null && !extension.hediffsToApply.NullOrEmpty())
            {
                foreach (HediffsToParts hediffToParts in extension.hediffsToApply)
                {
                    foundParts.Clear();
                    if (!hediffToParts.bodyParts.NullOrEmpty())
                    {
                        foreach (BodyPartDef bodyPartDef in hediffToParts.bodyParts)
                        {
                            if (pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).NullOrEmpty()) continue;
                            if (foundParts.NullOrEmpty() || !foundParts.ContainsKey(bodyPartDef))
                            {
                                foundParts.Add(bodyPartDef, 0);
                            }
                            Log.Message(pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).ToArray()[foundParts[bodyPartDef]].Label);
                            if (hediffToParts.onlyIfNew) EBSGUtilities.AddHediffToPart(pawn, pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).ToArray()[foundParts[bodyPartDef]], hediffToParts.hediff, hediffToParts.severity);
                            else EBSGUtilities.AddHediffToPart(pawn, pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).ToArray()[foundParts[bodyPartDef]], hediffToParts.hediff, hediffToParts.severity, hediffToParts.severity);
                            foundParts[bodyPartDef]++;
                        }
                    }
                    else
                    {
                        if (EBSGUtilities.HasHediff(pawn, hediffToParts.hediff))
                        {
                            if (hediffToParts.onlyIfNew) continue;
                            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffToParts.hediff);
                            hediff.Severity += hediffToParts.severity;
                        }
                        else
                        {
                            EBSGUtilities.AddOrAppendHediffs(pawn, hediffToParts.severity, 0, hediffToParts.hediff);
                        }
                    }
                }
                if (extension.vanishingGene) pawn.genes.RemoveGene(this);
            }
            else
            {
                Log.Error(def + " could not find the hediffs to add list.");
            }
        }
    }
}
