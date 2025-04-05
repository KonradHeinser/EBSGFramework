using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EBSGFramework
{
    public class JobGiver_GatherRitualMaterials : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Lord l = pawn.GetLord();
            if (l == null) return null;
            // Need to set up lord toil before continuing

            // Make sure you got the right lord toil

            // Make sure this is a pawn that's allowed to transport materials

            // Get the ClosestThingReachable for a material that hasn't been gathered yet 

            // Make a TakeCountToInventory job with a count equal to the minimum of stack count and needed for the material
            return null;
        }
    }
}
