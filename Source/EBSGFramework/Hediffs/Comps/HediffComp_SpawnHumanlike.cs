using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_SpawnHumanlike : HediffComp
    {
        HediffCompProperties_SpawnHumanlike Props => (HediffCompProperties_SpawnHumanlike)props;

        public int spawnLeft;

        public int ticksLeft;

        public Pawn mother;

        public Pawn father;

        public List<GeneDef> genes;

        public List<GeneDef> Genes
        {
            get
            {
                if (genes.NullOrEmpty())
                {
                    genes = new List<GeneDef>();
                    if (Props.staticXenotype != null)
                        genes = Props.staticXenotype.genes;
                    else if (Props.xenotypeSource == XenoSource.Father && father != null)
                        genes = PregnancyUtility.GetInheritedGenes(father, null);
                    else if (Props.xenotypeSource == XenoSource.Mother && mother != null)
                        genes = PregnancyUtility.GetInheritedGenes(null, mother);
                    else
                        genes = PregnancyUtility.GetInheritedGenes(father, mother);
                }

                return genes;
            }
        }

        public Faction faction;

        public override void CompPostMake()
        {
            base.CompPostMake();

            spawnLeft = Props.maxTotalSpawn;
            ticksLeft = Props.completionTicks.RandomInRange;
            mother = Pawn;
            if (faction == null) faction = mother?.Faction ?? father?.Faction;

            // When using give multiple hediffs, faction, father, and mother are set by the ability. 
            // The ability user is the "father", the target the "mother", and the faction is the same as the user's
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (Props.linkedHediff != null && !Pawn.HasHediff(Props.linkedHediff)) return;

            if (Props.onInterval && (spawnLeft > 0 || spawnLeft == -1) &&
                Props.validSeverity.ValidValue(parent.Severity) &&
                (Pawn.MapHeld != null || (Props.tickInCaravans && Pawn.GetCaravan() != null)))
            {
                ticksLeft -= delta;

                if (ticksLeft <= 0)
                {
                    if (!SpawnPawns(Props.developmentalStage))
                        return;

                    if (spawnLeft == 0 && Props.removeHediffOnFinalSpawn)
                        Pawn.health.RemoveHediff(parent);

                    if (spawnLeft == 0 && Props.killHostOnFinalSpawn)
                        Pawn.Kill(new DamageInfo(DamageDefOf.Cut, 99999f, 999f, -1f));

                    if (spawnLeft > 0)
                        if (ticksLeft + delta > 0) // Checking if the spawn was delayed
                            ticksLeft += Props.completionTicks.RandomInRange; // Resets timer with the stored time reducing the next iteration
                        else // If it was, make sure the next spawn can't happen immediately after the previous one
                            ticksLeft = Props.completionTicks.RandomInRange;
                }
            }
        }

        public override void Notify_PawnKilled()
        {
            if (spawnLeft != 0 && Props.onDeath && Props.validSeverity.ValidValue(parent.Severity)
                && (Props.linkedHediff == null || Pawn.HasHediff(Props.linkedHediff)))
            {
                AssignLinkedFather();
                SpawnPawns(Props.devStageForRemovalOrDeath, Props.spawnRemainingOnRemovalOrDeath);
            }

            if (spawnLeft == 0 && Props.removeHediffOnFinalSpawn)
                Pawn.health.RemoveHediff(parent);
        }

        public override void CompPostPostRemoved()
        {
            if (!Pawn.Dead && spawnLeft != 0 && (Props.linkedHediff == null || Pawn.HasHediff(Props.linkedHediff)))
            {
                AssignLinkedFather();
                if (Props.onRemoval && Props.validSeverity.ValidValue(parent.Severity))
                    SpawnPawns(Props.devStageForRemovalOrDeath, Props.spawnRemainingOnRemovalOrDeath);

                if (Props.killHostOnRemoval)
                    Pawn.Kill(new DamageInfo(DamageDefOf.Cut, 99999f, 999f, -1f));
            }

            if (Props.miscarriageThought && spawnLeft > 0)
            {
                if (mother?.Faction == faction)
                    mother?.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.motherMiscarriageThought ?? ThoughtDefOf.Miscarried);
                father?.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.fatherMiscarriageThought ?? ThoughtDefOf.PartnerMiscarried);
            }
        }

        public void AssignLinkedFather()
        {
            if (Props.linkedHediff == null) return;
            Hediff hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.linkedHediff);
            if (hediff is HediffWithTarget targetHediff && targetHediff.target is Pawn parent) father = parent;
        }

        public bool SpawnPawns(DevelopmentalStage developmentalStage, bool allRemaining = false)
        {
            Map map = Pawn.MapHeld;
            Caravan caravan = Pawn.GetCaravan();
            if (map == null && (faction != caravan?.Faction || !Props.allowInCaravans)) return false;
            AssignLinkedFather();

            int numberToSpawn = Props.spawnPerCompletion.RandomInRange; 
            IntVec3 initialPos = Pawn.PositionHeld;

            if (spawnLeft != -1)
            {
                if (allRemaining || numberToSpawn > spawnLeft)
                    numberToSpawn = spawnLeft;
                spawnLeft -= numberToSpawn;
            }

            float fixedAge = 0f;

            switch (developmentalStage)
            {
                case DevelopmentalStage.Adult:
                    fixedAge = 18f;
                    break;
                case DevelopmentalStage.Child:
                    fixedAge = 8f;
                    break;
            }
            
            EBSGUtilities.SpawnHumanlikes(numberToSpawn, initialPos, Pawn.MapHeld, Props.developmentalStage, father, mother, faction, Genes, 
                Props.staticPawnKind, Props.staticXenotype, Props.xenotypeSource, Props.filthOnCompletion,  Props.filthPerSpawn, 
                Props.sendLetters, "EBSG_CompSpawnPawnHediffText", Props.letterTextPawnDescription, Props.letterLabelNote,
                Props.bornThought, Props.motherBabyBornThought, Props.fatherBabyBornThought, Props.noGear, null, 
                Props.relations == InitialRelation.Both || Props.relations == InitialRelation.Mother ? Props.motherRelation ?? PawnRelationDefOf.Parent : null,
                Props.relations == InitialRelation.Both || Props.relations == InitialRelation.Father ? Props.fatherRelation ?? PawnRelationDefOf.Parent : null);

            return true;
        }


        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref mother, "mother");
            Scribe_References.Look(ref father, "father");
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref ticksLeft, "ticksLeft", 250);
            Scribe_Values.Look(ref spawnLeft, "spawnLeft", 1);
        }
    }
}
