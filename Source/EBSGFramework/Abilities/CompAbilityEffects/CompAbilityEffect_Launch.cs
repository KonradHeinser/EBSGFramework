using RimWorld.Planet;
using Verse.AI.Group;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace EBSGFramework
{
    public class CompAbilityEffect_Launch : CompAbilityEffect
    {
        public new CompProperties_Launch Props => (CompProperties_Launch)props;

        private Thing transporter;

        private IEnumerable<IThingHolder> Pod
        {
            get
            {
                if (transporter == null)
                    transporter = ThingMaker.MakeThing(Props.pawnTransporter ?? EBSGDefOf.EBSG_FlightPod);
                yield return transporter.TryGetComp<CompTransporter>();
            }
        }

        public override void Apply(GlobalTargetInfo target)
        {
            var dropOptions = DropOptions(target.Tile, TryLaunch);
            if (dropOptions.options.Count > 1)
                if (!Find.WorldObjects.ObjectsAt(target.Tile).Any() && target.Tile.LayerDef.canFormCaravans)
                    dropOptions.options.First().action();
                else
                    Find.WindowStack.Add(new Dialog_NodeTree(dropOptions));
            else // Cancel is somehow the only option
                parent.Refund();
        }

        private DiaNode DropOptions(PlanetTile destination, Action<PlanetTile, TransportersArrivalAction> action)
        {
            DiaNode root = new DiaNode("EBSG_SelectArrivalAction".Translate());

            if (TransportersArrivalAction_FormCaravan.CanFormCaravanAt(Pod, destination))
            {
                DiaOption caravan = new DiaOption("FormCaravanHere".Translate())
                {
                    action = delegate { action(destination, new TransportersArrivalAction_FormCaravan("MessagePawnArrived")); },
                    resolveTree = true
                };
                root.options.Add(caravan);
            }                
            
            var objects = Find.WorldObjects.ObjectsAt(destination);

            if (!objects.EnumerableNullOrEmpty())
                foreach (var obj in objects)
                {
                    if (ModsConfig.OdysseyActive && Props.checkJammer && obj.RequiresSignalJammerToReach)
                        continue;
                    if (obj is Settlement settlement && !settlement.HasMap)
                        foreach (FloatMenuOption option in TransportersArrivalAction_AttackSettlement.GetFloatMenuOptions(action, Pod, settlement))
                        {
                            DiaOption settlementOption = new DiaOption(option.Label);
                            if (!settlement.Faction.HostileTo(Faction.OfPlayer))
                            {
                                DiaNode confirmAttack = new DiaNode("ConfirmAttackFriendlyFaction".Translate(settlement.LabelCap, settlement.Faction));
                                DiaOption confirm = new DiaOption("Confirm".Translate())
                                {
                                    action = option.action,
                                    resolveTree = true
                                };
                                confirmAttack.options.Add(confirm);
                                DiaOption abortMission = new DiaOption("GoBack".Translate()) { link = root };
                                confirmAttack.options.Add(abortMission);
                                settlementOption.link = confirmAttack;
                            }
                            else
                            {
                                settlementOption.action = option.action;
                                settlementOption.resolveTree = true;
                            }
                            root.options.Add(settlementOption);
                        }
                    else
                        foreach (FloatMenuOption option in obj.GetTransportersFloatMenuOptions(Pod, action))
                        {
                            DiaOption transporterOption = new DiaOption(option.Label)
                            {
                                action = option.action,
                                resolveTree = true
                            };
                            root.options.Add(transporterOption);
                        }
                }

            DiaOption cancel = new DiaOption("Cancel".Translate())
            {
                action = delegate { parent.Refund(); },
                resolveTree = true
            };
            root.options.Add(cancel);
            
            return root;
        }

        private void TryLaunch(PlanetTile destination, TransportersArrivalAction action)
        {
            if (Pod.EnumerableNullOrEmpty())
            {
                Log.Error("Failed to launch pawn.");
                return;
            }

            Map map = parent.pawn.MapHeld;
            parent.pawn.GetLord()?.RemovePawn(parent.pawn);
            if (map != null)
            {
                IntVec3 position = parent.pawn.PositionHeld;

                GenSpawn.Spawn(transporter, position, map);

                CompTransporter transportComp = transporter.TryGetComp<CompTransporter>();
                transportComp.groupID = parent.pawn.thingIDNumber;
                transportComp.TryRemoveLord(map);
                
                transportComp.GetDirectlyHeldThings().TryAddOrTransfer(parent.pawn.SplitOff(1));
                ThingOwner directlyHeldThings = transportComp.GetDirectlyHeldThings();

                ActiveTransporter activeTransporter = (ActiveTransporter)ThingMaker.MakeThing(Props.pawnTransporter?.dropPodActive ?? EBSGDefOf.EBSG_PawnLanding);
                activeTransporter.Contents = new ActiveTransporterInfo();
                activeTransporter.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
                activeTransporter.Contents.sentTransporterDef = Props.pawnTransporter ?? EBSGDefOf.EBSG_FlightPod;
                activeTransporter.Contents.openDelay = 10;
                
                FlyShipLeaving obj = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(Props.skyfallerLeaving ?? EBSGDefOf.EBSG_PawnLeaving, activeTransporter);
                obj.groupID = transportComp.groupID;
                obj.destinationTile = destination;
                obj.arrivalAction = action;
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
                    transport.destinationTile = destination;
                    transport.arrivalAction = action;
                    if (transport is FlyingPawn flyingPawn)
                        flyingPawn.pawn = parent.pawn;
                    ActiveTransporterInfo podInfo = new ActiveTransporterInfo();
                    podInfo.innerContainer.TryAddRangeOrTransfer(caravan.AllThings);
                    podInfo.innerContainer.TryAddRangeOrTransfer(caravan.pawns);
                    podInfo.openDelay = 10;
                    podInfo.sentTransporterDef = Props.pawnTransporter ?? EBSGDefOf.EBSG_FlightPod;
                    transport.AddTransporter(podInfo, false);
                    Find.WorldObjects.Add(transport);
                    caravan.Destroy();
                }
            }
            transporter = null;
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (parent.pawn.InBed())
            {
                reason = "StatsReport_InBed".Translate();
                return true;
            }
            if (parent.pawn.MapHeld != null && !parent.pawn.PositionHeld.Standable(parent.pawn.MapHeld))
            {
                reason = "";
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

            // If the pawn isn't in a map or a caravan, run away
            if (!parent.pawn.Spawned && caravan == null)
                return false;

            if (parent.pawn.Spawned && parent.pawn.Position.Roofed(parent.pawn.Map))
            {
                if (throwMessages)
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "Roofed".Translate(), parent.pawn, MessageTypeDefOf.RejectInput, false);
                return false;
            }

            if (caravan != null && (Props.noMapTravelWhileImmobilized || Props.noMapTravelWhenTooMuchMass))
            {
                if (Props.noMapTravelWhenTooMuchMass)
                {
                    float maxMass = parent.pawn.StatOrOne(StatDefOf.CarryingCapacity, StatRequirement.Always, 60);
                    foreach (var pawn in caravan.PawnsListForReading.Where(pawn => pawn != parent.pawn)) // Pawns are usually heaviest
                    {
                        maxMass -= pawn.StatOrOne(StatDefOf.Mass);
                        if (maxMass < 0) return false;
                    }
                    foreach (Thing thing in caravan.AllThings) // Check the rest of the stuff
                    {
                        if (thing is Pawn) continue;

                        maxMass -= thing.StatOrOne(StatDefOf.Mass);
                        if (maxMass < 0) return false;
                    }
                }
                else
                    if (caravan.ImmobilizedByMass) return false;
            }
            PlanetTile tile = parent.pawn.Tile;
            PlanetLayer layer = target.Tile.Layer;
            PlanetTile layerTile = layer.GetClosestTile_NewTemp(tile);

            int distance = Props.maxDistance;
            if (Props.distanceFactorStat != null)
                distance = Mathf.FloorToInt(distance * parent.pawn.StatOrOne(Props.distanceFactorStat, StatRequirement.Always, 60));
            distance = Mathf.RoundToInt((float)distance / (float)layer.Def.rangeDistanceFactor);

            GenDraw.DrawWorldRadiusRing(layerTile, distance, CompPilotConsole.GetFuelRadiusMat(layerTile));

            if (Find.WorldGrid.TraversalDistanceBetween(layerTile, target.Tile, canTraverseLayers: true) > distance)
                return false;
            
            return DropOptions(target.Tile, null).options.Count > 1 && base.Valid(target, throwMessages);
        }
    }
}
