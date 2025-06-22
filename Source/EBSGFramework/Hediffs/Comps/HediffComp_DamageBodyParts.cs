using Verse;
using System.Collections.Generic;
using System.Reflection;

namespace EBSGFramework
{
    public class HediffComp_DamageBodyParts : HediffComp
    {
        private HediffCompProperties_DamageBodyParts Props => (HediffCompProperties_DamageBodyParts)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn.IsHashIntervalTick(60)) // Wait a few ticks before removing just to be safe
            {
                if (!Props.bodyPartsToRemove.NullOrEmpty())
                {
                    foreach (BodyPartDef bodyPartDef in Props.bodyPartsToRemove)
                    {
                        BodyPartRecord bodyPart = null;
                        foreach (BodyPartRecord notMissingPart in Pawn.health.hediffSet.GetNotMissingParts())
                        {
                            if (notMissingPart.def == bodyPartDef)
                            {
                                bodyPart = notMissingPart;
                                break;
                            }
                        }
                        if (bodyPart == null) continue; // If no part is found, just "continue" down the list
                        Pawn.TakeDamage(new DamageInfo(EBSGDefOf.EBSG_GeneticDamage, 99999f, 999f, -1f, null, bodyPart, spawnFilth: false, preventCascade: true));

                        var missing = Pawn.health.hediffSet.GetMissingPartFor(bodyPart);
                        if (missing?.TendableNow() == true)
                            missing.Tended(1, 1);
                    }
                }
                if (!Props.bodyPartsToDamage.NullOrEmpty())
                {
                    List<BodyPartRecord> alreadyTargettedParts = new List<BodyPartRecord>();
                    foreach (PartToDamage partToDamage in Props.bodyPartsToDamage)
                    {
                        BodyPartRecord bodyPart = null;
                        foreach (BodyPartRecord notMissingPart in Pawn.health.hediffSet.GetNotMissingParts())
                        {
                            if (notMissingPart.def == partToDamage.bodyPart && (alreadyTargettedParts.NullOrEmpty() || !alreadyTargettedParts.Contains(notMissingPart)))
                            {
                                alreadyTargettedParts.Add(notMissingPart);
                                bodyPart = notMissingPart;
                                break;
                            }
                        }
                        if (bodyPart == null) continue; // If no part is found, just "continue" down the list
                        if (partToDamage.damagePercentage > 0) Pawn.TakeDamage(new DamageInfo(EBSGDefOf.EBSG_GeneticDamage, bodyPart.def.hitPoints * partToDamage.damagePercentage * Pawn.HealthScale, 999f, -1f, null, bodyPart));
                        else Pawn.TakeDamage(new DamageInfo(EBSGDefOf.EBSG_GeneticDamage, partToDamage.damageAmount, 999f, -1f, null, bodyPart));

                        var missing = Pawn.health.hediffSet.GetMissingPartFor(bodyPart);
                        if (missing?.TendableNow() == true)
                            missing.Tended(1, 1);
                    }
                }
                Pawn.health.RemoveHediff(parent);
            }
        }
    }
}
