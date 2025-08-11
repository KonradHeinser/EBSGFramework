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
            if (Pawn.IsHashIntervalTick(12)) // Wait a few ticks before removing just to be safe
            {
                if (!Props.bodyPartsToRemove.NullOrEmpty())
                {
                    var parts = new List<BodyPartDef>(Props.bodyPartsToRemove);
                    parts.Shuffle(); // Picking random parts requires shuffling
                    int count = Props.removeCount;
                    foreach (BodyPartDef bodyPartDef in parts)
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
                        if (bodyPart == null) 
                            continue; // If no part is found, time to leave
                        Pawn.TakeDamage(new DamageInfo(EBSGDefOf.EBSG_GeneticDamage, 99999f, 999f, -1f, null, bodyPart, spawnFilth: false, preventCascade: true));

                        var missing = Pawn.health.hediffSet.GetMissingPartFor(bodyPart);
                        if (missing?.TendableNow() == true)
                            missing.Tended(1, 1);

                        count--;
                        if (count == 0) // Will never happen when the remove count starts at 0
                            break;
                    }
                }
                if (!Props.bodyPartsToDamage.NullOrEmpty())
                {
                    var parts = new List<PartToDamage>(Props.bodyPartsToDamage);
                    parts.Shuffle(); // Picking random parts requires shuffling
                    int count = Props.damageCount;
                    List<BodyPartRecord> alreadyTargettedParts = new List<BodyPartRecord>();
                    foreach (PartToDamage partToDamage in parts)
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
                        if (bodyPart == null) 
                            continue; // If no part is found, time to leave
                        if (partToDamage.damagePercentage > 0) Pawn.TakeDamage(new DamageInfo(EBSGDefOf.EBSG_GeneticDamage, bodyPart.def.hitPoints * partToDamage.damagePercentage * Pawn.HealthScale, 999f, -1f, null, bodyPart));
                        else Pawn.TakeDamage(new DamageInfo(EBSGDefOf.EBSG_GeneticDamage, partToDamage.damageAmount, 999f, -1f, null, bodyPart));

                        var missing = Pawn.health.hediffSet.GetMissingPartFor(bodyPart);
                        if (missing?.TendableNow() == true)
                            missing.Tended(1, 1);

                        count--;
                        if (count == 0) // Will never happen when the remove count starts at 0
                            break; 
                    }
                }
                Pawn.health.RemoveHediff(parent);
            }
        }
    }
}
