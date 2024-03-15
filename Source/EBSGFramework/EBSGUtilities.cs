﻿using Verse;
using Verse.AI;
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
                    cachedLethalCapacities.Add(EBSGDefOf.Metabolism);
                }
                return cachedLethalCapacities;
            }
        }

        public static bool TargetIsPawn(LocalTargetInfo target, out Pawn pawn)
        {
            if (target.HasThing && target.Thing is Pawn targetPawn)
            {
                pawn = targetPawn;
                return true;
            }

            pawn = null;
            return false;
        }

        public static void AddedHediffError(Hediff hediff, Pawn pawn)
        {
            Log.Error(hediff.def + " is missing tags that are required for one of its EBSG comps. Removing the hediff to avoid more errors.");
            pawn.health.RemoveHediff(hediff);
        }

        public static bool ConditionOrExclusiveIsActive(GameConditionDef gameCondition, Map map)
        {
            if (map.GameConditionManager != null && !map.GameConditionManager.ActiveConditions.NullOrEmpty())
            {
                if (map.GameConditionManager.ConditionIsActive(gameCondition)) return true;
                foreach (GameCondition condition in map.GameConditionManager.ActiveConditions)
                {
                    if (!condition.def.CanCoexistWith(gameCondition) || !gameCondition.CanCoexistWith(condition.def)) return true;
                }
            }
            return false;
        }

        public static BodyPartRecord GetSemiRandomPartFromList(List<BodyPartDef> bodyParts, Pawn pawn)
        {
            List<BodyPartRecord> parts = pawn.RaceProps.body.GetPartsWithDef(bodyParts.RandomElement());


            if (parts.NullOrEmpty())
            {
                bodyParts.Shuffle();
                foreach (BodyPartDef bodyPart in bodyParts)
                {
                    parts = pawn.RaceProps.body.GetPartsWithDef(bodyPart);
                    if (!parts.NullOrEmpty()) break;
                }
            }

            return parts[0];
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
                    newHediff.Severity = hediff.initialSeverity;
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
                        newHediff.Severity = hediffDef.initialSeverity;
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

        public static IEnumerable<IntVec3> AffectedCells(LocalTargetInfo target, Map map, Pawn pawn, float radius)
        {
            if (target.Cell.Filled(pawn.Map))
            {
                yield break;
            }
            foreach (IntVec3 item in GenRadial.RadialCellsAround(target.Cell, radius, true))
            {
                if (item.InBounds(map) && GenSight.LineOfSightToEdges(target.Cell, item, map, true))
                {
                    yield return item;
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

        public static Hediff AddHediffToPart(Pawn pawn, BodyPartRecord bodyPart, HediffDef hediffDef, float initialSeverity = 1, float severityAdded = 0, bool onlyNew = false)
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

            if (firstHediffOfDef != null && onlyNew) pawn.health.RemoveHediff(firstHediffOfDef);

            if (firstHediffOfDef != null && !onlyNew)
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
                else if (initialSeverity > 0)
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
                    else if (initialSeverity > 0)
                    {
                        Hediff newHediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        newHediff.Severity = initialSeverity;
                        pawn.health.AddHediff(newHediff);
                    }
                }
            }
        }

        public static bool CheckGeneTrio(Pawn pawn, List<GeneDef> oneOfGenes = null, List<GeneDef> allOfGenes = null, List<GeneDef> noneOfGenes = null)
        {
            if (pawn == null || pawn.genes == null) return false;

            if (!oneOfGenes.NullOrEmpty() && !PawnHasAnyOfGenes(pawn, oneOfGenes)) return false;
            if (!allOfGenes.NullOrEmpty() && !PawnHasAllOfGenes(pawn, allOfGenes)) return false;
            if (!noneOfGenes.NullOrEmpty() && PawnHasAnyOfGenes(pawn, noneOfGenes)) return false;

            return true;
        }

        public static bool CheckHediffTrio(Pawn pawn, List<HediffDef> oneOfHediffs = null, List<HediffDef> allOfHediffs = null, List<HediffDef> noneOfHediffs = null)
        {
            if (pawn == null || pawn.health == null) return false;

            if (!oneOfHediffs.NullOrEmpty() && !PawnHasAnyOfHediffs(pawn, oneOfHediffs)) return false;
            if (!allOfHediffs.NullOrEmpty() && !PawnHasAllOfHediffs(pawn, allOfHediffs)) return false;
            if (!noneOfHediffs.NullOrEmpty() && PawnHasAnyOfHediffs(pawn, noneOfHediffs)) return false;

            return true;
        }

        public static bool CheckPawnCapabilitiesTrio(Pawn pawn, List<CapCheck> capChecks = null, List<SkillCheck> skillChecks = null, List<StatCheck> statChecks = null)
        {
            if (pawn == null) return false;

            if (!capChecks.NullOrEmpty())
            {
                foreach (CapCheck capCheck in capChecks)
                {
                    if (!pawn.health.capacities.CapableOf(capCheck.capacity))
                    {
                        if (capCheck.minCapValue > 0)
                        {
                            return false;
                        }
                        continue;
                    }
                    float capValue = pawn.health.capacities.GetLevel(capCheck.capacity);
                    if (capValue < capCheck.minCapValue)
                    {
                        return false;
                    }
                    if (capValue > capCheck.maxCapValue)
                    {
                        return false;
                    }
                }
            }
            if (!skillChecks.NullOrEmpty())
            {
                foreach (SkillCheck skillCheck in skillChecks)
                {
                    SkillRecord skill = pawn.skills.GetSkill(skillCheck.skill);
                    if (skill == null || skill.TotallyDisabled || skill.PermanentlyDisabled)
                    {
                        if (skillCheck.minLevel > 0)
                        {
                            return false;
                        }
                        continue;
                    }
                    if (skill.Level < skillCheck.minLevel)
                    {
                        return false;
                    }
                    if (skill.Level > skillCheck.maxLevel)
                    {
                        return false;
                    }
                }
            }
            if (!statChecks.NullOrEmpty())
            {
                foreach (StatCheck statCheck in statChecks)
                {
                    float statValue = pawn.GetStatValue(statCheck.stat);
                    if (statValue < statCheck.minStatValue)
                    {
                        return false;
                    }
                    if (statValue > statCheck.maxStatValue)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool CastingAbility(Pawn pawn)
        {
            if (pawn.stances.curStance is Stance_Busy stance_Busy)
            {
                if (stance_Busy.verb.verbProps.verbClass == typeof(Verb_CastAbility)) return true;
            }
            return false;
        }

        public static Thing GetCurrentTarget(Pawn pawn, bool onlyHostiles = true, bool onlyInFaction = false, bool autoSearch = false, float searchRadius = 50, bool LoSRequired = false)
        {
            if (pawn.stances.curStance is Stance_Busy stance_Busy)
            {
                Thing thing = stance_Busy.verb.CurrentTarget.Thing;
                if (LoSRequired && !GenSight.LineOfSight(pawn.Position, thing.Position, pawn.Map)) return null;
                if (onlyHostiles && !thing.HostileTo(pawn)) return null;
                if (onlyInFaction)
                {
                    if (thing is Pawn otherPawn && otherPawn.Faction == pawn.Faction) return thing;
                    return null;
                }
                return thing;
            }
            if (pawn.IsAttacking() && pawn.CurJob != null)
            {
                Thing thing = pawn.CurJob.targetA.Thing;
                if (thing != null)
                {
                    if (LoSRequired && !GenSight.LineOfSight(pawn.Position, thing.Position, pawn.Map)) return null;
                    if (onlyHostiles && !thing.HostileTo(pawn)) return null;
                    if (onlyInFaction)
                    {
                        if (thing is Pawn otherPawn && otherPawn.Faction == pawn.Faction) return thing;
                        return null;
                    }
                    return thing;
                }
            }
            if (autoSearch)
            {
                List<Pawn> pawns = pawn.Map.mapPawns.AllPawns;
                pawns.SortBy((Pawn c) => c.Position.DistanceToSquared(pawn.Position));
                foreach (Pawn otherPawn in pawns)
                {
                    if (LoSRequired && !GenSight.LineOfSight(pawn.Position, otherPawn.Position, pawn.Map)) continue;
                    if (otherPawn.Dead || otherPawn.Downed) continue;
                    if (otherPawn.Position.DistanceTo(pawn.Position) > searchRadius) break;
                    if (onlyHostiles && otherPawn.HostileTo(pawn)) return otherPawn;
                    if (onlyInFaction && otherPawn.Faction == pawn.Faction) return otherPawn;
                    if (!onlyHostiles && !onlyInFaction) return otherPawn;
                }
            }
            return null;
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
            if (pawn.health == null) return false;
            if (pawn.health.hediffSet.HasHediff(hediff)) return true;
            return false;
        }

        public static bool PawnHasAnyOfHediffs(Pawn pawn, List<HediffDef> hediffs)
        {
            if (pawn.health == null || pawn.health.hediffSet.hediffs.NullOrEmpty() || hediffs.NullOrEmpty()) return false;
            foreach (HediffDef hediff in hediffs)
                if (HasHediff(pawn, hediff)) return true;
            return false;
        }

        public static bool PawnHasAllOfHediffs(Pawn pawn, List<HediffDef> hediffs)
        {
            if (hediffs.NullOrEmpty()) return true;
            foreach (HediffDef hediff in hediffs)
                if (!HasHediff(pawn, hediff))
                    return false;
            return true;
        }

        public static bool AllNeedLevelsMet(Pawn pawn, List<NeedLevel> needLevels)
        {
            if (needLevels.NullOrEmpty() || pawn.needs == null) return true;
            foreach (NeedLevel needLevel in needLevels)
            {
                Need need = pawn.needs.TryGetNeed(needLevel.need);
                if (need != null)
                    if (need.CurLevel < needLevel.minNeedLevel || need.CurLevel > needLevel.maxNeedLevel)
                        return false;
            }
            return true;
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

        public static bool PawnHasAnyOfGenes(Pawn pawn, List<GeneDef> geneDefs = null, List<Gene> genes = null)
        {
            if (geneDefs.NullOrEmpty() && genes.NullOrEmpty()) return true;
            if (pawn.genes == null) return false;

            if (!geneDefs.NullOrEmpty())
            {
                foreach (Gene gene in pawn.genes.GenesListForReading)
                {
                    if (geneDefs.Contains(gene.def)) return true;
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

        public static bool PawnHasAllOfGenes(Pawn pawn, List<GeneDef> geneDefs = null, List<Gene> genes = null)
        {
            if (geneDefs.NullOrEmpty() && genes.NullOrEmpty()) return true;
            if (pawn.genes == null) return false;

            if (!geneDefs.NullOrEmpty())
            {
                foreach (GeneDef gene in geneDefs)
                {
                    if (!pawn.genes.HasGene(gene)) return false;
                }
            }
            if (!genes.NullOrEmpty())
            {
                foreach (Gene gene in genes)
                {
                    if (!pawn.genes.HasGene(gene.def)) return false;
                }
            }

            return true;
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

        public static void RemoveGenesFromPawn(Pawn pawn, List<GeneDef> genes = null, GeneDef gene = null)
        {
            if (pawn.genes == null) return;
            if (gene != null)
            {
                Gene target = pawn.genes.GetGene(gene);
                if (target != null) pawn.genes.RemoveGene(target);
            }
            if (!genes.NullOrEmpty())
            {
                foreach (GeneDef g in genes)
                {
                    Gene target = pawn.genes.GetGene(g);
                    if (target != null) pawn.genes.RemoveGene(target);
                }
            }
        }

        public static List<GeneDef> AddGenesToPawn(Pawn pawn, bool xenogene = true, List<GeneDef> genes = null, GeneDef gene = null)
        {
            if (pawn.genes == null) return null;
            List<GeneDef> addedGenes = new List<GeneDef>();

            if (gene != null)
            {
                if (!pawn.genes.HasGene(gene))
                {
                    pawn.genes.AddGene(gene, xenogene);
                    addedGenes.Add(gene);
                }
            }

            if (!genes.NullOrEmpty())
            {
                foreach (GeneDef geneDef in genes)
                {
                    if (!pawn.genes.HasGene(geneDef))
                    {
                        pawn.genes.AddGene(geneDef, xenogene);
                        addedGenes.Add(geneDef);
                    }
                }
            }

            return addedGenes;
        }

        public static List<AbilityDef> GivePawnAbilities(Pawn pawn, List<AbilityDef> abilities = null, AbilityDef ability = null)
        {
            List<AbilityDef> addedAbilities = new List<AbilityDef>();

            if (ability != null)
            {
                if (pawn.abilities.GetAbility(ability) == null)
                {
                    pawn.abilities.GainAbility(ability);
                    addedAbilities.Add(ability);
                }
            }

            if (!abilities.NullOrEmpty())
            {
                foreach (AbilityDef abilityDef in abilities)
                {
                    if (pawn.abilities.GetAbility(abilityDef) == null)
                    {
                        pawn.abilities.GainAbility(abilityDef);
                        addedAbilities.Add(abilityDef);
                    }
                }
            }

            return addedAbilities;
        }

        public static List<AbilityDef> RemovePawnAbilities(Pawn pawn, List<AbilityDef> abilities = null, AbilityDef ability = null)
        {
            List<AbilityDef> removedAbilities = new List<AbilityDef>();

            if (ability != null)
            {
                if (pawn.abilities.GetAbility(ability) != null)
                {
                    pawn.abilities.RemoveAbility(ability);
                    removedAbilities.Add(ability);
                }
            }

            if (!abilities.NullOrEmpty())
            {
                foreach (AbilityDef abilityDef in abilities)
                {
                    if (pawn.abilities.GetAbility(abilityDef) != null)
                    {
                        pawn.abilities.RemoveAbility(abilityDef);
                        removedAbilities.Add(abilityDef);
                    }
                }
            }

            return removedAbilities;
        }

        public static void GainRandomGeneSet(Pawn pawn, bool inheritGenes, bool removeGenesFromOtherLists,
                List<RandomXenoGenes> geneSets = null, List<GeneDef> alwaysAddedGenes = null, List<GeneDef> alwaysRemovedGenes = null, bool showMessage = true)
        {
            if (pawn.genes == null) return;
            List<GeneDef> genesToAdd = new List<GeneDef>();
            bool reverseInheritence = false;

            List<GeneDef> remainingGenes = new List<GeneDef>();
            foreach (Gene currentGene in pawn.genes.GenesListForReading) // Puts genes into a list that's easier to check
            {
                remainingGenes.Add(currentGene.def);
            }

            // Select a geneSet to be added
            if (!geneSets.NullOrEmpty())
            {
                float totalWeight = 0;
                foreach (RandomXenoGenes xenoGeneSet in geneSets)
                {
                    totalWeight += xenoGeneSet.weightOfGeneSet;
                }

                double randomNumber = new Random().NextDouble() * totalWeight;
                foreach (RandomXenoGenes xenoGeneSet in geneSets)
                {
                    if (randomNumber <= xenoGeneSet.weightOfGeneSet)
                    {
                        genesToAdd = xenoGeneSet.geneSet;
                        reverseInheritence = xenoGeneSet.reverseInheritence;
                        break;
                    }
                    randomNumber -= xenoGeneSet.weightOfGeneSet;
                }
            }

            if (reverseInheritence) inheritGenes = !inheritGenes;

            if (removeGenesFromOtherLists && !geneSets.NullOrEmpty())
            {
                foreach (RandomXenoGenes xenoGeneSet in geneSets) // For each list
                {
                    RemoveGenesFromPawn(pawn, xenoGeneSet.geneSet);
                }
            }
            else if (!geneSets.NullOrEmpty())
            {
                foreach (RandomXenoGenes xenoGeneSet in geneSets) // For each list
                {
                    if (xenoGeneSet.alwaysRemoveGenes)
                    {
                        RemoveGenesFromPawn(pawn, xenoGeneSet.geneSet);
                    }
                }
            }

            // Add and remove genes
            AddGenesToPawn(pawn, !inheritGenes, alwaysAddedGenes);
            AddGenesToPawn(pawn, !inheritGenes, genesToAdd);
            RemoveGenesFromPawn(pawn, alwaysRemovedGenes);

            // Wrap things up
            if (pawn.Faction == Faction.OfPlayer && showMessage) // If the pawn is in the player faction, give a message based on what is most relevant to the player.
            {
                if (!geneSets.NullOrEmpty()) Messages.Message("Random genes successfully generated!", MessageTypeDefOf.NeutralEvent, false);
                else if (!alwaysAddedGenes.NullOrEmpty()) Messages.Message("Genes successfully added to pawn!", MessageTypeDefOf.NeutralEvent, false);
                else if (!alwaysRemovedGenes.NullOrEmpty()) Messages.Message("Genes successfully removed from pawn!", MessageTypeDefOf.NeutralEvent, false);
                else Messages.Message("A gene randomizer has been successfully processed, but the mod dev gave me NOTHING to work with. Why?", MessageTypeDefOf.NeutralEvent, false);
            }
        }

        public static bool EquivalentGeneLists(List<GeneDef> geneListA, List<GeneDef> geneListB)
        {
            if (geneListA.NullOrEmpty()) return geneListB.NullOrEmpty();
            foreach (GeneDef gene in geneListA)
            {
                if (geneListB.NullOrEmpty()) return false;
                if (geneListB.Contains(gene))
                {
                    geneListB.Remove(gene);
                }
                else return false;
            }
            if (!geneListB.NullOrEmpty()) return false;
            return true;
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

        public static float StatFactorOrOne(Pawn pawn, StatDef statDef = null)
        {
            if (statDef == null) return 1;
            return pawn.GetStatValue(statDef);
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
            if (relatedGene == null) return true;
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
                        wornApparel[i].Notify_PawnResurrected(pawn);
                    }
                }
            }
            PawnDiedOrDownedThoughtsUtility.RemoveDiedThoughts(pawn);
            if (pawn.royalty != null)
            {
                pawn.royalty.Notify_Resurrected();
            }

            if (pawn.guest != null && pawn.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Execution))
                pawn.guest.SetNoInteraction();

            if (flag2 && pawn != null)
            {
                Find.Selector.Select(pawn, false, false);
            }
        }
    }
}
