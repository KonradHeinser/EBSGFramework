using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace EBSGFramework
{
    public class Need_GenericNeed : Need
    {
        public bool alreadyChecked;

        private EBSGExtension cachedExtension;

        public EBSGExtension Extension
        {
            get
            {
                if (cachedExtension == null && !alreadyChecked)
                {
                    cachedExtension = def.GetModExtension<EBSGExtension>();
                    alreadyChecked = true;
                }

                return cachedExtension;
            }
        }

        public List<float> ThresholdPercentages
        {
            get
            {
                if (Extension == null)
                    return new List<float>() { 0.3f };
                return Extension.thresholdPercentages;
            }
        }

        public Need_GenericNeed(Pawn pawn) : base(pawn)
        {
        }

        public float FallMultiplier
        {
            get
            {
                if (Extension != null && Extension.fallStat != null)
                    return pawn.GetStatValue(Extension.fallStat);
                return 1f;
            }
        }

        public override void NeedInterval()
        {
            if (!IsFrozen)
            {
                CurLevel -= def.fallPerDay / 400f * FallMultiplier;
                if (Extension != null && Extension.hediffWhenEmpty != null)
                {
                    if (CurLevel <= 0)
                        EBSGUtilities.AddOrAppendHediffs(pawn, Extension.initialSeverity, Extension.risePerDayWhenEmpty / 400f, Extension.hediffWhenEmpty);
                    else
                        EBSGUtilities.AddOrAppendHediffs(pawn, 0, 1 - (Extension.fallPerDayWhenNotEmpty / 400f), Extension.hediffWhenEmpty);
                }
            }
        }

        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1, bool drawArrows = true, bool doTooltip = true, Rect? rectForTooltip = null, bool drawLabel = true)
        {
            threshPercents = ThresholdPercentages;
            base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip, rectForTooltip, drawLabel);
        }
    }
}
