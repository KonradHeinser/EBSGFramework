using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class StageOverlay
    {
        public string label;
        public string overrideLabel;
        public string untranslatedLabel;
        public bool? becomeVisible;
        public bool? lifeThreatening;
        public TaleDef tale;
        public float? vomitMtbDays;
        public float? deathMtbDays;
        public bool? mtbDeathDestroysBrain;
        public float painFactor = 1f;
        public float painOffset = 0f;
        public float totalBleedFactor = 1f;
        public float? naturalHealingFactor;
        public float? forgetMemoryThoughtMtbDays;
        public float pctConditionalThoughtsNullified = 0f;
        public float opinionOfOthersFactor = 1f;
        public float fertilityFactor = 1f;
        public float hungerRateFactor = 1f;
        public float hungerRateFactorOffset = 0f;
        public float restFallFactor = 1f;
        public float restFallFactorOffset = 0f;
        public float socialFightChanceFactor = 1f;
        public float foodPoisoningChanceFactor = 1f;
        public float? mentalBreakMtbDays;
        public string mentalBreakExplanation;
        public List<MentalBreakIntensity> allowedMentalBreakIntensities;
        public List<HediffDef> makeImmuneTo;
        public List<PawnCapacityModifier> capMods;
        public List<HediffGiver> hediffGivers;
        public List<MentalStateGiver> mentalStateGivers;
        public List<StatModifier> statOffsets;
        public List<StatModifier> statOffsetFactors;
        public List<StatModifier> statFactors;
        public List<StatModifier> statFactorOffsets;
        public bool? multiplyStatChangesBySeverity;
        public StatDef statOffsetEffectMultiplier;
        public StatDef statFactorEffectMultiplier;
        public StatDef capacityFactorEffectMultiplier;
        public WorkTags? disabledWorkTags;
        public string overrideTooltip;
        public string extraTooltip;
        public float partEfficiencyOffset = 0f;
        public bool? partIgnoreMissingHP;


        public virtual HediffStage ModifyHediffStage(HediffStage stage)
        {
            if (label != null)
                stage.label = label;

            if (becomeVisible != null)
                stage.becomeVisible = (bool)becomeVisible;

            if (untranslatedLabel != null)
                stage.untranslatedLabel = untranslatedLabel;

            if (lifeThreatening != null)
                stage.lifeThreatening = (bool)lifeThreatening;

            if (tale != null)
                stage.tale = tale;

            if (vomitMtbDays != null)
                stage.vomitMtbDays = (float)vomitMtbDays;

            if (deathMtbDays != null)
                stage.deathMtbDays = (float)deathMtbDays;

            if (mtbDeathDestroysBrain != null)
                stage.mtbDeathDestroysBrain = (bool)mtbDeathDestroysBrain;

            stage.painFactor *= painFactor;
            stage.painOffset += painOffset;
            stage.totalBleedFactor *= totalBleedFactor;

            if (naturalHealingFactor != null)
                if (stage.naturalHealingFactor == -1)
                    stage.naturalHealingFactor = (float)naturalHealingFactor;
                else
                    stage.naturalHealingFactor *= (float)naturalHealingFactor;

            if (forgetMemoryThoughtMtbDays != null)
                stage.forgetMemoryThoughtMtbDays = (float)forgetMemoryThoughtMtbDays;
            
            stage.pctConditionalThoughtsNullified += pctConditionalThoughtsNullified;
            stage.opinionOfOthersFactor *= opinionOfOthersFactor;
            stage.fertilityFactor *= fertilityFactor;
            stage.hungerRateFactor *= hungerRateFactor;
            stage.hungerRateFactorOffset += hungerRateFactorOffset;
            stage.restFallFactor *= restFallFactor;
            stage.restFallFactorOffset += restFallFactorOffset;
            stage.socialFightChanceFactor *= socialFightChanceFactor;
            stage.foodPoisoningChanceFactor *= foodPoisoningChanceFactor;

            if (mentalBreakMtbDays != null)
                stage.mentalBreakMtbDays = (float)mentalBreakMtbDays;

            if (mentalBreakExplanation != null)
                stage.mentalBreakExplanation = mentalBreakExplanation;

            if (!allowedMentalBreakIntensities.NullOrEmpty())
                if (!stage.allowedMentalBreakIntensities.NullOrEmpty())
                    stage.allowedMentalBreakIntensities = stage.allowedMentalBreakIntensities.Union(allowedMentalBreakIntensities).ToList();
                else
                    stage.allowedMentalBreakIntensities = allowedMentalBreakIntensities;

            if (!makeImmuneTo.NullOrEmpty())
                if (stage.makeImmuneTo.NullOrEmpty())
                    stage.makeImmuneTo = stage.makeImmuneTo.Union(makeImmuneTo).ToList();
                else
                    stage.makeImmuneTo = makeImmuneTo;

            if (!capMods.NullOrEmpty())
                if (!stage.capMods.NullOrEmpty())
                {
                    List<PawnCapacityModifier> unusedCapMods = new List<PawnCapacityModifier>(capMods);
                    foreach (PawnCapacityModifier ourCap in capMods)
                        foreach (PawnCapacityModifier stageCap in stage.capMods)
                        {
                            if (ourCap.capacity != stageCap.capacity)
                                continue;

                            unusedCapMods.Remove(ourCap);
                            stageCap.offset += ourCap.offset;
                            stageCap.setMax = Math.Min(stageCap.setMax, ourCap.setMax);
                            stageCap.postFactor *= ourCap.postFactor;
                        }

                    if (!unusedCapMods.NullOrEmpty())
                        stage.capMods.AddRange(unusedCapMods);
                }
                else
                    stage.capMods = capMods;

            if (hediffGivers != null)
                if (stage.hediffGivers != null)
                    stage.hediffGivers = stage.hediffGivers.Union(hediffGivers).ToList();
                else
                    stage.hediffGivers = hediffGivers;

            if (!mentalStateGivers.NullOrEmpty())
                if (stage.mentalStateGivers != null)
                    stage.mentalStateGivers = stage.mentalStateGivers.Union(mentalStateGivers).ToList();
                else
                    stage.mentalStateGivers = mentalStateGivers;

            if (!statOffsetFactors.NullOrEmpty())
                if (!stage.statOffsets.NullOrEmpty())
                {
                    foreach (StatModifier ourMod in statOffsetFactors)
                        foreach (StatModifier stageMod in stage.statOffsets)
                        {
                            if (ourMod.stat != stageMod.stat)
                                continue;

                            stageMod.value *= ourMod.value;
                            break;
                        }
                }
                else
                    stage.statOffsets = statOffsetFactors;

            if (!statOffsets.NullOrEmpty())
            { 
                List<StatModifier> unusedStatOffsets = new List<StatModifier>(statOffsets);
                if (!stage.statOffsets.NullOrEmpty())
                {
                    foreach (StatModifier ourMod in statOffsets)
                        foreach (StatModifier stageMod in stage.statOffsets)
                        {
                            if (ourMod.stat != stageMod.stat)
                                continue;

                            unusedStatOffsets.Remove(ourMod);
                            stageMod.value += ourMod.value;
                            break;
                        }

                    if (!unusedStatOffsets.NullOrEmpty())
                        stage.statOffsets.AddRange(unusedStatOffsets);
                }
                else
                    stage.statOffsets = statOffsets;
            }

            if (!statFactors.NullOrEmpty())
                if (!stage.statFactors.NullOrEmpty())
                {
                    foreach (StatModifier ourMod in statFactors)
                        foreach (StatModifier stageMod in stage.statFactors)
                        {
                            if (ourMod.stat != stageMod.stat)
                                continue;

                            stageMod.value *= ourMod.value;
                            break;
                        }
                }
                else
                    stage.statFactors = statFactors;

            if (!statFactorOffsets.NullOrEmpty())
            {
                List<StatModifier> unusedStatFactorOffsets = new List<StatModifier>(statFactorOffsets);
                if (!stage.statFactors.NullOrEmpty())
                { 
                    foreach (StatModifier ourMod in statFactorOffsets)
                        foreach (StatModifier stageMod in stage.statFactors)
                        {
                            if (ourMod.stat != stageMod.stat)
                                continue;

                            unusedStatFactorOffsets.Remove(ourMod);
                            stageMod.value += ourMod.value;
                            break;
                        }

                    if (!unusedStatFactorOffsets.NullOrEmpty())
                        stage.statFactors.AddRange(unusedStatFactorOffsets);
                }
                else
                    stage.statFactors = statFactors;
            }

            if (multiplyStatChangesBySeverity != null)
                stage.multiplyStatChangesBySeverity = (bool)multiplyStatChangesBySeverity;

            if (statOffsetEffectMultiplier != null)
                stage.statOffsetEffectMultiplier = statOffsetEffectMultiplier;

            if (statFactorEffectMultiplier != null)
                stage.statFactorEffectMultiplier = statFactorEffectMultiplier;

            if (capacityFactorEffectMultiplier != null)
                stage.capacityFactorEffectMultiplier = capacityFactorEffectMultiplier;

            if (disabledWorkTags != null)
                stage.disabledWorkTags = (WorkTags)disabledWorkTags;

            if (overrideTooltip != null)
                stage.overrideTooltip = overrideTooltip;

            if (extraTooltip != null)
                stage.extraTooltip = extraTooltip;

            stage.partEfficiencyOffset += partEfficiencyOffset;

            if (partIgnoreMissingHP != null)
                stage.partIgnoreMissingHP = (bool)partIgnoreMissingHP;

            return stage;
        }
    }
}
