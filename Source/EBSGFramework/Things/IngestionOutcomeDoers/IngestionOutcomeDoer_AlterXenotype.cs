using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using static HarmonyLib.Code;

namespace EBSGFramework
{
    public class IngestionOutcomeDoer_AlterXenotype : IngestionOutcomeDoer
    {
        public List<RandomXenotype> xenotypes;

        public FloatRange severities = new FloatRange(0f, 999f);

        public ThingDef filth;

        public IntRange filthCount = new IntRange(4, 7);

        public bool setXenotype = true; // Clears old xeno genes and replaces them with the xenotype's

        public bool sendMessage = true;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            pawn.AlterXenotype(xenotypes, filth, filthCount, setXenotype, sendMessage);
        }
    }
}
