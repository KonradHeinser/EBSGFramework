using RimWorld;
using Verse;
using Verse.Sound;

namespace EBSGFramework
{
    public class CompAbilityEffect_InstantKill : CompAbilityEffect
    {
        private static EBSGCache_Component cache;

        public static EBSGCache_Component Cache
        {
            get
            {
                if (cache == null)
                    cache = Current.Game.GetComponent<EBSGCache_Component>();

                if (cache != null && cache.loaded)
                    return cache;
                return null;
            }
        }

        public new CompProperties_InstantKill Props => (CompProperties_InstantKill)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Thing != null && target.Thing is Pawn victim && !victim.Dead)
            {
                float bloodMutliplier = 1;
                if (Props.multiplyBloodByBodySize) bloodMutliplier = victim.BodySize;
                IntVec3 initialPosition = victim.Position;

                int randomInRange = (int)(Props.bloodFilthToSpawnRange.RandomInRange * bloodMutliplier);
                for (int i = 0; i < randomInRange; i++)
                {
                    IntVec3 c = initialPosition;
                    if (randomInRange > 1)
                    {
                        c = c.RandomAdjacentCell8Way();
                    }
                    if (randomInRange > 10)
                    {
                        float radiusChecker = 10;
                        while (randomInRange > radiusChecker)
                        {
                            c = c.RandomAdjacentCell8Way();
                            radiusChecker *= 2f;
                        }
                    }
                    if (c.InBounds(victim.MapHeld))
                    {
                        ThingDef bloodType = victim.RaceProps.BloodDef;

                        if (Props.filthReplacement != null && Props.filthReplacement.thingClass == typeof(Filth))
                        {
                            bloodType = Props.filthReplacement;
                        }
                        else if (EBSGUtilities.PawnHasAnyOfGenes(victim, out var first, Cache?.bloodReplacingGenes))
                        {
                            bloodType = first.GetModExtension<EBSGExtension>().bloodReplacement;
                        }

                        FilthMaker.TryMakeFilth(c, victim.MapHeld, bloodType, victim.LabelShort);
                    }
                }

                if (Props.explosionSound != null) Props.explosionSound.PlayOneShot(new TargetInfo(initialPosition, victim.MapHeld));

                DamageDef damageToReport = Props.damageDefToReport;
                if (damageToReport == null)
                {
                    if (ModsConfig.BiotechActive) damageToReport = DamageDefOf.Vaporize;
                    else damageToReport = DamageDefOf.Burn;
                }
                victim.TakeDamage(new DamageInfo(damageToReport, 99999f, 999f, -1f, parent.pawn, victim.health.hediffSet.GetBrain()));

                if (Props.deleteCorpse && victim != null && victim.Corpse != null) victim.Corpse.Destroy(DestroyMode.KillFinalize);
            }
        }
    }
}
