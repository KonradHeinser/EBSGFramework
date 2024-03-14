using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_GiveHediffsToNonAlliesInRange : HediffComp
    {
        private Mote mote;
        public HediffCompProperties_GiveHediffsToNonAlliesInRange Props => (HediffCompProperties_GiveHediffsToNonAlliesInRange)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!parent.pawn.Awake() || parent.pawn.health == null || parent.pawn.health.InPainShock || !parent.pawn.Spawned || (Props.onlyWhileDrafted && !Pawn.Drafted))
            {
                return;
            }

            // Get all a list of all pawns, and a list of all player pawns
            List<Pawn> list = (List<Pawn>)parent.pawn.Map.mapPawns.AllPawnsSpawned;
            List<Pawn> allies = parent.pawn.Map.mapPawns.SpawnedPawnsInFaction(parent.pawn.Faction);
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

            if (!list.NullOrEmpty())
            {
                foreach (Pawn item in list)
                {
                    if (allies.Contains(item) || (item.Faction != null && item.Faction.AllyOrNeutralTo(Pawn.Faction))) continue; // If it's an ally/non-enemy
                    if (item.Dead || item.health == null || (Props.targetingParameters != null && !Props.targetingParameters.CanTarget(item))) continue;
                    if (Props.rangeStat != null)
                    {
                        if (!(item.Position.DistanceTo(parent.pawn.Position) <= parent.pawn.GetStatValue(Props.rangeStat))) continue;
                    }
                    else if (!(item.Position.DistanceTo(parent.pawn.Position) <= Props.range)) continue;
                    if (Props.psychic && item.GetStatValue(StatDefOf.PsychicSensitivity) == 0) continue;
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
                        Log.Error("HediffComp_GiveHediffsToNonAlliesInRange has a hediff in props which does not have a HediffComp_Disappears");
                    }
                    else
                    {
                        hediffComp_Disappears.ticksToDisappear = 5;
                    }
                }
            }
        }
    }
}
