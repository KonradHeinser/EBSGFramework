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
            if (map != null)
            {
                IntVec3 position = parent.pawn.Position;

                Thing transporter = ThingMaker.MakeThing(ThingDefOf.TransportPod);
                GenSpawn.Spawn(transporter, position, map);

                CompTransporter transportComp = transporter.TryGetComp<CompTransporter>();
                transportComp.groupID = Find.UniqueIDsManager.GetNextTransporterGroupID();
                transportComp.TryRemoveLord(map);

                transportComp.GetDirectlyHeldThings().TryAddOrTransfer(parent.pawn.SplitOff(1));
                ThingOwner directlyHeldThings = transportComp.GetDirectlyHeldThings();

                ActiveTransporter activeDropPod = (ActiveTransporter)ThingMaker.MakeThing(Props.skyfallerArriving ?? ThingDefOf.ActiveDropPod);
                activeDropPod.Contents = new ActiveTransporterInfo();
                activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
                if (activeDropPod is FlyPawnArriving pawnArriving)
                    pawnArriving.pawn = parent.pawn;

                FlyShipLeaving obj = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(Props.skyfallerLeaving ?? EBSGDefOf.EBSG_PawnLeaving, activeDropPod);
                obj.groupID = transportComp.groupID;
                obj.destinationTile = target.Tile;
                
                obj.arrivalAction = new TransportersArrivalAction_FormCaravan("MessagePawnArrived");
                obj.worldObjectDef = Props.worldObject ?? EBSGDefOf.EBSG_PawnFlying;
                if (obj is FlyPawnLeaving pawnLeaving)
                    pawnLeaving.pawn = parent.pawn;

                transportComp.CleanUpLoadingVars(map);
                transportComp.parent.Destroy();

                GenSpawn.Spawn(obj, position, map);
            }
            else
            {
                Caravan caravan = parent.pawn.GetCaravan();
                if (caravan == null) return;
                
                WorldObject newCaravan = WorldObjectMaker.MakeWorldObject(Props.worldObject ?? EBSGDefOf.EBSG_PawnFlying);
                if (newCaravan is TravellingTransporters transport)
                {
                    transport.Tile = caravan.Tile;
                    transport.destinationTile = target.Tile;
                    transport.arrivalAction = new TransportersArrivalAction_FormCaravan("MessagePawnArrived");
                    if (transport is FlyingPawn flyingPawn)
                        flyingPawn.pawn = parent.pawn;
                    ActiveTransporterInfo podInfo = new ActiveTransporterInfo();
                    podInfo.innerContainer.TryAddRangeOrTransfer(caravan.AllThings);
                    podInfo.innerContainer.TryAddRangeOrTransfer(caravan.pawns);
                    transport.AddTransporter(podInfo, false);
                    Find.WorldObjects.Add(transport);
                    caravan.Destroy();
                }
            }
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (parent.pawn.InBed())
            {
                reason = "StatsReport_InBed".Translate();
                return true;
            }
            return base.GizmoDisabled(out reason);
        }

        public override bool CanApplyOn(GlobalTargetInfo target)
        {
            return Valid(target, true) && base.CanApplyOn(target);
        }

        public override bool Valid(GlobalTargetInfo target, bool throwMessages = false)
        {
            Caravan caravan = parent.pawn.GetCaravan();

            // If the pawn isn't in a map or a caravan, get the fuck out of here
            if (parent.pawn.Map == null && caravan == null) return false;
            if (parent.pawn.Map != null && !parent.pawn.Spawned) return false;

            if (caravan != null && (Props.noMapTravelWhileImobilized || Props.noMapTravelWhenTooMuchMass))
            {
                if (Props.noMapTravelWhenTooMuchMass)
                {
                    float maxMass = parent.pawn.GetStatValue(StatDefOf.CarryingCapacity);
                    foreach (Pawn pawn in caravan.PawnsListForReading)
                    {
                        if (pawn == parent.pawn) continue;

                        maxMass -= pawn.GetStatValue(StatDefOf.Mass);
                        if (maxMass < 0) return false;
                    }
                    foreach (Thing thing in caravan.AllThings)
                    {
                        if (thing is Pawn pawn) continue;

                        maxMass -= thing.GetStatValue(StatDefOf.Mass);
                        if (maxMass < 0) return false;
                    }
                }
                else
                {
                    if (caravan.ImmobilizedByMass) return false;
                }
            }

            int tile = parent.pawn.Tile;

            GenDraw.DrawWorldRadiusRing(tile, Props.maxDistance);

            if (Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile) > Props.maxDistance) return false;
            if (Find.World.Impassable(target.Tile)) return false;

            return base.Valid(target, throwMessages);
        }
    }
}
