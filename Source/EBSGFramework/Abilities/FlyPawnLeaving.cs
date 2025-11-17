using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace EBSGFramework
{
    public class FlyPawnLeaving : FlyShipLeaving
    {
        public Pawn pawn;
        public override Graphic Graphic => pawn?.Graphic ?? base.Graphic;

        private bool alreadyLeft;

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (pawn?.Drawer?.renderer != null)
            {
                if (def.skyfaller.xPositionCurve != null)
                    drawLoc.x += def.skyfaller.xPositionCurve.Evaluate(TimeInAnimation);
                if (def.skyfaller.zPositionCurve != null) 
                    drawLoc.z += def.skyfaller.zPositionCurve.Evaluate(TimeInAnimation);
                pawn.Drawer.renderer.RenderPawnAt(drawLoc, pawn.Rotation);
                DrawDropSpotShadow();
            }
            else
                base.DrawAt(drawLoc, flip);
        }

        protected override void LeaveMap()
        {
            if (alreadyLeft || !createWorldObject)
            {
                if (Contents != null)
                {
                    foreach (Thing item in Contents.innerContainer)
                    {
                        if (item is Pawn p)
                        {
                            p.ExitMap(false, Rot4.Invalid);
                        }
                    }
                    Contents.innerContainer.ClearAndDestroyContentsOrPassToWorld(DestroyMode.QuestLogic);
                }
                base.LeaveMap();
                return;
            }
            if (groupID < 0)
            {
                Log.Error("Drop pod left the map, but its group ID is " + groupID);
                Destroy();
                return;
            }
            if (destinationTile < 0)
            {
                Log.Error("Drop pod left the map, but its destination tile is " + destinationTile);
                Destroy();
                return;
            }
            Lord lord = TransporterUtility.FindLord(groupID, Map);
            if (lord != null)
            {
                Map.lordManager.RemoveLord(lord);
            }
            var travelingTransportPods = (TravellingTransporters)WorldObjectMaker.MakeWorldObject(worldObjectDef ?? WorldObjectDefOf.TravellingTransporters);
            travelingTransportPods.Tile = Map.Tile;
            travelingTransportPods.SetFaction(pawn.Faction);
            travelingTransportPods.destinationTile = destinationTile;
            travelingTransportPods.arrivalAction = arrivalAction;

            if (travelingTransportPods is FlyingPawn flyingPawn)
                flyingPawn.pawn = pawn;

            Find.WorldObjects.Add(travelingTransportPods);
            alreadyLeft = true;
            travelingTransportPods.AddTransporter(Contents, true);
            Contents = null;
            Destroy();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref pawn, "pawn");
        } 
    }
}
