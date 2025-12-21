using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_InstantKill : CompProperties_AbilityEffect
    {
        public bool deleteCorpse = true;

        public DamageDef damageDefToReport;

        public SoundDef explosionSound;

        public ThingDef filthReplacement;

        public bool makeFilth = true;

        public IntRange bloodFilthToSpawnRange;

        public bool multiplyBloodByBodySize = true;

        public ThingDef thingToMake;

        public float bodySizeFactor = 1f;

        public int count = 0;

        public ThingDef stuff;

        public SuccessChance successChance;

        public CompProperties_InstantKill()
        {
            compClass = typeof(CompAbilityEffect_InstantKill);
        }
    }
}
