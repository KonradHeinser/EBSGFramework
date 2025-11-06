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
                if (!dictionary2.ContainsKey(phrase)) return false;
                if (dictionary1[phrase] != dictionary2[phrase]) return false;
                dictionary2.Remove(phrase);
            }
            if (!dictionary2.NullOrEmpty()) return false;
            return true;
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

            if (firstHediffOfDef != null && !onlyNew)
            {
                if (firstHediffOfDef is Hediff_Psylink psylink)
                    psylink.ChangeLevel(Mathf.CeilToInt(severityAdded), false);
                else if (firstHediffOfDef is Hediff_Level level)
                    level.ChangeLevel(Mathf.CeilToInt(severityAdded));
                else
                    firstHediffOfDef.Severity += severityAdded;
            }
            else if (initialSeverity > 0)
            {
                firstHediffOfDef = pawn.CreateComplexHediff(initialSeverity, hediffDef, other, bodyPart);
                pawn.health.AddHediff(firstHediffOfDef);
            }

            return firstHediffOfDef;
        }

        public static List<Hediff> GetHediffFromParts(this Pawn pawn, HediffDef hediff, List<BodyPartDef> bodyParts)
        {
            List<Hediff> hediffs = new List<Hediff>();
            if (hediff != null && pawn.health?.hediffSet?.hediffs.NullOrEmpty() == false)
            {
                
                Dictionary<BodyPartDef, int> partCounts = new Dictionary<BodyPartDef, int>();
                foreach (Hediff h in pawn.health.hediffSet.hediffs)
                {
                    if (hediff != h.def)
                        continue;

                    if (!bodyParts.NullOrEmpty())
                    {
                        if (h.Part == null)
                            continue;

                        if (!bodyParts.Contains(h.Part.def))
                            continue;

                        if (partCounts.ContainsKey(h.Part.def) &&
                            partCounts[h.Part.def] == bodyParts.Where(arg => arg == h.Part.def).Count())
                            continue;

                        if (partCounts.ContainsKey(h.Part.def))
                            partCounts[h.Part.def]++;
                        else
                            partCounts[h.Part.def] = 1;
                    }
                    hediffs.Add(h);
                }
            }
            return hediffs;
        }

        public static void GiveHediffs(this List<HediffToGive> hediffs, Pawn caster, Pawn target = null, int durationCaster = -1, int durationTarget = -1, bool psychic = false)
        {
            bool checkCaster = caster != null && (!psychic || caster.StatOrOne(StatDefOf.PsychicSensitivity) > 0);
            bool checkTarget = target != null && (!psychic || target.StatOrOne(StatDefOf.PsychicSensitivity) > 0);

            foreach (HediffToGive hediff in hediffs)
            {
                float severity = hediff.severity.RandomInRange;

                List<BodyPartDef> partChecks = new List<BodyPartDef>();
                if (!hediff.bodyParts.NullOrEmpty())
                    partChecks = new List<BodyPartDef>(hediff.bodyParts);
                else if (hediff.onlyBrain)
                    partChecks.Add(caster.health.hediffSet.GetBrain().def);

                if (checkCaster && (hediff.applyToSelf || hediff.onlyApplyToSelf) && 
                    (!hediff.psychic || caster.StatOrOne(StatDefOf.PsychicSensitivity) > 0))
                {
                    HandleHediffToGive(caster, target, hediff.hediffDef, severity,
                        hediff.replaceExisting, hediff.skipExisting, partChecks,
                        durationCaster);

                    if (!hediff.hediffDefs.NullOrEmpty())
                        foreach (HediffDef hd in hediff.hediffDefs)
                            HandleHediffToGive(caster, target, hd, severity,
                                hediff.replaceExisting, hediff.skipExisting, partChecks,
                                durationCaster);
                }
                if (checkTarget && hediff.applyToTarget && !hediff.onlyApplyToSelf &&
                    (!hediff.psychic || target.StatOrOne(StatDefOf.PsychicSensitivity) > 0))
                {
                    HandleHediffToGive(target, caster, hediff.hediffDef, severity,
                        hediff.replaceExisting, hediff.skipExisting, partChecks,
                        durationTarget);

                    if (!hediff.hediffDefs.NullOrEmpty())
                        foreach (HediffDef hd in hediff.hediffDefs)
                            HandleHediffToGive(target, caster, hd, severity, 
                                hediff.replaceExisting, hediff.skipExisting, partChecks, 
                                durationTarget);
                }
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
                    foreach (var t in comp.TraitsListForReading)
                        if (traits.Contains(t))
                            foundTraits.Add(t);
            }

            return foundTraits;
        }

        public static void HandleHediffToGive(Pawn p, Pawn o, HediffDef hd, float severity, bool replaceExisting, bool skipExisting, List<BodyPartDef> partChecks, int duration = -1)
        {
            if (hd == null) return;
            var bodyParts = new List<BodyPartDef>(partChecks);
            var foundHediffs = p.GetHediffFromParts(hd, bodyParts);
            bool flag = false;
            for (int i = 0; i < foundHediffs.Count; i++)
            {
                if (skipExisting)
                {
                    if (!bodyParts.NullOrEmpty())
                        bodyParts.Remove(foundHediffs[i].Part.def);
                    flag = true;
                }
                else if (replaceExisting)
                    p.health.RemoveHediff(foundHediffs[i]);
                else if (severity > 0)
                {
                    foundHediffs[i].Severity += severity;
                    if (!bodyParts.NullOrEmpty())
                        bodyParts.Remove(foundHediffs[i].Part?.def);
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
            // Clamps the initial value immediately so the change is carried for all new hediffs. Generaaly only relevant in cases where the initial can be changed by other things
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
            stage.fertilityFactor = newStage.fertilityFactor;
            stage.hungerRateFactor = newStage.hungerRateFactor;
            stage.hungerRateFactorOffset = newStage.hungerRateFactorOffset;
            stage.restFallFactor = newStage.restFallFactor;
            stage.restFallFactorOffset = newStage.restFallFactorOffset;
            stage.socialFightChanceFactor = newStage.socialFightChanceFactor;
            stage.foodPoisoningChanceFactor = newStage.foodPoisoningChanceFactor;
            stage.mentalBreakMtbDays = newStage.mentalBreakMtbDays;
            stage.mentalBreakExplanation = newStage.mentalBreakExplanation;
            stage.allowedMentalBreakIntensities = newStage.allowedMentalBreakIntensities;
            stage.makeImmuneTo = newStage.makeImmuneTo;
            stage.capMods = newStage.capMods;
            stage.hediffGivers = newStage.hediffGivers;
            stage.mentalStateGivers = newStage.mentalStateGivers;
            stage.statOffsets = newStage.statOffsets;
            stage.statFactors = newStage.statFactors;
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

        public static bool NeedToSatisfyIDG(this Pawn pawn, out List<Hediff_Dependency> dependencies)
        {
            dependencies = new List<Hediff_Dependency>();
            if (pawn.genes?.GenesListForReading.NullOrEmpty() != false)
                return false;

            foreach (Gene gene in pawn.genes.GenesListForReading)
                if (gene is Gene_Dependency d && d.LinkedHediff?.ShouldSatisfy == true)
                    dependencies.Add(d.LinkedHediff);

            return !dependencies.NullOrEmpty();
        }
        
        public static bool CheckGeneTrio(this Pawn pawn, List<GeneDef> oneOfGenes = null, List<GeneDef> allOfGenes = null, List<GeneDef> noneOfGenes = null)
        {
            if (pawn?.genes == null) return oneOfGenes.NullOrEmpty() && allOfGenes.NullOrEmpty();

            if (!oneOfGenes.NullOrEmpty() && !PawnHasAnyOfGenes(pawn, out _, oneOfGenes)) return false;
            if (!allOfGenes.NullOrEmpty() && !PawnHasAllOfGenes(pawn, allOfGenes)) return false;
            if (!noneOfGenes.NullOrEmpty() && PawnHasAnyOfGenes(pawn, out _, noneOfGenes)) return false;

            return true;
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

        public static IntVec3 FindDestination(Map targetMap, bool targetCenter = false)
        {
            IntVec3 target;
            if (targetCenter)
            {
                target = targetMap.Center;
                if (target.Standable(targetMap))
                    return target;
                
                target = CellFinder.StandableCellNear(target, targetMap, 50);
                if (target.IsValid) return target;
            }
            target = CellFinder.RandomEdgeCell(targetMap);
            if (target.Standable(targetMap)) return target;
            target = CellFinder.StandableCellNear(target, targetMap, 30);
            if (target.IsValid) return target;

            target = CellFinder.RandomEdgeCell(targetMap);
            target = CellFinder.StandableCellNear(target, targetMap, 30); // If the first time fails try a second time just to see if the first one was bad luck
            return target.IsValid ? target : IntVec3.Invalid;
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
            if (other == null)
                return pawn.HasHediff(hediff, out result);

            result = null;
            if (pawn?.health?.hediffSet == null || hediff == null) return false;

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

            foreach (var skillLevel in skillLevels)
                if (AllSkillLevelsMet(pawn, skillLevel, includeAptitudes))
                    return true;
            return false;
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
            {
                return val <= range.max;
            }
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

            if (ModsConfig.BiotechActive && pawn.genes?.GenesListForReading.NullOrEmpty() == false
                && !searchList.NullOrEmpty())
                foreach (var g in pawn.genes.GenesListForReading)
                    if (searchList.Contains(g.def))
                        results.Add(g.def);

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

        public static bool PawnHasAllOfGenes(this Pawn pawn, out GeneDef failOn, List<GeneDef> geneDefs = null, List<Gene> genes = null)
        {
            failOn = null;
            if (geneDefs.NullOrEmpty() && genes.NullOrEmpty()) return true;
            if (pawn.genes == null) return false;

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

            if (sendMessage && pawn.Faction.IsPlayer && (pawn.MapHeld != null || pawn.GetCaravan() != null))
                Messages.Message(message.TranslateOrFormat(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);

            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        public static void GainRandomGeneSet(this Pawn pawn, bool inheritGenes, bool removeGenesFromOtherLists,
                List<RandomXenoGenes> geneSets = null, List<GeneDef> alwaysAddedGenes = null, List<GeneDef> alwaysRemovedGenes = null, bool showMessage = true)
        {
            if (pawn.genes?.GenesListForReading.NullOrEmpty() != false) return;
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

        public static float StatOrOne(this Thing thing, StatDef statDef = null, StatRequirement statReq = StatRequirement.Always, int cacheDuration = 600)
        {
            if (statDef == null) return 1;
            var value = thing.GetStatValue(statDef, true, cacheDuration);
            switch (statReq)
            {
                case StatRequirement.Always:
                    return value;
                case StatRequirement.Lower:
                    if (value < statDef.defaultBaseValue)
                        return value;
                    return 1;
                case StatRequirement.Higher:
                    if (value > statDef.defaultBaseValue)
                        return value;
                    return 1;
                case StatRequirement.Pawn:
                    if (thing is Pawn)
                        return value;
                    return 1f;
                case StatRequirement.PawnLower:
                    if (thing is Pawn && value < statDef.defaultBaseValue)
                        return value;
                    return 1f;
                case StatRequirement.PawnHigher:
                    if (thing is Pawn && value > statDef.defaultBaseValue)
                        return value;
                    return 1f;
                case StatRequirement.NonPawn:
                    if (thing is Pawn)
                        return 1f;
                    return value;
                case StatRequirement.NonPawnLower:
                    if (thing is Pawn || value >= statDef.defaultBaseValue)
                        return 1f;
                    return value;
                case StatRequirement.NonPawnHigher:
                    if (thing is Pawn || value <= statDef.defaultBaseValue)
                        return 1f;
                    return value;
                case StatRequirement.Humanlike:
                    if (thing is Pawn p && p.RaceProps.Humanlike)
                        return value;
                    return 1f;
                case StatRequirement.HumanlikeLower:
                    if (thing is Pawn l && l.RaceProps.Humanlike && value < statDef.defaultBaseValue)
                        return value;
                    return 1f;
                case StatRequirement.HumanlikeHigher:
                    if (thing is Pawn h && h.RaceProps.Humanlike && value > statDef.defaultBaseValue)
                        return value;
                    return 1f;
            }
            return 1f;
        }

        public static float OutStatModifiedDamage(float damage, DamageModifyingStatsExtension extension, Thing victim, Thing attacker = null)
        {
            float offset = 0;
            
            if (attacker != null)
            {
                if (!extension.outgoingAttackerFactors.NullOrEmpty())
                    foreach (StatDef stat in extension.outgoingAttackerFactors)
                        damage *= attacker.StatOrOne(stat, extension.outAttackFactorReq);
                
                if (!extension.outgoingAttackerModifiers.NullOrEmpty())
                    foreach (StatModifier stat in extension.outgoingAttackerModifiers)
                        offset += attacker.StatOrOne(stat.stat) * stat.value;
                
                if (!extension.outgoingAttackerDivisors.NullOrEmpty())
                    foreach (StatDef stat in extension.outgoingAttackerDivisors)
                        damage /= attacker.StatOrOne(stat, extension.outAttackDivReq);
            }

            if (!extension.outgoingTargetFactors.NullOrEmpty())
                foreach (StatDef stat in extension.outgoingTargetFactors)
                    damage *= victim.StatOrOne(stat, extension.outTargetFactorReq);

            if (!extension.outgoingTargetModifiers.NullOrEmpty())
                foreach (StatModifier stat in extension.outgoingTargetModifiers)
                    offset += victim.StatOrOne(stat.stat) * stat.value;

            if (!extension.outgoingTargetDivisors.NullOrEmpty())
                foreach (StatDef stat in extension.outgoingTargetDivisors)
                    damage /= victim.StatOrOne(stat, extension.outTargetDivReq);
            
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
                    foreach (StatDef stat in extension.incomingAttackerFactors)
                        damage *= attacker.StatOrOne(stat, extension.inAttackFactorReq);

                if (!extension.incomingAttackerModifiers.NullOrEmpty())
                    foreach (StatModifier stat in extension.incomingAttackerModifiers)
                        offset += attacker.StatOrOne(stat.stat) * stat.value;

                if (!extension.incomingAttackerDivisors.NullOrEmpty())
                    foreach (StatDef stat in extension.incomingAttackerDivisors)
                        damage /= attacker.StatOrOne(stat, extension.inAttackDivReq);
            }

            if (!extension.incomingTargetFactors.NullOrEmpty())
                foreach (StatDef stat in extension.incomingTargetFactors)
                    damage *= victim.StatOrOne(stat, extension.inTargetFactorReq);

            if (!extension.incomingTargetModifiers.NullOrEmpty())
                foreach (StatModifier stat in extension.incomingTargetModifiers)
                    offset += victim.StatOrOne(stat.stat) * stat.value;

            if (!extension.incomingTargetDivisors.NullOrEmpty())
                foreach (StatDef stat in extension.incomingTargetDivisors)
                {
                    damage /= Mathf.Max(victim.StatOrOne(stat, extension.inTargetDivReq), 0.0001f);
                }

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

            foreach (GeneDef gene in relatedGenes)
                if (pawn.genes.HasActiveGene(gene)) 
                    return true;

            return false;
        }

        public static bool CheckPawnGenes(this Pawn pawn, GeneDef gene, List<GeneDef> genes, bool mustHaveAll = false)
        {
            if (!ModsConfig.BiotechActive || pawn.genes == null)
                return false;

            if (pawn.genes.HasActiveGene(gene))
                return true;

            if (!genes.NullOrEmpty())
                if (mustHaveAll)
                {
                    foreach (GeneDef g in genes)
                        if (!pawn.genes.HasActiveGene(g))
                            return false;
                    return true;
                }
                else
                    foreach (GeneDef g in genes)
                        if (pawn.genes.HasActiveGene(g))
                            return true;

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
                    for (int i = 0; i < wornApparel.Count; i++)
                        wornApparel[i].Notify_PawnResurrected(pawn);
                }
            }
            PawnDiedOrDownedThoughtsUtility.RemoveDiedThoughts(pawn);
            if (pawn.royalty != null)
                pawn.royalty.Notify_Resurrected();

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
                if (thing.Position.DistanceTo(pawn.Position) > searchRadius)
                {
                    if (autoSearch) return AutoSearchTarget(pawn, onlyHostiles, onlyInFaction, searchRadius, LoSRequired, ability);
                    return null;
                }
                if ((LoSRequired && !GenSight.LineOfSight(pawn.Position, thing.Position, pawn.Map)) || (onlyHostiles && !thing.HostileTo(pawn)))
                {
                    if (autoSearch) return AutoSearchTarget(pawn, onlyHostiles, onlyInFaction, searchRadius, LoSRequired, ability);
                    return null;
                }
                if (ability?.Valid(thing) == false)
                {
                    if (autoSearch) return AutoSearchTarget(pawn, onlyHostiles, onlyInFaction, searchRadius, LoSRequired, ability);
                    return null;
                }
                if (thing is Pawn otherPawn)
                {
                    if (otherPawn == pawn) return null;
                    if (!allowDowned && (otherPawn.Downed || otherPawn.Dead))
                    {
                        if (autoSearch) return AutoSearchTarget(pawn, onlyHostiles, onlyInFaction, searchRadius, LoSRequired, ability);
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
            if (autoSearch) return AutoSearchTarget(pawn, onlyHostiles, onlyInFaction, searchRadius, LoSRequired, ability);

            return null;
        }

        public static Pawn AutoSearchTarget(this Pawn pawn, bool onlyHostiles = true, bool onlyInFaction = false, float searchRadius = 50, bool LoSRequired = false, Ability ability = null)
        {

            List<Pawn> pawns = new List<Pawn>(pawn.Map.mapPawns.AllPawnsSpawned);
            pawns.SortBy((Pawn c) => c.Position.DistanceToSquared(pawn.Position));
            foreach (Pawn otherPawn in pawns)
            {
                if (otherPawn.Position.DistanceTo(pawn.Position) > searchRadius) break;
                if (otherPawn == pawn) continue;
                if (LoSRequired && !GenSight.LineOfSight(pawn.Position, otherPawn.Position, pawn.Map)) continue;
                if (otherPawn.DeadOrDowned || otherPawn.IsPrisonerOfColony ||
                    otherPawn.IsSlaveOfColony) continue;
                if (ability?.Valid(otherPawn) == false) continue;
                if (onlyHostiles && otherPawn.HostileTo(pawn)) return otherPawn;
                if (onlyInFaction && otherPawn.Faction == pawn.Faction) return otherPawn;
                if (!onlyHostiles && !onlyInFaction) return otherPawn;
            }
            
            return null;
        }
        
        public static bool Valid(this Ability ability, LocalTargetInfo target)
        {
            if (ability == null) return true; // This shouldn't ever happen, and is acting more as crazy error catching

            if (target.Thing != null)
                if (!ability.verb.targetParams.CanTarget(target.Thing)) return false;
            if (!ability.comps.NullOrEmpty())
                foreach (CompAbilityEffect abilityEffect in ability.comps)
                {
                    if (!abilityEffect.Valid(target)) return false;
                    if (target.Thing != null && target.Thing is Pawn pawn)
                    {
                        if (abilityEffect is CompAbilityEffect_GiveHediff giveComp &&
                            ((giveComp.Props.psychic && pawn.StatOrOne(StatDefOf.PsychicSensitivity) <= 0) ||
                            giveComp.Props.durationMultiplier != null && pawn.StatOrOne(giveComp.Props.durationMultiplier) <= 0))
                            return false;
                        if (abilityEffect is CompAbilityEffect_GiveMultipleHediffs giveMultiComp &&
                            ((giveMultiComp.Props.psychic && pawn.StatOrOne(StatDefOf.PsychicSensitivity) <= 0) ||
                            giveMultiComp.Props.durationMultiplier != null && pawn.StatOrOne(giveMultiComp.Props.durationMultiplier) <= 0))
                            return false;
                        if (abilityEffect is CompAbilityEffect_BloodDrain bloodComp &&
                            ((bloodComp.Props.psychic && pawn.StatOrOne(StatDefOf.PsychicSensitivity) <= 0) ||
                            (bloodComp.Props.replacementHediff != null && pawn.HasHediff(bloodComp.Props.replacementHediff))))
                            return false;
                        if (abilityEffect is CompAbilityEffect_Stun stunComp &&
                            ((stunComp.Props.psychic && pawn.StatOrOne(StatDefOf.PsychicSensitivity) <= 0) ||
                            (stunComp.Props.durationMultiplier != null && pawn.StatOrOne(stunComp.Props.durationMultiplier) <= 0) ||
                            (ability.lastCastTick >= 0 && ability.def.EffectDuration() > 0 &&
                            Find.TickManager.TicksGame - ability.lastCastTick < ability.def.EffectDuration())))
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
            if (pawn.playerSettings != null && pawn.playerSettings.UsesConfigurableHostilityResponse && pawn.playerSettings.hostilityResponse == HostilityResponseMode.Attack) return true;
            return false;
        }
    }
}
