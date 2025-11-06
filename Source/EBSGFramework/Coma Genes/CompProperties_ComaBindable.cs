using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_ComaBindable : CompProperties
    {
        public string unbindTexPath = "UI/Gizmos/DeathrestAutoWake";

        public int stackLimit = 1;

        public bool countsTowardsBuildingLimit = true;

        public bool displayTimeActive = true;

        public float comaRestEffectivenessFactor = 1f;

        public float hemogenLimitOffset; // Left in due to there not really being a way to increase it via hediff

        public string connectionLinePath;

        public HediffDef hediffToApply;

        public float severity = 1f;

        public SoundDef soundWorking;

        public SoundDef soundStart;

        public SoundDef soundEnd;

        public List<GeneDef> relatedGenes;

        public CompProperties_ComaBindable()
        {
            compClass = typeof(CompComaGeneBindable);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            if (!Mathf.Approximately(comaRestEffectivenessFactor, 1f))
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Building, "EBSG_StatsReport_ComaRestEffectiveness".Translate(), comaRestEffectivenessFactor.ToStringPercent(), "EBSG_StatsReport_ComaRestEffectiveness_Desc".Translate(), 900);
            }
            if (countsTowardsBuildingLimit)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Building, "EBSG_StatsReport_ComaRestConnectionLimit".Translate(), (stackLimit <= 0) ? "Unlimited".Translate().ToString() : stackLimit.ToString(), "EBSG_StatsReport_ComaRestConnectionLimit_Desc".Translate(), 910);
            }
        }
    }
}
