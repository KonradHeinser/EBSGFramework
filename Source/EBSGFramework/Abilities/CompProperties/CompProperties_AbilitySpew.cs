using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilitySpew : CompProperties_AbilityEffect
    {
        public DamageDef damage;

        public float range;

        public float angle;

        public ThingDef filthDef;

        public int amount = -1;

        public EffecterDef effecterDef;

        public float fireChance = 0f;

        public CompProperties_AbilitySpew()
        {
            compClass = typeof(CompAbilityEffect_Spew);
        }
    }
}
