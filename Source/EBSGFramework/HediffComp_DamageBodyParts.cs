using System;
using RimWorld;
using Verse;
using System.Collections.Generic;

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
                        Pawn.TakeDamage(new DamageInfo(EBSGDefOf.EBSG_GeneticDeformity, 99999f, 999f, -1f, null, bodyPart));
                    }
                }
                if (!Props.bodyPartsToDamage.NullOrEmpty())
                {
                    foreach (PartToDamage partToDamage in Props.bodyPartsToDamage)
                    {
                        BodyPartRecord bodyPart = null;
                        foreach (BodyPartRecord notMissingPart in Pawn.health.hediffSet.GetNotMissingParts())
                        {
                            if (notMissingPart.def == partToDamage.bodyPart)
                            {
                                bodyPart = notMissingPart;
                                break;
                            }
                        }
                        if (bodyPart == null) continue; // If no part is found, just "continue" down the list
                        Pawn.TakeDamage(new DamageInfo(EBSGDefOf.EBSG_GeneticDeformity, partToDamage.damageAmount, 999f, -1f, null, bodyPart));
                    }
                }
                Pawn.health.RemoveHediff(parent);
            }
        }
    }
}
