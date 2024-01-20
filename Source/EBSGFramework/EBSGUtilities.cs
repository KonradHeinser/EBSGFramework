using Verse;
using Verse.AI.Group;
using System.Collections.Generic;
using System;
using RimWorld.Planet;
using System.Linq;
using RimWorld;

namespace EBSGFramework
{
    public class EBSGUtilities
    {
        public static List<PawnCapacityDef> cachedLethalCapacities = new List<PawnCapacityDef>();

        public static List<PawnCapacityDef> LethalCapacities
        {
            get
            {
                if (cachedLethalCapacities.NullOrEmpty())
                {
                    cachedLethalCapacities.Add(PawnCapacityDefOf.BloodPumping);
                    cachedLethalCapacities.Add(PawnCapacityDefOf.BloodFiltration);
                    cachedLethalCapacities.Add(PawnCapacityDefOf.Breathing);
                    cachedLethalCapacities.Add(PawnCapacityDefOf.Consciousness);
                    cachedLethalCapacities.Add(PawnCapacityDefOf.Metabolism);
                }
                return cachedLethalCapacities;
            }
        }

        public static Hediff GetFirstHediffAttachedToPart(Pawn pawn, HediffDef hediffDef, BodyPartRecord bodyPartRecord = null, BodyPartDef bodyPartDef = null)
        {
            if (hediffDef == null) return null;
            Hediff hediff = null;

            if (HasHediff(pawn, hediffDef))
            {
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (bodyPartRecord != null)
                {
                    if (hediff.Part != bodyPartRecord)
                    {
                        hediff = null;
                        foreach (Hediff hediffOnPawn in pawn.health.hediffSet.hediffs)
                        {
                            if (hediffOnPawn.def == hediffDef && hediffOnPawn.Part == bodyPartRecord)
                            {
                                hediff = hediffOnPawn;
                                break;
                            }
                        }
                    }
                }
                else if (bodyPartDef != null)
                {
                    if (hediff.Part.def != bodyPartDef)
                    {
                        hediff = null;
                        foreach (Hediff hediffOnPawn in pawn.health.hediffSet.hediffs)
                        {
                            if (hediffOnPawn.def == hediffDef && hediffOnPawn.Part.def == bodyPartDef)
                            {
                                hediff = hediffOnPawn;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // If there's no set body part, just get the newest version of that hediff def as that's most likely to be the correct one
                    foreach (Hediff hediffOnPawn in pawn.health.hediffSet.hediffs)
                    {
                        if (hediffOnPawn.def == hediffDef && hediffOnPawn.ageTicks < hediff.ageTicks) hediff = hediffOnPawn;
                    }
                }
            }
            return hediff;
        }

        public static void RemoveDamage(Pawn pawn, HediffDef hediffDef, BodyPartRecord bodyPart, float damageRemoved)
        {
            while (damageRemoved > 0)
            {
                Hediff hediff = GetFirstHediffAttachedToPart(pawn, hediffDef, bodyPart);
                //Log.Message("Removing damage of " + hediff.Label + " from " + bodyPart.Label);
                if (hediff != null)
                {
                    float removalAmount = (hediff.Severity > damageRemoved) ? damageRemoved : hediff.Severity;
                    damageRemoved -= removalAmount;
                    hediff.Severity -= removalAmount;

                }
                else break;
            }
        }

        public static List<HediffDef> ApplyHediffs(Pawn pawn, HediffDef hediff = null, List<HediffDef> hediffs = null)
        {
            List<HediffDef> addedHediffs = new List<HediffDef>();
            if (hediff != null)
            {
                Hediff checkedHediff = pawn.health.hediffSet?.GetFirstHediffOfDef(hediff);
                if (checkedHediff == null)
                {
                    addedHediffs.Add(hediff);
                    Hediff newHediff = HediffMaker.MakeHediff(hediff, pawn);
                    newHediff.Severity = 0.5f;
                    pawn.health.AddHediff(newHediff);
                }
            }
            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in hediffs)
                {
                    Hediff checkedHediff = pawn.health.hediffSet?.GetFirstHediffOfDef(hediffDef);
                    if (checkedHediff == null)
                    {
                        addedHediffs.Add(hediffDef);
                        Hediff newHediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        newHediff.Severity = 0.5f;
                        pawn.health.AddHediff(newHediff);
                    }
                }
            }
            return addedHediffs;
        }

        public static void RemoveHediffs(Pawn pawn, HediffDef hediff = null, List<HediffDef> hediffs = null)
        {
            if (hediff != null)
            {
                Hediff hediffToRemove = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
                if (hediffToRemove != null) pawn.health.RemoveHediff(hediffToRemove);
            }

            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in hediffs)
                {
                    Hediff hediffToRemove = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                    if (hediffToRemove != null) pawn.health.RemoveHediff(hediffToRemove);
                }
            }
        }

