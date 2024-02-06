using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
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
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (delayTicks < 0) return;
            if (delayTicks == 0)
            {
                delayTicks--;
                EBSGUtilities.GainRandomGeneSet(parent.pawn, Props.inheritable, Props.removeGenesFromOtherLists, Props.geneSets, Props.alwaysAddedGenes, Props.alwaysRemovedGenes);
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
