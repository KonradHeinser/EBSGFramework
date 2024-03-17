using RimWorld.Planet;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompAbilityEffect_Launch : CompAbilityEffect
    {
        public new CompProperties_Launch Props => (CompProperties_Launch)props;

        public override void Apply(GlobalTargetInfo target)
        {
            Map map = parent.pawn.Map;
            IntVec3 position = parent.pawn.Position;

            Thing transporter = ThingMaker.MakeThing(ThingDefOf.TransportPod);
            GenSpawn.Spawn(transporter, position, map);

            CompTransporter transportComp = transporter.TryGetComp<CompTransporter>();
            transportComp.groupID = parent.pawn.thingIDNumber;
            transportComp.TryRemoveLord(map);

            transportComp.GetDirectlyHeldThings().TryAddOrTransfer(parent.pawn.SplitOff(1));
            ThingOwner directlyHeldThings = transportComp.GetDirectlyHeldThings();

            ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
            activeDropPod.Contents = new ActiveDropPodInfo();
            activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);

            FlyShipLeaving obj = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(Props.skyfallerLeaving ?? ThingDefOf.DropPodLeaving, activeDropPod);
            obj.groupID = transportComp.groupID;
            obj.destinationTile = target.Tile;
            obj.arrivalAction = new TransportPodsArrivalAction_FormCaravan("MessagePawnArrived");
            obj.worldObjectDef = Props.worldObject ?? WorldObjectDefOf.TravelingTransportPods;

            transportComp.CleanUpLoadingVars(map);
            transportComp.parent.Destroy();

            GenSpawn.Spawn(obj, position, map);
        }

        public override bool CanApplyOn(GlobalTargetInfo target)
        {
            return Valid(target, true);
        }

        public override bool Valid(GlobalTargetInfo target, bool throwMessages = false)
        {
            if (!parent.pawn.Spawned || parent.pawn.Map == null) return false;

            if (Find.WorldGrid.TraversalDistanceBetween(parent.pawn.Map.Tile, target.Tile) > Props.maxDistance) return false;
            if (Find.World.Impassable(target.Tile)) return false;

            return base.Valid(target, throwMessages);
        }
    }
}
