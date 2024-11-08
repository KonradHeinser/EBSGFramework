using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;

namespace EBSGFramework
{
    public class PlaceWorker_WallTombs_InsideWallBlock : Placeworker_AttachedToWall
    {
        private bool IsBlockingWallObject(Thing thing)
        {
            // Check if the thing is an attachment that should block.
            bool thingHolderAttachment = thing.def.building is BuildingProperties bp && thing as IThingHolder != null && bp.isAttachment;

            // Check if the thing is a blueprint of an attachment that would _become_ a blocker (once completed).
            bool blueprintAttachment = thing.def.entityDefToBuild is ThingDef bpItem && bpItem.building is BuildingProperties bpBProps &&
                                       bpItem.thingClass.GetInterfaces().Contains(typeof(IThingHolder)) && bpBProps.isAttachment;
            return thingHolderAttachment || blueprintAttachment;
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            var prevResult = base.AllowsPlacing(checkingDef, loc, rot, map, thingToIgnore, thing);
            if (!prevResult.Accepted)
                return prevResult;

            var wallLocation = loc + GenAdj.CardinalDirections[rot.AsInt];

            // Get all things adjacent to the wall and facing away from it.
            List<(Rot4 rot, IntVec3 pos)> wallAdjacentItems = new List<(Rot4 rot, IntVec3 pos)>
            {
                (Rot4.North, IntVec3.South),
                (Rot4.South, IntVec3.North),
                (Rot4.East, IntVec3.West),
                (Rot4.West, IntVec3.East)
            };

            // Other containers, or blueprints of containers.
            List<Thing> conflictingThings = wallAdjacentItems
                .SelectMany(x => (x.pos + wallLocation)
                    .GetThingList(map)
                        .Where(item => item.Rotation == x.rot && IsBlockingWallObject(item))).Where(x => x != null).ToList();

            if (!conflictingThings.NullOrEmpty())
                return "EBSG_InTheWay".Translate(conflictingThings.First().LabelCap);

            // Over the wall Freezers and stuff.
            List<Thing> conflictingThings2 = wallLocation.GetThingList(map).Where(x => x.def.building is BuildingProperties bp && bp.isPlaceOverableWall && bp.isAttachment).ToList();

            if (!conflictingThings2.NullOrEmpty())
                return "EBSG_InTheWay".Translate(conflictingThings2.First().LabelCap);

            return true;
        }
    }
}
