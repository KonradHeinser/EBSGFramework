using System;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_DRGLaunch : CompAbilityEffect
    {
        public new CompProperties_DRGLaunch Props => (CompProperties_DRGLaunch)props;

        public int MaxDistance
        {
            get
            {
                if (Props.mainResourceGene != null && parent.pawn.genes.GetGene(Props.mainResourceGene) is ResourceGene resourceGene)
                {
                    int maxDistance = (int)Math.Floor((resourceGene.Value - Props.baseCost) / Props.costPerTile);

                    if (maxDistance > Props.maxDistance) maxDistance = Props.maxDistance;

                    return maxDistance;
                }

                return 0;
            }
        }

        public override void Apply(GlobalTargetInfo target)
        {
            if (parent.pawn.genes.GetGene(Props.mainResourceGene) is ResourceGene resourceGene)
                ResourceGene.OffsetResource(parent.pawn, 0 - Props.baseCost - Props.costPerTile * Find.WorldGrid.TraversalDistanceBetween(parent.pawn.Map.Tile, target.Tile), resourceGene);
            else
            {
                Log.Error(parent.def + " attempted to launch, but the resource gene listed isn't a resource gene.");
                return;
            }

            Map map = parent.pawn.Map;
            IntVec3 position = parent.pawn.Position;

            Thing transporter = ThingMaker.MakeThing(ThingDefOf.TransportPod);
            GenSpawn.Spawn(transporter, position, map);

            CompTransporter transportComp = transporter.TryGetComp<CompTransporter>();
            transportComp.groupID = parent.pawn.thingIDNumber;
            transportComp.TryRemoveLord(map);

            transportComp.GetDirectlyHeldThings().TryAddOrTransfer(parent.pawn.SplitOff(1));
            ThingOwner directlyHeldThings = transportComp.GetDirectlyHeldThings();

            ActiveTransporter activeDropPod = (ActiveTransporter)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
            activeDropPod.Contents = new ActiveTransporterInfo();
            activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);

            FlyShipLeaving obj = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(Props.skyfallerLeaving ?? ThingDefOf.DropPodLeaving, activeDropPod);
            obj.groupID = transportComp.groupID;
            obj.destinationTile = target.Tile;
            obj.arrivalAction = new TransportersArrivalAction_FormCaravan("MessagePawnArrived");
            obj.worldObjectDef = Props.worldObject ?? WorldObjectDefOf.TravellingTransporters;

            transportComp.CleanUpLoadingVars(map);
            transportComp.parent.Destroy();

            GenSpawn.Spawn(obj, position, map);
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (!parent.pawn.HasRelatedGene(Props.mainResourceGene))
            {
                reason = "AbilityDisabledNoResourceGene".Translate(parent.pawn, Props.mainResourceGene.LabelCap);
                return true;
            }

            if (!(parent.pawn.genes.GetGene(Props.mainResourceGene) is ResourceGene gene_Resource))
            {
                reason = "AbilityDisabledNoResourceGene".Translate(parent.pawn, Props.mainResourceGene.LabelCap);
                return true;
            }

            float cost = Props.baseCost + Props.costPerTile;

            if (gene_Resource.Value < cost)
            {
                reason = "AbilityDisabledNoResource".Translate(parent.pawn, gene_Resource.ResourceLabel);
                return true;
            }

            if (cost != 0 && cost > gene_Resource.Value)
            {
                reason = "AbilityDisabledNoResource".Translate(parent.pawn, gene_Resource.ResourceLabel);
                return true;
            }

            reason = null;
            return false;
        }

        public override bool CanApplyOn(GlobalTargetInfo target)
        {
            return Valid(target, true);
        }

        public override bool Valid(GlobalTargetInfo target, bool throwMessages = false)
        {
            if (!parent.pawn.Spawned || parent.pawn.Map == null ||
                Props.mainResourceGene == null || !parent.pawn.HasRelatedGene(Props.mainResourceGene)) return false;

            GenDraw.DrawWorldRadiusRing(parent.pawn.Map.Tile, MaxDistance);

            if (Find.WorldGrid.TraversalDistanceBetween(parent.pawn.Map.Tile, target.Tile) > MaxDistance) return false;
            if (Find.World.Impassable(target.Tile)) return false;

            return base.Valid(target, throwMessages);
        }
    }
}
