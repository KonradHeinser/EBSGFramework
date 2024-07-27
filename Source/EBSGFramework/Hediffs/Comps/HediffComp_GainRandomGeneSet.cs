using System.Linq;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_GainRandomGeneSet : HediffComp
    {
        public HediffCompProperties_GainRandomGeneSet Props => (HediffCompProperties_GainRandomGeneSet)props;
        public int delayTicks;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (Props.delayTicks < 10) // To avoid potential issues of having multiple things happen to a pawn at spawn, wait several ticks
            {
                delayTicks = 10;
            }
            else
            {
                delayTicks = Props.delayTicks;
            }

            // Some semi-arbitrary additions to try to increase randomness. The tick increase is very slight, but 
            if (Pawn.gender == Gender.Male) delayTicks += 1;
            else if (Pawn.gender == Gender.Female) delayTicks += 2;
            if (Pawn.genes != null && Pawn.genes.GenesListForReading.Count() < 30) delayTicks += Pawn.genes.GenesListForReading.Count();
            if (Pawn.equipment != null && !Pawn.equipment.AllEquipmentListForReading.NullOrEmpty()) delayTicks += Pawn.equipment.AllEquipmentListForReading.Count();
            if (Pawn.health != null && !Pawn.health.hediffSet.hediffs.NullOrEmpty()) delayTicks += Pawn.health.hediffSet.hediffs.Count();
            if (Pawn.ageTracker.AgeBiologicalYears < 60) delayTicks += Pawn.ageTracker.AgeBiologicalYears;
            if (!Pawn.relations.RelatedPawns.EnumerableNullOrEmpty()) delayTicks += Pawn.relations.RelatedPawns.Count();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (delayTicks < 0) return;
            if (delayTicks == 0)
            {
                delayTicks--;
                EBSGUtilities.GainRandomGeneSet(parent.pawn, Props.inheritable, Props.removeGenesFromOtherLists, Props.geneSets, Props.alwaysAddedGenes, Props.alwaysRemovedGenes, Props.showMessage);
                if (parent.pawn.health.hediffSet.GetFirstHediffOfDef(parent.def) != null && Props.removeHediffAfterwards)
                {
                    parent.pawn.health.RemoveHediff(parent.pawn.health.hediffSet.GetFirstHediffOfDef(parent.def));
                }
            }
            else if (parent.Severity >= Props.minSeverity && parent.Severity <= Props.maxSeverity)
            {
                delayTicks--;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref delayTicks, "EBSG_GeneSetDelayTicks", 10);
        }
    }
}
