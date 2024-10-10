using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
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
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"),
                postfix: new HarmonyMethod(patchType, nameof(AddHumanlikeOrdersPostfix)));

            /*
            harmony.Patch(AccessTools.Method(typeof(Ability), "PreActivate"),
                postfix: new HarmonyMethod(patchType, nameof(PreActivatePostfix)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(Pawn_AbilityTracker), nameof(Pawn_AbilityTracker.AllAbilitiesForReading)),
                postfix: new HarmonyMethod(patchType, nameof(AllAbilitiesForReadingPostfix)));
            */

            /*
            harmony.Patch(AccessTools.Method(typeof(FoodUtility), "WillEat", new[] { typeof(Pawn), typeof(ThingDef), typeof(Pawn), typeof(bool), typeof(bool) }),
                postfix: new HarmonyMethod(patchType, nameof(WillEatThingDefPostfix)));
            harmony.Patch(AccessTools.Method(typeof(FoodUtility), "WillEat", new[] { typeof(Pawn), typeof(Thing), typeof(Pawn), typeof(bool), typeof(bool) }),
                postfix: new HarmonyMethod(patchType, nameof(WillEatThingPostfix)));
            */

            harmony.Patch(AccessTools.Method(typeof(ForbidUtility), nameof(ForbidUtility.IsForbidden), new[] { typeof(Thing), typeof(Pawn) }),
                postfix: new HarmonyMethod(patchType, nameof(IsForbiddenPostfix)));
            harmony.Patch(AccessTools.Method(typeof(GeneUtility), nameof(GeneUtility.IsBloodfeeder)),
                postfix: new HarmonyMethod(patchType, nameof(IsBloodfeederPostfix)));
            harmony.Patch(AccessTools.Method(typeof(GeneUIUtility), "RecacheGenes"),
                postfix: new HarmonyMethod(patchType, nameof(RecacheGenesPostfix)));
            harmony.Patch(AccessTools.Method(typeof(CompAbilityEffect_ReimplantXenogerm), nameof(CompAbilityEffect_ReimplantXenogerm.PawnIdeoCanAcceptReimplant)),
                 postfix: new HarmonyMethod(patchType, nameof(PawnIdeoCanAcceptReimplantPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Xenogerm), nameof(Xenogerm.PawnIdeoDisallowsImplanting)),
                 postfix: new HarmonyMethod(patchType, nameof(PawnIdeoDisallowsImplantingPostFix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "TryGenerateNewPawnInternal"),
                postfix: new HarmonyMethod(patchType, nameof(TryGenerateNewPawnInternalPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GeneratePawnRelations"),
                prefix: new HarmonyMethod(patchType, nameof(CanGeneratePawnRelationsPrefix)));
            harmony.Patch(AccessTools.Method(typeof(Dialog_CreateXenotype), "DrawGene"),
                prefix: new HarmonyMethod(patchType, nameof(DrawGenePrefix)));
            harmony.Patch(AccessTools.Method(typeof(GeneDef), nameof(GeneDef.ConflictsWith)),
                postfix: new HarmonyMethod(patchType, nameof(GeneConflictsWithPostfix)));
            harmony.Patch(AccessTools.Method(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts)),
                postfix: new HarmonyMethod(patchType, nameof(MakeRecipeProductsPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.DropBloodFilth)),
                prefix: new HarmonyMethod(patchType, nameof(DropBloodFilthPrefix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.DropBloodSmear)),
                prefix: new HarmonyMethod(patchType, nameof(DropBloodSmearPrefix)));

            // Needs Harmony patches
            harmony.Patch(AccessTools.Method(typeof(Need_Seeker), nameof(Need_Seeker.NeedInterval)),
                postfix: new HarmonyMethod(patchType, nameof(SeekerNeedMultiplier)));
            if (ModsConfig.BiotechActive)
            {
                harmony.Patch(AccessTools.Method(typeof(Need_KillThirst), nameof(Need_KillThirst.NeedInterval)),
                    postfix: new HarmonyMethod(patchType, nameof(KillThirstPostfix)));
            }

            harmony.Patch(AccessTools.Method(typeof(Need_Joy), nameof(Need_Joy.GainJoy)),
                prefix: new HarmonyMethod(patchType, nameof(GainJoyPrefix)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.GrowthPointsPerDay)),
                postfix: new HarmonyMethod(patchType, nameof(GrowthPointStatPostfix)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(Pawn_PsychicEntropyTracker), "PsyfocusFallPerDay"),
                postfix: new HarmonyMethod(patchType, nameof(PsyfocusFallPerDayPostFix)));

            // Stat Harmony patches
            harmony.Patch(AccessTools.PropertyGetter(typeof(Gene_Deathrest), nameof(Gene_Deathrest.DeathrestEfficiency)),
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
            harmony.Patch(AccessTools.Method(typeof(Gene), nameof(Gene.PostAdd)),
                postfix: new HarmonyMethod(patchType, nameof(PostAddGenePostfix)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(Gene_Hemogen), nameof(Gene_Hemogen.InitialResourceMax)),
                postfix: new HarmonyMethod(patchType, nameof(HemogenMaxPostFix)));

            // Vanilla code bug fixes
            harmony.Patch(AccessTools.Method(typeof(Widgets), nameof(Widgets.DefIcon)),
                prefix: new HarmonyMethod(patchType, nameof(DefIconPrefix)));

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        // Bug fixes
        public static bool DefIconPrefix(Rect rect, ref Def def, float scale = 1f, Material material = null, int? graphicIndexOverride = null)
        {
            if (EBSG_Settings.defaultToRecipeIcon && !(def is BuildableDef))
            {
                if (def is RecipeDef recipe && recipe.UIIconThing != null && recipe.UIIcon != null)
                {
                    Widgets.DrawTextureFitted(rect, recipe.UIIcon, scale, material);
                    return false;
                }
            }
            return true;
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
                            if (requireOneOfXenotypeToEquip.Count > 1) cantReason = "EBSG_XenoRestrictedEquipment_AnyOne".Translate();
                            else cantReason = "EBSG_XenoRestrictedEquipment_One".Translate(extension.requireOneOfXenotypeToEquip[0].LabelCap);
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
                    if (extension.noEquipment)
                    {
                        cantReason = "EBSG_NoEquipment".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                    else if (!extension.limitedToEquipments.NullOrEmpty() && !extension.limitedToEquipments.Contains(thing.def))
                    {
                        cantReason = "EBSG_LimitedList".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                    else if (thing.def.IsWeapon && extension.noWeapons)
                    {
                        cantReason = "EBSG_NoWeapon".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                    else if (thing.def.IsWeapon && extension.onlyMelee && !thing.def.IsMeleeWeapon)
                    {
                        cantReason = "EBSG_OnlyMelee".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                    else if (thing.def.IsWeapon && extension.onlyRanged && thing.def.IsRangedWeapon)
                    {
                        cantReason = "EBSG_OnlyRanged".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                    else if (thing.def.IsWeapon && !extension.limitedToWeapons.NullOrEmpty() && !extension.limitedToWeapons.Contains(thing.def))
                    {
                        cantReason = "EBSG_LimitedList".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                    else if (thing.def.IsApparel && extension.noApparel)
                    {
                        cantReason = "EBSG_NoApparel".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                    else if (thing.def.IsApparel && !extension.limitedToApparels.NullOrEmpty() && !extension.limitedToApparels.Contains(thing.def))
                    {
                        cantReason = "EBSG_LimitedList".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                    else if (!extension.forbiddenEquipments.NullOrEmpty() && extension.forbiddenEquipments.Contains(thing.def))
                    {
                        cantReason = "EBSG_ForbiddenList".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                }
                if (flag)
                    if (Cache != null && Cache.NeedEquipRestrictGeneCheck())
                    {
                        if (!Cache.noEquipment.NullOrEmpty())
                            if (EBSGUtilities.PawnHasAnyOfGenes(pawn, out var gene, Cache.noEquipment))
                            {
                                cantReason = "EBSG_NoEquipment".Translate(gene.LabelCap);
                                flag = false;
                            }
                        if (flag && thing.def.IsWeapon && !Cache.noWeapon.NullOrEmpty())
                            if (EBSGUtilities.PawnHasAnyOfGenes(pawn, out var gene, Cache.noWeapon))
                            {
                                cantReason = "EBSG_NoWeapon".Translate(gene.LabelCap);
                                flag = false;
                            }
                        if (flag && thing.def.IsApparel && !Cache.noApparel.NullOrEmpty())
                            if (EBSGUtilities.PawnHasAnyOfGenes(pawn, out var gene, Cache.noApparel))
                            {
                                cantReason = "EBSG_NoApparel".Translate(gene.LabelCap);
                                flag = false;
                            }
                        if (flag && !Cache.equipRestricting.NullOrEmpty())
                            foreach (GeneDef gene in Cache.equipRestricting)
                                if (EBSGUtilities.HasRelatedGene(pawn, gene))
                                {
                                    extension = gene.GetModExtension<EquipRestrictExtension>();
                                    if (!extension.limitedToEquipments.NullOrEmpty() && !extension.limitedToEquipments.Contains(thing.def))
                                    {
                                        cantReason = "EBSG_LimitedList".Translate(gene.LabelCap);
                                        flag = false;
                                    }
                                    else if (thing.def.IsWeapon && extension.onlyMelee && !thing.def.IsMeleeWeapon)
                                    {
                                        cantReason = "EBSG_OnlyMelee".Translate(gene.LabelCap);
                                        flag = false;
                                    }
                                    else if (thing.def.IsWeapon && extension.onlyRanged && thing.def.IsRangedWeapon)
                                    {
                                        cantReason = "EBSG_OnlyRanged".Translate(gene.LabelCap);
                                        flag = false;
                                    }
                                    else if (thing.def.IsWeapon && !extension.limitedToWeapons.NullOrEmpty() && !extension.limitedToWeapons.Contains(thing.def))
                                    {
                                        cantReason = "EBSG_LimitedList".Translate(gene.LabelCap);
                                        flag = false;
                                    }
                                    else if (thing.def.IsApparel && !extension.limitedToApparels.NullOrEmpty() && !extension.limitedToApparels.Contains(thing.def))
                                    {
                                        cantReason = "EBSG_LimitedList".Translate(gene.LabelCap);
                                        flag = false;
                                    }
                                    else if (!extension.forbiddenEquipments.NullOrEmpty() && extension.forbiddenEquipments.Contains(thing.def))
                                    {
                                        cantReason = "EBSG_ForbiddenList".Translate(gene.LabelCap);
                                        flag = false;
                                    }
                                    if (!flag) break;
                                }
                    }
            }
            __result = flag;
        }

        public static void TryGenerateNewPawnInternalPostfix(ref Pawn __result)
        {
            if (__result != null && __result.genes != null)
            {
                bool flagApparel = true; // Need for apparel check
                bool flagWeapon = true; // Need for weapon check

                if (__result.genes.Xenotype != null && __result.genes.Xenotype.HasModExtension<EquipRestrictExtension>())
                {
                    EquipRestrictExtension equipRestrict = __result.genes.Xenotype.GetModExtension<EquipRestrictExtension>();
                    if (equipRestrict.noEquipment)
                    {
                        if (__result.equipment != null) __result.equipment.DestroyAllEquipment();
                        if (__result.apparel != null) __result.apparel.DestroyAll();
                        return;
                    }

                    if (!equipRestrict.forbiddenEquipments.NullOrEmpty())
                    {
                        if (__result.apparel != null && !__result.apparel.WornApparel.NullOrEmpty())
                        {
                            List<Apparel> apparels = new List<Apparel>(__result.apparel.WornApparel);
                            foreach (Apparel apparel in apparels)
                                if (equipRestrict.forbiddenEquipments.Contains(apparel.def))
                                    __result.apparel.Remove(apparel);
                        }
                        if (__result.equipment != null && !__result.equipment.AllEquipmentListForReading.NullOrEmpty())
                        {
                            List<ThingWithComps> equipment = new List<ThingWithComps>(__result.equipment.AllEquipmentListForReading);
                            foreach (ThingWithComps thing in equipment)
                                if (equipRestrict.forbiddenEquipments.Contains(thing.def))
                                    __result.equipment.DestroyEquipment(thing);
                        }
                    }

                    if (__result.apparel != null)
                    {
                        if (equipRestrict.noApparel)
                        {
                            __result.apparel.DestroyAll();
                            flagApparel = false;
                        }

                        else if ((!equipRestrict.limitedToApparels.NullOrEmpty() || !equipRestrict.limitedToEquipments.NullOrEmpty()) && !__result.apparel.WornApparel.NullOrEmpty())
                        {
                            List<Apparel> apparels = new List<Apparel>(__result.apparel.WornApparel);
                            foreach (Apparel apparel in apparels)
                                if (!CheckEquipLists(equipRestrict.limitedToApparels, equipRestrict.limitedToEquipments, apparel.def))
                                    __result.apparel.Remove(apparel);
                        }
                    }
                    else
                        flagApparel = false;

                    if (__result.equipment != null)
                    {
                        if (equipRestrict.noWeapons)
                        {
                            __result.equipment.DestroyAllEquipment();
                            flagWeapon = false;
                        }
                        else if ((!equipRestrict.limitedToWeapons.NullOrEmpty() || !equipRestrict.limitedToEquipments.NullOrEmpty()) && !__result.equipment.AllEquipmentListForReading.NullOrEmpty())
                        {
                            List<ThingWithComps> equipment = new List<ThingWithComps>(__result.equipment.AllEquipmentListForReading);
                            foreach (ThingWithComps thing in equipment)
                                if (!CheckEquipLists(equipRestrict.limitedToWeapons, equipRestrict.limitedToEquipments, thing.def))
                                    __result.equipment.DestroyEquipment(thing);
                        }
                    }
                    else
                        flagWeapon = false;
                }

                if (flagApparel && __result.apparel.WornApparel.NullOrEmpty()) flagApparel = false;
                if (flagWeapon && __result.equipment.AllEquipmentListForReading.NullOrEmpty()) flagWeapon = false;

                if (!flagApparel && !flagWeapon) return; // If both weapons and apparel have been cleared, everything should be gone already

                if (!__result.genes.GenesListForReading.NullOrEmpty() && Cache != null && Cache.NeedEquipRestrictGeneCheck())
                    foreach (Gene gene in __result.genes.GenesListForReading)
                    {
                        if (!flagApparel && !flagWeapon) return; // If both weapons and apparel have been cleared, everything should be gone already
                        if (!gene.def.HasModExtension<EquipRestrictExtension>()) continue;

                        EquipRestrictExtension equipRestrict = gene.def.GetModExtension<EquipRestrictExtension>();

                        if (equipRestrict.noEquipment)
                        {
                            if (flagWeapon) __result.equipment.DestroyAllEquipment();
                            if (flagApparel) __result.apparel.DestroyAll();
                            return;
                        }

                        if (!equipRestrict.forbiddenEquipments.NullOrEmpty())
                        {
                            if (flagApparel)
                            {
                                List<Apparel> apparels = new List<Apparel>(__result.apparel.WornApparel);
                                foreach (Apparel apparel in apparels)
                                    if (equipRestrict.forbiddenEquipments.Contains(apparel.def))
                                        __result.apparel.Remove(apparel);
                            }
                            if (flagWeapon)
                            {
                                List<ThingWithComps> equipment = new List<ThingWithComps>(__result.equipment.AllEquipmentListForReading);
                                foreach (ThingWithComps thing in equipment)
                                    if (equipRestrict.forbiddenEquipments.Contains(thing.def))
                                        __result.equipment.DestroyEquipment(thing);
                            }
                        }

                        if (flagApparel)
                        {
                            if (equipRestrict.noApparel)
                            {
                                __result.apparel.DestroyAll();
                                flagApparel = false;
                            }

                            else if ((!equipRestrict.limitedToApparels.NullOrEmpty() || !equipRestrict.limitedToEquipments.NullOrEmpty()) && !__result.apparel.WornApparel.NullOrEmpty())
                            {
                                List<Apparel> apparels = new List<Apparel>(__result.apparel.WornApparel);
                                foreach (Apparel apparel in apparels)
                                    if (!CheckEquipLists(equipRestrict.limitedToApparels, equipRestrict.limitedToEquipments, apparel.def))
                                        __result.apparel.Remove(apparel);
                            }
                        }
                        else
                            flagApparel = false;

                        if (flagWeapon)
                            if (equipRestrict.noWeapons)
                            {
                                __result.equipment.DestroyAllEquipment();
                                flagWeapon = false;
                            }
                            else if ((!equipRestrict.limitedToWeapons.NullOrEmpty() || !equipRestrict.limitedToEquipments.NullOrEmpty()) && !__result.equipment.AllEquipmentListForReading.NullOrEmpty())
                            {
                                List<ThingWithComps> equipment = new List<ThingWithComps>(__result.equipment.AllEquipmentListForReading);
                                foreach (ThingWithComps thing in equipment)
                                    if (!CheckEquipLists(equipRestrict.limitedToWeapons, equipRestrict.limitedToEquipments, thing.def))
                                        __result.equipment.DestroyEquipment(thing);
                            }

                        if (flagApparel && __result.apparel.WornApparel.NullOrEmpty()) flagApparel = false;
                        if (flagWeapon && __result.equipment.AllEquipmentListForReading.NullOrEmpty()) flagWeapon = false;
                    }
            }
        }

        public static bool CanGeneratePawnRelationsPrefix(Pawn pawn)
        {
            EBSGRecorder recorder = DefDatabase<EBSGRecorder>.GetNamedSilentFail("EBSG_Recorder");
            if (recorder != null && !recorder.pawnKindsWithoutIntialRelationships.NullOrEmpty())
                if (recorder.pawnKindsWithoutIntialRelationships.Contains(pawn.kindDef)) return false;

            return true;
        }

        public static bool DrawGenePrefix(GeneDef geneDef, ref bool __result)
        {
            EBSGRecorder recorder = DefDatabase<EBSGRecorder>.GetNamedSilentFail("EBSG_Recorder");
            if (recorder != null)
            {
                if (!recorder.hiddenGenes.NullOrEmpty() && recorder.hiddenGenes.Contains(geneDef))
                {
                    __result = false;
                    return false;
                }
                if (!recorder.hiddenTemplates.NullOrEmpty())
                    foreach (GeneTemplateDef template in recorder.hiddenTemplates)
                        if (geneDef.defName.StartsWith(template.defName + "_", StringComparison.Ordinal))
                        {
                            __result = false;
                            return false;
                        }
            }
            return true;
        }

        public static void GeneConflictsWithPostfix(GeneDef other, GeneDef __instance, ref bool __result)
        {
            if (!__result)
            {
                if (__instance.HasModExtension<EBSGExtension>())
                {
                    EBSGExtension extension = __instance.GetModExtension<EBSGExtension>();
                    if (!extension.conflictingGenes.NullOrEmpty() && extension.conflictingGenes.Contains(other))
                    {
                        __result = true;
                        return;
                    }
                }
                if (other.HasModExtension<EBSGExtension>())
                {
                    EBSGExtension extension = other.GetModExtension<EBSGExtension>();
                    if (!extension.conflictingGenes.NullOrEmpty() && extension.conflictingGenes.Contains(__instance))
                        __result = true;
                }
            }
        }

        // Things represents the temporary list, while equipment represents the universal one. thing is the item in question. False means it wasn't in either list
        private static bool CheckEquipLists(List<ThingDef> things, List<ThingDef> equipment, ThingDef thing)
        {
            if (!things.NullOrEmpty() && things.Contains(thing)) return true;
            if (!equipment.NullOrEmpty() && equipment.Contains(thing)) return true;
            return false;
        }

        public static void SecondaryLovinChanceFactorPostFix(ref float __result, Pawn otherPawn, ref Pawn ___pawn)
        {
            if (__result != 0 && ModsConfig.BiotechActive && otherPawn.genes != null && Cache != null && !Cache.grcGenes.NullOrEmpty())
            {
                if (EBSGUtilities.PawnHasAnyOfGenes(otherPawn, out GeneDef firstMatch, Cache.grcGenes))
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
                            if (__result < 0) __result = 0;
                            if (__result == 0) return;
                        }
                }
            }
        }

        public static void RomanceFactorsPostFix(ref string __result, Pawn romancer, Pawn romanceTarget)
        {
            if (ModsConfig.BiotechActive && romancer.genes != null && Cache != null && !Cache.grcGenes.NullOrEmpty() &&
                EBSGUtilities.PawnHasAnyOfGenes(romancer, out GeneDef firstMatch, Cache.grcGenes))
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
                        if (!extension.carrierStats.NullOrEmpty())
                            foreach (StatDef stat in extension.carrierStats)
                            {
                                float statValue = romancer.GetStatValue(stat);
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
                        if (!extension.otherStats.NullOrEmpty())
                            foreach (StatDef stat in extension.otherStats)
                            {
                                float statValue = romanceTarget.GetStatValue(stat);
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
                        if (EBSGUtilities.HasRelatedGene(___pawn, geneticMultiplier.gene) && geneticMultiplier.multiplier != 0 && !EBSGUtilities.PawnHasAnyOfGenes(___pawn, out var gene, geneticMultiplier.nullifyingGenes))
                        {
                            __result *= geneticMultiplier.multiplier;
                            ensureReverse |= geneticMultiplier.multiplier < 0;
                        }

                if (ensureReverse && positiveValue == __result > 0) __result *= -1;
            }
            // This is a universal mood factor, as opposed to the specialized ones above. Usually ends up returning 1
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
            if (Cache != null && !Cache.murderousNeeds.NullOrEmpty() && dinfo?.Instigator != null && dinfo.Value.Instigator is Pawn pawn &&
                pawn.needs != null && !pawn.needs.AllNeeds.NullOrEmpty())
                foreach (Need need in pawn.needs.AllNeeds)
                    if (need is Need_Murderous murderNeed)
                        murderNeed.Notify_KilledPawn(dinfo, __instance);
        }

        public static void AddHumanlikeOrdersPostfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
        {
            if (Cache?.ComaNeedsExist() == true && pawn.genes != null)
            {
                Building_Bed bed = null;

                IntVec3 clickCell = IntVec3.FromVector3(clickPos);
                if (clickCell.GetThingList(pawn.Map).NullOrEmpty()) return;

                foreach (Thing thing in clickCell.GetThingList(pawn.Map))
                    if (thing.def.IsBed)
                    {
                        bed = thing as Building_Bed;
                        break;
                    }
                if (bed == null) return;

                Gene_Coma comaGene = pawn.genes.GetFirstGeneOfType<Gene_Coma>();
                if (comaGene != null)
                {
                    // Always ensures the coma gene closest to empty is used. This shouldn't really be needed, but I can't assume that everyone listens to my warning
                    float needLevel = comaGene.ComaNeed.CurLevel;

                    foreach (Gene gene in pawn.genes.GenesListForReading)
                        if (gene is Gene_Coma coma && coma.ComaNeed.CurLevel < needLevel)
                        {
                            comaGene = coma;
                            needLevel = coma.ComaNeed.CurLevel;
                        }

                    if (!pawn.CanReach(bed, PathEndMode.OnCell, Danger.Deadly))
                    {
                        opts.Add(new FloatMenuOption("EBSG_CannotRest".Translate().CapitalizeFirst() + ": " + "NoPath".Translate().CapitalizeFirst(), null));
                        return;
                    }
                    AcceptanceReport acceptanceReport2 = bed.CompAssignableToPawn.CanAssignTo(pawn);
                    if (!acceptanceReport2.Accepted)
                    {
                        opts.Add(new FloatMenuOption("EBSG_CannotRest".Translate().CapitalizeFirst() + ": " + acceptanceReport2.Reason, null));
                        return;
                    }
                    if ((!bed.CompAssignableToPawn.HasFreeSlot || !RestUtility.BedOwnerWillShare(bed, pawn, pawn.guest.GuestStatus)) && !bed.IsOwner(pawn))
                    {
                        opts.Add(new FloatMenuOption("EBSG_CannotRest".Translate().CapitalizeFirst() + ": " + "AssignedToOtherPawn".Translate(bed).CapitalizeFirst(), null));
                        return;
                    }

                    if (comaGene.Extension.needBedOutOfSunlight)
                        foreach (IntVec3 item25 in bed.OccupiedRect())
                            if (item25.GetRoof(bed.Map) == null)
                            {
                                opts.Add(new FloatMenuOption("EBSG_CannotRest".Translate().CapitalizeFirst() + ": " + "ThingIsSkyExposed".Translate(bed).CapitalizeFirst(), null));
                                return;
                            }
                    if (RestUtility.IsValidBedFor(bed, pawn, pawn, true, false, false, pawn.GuestStatus))
                    {
                        opts.Add(new FloatMenuOption(EBSGUtilities.TranslateOrLiteral(comaGene.Extension.startRestLabel) ?? "EBSG_StartRest".Translate(), delegate
                        {
                            Job job25 = JobMaker.MakeJob(comaGene.Extension.relatedJob, bed);
                            job25.forceSleep = true;
                            pawn.jobs.TryTakeOrderedJob(job25, JobTag.Misc);
                        }));
                    }
                }
            }
        }

        public static void PreActivatePostfix(Ability __instance, Pawn ___pawn)
        {
            if (Cache?.needEquippableAbilityPatches == true)
            {
                if (___pawn.apparel?.WornApparel.NullOrEmpty() == false)
                {
                    List<Apparel> equipment = new List<Apparel>(___pawn.apparel.WornApparel);
                    foreach (ThingWithComps thing in equipment)
                        thing.TryGetComp<CompAbilityLimitedCharges>()?.UsedAbility(__instance);
                }

                if (___pawn.equipment?.AllEquipmentListForReading.NullOrEmpty() == false)
                {
                    List<ThingWithComps> equipment = new List<ThingWithComps>(___pawn.equipment.AllEquipmentListForReading);
                    foreach (ThingWithComps thing in equipment)
                        thing.TryGetComp<CompAbilityLimitedCharges>()?.UsedAbility(__instance);
                }
            }
        }

        public static void AllAbilitiesForReadingPostfix(ref List<Ability> __result, List<Ability> ___allAbilitiesCached, Pawn ___pawn, ref bool ___allAbilitiesCachedDirty)
        {
            if (Cache?.needEquippableAbilityPatches == true)
            {
                if (___pawn.apparel?.WornApparel.NullOrEmpty() == false)
                    foreach (Apparel thing in ___pawn.apparel.WornApparel)
                    {
                        Ability ability = thing.TryGetComp<CompAbilityLimitedCharges>()?.AbilityForReading;
                        if (ability != null)
                        {
                            ___allAbilitiesCached.Add(ability);
                        }
                    }

                if (___pawn.equipment?.AllEquipmentListForReading.NullOrEmpty() == false)
                    foreach (ThingWithComps thing in ___pawn.equipment.AllEquipmentListForReading)
                    {
                        Ability ability = thing.TryGetComp<CompAbilityLimitedCharges>()?.AbilityForReading;
                        if (ability != null)
                        {
                            ___allAbilitiesCached.Add(ability);
                        }
                    }
                __result = ___allAbilitiesCached;
            }
        }

        /*
        public static void WillEatThingDefPostfix(ref bool __result, Pawn p, ThingDef food, Pawn getter, bool careIfNotAcceptableForTitle, bool allowVenerated)
        {
            if (__result && Cache?.NeedEatPatch == true && p.genes != null)
            {
                if (EBSGUtilities.PawnHasAnyOfGenes(p, out var first, Cache.forbidFoods))
                {
                    FoodExtension foodExtension = first.GetModExtension<FoodExtension>();

                    if (foodExtension.forbiddenFoods.Contains(food))
                    {
                        __result = false;
                        return;
                    }
                }
            }
        }

        public static void WillEatThingPostfix(ref bool __result, Pawn p, Thing food, Pawn getter, bool careIfNotAcceptableForTitle, bool allowVenerated)
        {
            if (__result && Cache?.NeedEatPatch == true && p.genes != null)
            {
                if (EBSGUtilities.PawnHasAnyOfGenes(p, out var first, Cache.forbidFoods))
                {
                    FoodExtension foodExtension = first.GetModExtension<FoodExtension>();

                    if (foodExtension.forbiddenFoods.Contains(food.def))
                    {
                        __result = false;
                        return;
                    }
                }
            }
        }
        */
        public static void IsForbiddenPostfix(Thing t, Pawn pawn, ref bool __result)
        {
            if (!__result && t is Corpse corpse && (t.Faction == null || pawn.Faction == null || t.Faction != pawn.Faction))
            {
                MultipleLives_Component multipleLives = Current.Game.GetComponent<MultipleLives_Component>();
                if (multipleLives != null && multipleLives.loaded && !multipleLives.forbiddenCorpses.NullOrEmpty())
                    __result = multipleLives.forbiddenCorpses.Contains(corpse);
            }
        }

        public static void IsBloodfeederPostfix(Pawn pawn, ref bool __result)
        {
            if (!__result && DefDatabase<EBSGRecorder>.GetNamedSilentFail("EBSG_Recorder") != null)
                __result = EBSGUtilities.HasAnyOfRelatedGene(pawn, DefDatabase<EBSGRecorder>.GetNamedSilentFail("EBSG_Recorder").bloodfeederGenes);
        }

        public static void RecacheGenesPostfix(Thing target, GeneSet genesOverride, ref List<Gene> ___xenogenes, ref List<Gene> ___endogenes)
        {
            if (target is Pawn pawn)
            {
                if (pawn.genes.Xenotype != null && pawn.genes.Xenotype.HasModExtension<EBSGExtension>() && pawn.genes.Xenotype.GetModExtension<EBSGExtension>().hideAllInactiveGenesForXenotype)
                {
                    foreach (Gene gene in pawn.genes.Xenogenes)
                        if (gene.Overridden && ___xenogenes.Contains(gene))
                            ___xenogenes.Remove(gene);

                    foreach (Gene gene in pawn.genes.Endogenes)
                        if (gene.Overridden && ___endogenes.Contains(gene))
                            ___endogenes.Remove(gene);
                }
                else
                {
                    if (pawn.genes.Xenotype != null && pawn.genes.Xenotype.HasModExtension<EBSGExtension>())
                    {
                        EBSGExtension extension = pawn.genes.Xenotype.GetModExtension<EBSGExtension>();
                        if (extension.hideAllInactiveSkinColorGenesForXenotype)
                        {
                            foreach (Gene gene in pawn.genes.Xenogenes)
                                if (gene.def.skinColorOverride != null && gene.Overridden && ___xenogenes.Contains(gene))
                                    ___xenogenes.Remove(gene);

                            foreach (Gene gene in pawn.genes.Endogenes)
                                if (gene.def.skinColorOverride != null && gene.Overridden && ___endogenes.Contains(gene))
                                    ___endogenes.Remove(gene);
                        }

                        if (extension.hideAllInactiveHairColorGenesForXenotype)
                        {
                            foreach (Gene gene in pawn.genes.Xenogenes)
                                if (gene.def.endogeneCategory == EndogeneCategory.HairColor && gene.Overridden && ___xenogenes.Contains(gene))
                                    ___xenogenes.Remove(gene);

                            foreach (Gene gene in pawn.genes.Endogenes)
                                if (gene.def.endogeneCategory == EndogeneCategory.HairColor && gene.Overridden && ___endogenes.Contains(gene))
                                    ___endogenes.Remove(gene);
                        }
                    }

                    if (Cache != null && !Cache.hiddenWhenInactive.NullOrEmpty())
                    {
                        foreach (Gene gene in pawn.genes.Xenogenes)
                            if (gene.Overridden && ___xenogenes.Contains(gene) && Cache.hiddenWhenInactive.Contains(gene.def))
                                ___xenogenes.Remove(gene);

                        foreach (Gene gene in pawn.genes.Endogenes)
                            if (gene.Overridden && ___endogenes.Contains(gene) && Cache.hiddenWhenInactive.Contains(gene.def))
                                ___endogenes.Remove(gene);
                    }
                }
            }
        }

        public static void PawnIdeoCanAcceptReimplantPostfix(ref bool __result, Pawn implanter, Pawn implantee)
        {
            if (__result && DefDatabase<EBSGRecorder>.GetNamedSilentFail("EBSG_Recorder") != null)
            {
                EBSGRecorder recorder = DefDatabase<EBSGRecorder>.GetNamed("EBSG_Recorder");
                if (!recorder.geneEvents.NullOrEmpty())
                    foreach (GeneEvent geneEvent in recorder.geneEvents)
                        if (geneEvent.propagateEvent != null && !IdeoUtility.DoerWillingToDo(geneEvent.propagateEvent, implantee) &&
                            implanter.genes.GenesListForReading.Any((Gene x) => x.def == geneEvent.gene))
                        {
                            __result = false;
                            break;
                        }
            }
        }

        public static void PawnIdeoDisallowsImplantingPostFix(ref bool __result, Pawn selPawn, ref Xenogerm __instance)
        {
            if (!__result && DefDatabase<EBSGRecorder>.GetNamedSilentFail("EBSG_Recorder") != null)
            {
                EBSGRecorder recorder = DefDatabase<EBSGRecorder>.GetNamed("EBSG_Recorder");
                if (!recorder.geneEvents.NullOrEmpty())
                    foreach (GeneEvent geneEvent in recorder.geneEvents)
                        if (geneEvent.propagateEvent != null && !IdeoUtility.DoerWillingToDo(geneEvent.propagateEvent, selPawn) &&
                            __instance.GeneSet.GenesListForReading.Any((GeneDef x) => x == geneEvent.gene))
                        {
                            __result = true;
                            break;
                        }
            }
        }

        public static bool DropBloodFilthPrefix(Pawn ___pawn)
        {
            if (Cache != null && !Cache.bloodReplacingGenes.NullOrEmpty() && (___pawn.Spawned || ___pawn.ParentHolder is Pawn_CarryTracker)
                && ___pawn.SpawnedOrAnyParentSpawned && EBSGUtilities.PawnHasAnyOfGenes(___pawn, out GeneDef bloodGene, Cache.bloodReplacingGenes))
            {
                EBSGExtension bloodExtension = bloodGene.GetModExtension<EBSGExtension>();
                if (bloodExtension.bloodFilthAmount <= 0 || !Rand.Chance(bloodExtension.bloodDropChance)) return false;
                if (bloodExtension.bloodReplacement != null)
                    FilthMaker.TryMakeFilth(___pawn.PositionHeld, ___pawn.MapHeld, bloodExtension.bloodReplacement, ___pawn.LabelIndefinite(),
                        bloodExtension.bloodFilthAmount);
            }
            return true;
        }

        public static bool DropBloodSmearPrefix(Pawn ___pawn, ref Vector3? ___lastSmearDropPos)
        {
            if (Cache != null && !Cache.bloodSmearReplacingGenes.NullOrEmpty()
                && EBSGUtilities.PawnHasAnyOfGenes(___pawn, out GeneDef bloodGene, Cache.bloodSmearReplacingGenes))
            {
                EBSGExtension bloodExtension = bloodGene.GetModExtension<EBSGExtension>();
                if (!Rand.Chance(bloodExtension.bloodSmearDropChance)) return false;
                if (bloodExtension.bloodReplacement != null)
                {
                    FilthMaker.TryMakeFilth(___pawn.PositionHeld, ___pawn.MapHeld, bloodExtension.bloodSmearReplacement, out var outFilth, ___pawn.LabelIndefinite());
                    if (outFilth != null)
                    {
                        float rotation = ((!___lastSmearDropPos.HasValue) ? ___pawn.pather.lastMoveDirection : (___lastSmearDropPos.Value - ___pawn.DrawPos).AngleFlat());
                        outFilth.SetOverrideDrawPositionAndRotation(___pawn.DrawPos.WithY(bloodExtension.bloodSmearReplacement.Altitude), rotation);
                        ___lastSmearDropPos = ___pawn.DrawPos;
                    }
                }
            }
            return true;
        }

        // Harmony patches for stats

        public static void DeathrestEfficiencyPostfix(ref float __result, Pawn ___pawn)
        {
            if (___pawn != null)
                __result *= ___pawn.GetStatValue(EBSGDefOf.EBSG_DeathrestEfficiency);
        }

        public static void DeathrestNeedIntervalPostfix(ref Need_Deathrest __instance, Pawn ___pawn)
        {
            if (!__instance.Deathresting)
                __instance.CurLevel += -1f / 30f / 400f * (___pawn.GetStatValue(EBSGDefOf.EBSG_DeathrestFallRate) - 1);
            else
                __instance.CurLevel += 1f / 30f / 400f * (___pawn.GetStatValue(EBSGDefOf.EBSG_DeathrestRiseRate) - 1);
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

        public static bool GainJoyPrefix(ref float amount, Pawn ___pawn)
        {
            amount *= ___pawn.GetStatValue(EBSGDefOf.EBSG_JoyRiseRate);
            return true;
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
            if (pawn.health != null && !pawn.health.hediffSet.hediffs.NullOrEmpty())
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
                MultipleLives_Component multipleLives = Current.Game.GetComponent<MultipleLives_Component>();
                if (multipleLives != null && multipleLives.loaded && !multipleLives.forbiddenCorpses.NullOrEmpty() && multipleLives.forbiddenCorpses.Contains(corpse))
                {
                    dinfo.SetAmount(0);
                    return true;
                }
            }
            if (__instance is Pawn victim && Cache != null)
            {
                if (dinfo.Instigator is Pawn attacker)
                    if (victim.RaceProps.Humanlike)
                    {
                        if (!Cache.humanoidSlayingStats.NullOrEmpty())
                            foreach (StatDef stat in Cache.humanoidSlayingStats)
                                dinfo.SetAmount(dinfo.Amount * attacker.GetStatValue(stat));
                    }
                    else if (victim.RaceProps.Animal)
                    {
                        if (!Cache.animalSlayingStats.NullOrEmpty())
                            foreach (StatDef stat in Cache.animalSlayingStats)
                                dinfo.SetAmount(dinfo.Amount * attacker.GetStatValue(stat));
                    }
                    else if (victim.RaceProps.IsMechanoid)
                    {
                        if (!Cache.mechanoidSlayingStats.NullOrEmpty())
                            foreach (StatDef stat in Cache.mechanoidSlayingStats)
                                dinfo.SetAmount(dinfo.Amount * attacker.GetStatValue(stat));
                    }
                    else if (victim.RaceProps.Insect)
                    {
                        if (!Cache.insectSlayingStats.NullOrEmpty())
                            foreach (StatDef stat in Cache.insectSlayingStats)
                                dinfo.SetAmount(dinfo.Amount * attacker.GetStatValue(stat));
                    }
                    else if (ModsConfig.AnomalyActive && victim.RaceProps.IsAnomalyEntity)
                    {
                        if (!Cache.entitySlayingStats.NullOrEmpty())
                            foreach (StatDef stat in Cache.entitySlayingStats)
                                dinfo.SetAmount(dinfo.Amount * attacker.GetStatValue(stat));
                    }
                    else if (victim.RaceProps.Dryad)
                    {
                        if (!Cache.dryadSlayingStats.NullOrEmpty())
                            foreach (StatDef stat in Cache.dryadSlayingStats)
                                dinfo.SetAmount(dinfo.Amount * attacker.GetStatValue(stat));
                    }

                if (dinfo.Weapon != null)
                    if (victim.RaceProps.Humanlike)
                    {
                        if (!Cache.humanoidSlayingStats.NullOrEmpty())
                            foreach (StatDef stat in Cache.humanoidSlayingStats)
                                dinfo.SetAmount(dinfo.Amount * dinfo.Weapon.statBases.GetStatValueFromList(stat, 1));
                    }
                    else if (victim.RaceProps.Animal)
                    {
                        if (!Cache.animalSlayingStats.NullOrEmpty())
                            foreach (StatDef stat in Cache.animalSlayingStats)
                                dinfo.SetAmount(dinfo.Amount * dinfo.Weapon.statBases.GetStatValueFromList(stat, 1));
                    }
                    else if (victim.RaceProps.IsMechanoid)
                    {
                        if (!Cache.mechanoidSlayingStats.NullOrEmpty())
                            foreach (StatDef stat in Cache.mechanoidSlayingStats)
                                dinfo.SetAmount(dinfo.Amount * dinfo.Weapon.statBases.GetStatValueFromList(stat, 1));
                    }
                    else if (victim.RaceProps.Insect)
                    {
                        if (!Cache.insectSlayingStats.NullOrEmpty())
                            foreach (StatDef stat in Cache.insectSlayingStats)
                                dinfo.SetAmount(dinfo.Amount * dinfo.Weapon.statBases.GetStatValueFromList(stat, 1));
                    }
                    else if (ModsConfig.AnomalyActive && victim.RaceProps.IsAnomalyEntity)
                    {
                        if (!Cache.entitySlayingStats.NullOrEmpty())
                            foreach (StatDef stat in Cache.entitySlayingStats)
                                dinfo.SetAmount(dinfo.Amount * dinfo.Weapon.statBases.GetStatValueFromList(stat, 1));
                    }
                    else if (victim.RaceProps.Dryad)
                    {
                        if (!Cache.dryadSlayingStats.NullOrEmpty())
                            foreach (StatDef stat in Cache.dryadSlayingStats)
                                dinfo.SetAmount(dinfo.Amount * dinfo.Weapon.statBases.GetStatValueFromList(stat, 1));
                    }
            }
            return true;
        }

        public static void TakeDamagePostfix(ref DamageInfo dinfo, Thing __instance, DamageWorker.DamageResult __result)
        {
            if (Cache != null && !Cache.explosiveAttackHediffs.NullOrEmpty() && dinfo.Instigator != null && dinfo.Instigator is Pawn pawn
                && !pawn.Dead && HasSpecialExplosion(pawn) && !DoingSpecialExplosion(pawn, dinfo, __instance)
                && EBSGUtilities.GetCurrentTarget(pawn, false) == __instance && !EBSGUtilities.CastingAbility(pawn))
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

        public static void HemogenMaxPostFix(Pawn ___pawn, ref float __result, GeneDef ___def)
        {
            if (___def.geneClass == typeof(Gene_Hemogen))
            {
                __result += ___pawn.GetStatValue(EBSGDefOf.EBSG_HemogenMaxOffset);
                __result *= ___pawn.GetStatValue(EBSGDefOf.EBSG_HemogenMaxFactor);
            }
        }

        public static void PostAddGenePostfix(Pawn ___pawn)
        {
            if (Cache != null) Cache.CachePawnWithGene(___pawn);
        }

        public static void MakeRecipeProductsPostfix(ref IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient,
            IBillGiver billGiver, Precept_ThingStyle precept = null, ThingStyleDef style = null, int? overrideGraphicIndex = null)
        {
            if (recipeDef.HasModExtension<EBSGExtension>())
            {
                EBSGExtension extension = recipeDef.GetModExtension<EBSGExtension>();
                if (!extension.thingCountList.NullOrEmpty())
                {
                    List<Thing> newResult = new List<Thing>(__result);

                    float efficiency = ((recipeDef.efficiencyStat != null) ? worker.GetStatValue(recipeDef.efficiencyStat) : 1f);
                    if (recipeDef.workTableEfficiencyStat != null && billGiver is Building_WorkTable thing)
                        efficiency *= thing.GetStatValue(recipeDef.workTableEfficiencyStat);

                    foreach (List<ThingDefCountClass> options in extension.thingCountList)
                    {
                        bool flag = false;
                        ThingDefCountClass thingClass = null;

                        if (options.Count() == 1)
                        {
                            thingClass = options[0];
                            if (thingClass.chance != null)
                                flag |= Rand.Chance((float)thingClass.chance);
                            else
                                flag = true;
                        }
                        else
                        {
                            thingClass = options.RandomElementByWeight((arg) => (float)arg.chance);
                            flag = true;
                        }

                        if (flag)
                        {
                            Thing newThing = ThingMaker.MakeThing(thingClass.thingDef, thingClass.thingDef.MadeFromStuff ? thingClass.stuff ?? dominantIngredient.def : null);
                            newThing.stackCount = Mathf.CeilToInt((float)thingClass.count * efficiency);
                            if (dominantIngredient != null && recipeDef.useIngredientsForColor)
                                newThing.SetColor(dominantIngredient.DrawColor, false);

                            CompIngredients compIngredients = newThing.TryGetComp<CompIngredients>();
                            if (compIngredients != null)
                                for (int l = 0; l < ingredients.Count; l++)
                                    compIngredients.RegisterIngredient(ingredients[l].def);

                            newThing.Notify_RecipeProduced(worker);

                            // PostProcessProduct Stuff
                            CompQuality compQuality = newThing.TryGetComp<CompQuality>();
                            if (compQuality != null)
                            {
                                if (extension.staticQuality)
                                    compQuality.SetQuality(thingClass.quality, ArtGenerationContext.Colony);
                                else
                                {
                                    if (recipeDef.workSkill == null)
                                        Log.Error(string.Concat(recipeDef, " needs workSkill because it creates a product with a quality."));

                                    QualityCategory q = QualityUtility.GenerateQualityCreatedByPawn(worker, recipeDef.workSkill);
                                    compQuality.SetQuality(q, ArtGenerationContext.Colony);
                                }

                                QualityUtility.SendCraftNotification(newThing, worker);
                            }

                            CompArt compArt = newThing.TryGetComp<CompArt>();
                            if (compArt != null)
                                compArt.JustCreatedBy(worker);
                            if (compQuality != null && (int)compQuality.Quality >= 4)
                                TaleRecorder.RecordTale(TaleDefOf.CraftedArt, worker, newThing);

                            if (worker.Ideo != null)
                                newThing.StyleDef = worker.Ideo.GetStyleFor(newThing.def);

                            if (precept != null)
                                newThing.StyleSourcePrecept = precept;
                            else if (style != null)
                                newThing.StyleDef = style;
                            else if (!newThing.def.randomStyle.NullOrEmpty() && Rand.Chance(newThing.def.randomStyleChance))
                                newThing.SetStyleDef(newThing.def.randomStyle.RandomElementByWeight((ThingStyleChance x) => x.Chance).StyleDef);

                            newThing.overrideGraphicIndex = overrideGraphicIndex;
                            if (newThing.def.Minifiable)
                                newThing = newThing.MakeMinified();

                            newResult.Add(newThing);
                        }
                    }

                    __result = newResult.AsEnumerable();
                }
            }
        }
    }
}
