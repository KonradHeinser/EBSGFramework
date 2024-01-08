using Verse;
using System.Collections.Generic;
using System;
using RimWorld;

namespace EBSGFramework
{
    public class EBSGUtilities
    {
        public static List<PawnCapacityDef> cachedLethalCapacities = new List<PawnCapacityDef>();

        public static List<PawnCapacityDef> LethalCapacities
        {
            get
            {
                if (cachedLethalCapacities.NullOrEmpty())
                {
                    cachedLethalCapacities.Add(PawnCapacityDefOf.BloodPumping);
                    cachedLethalCapacities.Add(PawnCapacityDefOf.BloodFiltration);
                    cachedLethalCapacities.Add(PawnCapacityDefOf.Breathing);
                    cachedLethalCapacities.Add(PawnCapacityDefOf.Consciousness);
                    cachedLethalCapacities.Add(PawnCapacityDefOf.Metabolism);
                }
                return cachedLethalCapacities;
            }
        }

        public static List<HediffDef> ApplyHediffs(Pawn pawn, HediffDef hediff = null, List<HediffDef> hediffs = null)
        {
            List<HediffDef> addedHediffs = new List<HediffDef>();
            if (hediff != null)
            {
                Hediff checkedHediff = pawn.health.hediffSet?.GetFirstHediffOfDef(hediff);
                if (checkedHediff == null)
                {
                    addedHediffs.Add(hediff);
                    Hediff newHediff = HediffMaker.MakeHediff(hediff, pawn);
                    newHediff.Severity = 0.5f;
                    pawn.health.AddHediff(newHediff);
                }
            }
            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in hediffs)
                {
                    Hediff checkedHediff = pawn.health.hediffSet?.GetFirstHediffOfDef(hediffDef);
                    if (checkedHediff == null)
                    {
                        addedHediffs.Add(hediffDef);
                        Hediff newHediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        newHediff.Severity = 0.5f;
                        pawn.health.AddHediff(newHediff);
                    }
                }
            }
            return addedHediffs;
        }

        public static void RemoveHediffs(Pawn pawn, HediffDef hediff = null, List<HediffDef> hediffs = null)
        {

            if (hediff != null)
            {
                Hediff hediffToRemove = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
                if (hediffToRemove != null) pawn.health.RemoveHediff(hediffToRemove);
            }

            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in hediffs)
                {
                    Hediff hediffToRemove = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                    if (hediffToRemove != null) pawn.health.RemoveHediff(hediffToRemove);
                }
            }
        }

        public static bool EqualCountingDictionaries(Dictionary<string, int> dictionary1, Dictionary<string, int> dictionary2)
        {
            foreach (string phrase in dictionary1.Keys)
            {
                if (!dictionary2.ContainsKey(phrase)) return false;
                if (dictionary1[phrase] != dictionary2[phrase]) return false;
                dictionary2.Remove(phrase);
            }
            if (!dictionary2.NullOrEmpty()) return false;
            return true;
        }

        public static void AddHediffToPart(Pawn pawn, BodyPartRecord bodyPart, HediffDef hediffDef, float initialSeverity = 1, float severityAdded = 0)
        {
            Hediff firstHediffOfDef = null;
            if (HasHediff(pawn, hediffDef))
            {
                Hediff testHediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (testHediff.Part == bodyPart) firstHediffOfDef = testHediff;
                else
                {
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs) // Go through all the hediffs to try to find the hediff on the specified part
                    {
                        if (hediff.Part == bodyPart && hediff.def == hediffDef) firstHediffOfDef = hediff;
                        break;
                    }
                }
            }

            if (firstHediffOfDef != null)
            {
                try
                {
                    try // Try to make it a psylink
                    {
                        Hediff_Psylink hediff_Level = (Hediff_Psylink)firstHediffOfDef;
                        hediff_Level.ChangeLevel((int)Math.Ceiling(severityAdded), false);
                    }
                    catch // Try to make it a leveling hediff
                    {
                        Hediff_Level hediff_Level = (Hediff_Level)firstHediffOfDef;
                        hediff_Level.ChangeLevel((int)Math.Ceiling(severityAdded));
                    }
                }
                catch // Just increase the severity
                {
                    firstHediffOfDef.Severity += severityAdded;
                }
            }
            else
            {
                firstHediffOfDef = pawn.health.AddHediff(hediffDef, bodyPart);
                firstHediffOfDef.Severity = initialSeverity;
            }
        }

        public static void AddOrAppendHediffs(Pawn pawn, float initialSeverity = 1, float severityPerTick = 0, HediffDef hediff = null, List<HediffDef> hediffs = null)
        {
            if (hediff != null)
            {
                if (HasHediff(pawn, hediff))
                {
                    pawn.health.hediffSet.GetFirstHediffOfDef(hediff).Severity += severityPerTick;
                }
                else
                {
                    Hediff newHediff = HediffMaker.MakeHediff(hediff, pawn);
                    newHediff.Severity = initialSeverity;
                    pawn.health.AddHediff(newHediff);
                }
            }
            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in hediffs)
                {
                    if (HasHediff(pawn, hediffDef))
                    {
                        pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef).Severity += severityPerTick;
                    }
                    else
                    {
                        Hediff newHediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        newHediff.Severity = initialSeverity;
                        pawn.health.AddHediff(newHediff);
                    }
                }
            }
        }

        public static void FadingHediffs(Pawn pawn, float severityPerTick = 0, HediffDef hediff = null, List<HediffDef> hediffs = null)
        {
            if (hediff != null)
            {
                if (HasHediff(pawn, hediff))
                {
                    pawn.health.hediffSet.GetFirstHediffOfDef(hediff).Severity -= severityPerTick;
                }
            }
            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in hediffs)
                {
                    if (HasHediff(pawn, hediffDef))
                    {
                        pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef).Severity -= severityPerTick;
                    }
                }
            }
        }

        public static bool HasHediff(Pawn pawn, HediffDef hediff) // Only made this to make checking for null hediffSets require less work
        {
            if (pawn.health.hediffSet == null) return false;
            if (pawn.health.hediffSet.HasHediff(hediff)) return true;
            return false;
        }
    }
}
