using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace EBSGFramework
{
    public class CompAbilityEffect_InstantKill : CompAbilityEffect
    {
        public new CompProperties_InstantKill Props => (CompProperties_InstantKill)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Thing != null && target.Thing is Pawn victim)
            {
                float bloodMutliplier = 1;
                if (Props.multiplyBloodByBodySize) bloodMutliplier = victim.BodySize;
                IntVec3 initialPosition = victim.Position;

                DamageDef damageToReport = Props.damageDefToReport;
                if (damageToReport == null)
                {
                    if (ModsConfig.BiotechActive) damageToReport = DamageDefOf.Vaporize;
                    else damageToReport = DamageDefOf.Burn;
                }

                victim.TakeDamage(new DamageInfo(damageToReport, 99999f, 999f, -1f, parent.pawn, victim.health.hediffSet.GetBrain()));

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
                            radiusChecker *= 1.5f;
                        }
                    }
                    if (c.InBounds(victim.MapHeld))
                    {
                        ThingDef bloodType = victim.RaceProps.BloodDef;

                        if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core"))
                        {
                            VFECompatabilityUtilities.BloodType(victim);
                        }

                        FilthMaker.TryMakeFilth(c, victim.MapHeld, bloodType, victim.LabelShort);
                    }
                }

                if (Props.explosionSound != null) Props.explosionSound.PlayOneShot(new TargetInfo(initialPosition, victim.MapHeld));

                if (Props.deleteCorpse && !victim.Corpse.Destroyed) victim.Corpse.Destroy();
            }
        }
    }
}
