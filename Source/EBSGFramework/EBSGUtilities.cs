﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace EBSGFramework
{
    public static class EBSGUtilities
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

        public static void GiveSimplePlayerMessage(string message, TargetInfo target, MessageTypeDef messageType)
        {
            if (message.CanTranslate())
                Messages.Message(message.Translate(), target, messageType);
            else
                Messages.Message(message, target, messageType);
        }

        public static string TranslateOrLiteral(this string input, string arg1 = null, string arg2 = null, string arg3 = null, string arg4 = null)
        {
            if (input == null) return null;
            if (input.CanTranslate())
                return input.Translate(arg1, arg2, arg3, arg4).Resolve();
            if (arg1 != null)
            {
                input = input.Replace("{0}", arg1);
                if (arg2 != null)
                {
                    input = input.Replace("{1}", arg2);
                    if (arg3 != null)
                    {
                        input = input.Replace("{2}", arg3);
                        if (arg4 != null)
                            input = input.Replace("{3}", arg4);
                    }
                }
            }
            return input;
        }

        public static bool AbilityCompSucceeds(float baseChance, Pawn caster, StatDef casterStat, bool casterDivides, Pawn target, StatDef targetStat, bool targetMultiplies)
        {
            float finalChance = AbilityCompSuccessChance(baseChance, caster, casterStat, casterDivides, target, targetStat, targetMultiplies);

            if (finalChance == 0) return false;
            if (finalChance == 1) return true;
            return Rand.RangeInclusive(0, 1) >= finalChance;
        }

        public static float AbilityCompSuccessChance(float baseChance, Pawn caster, StatDef casterStat, bool casterDivides, Pawn target, StatDef targetStat, bool targetMultiplies)
        {
            float finalChance = baseChance;

            if (caster != null && casterStat != null)
            {
                float casterStatValue = caster.GetStatValue(casterStat);
                if (!casterDivides) finalChance *= casterStatValue;
                else if (casterStatValue <= 0) finalChance = 1; // Avoiding weird divisors making things act out of the expected
                else finalChance /= caster.GetStatValue(casterStat);
            }

            if (target != null && targetStat != null)
            {
                float targetStatValue = target.GetStatValue(targetStat);
                if (!targetMultiplies)
                    // Avoids weird stat situations where we suddenly end up with a 0 or negative divisor by just making the result 1. That's presumably the general goal when lower is supposed to make the value higher
                    if (targetStatValue <= 0) finalChance = 1;
                    else finalChance /= targetStatValue;
                else finalChance *= target.GetStatValue(targetStat);
            }

            if (finalChance <= 0) return 0f;
            if (finalChance >= 1) return 1f;
            return finalChance;
        }

        public static bool TargetIsPawn(this LocalTargetInfo target, out Pawn pawn)
        {
            if (target.HasThing && target.Thing is Pawn targetPawn)
            {
                pawn = targetPawn;
                return true;
            }
            pawn = null;
            return false;
        }

        public static void AddedHediffError(this Hediff hediff, Pawn pawn)
        {
            Log.Error(hediff.def + " is missing things that are required for one of its EBSG comps. Removing the hediff to avoid more errors.");
            pawn.health.RemoveHediff(hediff);
        }

        public static int RemoveAllOfHediffs(this Pawn pawn, List<Hediff> hediffs)
        {
            int removeCount = 0;
            if (pawn != null && !hediffs.NullOrEmpty() && pawn.health != null && !pawn.health.hediffSet.hediffs.NullOrEmpty())
                foreach (Hediff hediff in hediffs)
                    if (pawn.HediffInHediffSet(hediff))
                    {
                        removeCount++;
                        pawn.health.RemoveHediff(hediff);
                    }
            return removeCount;
        }

        public static bool PawnHasAnyHediff(this Corpse corpse)
        {
            return corpse.InnerPawn.PawnHasAnyHediff();
        }

        public static bool PawnHasAnyHediff(this Pawn pawn)
        {
            return pawn.health != null && !pawn.health.hediffSet.hediffs.NullOrEmpty();
        }

        private static bool HediffInHediffSet(this Pawn pawn, Hediff hediff)
        {
            if (pawn.health == null || pawn.health.hediffSet.hediffs.NullOrEmpty()) return false;
            return pawn.health.hediffSet.hediffs.Contains(hediff);
        }

        public static bool ConditionOrExclusiveIsActive(this GameConditionDef gameCondition, Map map)
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

        public static BodyPartRecord GetSemiRandomPartFromList(this Pawn pawn, List<BodyPartDef> bodyParts)
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

        public static Thing CreateThingCreationItem(ThingCreationItem item, Pawn creater = null)
        {
            if (!Rand.Chance(item.chance)) return null;

            Thing thing = ThingMaker.MakeThing(item.thing, item.stuff);
            thing.stackCount = Math.Min(item.count, item.thing.stackLimit);
            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                compQuality.SetQuality(item.quality, ArtGenerationContext.Colony);

                if (creater != null)
                    QualityUtility.SendCraftNotification(thing, creater);
            }

            if (thing.TryGetComp<CompSpawnBaby>() != null && creater != null)
            {
                CompSpawnBaby babyComp = thing.TryGetComp<CompSpawnBaby>();
                Pawn mother = null;
                Pawn father = null;

                if (item.linkingHediff != null && HasHediff(creater, item.linkingHediff))
                {
                    creater.health.hediffSet.TryGetHediff(item.linkingHediff, out Hediff hediff);
                    if (hediff is HediffWithTarget linkingHediff && linkingHediff.target is Pawn partner)
                        if (partner.gender == Gender.Male)
                        {
                            mother = creater;
                            father = partner;
                        }
                        else
                        {
                            mother = partner;
                            father = creater;
                        }
                }
                else
                {
                    if (creater.gender == Gender.Male) father = creater;
                    else mother = creater;
                }

                babyComp.mother = mother;
                babyComp.father = father;
                babyComp.faction = creater.Faction;
            }

            return thing;
        }

        public static Hediff GetFirstHediffAttachedToPart(this Pawn pawn, HediffDef hediffDef, BodyPartRecord bodyPartRecord = null, BodyPartDef bodyPartDef = null)
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

        public static void RemoveDamage(this Pawn pawn, HediffDef hediffDef, BodyPartRecord bodyPart, float damageRemoved)
        {
            while (damageRemoved > 0)
            {
                Hediff hediff = pawn.GetFirstHediffAttachedToPart(hediffDef, bodyPart);
                if (hediff != null)
                {
                    float removalAmount = (hediff.Severity > damageRemoved) ? damageRemoved : hediff.Severity;
                    damageRemoved -= removalAmount;
                    hediff.Severity -= removalAmount;

                }
                else break;
            }
        }

        public static List<HediffDef> ApplyHediffs(this Pawn pawn, HediffDef hediff = null, List<HediffDef> hediffs = null)
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

        public static void RemoveHediffs(this Pawn pawn, HediffDef hediff = null, List<HediffDef> hediffs = null)
        {
            if (pawn?.health?.hediffSet == null) return;
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

        public static bool WithinAges(this Pawn pawn, float min, float max)
        {
            return (pawn.ageTracker.AgeBiologicalYearsFloat >= min || min == -1f) && (pawn.ageTracker.AgeBiologicalYearsFloat <= max || max == -1f);
        }

        public static bool WithinAges(this Pawn pawn, FloatRange ageRange)
        {
            return ageRange.Includes(pawn.ageTracker.AgeBiologicalYearsFloat);
        }

        public static bool WithinSeverityRanges(float severity, FloatRange? severityRange = null, List<FloatRange> severityRanges = null, bool assumeMin = true)
        {
            if (severityRange != null)
            {
                var s = (FloatRange)severityRange;
                if (s.min == s.max)
                {
                    if (assumeMin)
                        return severity >= s.min;
                    return severity <= s.min;
                }
                return s.Includes(severity);
            }

            if (!severityRanges.NullOrEmpty())
            {
                foreach (FloatRange f in severityRanges)
                {
                    if (f.Includes(severity)) return true;
                    if (f.min == f.max)
                        if (assumeMin)
                        {
                            if (severity >= f.min)
                                return true;
                        }
                        else if (severity <= f.min)
                            return true;
                }
            }

            return false;
        }

        public static void AddHediffToParts(this Pawn pawn, List<HediffToParts> hediffs = null, HediffToParts hediffToParts = null, bool removeWhenBeyondAges = false)
        {
            if (hediffToParts != null)
            {
                if (!pawn.WithinAges(hediffToParts.validAges) && !pawn.WithinAges(hediffToParts.minAge, hediffToParts.maxAge))
                {
                    if (removeWhenBeyondAges)
                        pawn.RemoveHediffsFromParts(null, hediffToParts);
                }
                else if (Rand.Chance(hediffToParts.chance))
                {
                    if (!hediffToParts.bodyParts.NullOrEmpty())
                    {
                        Dictionary<BodyPartDef, int> foundParts = new Dictionary<BodyPartDef, int>();

                        foreach (BodyPartDef bodyPartDef in hediffToParts.bodyParts)
                        {
                            if (pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).NullOrEmpty()) continue;
                            if (foundParts.NullOrEmpty() || !foundParts.ContainsKey(bodyPartDef))
                                foundParts.Add(bodyPartDef, 0);

                            if (hediffToParts.onlyIfNew) pawn.AddHediffToPart(pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).ToArray()[foundParts[bodyPartDef]], hediffToParts.hediff, hediffToParts.severity);
                            else pawn.AddHediffToPart(pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).ToArray()[foundParts[bodyPartDef]], hediffToParts.hediff, hediffToParts.severity, hediffToParts.severity);
                            foundParts[bodyPartDef]++;
                        }
                    }
                    else
                    {
                        if (HasHediff(pawn, hediffToParts.hediff))
                        {
                            if (!hediffToParts.onlyIfNew)
                            {
                                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffToParts.hediff);
                                hediff.Severity += hediffToParts.severity;
                            }
                        }
                        else
                            pawn.AddOrAppendHediffs(hediffToParts.severity, 0, hediffToParts.hediff);
                    }
                }
            }
            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffToParts hediffParts in hediffs)
                {
                    if (!WithinAges(pawn, hediffParts.validAges) || !WithinAges(pawn, hediffParts.minAge, hediffParts.maxAge))
                    {
                        if (removeWhenBeyondAges)
                            pawn.RemoveHediffsFromParts(null, hediffParts);
                        continue;
                    }
                    if (!Rand.Chance(hediffParts.chance)) continue;
                    if (!hediffParts.bodyParts.NullOrEmpty())
                    {
                        Dictionary<BodyPartDef, int> foundParts = new Dictionary<BodyPartDef, int>();
                        foreach (BodyPartDef bodyPartDef in hediffParts.bodyParts)
                        {
                            if (pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).NullOrEmpty()) continue;
                            if (foundParts.NullOrEmpty() || !foundParts.ContainsKey(bodyPartDef))
                                foundParts.Add(bodyPartDef, 0);

                            if (hediffParts.onlyIfNew) pawn.AddHediffToPart(pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).ToArray()[foundParts[bodyPartDef]], hediffParts.hediff, hediffParts.severity);
                            else pawn.AddHediffToPart(pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).ToArray()[foundParts[bodyPartDef]], hediffParts.hediff, hediffParts.severity, hediffParts.severity);
                            foundParts[bodyPartDef]++;
                        }
                    }
                    else
                    {
                        if (HasHediff(pawn, hediffParts.hediff))
                        {
                            if (hediffParts.onlyIfNew) continue;
                            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffParts.hediff);
                            hediff.Severity += hediffParts.severity;
                        }
                        else
                        {
                            pawn.AddOrAppendHediffs(hediffParts.severity, 0, hediffParts.hediff);
                        }
                    }
                }
            }
        }

        public static void RemoveHediffsFromParts(this Pawn pawn, List<HediffToParts> hediffs = null, HediffToParts hediffToParts = null)
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
                            foreach (Hediff hediff in pawn.health.hediffSet.hediffs) // Go through all the hediffs to try to find the hediff on the specified part
                                if (hediff.Part.def == bodyPart && hediff.def == hediffToParts.hediff)
                                {
                                    firstHediffOfDef = hediff;
                                    break;
                                }

                        if (firstHediffOfDef != null) pawn.health.RemoveHediff(firstHediffOfDef);
                    }
                }
            }
            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffToParts hediffPart in hediffs)
                {
                    if (!hediffPart.removeOnRemove) continue;
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
                                foreach (Hediff hediff in pawn.health.hediffSet.hediffs) // Go through all the hediffs to try to find the hediff on the specified part
                                    if (hediff.Part.def == bodyPart && hediff.def == hediffPart.hediff)
                                    {
                                        firstHediffOfDef = hediff;
                                        break;
                                    }

                            if (firstHediffOfDef != null) pawn.health.RemoveHediff(firstHediffOfDef);
                        }
                    }
                }
            }
        }

        public static bool GetThings(List<Thing> lookThrough, List<ThingDef> validThings, out List<Thing> matches)
        {
            matches = new List<Thing>();

            if (lookThrough.NullOrEmpty() || validThings.NullOrEmpty()) return false;

            foreach (Thing thing in lookThrough)
                if (validThings.Contains(thing.def))
                    matches.Add(thing);

            return !matches.NullOrEmpty();
        }

        public static void ThingAndSoundMaker(IntVec3 position, Map map, ThingDef thing = null, List<ThingDef> things = null, SoundDef sound = null)
        {
            if (position.IsValid && map != null)
            {
                if (thing != null)
                    GenSpawn.Spawn(ThingMaker.MakeThing(thing), position, map);
                if (!things.NullOrEmpty())
                    foreach (ThingDef newThing in things)
                        GenSpawn.Spawn(ThingMaker.MakeThing(newThing), position, map);
                if (sound != null) sound.PlayOneShot(new TargetInfo(position, map));
            }
        }

        public static bool GenerateThingFromCountClass(List<ThingDefCountClass> thingDefs, out List<Thing> results, Pawn pawn1 = null, Pawn pawn2 = null)
        {
            results = new List<Thing>();

            if (!thingDefs.NullOrEmpty())
                foreach (ThingDefCountClass thingCountClass in thingDefs)
                    if (Rand.Chance(thingCountClass.DropChance))
                    {
                        Thing thing = ThingMaker.MakeThing(thingCountClass.thingDef, thingCountClass.thingDef.MadeFromStuff ? thingCountClass.stuff : null);
                        thing.stackCount = thingCountClass.count;
                        if (thingCountClass.color != null)
                            thing.SetColor((Color)thingCountClass.color);
                        CompQuality quality = thing.TryGetComp<CompQuality>();
                        if (quality != null)
                            quality.SetQuality(thingCountClass.quality, null);
                        CompSpawnBabyRecharger spawnBaby = thing.TryGetComp<CompSpawnBabyRecharger>();
                        if (spawnBaby != null)
                        {
                            spawnBaby.mother = pawn1;
                            spawnBaby.father = pawn2;
                        }
                        results.Add(thing);
                    }

            return !results.NullOrEmpty();
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

        public static Hediff AddHediffToPart(this Pawn pawn, BodyPartRecord bodyPart, HediffDef hediffDef, float initialSeverity = 1, float severityAdded = 0, bool onlyNew = false)
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

            if (firstHediffOfDef != null && onlyNew) return null;

            if (firstHediffOfDef != null && !onlyNew)
            {
                try
                {
                    try // Try to make it a psylink
                    {
                        Hediff_Psylink hediff_Level = (Hediff_Psylink)firstHediffOfDef;
                        hediff_Level.ChangeLevel(Mathf.CeilToInt(severityAdded), false);
                    }
                    catch // Try to make it a leveling hediff
                    {
                        Hediff_Level hediff_Level = (Hediff_Level)firstHediffOfDef;
                        hediff_Level.ChangeLevel(Mathf.CeilToInt(severityAdded));
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

        public static Hediff CreateComplexHediff(this Pawn pawn, float severity, HediffDef hediff, Pawn other = null, BodyPartRecord bodyPart = null)
        {
            if (pawn?.health == null) return null;

            Hediff newHediff = HediffMaker.MakeHediff(hediff, pawn, bodyPart);
            newHediff.Severity = severity;

            if (newHediff is HediffWithTarget targetHediff)
                targetHediff.target = other;

            HediffComp_Link hediffComp_Link = newHediff.TryGetComp<HediffComp_Link>();
            if (hediffComp_Link != null)
            {
                hediffComp_Link.other = other;
                hediffComp_Link.drawConnection = other != pawn;
            }

            HediffComp_SpawnHumanlike hediffComp_SpawnBaby = newHediff.TryGetComp<HediffComp_SpawnHumanlike>();
            if (hediffComp_SpawnBaby != null)
            {
                hediffComp_SpawnBaby.faction = pawn.Faction;
                hediffComp_SpawnBaby.father = other;
                hediffComp_SpawnBaby.mother = pawn;
            }

            return newHediff;
        }

        public static void AddOrAppendHediffs(this Pawn pawn, float initialSeverity = 1, float severityIncrease = 0, HediffDef hediff = null, List<HediffDef> hediffs = null, Pawn other = null)
        {
            if (hediff != null)
            {
                if (HasHediff(pawn, hediff))
                    pawn.health.hediffSet.GetFirstHediffOfDef(hediff).Severity += severityIncrease;
                else if (initialSeverity > 0)
                    pawn.health.AddHediff(pawn.CreateComplexHediff(initialSeverity, hediff, other));
            }
            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in hediffs)
                {
                    if (HasHediff(pawn, hediffDef))
                        pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef).Severity += severityIncrease;
                    else if (initialSeverity > 0)
                        pawn.health.AddHediff(pawn.CreateComplexHediff(initialSeverity, hediffDef, other));
                }
            }
        }

        public static bool CheckGeneTrio(this Pawn pawn, List<GeneDef> oneOfGenes = null, List<GeneDef> allOfGenes = null, List<GeneDef> noneOfGenes = null)
        {
            if (pawn == null || pawn.genes == null) return false;

            if (!oneOfGenes.NullOrEmpty() && !PawnHasAnyOfGenes(pawn, out var oneOfGene, oneOfGenes)) return false;
            if (!allOfGenes.NullOrEmpty() && !PawnHasAllOfGenes(pawn, allOfGenes)) return false;
            if (!noneOfGenes.NullOrEmpty() && PawnHasAnyOfGenes(pawn, out var noneOfGene, noneOfGenes)) return false;

            return true;
        }

        public static bool CheckHediffTrio(this Pawn pawn, List<HediffDef> oneOfHediffs = null, List<HediffDef> allOfHediffs = null, List<HediffDef> noneOfHediffs = null)
        {
            if (pawn == null || pawn.health == null) return false;

            if (!oneOfHediffs.NullOrEmpty() && !PawnHasAnyOfHediffs(pawn, oneOfHediffs)) return false;
            if (!allOfHediffs.NullOrEmpty() && !PawnHasAllOfHediffs(pawn, allOfHediffs)) return false;
            if (!noneOfHediffs.NullOrEmpty() && PawnHasAnyOfHediffs(pawn, noneOfHediffs)) return false;

            return true;
        }

        public static bool CheckPawnCapabilitiesTrio(this Pawn pawn, List<CapCheck> capChecks = null, List<SkillCheck> skillChecks = null, List<StatCheck> statChecks = null)
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

        public static void FadingHediffs(this Pawn pawn, float severityPerTick = 0, HediffDef hediff = null, List<HediffDef> hediffs = null)
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

        public static bool HasHediff(this Pawn pawn, HediffDef hediff) // Only made this to make checking for null hediffSets require less work
        {
            if (pawn?.health?.hediffSet == null || hediff == null) return false;
            if (pawn.health.hediffSet.HasHediff(hediff)) return true;
            return false;
        }

        public static bool HasHediff(this Pawn pawn, HediffDef hediff, out Hediff result)
        {
            result = null;
            if (pawn?.health?.hediffSet == null || hediff == null) return false;
            result = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
            return result != null;
        }

        public static bool HasHediff(this Pawn pawn, HediffDef hediff, BodyPartRecord bodyPart)
        {
            if (pawn?.health?.hediffSet == null || hediff == null) return false;
            if (pawn.health.hediffSet.HasHediff(hediff, bodyPart)) return true;
            return false;
        }

        public static bool HasHediff(this Pawn pawn, HediffDef hediff, BodyPartRecord bodyPart, out Hediff result)
        {
            result = null;
            if (pawn?.health?.hediffSet == null || hediff == null) return false;
            foreach (Hediff h in pawn.health.hediffSet.hediffs)
                if (h.def == hediff && h.Part == bodyPart)
                {
                    result = h;
                    break;
                }
            return result != null;
        }

        public static bool PawnHasAnyOfHediffs(this Pawn pawn, List<HediffDef> hediffs, out List<Hediff> matches)
        {
            matches = new List<Hediff>();
            if (pawn.health == null || pawn.health.hediffSet.hediffs.NullOrEmpty() || hediffs.NullOrEmpty()) return false;
            foreach (HediffDef hediff in hediffs)
                if (HasHediff(pawn, hediff, out var match)) matches.Add(match);
            return !matches.NullOrEmpty();
        }

        public static bool PawnHasAnyOfHediffs(this Pawn pawn, List<HediffDef> hediffs)
        {
            if (pawn.health == null || pawn.health.hediffSet.hediffs.NullOrEmpty() || hediffs.NullOrEmpty()) return false;
            foreach (HediffDef hediff in hediffs)
                if (HasHediff(pawn, hediff)) return true;
            return false;
        }

        public static bool PawnHasAllOfHediffs(this Pawn pawn, List<HediffDef> hediffs)
        {
            if (hediffs.NullOrEmpty()) return true;
            foreach (HediffDef hediff in hediffs)
                if (!HasHediff(pawn, hediff))
                    return false;
            return true;
        }

        public static bool AllNeedLevelsMet(this Pawn pawn, List<NeedLevel> needLevels)
        {
            if (needLevels.NullOrEmpty() || pawn?.needs == null) return true;
            foreach (NeedLevel needLevel in needLevels)
            {
                Need need = pawn.needs.TryGetNeed(needLevel.need);
                if (need != null)
                    if (need.CurLevel < needLevel.minNeedLevel || need.CurLevel > needLevel.maxNeedLevel)
                    {
                        Log.Message($"{need.LabelCap} ({need.CurLevel}) is not between {needLevel.minNeedLevel} and {needLevel.maxNeedLevel}");
                        return false;
                    }
                // Doesn't have an else section because if the need doesn't exist, it's presumed to be at whatever level it needs to be
            }
            return true;
        }

        public static bool CapacityConditionsMet(this Pawn pawn, List<CapCheck> capLimiters)
        {
            if (capLimiters.NullOrEmpty()) return true;
            foreach (CapCheck capCheck in capLimiters)
            {
                if (!pawn.health.capacities.CapableOf(capCheck.capacity))
                {
                    if (capCheck.minCapValue > 0)
                        return false;
                    continue;
                }

                float capValue = pawn.health.capacities.GetLevel(capCheck.capacity);
                if (capValue < capCheck.minCapValue)
                    return false;
                if (capValue > capCheck.maxCapValue)
                    return false;
            }
            return true;
        }

        public static bool AllSkillLevelsMet(this Pawn pawn, List<SkillCheck> skillLimiters)
        {
            if (skillLimiters.NullOrEmpty() || pawn.skills == null) return true;

            foreach (SkillCheck skillCheck in skillLimiters)
            {
                SkillRecord skill = pawn.skills.GetSkill(skillCheck.skill);
                if (skill == null || skill.TotallyDisabled || skill.PermanentlyDisabled)
                {
                    if (skillCheck.minLevel > 0)
                        return false;
                    continue;
                }

                if (skill.Level < skillCheck.minLevel)
                    return false;
                if (skill.Level > skillCheck.maxLevel)
                    return false;
            }

            return true;
        }

        public static void HandleNeedOffsets(this Pawn pawn, List<NeedOffset> needOffsets, bool preventRepeats = true, int hashInterval = 200, bool hourlyRate = false, bool dailyRate = false)
        {
            if (needOffsets.NullOrEmpty() || pawn.needs == null || pawn.needs.AllNeeds.NullOrEmpty()) return;
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

        public static void HandleDRGOffsets(this Pawn pawn, List<GeneEffect> geneEffects, int hashInterval = 200, bool hourlyRate = false, bool dailyRate = false)
        {
            if (geneEffects.NullOrEmpty() || pawn.genes == null || pawn.genes.GenesListForReading.NullOrEmpty()) return;

            foreach (GeneEffect geneEffect in geneEffects)
            {
                if (HasRelatedGene(pawn, geneEffect.gene) && pawn.genes.GetGene(geneEffect.gene) is ResourceGene resourceGene)
                {
                    float offset = geneEffect.offset;
                    if (geneEffect.statFactor != null) offset *= pawn.GetStatValue(geneEffect.statFactor);
                    if (hourlyRate) offset *= hashInterval / 2500f;
                    else if (dailyRate) offset *= hashInterval / 60000f;
                    ResourceGene.OffsetResource(pawn, offset, resourceGene);
                }
            }
        }

        public static bool BadWeather(this Map map)
        {
            // Couldn't use the vanilla enjoyable outside, so I just checked the favorability
            return map.weatherManager.curWeather.favorability == Favorability.Bad || map.weatherManager.curWeather.favorability == Favorability.VeryBad;
        }

        public static bool PawnHasAnyOfGenes(this Pawn pawn, out GeneDef firstMatch, List<GeneDef> geneDefs = null, List<Gene> genes = null)
        {
            firstMatch = null;
            if (geneDefs.NullOrEmpty() && genes.NullOrEmpty()) return false;
            if (pawn.genes?.GenesListForReading.NullOrEmpty() != false) return false;

            if (!geneDefs.NullOrEmpty())
            {
                foreach (Gene gene in pawn.genes.GenesListForReading)
                {
                    if (!gene.Active || gene.Overridden) continue;
                    if (geneDefs.Contains(gene.def))
                    {
                        firstMatch = gene.def;
                        return true;
                    }
                }
            }
            if (!genes.NullOrEmpty())
            {
                foreach (Gene gene in pawn.genes.GenesListForReading)
                {
                    if (!gene.Active || gene.Overridden) continue;
                    if (genes.Contains(gene))
                    {
                        firstMatch = gene.def;
                        return true;
                    }
                }
            }

            return false;
        }

        public static List<GeneDef> GetAllGenesOnListFromPawn(this Pawn pawn, List<GeneDef> searchList)
        {
            List<GeneDef> results = new List<GeneDef>();

            if (pawn.genes?.GenesListForReading.NullOrEmpty() == false && !searchList.NullOrEmpty())
                foreach (GeneDef gene in searchList)
                    if (pawn.genes.GetGene(gene) != null)
                        results.Add(gene);

            return results;
        }

        public static List<GeneDef> GetAllMatchingGenes(List<GeneDef> listA, List<GeneDef> listB)
        {
            List<GeneDef> results = new List<GeneDef>();

            if (!listA.NullOrEmpty() && !listB.NullOrEmpty())
                foreach (GeneDef gene in listA)
                    if (listB.Contains(gene))
                        results.Add(gene);

            return results;
        }

        public static bool PawnHasAllOfGenes(this Pawn pawn, List<GeneDef> geneDefs = null, List<Gene> genes = null)
        {
            if (geneDefs.NullOrEmpty() && genes.NullOrEmpty()) return true;
            if (pawn.genes == null) return false;

            if (!geneDefs.NullOrEmpty())
            {
                foreach (GeneDef gene in geneDefs)
                {
                    if (!HasRelatedGene(pawn, gene)) return false;
                }
            }
            if (!genes.NullOrEmpty())
            {
                foreach (Gene gene in genes)
                {
                    if (!HasRelatedGene(pawn, gene.def)) return false;
                }
            }

            return true;
        }

        public static bool NeedFrozen(this Pawn pawn, NeedDef def)
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

        public static void RemoveGenesFromPawn(this Pawn pawn, List<GeneDef> genes = null, GeneDef gene = null)
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

        public static List<GeneDef> AddGenesToPawn(this Pawn pawn, bool xenogene = true, List<GeneDef> genes = null, GeneDef gene = null)
        {
            if (pawn.genes == null) return null;
            List<GeneDef> addedGenes = new List<GeneDef>();

            if (gene != null)
            {
                if (!HasRelatedGene(pawn, gene))
                {
                    pawn.genes.AddGene(gene, xenogene);
                    addedGenes.Add(gene);
                }
            }

            if (!genes.NullOrEmpty())
            {
                foreach (GeneDef geneDef in genes)
                {
                    if (!HasRelatedGene(pawn, gene))
                    {
                        pawn.genes.AddGene(geneDef, xenogene);
                        addedGenes.Add(geneDef);
                    }
                }
            }

            return addedGenes;
        }

        public static bool PawnHasAnyOfAbilities(this Pawn pawn, List<AbilityDef> abilities, out List<Ability> foundAbilities)
        {
            foundAbilities = new List<Ability>();

            if (pawn.abilities?.AllAbilitiesForReading.NullOrEmpty() == false)
                foreach (AbilityDef ability in abilities)
                {
                    Ability a = pawn.abilities.GetAbility(ability);
                    if (a != null)
                        foundAbilities.Add(a);
                }

            return !foundAbilities.NullOrEmpty();
        }

        public static List<AbilityDef> GivePawnAbilities(this Pawn pawn, List<AbilityDef> abilities = null, AbilityDef ability = null)
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

        public static List<AbilityDef> RemovePawnAbilities(this Pawn pawn, List<AbilityDef> abilities = null, AbilityDef ability = null)
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

        public static void AlterXenotype(this Pawn pawn, List<RandomXenotype> xenotypes, ThingDef filth, IntRange filthCount, bool setXenotype = true, bool sendMessage = true)
        {
            AlterXenotype(pawn, xenotypes.RandomElementByWeight((arg) => arg.weight).xenotype, filth, filthCount, setXenotype, sendMessage);
        }

        public static void AlterXenotype(this Pawn pawn, XenotypeDef xenotype, ThingDef filth, IntRange filthCount, bool setXenotype = true, bool sendMessage = true)
        {
            if (pawn?.genes == null || xenotype == null) return;

            if (setXenotype)
            {
                pawn.genes.Endogenes.RemoveWhere((arg) => arg.def.endogeneCategory != EndogeneCategory.HairColor && arg.def.endogeneCategory != EndogeneCategory.Melanin);
                pawn.genes.SetXenotype(xenotype);
            }
            else
            {
                pawn.genes.SetXenotypeDirect(xenotype);
                bool isGermline = xenotype.inheritable;
                List<Gene> genesListForReading = new List<Gene>(pawn.genes.GenesListForReading);
                List<Gene> genesListToRemove = new List<Gene>();
                for (int i = 0; i < xenotype.genes.Count; i++)
                {
                    if (!genesListForReading.NullOrEmpty())
                    {
                        foreach (Gene gene in genesListForReading)
                            if (xenotype.genes[i].ConflictsWith(gene.def) || xenotype.genes[i].prerequisite?.ConflictsWith(gene.def) == true)
                                genesListToRemove.Add(gene);

                        foreach (Gene gene in genesListToRemove)
                        {
                            genesListForReading.Remove(gene);
                            pawn.genes.RemoveGene(gene);
                        }
                    }
                    pawn.genes.AddGene(xenotype.genes[i], !isGermline);
                }
            }

            if (pawn.Spawned && filth != null)
                FilthMaker.TryMakeFilth(pawn.Position, pawn.Map, filth, pawn.LabelIndefinite(), filthCount.RandomInRange);

            if (sendMessage)
                Messages.Message("EBSG_XenotypeApplied".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);

            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        public static void GainRandomGeneSet(this Pawn pawn, bool inheritGenes, bool removeGenesFromOtherLists,
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

                double randomNumber = new System.Random().NextDouble() * totalWeight;
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
                if (!geneSets.NullOrEmpty()) Messages.Message("EBSG_RandomGenesGenerated".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);
                else if (!alwaysAddedGenes.NullOrEmpty()) Messages.Message("EBSG_GenesAdded".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);
                else if (!alwaysRemovedGenes.NullOrEmpty()) Messages.Message("EBSG_GenesRemoved".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);
                else Log.Error("A gene randomizer has been successfully processed, but no gene sets were listed.");
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

        public static bool AnyGeneDefSame(List<GeneDef> listA, List<GeneDef> listB)
        {
            if (listA.NullOrEmpty() || listB.NullOrEmpty()) return false;
            foreach (GeneDef gene in listA)
                if (listB.Contains(gene)) return true;
            return false;
        }

        public static bool CheckNearbyWater(this Pawn pawn, int maxNeededForTrue, out int waterCount, float maxDistance = 0)
        {

            if (!pawn.Spawned || pawn.Map == null)
            {
                waterCount = 0;
                return false; // If either of these situations are true, we really need to get out of here
            }

            return CheckNearbyWater(pawn.Position, pawn.Map, maxNeededForTrue, out waterCount, maxDistance);
        }

        public static bool CheckNearbyWater(this IntVec3 pos, Map map, int maxNeededForTrue, out int waterCount, float maxDistance = 0)
        {
            waterCount = 0;

            if (maxDistance <= 0) // If max distance is just the pawn's tile, only need to check the pawn's tile
            {
                if (pos.GetTerrain(map).IsWater) waterCount++;
                if (maxNeededForTrue <= waterCount) return true;
                return false;
            }

            List<IntVec3> waterTiles = map.AllCells.Where((IntVec3 p) => p.DistanceTo(pos) <= maxDistance && p.GetTerrain(map).IsWater).ToList();
            waterCount = waterTiles.Count;
            return maxNeededForTrue <= waterCount;
        }

        public static bool CheckNearbyTerrain(this Pawn pawn, List<TerrainDistance> terrains, out TerrainDef missingTerrain, out bool negativeTerrain)
        {
            if (!pawn.Spawned || pawn.Map == null || !pawn.Position.IsValid)
            {
                missingTerrain = null;
                negativeTerrain = false;
                return false; // If any of these situations are true, we really need to get out of here
            }

            return CheckNearbyTerrain(pawn.Position, pawn.Map, terrains, out missingTerrain, out negativeTerrain);
        }

        public static bool CheckNearbyTerrain(this Thing thing, List<TerrainDistance> terrains, out TerrainDef missingTerrain, out bool negativeTerrain)
        {
            if (!thing.Spawned || thing.Map == null || !thing.Position.IsValid)
            {
                missingTerrain = null;
                negativeTerrain = false;
                return false; // If any of these situations are true, we really need to get out of here
            }

            return CheckNearbyTerrain(thing.Position, thing.Map, terrains, out missingTerrain, out negativeTerrain);
        }

        public static bool CheckNearbyTerrain(this IntVec3 pos, Map map, List<TerrainDistance> terrains, out TerrainDef missingTerrain, out bool negativeTerrain)
        {
            negativeTerrain = false;
            missingTerrain = null;

            if (terrains.NullOrEmpty())
            {
                missingTerrain = null;
                return true;
            }

            bool flag = false; // Checks for desired terrain

            foreach (TerrainDistance terrain in terrains)
            {
                if (terrain.count <= 0)
                {
                    if (pos.GetTerrain(map) == terrain.terrain)
                    {
                        negativeTerrain = true;
                        missingTerrain = terrain.terrain;
                        continue;
                    }
                    List<IntVec3> terrainTiles = map.AllCells.Where((IntVec3 p) => p.DistanceTo(pos) <= terrain.maxDistance && p.GetTerrain(map) == terrain.terrain).ToList();
                    if (terrainTiles.NullOrEmpty())
                        return true;

                    negativeTerrain = true;
                    missingTerrain = terrain.terrain;
                }
                else if (!flag)
                {
                    // Checks the center tile first to try to avoid having to deal with all map tiles
                    if (terrain.count == 1 && pos.GetTerrain(map) == terrain.terrain)
                        return true;

                    if (terrain.maxDistance == 0)
                    {
                        if (pos.GetTerrain(map) != terrain.terrain)
                        {
                            negativeTerrain = false;
                            missingTerrain = terrain.terrain;
                            continue;
                        }
                        return true;
                    }
                    else
                    {
                        List<IntVec3> terrainTiles = map.AllCells.Where((IntVec3 p) => p.DistanceTo(pos) <= terrain.maxDistance && p.GetTerrain(map) == terrain.terrain).ToList();
                        if (terrainTiles.NullOrEmpty() || terrainTiles.Count < terrain.count)
                        {
                            negativeTerrain = false;
                            missingTerrain = terrain.terrain;
                            continue;
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        public static float StatFactorOrOne(this Pawn pawn, StatDef statDef = null)
        {
            if (statDef == null) return 1;
            return pawn.GetStatValue(statDef);
        }

        public static void RemoveChronicHediffs(this Pawn pawn)
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

        public static bool HasRelatedGene(this Pawn pawn, GeneDef relatedGene)
        {
            if (!ModsConfig.BiotechActive || pawn.genes == null || relatedGene == null) return false;
            return pawn.genes.HasActiveGene(relatedGene);
        }

        public static bool HasAnyOfRelatedGene(this Pawn pawn, List<GeneDef> relatedGenes)
        {
            if (!ModsConfig.BiotechActive || pawn == null || relatedGenes.NullOrEmpty() || pawn.genes == null) return false;

            foreach (GeneDef gene in relatedGenes)
                if (pawn.genes.HasActiveGene(gene)) return true;

            return false;
        }

        public static List<IntVec3> GetCone(LocalTargetInfo target, Pawn pawn, float minDistance, float maxDistance, float minAngle, float maxAngle)
        {
            var affectedCells = new List<IntVec3>();
            Vector3 targetPos = target.Cell.ToVector3Shifted();
            Vector3 startPosition = pawn.Position.ToVector3Shifted();

            // If the targetPos is closer than the min distance, push it out to that distance.
            if ((targetPos - startPosition).magnitude < minDistance)
                targetPos = startPosition + (targetPos - startPosition).normalized * minDistance;

            float distanceToTarget = (targetPos - startPosition).magnitude;

            float percentOfMaxDistnace = distanceToTarget / maxDistance;

            float angleAtDistance = Mathf.Lerp(maxAngle, minAngle, percentOfMaxDistnace);

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(pawn.Position, distanceToTarget, true))
            {
                Vector3 cellPos = cell.ToVector3Shifted();
                Vector3 direction = (cellPos - startPosition).normalized;
                float currentDistance = (targetPos - startPosition).magnitude;
                float angle = Vector3.Angle(direction, targetPos - startPosition);

                if (angle <= angleAtDistance / 2f &&
                    GenSight.LineOfSight(startPosition.ToIntVec3(), cell, pawn.Map, true) &&
                    !cell.Equals(pawn.Position)) // Check if it's not the cell the pawn is standing on
                {
                    affectedCells.Add(cell);
                }
            }
            return affectedCells;
        }


        // Resurrect utility with bug fix

        public static void TryToRevivePawn(this Pawn pawn)
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

        // EBSGAI Utilities

        public static Thing GetCurrentTarget(this Pawn pawn, bool onlyHostiles = true, bool onlyInFaction = false, bool autoSearch = false, float searchRadius = 50, bool LoSRequired = false, bool allowDowned = false)
        {
            if (!pawn.Spawned) return null;
            if (onlyHostiles && onlyInFaction) return null;
            if (pawn.stances.curStance is Stance_Busy stance_Busy && stance_Busy.verb?.CurrentTarget.Thing != null)
            {
                Thing thing = stance_Busy.verb.CurrentTarget.Thing;
                if (thing.Position.DistanceTo(pawn.Position) > searchRadius)
                {
                    if (autoSearch) return AutoSearchTarget(pawn, onlyHostiles, onlyInFaction, searchRadius, LoSRequired);
                    return null;
                }
                if ((LoSRequired && !GenSight.LineOfSight(pawn.Position, thing.Position, pawn.Map)) || (onlyHostiles && !thing.HostileTo(pawn)))
                {
                    if (autoSearch) return AutoSearchTarget(pawn, onlyHostiles, onlyInFaction, searchRadius, LoSRequired);
                    return null;
                }
                if (thing is Pawn otherPawn)
                {
                    if (otherPawn == pawn) return null;
                    if (!allowDowned && (otherPawn.Downed || otherPawn.Dead))
                    {
                        if (autoSearch) return AutoSearchTarget(pawn, onlyHostiles, onlyInFaction, searchRadius, LoSRequired);
                        return null;
                    }
                    if (onlyInFaction)
                    {
                        if (otherPawn.Faction == pawn.Faction) return thing;
                        return null;
                    }
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
            if (autoSearch) return AutoSearchTarget(pawn, onlyHostiles, onlyInFaction, searchRadius, LoSRequired);

            return null;
        }

        public static Pawn AutoSearchTarget(this Pawn pawn, bool onlyHostiles = true, bool onlyInFaction = false, float searchRadius = 50, bool LoSRequired = false)
        {

            List<Pawn> pawns = pawn.Map.mapPawns.AllPawns;
            pawns.SortBy((Pawn c) => c.Position.DistanceToSquared(pawn.Position));
            foreach (Pawn otherPawn in pawns)
            {
                if (otherPawn == pawn) continue;
                if (LoSRequired && !GenSight.LineOfSight(pawn.Position, otherPawn.Position, pawn.Map)) continue;
                if (otherPawn.Dead || otherPawn.Downed) continue;
                if (otherPawn.Position.DistanceTo(pawn.Position) > searchRadius) break;
                if (onlyHostiles && otherPawn.HostileTo(pawn)) return otherPawn;
                if (onlyInFaction && otherPawn.Faction == pawn.Faction) return otherPawn;
                if (!onlyHostiles && !onlyInFaction) return otherPawn;
            }

            return null;
        }

        public static bool NeedToMove(Ability ability, Pawn pawn, Pawn targetPawn = null)
        {
            if (ability.verb.verbProps.rangeStat != null) // If there's a range stat and the target is at risk for becoming too far away, move closer
            {
                if (targetPawn.pather.Moving)
                {
                    if (targetPawn.Position.DistanceTo(pawn.Position) > pawn.GetStatValue(ability.verb.verbProps.rangeStat))
                    {
                        return true;
                    }
                }
                else
                {
                    if (targetPawn.Position.DistanceTo(pawn.Position) > Math.Ceiling(pawn.GetStatValue(ability.verb.verbProps.rangeStat) / 2))
                    {
                        return true;
                    }
                }
            }
            else // If there's no range stat, just try to get in normal range
            {
                if (targetPawn.pather.Moving)
                {
                    if (targetPawn.Position.DistanceTo(pawn.Position) > ability.verb.verbProps.range)
                    {
                        return true;
                    }
                }
                else
                {
                    if (targetPawn.Position.DistanceTo(pawn.Position) > Math.Ceiling(ability.verb.verbProps.range / 2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CastingAbility(this Pawn pawn)
        {
            if (pawn.stances.curStance is Stance_Busy stance_Busy)
            {
                if (stance_Busy.verb.verbProps.verbClass == typeof(Verb_CastAbility)) return true;
            }
            return false;
        }

        public static Job GoToTarget(LocalTargetInfo target)
        {
            Job job = JobMaker.MakeJob(JobDefOf.Goto, target);
            job.checkOverrideOnExpire = true;
            job.expiryInterval = 500;
            job.collideWithPawns = true;
            return job;
        }

        public static bool AutoAttackingColonist(this Pawn pawn)
        {
            if (pawn.playerSettings != null && pawn.playerSettings.UsesConfigurableHostilityResponse && pawn.playerSettings.hostilityResponse == HostilityResponseMode.Attack) return true;
            return false;
        }
    }
}