        public static void RemoveHediffsFromParts(Pawn pawn, List<HediffsToParts> hediffs = null, HediffsToParts hediffToParts = null)
        {
            if (hediffToParts != null && HasHediff(pawn, hediffToParts.hediff))
            {
                if (hediffToParts.bodyParts.NullOrEmpty()) RemoveHediffs(pawn, hediffToParts.hediff);
                else
                {
                    foreach (BodyPartDef bodyPart in hediffToParts.bodyParts)
                    {
                        Hediff firstHediffOfDef = null;
                        Hediff testHediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffToParts.hediff);

                        if (testHediff.Part.def == bodyPart) firstHediffOfDef = testHediff;
                        else
                        {
                            foreach (Hediff hediff in pawn.health.hediffSet.hediffs) // Go through all the hediffs to try to find the hediff on the specified part
                            {
                                if (hediff.Part.def == bodyPart && hediff.def == hediffToParts.hediff) firstHediffOfDef = hediff;
                                break;
                            }
                        }
                        if (firstHediffOfDef != null) pawn.health.RemoveHediff(firstHediffOfDef);
                    }
                }
            }
            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffsToParts hediffPart in hediffs)
                {
                    if (!HasHediff(pawn, hediffPart.hediff)) continue;
                    if (hediffPart.bodyParts.NullOrEmpty()) RemoveHediffs(pawn, hediffPart.hediff);
                    else
                    {
                        foreach (BodyPartDef bodyPart in hediffPart.bodyParts)
                        {
                            Hediff firstHediffOfDef = null;
                            Hediff testHediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffPart.hediff);

                            if (testHediff.Part.def == bodyPart) firstHediffOfDef = testHediff;
                            else
                            {
                                foreach (Hediff hediff in pawn.health.hediffSet.hediffs) // Go through all the hediffs to try to find the hediff on the specified part
                                {
                                    if (hediff.Part.def == bodyPart && hediff.def == hediffPart.hediff) firstHediffOfDef = hediff;
                                    break;
                                }
                            }
                            if (firstHediffOfDef != null) pawn.health.RemoveHediff(firstHediffOfDef);
                        }
                    }
                }
            }
        }

        public static bool EqualCountingDictionaries(Dictionary<string, int> dictionary1, Dictionary<string, int> dictionary2)
        {
            foreach (string phrase in dictionary1.Keys)
            {
                if (!dictionary2.ContainsKey(phrase)) return false;
                if (dictionary1[phrase] != dictionary2[phrase]) return false;
                dictionary2.Remove(phrase);
            }
            if (!dictionary2.NullOrEmpty()) return false;
            return true;
        }

        public static Hediff AddHediffToPart(Pawn pawn, BodyPartRecord bodyPart, HediffDef hediffDef, float initialSeverity = 1, float severityAdded = 0)
        {
            Hediff firstHediffOfDef = null;
            if (HasHediff(pawn, hediffDef))
            {
                Hediff testHediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (testHediff.Part == bodyPart) firstHediffOfDef = testHediff;
                else
                {
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs) // Go through all the hediffs to try to find the hediff on the specified part
                    {
                        if (hediff.Part == bodyPart && hediff.def == hediffDef) firstHediffOfDef = hediff;
                        break;
                    }
                }
            }

            if (firstHediffOfDef != null)
            {
                try
                {
                    try // Try to make it a psylink
                    {
                        Hediff_Psylink hediff_Level = (Hediff_Psylink)firstHediffOfDef;
                        hediff_Level.ChangeLevel((int)Math.Ceiling(severityAdded), false);
                    }
                    catch // Try to make it a leveling hediff
                    {
                        Hediff_Level hediff_Level = (Hediff_Level)firstHediffOfDef;
                        hediff_Level.ChangeLevel((int)Math.Ceiling(severityAdded));
                    }
                }
                catch // Just increase the severity
                {
                    firstHediffOfDef.Severity += severityAdded;
                }
            }
            else
            {
                firstHediffOfDef = pawn.health.AddHediff(hediffDef, bodyPart);
                firstHediffOfDef.Severity = initialSeverity;
            }

            return firstHediffOfDef;
        }

        public static void AddOrAppendHediffs(Pawn pawn, float initialSeverity = 1, float severityIncrease = 0, HediffDef hediff = null, List<HediffDef> hediffs = null)
        {
            if (hediff != null)
            {
                if (HasHediff(pawn, hediff))
                {
                    pawn.health.hediffSet.GetFirstHediffOfDef(hediff).Severity += severityIncrease;
                }
                else
                {
                    Hediff newHediff = HediffMaker.MakeHediff(hediff, pawn);
                    newHediff.Severity = initialSeverity;
                    pawn.health.AddHediff(newHediff);
                }
            }
            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in hediffs)
                {
                    if (HasHediff(pawn, hediffDef))
                    {
                        pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef).Severity += severityIncrease;
                    }
                    else
                    {
                        Hediff newHediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        newHediff.Severity = initialSeverity;
                        pawn.health.AddHediff(newHediff);
                    }
                }
            }
        }

        public static void FadingHediffs(Pawn pawn, float severityPerTick = 0, HediffDef hediff = null, List<HediffDef> hediffs = null)
        {
            if (hediff != null)
            {
                if (HasHediff(pawn, hediff))
                {
                    pawn.health.hediffSet.GetFirstHediffOfDef(hediff).Severity -= severityPerTick;
                }
            }
            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in hediffs)
                {
                    if (HasHediff(pawn, hediffDef))
                    {
                        pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef).Severity -= severityPerTick;
                    }
                }
            }
        }

        public static IntVec3 FindDestination(Map targetMap, bool targetCenter = false)
        {
            IntVec3 target = IntVec3.Invalid;
            if (targetCenter)
            {
                target = targetMap.Center;
                if (target.Standable(targetMap))
                {
                    return target;
                }
                target = CellFinder.StandableCellNear(target, targetMap, 50);
                if (target.IsValid) return target;
            }
            target = CellFinder.RandomEdgeCell(targetMap);
            if (target.Standable(targetMap)) return target;
            target = CellFinder.StandableCellNear(target, targetMap, 30);
            if (target.IsValid) return target;

            target = CellFinder.RandomEdgeCell(targetMap);
            target = CellFinder.StandableCellNear(target, targetMap, 30); // If the first time fails try a second time just to see if the first one was bad luck
            if (target.IsValid) return target;

            return IntVec3.Invalid;
        }

        public static bool HasHediff(Pawn pawn, HediffDef hediff) // Only made this to make checking for null hediffSets require less work
        {
            if (pawn.health.hediffSet == null) return false;
            if (pawn.health.hediffSet.HasHediff(hediff)) return true;
            return false;
        }

        public static void HandleNeedOffsets(Pawn pawn, List<NeedOffset> needOffsets, bool preventRepeats = true, int hashInterval = 200, bool hourlyRate = false, bool dailyRate = false)
        {
            if (needOffsets.NullOrEmpty()) return;
            List<Need> alreadyPickedNeeds = new List<Need>();
            foreach (NeedOffset needOffset in needOffsets)
            {
                Need need;
                if (needOffset.need == null && preventRepeats)
                {
                    if (preventRepeats) need = pawn.needs.AllNeeds.Where((Need n) => !alreadyPickedNeeds.Contains(n)).RandomElement();
                    else need = pawn.needs.AllNeeds.RandomElement();
                }
                else need = pawn.needs.TryGetNeed(needOffset.need);

                if (need != null)
                {
                    alreadyPickedNeeds.Add(need);
                    float offset = needOffset.offset;
                    if (needOffset.offsetFactorStat != null) offset *= pawn.GetStatValue(needOffset.offsetFactorStat);
                    if (hourlyRate) offset *= hashInterval / 2500f;
                    else if (dailyRate) offset *= hashInterval / 60000f;
                    need.CurLevel += offset;
                }
            }
        }

        public static bool BadWeather(Map map)
        {
            // Couldn't use the vanilla enjoyable outside, so I just checked the favorability
            return map.weatherManager.curWeather.favorability == Favorability.Bad || map.weatherManager.curWeather.favorability == Favorability.VeryBad;
        }

        public static bool PawnHasAnyOfGenes(List<GeneDef> genesDefs, List<Gene> genes, Pawn pawn)
        {
            if (pawn.genes == null) return false;

            if (!genesDefs.NullOrEmpty())
            {
                foreach (Gene gene in pawn.genes.GenesListForReading)
                {
                    if (genesDefs.Contains(gene.def)) return true;
                }
            }
            if (!genes.NullOrEmpty())
            {
                foreach (Gene gene in pawn.genes.GenesListForReading)
                {
                    if (genes.Contains(gene)) return true;
                }
            }

            return false;
        }

        public static bool NeedFrozen(Pawn pawn, NeedDef def)
        {
            if (pawn.Suspended)
            {
                return true;
            }
            if (def.freezeWhileSleeping && !pawn.Awake())
            {
                return true;
            }
            if (def.freezeInMentalState && pawn.InMentalState)
            {
                return true;
            }
            if (!pawn.SpawnedOrAnyParentSpawned && !pawn.IsCaravanMember() && !PawnUtility.IsTravelingInTransportPodWorldObject(pawn))
            {
                return true;
            }
            return false;
        }

        public static bool CheckNearbyWater(Pawn pawn, int maxNeededForTrue, out int waterCount, float maxDistance = 0)
        {
            waterCount = 0;
            if (!pawn.Spawned || pawn.Map == null) return false; // If either of these situations are true, we really need to get out of here

            if (maxDistance <= 0) // If max distance is just the pawn's tile, only need to check the pawn's tile
            {
                if (pawn.Position.GetTerrain(pawn.Map).IsWater) waterCount++;
                if (maxNeededForTrue <= waterCount) return true;
                return false;
            }

            List<IntVec3> waterTiles = pawn.Map.AllCells.Where((IntVec3 p) => p.DistanceTo(pawn.Position) <= maxDistance && p.GetTerrain(pawn.Map).IsWater).ToList();
            waterCount = waterTiles.Count;
            return maxNeededForTrue <= waterCount;
        }

        public static void RemoveChronicHediffs(Pawn pawn)
        {
            if (pawn.health.hediffSet != null && !pawn.health.hediffSet.hediffs.NullOrEmpty())
            {
                List<Hediff> hediffsToRemove = new List<Hediff>();
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff.def.chronic) hediffsToRemove.Add(hediff);
                }
                if (!hediffsToRemove.NullOrEmpty())
                {
                    foreach (Hediff hediff in hediffsToRemove) pawn.health.RemoveHediff(hediff);
                }
            }
        }

        public static bool HasRelatedGene(Pawn pawn, GeneDef relatedGene)
        {
            if (!ModsConfig.BiotechActive || pawn.genes == null) return false;
            return pawn.genes.HasGene(relatedGene);
        }

        public static bool HasAnyOfRelatedGene(Pawn pawn, List<GeneDef> relatedGenes)
        {
            if (!ModsConfig.BiotechActive || pawn.genes == null) return false;

            foreach (GeneDef gene in relatedGenes)
            {
                if (pawn.genes.HasGene(gene)) return true;
            }
            return false;
        }

        // Resurrect utility with bug fix

        public static void TryToRevivePawn(Pawn pawn)
        {
            if (!pawn.Dead || pawn.Discarded) return; // If these events pass, just silently fail

            Corpse corpse = pawn.Corpse;
            bool flag = false;
            IntVec3 loc = IntVec3.Invalid;
            Map map = null;
            bool flag2 = Find.Selector.IsSelected(corpse);
            if (corpse != null)
            {
                flag = corpse.Spawned;
                loc = corpse.Position;
                map = corpse.Map;
                corpse.InnerPawn = null;
                if (!corpse.Destroyed) corpse.Destroy();
            }
            if (flag && pawn.IsWorldPawn())
            {
                Find.WorldPawns.RemovePawn(pawn);
            }
            pawn.ForceSetStateToUnspawned();
            PawnComponentsUtility.CreateInitialComponents(pawn);
            pawn.health.Notify_Resurrected();
            if (pawn.Faction != null && pawn.Faction.IsPlayer)
            {
                if (pawn.workSettings != null)
                {
                    pawn.workSettings.EnableAndInitialize();
                }
                Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(pawn, PopAdaptationEvent.GainedColonist);
            }
            if (pawn.RaceProps.IsMechanoid && MechRepairUtility.IsMissingWeapon(pawn))
            {
                MechRepairUtility.GenerateWeapon(pawn);
            }
            if (flag)
            {
                GenSpawn.Spawn(pawn, loc, map);
                if (pawn.Faction != null && pawn.Faction != Faction.OfPlayer && pawn.HostileTo(Faction.OfPlayer))
                {
                    LordMaker.MakeNewLord(pawn.Faction, new LordJob_AssaultColony(pawn.Faction), pawn.Map, Gen.YieldSingle(pawn));
                }
                if (pawn.apparel != null)
                {
                    List<Apparel> wornApparel = pawn.apparel.WornApparel;
                    for (int i = 0; i < wornApparel.Count; i++)
                    {
                        wornApparel[i].Notify_PawnResurrected();
                    }
                }
            }
            PawnDiedOrDownedThoughtsUtility.RemoveDiedThoughts(pawn);
            if (pawn.royalty != null)
            {
                pawn.royalty.Notify_Resurrected();
            }
            if (pawn.guest != null && pawn.guest.interactionMode == PrisonerInteractionModeDefOf.Execution)
            {
                pawn.guest.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
            }
            if (flag2 && pawn != null)
            {
                Find.Selector.Select(pawn, false, false);
            }
        }
    }
}
