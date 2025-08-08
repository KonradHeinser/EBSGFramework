using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class IngestionOutcomeDoer_AddOrAppendHediff : IngestionOutcomeDoer
    {
        public HediffDef hediffDef;

        public float initialSeverity = 0f;

        public float severityChange = 0f;

        public ChemicalDef toleranceChemical;

        public bool divideByBodySize;

        public bool multiplyByGeneToleranceFactors;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            float initial = initialSeverity >= 0 ? initialSeverity : hediffDef.initialSeverity;
            float change = severityChange;
            AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref initial, multiplyByGeneToleranceFactors, divideByBodySize);
            AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref change, multiplyByGeneToleranceFactors, divideByBodySize);
            pawn.AddOrAppendHediffs(initial + initial != 0 ? change * (ingestedCount - 1) : 0, change * ingestedCount, hediffDef);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            if (!parentDef.IsDrug || !(chance >= 1f))
                yield break;

            foreach (StatDrawEntry item in hediffDef.SpecialDisplayStats(StatRequest.ForEmpty()))
                yield return item;
        }
    }
}
