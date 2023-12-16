using System;
using Verse;
using System.Collections.Generic;

// I made this because the VFE hediff adder makes the hediffs pop up again if a new gene is added through any other method, like a gene randomizing hediff as a wild example.
namespace EBSGFramework 
{
    public class HediffAdder : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            EBSGExtension extension = def.GetModExtension<EBSGExtension>();
            if (extension != null && !extension.hediffsToApply.NullOrEmpty())
            {
                foreach (HediffsToParts hediffToParts in extension.hediffsToApply)
                {
                    if (!hediffToParts.bodyParts.NullOrEmpty())
                    {
                        List<BodyPartRecord> alreadyTargettedParts = new List<BodyPartRecord>();
                        foreach (BodyPartDef bodyPartDef in hediffToParts.bodyParts)
                        {
                            Hediff firstHediffOfDef = null;
                            BodyPartRecord bodyPart = null;
                            foreach (BodyPartRecord notMissingPart in pawn.health.hediffSet.GetNotMissingParts())
                            {
                                if (notMissingPart.def == bodyPartDef && (alreadyTargettedParts.NullOrEmpty() || !alreadyTargettedParts.Contains(notMissingPart)))
                                {
                                    alreadyTargettedParts.Add(notMissingPart);
                                    bodyPart = notMissingPart;
                                    break;
                                }
                            }
                            if (bodyPart == null) continue; // If no part is found, just "continue" down the list
                            if (pawn.health.hediffSet?.HasHediff(hediffToParts.hediff) == true)
                            {
                                Hediff testHediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffToParts.hediff);
                                if (testHediff.Part.def == bodyPart.def) firstHediffOfDef = testHediff;
                                else
                                {
                                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs) // Go through all the hediffs to try to find the hediff on the specified part
                                    {
                                        if (hediff.Part == bodyPart && hediff.def == hediffToParts.hediff) firstHediffOfDef = hediff;
                                        break;
                                    }
                                }
                            }

                            if (firstHediffOfDef != null)
                            {
                                if (hediffToParts.onlyIfNew) continue;
                                try
                                {
                                    try
                                    {
                                        Hediff_Psylink hediff_Level = (Hediff_Psylink)firstHediffOfDef;
                                        hediff_Level.ChangeLevel((int)Math.Ceiling(hediffToParts.severity), false);
                                    }
                                    catch
                                    {
                                        Hediff_Level hediff_Level = (Hediff_Level)firstHediffOfDef;
                                        hediff_Level.ChangeLevel((int)Math.Ceiling(hediffToParts.severity));
                                    }
                                }
                                catch
                                {
                                    firstHediffOfDef.Severity += hediffToParts.severity;
                                }
                            }
                            else
                            {
                                firstHediffOfDef = pawn.health.AddHediff(hediffToParts.hediff, bodyPart);
                                firstHediffOfDef.Severity = hediffToParts.severity;
                            }
                        }
                    }
                    else
                    {
                        if (pawn.health.hediffSet?.HasHediff(hediffToParts.hediff) == true)
                        {
                            if (hediffToParts.onlyIfNew) continue;
                            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffToParts.hediff);
                            hediff.Severity += hediffToParts.severity;
                        }
                        else
                        {
                            pawn.health.AddHediff(hediffToParts.hediff);
                            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffToParts.hediff);
                            hediff.Severity = hediffToParts.severity;
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
