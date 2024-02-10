using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_InstantKill : CompProperties_AbilityEffect
    {
        public bool deleteCorpse = true;

        public DamageDef damageDefToReport;

        public SoundDef explosionSound;

        public IntRange bloodFilthToSpawnRange;

        public bool multiplyBloodByBodySize = true;

        public CompProperties_InstantKill()
        {
            compClass = typeof(CompAbilityEffect_InstantKill);
        }
    }
}
