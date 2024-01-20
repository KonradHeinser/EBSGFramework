using System.Collections.Generic;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class IngestionOutcomeDoer_GiveHediffBasedOnHediffSeverity : IngestionOutcomeDoer
    {
        public HediffDef hediffDef;

        public HediffDef checkedHediffDef;

        public bool getSeveritySum = false;

        public bool useHighest = false;

        public bool justFirst = false;

        public List<HediffDef> checkedHediffList;

        public SimpleCurve severityCurve;

        public ChemicalDef toleranceChemical;

        private bool divideByBodySize;

        public bool multiplyByGeneToleranceFactors;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            float effect = 0;
            if (checkedHediffDef != null)
            {
                if (EBSGUtilities.HasHediff(pawn, checkedHediffDef))
                {
                    effect = severityCurve.Evaluate(pawn.health.hediffSet.GetFirstHediffOfDef(checkedHediffDef).Severity);
                }
                else effect = severityCurve.Evaluate(0);
            }
            else if (!checkedHediffList.NullOrEmpty())
            {
                if (getSeveritySum)
                {
                    float severitySum = 0;
                    foreach (HediffDef hediff in checkedHediffList)
                    {
                        if (EBSGUtilities.HasHediff(pawn, hediff))
                        {
                            severitySum += pawn.health.hediffSet.GetFirstHediffOfDef(hediff).Severity;
                        }
                    }
                    effect = severityCurve.Evaluate(severitySum);
                }
                else if (useHighest)
                {
                    float severityToUse = 0;
                    foreach (HediffDef hediff in checkedHediffList)
                    {
                        if (EBSGUtilities.HasHediff(pawn, hediff))
                        {
                            if (pawn.health.hediffSet.GetFirstHediffOfDef(hediff).Severity > severityToUse) severityToUse = pawn.health.hediffSet.GetFirstHediffOfDef(hediff).Severity;
                        }
                    }
                    effect = severityCurve.Evaluate(severityToUse);
                }
                else if (justFirst)
                {
                    foreach (HediffDef hediff in checkedHediffList)
                    {
                        if (EBSGUtilities.HasHediff(pawn, hediff))
                        {
                            effect = severityCurve.Evaluate(pawn.health.hediffSet.GetFirstHediffOfDef(hediff).Severity);
                            break;
                        }
                    }
                }
                else
                {
                    foreach (HediffDef hediff in checkedHediffList)
                    {
                        if (EBSGUtilities.HasHediff(pawn, hediff))
                        {
                            effect += severityCurve.Evaluate(pawn.health.hediffSet.GetFirstHediffOfDef(hediff).Severity);
                        }
                        else effect += severityCurve.Evaluate(0);
                    }
                }
            }
            else
            {
                if (EBSGUtilities.HasHediff(pawn, hediffDef))
                {
                    effect = severityCurve.Evaluate(pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef).Severity);
                }
                else effect = severityCurve.Evaluate(0);
            }

            if (divideByBodySize) effect /= pawn.BodySize;
            AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize_NewTemp(pawn, toleranceChemical, ref effect, multiplyByGeneToleranceFactors);

            EBSGUtilities.AddOrAppendHediffs(pawn, effect, effect, hediffDef);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            if (!parentDef.IsDrug || !(chance >= 1f))
            {
                yield break;
            }
            foreach (StatDrawEntry item in hediffDef.SpecialDisplayStats(StatRequest.ForEmpty()))
            {
                yield return item;
            }
        }
    }
}
