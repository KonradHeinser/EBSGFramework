using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EBSGFramework
{
    public class CompAbilityEffect_LongDistanceTeleport : CompAbilityEffect
    {
        public new CompProperties_LongDistanceTeleport Props => (CompProperties_LongDistanceTeleport)props;

        public override void Apply(GlobalTargetInfo target)
        {
            Caravan caravan = parent.pawn.GetCaravan();
            Map targetMap = GetMap(target);

            IntVec3 targetCell = IntVec3.Invalid;
            List<Pawn> list = PawnsToSkip().ToList();
            if (parent.pawn.Spawned)
            {
                if (Props.effecterUsed != null)
                {
                    foreach (Pawn item in list)
                    {
                        parent.AddEffecterToMaintain(Props.effecterUsed.Spawn(item, item.Map), item.Position, 60);
                    }
                }
                else if (ModsConfig.RoyaltyActive)
                {
                    foreach (Pawn item in list)
                    {
                        parent.AddEffecterToMaintain(EffecterDefOf.Skip_Entry.Spawn(item, item.Map), item.Position, 60);
                    }
                }
                if (Props.soundPlayed != null)
                {
                    Props.soundPlayed.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
                }
                else if (ModsConfig.RoyaltyActive)
                {
                    SoundDefOf.Psycast_Skip_Pulse.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
                }

            }
            if (ShouldEnterMap(target, targetMap))
            {
                if (!Props.requireAllyAtDestination && targetMap.IsPlayerHome)
                {
                    targetCell = EBSGUtilities.FindDestination(targetMap, true);
                }
                if (!target.IsValid)
                {
                    Pawn pawn = AlliedPawnOnMap(targetMap);
                    if (pawn != null)
                    {
                        targetCell = pawn.Position;
                    }
                    else if (!Props.requireAllyAtDestination)
                    {
                        targetCell = EBSGUtilities.FindDestination(targetMap);
                    }
                    else
                    {
                        targetCell = parent.pawn.Position;
                    }
                }
            }

            if (targetCell.IsValid)
            {
                foreach (Pawn item2 in list)
                {
                    if (item2.Spawned)
                    {
                        item2.teleporting = true;
                        item2.ExitMap(false, Rot4.Invalid);
                        AbilityUtility.DoClamor(item2.Position, Props.clamorRadius, parent.pawn, Props.clamorType);
                        item2.teleporting = false;
                    }
                    CellFinder.TryFindRandomSpawnCellForPawnNear(targetCell, targetMap, out var result, 4, (IntVec3 cell) => cell != targetCell && cell.GetRoom(targetMap) == targetCell.GetRoom(targetMap));
                    GenSpawn.Spawn(item2, result, targetMap);
                    if (item2.drafter != null && item2.IsColonistPlayerControlled && !item2.Downed)
                    {
                        item2.drafter.Drafted = true;
                    }
                    item2.stances.stunner.StunFor(Props.stunTicks.RandomInRange, parent.pawn, false);
                    item2.Notify_Teleported();
                    CompAbilityEffect_Teleport.SendSkipUsedSignal(item2, parent.pawn);
                    if (item2.IsPrisoner)
                    {
                        item2.guest.WaitInsteadOfEscapingForDefaultTicks();
                    }

                    if (Props.maintainedEffect != null)
                    {
                        parent.AddEffecterToMaintain(Props.maintainedEffect.Spawn(item2, item2.Map), item2.Position, Props.maintainDuration, targetMap);
                    }
                    else if (ModsConfig.RoyaltyActive)
                    {
                        parent.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(item2, item2.Map), item2.Position, 60, targetMap);
                    }

                    if (Props.exitSound != null)
                    {
                        Props.exitSound.PlayOneShot(new TargetInfo(result, item2.Map));
                    }
                    else if (ModsConfig.RoyaltyActive)
                    {
                        SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(result, item2.Map));
                    }

                    if ((item2.IsColonist || item2.RaceProps.packAnimal || item2.IsColonyMech) && item2.Map.IsPlayerHome)
                    {
                        item2.inventory.UnloadEverything = true;
                    }
                }
                caravan?.Destroy();
                return;
            }
            if (target.WorldObject is Caravan caravan2 && caravan2.Faction == parent.pawn.Faction)
            {
                if (caravan != null)
                {
                    caravan.pawns.TryTransferAllToContainer(caravan2.pawns);
                    caravan2.Notify_Merged(new List<Caravan> { caravan });
                    caravan.Destroy();
                    return;
                }
                {
                    foreach (Pawn item3 in list)
                    {
                        caravan2.AddPawn(item3, true);
                        item3.ExitMap(false, Rot4.Invalid);
                        AbilityUtility.DoClamor(item3.Position, Props.clamorRadius, parent.pawn, Props.clamorType);
                    }
                    return;
                }
            }
            if (caravan != null)
            {
                caravan.Tile = target.Tile;
                caravan.pather.StopDead();
                return;
            }

            CaravanMaker.MakeCaravan(list, parent.pawn.Faction, target.Tile, false);
            foreach (Pawn item4 in list)
            {
                item4.ExitMap(false, Rot4.Invalid);
            }
        }

        private Map GetMap(GlobalTargetInfo target)
        {
            if ((target.WorldObject as MapParent)?.Map != null) return (target.WorldObject as MapParent)?.Map;
            List<Settlement> playerSettlments = new List<Settlement>(Find.WorldObjects.Settlements.Where((Settlement s) => s.Faction == parent.pawn.Faction && s.Tile == target.Tile && s.HasMap && (!Props.requireAllyAtDestination || AlliedPawnOnMap(s.Map) != null)));
            if (!playerSettlments.NullOrEmpty())
            {
                return playerSettlments[0].Map;
            }
            if (Find.WorldObjects.AnyMapParentAt(target.Tile))
            {
                List<MapParent> maps = Find.WorldObjects.MapParents.Where((MapParent p) => p.Tile == target.Tile && p.HasMap && (!Props.requireAllyAtDestination || AlliedPawnOnMap(p.Map) != null)).ToList();
                if (!maps.NullOrEmpty())
                {
                    return maps[0].Map;
                }
            }
            return null;
        }

        public override IEnumerable<PreCastAction> GetPreCastActions()
        {
            yield return new PreCastAction
            {
                action = delegate
                {
                    foreach (Pawn item in PawnsToSkip())
                    {
                        FleckCreationData dataAttachedOverlay = FleckMaker.GetDataAttachedOverlay(item, FleckDefOf.PsycastSkipFlashEntry, new Vector3(-0.5f, 0f, -0.5f));
                        dataAttachedOverlay.link.detachAfterTicks = 5;
                        item.Map.flecks.CreateFleck(dataAttachedOverlay);
                    }
                },
                ticksAwayFromCast = 5
            };
        }

        private IEnumerable<Pawn> PawnsToSkip()
        {
            Caravan caravan = parent.pawn.GetCaravan();
            if (caravan != null)
            {
                foreach (Pawn pawn2 in caravan.pawns)
                {
                    yield return pawn2;
                }
                yield break;
            }
            bool homeMap = parent.pawn.Map.IsPlayerHome;
            foreach (Thing item in GenRadial.RadialDistinctThingsAround(parent.pawn.Position, parent.pawn.Map, parent.def.EffectRadius, true))
            {
                if (item is Pawn pawn && (!pawn.Dead || Props.bringCorpses) && (!Props.onlyAllies || pawn.Faction == parent.pawn.Faction || (!homeMap && pawn.RaceProps.Animal && pawn.Faction != null && pawn.Faction == parent.pawn.Faction)))
                {
                    yield return pawn;
                }
            }
        }

        private Pawn AlliedPawnOnMap(Map targetMap)
        {
            return targetMap.mapPawns.AllPawnsSpawned.FirstOrDefault((Pawn p) => !p.NonHumanlikeOrWildMan() && p.IsColonist && p.HomeFaction == parent.pawn.Faction && !PawnsToSkip().Contains(p));
        }

        private bool ShouldEnterMap(GlobalTargetInfo target, Map targetMap)
        {
            if (target.WorldObject is Caravan caravan && caravan.Faction == parent.pawn.Faction)
            {
                return false;
            }
            if (targetMap != null)
            {
                if (AlliedPawnOnMap(targetMap) == null && Props.requireAllyAtDestination)
                {
                    return targetMap == parent.pawn.Map;
                }
                return true;
            }
            return false;
        }


        private bool ShouldJoinCaravan(GlobalTargetInfo target)
        {
            if (target.WorldObject is Caravan caravan)
            {
                return caravan.Faction == parent.pawn.Faction;
            }
            return false;
        }

        public override bool Valid(GlobalTargetInfo target, bool throwMessages = false)
        {
            if (Props.maxDistance > 0)
            {
                if (Find.WorldGrid.TraversalDistanceBetween(parent.pawn.Tile, target.Tile, true) > Props.maxDistance) return false;
            }
            if (Find.World.Impassable(target.Tile)) return false;

            Caravan caravan = parent.pawn.GetCaravan();

            if (caravan != null && caravan.ImmobilizedByMass) return false;

            Caravan caravan2 = target.WorldObject as Caravan;
            if (caravan != null && caravan == caravan2) return false;

            Map map = GetMap(target);
            if (Props.requireAllyAtDestination && !ShouldEnterMap(target, map) && !ShouldJoinCaravan(target)) return false;

            return base.Valid(target, throwMessages);
        }

        public override bool CanApplyOn(GlobalTargetInfo target)
        {
            if (Props.requireAllyAtDestination && target.WorldObject is MapParent mapParent && mapParent != null && AlliedPawnOnMap(mapParent.Map) == null)
            {
                return false;
            }
            return base.CanApplyOn(target);
        }

        public override Window ConfirmationDialog(GlobalTargetInfo target, Action confirmAction)
        {
            Pawn pawn = PawnsToSkip().FirstOrDefault((Pawn p) => p.IsQuestLodger());
            if (pawn != null)
            {
                return Dialog_MessageBox.CreateConfirmation("FarskipConfirmTeleportingLodger".Translate(pawn.Named("PAWN")), confirmAction);
            }
            return null;
        }

        public override string WorldMapExtraLabel(GlobalTargetInfo target)
        {
            Caravan caravan = parent.pawn.GetCaravan();
            if (caravan != null && caravan.ImmobilizedByMass) return "CaravanImmobilizedByMass".Translate();

            Caravan caravan2 = target.WorldObject as Caravan; // If the player is just targetting their own caravan, no need to say anything
            if (caravan2 != null && caravan2 == caravan) return null;

            if (Props.maxDistance > 0)
            {
                if (Find.WorldGrid.TraversalDistanceBetween(parent.pawn.Tile, target.Tile, true) > Props.maxDistance) return "AbilityTooFarToTeleport".Translate();
            }

            if (!Valid(target) && Props.requireAllyAtDestination) return "AbilityNeedAllyToSkip".Translate();

            if (!Valid(target)) return "AbilityPassableOnly".Translate();

            if (ShouldJoinCaravan(target)) return "AbilitySkipToJoinCaravan".Translate();

            Map map = GetMap(target);
            if (ShouldEnterMap(target, map))
            {
                if (map.IsPlayerHome) return "AbilityHomeTeleport".Translate();

                if (AlliedPawnOnMap(map) != null) return "AbilitySkipToRandomAlly".Translate();

                return "AbilityMapTeleport".Translate();
            }

            return "AbilityTileTeleport".Translate();
        }
    }
}
