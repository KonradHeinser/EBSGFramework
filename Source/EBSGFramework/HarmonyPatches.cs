using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        public static void CanEquipPostfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason)
        {
            EBSGExtension extension = thing.def.GetModExtension<EBSGExtension>();
            if (extension != null && __result)
            { // Attempt to get the various limiting lists
                List<GeneDef> requiredGenesToEquip = extension.requiredGenesToEquip;
                List<GeneDef> requireOneOfGenesToEquip = extension.requireOneOfGenesToEquip;
                List<GeneDef> forbiddenGenesToEquip = extension.forbiddenGenesToEquip;
                List<XenotypeDef> requireOneOfXenotypeToEquip = extension.requireOneOfXenotypeToEquip;
                List<XenotypeDef> forbiddenXenotypesToEquip = extension.forbiddenXenotypesToEquip;

                Pawn_GeneTracker currentGenes = pawn.genes;
                if (!requiredGenesToEquip.NullOrEmpty() || !requireOneOfGenesToEquip.NullOrEmpty() || !forbiddenGenesToEquip.NullOrEmpty() || !requireOneOfXenotypeToEquip.NullOrEmpty() || !forbiddenXenotypesToEquip.NullOrEmpty())
                {
                    bool flag = true;
                    if (!requireOneOfXenotypeToEquip.NullOrEmpty() && !requireOneOfXenotypeToEquip.Contains(pawn.genes.Xenotype) && flag)
                    {
                        cantReason = "SHG_XenoRestrictedEquipment_AnyOne".Translate();
                        flag = false;
                    }
                    if (!forbiddenXenotypesToEquip.NullOrEmpty() && forbiddenXenotypesToEquip.Contains(pawn.genes.Xenotype) && flag)
                    {
                        cantReason = "SHG_XenoRestrictedEquipment_None".Translate();
                        flag = false;
                    }
                    if (!forbiddenGenesToEquip.NullOrEmpty() && flag)
                    {
                        foreach (Gene gene in currentGenes.GenesListForReading)
                        {
                            if (forbiddenGenesToEquip.Contains(gene.def))
                            {
                                cantReason = "SHG_GeneRestrictedEquipment_None".Translate();
                                flag = false;
                                break;
                            }
                        }
                    }
                    if (!requiredGenesToEquip.NullOrEmpty() && flag)
                    {
                        foreach (Gene gene in currentGenes.GenesListForReading)
                        {
                            if (requiredGenesToEquip.Contains(gene.def))
                            {
                                requiredGenesToEquip.Remove(gene.def);
                            }
                        }
                        if (!requiredGenesToEquip.NullOrEmpty())
                        {
                            cantReason = "SHG_GeneRestrictedEquipment_All".Translate();
                            flag = false;
                        }
                    }
                    if (!requireOneOfGenesToEquip.NullOrEmpty() && flag)
                    {
                        flag = false;
                        cantReason = "SHG_GeneRestrictedEquipment_AnyOne".Translate();
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
                    __result = flag;
                }
                else __result = true;
            }
        }

        public static void DeathrestEfficiencyPostfix(ref int __result, Pawn ___pawn)
        {
            if (___pawn != null)
            {
                __result = (int)Math.Round(__result/ ___pawn.GetStatValue(EBSGDefOf.EBSG_DeathrestEfficiency), 0);
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
    }
}
