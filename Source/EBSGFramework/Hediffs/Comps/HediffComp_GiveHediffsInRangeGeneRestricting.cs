﻿using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_GiveHediffsInRangeGeneRestricting : HediffComp
    {
        private Mote mote;

        public HediffCompProperties_GiveHediffsInRangeGeneRestricting Props => (HediffCompProperties_GiveHediffsInRangeGeneRestricting)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!parent.pawn.Awake() || parent.pawn.health == null || parent.pawn.health.InPainShock || !parent.pawn.Spawned)
            {
                return;
            }
            if (!Props.hideMoteWhenNotDrafted || parent.pawn.Drafted)
            {
                if (Props.mote != null && (mote == null || mote.Destroyed))
                {
                    mote = MoteMaker.MakeAttachedOverlay(parent.pawn, Props.mote, Vector3.zero);
                }
                if (mote != null)
                {
                    mote.Maintain();
                }
            }
            List<Pawn> list = null;
            list = ((!Props.onlyPawnsInSameFaction || parent.pawn.Faction == null) ? (List<Pawn>)parent.pawn.Map.mapPawns.AllPawnsSpawned : parent.pawn.Map.mapPawns.SpawnedPawnsInFaction(parent.pawn.Faction));
            foreach (Pawn item in list)
            {
                if (!item.RaceProps.Humanlike || item.Dead || item.health == null || item == parent.pawn || !(item.Position.DistanceTo(parent.pawn.Position) <= Props.range) || !Props.targetingParameters.CanTarget(item) || (!Props.forbiddenGenes.NullOrEmpty() && EBSGUtilities.PawnHasAnyOfGenes(item, Props.forbiddenGenes)))
                {
                    continue;
                }
                Hediff hediff = item.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                if (hediff == null)
                {
                    hediff = item.health.AddHediff(Props.hediff, item.health.hediffSet.GetBrain());
                    hediff.Severity = Props.initialSeverity;
                    HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
                    if (hediffComp_Link != null)
                    {
                        hediffComp_Link.drawConnection = true;
                        hediffComp_Link.other = parent.pawn;
                    }
                }
                HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                if (hediffComp_Disappears == null)
                {
                    Log.Error("HediffComp_GiveHediffsInRangeGeneRestricting has a hediff in props which does not have a HediffComp_Disappears");
                }
                else
                {
                    hediffComp_Disappears.ticksToDisappear = 5;
                }
            }
        }
    }
}
