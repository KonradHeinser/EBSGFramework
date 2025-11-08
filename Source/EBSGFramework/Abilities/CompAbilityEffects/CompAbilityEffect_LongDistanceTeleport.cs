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

            List<Pawn> list = PawnsToSkip(target).ToList();
            if (parent.pawn.Spawned)
            {
                if (Props.effecterUsed != null)
                    foreach (Pawn item in list)
                        parent.AddEffecterToMaintain(Props.effecterUsed.Spawn(item, item.Map), item.Position, 60);

                Props.soundPlayed?.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
            }

            if (ShouldEnterMap(target, targetMap))
            {
                if (target.WorldObject is Settlement settlement && CanAttackSettlement(settlement, target))
                {
                    if (!settlement.HasMap)
                        LongEventHandler.QueueLongEvent(delegate { SettlementAttack(settlement, list); }, "GeneratingMapForNewEncounter", false, null);
                    else
                        SettlementAttack(settlement, list);
                }
                else
                {
                    if (targetMap == null)
                        LongEventHandler.QueueLongEvent(delegate { GoToMap(target.WorldObject, list); }, "GeneratingMapForNewEncounter", false, null);
                    else
                        GoToMap(target.WorldObject, list);
                }

                return;
            }

            if (target.WorldObject is Caravan caravan2 && caravan2.Faction == parent.pawn.Faction)
            {
                if (caravan != null)
                {
                    caravan.pawns.TryTransferAllToContainer(caravan2.pawns);
                    caravan2.Notify_Merged(new List<Caravan> { caravan });
                    caravan.Destroy();
                }
                else
                {
                    foreach (Pawn item3 in list)
                    {
                        caravan2.AddPawn(item3, true);
                        item3.ExitMap(false, Rot4.Invalid);
                        AbilityUtility.DoClamor(item3.Position, Props.clamorRadius, parent.pawn, Props.clamorType);
                    }
                }

                return;
            }

            if (caravan != null)
            {
                caravan.Tile = target.Tile;
                caravan.pather.StopDead();
                return;
            }

            if (target.Tile.Layer.Def.canFormCaravans)
            {
                CaravanMaker.MakeCaravan(list, parent.pawn.Faction, target.Tile, false);
                foreach (Pawn item4 in list)
                    item4.ExitMap(false, Rot4.Invalid);
            }
            else
                Log.Error($"{parent.pawn.LabelShortCap} used {parent.def.LabelCap}, but the destination shouldn't have been considered valid.");
        }

        private void TeleportPawns(List<Pawn> pawns, Map targetMap, IntVec3 targetCell)
        {
            foreach (Pawn item2 in pawns)
            {
                if (item2.Spawned)
                {
                    item2.teleporting = true;
                    item2.ExitMap(false, Rot4.Invalid);
                    AbilityUtility.DoClamor(item2.Position, Props.clamorRadius, parent.pawn, Props.clamorType);
                    item2.teleporting = false;
                }

                CellFinder.TryFindRandomSpawnCellForPawnNear(targetCell, targetMap, out var result, 4, cell => cell != targetCell && cell.GetRoom(targetMap) == targetCell.GetRoom(targetMap));

                GenSpawn.Spawn(item2, result, targetMap);
                if (item2.drafter != null && item2.IsColonistPlayerControlled && !item2.Downed)
                    item2.drafter.Drafted = true;
                item2.stances.stunner.StunFor(Props.stunTicks.RandomInRange, parent.pawn, false);
                item2.Notify_Teleported();
                CompAbilityEffect_Teleport.SendSkipUsedSignal(item2, parent.pawn);
                if (item2.IsPrisoner)
                    item2.guest.WaitInsteadOfEscapingForDefaultTicks();

                if (Props.maintainedEffect != null)
                    parent.AddEffecterToMaintain(Props.maintainedEffect.Spawn(item2, item2.Map), item2.Position, Props.maintainDuration, targetMap);
                else if (ModsConfig.RoyaltyActive)
                    parent.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(item2, item2.Map), item2.Position, 60, targetMap);

                if (Props.exitSound != null)
                    Props.exitSound.PlayOneShot(new TargetInfo(result, item2.Map));
                else if (ModsConfig.RoyaltyActive)
                    SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(result, item2.Map));

                if ((item2.IsColonist || item2.RaceProps.packAnimal || item2.IsColonyMech) && item2.Map.IsPlayerHome)
                    item2.inventory.UnloadEverything = true;
            }
        }

        private IntVec3 GetCell(Map targetMap, GlobalTargetInfo? target = null)
        {
            if (targetMap == null)
                if (target.HasValue)
                    targetMap = GetOrGenerateMapUtility.GetOrGenerateMap(target.Value.Tile, null);
                else
                    return IntVec3.Invalid;

            if (targetMap != null)
            {
                if (!Props.requireAllyAtDestination && targetMap.IsPlayerHome)
                    return targetMap.FindDestination(true);

                Pawn pawn = AlliedPawnOnMap(targetMap);
                if (pawn != null)
                    return pawn.Position;
                else if (!Props.requireAllyAtDestination)
                    return targetMap.FindDestination();
                else
                    return parent.pawn.Position;
            }

            return IntVec3.Invalid;
        }

        private void GoToMap(WorldObject obj, List<Pawn> list)
        {
            var targetMap = GetOrGenerateMapUtility.GetOrGenerateMap(obj.Tile, null);
            var targetCell = GetCell(targetMap);
            if (!targetCell.IsValid)
                return;
            Caravan caravan = parent.pawn.GetCaravan();
            TeleportPawns(list, targetMap, targetCell);
            caravan?.Destroy();
        }

        private void SettlementAttack(Settlement settlement, List<Pawn> list)
        {
            bool newMap = settlement.HasMap;
            var targetMap = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
            GoToMap(settlement, list);

            TaggedString letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate();
            TaggedString letterText = "LetterCaravanEnteredEnemyBase".Translate(parent.pawn.Label, settlement.Label.ApplyTag(TagType.Settlement, settlement.Faction.GetUniqueLoadID())).CapitalizeFirst();
            SettlementUtility.AffectRelationsOnAttacked(settlement, ref letterText);
            if (newMap)
            {
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(targetMap.mapPawns.AllPawns, ref letterLabel, ref letterText, "LetterRelatedPawnsSettlement".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
            }

            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, list, settlement.Faction);
            Find.GoodwillSituationManager.RecalculateAll(true);
        }

        private Map GetMap(GlobalTargetInfo target)
        {
            if ((target.WorldObject as MapParent)?.Map != null) return (target.WorldObject as MapParent)?.Map;
            List<Settlement> playerSettlements = new List<Settlement>(Find.WorldObjects.Settlements.Where(s => s.Faction == parent.pawn.Faction && s.Tile == target.Tile && s.HasMap && (!Props.requireAllyAtDestination || AlliedPawnOnMap(s.Map) != null)));
            if (!playerSettlements.NullOrEmpty())
                return playerSettlements[0].Map;

            if (Find.WorldObjects.AnyMapParentAt(target.Tile))
            {
                List<MapParent> maps = Find.WorldObjects.MapParents.Where(p => p.Tile == target.Tile && p.HasMap && (!Props.requireAllyAtDestination || AlliedPawnOnMap(p.Map) != null)).ToList();
                if (!maps.NullOrEmpty())
                    return maps[0].Map;
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

        private IEnumerable<Pawn> PawnsToSkip(GlobalTargetInfo? target = null)
        {
            Caravan caravan = parent.pawn.GetCaravan();
            if (caravan != null)
                foreach (Pawn pawn2 in caravan.pawns)
                    yield return pawn2;
            else
                foreach (Thing item in GenRadial.RadialDistinctThingsAround(parent.pawn.Position, parent.pawn.Map, parent.def.EffectRadius, true))
                    if (item is Pawn pawn && BringPawn(pawn, target))
                        yield return pawn;
        }

        private bool BringPawn(Pawn pawn, GlobalTargetInfo? target = null)
        {
            if (pawn.Dead && !Props.bringCorpses)
                return false;

            if ((Props.onlyAllies || pawn.RaceProps.Animal) &&
                (pawn.Faction == null || pawn.Faction != parent.pawn.Faction))
                return false;

            if (pawn.RaceProps.Animal && !BringAnimal(target))
                return false;

            return true;
        }

        private bool BringAnimal(GlobalTargetInfo? target = null)
        {
            // If the current map isn't the player's assume it's going to be deleted and the animal needs to be brought no matter what
            if (parent.pawn.MapHeld?.IsPlayerHome != true)
                return true;

            // Only need to worry about the destination during the final check
            if (!target.HasValue)
                return false;

            Map map = GetMap(target.Value);
            // Make sure that the map that will be generated is less likely to kill off animals (i.e. space)
            if (map == null)
                return target?.Tile.Tile.PrimaryBiome.AllWildAnimals.EnumerableNullOrEmpty() == false;

            // Make sure the map in question either is the player's, or won't off animals randomly
            return map.IsPlayerHome || !map.Biome.AllWildAnimals.EnumerableNullOrEmpty();
        }

        private Pawn AlliedPawnOnMap(Map targetMap)
        {
            return targetMap.mapPawns.AllPawnsSpawned.FirstOrDefault(p => !p.NonHumanlikeOrWildMan() && p.IsColonist && p.HomeFaction == parent.pawn.Faction && !PawnsToSkip().Contains(p));
        }

        private bool ShouldEnterMap(GlobalTargetInfo target, Map targetMap)
        {
            if (target.WorldObject is Caravan)
                return false;

            if (targetMap != null)
            {
                if (AlliedPawnOnMap(targetMap) == null && Props.requireAllyAtDestination)
                    return targetMap == parent.pawn.Map;
                return true;
            }
            else if (!Props.requireAllyAtDestination && target.WorldObject is MapParent mapObj && !(mapObj is Settlement))
                return true;
            else if (target.WorldObject is Settlement settlement && CanAttackSettlement(settlement, target))
                return true;

            return false;
        }

        private bool CanAttackSettlement(Settlement settlement, GlobalTargetInfo target)
        {
            // If the pawn is a prisoner or is targeting a settlement with the same faction as it or its host, it will not attack
            if (settlement.Faction == parent.pawn.Faction || settlement.Faction == parent.pawn.HostFaction
                                                          || parent.pawn.IsPrisoner)
                return false;

            // If we can leave it to caravan stuff, do so because the player might just be there to trade
            if (target.Tile.Layer.Def.canFormCaravans)
                return false;

            if (!settlement.Spawned || !settlement.Attackable)
                return false;

            if (settlement.EnterCooldownBlocksEntering())
                return false;

            return true;
        }

        private bool ShouldJoinCaravan(GlobalTargetInfo target)
        {
            if (target.WorldObject is Caravan caravan)
                return caravan.Faction == parent.pawn.Faction;
            return false;
        }

        public override bool Valid(GlobalTargetInfo target, bool throwMessages = false)
        {
            if (Props.maxDistance > 0)
                if (Find.WorldGrid.TraversalDistanceBetween(parent.pawn.Tile, target.Tile) > Props.maxDistance / target.Tile.Layer.Def.rangeDistanceFactor)
                    return false;

            if (Find.World.Impassable(target.Tile) || (!(target.WorldObject is MapParent) && !target.Tile.Layer.Def.canFormCaravans))
                return false;

            Caravan caravan = parent.pawn.GetCaravan();

            if (caravan != null && caravan.ImmobilizedByMass)
                return false;

            Caravan caravan2 = target.WorldObject as Caravan;
            if (caravan != null && caravan == caravan2)
                return false;

            if (!ShouldEnterMap(target, GetMap(target)) && !ShouldJoinCaravan(target) && !target.Tile.Layer.Def.canFormCaravans)
                return false;

            return base.Valid(target, throwMessages);
        }

        public override bool CanApplyOn(GlobalTargetInfo target)
        {
            return Valid(target, true) && base.CanApplyOn(target);
        }

        public override Window ConfirmationDialog(GlobalTargetInfo target, Action confirmAction)
        {
            Pawn pawn = PawnsToSkip().FirstOrDefault(p => p.IsQuestLodger());
            if (pawn != null)
                return Dialog_MessageBox.CreateConfirmation("FarskipConfirmTeleportingLodger".Translate(pawn.Named("PAWN")), confirmAction);

            if (target.WorldObject is Settlement settlement && CanAttackSettlement(settlement, target)
                                                            && settlement.Faction.AllyOrNeutralTo(parent.pawn.Faction))
                return Dialog_MessageBox.CreateConfirmation("ConfirmAttackFriendlyFaction".Translate(settlement.LabelCap, settlement.Faction.Name), confirmAction);

            return null;
        }

        public override string WorldMapExtraLabel(GlobalTargetInfo target)
        {
            Caravan caravan = parent.pawn.GetCaravan();
            if (caravan != null && caravan.ImmobilizedByMass)
                return "CaravanImmobilizedByMass".Translate();

            // If the player is just targeting their own caravan, no need to say anything
            if (target.WorldObject is Caravan caravan2 && caravan2 == caravan) return null;

            if (Props.maxDistance > 0 && Find.WorldGrid.TraversalDistanceBetween(parent.pawn.Tile, target.Tile) > Props.maxDistance / target.Tile.Layer.Def.rangeDistanceFactor)
                return "AbilityOutOfRange".Translate();

            if (!Valid(target))
                return Props.requireAllyAtDestination ? "AbilityNeedAllyToSkip".Translate() : "AbilityWorldTileImpassable".Translate();

            if (ShouldJoinCaravan(target))
                return "AbilityJoinCaravan".Translate();

            if (target.WorldObject is Settlement settlement && CanAttackSettlement(settlement, target))
                return "AttackSettlement".Translate(settlement.LabelCap);

            Map map = GetMap(target);
            if (ShouldEnterMap(target, map))
            {
                if (map != null)
                {
                    if (map.IsPlayerHome)
                        return "AbilityHomeTeleport".Translate();

                    if (AlliedPawnOnMap(map) != null)
                        return "AbilityGoToAlly".Translate();
                }

                return "AbilityMapTeleport".Translate();
            }

            return "FormCaravanHere".Translate();
        }
    }
}
