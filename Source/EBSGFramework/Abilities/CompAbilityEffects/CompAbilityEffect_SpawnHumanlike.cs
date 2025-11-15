using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_SpawnHumanlike : CompAbilityEffect
    {
        public new CompProperties_AbilitySpawnHumanlike Props => (CompProperties_AbilitySpawnHumanlike)props;

        public Pawn Caster => parent.pawn;

        public List<GeneDef> Genes(Pawn other = null)
        {
            var genes = new List<GeneDef>(Props.staticGenes);
            if (Props.staticXenotype != null)
                genes.AddRange(Props.staticXenotype.genes);
            else
                switch (Props.xenotypeSource)
                {
                    case XenoSource.Father when Caster.genes?.GenesListForReading.NullOrEmpty() == false:
                        genes.AddRange(PregnancyUtility.GetInheritedGenes(Caster, null));
                        break;
                    case XenoSource.Mother when other?.genes?.GenesListForReading.NullOrEmpty() == false:
                        genes.AddRange(PregnancyUtility.GetInheritedGenes(null, other));
                        break;
                    case XenoSource.Hybrid:
                        genes.AddRange(PregnancyUtility.GetInheritedGenes(Caster, other));
                        break;
                }
            return genes;
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true) && base.CanApplyOn(target, dest);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (Props.bodySizeFactor > 0 && target.Thing == null)
                return false;

            if (Props.linkedHediff != null && (!Caster.HasHediff(Props.linkedHediff, out var result) || !(result is HediffWithTarget targeter) || !(targeter.target is Pawn)))
            {
                if (throwMessages)
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityNoCasterHediffOne".Translate(Props.linkedHediff.label), Caster, MessageTypeDefOf.RejectInput, false);
                return false;
            }
            
            if (Props.bodySizeFactor > 0)
                switch (target.Thing)
                {
                    case Pawn p:
                        if (Mathf.FloorToInt(p.BodySize * Props.bodySizeFactor) < 1)
                        {
                            if (throwMessages)
                                Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "TargetTooSmall".Translate(), target.Thing, MessageTypeDefOf.RejectInput, false);
                            return false;
                        }
                        break;
                    case Corpse c:
                        if (Mathf.FloorToInt(c.InnerPawn.BodySize * Props.bodySizeFactor) < 1)
                        {
                            if (throwMessages)
                                Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "TargetTooSmall".Translate(), target.Thing, MessageTypeDefOf.RejectInput, false);
                            return false;
                        }
                        break;
                    default:
                        return false;
                }
            
            return base.Valid(target, throwMessages);
        }

        public override bool GizmoDisabled(out string reason)
        {
            reason = null;
            if (Props.linkedHediff != null && (!Caster.HasHediff(Props.linkedHediff, out var result) || !(result is HediffWithTarget targeter) || !(targeter.target is Pawn)))
            {
                reason = "AbilityNoCasterHediffOne".Translate(Props.linkedHediff.label);
                return true;
            }
            return base.GizmoDisabled(out reason);
        }

        private void Refund()
        {
            // Somewhat mitigates the effects of impossible miscasts
            if (parent.UsesCharges)
                parent.RemainingCharges += 1;
            else
                parent.ResetCooldown();
        }
        
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn other = (target.Thing is Corpse corpse) ? corpse.InnerPawn : target.Pawn;
            Hediff link = null;
            if (Props.linkedHediff != null)
            {
                if (Caster.HasHediff(Props.linkedHediff, out link) && link is HediffWithTarget targeter && targeter.target is Pawn t)
                    other = t;
                else
                {
                    Refund();
                    return; // This should be impossible, but weirder things have happened
                }
            }

            var numberToSpawn = Props.spawnCount.RandomInRange;

            if (Props.bodySizeFactor > 0)
                switch (target.Thing)
                {
                    case Pawn p:
                        numberToSpawn = Mathf.FloorToInt(p.BodySize * Props.bodySizeFactor);
                        break;
                    case Corpse c:
                        numberToSpawn = Mathf.FloorToInt(c.InnerPawn.BodySize * Props.bodySizeFactor);
                        break;
                    default:
                        Refund();
                        return;
                }
            
            IntVec3 initialPos;
            switch (Props.destination)
            {
                case AbilityEffectDestination.Caster:
                    initialPos = Caster.PositionHeld;
                    break;
                case AbilityEffectDestination.Selected:
                    initialPos = target.Cell;
                    break;
                default:
                    initialPos = Rand.Bool ? target.Cell : Caster.PositionHeld;
                    break;
            }
                                    
            if (target.Thing != null)
                switch (Props.targetEffect)
                {
                    case DeathType.Kill:
                        target.Thing.Kill();
                        break;
                    case DeathType.Vanish:
                        if (target.Pawn != null)
                            target.Pawn.VaporizePawn();
                        else
                            target.Thing.Destroy();
                        break;
                    case DeathType.None:
                    default:
                        break;
                }

            EBSGUtilities.SpawnHumanlikes(numberToSpawn, initialPos, Caster.MapHeld, Props.developmentalStage, Caster, other, Caster.Faction, 
                Genes(other), Props.staticPawnKind ?? Caster.kindDef, Props.staticXenotype, Props.xenotypeSource, 
                Props.filthOnCompletion, Props.filthPerSpawn, 
                Props.sendLetters, "EBSG_AbilitySpawnHumanlike", Props.letterTextPawnDescription, Props.letterLabelNote, 
                Props.bornThought, Props.motherBabyBornThought, Props.fatherBabyBornThought, Props.noGear, Props.xenotypeLabel,
                Props.relations == InitialRelation.Both || Props.relations == InitialRelation.Mother ? Props.motherRelation ?? PawnRelationDefOf.Parent : null,
                Props.relations == InitialRelation.Both || Props.relations == InitialRelation.Father ? Props.fatherRelation ?? PawnRelationDefOf.Parent : null);
            
            if (Props.removeLink && link != null)
                Caster.health.RemoveHediff(link);
        }
    }
}