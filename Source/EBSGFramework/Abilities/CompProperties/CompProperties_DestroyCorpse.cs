using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class CompProperties_AbilityDestroyCorpse : CompProperties_AbilityEffect
    {
        public DamageDef damageDefToReport;

        public SoundDef explosionSound;

        public ThingDef filthReplacement;

        public bool makeFilth = true;

        public IntRange bloodFilthToSpawnRange = IntRange.Zero;

        public bool multiplyBloodByBodySize = true;

        public bool multiplyBloodByRemainingBlood = false;

        public bool useHemogenGainFactor = true;

        public ThingDef thingToMake;

        public float bodySizeFactor = 1f;

        public int count = 0;

        public ThingDef stuff;

        public float hemogenGain = 0f;

        public bool multiplyHemogenByBodySize = true;

        public bool multiplyHemogenByRemainingBlood = true;

        public bool mustBeHemogenic = false;

        public float nutritionGain = 0f;

        public bool multiplyNutritionByBodySize = true;

        public bool multiplyNutritionByRemainingBlood = false;

        public GeneDef resourceMainGene = null;

        public bool mustHaveResourceGene = false;

        public float resourceGain = 0f;

        public bool multiplyResourceByBodySize = true;

        public bool multiplyResourceByRemainingBlood = false;

        public bool useResourceGainFactor = true;

        public CompProperties_AbilityDestroyCorpse()
        {
            compClass = typeof(CompAbilityEffect_DestroyCorpse);
        }
    }
}
