using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("Rimworld.Alite.EBSG.main");
            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), nameof(EquipmentUtility.CanEquip), new[] { typeof(Thing), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool) }),
                postfix: new HarmonyMethod(patchType, nameof(CanEquipPostfix)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(Gene_Deathrest), nameof(Gene_Deathrest.MinDeathrestTicks)),
                postfix: new HarmonyMethod(patchType, nameof(DeathrestEfficiencyPostfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.BodyResourceGrowthSpeed)),
                postfix: new HarmonyMethod(patchType, nameof(BodyResourceGrowthSpeedPostfix)));
            harmony.Patch(AccessTools.Method(typeof(HediffGiver_Bleeding), nameof(HediffGiver_Bleeding.OnIntervalPassed)),
                postfix: new HarmonyMethod(patchType, nameof(BloodRecoveryPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryLovinChanceFactor)),
                          postfix: new HarmonyMethod(patchType, nameof(SecondaryLovinChanceFactorPostFix)));
            harmony.Patch(AccessTools.Method(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.RomanceFactors)),
                          postfix: new HarmonyMethod(patchType, nameof(RomanceFactorsPostFix)));

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
                            {
                                if (requiredGenesToEquip.Contains(gene.def)) requiredGenesToEquip.Remove(gene.def);
                            }
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
                            {
                                if (requiredGenesToEquip.Contains(gene.def))
                                {
                                    flag = true;
                                    cantReason = null;
                                    break;
                                }
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
                        {
                            foreach (HediffDef hediffDef in forbiddenHediffsToEquip)
                            {
                                if (hediffSet.HasHediff(hediffDef))
                                {
                                    cantReason = "EBSG_HediffRestrictedEquipment_None".Translate(hediffDef.LabelCap);
                                    flag = false; 
                                    break;
                                }
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
                            {
                                if (requireOneOfHediffsToEquip.Count > 1) cantReason = "EBSG_HediffRestrictedEquipment_AnyOne".Translate();
                                else cantReason = "EBSG_HediffRestrictedEquipment_One".Translate(requireOneOfHediffsToEquip[0]);
                            }
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
            if (flag && !pawn.genes.GenesListForReading.NullOrEmpty())
            {
                XenotypeDef xenotype = pawn.genes.Xenotype;
                if (xenotype != null && xenotype.HasModExtension<EquipRestrictExtension>())
                {
                    extension = xenotype.GetModExtension<EquipRestrictExtension>();
                    if (!extension.limitedToEquipments.NullOrEmpty() && !extension.limitedToEquipments.Contains(thing.def))
                    {
                        cantReason = "EBSG_LimitedList".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                    else if (!thing.def.IsWeapon && !extension.limitedToWeapons.NullOrEmpty() && !extension.limitedToWeapons.Contains(thing.def))
                    {
                        cantReason = "EBSG_LimitedList".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                    else if (thing.def.IsApparel && !extension.limitedToApparels.NullOrEmpty() && !extension.limitedToApparels.Contains(thing.def))
                    {
                        cantReason = "EBSG_LimitedList".Translate(xenotype.LabelCap);
                        flag = false;
                    }
                    else if (extension != null && !extension.forbiddenEquipments.NullOrEmpty() && extension.forbiddenEquipments.Contains(thing.def))
                    {
                        cantReason = "EBSG_ForbiddenList".Translate(xenotype.LabelCap);
                        flag = false;
                    }

                }
                if (flag)
                {
                    foreach (Gene gene in pawn.genes.GenesListForReading)
                    {
                        if (!gene.def.HasModExtension<EquipRestrictExtension>()) continue;
                        extension = gene.def.GetModExtension<EquipRestrictExtension>();
                        if (!extension.limitedToEquipments.NullOrEmpty() && !extension.limitedToEquipments.Contains(thing.def))
                        {
                            cantReason = "EBSG_LimitedList".Translate(gene.LabelCap);
                            flag = false;
                            break;
                        }
                        if (!thing.def.IsWeapon && !extension.limitedToWeapons.NullOrEmpty() && !extension.limitedToWeapons.Contains(thing.def))
                        {
                            cantReason = "EBSG_LimitedList".Translate(gene.LabelCap);
                            flag = false;
                            break;
                        }
                        if (thing.def.IsApparel && !extension.limitedToApparels.NullOrEmpty() && !extension.limitedToApparels.Contains(thing.def))
                        {
                            cantReason = "EBSG_LimitedList".Translate(gene.LabelCap);
                            flag = false;
                            break;
                        }
                        if (extension != null && !extension.forbiddenEquipments.NullOrEmpty() && extension.forbiddenEquipments.Contains(thing.def))
                        {
                            cantReason = "EBSG_ForbiddenList".Translate(gene.LabelCap);
                            flag = false;
                            break;
                        }
                    }
                }
            }
            __result = flag;
        }
            // Need to add a check on the genes and xenotype after checking for thing extension


        public static void DeathrestEfficiencyPostfix(ref int __result, Pawn ___pawn)
        {
            if (___pawn != null)
            {
                __result = (int)Math.Round(__result / ___pawn.GetStatValue(EBSGDefOf.EBSG_DeathrestEfficiency), 0);
            }
        }

        public static void BodyResourceGrowthSpeedPostfix(ref float __result, Pawn pawn)
        {
            if (pawn != null)
            {
                __result *= pawn.GetStatValue(EBSGDefOf.EBSG_PawnGestationSpeed);
            }
        }

        public static void BloodRecoveryPostfix(Pawn pawn, Hediff cause)
        {
            HediffSet hediffSet = pawn.health.hediffSet;
            if (hediffSet.BleedRateTotal < 0.1f)
            {
                HealthUtility.AdjustSeverity(pawn, HediffDefOf.BloodLoss, (-0.00033333333f * pawn.GetStatValue(EBSGDefOf.EBSG_BloodlossRecoveryBonus)));
            }
        }

        public static void SecondaryLovinChanceFactorPostFix(ref float __result, Pawn otherPawn, ref Pawn ___pawn)
        {
            if (ModsConfig.BiotechActive && otherPawn.genes != null)
            {
                List<Gene> genesListForReading = otherPawn.genes.GenesListForReading;

                foreach (Gene gene in genesListForReading)
                {
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
        }

        public static void RomanceFactorsPostFix(ref string __result, Pawn romancer, Pawn romanceTarget)
        {
            if (ModsConfig.BiotechActive && romancer.genes != null)
            {
                List<Gene> genesListForReading = romancer.genes.GenesListForReading;
                float num = 1f;
                bool flag = false;
                foreach (Gene gene in genesListForReading)
                {
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
                }
                if (flag)
                {
                    StringBuilder stringBuilder = new StringBuilder(__result);
                    stringBuilder.AppendLine(" - " + "EBSG_GeneticRomanceChance".Translate() + ": x" + num.ToStringPercent());
                    __result = stringBuilder.ToString();
                }
            }
        }
    }
}
