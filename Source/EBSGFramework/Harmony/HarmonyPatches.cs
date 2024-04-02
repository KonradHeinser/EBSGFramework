using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        private static EBSGCache_Component cache;

        public static EBSGCache_Component Cache
        {
            get
            {
                if (cache == null)
                    cache = Current.Game.GetComponent<EBSGCache_Component>();

                if (cache != null && cache.loaded)
                    return cache;
                return null;
            }
        }

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("Rimworld.Alite.EBSG.main");

            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), nameof(EquipmentUtility.CanEquip), new[] { typeof(Thing), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool) }),
                postfix: new HarmonyMethod(patchType, nameof(CanEquipPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryLovinChanceFactor)),
                postfix: new HarmonyMethod(patchType, nameof(SecondaryLovinChanceFactorPostFix)));
            harmony.Patch(AccessTools.Method(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.RomanceFactors)),
                postfix: new HarmonyMethod(patchType, nameof(RomanceFactorsPostFix)));
            harmony.Patch(AccessTools.Method(typeof(Thought), nameof(Thought.MoodOffset)),
                postfix: new HarmonyMethod(patchType, nameof(GeneticThoughtMultiplier)));
            harmony.Patch(AccessTools.Method(typeof(Thought_SituationalSocial), nameof(Thought_SituationalSocial.OpinionOffset)),
                postfix: new HarmonyMethod(patchType, nameof(GeneticThoughtMultiplier)));
            harmony.Patch(AccessTools.Method(typeof(Thing), nameof(Thing.TakeDamage)),
                prefix: new HarmonyMethod(patchType, nameof(TakeDamagePrefix)));
            harmony.Patch(AccessTools.Method(typeof(Thing), nameof(Thing.TakeDamage)),
                postfix: new HarmonyMethod(patchType, nameof(TakeDamagePostfix)));
            harmony.Patch(AccessTools.Method(typeof(ThingMaker), nameof(ThingMaker.MakeThing)),
                postfix: new HarmonyMethod(patchType, nameof(MakeThingPostfix)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(ThingFilter), nameof(ThingFilter.AllowedThingDefs)),
                postfix: new HarmonyMethod(patchType, nameof(AllowedThingDefsPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_PathFollower), "CostToMoveIntoCell", new[] { typeof(Pawn), typeof(IntVec3) }),
                postfix: new HarmonyMethod(patchType, nameof(CostToMoveIntoCellPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn), "DoKillSideEffects"),
                postfix: new HarmonyMethod(patchType, nameof(DoKillSideEffectsPostfix)));
            harmony.Patch(AccessTools.Method(typeof(ForbidUtility), nameof(ForbidUtility.IsForbidden), new[] { typeof(Thing), typeof(Pawn) }),
                postfix: new HarmonyMethod(patchType, nameof(IsForbiddenPostfix)));

            // Needs Harmony patches
            harmony.Patch(AccessTools.Method(typeof(Need_Seeker), nameof(Need_Seeker.NeedInterval)),
                postfix: new HarmonyMethod(patchType, nameof(SeekerNeedMultiplier)));
            if (ModsConfig.BiotechActive)
            {
                harmony.Patch(AccessTools.Method(typeof(Need_KillThirst), nameof(Need_KillThirst.NeedInterval)),
                    postfix: new HarmonyMethod(patchType, nameof(KillThirstPostfix)));
            }
            harmony.Patch(AccessTools.Method(typeof(Need_Joy), nameof(Need_Joy.GainJoy)),
                postfix: new HarmonyMethod(patchType, nameof(GainJoyPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Need_Mood), nameof(Need_Mood.NeedInterval)),
                postfix: new HarmonyMethod(patchType, nameof(SeekerNeedMultiplier)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.GrowthPointsPerDay)),
                postfix: new HarmonyMethod(patchType, nameof(GrowthPointStatPostfix)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(Pawn_PsychicEntropyTracker), "PsyfocusFallPerDay"),
                postfix: new HarmonyMethod(patchType, nameof(PsyfocusFallPerDayPostFix)));

            // Stat Harmony patches
            harmony.Patch(AccessTools.PropertyGetter(typeof(Gene_Deathrest), nameof(Gene_Deathrest.MinDeathrestTicks)),
                postfix: new HarmonyMethod(patchType, nameof(DeathrestEfficiencyPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Need_Deathrest), nameof(Need_Deathrest.NeedInterval)),
                postfix: new HarmonyMethod(patchType, nameof(DeathrestNeedIntervalPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.BodyResourceGrowthSpeed)),
                postfix: new HarmonyMethod(patchType, nameof(BodyResourceGrowthSpeedPostfix)));
            harmony.Patch(AccessTools.Method(typeof(HediffGiver_Bleeding), nameof(HediffGiver_Bleeding.OnIntervalPassed)),
                postfix: new HarmonyMethod(patchType, nameof(BloodRecoveryPostfix)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.HealthScale)),
                postfix: new HarmonyMethod(patchType, nameof(PawnHealthinessPostfix)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(DamageInfo), nameof(DamageInfo.Amount)),
                postfix: new HarmonyMethod(patchType, nameof(DamageAmountPostfix)));

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void CanEquipPostfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason)
        {
            if (!__result) return;
            EquipRestrictExtension extension = thing.def.GetModExtension<EquipRestrictExtension>();
            bool flag = true;
            if (extension != null)
            {       // Attempt to get the various limiting lists
                List<GeneDef> requiredGenesToEquip = extension.requiredGenesToEquip;
                List<GeneDef> requireOneOfGenesToEquip = extension.requireOneOfGenesToEquip;
                List<GeneDef> forbiddenGenesToEquip = extension.forbiddenGenesToEquip;
                List<XenotypeDef> requireOneOfXenotypeToEquip = extension.requireOneOfXenotypeToEquip;
                List<XenotypeDef> forbiddenXenotypesToEquip = extension.forbiddenXenotypesToEquip;
                List<HediffDef> requiredHediffsToEquip = extension.requiredHediffsToEquip;
                List<HediffDef> requireOneOfHediffsToEquip = extension.requireOneOfHediffsToEquip;
                List<HediffDef> forbiddenHediffsToEquip = extension.forbiddenHediffsToEquip;
                // Gene Check
                if (!pawn.genes.GenesListForReading.NullOrEmpty())
                {
                    Pawn_GeneTracker currentGenes = pawn.genes;
                    if (!requiredGenesToEquip.NullOrEmpty() || !requireOneOfGenesToEquip.NullOrEmpty() || !forbiddenGenesToEquip.NullOrEmpty() ||
                        !requireOneOfXenotypeToEquip.NullOrEmpty() || !forbiddenXenotypesToEquip.NullOrEmpty())
                    {
                        if (!requireOneOfXenotypeToEquip.NullOrEmpty() && !requireOneOfXenotypeToEquip.Contains(pawn.genes.Xenotype) && flag)
                        {
                            if (requiredGenesToEquip.Count > 1) cantReason = "EBSG_XenoRestrictedEquipment_AnyOne".Translate();
                            else cantReason = "EBSG_XenoRestrictedEquipment_One".Translate(pawn.genes.Xenotype.LabelCap);
                            flag = false;
                        }
                        if (!forbiddenXenotypesToEquip.NullOrEmpty() && forbiddenXenotypesToEquip.Contains(pawn.genes.Xenotype) && flag)
                        {
                            cantReason = "EBSG_XenoRestrictedEquipment_None".Translate(pawn.genes.Xenotype.LabelCap);
                            flag = false;
                        }
                        if (!forbiddenGenesToEquip.NullOrEmpty() && flag)
                        {
                            foreach (Gene gene in currentGenes.GenesListForReading)
                            {
                                if (forbiddenGenesToEquip.Contains(gene.def))
                                {
                                    cantReason = "EBSG_GeneRestrictedEquipment_None".Translate(gene.LabelCap);
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (!requiredGenesToEquip.NullOrEmpty() && flag)
                        {
                            foreach (Gene gene in currentGenes.GenesListForReading)
                                if (requiredGenesToEquip.Contains(gene.def)) requiredGenesToEquip.Remove(gene.def);
                            if (!requiredGenesToEquip.NullOrEmpty())
                            {
                                if (extension.requiredGenesToEquip.Count > 1) cantReason = "EBSG_GeneRestrictedEquipment_All".Translate();
                                else cantReason = "EBSG_GeneRestrictedEquipment_One".Translate(extension.requiredGenesToEquip[0]);
                                flag = false;
                            }
                        }
                        if (!requireOneOfGenesToEquip.NullOrEmpty() && flag)
                        {
                            flag = false;
                            if (requireOneOfGenesToEquip.Count > 1) cantReason = "EBSG_GeneRestrictedEquipment_AnyOne".Translate();
                            else cantReason = "EBSG_GeneRestrictedEquipment_One".Translate(requireOneOfGenesToEquip[0]);
                            foreach (Gene gene in currentGenes.GenesListForReading)
                                if (requiredGenesToEquip.Contains(gene.def))
                                {
                                    flag = true;
                                    cantReason = null;
                                    break;
                                }
                        }
                    }
                }
                else
                {
                    if (!requiredGenesToEquip.NullOrEmpty() || !requireOneOfGenesToEquip.NullOrEmpty() || !requireOneOfXenotypeToEquip.NullOrEmpty())
                    {
                        cantReason = "EBSG_GenesNotFound".Translate();
                        flag = false;
                    }
                }

                // Hediff Check
                HediffSet hediffSet = pawn.health.hediffSet;
                if (flag && !hediffSet.hediffs.NullOrEmpty())
                {
                    if (!requiredHediffsToEquip.NullOrEmpty() || !requireOneOfHediffsToEquip.NullOrEmpty() || !forbiddenHediffsToEquip.NullOrEmpty())
                    {
                        if (!forbiddenHediffsToEquip.NullOrEmpty())
                            foreach (HediffDef hediffDef in forbiddenHediffsToEquip)
                            {
                                if (hediffSet.HasHediff(hediffDef))
                                {
                                    cantReason = "EBSG_HediffRestrictedEquipment_None".Translate(hediffDef.LabelCap);
                                    flag = false;
                                    break;
                                }
                            }

                        if (flag && !requireOneOfHediffsToEquip.NullOrEmpty())
                        {
                            flag = false;
                            foreach (HediffDef hediffDef in requireOneOfHediffsToEquip)
                            {
                                if (hediffSet.HasHediff(hediffDef))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                                if (requireOneOfHediffsToEquip.Count > 1) cantReason = "EBSG_HediffRestrictedEquipment_AnyOne".Translate();
                                else cantReason = "EBSG_HediffRestrictedEquipment_One".Translate(requireOneOfHediffsToEquip[0]);
                        }

                        if (flag && !requiredHediffsToEquip.NullOrEmpty())
                        {
                            foreach (Hediff hediff in hediffSet.hediffs)
                            {
                                if (requiredHediffsToEquip.Contains(hediff.def)) requiredHediffsToEquip.Remove(hediff.def);
                            }
                            if (!requiredHediffsToEquip.NullOrEmpty())
                            {
                                if (extension.requiredHediffsToEquip.Count > 1) cantReason = "EBSG_HediffRestrictedEquipment_All".Translate();
                                else cantReason = "EBSG_HediffRestrictedEquipment_One".Translate(requiredHediffsToEquip[0]);
                                flag = false;
                            }
                        }

                    }
                }
            }
            if (flag && pawn.genes != null && !pawn.genes.GenesListForReading.NullOrEmpty())
            {
                XenotypeDef xenotype = pawn.genes.Xenotype;
                if (xenotype != null && xenotype.HasModExtension<EquipRestrictExtension>())
                {
                    extension = xenotype.GetModExtension<EquipRestrictExtension>();
                    if (extension.noEquipment || (!extension.limitedToEquipments.NullOrEmpty() && !extension.limitedToEquipments.Contains(thing.def)))
                    {
                        cantReason = "EBSG_LimitedList".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                    if (thing.def.IsWeapon && (extension.noWeapons || (!extension.limitedToWeapons.NullOrEmpty() && !extension.limitedToWeapons.Contains(thing.def))
                        || (extension.onlyMelee && !thing.def.IsMeleeWeapon) || (extension.onlyRanged && thing.def.IsRangedWeapon)))
                    {
                        cantReason = "EBSG_LimitedList".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                    if (thing.def.IsApparel && (extension.noApparel || (!extension.limitedToApparels.NullOrEmpty() && !extension.limitedToApparels.Contains(thing.def))))
                    {
                        cantReason = "EBSG_LimitedList".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                    if (!extension.forbiddenEquipments.NullOrEmpty() && extension.forbiddenEquipments.Contains(thing.def))
                    {
                        cantReason = "EBSG_ForbiddenList".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                }
                if (flag)
                    foreach (Gene gene in pawn.genes.GenesListForReading)
                    {
                        if (!gene.def.HasModExtension<EquipRestrictExtension>()) continue;
                        extension = gene.def.GetModExtension<EquipRestrictExtension>();
                        if (extension.noEquipment || (!extension.limitedToEquipments.NullOrEmpty() && !extension.limitedToEquipments.Contains(thing.def)))
                        {
                            cantReason = "EBSG_LimitedList".Translate(gene.LabelCap);
                            flag = false;
                            break;
                        }
                        if (thing.def.IsWeapon && (extension.noWeapons || (!extension.limitedToWeapons.NullOrEmpty() && !extension.limitedToWeapons.Contains(thing.def))
                            || (extension.onlyMelee && !thing.def.IsMeleeWeapon) || (extension.onlyRanged && thing.def.IsRangedWeapon)))
                        {
                            cantReason = "EBSG_LimitedList".Translate(gene.LabelCap);
                            flag = false;
                            break;
                        }
                        if (thing.def.IsApparel && (extension.noApparel || (!extension.limitedToApparels.NullOrEmpty() && !extension.limitedToApparels.Contains(thing.def))))
                        {
                            cantReason = "EBSG_LimitedList".Translate(gene.LabelCap);
                            flag = false;
                            break;
                        }
                        if (!extension.forbiddenEquipments.NullOrEmpty() && extension.forbiddenEquipments.Contains(thing.def))
                        {
                            cantReason = "EBSG_ForbiddenList".Translate(gene.LabelCap);
                            flag = false;
                            break;
                        }
                    }
            }
            __result = flag;
        }

        public static void SecondaryLovinChanceFactorPostFix(ref float __result, Pawn otherPawn, ref Pawn ___pawn)
        {
            if (ModsConfig.BiotechActive && otherPawn.genes != null)
            {
                List<Gene> genesListForReading = otherPawn.genes.GenesListForReading;

                foreach (Gene gene in genesListForReading)
                    if (gene.def.HasModExtension<GRCExtension>())
                    {
                        float num = 1f;
                        GRCExtension extension = gene.def.GetModExtension<GRCExtension>();
                        if (extension.carrierStat != null)
                        {
                            float statValue = otherPawn.GetStatValue(extension.carrierStat);
                            if (extension.onlyWhileLoweredCarrier && statValue < 1) num *= statValue;
                            else if (extension.onlyWhileRaisedCarrier && statValue > 1) num *= statValue;
                            else if (!extension.onlyWhileLoweredCarrier && !extension.onlyWhileRaisedCarrier) num *= statValue;
                        }
                        if (extension.otherStat != null)
                        {
                            float statValue = ___pawn.GetStatValue(extension.otherStat);
                            if (extension.onlyWhileLoweredOther && statValue < 1) num *= statValue;
                            else if (extension.onlyWhileRaisedOther && statValue > 1) num *= statValue;
                            else if (!extension.onlyWhileLoweredOther && !extension.onlyWhileRaisedOther) num *= statValue;
                        }
                        __result *= num;
                    }
            }
        }

        public static void RomanceFactorsPostFix(ref string __result, Pawn romancer, Pawn romanceTarget)
        {
            if (ModsConfig.BiotechActive && romancer.genes != null)
            {
                List<Gene> genesListForReading = romancer.genes.GenesListForReading;
                float num = 1f;
                bool flag = false;

                foreach (Gene gene in genesListForReading)
                    if (gene.def.HasModExtension<GRCExtension>())
                    {
                        GRCExtension extension = gene.def.GetModExtension<GRCExtension>();
                        if (extension.carrierStat != null)
                        {
                            float statValue = romancer.GetStatValue(extension.carrierStat);
                            if (extension.onlyWhileLoweredCarrier && statValue < 1) num *= statValue;
                            else if (extension.onlyWhileRaisedCarrier && statValue > 1) num *= statValue;
                            else if (!extension.onlyWhileLoweredCarrier && !extension.onlyWhileRaisedCarrier) num *= statValue;
                        }
                        if (extension.otherStat != null)
                        {
                            float statValue = romanceTarget.GetStatValue(extension.otherStat);
                            if (extension.onlyWhileLoweredOther && statValue < 1) num *= statValue;
                            else if (extension.onlyWhileRaisedOther && statValue > 1) num *= statValue;
                            else if (!extension.onlyWhileLoweredOther && !extension.onlyWhileRaisedOther) num *= statValue;
                        }
                        flag = true;
                    }

                if (flag)
                {
                    StringBuilder stringBuilder = new StringBuilder(__result);
                    stringBuilder.AppendLine(" - " + "EBSG_GeneticRomanceChance".Translate() + ": x" + num.ToStringPercent());
                    __result = stringBuilder.ToString();
                }
            }
        }

        public static void GeneticThoughtMultiplier(Pawn ___pawn, ref float __result, ThoughtDef ___def)
        {
            if (___def.HasModExtension<EBSGExtension>() && ___pawn.genes != null)
            {
                EBSGExtension extension = ___def.GetModExtension<EBSGExtension>();
                bool ensureReverse = false;
                bool positiveValue = __result > 0;

                if (!extension.geneticMultipliers.NullOrEmpty())
                    foreach (GeneticMultiplier geneticMultiplier in extension.geneticMultipliers)
                        if (___pawn.genes.HasGene(geneticMultiplier.gene) && geneticMultiplier.multiplier != 0 && !EBSGUtilities.PawnHasAnyOfGenes(___pawn, geneticMultiplier.nullifyingGenes))
                        {
                            __result *= geneticMultiplier.multiplier;
                            ensureReverse |= geneticMultiplier.multiplier < 0;
                        }

                if (ensureReverse && positiveValue == __result > 0) __result *= -1;
            }

            if (__result != 0 && Cache != null) __result *= Cache.GetGeneMoodFactor(___pawn);
        }

        public static void MakeThingPostfix(ref ThingDef def, ref ThingDef stuff, ref Thing __result)
        {
            if (stuff != null && __result is ThingWithComps compThing && !stuff.comps.NullOrEmpty() && stuff.HasComp(typeof(CompRegenerating)))
            {
                CompRegenerating compRegenerating = (CompRegenerating)Activator.CreateInstance(typeof(CompRegenerating));
                compRegenerating.parent = compThing;
                compThing.AllComps.Add(compRegenerating);
                compRegenerating.Initialize(stuff.comps.First((CompProperties c) => c.GetType() == typeof(CompProperties_Regenerating)));
            }
        }

        public static void AllowedThingDefsPostfix(ref IEnumerable<ThingDef> __result)
        {
            List<ThingDef> invalidThings = new List<ThingDef>();
            foreach (ThingDef th in __result.Where((ThingDef t) => !t.comps.NullOrEmpty()))
                foreach (CompProperties comp in th.comps)
                    if (comp is CompProperties_Indestructible)
                    {
                        invalidThings.Add(th);
                        break;
                    }
            if (!invalidThings.NullOrEmpty())
                __result = __result.Where((ThingDef t) => !invalidThings.Contains(t));
        }

        public static void CostToMoveIntoCellPostfix(Pawn pawn, IntVec3 c, ref float __result)
        {
            if (pawn.Map != null)
            {
                if (__result == 10000 && !pawn.Map.thingGrid.ThingsListAt(c).NullOrEmpty()) return; // If impassable due to a thing, it's probably a wall
                if (pawn.Map != null && Cache != null && Cache.GetPawnTerrainComp(pawn, out HediffCompProperties_TerrainCostOverride terrainComp))
                {
                    if (c.Impassable(pawn.Map)) return; // If the tile is impassable, I don't want to touch that.

                    // Universal Checks
                    if (!EBSGUtilities.CheckGeneTrio(pawn, terrainComp.pawnHasAnyOfGenes, terrainComp.pawnHasAllOfGenes, terrainComp.pawnHasNoneOfGenes) ||
                        !EBSGUtilities.CheckHediffTrio(pawn, terrainComp.pawnHasAnyOfHediffs, terrainComp.pawnHasAllOfHediffs, terrainComp.pawnHasNoneOfHediffs) ||
                        !EBSGUtilities.CheckPawnCapabilitiesTrio(pawn, terrainComp.pawnCapLimiters, terrainComp.pawnSkillLimiters, terrainComp.pawnStatLimiters) ||
                        !EBSGUtilities.AllNeedLevelsMet(pawn, terrainComp.pawnNeedLevels)) return;

                    float num = (c.x != pawn.Position.x && c.z != pawn.Position.z) ? pawn.TicksPerMoveDiagonal : pawn.TicksPerMoveCardinal;
                    TerrainDef terrainDef = pawn.Map.terrainGrid.TerrainAt(c);

                    if (!terrainComp.terrainSets.NullOrEmpty() && terrainDef != null)
                        foreach (TerrainLinker terrain in terrainComp.terrainSets)
                        {
                            // These check all 10 lists
                            if (!EBSGUtilities.CheckGeneTrio(pawn, terrain.pawnHasAnyOfGenes, terrain.pawnHasAllOfGenes, terrain.pawnHasNoneOfGenes) ||
                                !EBSGUtilities.CheckHediffTrio(pawn, terrain.pawnHasAnyOfHediffs, terrain.pawnHasAllOfHediffs, terrain.pawnHasNoneOfHediffs) ||
                                !EBSGUtilities.CheckPawnCapabilitiesTrio(pawn, terrain.pawnCapLimiters, terrain.pawnSkillLimiters, terrain.pawnStatLimiters) ||
                                !EBSGUtilities.AllNeedLevelsMet(pawn, terrain.pawnNeedLevels)) continue;

                            if (terrain.newCost >= 0 && ((terrain.terrain != null && terrain.terrain == terrainDef) ||
                                (!terrain.terrains.NullOrEmpty() && terrain.terrains.Contains(terrainDef))))
                            {
                                __result = num + terrain.newCost;
                                return;
                            }
                        }
                    if (terrainComp.universalCostOverride >= 0) __result = num + terrainComp.universalCostOverride;
                }
            }
        }

        public static void DoKillSideEffectsPostfix(DamageInfo? dinfo, Hediff exactCulprit, bool spawned, Pawn __instance)
        {
            if (dinfo?.Instigator != null && dinfo.Value.Instigator is Pawn pawn)
            {
                if (pawn.needs != null && !pawn.needs.AllNeeds.NullOrEmpty())
                {
                    foreach (Need need in pawn.needs.AllNeeds)
                    {
                        if (need is Need_Murderous murderNeed)
                            murderNeed.Notify_KilledPawn(dinfo, __instance);
                    }
                }
            }
        }

        public static void IsForbiddenPostfix(Thing t, Pawn pawn, ref bool __result)
        {
            if (!__result && (t.Faction == null || pawn.Faction == null || t.Faction != pawn.Faction) && t is Corpse corpse)
            {
                MultipleLives_Component multipleLives = Current.Game.GetComponent<MultipleLives_Component>();
                if (multipleLives != null && multipleLives.loaded && !multipleLives.forbiddenCorpses.NullOrEmpty())
                    __result = multipleLives.forbiddenCorpses.Contains(corpse);
            }
        }

        // Harmony patches for stats

        public static void DeathrestEfficiencyPostfix(ref int __result, Pawn ___pawn)
        {
            if (___pawn != null)
                __result = (int)Math.Round(__result / ___pawn.GetStatValue(EBSGDefOf.EBSG_DeathrestEfficiency), 0);
        }

        public static void DeathrestNeedIntervalPostfix(ref Need_Deathrest __instance, Pawn ___pawn)
        {
            if (!__instance.Deathresting)
                __instance.CurLevel += -1f / 30f / 400f * (___pawn.GetStatValue(EBSGDefOf.EBSG_DeathrestFallRate) - 1);
        }

        public static void GrowthPointStatPostfix(ref float __result, Pawn ___pawn)
        {
            if (___pawn != null)
                __result *= ___pawn.GetStatValue(EBSGDefOf.EBSG_GrowthPointRate);
        }

        public static void KillThirstPostfix(Pawn ___pawn)
        {
            if (___pawn != null && ModsConfig.BiotechActive)
            {
                Need killThirst = ___pawn.needs.TryGetNeed<Need_KillThirst>();
                if (killThirst != null)
                    ___pawn.needs.TryGetNeed<Need_KillThirst>().CurLevel -= 8.333333E-05f * (___pawn.GetStatValue(EBSGDefOf.EBSG_KillThirstRate) - 1);
            }
        }

        public static void GainJoyPostfix(float amount, JoyKindDef joyKind, Pawn ___pawn, JoyToleranceSet ___tolerances)
        {
            if (!(amount <= 0f))
            {
                amount *= ___tolerances.JoyFactorFromTolerance(joyKind) * (___pawn.GetStatValue(EBSGDefOf.EBSG_JoyRiseRate) - 1);
                amount = Mathf.Min(amount, 1f - ___pawn.needs.joy.CurLevel);
                ___pawn.needs.joy.CurLevel += amount;
            }
        }

        public static void SeekerNeedMultiplier(NeedDef ___def, Need __instance, Pawn ___pawn)
        {
            float increase = ___def.seekerRisePerHour * 0.06f;
            float decrease = ___def.seekerFallPerHour * 0.06f;
            float curInstantLevel;
            if (EBSGUtilities.NeedFrozen(___pawn, ___def)) return;
            switch (___def.ToString())
            {
                case "Beauty":
                    curInstantLevel = ___pawn.needs.beauty.CurInstantLevel;
                    if (curInstantLevel > ___pawn.needs.beauty.CurLevel)
                    {
                        ___pawn.needs.beauty.CurLevel += increase * (___pawn.GetStatValue(EBSGDefOf.EBSG_BeautyRiseRate) - 1);
                        ___pawn.needs.beauty.CurLevel = Mathf.Min(___pawn.needs.beauty.CurLevel, curInstantLevel);
                    }
                    if (curInstantLevel < ___pawn.needs.beauty.CurLevel)
                    {
                        ___pawn.needs.beauty.CurLevel -= decrease * (___pawn.GetStatValue(EBSGDefOf.EBSG_BeautyFallRate) - 1);
                        ___pawn.needs.beauty.CurLevel = Mathf.Max(___pawn.needs.beauty.CurLevel, curInstantLevel);
                    }
                    break;
                case "Comfort":
                    curInstantLevel = ___pawn.needs.comfort.CurInstantLevel;
                    if (curInstantLevel > ___pawn.needs.comfort.CurLevel)
                    {
                        ___pawn.needs.comfort.CurLevel += increase * (___pawn.GetStatValue(EBSGDefOf.EBSG_ComfortRiseRate) - 1);
                        ___pawn.needs.comfort.CurLevel = Mathf.Min(___pawn.needs.comfort.CurLevel, curInstantLevel);
                    }
                    if (curInstantLevel < ___pawn.needs.comfort.CurLevel)
                    {
                        ___pawn.needs.comfort.CurLevel -= decrease * (___pawn.GetStatValue(EBSGDefOf.EBSG_ComfortFallRate) - 1);
                        ___pawn.needs.comfort.CurLevel = Mathf.Max(___pawn.needs.comfort.CurLevel, curInstantLevel);
                    }
                    break;
                case "Mood":
                    curInstantLevel = ___pawn.needs.mood.CurInstantLevel;
                    if (curInstantLevel > ___pawn.needs.mood.CurLevel)
                    {
                        ___pawn.needs.mood.CurLevel += increase * (___pawn.GetStatValue(EBSGDefOf.EBSG_MoodRiseRate) - 1);
                        ___pawn.needs.mood.CurLevel = Mathf.Min(___pawn.needs.mood.CurLevel, curInstantLevel);
                    }
                    if (curInstantLevel < ___pawn.needs.mood.CurLevel)
                    {
                        ___pawn.needs.mood.CurLevel -= decrease * (___pawn.GetStatValue(EBSGDefOf.EBSG_MoodFallRate) - 1);
                        ___pawn.needs.mood.CurLevel = Mathf.Max(___pawn.needs.mood.CurLevel, curInstantLevel);
                    }
                    break;
            }
        }

        public static void BodyResourceGrowthSpeedPostfix(ref float __result, Pawn pawn)
        {
            if (pawn != null)
                __result *= pawn.GetStatValue(EBSGDefOf.EBSG_PawnGestationSpeed);
        }

        public static void PsyfocusFallPerDayPostFix(ref float __result, Pawn ___pawn)
        {

            if (___pawn != null && ___pawn.GetPsylinkLevel() > 0)
                __result = (__result + ___pawn.GetStatValue(EBSGDefOf.EBSG_PsyfocusFallRateOffset)) * ___pawn.GetStatValue(EBSGDefOf.EBSG_PsyfocusFallRateFactor);
        }

        public static void BloodRecoveryPostfix(Pawn pawn, Hediff cause)
        {
            HediffSet hediffSet = pawn.health.hediffSet;
            if (hediffSet.BleedRateTotal < 0.1f)
                HealthUtility.AdjustSeverity(pawn, HediffDefOf.BloodLoss, (-0.00033333333f * pawn.GetStatValue(EBSGDefOf.EBSG_BloodlossRecoveryBonus)));
        }

        public static void PawnHealthinessPostfix(Pawn __instance, ref float __result)
        {
            if (__instance != null)
                __result *= __instance.GetStatValue(EBSGDefOf.EBSG_Healthiness);
        }

        public static bool HasSpecialExplosion(Pawn pawn)
        {
            if (!pawn.Dead && pawn.health != null && !pawn.health.hediffSet.hediffs.NullOrEmpty())
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    HediffComp_ExplodingAttacks explodingComp = hediff.TryGetComp<HediffComp_ExplodingAttacks>();
                    if (explodingComp != null && hediff.Severity >= explodingComp.Props.minSeverity && hediff.Severity <= explodingComp.Props.maxSeverity) return true;
                    HediffComp_ExplodingRangedAttacks rangedExplodingComp = hediff.TryGetComp<HediffComp_ExplodingRangedAttacks>();
                    if (rangedExplodingComp != null && hediff.Severity >= rangedExplodingComp.Props.minSeverity && hediff.Severity <= rangedExplodingComp.Props.maxSeverity) return true;
                    HediffComp_ExplodingMeleeAttacks meleeExplodingComp = hediff.TryGetComp<HediffComp_ExplodingMeleeAttacks>();
                    if (meleeExplodingComp != null && hediff.Severity >= meleeExplodingComp.Props.minSeverity && hediff.Severity <= meleeExplodingComp.Props.maxSeverity) return true;
                }

            return false;
        }

        public static bool DoingSpecialExplosion(Pawn pawn, DamageInfo dinfo, Thing mainTarget)
        {
            if (pawn.health.hediffSet != null)
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    HediffComp_ExplodingAttacks explodingComp = hediff.TryGetComp<HediffComp_ExplodingAttacks>();
                    if (explodingComp != null && dinfo.Def == explodingComp.Props.damageDef && explodingComp.currentlyExploding) return true;

                    HediffComp_ExplodingRangedAttacks rangedExplodingComp = hediff.TryGetComp<HediffComp_ExplodingRangedAttacks>();
                    if (rangedExplodingComp != null && dinfo.Def == rangedExplodingComp.Props.damageDef && rangedExplodingComp.currentlyExploding) return true;

                    HediffComp_ExplodingMeleeAttacks meleeExplodingComp = hediff.TryGetComp<HediffComp_ExplodingMeleeAttacks>();
                    if (meleeExplodingComp != null && dinfo.Def == meleeExplodingComp.Props.damageDef && meleeExplodingComp.currentlyExploding) return true;
                }
            return false;
        }

        public static bool TakeDamagePrefix(ref DamageInfo dinfo, Thing __instance, DamageWorker.DamageResult __result)
        {
            if (__instance is Corpse corpse && corpse.InnerPawn != null && EBSGUtilities.PawnHasAnyHediff(corpse))
            {
                foreach (Hediff hediff in corpse.InnerPawn.health.hediffSet.hediffs)
                {
                    HediffComp_MultipleLives comp = hediff.TryGetComp<HediffComp_MultipleLives>();
                    if (comp != null && comp.pawnReviving && comp.Props.indestructibleWhileResurrecting)
                    {
                        dinfo.SetAmount(0);
                    }
                }
            }
            return true;
        }

        public static void TakeDamagePostfix(ref DamageInfo dinfo, Thing __instance, DamageWorker.DamageResult __result)
        {
            if (dinfo.Instigator != null && dinfo.Instigator is Pawn pawn && HasSpecialExplosion(pawn) && !DoingSpecialExplosion(pawn, dinfo, __instance) &&
                EBSGUtilities.GetCurrentTarget(pawn, false) == __instance && !EBSGUtilities.CastingAbility(pawn))
            {
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff.def.comps.NullOrEmpty()) continue;
                    HediffComp_ExplodingAttacks explodingComp = hediff.TryGetComp<HediffComp_ExplodingAttacks>();
                    if (explodingComp != null && hediff.Severity >= explodingComp.Props.minSeverity && hediff.Severity <= explodingComp.Props.maxSeverity)
                    {
                        explodingComp.currentlyExploding = true;
                        explodingComp.DoExplosion(__instance.Position);
                    }
                    if (dinfo.Def == null) continue; // Special catch

                    if (dinfo.Def.isRanged)
                    {
                        HediffComp_ExplodingRangedAttacks rangedExplodingComp = hediff.TryGetComp<HediffComp_ExplodingRangedAttacks>();
                        if (rangedExplodingComp != null && hediff.Severity >= rangedExplodingComp.Props.minSeverity && hediff.Severity <= rangedExplodingComp.Props.maxSeverity)
                        {
                            rangedExplodingComp.currentlyExploding = true;
                            rangedExplodingComp.DoExplosion(__instance.Position);
                        }
                    }
                    else if (!dinfo.Def.isExplosive)
                    {
                        HediffComp_ExplodingMeleeAttacks meleeExplodingComp = hediff.TryGetComp<HediffComp_ExplodingMeleeAttacks>();
                        if (meleeExplodingComp != null && hediff.Severity >= meleeExplodingComp.Props.minSeverity && hediff.Severity <= meleeExplodingComp.Props.maxSeverity)
                        {
                            meleeExplodingComp.currentlyExploding = true;
                            meleeExplodingComp.DoExplosion(__instance.Position);
                        }
                    }
                }
            }
        }

        public static void DamageAmountPostfix(ref float __result, DamageInfo __instance)
        {
            if (__instance.Instigator != null && __instance.Instigator is Pawn pawn)
                __result *= pawn.GetStatValue(EBSGDefOf.EBSG_OutgoingDamageFactor);
        }
    }
}
