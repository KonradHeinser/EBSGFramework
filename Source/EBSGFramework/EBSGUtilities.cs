using System;
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

        public static Thought_Memory GetFirstMemoryOfDefWhereOtherPawnIs(this MemoryThoughtHandler memory, ThoughtDef thought, Pawn otherPawn)
        {
            for (int i = 0; i > memory.Memories.Count; i++)
                if (memory.Memories[i].def ==  thought && memory.Memories[i].otherPawn == otherPawn)
                    return memory.Memories[i];
            
            return null;
        }

        public static float RemainingBlood(this Pawn pawn)
        {
            if (pawn.health?.CanBleed != true)
                return 0f;
            if (pawn.health?.hediffSet?.HasHediff(HediffDefOf.BloodLoss) != true)
                return 1f;
            return 1f - pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss).Severity;
        }

        public static float RemainingBlood(this Corpse corpse)
        {
            if (corpse.InnerPawn == null)
                return 0f;
            return corpse.InnerPawn.RemainingBlood();
        }

        public static void GiveSimplePlayerMessage(string message, TargetInfo target, MessageTypeDef messageType)
        {
            if (message == null) return;
            if (message.CanTranslate())
                Messages.Message(message.Translate(), target, messageType);
            else
                Messages.Message(message, target, messageType);
        }

        public static string TranslateOrFormat(this string input, string arg1 = null, string arg2 = null, string arg3 = null, string arg4 = null)
        {
            if (input == null) return null;
            if (input.CanTranslate())
                return input.Translate(arg1, arg2, arg3, arg4).Resolve();
            return input.Formatted(arg1, arg2, arg3, arg4);
        }

        public static bool TagCheck(List<string> a, List<string> b)
        {
            if (a.NullOrEmpty() || b.NullOrEmpty()) 
                return false;

            foreach (var str in a)
                if (b.Contains(str))
                    return true;

            return false;
        }

        public static bool CheckSeason(this Pawn pawn, List<Season> seasons, bool defaultActive = false)
        {
            if (seasons.NullOrEmpty() || pawn == null)
                return true;
            try
            {
                Season currentSeason = GenLocalDate.Season(pawn);
                switch (currentSeason)
                {
                    case Season.Undefined:
                        return defaultActive;
                    case Season.PermanentSummer:
                        return seasons.Contains(Season.PermanentSummer) || seasons.Contains(Season.Summer);
                    case Season.PermanentWinter:
                        return seasons.Contains(Season.PermanentWinter) || seasons.Contains(Season.Winter);
                    default:
                        return seasons.Contains(currentSeason);
                }
            }
            catch
            {
                return defaultActive;
            }
        }

        public static BodyTypeDef GetFixedBodyType(this Pawn pawn)
        {
            if (pawn.DevelopmentalStage == DevelopmentalStage.Adult && pawn.genes != null)
                foreach (var gene in pawn.genes.GenesListForReading)
                    if (gene.Active && gene.def.bodyType.HasValue)
                        return gene.def.bodyType.Value.ToBodyType(pawn);
            return null;
        }

        public static void ChangeGender(this Pawn pawn, Gender gender, BeardDef beard = null)
        {
            if (gender == pawn.gender || gender == Gender.None || !pawn.RaceProps.hasGenders) return;
            pawn.gender = gender;
            if (pawn.style != null)
                if (!pawn.style.CanWantBeard)
                    pawn.style.beardDef = BeardDefOf.NoBeard;
                else if (beard != null)
                    pawn.style.beardDef = beard;
            if (pawn.GetFixedBodyType() == null && pawn.story?.bodyType != null)
                    switch (pawn.gender)
                    {
                        case Gender.Female:
                            if (pawn.story.bodyType == BodyTypeDefOf.Male)
                            {
                                pawn.story.bodyType = BodyTypeDefOf.Female;
                                pawn.Drawer.renderer.SetAllGraphicsDirty();
                            }
                            break;
                        case Gender.Male:
                            if (pawn.story.bodyType == BodyTypeDefOf.Female)
                            {
                                pawn.story.bodyType = BodyTypeDefOf.Male;
                                pawn.Drawer.renderer.SetAllGraphicsDirty();
                            }
                            pawn.RemovePregnancies();
                            break;
                    }
        }

        public static void CheckGender(this Pawn pawn, List<GenderByAge> genderByAges, BeardDef beard = null)
        {
            if (!pawn.RaceProps.hasGenders) return;
            foreach (GenderByAge genderByAge in genderByAges)
                if (genderByAge.range.ValidValue(pawn.ageTracker.AgeBiologicalYearsFloat))
                {
                    if (genderByAge.gender != Gender.None)
                        pawn.ChangeGender(genderByAge.gender, beard);
                    return;
                }
        }

        public static bool PawnHasApparelOnLayer(this Pawn pawn, ApparelLayerDef layer = null, List<ApparelLayerDef> layers = null, List<BodyPartGroupDef> groups = null, List<ThingDef> exceptions = null)
        {
            if (pawn.apparel?.WornApparel?.NullOrEmpty() != false) 
                return false;

            if (layer != null)
                foreach (Apparel a in pawn.apparel.WornApparel)
                {
                    if (!exceptions.NullOrEmpty() && exceptions.Contains(a.def))
                        continue;

                    if (a.def.apparel.layers.Contains(layer) && ApparelHasAnyOfGroup(a, groups))
                        return true;
                }

            if (!layers.NullOrEmpty())
                foreach (Apparel a in pawn.apparel.WornApparel)
                {
                    if (!exceptions.NullOrEmpty() && exceptions.Contains(a.def))
                        continue;

                    foreach (var l in a.def.apparel.layers)
                        if (layers.Contains(l) && ApparelHasAnyOfGroup(a, groups))
                            return true;
                }

            return false;
        }

        public static bool ApparelHasAnyOfGroup(Apparel apparel, List<BodyPartGroupDef> groups)
        {
            if (groups.NullOrEmpty()) return true;
            if (apparel.def.apparel.bodyPartGroups.NullOrEmpty()) return false;

            foreach (var group in apparel.def.apparel.bodyPartGroups)
                if (groups.Contains(group))
                    return true;

            return false;
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

        public static int RemoveAllOfHediffs(this Pawn pawn, List<Hediff> hediffs)
        {
            if (hediffs.NullOrEmpty()) return 0;
            int removeCount = 0;
            if (pawn?.health?.hediffSet?.hediffs.NullOrEmpty() == false)
                foreach (Hediff hediff in hediffs)
                    if (hediff.pawn == pawn)
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
            return pawn.health?.hediffSet?.hediffs.NullOrEmpty() == false;
        }

        public static bool SetHasHediffs(this HediffSet set, List<HediffDef> hediffs, out List<Hediff> matches, bool checkPriceImpact = false)
        {
            matches = new List<Hediff>();

            if (!set.hediffs.NullOrEmpty() && !hediffs.NullOrEmpty())
                foreach (Hediff h in set.hediffs)
                    if ((!checkPriceImpact || !h.def.priceImpact) && hediffs.Contains(h.def))
                        matches.Add(h);

            return !matches.NullOrEmpty();
        }

        public static bool SetHasNoneOfHediffsMissing(this HediffSet set, List<HediffDef> hediffs, out List<HediffDef> remaining, bool checkPriceImpact = false)
        {
            remaining = new List<HediffDef>(hediffs);

            if (!set.hediffs.NullOrEmpty() && !hediffs.NullOrEmpty())
                foreach (Hediff h in set.hediffs)
                    if ((!checkPriceImpact || !h.def.priceImpact) && remaining.Contains(h.def))
                        remaining.Remove(h.def);

            return remaining.NullOrEmpty();
        }

        public static bool SetHasHediffsQuick(this HediffSet set, List<HediffDef> hediffs, out Hediff match, bool checkPriceImpact = false, bool emptyDefault = false)
        {
            match = null;
            if (hediffs.NullOrEmpty())
                return emptyDefault; // If the list is a forbidden list, this should remain false

            if (!set.hediffs.NullOrEmpty())
                foreach (Hediff h in set.hediffs)
                    if ((!checkPriceImpact || !h.def.priceImpact) && hediffs.Contains(h.def))
                    {
                        match = h;
                        break;
                    }

            return match != null;
        }

        public static bool ConditionOrExclusiveIsActive(this GameConditionDef gameCondition, Map map)
        {
            if (map.GameConditionManager != null && !map.GameConditionManager.ActiveConditions.NullOrEmpty())
            {
                if (map.GameConditionManager.ConditionIsActive(gameCondition)) 
                    return true;

                foreach (GameCondition condition in map.GameConditionManager.ActiveConditions)
                    if (!condition.def.CanCoexistWith(gameCondition) || !gameCondition.CanCoexistWith(condition.def)) 
                        return true;
            }
            return false;
        }

        public static BodyPartRecord GetSemiRandomPartFromList(this Pawn pawn, List<BodyPartDef> bodyParts)
        {
            // Try to grab a random set of parts
            List<BodyPartRecord> parts = pawn.RaceProps.body.GetPartsWithDef(bodyParts.RandomElement());

            if (parts.NullOrEmpty()) // If the first random didn't work, shuffle the list and start going through it
            {
                bodyParts.Shuffle();
                foreach (BodyPartDef bodyPart in bodyParts)
                {
                    parts = pawn.RaceProps.body.GetPartsWithDef(bodyPart);
                    if (!parts.NullOrEmpty()) break;
                }
            }
            if (parts.NullOrEmpty()) // If none of the parts could be found, give up
                return null;
            foreach (var part in parts) // Make sure you're returning a non-missing part
                if (!pawn.health.hediffSet.PartIsMissing(part))
                    return part;
            return null;
        }

        public static Thing CreateThingCreationItem(ThingCreationItem item, Pawn creater = null)
        {
            if (!Rand.Chance(item.chance) || (item.requireLink && item.linkingHediff != null &&
                !creater.HasHediff(item.linkingHediff))) return null;

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

        public static Hediff GetFirstHediffAttachedToPart(this Pawn pawn, HediffDef hediffDef, BodyPartRecord bodyPartRecord = null)
        {
            if (hediffDef == null) return null;

            if (bodyPartRecord == null)
            {
                if (pawn.HasHediff(hediffDef, out var result))
                    return result;
            }
            else if (HasHediff(pawn, hediffDef, bodyPartRecord, out var recordHediff))
                return recordHediff;

            return null;
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

        public static void RemovePregnancies(this Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null) return;
            List<Hediff> hediffsToRemove = new List<Hediff>();
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                if (hediff.def.pregnant)
                    hediffsToRemove.Add(hediff);
            pawn.RemoveAllOfHediffs(hediffsToRemove);
        }

        public static void RemoveHediffs(this Pawn pawn, HediffDef hediff = null, List<HediffDef> hediffs = null)
        {
            if (!pawn.PawnHasAnyHediff()) return;
            if (hediff != null && pawn.HasHediff(hediff, out var remove))
                pawn.health.RemoveHediff(remove);
            
            if (!hediffs.NullOrEmpty())
                foreach (HediffDef hediffDef in hediffs)
                    if (pawn.HasHediff(hediffDef, out var hediffToRemove))
                        pawn.health.RemoveHediff(hediffToRemove);
        }

        public static bool WithinAges(this Pawn pawn, float min, float max)
        {
            return (pawn.ageTracker.AgeBiologicalYearsFloat >= min || min == -1f) && (pawn.ageTracker.AgeBiologicalYearsFloat <= max || max == -1f);
        }

        public static bool WithinAges(this Pawn pawn, FloatRange ageRange)
        {
            return ageRange.ValidValue(pawn.ageTracker.AgeBiologicalYearsFloat);
        }

        public static bool ValidValue(this FloatRange range, float value, bool min = true)
        {
            if (range.min == range.max)
                if (min)
                    return value >= range.min;
                else
                    return value <= range.min;
            return range.Includes(value);
        }

        public static bool ValidValue(this IntRange range, int value, bool assumeMin = true)
        {
            if (range.min == range.max)
                if (assumeMin)
                    return value >= range.min;
                else
                    return value <= range.min;
            return range.Includes(value);
        }

        public static bool WithinSeverityRanges(float severity, FloatRange? severityRange = null, List<FloatRange> severityRanges = null, bool assumeMin = true)
        {
            if (severityRange != null)
                return ((FloatRange)severityRange).ValidValue(severity, assumeMin);

            if (!severityRanges.NullOrEmpty())
            {
                foreach (FloatRange f in severityRanges)
                    if (f.ValidValue(severity, assumeMin)) 
                        return true;
                return false;
            }

            return true;
        }

        public static void AddHediffToParts(this Pawn pawn, List<HediffToParts> hediffs = null, HediffToParts hediffToParts = null, bool removeWhenBeyondAges = false, int? degree = null)
        {
            if (pawn.health == null) return; // Unlikely, but possible
            if (hediffToParts != null && hediffToParts.DegreeCheck(degree))
            {
                if (!pawn.WithinAges(hediffToParts.validAges))
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
                    if (!hediffParts.DegreeCheck(degree))
                        continue;
                    if (!WithinAges(pawn, hediffParts.validAges))
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

                            pawn.AddHediffToPart(pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).ToArray()[foundParts[bodyPartDef]], hediffParts.hediff, hediffParts.severity, hediffParts.severity, hediffParts.onlyIfNew);
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

        public static void RemoveHediffsFromParts(this Pawn pawn, List<HediffToParts> hediffs = null, HediffToParts hediffToParts = null, int? degree = null)
        {
            if (hediffToParts != null && hediffToParts.DegreeCheck(degree))
                pawn.RemoveHediffFromParts(hediffToParts.hediff, hediffToParts.bodyParts);
            
            if (!hediffs.NullOrEmpty())
                foreach (HediffToParts hediffPart in hediffs)
                    if (hediffPart.removeOnRemove && hediffPart.DegreeCheck(degree)) 
                        pawn.RemoveHediffFromParts(hediffPart.hediff, hediffPart.bodyParts);
        }

        public static void RemoveHediffFromParts(this Pawn pawn, HediffDef hediff, List<BodyPartDef> bodyParts)
        {
            if (!pawn.HasHediff(hediff))
                return;

            if (bodyParts.NullOrEmpty())
                pawn.RemoveHediffs(hediff);
            else
                foreach (BodyPartDef bodyPart in bodyParts)
                {
                    Hediff firstHediffOfDef = null;
                    Hediff testHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(hediff);

                    if (testHediff.Part?.def == bodyPart) 
                        firstHediffOfDef = testHediff;
                    else if (pawn.health?.hediffSet?.hediffs.NullOrEmpty() == false)
                        foreach (Hediff h in pawn.health.hediffSet.hediffs) // Go through all the hediffs to try to find the hediff on the specified part
                            if (h.def == hediff && h.Part?.def == bodyPart)
                            {
                                firstHediffOfDef = h;
                                break;
                            }

                    if (firstHediffOfDef != null) 
                        pawn.health.RemoveHediff(firstHediffOfDef);
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
                if (!dictionary2.TryGetValue(phrase, out var value)) return false;
                if (dictionary1[phrase] != value) return false;
                dictionary2.Remove(phrase);
            }
            return dictionary2.NullOrEmpty();
        }

        public static Hediff AddHediffToPart(this Pawn pawn, BodyPartRecord bodyPart, HediffDef hediffDef, float initialSeverity = 1, float severityAdded = 0, bool onlyNew = false, Pawn other = null)
        {
            Hediff firstHediffOfDef = null;
            if (HasHediff(pawn, hediffDef, other, out var testHediff))
            {
                if (testHediff.Part == bodyPart) firstHediffOfDef = testHediff;
                else
                {
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs) // Go through all the hediffs to try to find the hediff on the specified part
                    {
                        if (hediff.Part == bodyPart && hediff.def == hediffDef) 
                            firstHediffOfDef = hediff;
                        break;
                    }
                }
            }

            if (firstHediffOfDef != null && onlyNew) 
                return null;

            if (firstHediffOfDef != null)
            {
                switch (firstHediffOfDef)
                {
                    case Hediff_Psylink psylink:
                        psylink.ChangeLevel(Mathf.CeilToInt(severityAdded), false);
                        break;
                    case Hediff_Level level:
                        level.ChangeLevel(Mathf.CeilToInt(severityAdded));
                        break;
                    default:
                        firstHediffOfDef.Severity += severityAdded;
                        break;
                }
            }
            else if (initialSeverity > 0)
            {
                firstHediffOfDef = pawn.CreateComplexHediff(initialSeverity, hediffDef, other, bodyPart);
                pawn.health.AddHediff(firstHediffOfDef);
            }

            return firstHediffOfDef;
        }

        public static List<Hediff> GetHediffFromParts(this Pawn pawn, HediffToParts hediffToParts)
        {
            return pawn.GetHediffFromParts(hediffToParts.hediff, hediffToParts.bodyParts);
        }
        
        public static List<Hediff> GetHediffFromParts(this Pawn pawn, HediffDef hediff, List<BodyPartDef> bodyParts)
        {
            List<Hediff> hediffs = new List<Hediff>();
            if (hediff != null && pawn.health?.hediffSet?.hediffs.NullOrEmpty() == false)
            {
                Dictionary<BodyPartDef, int> partCounts = new Dictionary<BodyPartDef, int>();
                foreach (var h in pawn.health.hediffSet.hediffs.Where(h => hediff == h.def))
                {
                    if (!bodyParts.NullOrEmpty())
                    {
                        if (h.Part == null)
                            continue;

                        if (!bodyParts.Contains(h.Part.def))
                            continue;

                        if (partCounts.ContainsKey(h.Part.def) &&
                            partCounts[h.Part.def] == bodyParts.Count(arg => arg == h.Part.def))
                            continue;

                        if (!partCounts.TryAdd(h.Part.def, 1))
                            partCounts[h.Part.def]++;
                    }
                    hediffs.Add(h);
                }
            }
            return hediffs;
        }

        public static void GiveHediffs(this List<HediffToGive> hediffs, Pawn caster, Pawn target = null, int durationCaster = -1, int durationTarget = -1, bool psychic = false, EndOn endOn = EndOn.End)
        {
            if (hediffs.NullOrEmpty())
                return;
            
            bool casterIsPsychic = (caster?.StatOrOne(StatDefOf.PsychicSensitivity) ?? 0) > 0;
            bool targetIsPsychic = (target?.StatOrOne(StatDefOf.PsychicSensitivity) ?? 0) > 0;
            bool checkCaster = caster != null && (!psychic || casterIsPsychic);
            bool checkTarget = target != null && (!psychic || targetIsPsychic);

            foreach (HediffToGive hediff in hediffs)
            {
                if (!Rand.Chance(hediff.chance))
                    if (endOn == EndOn.Fail || endOn == EndOn.FailIgnorePsychic)
                        break;
                    else
                        continue;
                float severity = hediff.severity.RandomInRange;
                bool flag = false; // Make sure the hediff doesn't fail due to psychic deafness
                List<BodyPartDef> partChecks = new List<BodyPartDef>();
                if (!hediff.bodyParts.NullOrEmpty())
                    partChecks = new List<BodyPartDef>(hediff.bodyParts);
                else if (hediff.onlyBrain)
                    partChecks.Add(caster.health.hediffSet.GetBrain().def);

                if (checkCaster && (hediff.applyToSelf || hediff.onlyApplyToSelf) && (!hediff.psychic || casterIsPsychic))
                {
                    HandleHediffToGive(caster, target, hediff.hediffDef, severity,
                        hediff.replaceExisting, hediff.skipExisting, partChecks, durationCaster);

                    if (!hediff.hediffDefs.NullOrEmpty())
                        foreach (HediffDef hd in hediff.hediffDefs)
                            HandleHediffToGive(caster, target, hd, severity, hediff.replaceExisting, hediff.skipExisting, 
                                partChecks, durationCaster);
                    flag = true;
                }
                if (checkTarget && hediff.applyToTarget && !hediff.onlyApplyToSelf && (!hediff.psychic || targetIsPsychic))
                {
                    HandleHediffToGive(target, caster, hediff.hediffDef, severity,
                        hediff.replaceExisting, hediff.skipExisting, partChecks, durationTarget);

                    if (!hediff.hediffDefs.NullOrEmpty())
                        foreach (HediffDef hd in hediff.hediffDefs)
                            HandleHediffToGive(target, caster, hd, severity, hediff.replaceExisting, hediff.skipExisting, 
                                partChecks, durationTarget);
                    flag = true;
                }

                bool flag2 = false;
                switch (endOn)
                {
                    case EndOn.Fail:
                        if (!flag)
                            flag2 = true;
                        break;
                    case EndOn.Success:
                        if (flag)
                            flag2 = true;
                        break;
                    case EndOn.FailIgnorePsychic:
                    case EndOn.End:
                    default:
                        break;
                }

                if (flag2)
                    break;
                if ((flag && endOn == EndOn.Success) || (!flag && endOn == EndOn.Fail))
                    break;
            }
        }

        public static WeaponTraitDef GetWeaponTrait(this ThingWithComps thing, WeaponTraitDef trait = null, List<WeaponTraitDef> traits = null)
        {
            var comp = thing.GetComp<CompUniqueWeapon>();

            if (comp?.TraitsListForReading.NullOrEmpty() == false)
            {
                if (trait != null && comp.TraitsListForReading.Contains(trait))
                    return trait;
                
                if (!traits.NullOrEmpty())
                    foreach (var t in comp.TraitsListForReading)
                        if (traits.Contains(t)) 
                            return t;
            }

            return null;
        }

        public static List<WeaponTraitDef> GetWeaponTraits(this ThingWithComps thing, WeaponTraitDef trait = null, List<WeaponTraitDef> traits = null)
        {
            var foundTraits = new List<WeaponTraitDef>();

            var comp = thing.GetComp<CompUniqueWeapon>();

            if (comp?.TraitsListForReading.NullOrEmpty() == false)
            {
                if (trait != null && comp.TraitsListForReading.Contains(trait))
                    foundTraits.Add(trait);

                if (!traits.NullOrEmpty())
                    foundTraits.AddRange(comp.TraitsListForReading.Where(traits.Contains));
            }

            return foundTraits;
        }

        public static void HandleHediffToGive(Pawn p, Pawn o, HediffDef hd, float severity, bool replaceExisting, bool skipExisting, List<BodyPartDef> partChecks, int duration = -1)
        {
            if (hd == null) return;
            var bodyParts = new List<BodyPartDef>(partChecks);
            var foundHediffs = p.GetHediffFromParts(hd, bodyParts);
            bool flag = false;
            foreach (var t in foundHediffs)
            {
                if (skipExisting)
                {
                    if (!bodyParts.NullOrEmpty())
                        bodyParts.Remove(t.Part.def);
                    flag = true;
                }
                else if (replaceExisting)
                    p.health.RemoveHediff(t);
                else if (severity > 0)
                {
                    t.Severity += severity;
                    if (!bodyParts.NullOrEmpty())
                        bodyParts.Remove(t.Part?.def);
                }
            }

            if (severity <= 0)
                return;

            if (!partChecks.NullOrEmpty())
            {
                if (bodyParts.NullOrEmpty())
                    return;
            }
            else if (flag) // Only needs checked when the body parts aren't able to handle the check
                return;

            var newHediffs = new List<Hediff>(p.CreateHediffOnParts(hd, severity, o, bodyParts, replaceExisting));

            if (!newHediffs.NullOrEmpty() && duration != -1)
                foreach (Hediff h in newHediffs)
                {
                    HediffComp_Disappears hediffComp_Disappears = h.TryGetComp<HediffComp_Disappears>();
                    if (hediffComp_Disappears != null)
                        hediffComp_Disappears.ticksToDisappear = duration;
                    p.health.AddHediff(h);
                }
        }

        public static List<Hediff> CreateHediffOnParts(this Pawn pawn, HediffDef hediff, float severity, Pawn other = null, List<BodyPartDef> bodyParts = null, bool replaceExisting = false)
        {
            if (hediff == null || pawn == null) return null;
            List<Hediff> hediffs = new List<Hediff>();

            if (bodyParts.NullOrEmpty())
            {
                Hediff newHediff = pawn.CreateComplexHediff(severity, hediff, other, null);
                pawn.health.AddHediff(newHediff);
                hediffs.Add(newHediff);
            }
            else
            {
                Dictionary<BodyPartDef, int> foundParts = new Dictionary<BodyPartDef, int>();

                foreach (BodyPartDef bodyPartDef in bodyParts)
                {
                    if (pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).NullOrEmpty()) continue;
                    if (foundParts.NullOrEmpty() || !foundParts.ContainsKey(bodyPartDef))
                        foundParts.Add(bodyPartDef, 0);
                    else if (pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).Count <= foundParts[bodyPartDef])
                        continue;
                    // Prevents someone from putting arm 4 times and breaking things. Ideally will allow race mods to make use of this

                    Hediff newHediff = pawn.AddHediffToPart(pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).ToArray()[foundParts[bodyPartDef]], hediff, severity, severity, replaceExisting, other);
                    foundParts[bodyPartDef]++;
                    hediffs.Add(newHediff);
                }
            }
            return hediffs;
        }

        public static Hediff CreateComplexHediff(this Pawn pawn, float severity, HediffDef hediff, Pawn other = null, BodyPartRecord bodyPart = null)
        {
            if (pawn?.health == null || hediff == null) return null;

            Hediff newHediff = HediffMaker.MakeHediff(hediff, pawn, bodyPart);
            newHediff.Severity = severity;

            if (other != null)
            {
                if (newHediff is HediffWithTarget targetHediff)
                    targetHediff.target = other;

                HediffComp_Link hediffComp_Link = newHediff.TryGetComp<HediffComp_Link>();
                if (hediffComp_Link != null)
                {
                    hediffComp_Link.other = other;
                    hediffComp_Link.drawConnection = other != pawn;
                }

                HediffComp_SpawnPawnKindOnRemoval hediffComp_SpawnPawnKindOnRemoval = newHediff.TryGetComp<HediffComp_SpawnPawnKindOnRemoval>();
                if (hediffComp_SpawnPawnKindOnRemoval != null)
                    hediffComp_SpawnPawnKindOnRemoval.instigator = other;
            }

            HediffComp_SpawnHumanlike hediffComp_SpawnBaby = newHediff.TryGetComp<HediffComp_SpawnHumanlike>();
            if (hediffComp_SpawnBaby != null)
            {
                hediffComp_SpawnBaby.faction = pawn.Faction;
                if (other != null)
                    hediffComp_SpawnBaby.father = other;
                hediffComp_SpawnBaby.mother = pawn;
            }

            return newHediff;
        }

        public static void ClampedSeverityOffset(this Hediff hediff, float change,  FloatRange limits)
        {
            hediff.Severity = Mathf.Clamp(hediff.Severity + change, limits.min, limits.max);
        }

        public static void AddOrAppendHediffs(this Pawn pawn, float initialSeverity = 1, float severityIncrease = 0, HediffDef hediff = null, List<HediffDef> hediffs = null, Pawn other = null, FloatRange? finalRange = null)
        {
            if (hediff != null)
                pawn.AddOrAppendHediff(initialSeverity, severityIncrease, hediff, other, finalRange);

            if (!hediffs.NullOrEmpty())
                foreach (HediffDef hediffDef in hediffs)
                    pawn.AddOrAppendHediff(initialSeverity, severityIncrease, hediffDef, other, finalRange);
        }

        public static void AddOrAppendHediff(this Pawn pawn, float initialSeverity = 1, float severityIncrease = 0, HediffDef hediff = null, Pawn other = null, FloatRange? finalRange = null)
        {
            // Clamps the initial value immediately so the change is carried for all new hediffs. Generally only relevant in cases where the initial can be changed by other things
            if (finalRange != null && initialSeverity > 0 && !finalRange.Value.ValidValue(initialSeverity))
                initialSeverity = Mathf.Clamp(initialSeverity, finalRange.Value.min, finalRange.Value.max);

            if (HasHediff(pawn, hediff, other, out var h))
                if (finalRange.HasValue && severityIncrease != 0)
                    h.ClampedSeverityOffset(severityIncrease, finalRange.Value);
                else
                    h.Severity += severityIncrease;
            else if (initialSeverity > 0)
                pawn.health.AddHediff(pawn.CreateComplexHediff(initialSeverity, hediff, other));
        }

        public static void CopyStageValues(this HediffStage stage, HediffStage newStage)
        {
            if (newStage == new HediffStage())
                return;
            stage.minSeverity = newStage.minSeverity;
            stage.label = newStage.label;
            stage.overrideLabel = newStage.overrideLabel;
            stage.untranslatedLabel = newStage.untranslatedLabel;
            stage.becomeVisible = newStage.becomeVisible;
            stage.lifeThreatening = newStage.lifeThreatening;
            stage.tale = newStage.tale;
            stage.vomitMtbDays = newStage.vomitMtbDays;
            stage.deathMtbDays = newStage.deathMtbDays;
            stage.mtbDeathDestroysBrain = newStage.mtbDeathDestroysBrain;
            stage.painFactor = newStage.painFactor;
            stage.painOffset = newStage.painOffset;
            stage.totalBleedFactor = newStage.totalBleedFactor;
            stage.naturalHealingFactor = newStage.naturalHealingFactor;
            stage.forgetMemoryThoughtMtbDays = newStage.forgetMemoryThoughtMtbDays;
            stage.pctConditionalThoughtsNullified = newStage.pctConditionalThoughtsNullified;
            stage.opinionOfOthersFactor = newStage.opinionOfOthersFactor;
            stage.hungerRateFactor = newStage.hungerRateFactor;
            stage.hungerRateFactorOffset = newStage.hungerRateFactorOffset;
            stage.restFallFactor = newStage.restFallFactor;
            stage.restFallFactorOffset = newStage.restFallFactorOffset;
            stage.socialFightChanceFactor = newStage.socialFightChanceFactor;
            stage.foodPoisoningChanceFactor = newStage.foodPoisoningChanceFactor;
            stage.mentalBreakMtbDays = newStage.mentalBreakMtbDays;
            stage.mentalBreakExplanation = newStage.mentalBreakExplanation;
            stage.allowedMentalBreakIntensities = newStage.allowedMentalBreakIntensities.NullOrEmpty() ? new List<MentalBreakIntensity>() : new List<MentalBreakIntensity>(newStage.allowedMentalBreakIntensities);
            stage.makeImmuneTo = newStage.makeImmuneTo.NullOrEmpty() ? null : new List<HediffDef>(newStage.makeImmuneTo);
            stage.capMods = newStage.capMods.NullOrEmpty() ? new List<PawnCapacityModifier>() : new List<PawnCapacityModifier>(newStage.capMods);
            stage.hediffGivers = newStage.hediffGivers.NullOrEmpty() ? new List<HediffGiver>() : new List<HediffGiver>(newStage.hediffGivers);
            stage.mentalStateGivers = newStage.mentalStateGivers.NullOrEmpty() ? new List<MentalStateGiver>() : new List<MentalStateGiver>(newStage.mentalStateGivers);
            stage.statOffsets = newStage.statOffsets.NullOrEmpty() ? new List<StatModifier>() : new List<StatModifier>(newStage.statOffsets);
            stage.statFactors = newStage.statFactors.NullOrEmpty() ? new List<StatModifier>() : new List<StatModifier>(newStage.statFactors);
            stage.multiplyStatChangesBySeverity = newStage.multiplyStatChangesBySeverity;
            stage.statOffsetEffectMultiplier = newStage.statOffsetEffectMultiplier;
            stage.statFactorEffectMultiplier = newStage.statFactorEffectMultiplier;
            stage.capacityFactorEffectMultiplier = newStage.capacityFactorEffectMultiplier;
            stage.disabledWorkTags = newStage.disabledWorkTags;
            stage.overrideTooltip = newStage.overrideTooltip;
            stage.extraTooltip = newStage.extraTooltip;
            stage.partEfficiencyOffset = newStage.partEfficiencyOffset;
            stage.partIgnoreMissingHP = newStage.partIgnoreMissingHP;
            stage.destroyPart = newStage.destroyPart;
        }

        // Copy old modifiers to ensure changing one won't cause weird issues down the line
        public static StatModifier Copy(this StatModifier modifier, float offset = 0f, float factor = 1f)
        {
            return new StatModifier()
            {
                stat = modifier.stat,
                value = modifier.value * factor + offset
            };
        }

        public static PawnCapacityModifier Copy(this PawnCapacityModifier modifier)
        {
            return new PawnCapacityModifier()
            {
                capacity = modifier.capacity,
                offset = modifier.offset,
                postFactor = modifier.postFactor,
                setMax = modifier.setMax,
                setMaxCurveEvaluateStat = modifier.setMaxCurveEvaluateStat,
                setMaxCurveOverride = modifier.setMaxCurveOverride,
                statFactorMod = modifier.statFactorMod
            };
        }

        public static bool NeedToSatisfyIDG(this Pawn pawn, out List<Hediff_Dependency> dependencies, bool quick = false)
        {
            dependencies = new List<Hediff_Dependency>();
            if (pawn.genes?.GenesListForReading.NullOrEmpty() != false)
                return false;

            foreach (Gene gene in pawn.genes.GenesListForReading)
                if (gene is Gene_Dependency d && d.LinkedHediff?.ShouldSatisfy == true)
                {
                    if (quick)
                        return true;
                    dependencies.Add(d.LinkedHediff);
                }

            return !dependencies.NullOrEmpty();
        }

        public static bool CheckXenotype(this Pawn pawn, out bool missing, List<XenotypeDef> oneOf = null, List<XenotypeDef> noneOf = null)
        {
            return CheckXenotype(pawn.genes, out missing, oneOf, noneOf);
        }

        public static bool CheckXenotype(this Pawn_GeneTracker tracker, out bool missing, List<XenotypeDef> oneOf = null, List<XenotypeDef> noneOf = null)
        {
            // missing is used to allow the thing calling to function to differentiate what list failed. If true, oneOf is the failure point
            // If the function returns true, then there is no reason to reference missing on the other side. 
            missing = true;
            if (tracker?.Xenotype == null)
                return oneOf.NullOrEmpty();

            if (!oneOf.NullOrEmpty() && !oneOf.Contains(tracker.Xenotype))
                return false;
            
            missing = false;
            return noneOf.NullOrEmpty() || !noneOf.Contains(tracker.Xenotype);
        }
        
        public static bool CheckGeneTrio(this Pawn pawn, List<GeneDef> oneOfGenes = null, List<GeneDef> allOfGenes = null, List<GeneDef> noneOfGenes = null)
        {
            if (pawn?.genes == null) return oneOfGenes.NullOrEmpty() && allOfGenes.NullOrEmpty();

            if (!oneOfGenes.NullOrEmpty() && !PawnHasAnyOfGenes(pawn, out _, oneOfGenes)) return false;
            if (!allOfGenes.NullOrEmpty() && !PawnHasAllOfGenes(pawn, allOfGenes)) return false;
            return noneOfGenes.NullOrEmpty() || !PawnHasAnyOfGenes(pawn, out _, noneOfGenes);
        }

        public static bool CheckGeneTrio(this Pawn_GeneTracker tracker, List<GeneDef> oneOfGenes = null, List<GeneDef> allOfGenes = null, List<GeneDef> noneOfGenes = null)
        {
            if (tracker?.GenesListForReading.NullOrEmpty() != false)
                return oneOfGenes.NullOrEmpty() && allOfGenes.NullOrEmpty();
            
            if (!oneOfGenes.NullOrEmpty() && !TrackerHasAnyOfGenes(tracker, out _, oneOfGenes)) return false;
            if (!allOfGenes.NullOrEmpty() && !TrackerHasAllOfGenes(tracker, allOfGenes)) return false;
            return noneOfGenes.NullOrEmpty() || !TrackerHasAnyOfGenes(tracker, out _, noneOfGenes);
        }

        public static bool CheckHediffTrio(this Pawn pawn, List<HediffDef> oneOfHediffs = null, List<HediffDef> allOfHediffs = null, List<HediffDef> noneOfHediffs = null, BodyPartRecord bodyPart = null)
        {
            if (pawn?.health == null) return oneOfHediffs.NullOrEmpty() && allOfHediffs.NullOrEmpty();
            
            if (!oneOfHediffs.NullOrEmpty() && !PawnHasAnyOfHediffs(pawn, oneOfHediffs, bodyPart)) return false;
            if (!allOfHediffs.NullOrEmpty() && !PawnHasAllOfHediffs(pawn, allOfHediffs, bodyPart)) return false;
            if (!noneOfHediffs.NullOrEmpty() && PawnHasAnyOfHediffs(pawn, noneOfHediffs, bodyPart)) return false;
            
            return true;
        }

        public static bool CheckHediffTrio(this Pawn pawn, List<HediffWithRange> oneOfHediffs = null, List<HediffWithRange> allOfHediffs = null, List<HediffWithRange> noneOfHediffs = null, BodyPartRecord bodyPart = null)
        {
            if (pawn?.health == null) return oneOfHediffs.NullOrEmpty() && allOfHediffs.NullOrEmpty();

            if (!oneOfHediffs.NullOrEmpty() && !PawnHasAnyOfHediffs(pawn, oneOfHediffs, bodyPart)) return false;
            if (!allOfHediffs.NullOrEmpty() && !PawnHasAllOfHediffs(pawn, allOfHediffs, bodyPart)) return false;
            if (!noneOfHediffs.NullOrEmpty() && PawnHasAnyOfHediffs(pawn, noneOfHediffs, bodyPart)) return false;

            return true;
        }

        public static bool CheckPawnCapabilitiesTrio(this Pawn pawn, List<CapCheck> capChecks = null, List<SkillLevel> skillChecks = null, List<StatCheck> statChecks = null)
        {
            if (pawn == null) return false;

            if (!capChecks.NullOrEmpty())
            {
                foreach (CapCheck capCheck in capChecks)
                {
                    if (!pawn.health.capacities.CapableOf(capCheck.capacity))
                    {
                        if (capCheck.range.min > 0)
                        {
                            return false;
                        }
                        continue;
                    }
                    float capValue = pawn.health.capacities.GetLevel(capCheck.capacity);
                    if (!capCheck.range.ValidValue(capValue))
                        return false;
                }
            }
            if (!skillChecks.NullOrEmpty())
            {
                foreach (SkillLevel skillCheck in skillChecks)
                {
                    SkillRecord skill = pawn.skills.GetSkill(skillCheck.skill);
                    if (skill == null || skill.TotallyDisabled || skill.PermanentlyDisabled)
                    {
                        if (skillCheck.range.min > 0)
                        {
                            return false;
                        }
                        continue;
                    }
                    if (!skillCheck.range.ValidValue(skill.Level))
                        return false;
                }
            }
            if (!statChecks.NullOrEmpty())
            {
                foreach (StatCheck statCheck in statChecks)
                {
                    float statValue = pawn.StatOrOne(statCheck.stat);
                    if (!statCheck.range.ValidValue(statValue))
                        return false;
                }
            }

            return true;
        }

        public static void FadingHediffs(this Pawn pawn, float severityPerTick = 0, HediffDef hediff = null, List<HediffDef> hediffs = null)
        {
            if (hediff != null && HasHediff(pawn, hediff, out Hediff h))
                h.Severity -= severityPerTick;
            
            if (!hediffs.NullOrEmpty())
                foreach (var hediffDef in hediffs)
                    if (HasHediff(pawn, hediffDef, out h))
                        h.Severity -= severityPerTick; 
        }

        public static IntVec3 FindDestination(this Map targetMap, bool targetCenter = false)
        {
            IntVec3 target;
            if (targetCenter) // If prioritizing center, start with seeing if it or a nearby cell is clear
            {
                target = targetMap.Center;
                if (target.Standable(targetMap))
                    return target;
                target = CellFinder.StandableCellNear(target, targetMap, 20);
                if (target.IsValid)
                    return target;
            }
            
            // Get a random edge cell and see if it's available 
            target = CellFinder.RandomEdgeCell(targetMap);
            if (target.Standable(targetMap)) return target;
            // Just find anything at this point, starting from that edge cell
            return RCellFinder.TryFindRandomClearCellsNear(target, 1, targetMap, out var cells) ? cells.First() : IntVec3.Invalid;
        }

        public static bool SetHasAllOfHediff(this HediffSet hediffSet, List<HediffDef> hediffs)
        {
            if (hediffs.NullOrEmpty())
                return true;
            
            return hediffSet?.hediffs.NullOrEmpty() == false && hediffSet.hediffs.All(h => hediffs.Contains(h.def));
        }
        
        public static bool SetHasAnyOfHediff(this HediffSet hediffSet, List<HediffDef> hediffs, out Hediff first)
        {
            first = null;
            if (hediffSet?.hediffs.NullOrEmpty() != false || hediffs.NullOrEmpty()) 
                return false;

            first = hediffSet.hediffs.FirstOrDefault(h => hediffs.Contains(h.def));
            return first != null;
        }
        
        public static bool HasHediff(this Pawn pawn, HediffToParts hediff)
        {
            return !pawn.GetHediffFromParts(hediff).NullOrEmpty();
        }
        
        public static bool HasHediff(this Pawn pawn, HediffDef hediff) // Only made this to make checking for null hediffSets require less work
        {
            if (pawn?.health?.hediffSet == null || hediff == null) return false;
            return pawn.health.hediffSet.HasHediff(hediff);
        }

        public static bool HasHediff(this Pawn pawn, HediffDef hediff, out Hediff result)
        {
            result = null;
            if (pawn?.health?.hediffSet == null || hediff == null) return false;
            result = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
            return result != null;
        }

        public static bool HasHediff(this Pawn pawn, HediffDef hediff, Pawn other, out Hediff result)
        {
            result = null;
            if (hediff == null)
                return false;
            
            // Check if there's actually supposed to be an other pawn
            if (other == null || !typeof(HediffWithTarget).IsAssignableFrom(hediff.hediffClass))
                return pawn.HasHediff(hediff, out result);

            if (pawn?.health?.hediffSet == null) return false;

            // Test to see if there's an easy way out
            result = pawn.health.hediffSet.GetFirstHediffOfDef(hediff); 
            if (result is HediffWithTarget targeter && targeter.target == other)
                return true;

            result = null;
            var listH = pawn.health.hediffSet.hediffs.Where((arg) => arg.def == hediff && 
                arg is HediffWithTarget t && t.target == other);
            if (!listH.EnumerableNullOrEmpty())
                result = listH.First();

            return result != null;
        }

        public static bool HasHediff(this Pawn pawn, HediffDef hediff, BodyPartRecord bodyPart)
        {
            if (pawn?.health?.hediffSet == null || hediff == null)
                return false;
            return pawn.health.hediffSet.HasHediff(hediff, bodyPart);
        }

        public static bool HasHediff(this Pawn pawn, HediffDef hediff, BodyPartRecord bodyPart, out Hediff result)
        {
            result = null;
            if (pawn?.health?.hediffSet == null || hediff == null) return false;

            if (bodyPart == null)
                return pawn.HasHediff(hediff, out result);

            foreach (var h in pawn.health.hediffSet.hediffs.Where(h => h.def == hediff && h.Part == bodyPart))
            {
                result = h;
                return true;
            }
            return false;
        }

        public static bool PawnHasAnyOfHediffs(this Pawn pawn, List<HediffDef> hediffs, out List<Hediff> matches, BodyPartRecord bodyPart = null)
        {
            if (pawn.health?.hediffSet?.hediffs?.NullOrEmpty() != false || hediffs.NullOrEmpty())
            {
                matches = new List<Hediff>();
                return false;
            }

            matches = pawn.health.hediffSet.hediffs.Where(hediff => hediffs.Contains(hediff.def) && (bodyPart == null || hediff.Part == bodyPart)).ToList();
            return !matches.NullOrEmpty();
        }

        public static bool PawnHasAnyOfHediffs(this Pawn pawn, List<HediffDef> hediffs, out Hediff match, BodyPartRecord bodyPart = null)
        {
            match = null;
            if (pawn.health?.hediffSet?.hediffs?.NullOrEmpty() != false || hediffs.NullOrEmpty()) 
                return false;

            foreach (var hediff in pawn.health.hediffSet.hediffs.Where(hediff => hediffs.Contains(hediff.def) && (bodyPart == null || hediff.Part == bodyPart)))
            {
                match = hediff;
                return true;
            }
            return false;
        }

        public static bool PawnHasAnyOfHediffs(this Pawn pawn, List<HediffWithRange> hediffs, out List<Hediff> matches, BodyPartRecord bodyPart = null)
        {
            matches = new List<Hediff>();
            if (pawn.health == null || pawn.health.hediffSet.hediffs.NullOrEmpty() || hediffs.NullOrEmpty()) return false;
            foreach (var hediff in hediffs)
            {
                var found = pawn.health.hediffSet.hediffs.Where((arg) => arg.def == hediff.hediff && (bodyPart == null || arg.Part == bodyPart) && hediff.range.ValidValue(arg.Severity));
                if (!found.EnumerableNullOrEmpty())
                    matches.AddRange(found);
            }
            return !matches.NullOrEmpty();
        }

        public static bool PawnHasAnyOfHediffs(this Pawn pawn, List<HediffDef> hediffs, BodyPartRecord bodyPart = null)
        {
            if (pawn?.health?.hediffSet?.hediffs.NullOrEmpty() != false || hediffs.NullOrEmpty()) return false;
            foreach (HediffDef hediff in hediffs)
                if (bodyPart != null)
                {
                    if (HasHediff(pawn, hediff, bodyPart)) return true;
                }
                else if (HasHediff(pawn, hediff)) return true;
            return false;
        }

        public static bool PawnHasAnyOfHediffs(this Pawn pawn, List<HediffWithRange> hediffs, BodyPartRecord bodyPart = null)
        {
            if (pawn?.health?.hediffSet?.hediffs.NullOrEmpty() != false || hediffs.NullOrEmpty()) return false;
            foreach (var hediff in hediffs)
                if (!pawn.health.hediffSet.hediffs.Where((arg) => arg.def == hediff.hediff && (bodyPart == null || arg.Part == bodyPart) && hediff.range.ValidValue(arg.Severity)).EnumerableNullOrEmpty())
                    return true;
            return false;
        }

        public static float PawnHediffRangeNum(this Pawn pawn, List<HediffWithRange> hediffs, BodyPartRecord bodyPart = null)
        {
            if (pawn?.health?.hediffSet?.hediffs.NullOrEmpty() != false || hediffs.NullOrEmpty()) return 0;

            return (from hediff in pawn.health.hediffSet.hediffs 
                from h in hediffs.Where(h => h.hediff == hediff.def) 
                select h.range.RandomInRange).FirstOrDefault();
        }

        public static bool PawnHasAllOfHediffs(this Pawn pawn, List<HediffDef> hediffs, BodyPartRecord bodyPart = null)
        {
            if (hediffs.NullOrEmpty()) return true;
            foreach (HediffDef hediff in hediffs)
                if (bodyPart != null)
                {
                    if (!HasHediff(pawn, hediff, bodyPart)) return false;
                }
                else if (!HasHediff(pawn, hediff)) return false;
            return true;
        }

        public static bool PawnHasAllOfHediffs(this Pawn pawn, List<HediffWithRange> hediffs, BodyPartRecord bodyPart = null)
        {
            if (hediffs.NullOrEmpty()) return true;
            foreach (var hediff in hediffs)
                if (pawn.health.hediffSet.hediffs.Where((arg) => arg.def == hediff.hediff && (bodyPart == null || arg.Part == bodyPart) && hediff.range.ValidValue(arg.Severity)).EnumerableNullOrEmpty())
                    return false;
            return true;
        }

        public static bool AllNeedLevelsMet(this Pawn pawn, List<NeedLevel> needLevels)
        {
            if (needLevels.NullOrEmpty() || pawn?.needs == null) return true;
            foreach (NeedLevel needLevel in needLevels)
            {
                Need need = pawn.needs.TryGetNeed(needLevel.need);
                if (need != null && !needLevel.range.ValidValue(need.CurLevel))
                    return false;
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
                    if (capCheck.range.min > 0)
                        return false;
                    continue;
                }

                float capValue = pawn.health.capacities.GetLevel(capCheck.capacity);
                if (!capCheck.range.ValidValue(capValue))
                    return false;
            }
            return true;
        }

        public static bool AllSkillLevelsMet(this Pawn pawn, List<List<SkillLevel>> skillLevels, bool includeAptitudes = true)
        {
            if (skillLevels.NullOrEmpty() || pawn.skills == null) return true;
            return Enumerable.Any(skillLevels, skillLevel => AllSkillLevelsMet(pawn, skillLevel, includeAptitudes));
        }

        public static bool AllSkillLevelsMet(this Pawn pawn, List<SkillLevel> skillLevels, bool includeAptitudes = true)
        {
            if (skillLevels.NullOrEmpty() || pawn.skills == null) return true;

            foreach (SkillLevel skillLevel in skillLevels)
            {
                SkillRecord skill = pawn.skills.GetSkill(skillLevel.skill);

                if (skill == null || skill.TotallyDisabled || skill.PermanentlyDisabled)
                {
                    if (skillLevel.range.min > 0 || skillLevel.range == skillLevel.defaultRange)
                        return false;
                    continue;
                }

                if (!skillLevel.range.ValidValue(skill.GetLevel(includeAptitudes)))
                    return false;
            }

            return true;
        }

        public static bool Includes(this IntRange range, int val)
        {
            if (val >= range.min)
                return val <= range.max;
            return false;
        }

        public static bool AllSkillLevelsMet(this Pawn pawn, List<SkillLevel> skillLimiters)
        {
            if (skillLimiters.NullOrEmpty() || pawn.skills == null) return true;

            foreach (SkillLevel skillCheck in skillLimiters)
            {
                SkillRecord skill = pawn.skills.GetSkill(skillCheck.skill);
                if (skill == null || skill.TotallyDisabled || skill.PermanentlyDisabled)
                {
                    if (skillCheck.range.min > 0)
                        return false;
                    continue;
                }
                if (!skillCheck.range.ValidValue(skill.Level))
                    return false;
            }

            return true;
        }

        public static void HandleNeedOffsets(this Pawn pawn, List<NeedOffset> needOffsets, bool preventRepeats = true, int hashInterval = 200, bool hourlyRate = false, bool dailyRate = false, int delta = 1)
        {
            if (needOffsets.NullOrEmpty() || pawn.needs == null || pawn.needs.AllNeeds.NullOrEmpty()) return;
            List<Need> alreadyPickedNeeds = new List<Need>();
            foreach (NeedOffset needOffset in needOffsets)
            {
                Need need;
                if (needOffset.need == null)
                {
                    need = preventRepeats ? pawn.needs.AllNeeds.Where(n => !alreadyPickedNeeds.Contains(n)).RandomElement() : pawn.needs.AllNeeds.RandomElement();
                }
                else need = pawn.needs.TryGetNeed(needOffset.need);

                if (need != null)
                {
                    alreadyPickedNeeds.Add(need);
                    float offset = needOffset.offset;
                    if (needOffset.offsetFactorStat != null) offset *= pawn.StatOrOne(needOffset.offsetFactorStat);
                    if (hourlyRate) offset *= hashInterval / 2500f;
                    else if (dailyRate) offset *= hashInterval / 60000f;
                    need.CurLevel += offset * delta;
                }
            }
        }

        public static void HandleDRGOffsets(this Pawn pawn, List<GeneEffect> geneEffects, int hashInterval = 200, bool hourlyRate = false, bool dailyRate = false, int delta = 1)
        {
            if (geneEffects.NullOrEmpty() || pawn.genes == null || pawn.genes.GenesListForReading.NullOrEmpty()) return;

            foreach (GeneEffect geneEffect in geneEffects)
            {
                if (HasRelatedGene(pawn, geneEffect.gene) && pawn.genes.GetGene(geneEffect.gene) is ResourceGene resourceGene)
                {
                    float offset = geneEffect.offset;
                    if (geneEffect.statFactor != null) offset *= pawn.StatOrOne(geneEffect.statFactor);
                    if (hourlyRate) offset *= hashInterval / 2500f;
                    else if (dailyRate) offset *= hashInterval / 60000f;
                    ResourceGene.OffsetResource(pawn, offset * delta, resourceGene);
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
            return TrackerHasAnyOfGenes(pawn.genes, out firstMatch, geneDefs, genes);
        }

        public static bool TrackerHasAnyOfGenes(this Pawn_GeneTracker tracker, out GeneDef firstMatch, List<GeneDef> geneDefs = null, List<Gene> genes = null)
        {
            firstMatch = null;
            if (tracker?.GenesListForReading.NullOrEmpty() != false || (geneDefs.NullOrEmpty() && genes.NullOrEmpty()))
                return false;

            var check = tracker.GenesListForReading.Where(g => g.Active && !g.Overridden);
            if (check.EnumerableNullOrEmpty())
                return false;
            
            if (!geneDefs.NullOrEmpty())
                foreach (var gene in check)
                    if (geneDefs.Contains(gene.def))
                    {
                        firstMatch = gene.def;
                        return true;
                    }
            
            if (!genes.NullOrEmpty())
                foreach (var gene in check)
                    if (genes.Contains(gene))
                    {
                        firstMatch = gene.def;
                        return true;
                    }

            return false;
        }

        public static bool GetSpecifiedGenesFromPawn(this Pawn pawn, List<GeneDef> genes, out List<Gene> matches)
        {
            matches = new List<Gene>();

            if (!genes.NullOrEmpty() && pawn.genes?.GenesListForReading.NullOrEmpty() == false)
                foreach (var g in pawn.genes.GenesListForReading)
                    if (genes.Contains(g.def))
                        matches.Add(g);

            return !matches.NullOrEmpty();
        }

        public static bool GetAllGenesOnListFromPawn(this Pawn pawn, List<GeneDef> searchList, out List<GeneDef> matches)
        {
            matches = GetAllGenesOnListFromPawn(pawn, searchList);
            return !matches.NullOrEmpty();
        }

        public static List<GeneDef> GetAllGenesOnListFromPawn(this Pawn pawn, List<GeneDef> searchList)
        {
            List<GeneDef> results = new List<GeneDef>();

            if (ModsConfig.BiotechActive && pawn.genes?.GenesListForReading.NullOrEmpty() == false && !searchList.NullOrEmpty()) 
                results.AddRange(from g in pawn.genes.GenesListForReading where searchList.Contains(g.def) select g.def);

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
            return TrackerHasAllOfGenes(pawn.genes, geneDefs, genes);
        }

        public static bool TrackerHasAllOfGenes(this Pawn_GeneTracker tracker, List<GeneDef> geneDefs = null, List<Gene> genes = null)
        {
            if (tracker?.GenesListForReading.NullOrEmpty() != false) 
                return false;
            
            return (geneDefs.NullOrEmpty() || geneDefs.All(tracker.HasActiveGene)) && (genes.NullOrEmpty() || genes.All(tracker.GenesListForReading.Contains));
        }

        public static bool PawnHasAllOfGenes(this Pawn pawn, out GeneDef failOn, List<GeneDef> geneDefs = null, List<Gene> genes = null)
        {
            failOn = null;
            if (geneDefs.NullOrEmpty() && genes.NullOrEmpty())
                return true;
            if (pawn.genes == null) 
                return false;

            if (!geneDefs.NullOrEmpty())
                foreach (GeneDef gene in geneDefs)
                    if (!HasRelatedGene(pawn, gene))
                    {
                        failOn = gene;
                        return false;
                    }

            if (!genes.NullOrEmpty())
                foreach (Gene gene in genes)
                    if (!HasRelatedGene(pawn, gene.def))
                    {
                        failOn = gene.def;
                        return false;
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

        public static List<GeneDef> AddGenesToPawn(this Pawn pawn, bool xenogene = true, List<GeneDef> genes = null, GeneDef gene = null, Gene parent = null)
        {
            if (pawn.genes == null) return null;
            List<GeneDef> addedGenes = new List<GeneDef>();

            if (gene != null && !HasRelatedGene(pawn, gene))
            {
                Gene newGene = pawn.genes.AddGene(gene, xenogene);
                if (parent != null)
                {
                    if (newGene is Gene_EvolvingGene evolving)
                        evolving.parent = parent as Gene_EvolvingGene;
                }
                addedGenes.Add(gene);
            }

            if (!genes.NullOrEmpty())
                foreach (GeneDef g in genes)
                    if (!HasRelatedGene(pawn, g))
                    {
                        pawn.genes.AddGene(g, xenogene);
                        addedGenes.Add(g);
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

        public static void AlterXenotype(this Pawn pawn, XenotypeDef xenotype, ThingDef filth, IntRange filthCount, bool setXenotype = true, bool sendMessage = true, string message = "EBSG_XenotypeApplied")
        {
            if (pawn?.genes == null || xenotype == null) return;

            if (setXenotype)
            {
                List<Gene> removeGenes = new List<Gene>(pawn.genes.Endogenes.Where((arg) => arg.def.endogeneCategory != EndogeneCategory.HairColor && arg.def.endogeneCategory != EndogeneCategory.Melanin));
                foreach (Gene gene in removeGenes)
                    pawn.genes.RemoveGene(gene);
                pawn.genes.SetXenotype(xenotype);
            }
            else
            {
                pawn.genes.SetXenotypeDirect(xenotype);
                bool isGermline = xenotype.inheritable;
                List<Gene> genesListForReading = new List<Gene>(pawn.genes.GenesListForReading);
                List<Gene> genesListToRemove = new List<Gene>();
                foreach (var t in xenotype.genes)
                {
                    if (!genesListForReading.NullOrEmpty())
                    {
                        genesListToRemove.AddRange(genesListForReading.Where(gene => 
                            t.ConflictsWith(gene.def) || t.prerequisite?.ConflictsWith(gene.def) == true));

                        foreach (Gene gene in genesListToRemove)
                        {
                            genesListForReading.Remove(gene);
                            pawn.genes.RemoveGene(gene);
                        }
                    }
                    pawn.genes.AddGene(t, !isGermline);
                }
            }

            if (pawn.Spawned && filth != null)
                FilthMaker.TryMakeFilth(pawn.Position, pawn.Map, filth, pawn.LabelIndefinite(), filthCount.RandomInRange);

            if (sendMessage && pawn.Faction.IsPlayer && (pawn.MapHeld != null || pawn.GetCaravan() != null))
                Messages.Message(message.TranslateOrFormat(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);

            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        public static void GainRandomGeneSet(this Pawn pawn, bool inheritGenes, bool removeGenesFromOtherLists,
                List<RandomXenoGenes> geneSets = null, List<GeneDef> alwaysAddedGenes = null, List<GeneDef> alwaysRemovedGenes = null, bool showMessage = true)
        {
            if (pawn.genes?.GenesListForReading.NullOrEmpty() != false) return;
            List<GeneDef> genesToAdd = new List<GeneDef>();
            bool reverseInheritance = false;

            // Select a geneSet to be added
            if (!geneSets.NullOrEmpty())
            {
                float totalWeight = geneSets.Sum(xenoGeneSet => xenoGeneSet.weightOfGeneSet);

                double randomNumber = new System.Random().NextDouble() * totalWeight;
                foreach (RandomXenoGenes xenoGeneSet in geneSets)
                {
                    if (randomNumber <= xenoGeneSet.weightOfGeneSet)
                    {
                        genesToAdd = xenoGeneSet.geneSet;
                        reverseInheritance = xenoGeneSet.reverseInheritence;
                        break;
                    }
                    randomNumber -= xenoGeneSet.weightOfGeneSet;
                }
            }

            if (reverseInheritance) inheritGenes = !inheritGenes;

            if (!geneSets.NullOrEmpty())
                if (removeGenesFromOtherLists)
                    foreach (RandomXenoGenes xenoGeneSet in geneSets) // For each list
                        RemoveGenesFromPawn(pawn, xenoGeneSet.geneSet);
                else
                    foreach (var xenoGeneSet in geneSets.Where(xenoGeneSet => xenoGeneSet.alwaysRemoveGenes))
                        RemoveGenesFromPawn(pawn, xenoGeneSet.geneSet);

            // Add and remove genes
            RemoveGenesFromPawn(pawn, alwaysRemovedGenes);
            AddGenesToPawn(pawn, !inheritGenes, alwaysAddedGenes);
            AddGenesToPawn(pawn, !inheritGenes, genesToAdd);

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
                    geneListB.Remove(gene);
                else return false;
            }
            return geneListB.NullOrEmpty();
        }

        public static bool AnyGeneDefSame(List<GeneDef> listA, List<GeneDef> listB)
        {
            if (listA.NullOrEmpty() || listB.NullOrEmpty()) return false;
            return Enumerable.Any(listA, listB.Contains);
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

            List<IntVec3> waterTiles = map.AllCells.Where(p => p.DistanceTo(pos) <= maxDistance && p.GetTerrain(map).IsWater).ToList();
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
                    List<IntVec3> terrainTiles = map.AllCells.Where(p => p.DistanceTo(pos) <= terrain.maxDistance && p.GetTerrain(map) == terrain.terrain).ToList();
                    if (terrainTiles.NullOrEmpty())
                        return true;

                    negativeTerrain = true;
                    missingTerrain = terrain.terrain;
                }
                else
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
                    }
                    else
                    {
                        List<IntVec3> terrainTiles = map.AllCells.Where(p => p.DistanceTo(pos) <= terrain.maxDistance && p.GetTerrain(map) == terrain.terrain).ToList();
                        if (terrainTiles.NullOrEmpty() || terrainTiles.Count < terrain.count)
                        {
                            negativeTerrain = false;
                            missingTerrain = terrain.terrain;
                            continue;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public static float StatOrOne(this Thing thing, StatDef statDef, StatRequirement statReq = StatRequirement.Always, int cacheDuration = 600)
        {
            if (statDef == null) return 1;
            var value = thing.GetStatValue(statDef, true, cacheDuration);
            switch (statReq)
            {
                case StatRequirement.Always:
                    return value;
                case StatRequirement.Lower:
                    return value < statDef.defaultBaseValue ? value : 1;
                case StatRequirement.Higher:
                    return value > statDef.defaultBaseValue ? value : 1;
                case StatRequirement.Pawn:
                    return thing is Pawn ? value : 1f;
                case StatRequirement.PawnLower:
                    return thing is Pawn && value < statDef.defaultBaseValue ? value : 1f;
                case StatRequirement.PawnHigher:
                    return thing is Pawn && value > statDef.defaultBaseValue ? value : 1f;
                case StatRequirement.NonPawn:
                    return thing is Pawn ? 1f : value;
                case StatRequirement.NonPawnLower:
                    return thing is Pawn || value >= statDef.defaultBaseValue ? 1f : value;
                case StatRequirement.NonPawnHigher:
                    return thing is Pawn || value <= statDef.defaultBaseValue ? 1f : value;
                case StatRequirement.Humanlike:
                    return thing is Pawn p && p.RaceProps.Humanlike ? value : 1f;
                case StatRequirement.HumanlikeLower:
                    return thing is Pawn l && l.RaceProps.Humanlike && value < statDef.defaultBaseValue ? value : 1f;
                case StatRequirement.HumanlikeHigher:
                    return thing is Pawn h && h.RaceProps.Humanlike && value > statDef.defaultBaseValue ? value : 1f;
            }

            return 1f;
        }

        public static float OutStatModifiedDamage(float damage, DamageModifyingStatsExtension extension, Thing victim, Thing attacker = null)
        {
            float offset = 0;
            
            if (attacker != null)
            {
                if (!extension.outgoingAttackerFactors.NullOrEmpty()) 
                    damage = extension.outgoingAttackerFactors.Aggregate(damage, (current, stat) => current * attacker.StatOrOne(stat, extension.outAttackFactorReq));

                if (!extension.outgoingAttackerModifiers.NullOrEmpty()) 
                    offset += extension.outgoingAttackerModifiers.Sum(stat => attacker.StatOrOne(stat.stat) * stat.value);

                if (!extension.outgoingAttackerDivisors.NullOrEmpty()) 
                    damage = extension.outgoingAttackerDivisors.Aggregate(damage, (current, stat) => current / attacker.StatOrOne(stat, extension.outAttackDivReq));
            }

            if (!extension.outgoingTargetFactors.NullOrEmpty()) 
                damage = extension.outgoingTargetFactors.Aggregate(damage, (current, stat) => current * victim.StatOrOne(stat, extension.outTargetFactorReq));

            if (!extension.outgoingTargetModifiers.NullOrEmpty()) 
                offset += extension.outgoingTargetModifiers.Sum(stat => victim.StatOrOne(stat.stat) * stat.value);

            if (!extension.outgoingTargetDivisors.NullOrEmpty()) 
                damage = extension.outgoingTargetDivisors.Aggregate(damage, (current, stat) => current / victim.StatOrOne(stat, extension.outTargetDivReq));

            damage += offset;
            
            if (extension.maxOutRemaining != -1f && damage > extension.maxOutRemaining)
                damage = extension.maxOutRemaining;
            
            if (damage < extension.minOutRemaining)
                damage = extension.minOutRemaining;
            
            return damage;
        }

        public static float IncStatModifiedDamage(float damage, DamageModifyingStatsExtension extension, Thing victim, Thing attacker = null)
        {
            float offset = 0;

            if (attacker != null)
            {
                if (!extension.incomingAttackerFactors.NullOrEmpty()) 
                    damage = extension.incomingAttackerFactors.Aggregate(damage, (current, stat) => current * attacker.StatOrOne(stat, extension.inAttackFactorReq));

                if (!extension.incomingAttackerModifiers.NullOrEmpty()) 
                    offset += extension.incomingAttackerModifiers.Sum(stat => attacker.StatOrOne(stat.stat) * stat.value);

                if (!extension.incomingAttackerDivisors.NullOrEmpty()) 
                    damage = extension.incomingAttackerDivisors.Aggregate(damage, (current, stat) => current / attacker.StatOrOne(stat, extension.inAttackDivReq));
            }

            if (!extension.incomingTargetFactors.NullOrEmpty()) 
                damage = extension.incomingTargetFactors.Aggregate(damage, (current, stat) => current * victim.StatOrOne(stat, extension.inTargetFactorReq));

            if (!extension.incomingTargetModifiers.NullOrEmpty()) 
                offset += extension.incomingTargetModifiers.Sum(stat => victim.StatOrOne(stat.stat) * stat.value);

            if (!extension.incomingTargetDivisors.NullOrEmpty()) 
                damage = extension.incomingTargetDivisors.Aggregate(damage, (current, stat) => current / Mathf.Max(victim.StatOrOne(stat, extension.inTargetDivReq), 0.0001f));

            damage += offset;

            if (extension.maxInRemaining != -1f && damage > extension.maxInRemaining)
                damage = extension.maxInRemaining;

            if (damage < extension.minInRemaining)
                damage = extension.minInRemaining;

            return damage;
        }

        public static void RemoveChronicHediffs(this Pawn pawn)
        {
            if (pawn.health.hediffSet != null && !pawn.health.hediffSet.hediffs.NullOrEmpty())
            {
                List<Hediff> hediffsToRemove = pawn.health.hediffSet.hediffs.Where(hediff => hediff.def.chronic).ToList();
                
                if (!hediffsToRemove.NullOrEmpty())
                    foreach (Hediff hediff in hediffsToRemove) 
                        pawn.health.RemoveHediff(hediff);
            }
        }

        public static bool QuickHasGene(this Pawn pawn, GeneDef gene)
        {
            return pawn.genes.HasActiveGene(gene);
        }

        public static bool HasRelatedGene(this Pawn pawn, GeneDef relatedGene)
        {
            if (!ModsConfig.BiotechActive || pawn.genes == null || relatedGene == null) 
                return false;

            return pawn.genes.HasActiveGene(relatedGene);
        }

        public static bool HasAnyOfRelatedGene(this Pawn pawn, List<GeneDef> relatedGenes)
        {
            if (!ModsConfig.BiotechActive || relatedGenes.NullOrEmpty() || pawn.genes == null) 
                return false;

            return relatedGenes.Any(pawn.genes.HasActiveGene);
        }

        public static bool CheckPawnGenes(this Pawn pawn, GeneDef gene, List<GeneDef> genes, bool mustHaveAll = false)
        {
            if (!ModsConfig.BiotechActive || pawn.genes == null)
                return false;

            if (pawn.genes.HasActiveGene(gene))
                return true;

            if (!genes.NullOrEmpty()) 
                return mustHaveAll ? genes.All(g => pawn.genes.HasActiveGene(g)) : Enumerable.Any(genes, pawn.genes.HasActiveGene);

            return false;
        }

        public static void SpawnHumanlikes(int numberToSpawn, IntVec3 initialPos, Map map, DevelopmentalStage developmentalStage, Pawn father, Pawn mother,
            Faction faction, List<GeneDef> genes, PawnKindDef staticPawnKind, XenotypeDef staticXenotype, XenoSource xenotypeSource, ThingDef filthOnCompletion,
            IntRange filthPerSpawn, bool sendLetters, string letterKey, string letterTextPawnDescription, string letterLabelNote, 
            bool bornThought, ThoughtDef motherBabyBornThought, ThoughtDef fatherBabyBornThought, bool noGear, string xenoLabel = null, 
            PawnRelationDef motherRelation = null, PawnRelationDef fatherRelation = null)
        {
            float fixedAge;

            switch (developmentalStage)
            {
                case DevelopmentalStage.Adult:
                    fixedAge = 18f;
                    break;
                case DevelopmentalStage.Child:
                    fixedAge = 8f;
                    break;
                case DevelopmentalStage.None:
                case DevelopmentalStage.Newborn:
                case DevelopmentalStage.Baby:
                default:
                    fixedAge = 0f;
                    break;
            }
            
            // If the faction is somehow null, the child will default to joining the player
            PawnGenerationRequest request = new PawnGenerationRequest(staticPawnKind ?? mother?.kindDef ?? father?.kindDef ?? PawnKindDefOf.Colonist,
                faction ?? Faction.OfPlayer, fixedLastName: RandomLastName(mother, father), allowDowned: true, forceNoIdeo: true, fixedBiologicalAge: fixedAge,
                fixedChronologicalAge: fixedAge, forcedXenotype: staticXenotype ?? XenotypeDefOf.Baseliner, developmentalStages: developmentalStage, forceNoGear:noGear)
            {
                DontGivePreArrivalPathway = true
            };
            
            if (staticXenotype == null)
                request.ForcedEndogenes = genes;

            List<IntVec3> alreadyUsedSpots = new List<IntVec3>();
            for (int i = 0; i < numberToSpawn; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(request);

                if (xenoLabel != null)
                {
                    switch (xenotypeSource)
                    {
                        case XenoSource.Mother when mother != null:
                            pawn.genes.iconDef = mother.genes.iconDef;
                            break;
                        case XenoSource.Father when father != null:
                            pawn.genes.iconDef = father.genes.iconDef;
                            break;
                        default:
                        {
                            if (GeneUtility.SameHeritableXenotype(mother, father) && mother?.genes?.UniqueXenotype == true)
                                pawn.genes.iconDef = mother.genes.iconDef;

                            if (TryGetInheritedXenotype(mother, father, out var xenotype))
                                pawn.genes?.SetXenotypeDirect(xenotype);
                            else if (ShouldByHybrid(mother, father))
                                pawn.genes.hybrid = true;

                            break;
                        }
                    }
                    pawn.genes.xenotypeName = xenoLabel;
                }
                else if (staticXenotype == null && (mother != null || father != null))
                    switch (xenotypeSource)
                    {
                        case XenoSource.Mother when mother != null:
                            pawn.genes.xenotypeName = mother.genes.xenotypeName;
                            pawn.genes.iconDef = mother.genes.iconDef;
                            break;
                        case XenoSource.Father when father != null:
                            pawn.genes.xenotypeName = father.genes.xenotypeName;
                            pawn.genes.iconDef = father.genes.iconDef;
                            break;
                        default:
                        {
                            if (GeneUtility.SameHeritableXenotype(mother, father) && mother?.genes?.UniqueXenotype == true)
                            {
                                pawn.genes.xenotypeName = mother.genes.xenotypeName;
                                pawn.genes.iconDef = mother.genes.iconDef;
                            }
                            
                            if (TryGetInheritedXenotype(mother, father, out var xenotype))
                                pawn.genes?.SetXenotypeDirect(xenotype);
                            else if (ShouldByHybrid(mother, father))
                            {
                                pawn.genes.hybrid = true;
                                pawn.genes.xenotypeName = "Hybrid".Translate();
                            }

                            break;
                        }
                    }

                if (map != null)
                {
                    IntVec3? intVec;

                    if (initialPos.Walkable(map) && (alreadyUsedSpots.NullOrEmpty() || !alreadyUsedSpots.Contains(initialPos)))
                    {
                        intVec = initialPos;
                        alreadyUsedSpots.Add(initialPos);
                    }
                    else intVec = CellFinder.RandomClosewalkCellNear(initialPos, map, 1, delegate (IntVec3 cell)
                    {
                        if (!alreadyUsedSpots.NullOrEmpty() && alreadyUsedSpots.Contains(cell)) return false;
                        if (cell != initialPos)
                        {
                            Building building = map.edificeGrid[cell];
                            if (building == null)
                            {
                                alreadyUsedSpots.Add(cell);
                                return true;
                            }

                            if (building.def?.IsBed != true) alreadyUsedSpots.Add(cell);
                            return building.def?.IsBed != true;
                        }
                        return false;
                    });
                    if (filthOnCompletion != null) 
                        FilthMaker.TryMakeFilth(intVec.Value, map, filthOnCompletion, filthPerSpawn.RandomInRange);

                    if (pawn.RaceProps.IsFlesh)
                    {
                        if (mother != null && motherRelation != null)
                            pawn.relations.AddDirectRelation(motherRelation, mother);
                        if (father != null && fatherRelation != null)
                            pawn.relations.AddDirectRelation(fatherRelation, father);
                    }
/*
                    if (pawn.playerSettings != null && origin?.playerSettings != null)
                        pawn.playerSettings.AreaRestrictionInPawnCurrentMap = origin.playerSettings.AreaRestrictionInPawnCurrentMap;
*/
                    if (GenSpawn.Spawn(pawn, intVec.Value, map) == null)
                        Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                    pawn.caller?.DoCall();
                }
                else
                {
                    Caravan caravan = mother.GetCaravan();
                    caravan.AddPawn(pawn, true);
                    if (!pawn.IsWorldPawn())
                        Find.WorldPawns.PassToWorld(pawn);
                }
                
                if (bornThought)
                {
                    if (mother?.Faction == faction)
                        mother?.needs?.mood?.thoughts?.memories?.TryGainMemory(motherBabyBornThought ?? ThoughtDefOf.BabyBorn, pawn);
                    father?.needs?.mood?.thoughts?.memories?.TryGainMemory(fatherBabyBornThought ?? ThoughtDefOf.BabyBorn, pawn);
                }

                if (sendLetters && faction.IsPlayer)
                {
                    pawn.babyNamingDeadline = Find.TickManager.TicksGame + 60000;
                    ChoiceLetter_BabyBirth birthLetter = (ChoiceLetter_BabyBirth)LetterMaker.MakeLetter("EBSG_CompSpawnPawn".Translate(pawn.Label, letterLabelNote.TranslateOrFormat()),
                        letterKey.Translate(father.Label, letterTextPawnDescription.TranslateOrFormat()), LetterDefOf.BabyBirth, pawn);
                    birthLetter.Start();
                    Find.LetterStack.ReceiveLetter(birthLetter);
                }
            }
        }

        public static void Refund(this Ability ability)
        {
            // Somewhat mitigates the effects of impossible miscasts
            if (ability.UsesCharges)
                ability.RemainingCharges += 1;
            else
                ability.ResetCooldown();

            Pawn caster = ability.pawn;
            
            foreach (var comp in ability.comps)
                switch (comp)
                {
                    case CompAbilityEffect_HemogenCost hemogenCost:
                        GeneUtility.OffsetHemogen(caster, hemogenCost.Props.hemogenCost);
                        break;
                    case CompAbilityEffect_ResourceCost resourceCost:
                        if (caster.genes?.GetGene(resourceCost.Props.mainResourceGene) is ResourceGene resourceGene)
                        {
                            float cost = resourceCost.Props.resourceCost;
                            if (resourceCost.Props.costFactorStat != null) cost *= caster.StatOrOne(resourceCost.Props.costFactorStat);
                            ResourceGene.OffsetResource(caster, 0f - cost, resourceGene, resourceGene.def.GetModExtension<DRGExtension>(), storeLimitPassing: !resourceCost.Props.checkMaximum);
                        }
                        break;
                }
        }

        public static void VaporizePawn(this Pawn pawn, DamageInfo? dinfo = null)
        {
            bool flag = PawnUtility.ShouldSendNotificationAbout(pawn);
            var caravan = pawn.GetCaravan();
            var map = pawn.MapHeld;
            pawn.Destroy();
            if (!pawn.Dead)
                pawn.health.SetDead();

            if (pawn?.Faction.IsPlayer == true)
                BillUtility.Notify_ColonistUnavailable(pawn);
            if (pawn.IsColonist)
                Find.StoryWatcher.statsRecord.Notify_ColonistKilled();
            pawn.royalty?.Notify_PawnKilled();
            
            if (flag)
                pawn.health.NotifyPlayerOfKilled(dinfo, null, caravan);

            Find.QuestManager.Notify_PawnKilled(pawn, dinfo);
            Find.FactionManager.Notify_PawnKilled(pawn);
            Find.IdeoManager.Notify_PawnKilled(pawn);
            if (pawn.IsMutant)
            {
                pawn.mutant.Notify_Died(null, dinfo);
                if (pawn.mutant.Def.clearMutantStatusOnDeath)
                    if (pawn.mutant.HasTurned)
                        pawn.mutant.Revert(true);
                    else
                        pawn.mutant = null;
            }

            pawn.duplicate?.Notify_PawnKilled();
            PawnComponentsUtility.RemoveComponentsOnKilled(pawn);
            if (ModsConfig.BiotechActive && MechanitorUtility.IsMechanitor(pawn))
                Find.History.Notify_MechanitorDied();
            if (ModsConfig.AnomalyActive && pawn.kindDef == PawnKindDefOf.Revenant)
                RevenantUtility.OnRevenantDeath(pawn, map);
            pawn.health.hediffSet.DirtyCache();
            pawn.Notify_DisabledWorkTypesChanged();
            Find.BossgroupManager.Notify_PawnKilled(pawn);
            try
            {
                //pawn.Ideo?.Notify_MemberDied(pawn); // Theoretically not needed because the lack of a corpse is the only thing that matters
                pawn.Ideo.Notify_MemberCorpseDestroyed(pawn);
                pawn.Ideo?.Notify_MemberLost(pawn, map);
            }
            catch (Exception ex)
            {
                Log.Error("Error while notifying ideo of pawn death: " + ex);
            }
            if (pawn.IsCreepJoiner)
                pawn.creepjoiner.Notify_CreepJoinerKilled();
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

            float percentOfMaxDistance = distanceToTarget / maxDistance;

            float angleAtDistance = Mathf.Lerp(maxAngle, minAngle, percentOfMaxDistance);

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(pawn.Position, distanceToTarget, true))
            {
                Vector3 cellPos = cell.ToVector3Shifted();
                Vector3 direction = (cellPos - startPosition).normalized;
                float angle = Vector3.Angle(direction, targetPos - startPosition);

                if (angle <= angleAtDistance / 2f &&
                    GenSight.LineOfSight(startPosition.ToIntVec3(), cell, pawn.Map, true) &&
                    !cell.Equals(pawn.Position)) // Check if it's not the cell the pawn is standing on
                    affectedCells.Add(cell);
            }
            return affectedCells;
        }

        // UI Stuff
        
        public static void TextFieldNumericLabeled<T>(this Listing_Standard standard, string label, TextAnchor anchor, ref T val, ref string buffer, float min = 0f, float max = 1E+09f, string tooltip = null, float labelPct = 0.75f) where T : struct
        {
            Rect rect = standard.GetRect(Text.LineHeight);
            if (!standard.BoundingRectCached.HasValue || rect.Overlaps(standard.BoundingRectCached.Value))
            {
                TextFieldNumericLabeled(rect, label, anchor, ref val, ref buffer, min, max, tooltip, labelPct);
            }

            standard.Gap(standard.verticalSpacing);
        }

        public static void TextFieldNumericLabeled<T>(Rect rect, string label, TextAnchor anchor, ref T val, ref string buffer, float min = 0f, float max = 1E+09f, string tooltip = null, float labelPct = 0.75f) where T : struct
        {
            Rect rect2 = rect.LeftPart(labelPct);
            Rect rect3 = rect.RightPart(1 - labelPct);
            Widgets.Label(rect2, label);
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(rect2, tooltip);
            }
            Text.Anchor = anchor;
            Widgets.TextFieldNumeric(rect3, ref val, ref buffer, min, max);
        }

        // Resurrect utility with bug fix

        public static SimpleCurve DementiaChancePerRotDaysCurve = new SimpleCurve
        {
            new CurvePoint(0.1f, 0.02f),
            new CurvePoint(5f, 0.8f)
        };

        public static SimpleCurve BlindnessChancePerRotDaysCurve = new SimpleCurve
        {
            new CurvePoint(0.1f, 0.02f),
            new CurvePoint(5f, 0.8f)
        };

        public static SimpleCurve ResurrectionPsychosisChancePerRotDaysCurve = new SimpleCurve
        {
            new CurvePoint(0.1f, 0.02f),
            new CurvePoint(5f, 0.8f)
        };

        public static void TryToRevivePawn(this Pawn pawn, bool sideEffects = false)
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
                if (!corpse.Destroyed) 
                    corpse.Destroy();
            }
            if (flag && pawn.IsWorldPawn())
                Find.WorldPawns.RemovePawn(pawn);
            
            pawn.ForceSetStateToUnspawned();
            PawnComponentsUtility.CreateInitialComponents(pawn);
            pawn.health.Notify_Resurrected();

            if (sideEffects)
            {
                BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
                float x2 = ((corpse?.GetComp<CompRottable>() == null) ? 0f : (corpse.GetComp<CompRottable>().RotProgress / 60000f));
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.ResurrectionSickness, pawn);
                if (!pawn.health.WouldDieAfterAddingHediff(hediff))
                    pawn.health.AddHediff(hediff);
                
                if (Rand.Chance(DementiaChancePerRotDaysCurve.Evaluate(x2)) && brain != null)
                {
                    Hediff hediff2 = HediffMaker.MakeHediff(HediffDefOf.Dementia, pawn, brain);
                    if (!pawn.health.WouldDieAfterAddingHediff(hediff2))
                        pawn.health.AddHediff(hediff2);
                }

                if (Rand.Chance(BlindnessChancePerRotDaysCurve.Evaluate(x2)))
                {
                    foreach (BodyPartRecord item in from x in pawn.health.hediffSet.GetNotMissingParts()
                                                    where x.def == BodyPartDefOf.Eye
                                                    select x)
                    {
                        if (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(item))
                        {
                            Hediff hediff3 = HediffMaker.MakeHediff(HediffDefOf.Blindness, pawn, item);
                            pawn.health.AddHediff(hediff3);
                        }
                    }
                }
                if (brain != null && Rand.Chance(ResurrectionPsychosisChancePerRotDaysCurve.Evaluate(x2)))
                {
                    Hediff hediff4 = HediffMaker.MakeHediff(HediffDefOf.ResurrectionPsychosis, pawn, brain);
                    if (!pawn.health.WouldDieAfterAddingHediff(hediff4))
                        pawn.health.AddHediff(hediff4);
                }
            }

            if (pawn.Faction != null && pawn.Faction.IsPlayer)
            {
                pawn.workSettings?.EnableAndInitialize();
                Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(pawn, PopAdaptationEvent.GainedColonist);
            }
            if (pawn.RaceProps.IsMechanoid && MechRepairUtility.IsMissingWeapon(pawn))
                MechRepairUtility.GenerateWeapon(pawn);
            
            if (flag)
            {
                GenSpawn.Spawn(pawn, loc, map);
                if (pawn.Faction != null && pawn.Faction != Faction.OfPlayer && pawn.HostileTo(Faction.OfPlayer))
                    LordMaker.MakeNewLord(pawn.Faction, new LordJob_AssaultColony(pawn.Faction), pawn.Map, Gen.YieldSingle(pawn));
                
                if (pawn.apparel != null)
                {
                    List<Apparel> wornApparel = pawn.apparel.WornApparel;
                    foreach (var t in wornApparel)
                        t.Notify_PawnResurrected(pawn);
                }
            }
            PawnDiedOrDownedThoughtsUtility.RemoveDiedThoughts(pawn);
            pawn.royalty?.Notify_Resurrected();

            if (pawn.guest != null && pawn.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Execution))
                pawn.guest.SetNoInteraction();

            if (flag2 && pawn != null)
                Find.Selector.Select(pawn, false, false);
        }

        // EBSGAI Utilities

        public static Thing GetCurrentTarget(this Pawn pawn, bool onlyHostiles = true, bool onlyInFaction = false, bool autoSearch = false, float searchRadius = 50, bool LoSRequired = false, bool allowDowned = false, Ability ability = null)
        {
            if (!pawn.Spawned) return null;
            if (onlyHostiles && onlyInFaction) return null;
            if (pawn.stances.curStance is Stance_Busy stance_Busy && stance_Busy.verb?.CurrentTarget.Thing != null)
            {
                Thing thing = stance_Busy.verb.CurrentTarget.Thing;
                if (thing.Position.DistanceTo(pawn.Position) > searchRadius || 
                    (LoSRequired && !GenSight.LineOfSight(pawn.Position, thing.Position, pawn.Map)) || 
                    (onlyHostiles && !thing.HostileTo(pawn)) || ability?.Valid(thing) == false)
                    return autoSearch ? AutoSearchTarget(pawn, onlyHostiles, onlyInFaction, searchRadius, LoSRequired, ability) : null;

                if (thing is Pawn otherPawn)
                {
                    if (otherPawn == pawn) return null;
                    if (!allowDowned && otherPawn.DeadOrDowned)
                        return autoSearch ? AutoSearchTarget(pawn, onlyHostiles, onlyInFaction, searchRadius, LoSRequired, ability) : null;
                    if (onlyInFaction)
                        return otherPawn.Faction == pawn.Faction ? thing : null;
                }

                return thing;
            }
            if (pawn.IsAttacking() && pawn.CurJob != null)
            {
                Thing thing = pawn.CurJob.targetA.Thing;
                if (thing != null)
                {
                    if (ability?.Valid(thing) == false) return null;
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
            return autoSearch ? AutoSearchTarget(pawn, onlyHostiles, onlyInFaction, searchRadius, LoSRequired, ability) : null;
        }

        public static Pawn AutoSearchTarget(this Pawn pawn, bool onlyHostiles = true, bool onlyInFaction = false, float searchRadius = 50, bool LoSRequired = false, Ability ability = null)
        {

            var pawns = new List<Pawn>(pawn.Map.mapPawns.AllPawnsSpawned).Where(p => p != pawn && !p.DeadOrDowned && !p.IsPrisoner && ability?.Valid(p) != false).ToList();
            pawns.SortBy(c => c.Position.DistanceToSquared(pawn.Position));
            foreach (Pawn otherPawn in pawns)
            {
                if (otherPawn.Position.DistanceTo(pawn.Position) > searchRadius) break;
                if (LoSRequired && !GenSight.LineOfSight(pawn.Position, otherPawn.Position, pawn.Map)) continue;
                if (onlyHostiles && otherPawn.HostileTo(pawn) || 
                    onlyInFaction && otherPawn.Faction == pawn.Faction || 
                    !onlyHostiles && !onlyInFaction) 
                    return otherPawn;
            }
            
            return null;
        }
        
        public static bool Valid(this Ability ability, LocalTargetInfo target)
        {
            if (ability == null) return true; // This shouldn't ever happen, and is acting more as crazy error catching

            if (target.Thing != null)
                if (!ability.verb.targetParams.CanTarget(target.Thing)) return false;
            if (!ability.comps.NullOrEmpty())
                foreach (var abilityEffect in ability.comps.Cast<CompAbilityEffect>())
                {
                    if (!abilityEffect.Valid(target)) return false;
                    if (target.Thing is Pawn pawn)
                    {
                        if (abilityEffect.Props.psychic && pawn.StatOrOne(StatDefOf.PsychicSensitivity) <= 0)
                            return false;

                        if (abilityEffect is CompAbilityEffect_WithDuration duration && duration.GetDurationSeconds(pawn) <= 0)
                            return false;

                        if (abilityEffect is CompAbilityEffect_BloodDrain blood && pawn.HasHediff(blood.Props.replacementHediff))
                            return false;

                        if (abilityEffect is CompAbilityEffect_Stun && pawn.stances.stunner.Stunned)
                            return false;
                    }
                }
            return true;
        }
        
        public static bool NeedToMove(Ability ability, Pawn pawn, Pawn targetPawn = null)
        {
            if (targetPawn.pather.Moving)
            {
                if (targetPawn.Position.DistanceTo(pawn.Position) > ability.verb.EffectiveRange)
                    return true;
            }
            else
            {
                if (targetPawn.Position.DistanceTo(pawn.Position) > Math.Ceiling(ability.verb.EffectiveRange / 2))
                    return true;
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
            return pawn.playerSettings != null && pawn.playerSettings.UsesConfigurableHostilityResponse && pawn.playerSettings.hostilityResponse == HostilityResponseMode.Attack;
        }
        
        // Private methods from the pregnancy utility

        private static List<string> tmpLastNames = new List<string>(2);

        public static string RandomLastName(Pawn geneticMother, Pawn father)
        {
            tmpLastNames.Clear();
            if (geneticMother != null)
                tmpLastNames.Add(PawnNamingUtility.GetLastName(geneticMother));

            if (father != null)
                tmpLastNames.Add(PawnNamingUtility.GetLastName(father));

            return tmpLastNames.Count == 0 ? null : tmpLastNames.RandomElement();
        }

        public static bool TryGetInheritedXenotype(Pawn mother, Pawn father, out XenotypeDef xenotype)
        {
            xenotype = null;
            if (mother?.genes?.Xenotype?.inheritable == true)
            {
                if (father?.genes == null || mother?.genes?.Xenotype == father?.genes?.Xenotype)
                    xenotype = mother?.genes?.Xenotype;
            }
            else if (mother?.genes == null && father?.genes?.Xenotype?.inheritable == true)
                xenotype = father.genes.Xenotype;
            return xenotype != null;
        }

        public static bool ShouldByHybrid(Pawn mother, Pawn father)
        {
            bool flag = mother?.genes != null;
            bool flag2 = father?.genes != null;
            if (flag && flag2)
            {
                if (mother.genes.hybrid && father.genes.hybrid)
                    return true;

                if (mother.genes.Xenotype.inheritable && father.genes.Xenotype.inheritable)
                    return true;

                bool num = mother.genes.Xenotype.inheritable || mother.genes.hybrid;
                bool flag3 = father.genes.Xenotype.inheritable || father.genes.hybrid;
                if (num || flag3)
                    return true;
            }
            return (flag && !flag2 && mother.genes.hybrid) || (flag2 && !flag && father.genes.hybrid);
        }
    }
}
