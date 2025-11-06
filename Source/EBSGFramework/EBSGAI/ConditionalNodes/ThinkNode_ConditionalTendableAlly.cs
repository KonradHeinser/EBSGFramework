using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalTendableAlly : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            List<Pawn> allies = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
            foreach (Pawn ally in allies) // Then look for any tendable hediff
                if (ally.health.hediffSet.HasTendableHediff()) return true;
            
            return false;
        }
    }
}
