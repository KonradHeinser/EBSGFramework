using System.Collections.Generic;
using System.Linq;
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
            if (!Pawn.Awake() || Pawn.health == null || Pawn.health.InPainShock || !Pawn.Spawned || (Props.onlyWhileDrafted && !Pawn.Drafted && Pawn.IsPlayerControlled))
                return;

            if (Props.interval != null)
            {
                if (!Pawn.IsHashIntervalTick(Props.interval.Value)) // Allow for periodic intervals instead
                    return;
            }
            else if (!Pawn.IsHashIntervalTick(5)) // Avoid spamming
                return;
            
            // Get all a list of all pawns, and a list of all allied pawns
            var list = new List<Pawn>(parent.pawn.Map.mapPawns.AllPawnsSpawned);
            list.SortBy(c => c.Position.DistanceToSquared(Pawn.Position));
            var allies = Pawn.Map.mapPawns.SpawnedPawnsInFaction(Pawn.Faction);

            if (!Props.hideMoteWhenNotDrafted || Pawn.Drafted)
            {
                if (Props.mote != null && (mote == null || mote.Destroyed))
                    mote = MoteMaker.MakeAttachedOverlay(parent.pawn, Props.mote, Vector3.zero);
                mote?.Maintain();
            }

            if (!list.NullOrEmpty())
            {
                float range = Props.rangeStat != null ? Pawn.StatOrOne(Props.rangeStat, StatRequirement.Always, 60) : Props.range;
                foreach (var item in list.Where(p => p.health != null && !p.Dead &&
                                                     !allies.Contains(p) && (p.Faction == null || !p.Faction.AllyOrNeutralTo(Pawn.Faction))))
                {
                    if (Props.targetingParameters != null && !Props.targetingParameters.CanTarget(item)) continue;
                    if (Props.psychic && item.StatOrOne(StatDefOf.PsychicSensitivity, StatRequirement.Always, 60) == 0) continue;
                    if (item.Position.DistanceTo(Pawn.Position) > range) break;
                    if (Props.interval != null && Props.successChance?.Success(parent.pawn, item) == false)
                        continue;
                    
                    var hediff = item.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                    if (hediff == null)
                    {
                        hediff = item.CreateComplexHediff(Props.initialSeverity, Props.hediff, Pawn,
                            item.health.hediffSet.GetBrain());
                        item.health.AddHediff(hediff);
                    }
                    var hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                    if (hediffComp_Disappears == null)
                        Log.Error("HediffComp_GiveHediffsToNonAlliesInRange has a hediff in props which does not have a HediffComp_Disappears");
                    else
                        hediffComp_Disappears.ticksToDisappear = Props.duration;
                }
            }
        }
    }
}
