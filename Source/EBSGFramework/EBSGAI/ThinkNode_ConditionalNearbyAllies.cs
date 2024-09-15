using Verse;
using Verse.AI;
using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalNearbyAllies : ThinkNode_Conditional
    {
        private float radius = 5; // Defaults to checking for any nearby melee enemy
        private int minCount = 1;

        protected override bool Satisfied(Pawn pawn)
        {
            // If the pawn isn't spawned or doesn't have a faction, then they technically don't have allies
            if (!pawn.Spawned || pawn.Faction == null) return false;
            Map map = pawn.Map;
            List<Pawn> list = pawn.Map.mapPawns.AllHumanlikeSpawned;
            list.SortBy((Pawn c) => c.Position.DistanceToSquared(pawn.Position));
            int count = 0;
            foreach (Pawn p in list)
            {
                if (p.Position.DistanceTo(pawn.Position) > radius) break;
                if (p.Downed || p.Dead || p.Faction == null || p.Faction == pawn.Faction || p == pawn) continue;
                CompCanBeDormant comp = p.GetComp<CompCanBeDormant>();
                if (comp != null && !comp.Awake) continue;
                count++;
                if (count >= minCount) return true;
            }
            return false;
        }
    }
}
