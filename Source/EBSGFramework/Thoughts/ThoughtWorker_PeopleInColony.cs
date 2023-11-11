using System;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class ThoughtWorker_PeopleInColony : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
            {
                return ThoughtState.Inactive;
            }
            int freeColonistsAndPrisonersSpawnedCount = p.Map.mapPawns.FreeColonistsAndPrisonersSpawnedCount;
            if (freeColonistsAndPrisonersSpawnedCount <= 1)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            if (freeColonistsAndPrisonersSpawnedCount <= 5)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            if (freeColonistsAndPrisonersSpawnedCount <= 10)
            {
                return ThoughtState.ActiveAtStage(2);
            }
            if (freeColonistsAndPrisonersSpawnedCount <= 15)
            {
                return ThoughtState.ActiveAtStage(3);
            }
            return ThoughtState.ActiveAtStage(4);
        }
    }
}
