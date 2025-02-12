using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_CreateLinkedHediff : CompProperties_AbilityEffect
    {
        public HediffDef hediffOnCaster;

        public bool casterHediffOnBrain = false;

        public HediffDef hediffOnTarget;

        public bool targetHediffOnBrain = false;

        public SuccessChance successChance;

        public float baseSuccessChance = 1f;

        public StatDef casterStatChance;

        public bool casterStatDivides = false;

        public StatDef targetStatChance;

        public bool targetStatMultiplies = false;

        public string successMessage = null;

        public string failureMessage = null;

        public CompProperties_CreateLinkedHediff()
        {
            compClass = typeof(CompAbilityEffect_CreateLinkedHediff);
        }
    }
}
