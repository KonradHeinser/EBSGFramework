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

        protected virtual float FallPerDay => def.fallPerDay;

        protected virtual FloatRange DefaultAgeRange => FloatRange.Zero;

        protected virtual bool AgeCompatFlag => false; // To be removed once 1.7 rolls around and murderous need compat isn't needed

        protected FloatRange ValidAgeRange
        {
            get
            {
                // return Extension?.ageRange ?? DefaultAgeRange;
                if (Extension != null)
                {
                    if (Extension.ageRange != FloatRange.Zero)
                        return Extension.ageRange;

                    if (AgeCompatFlag)
                        return new FloatRange(Extension.minAgeForNeed, Extension.maxAgeForNeed);
                }
                return DefaultAgeRange;
            }
        }

        public Need_GenericNeed(Pawn pawn) : base(pawn)
        {
        }

        public float FallMultiplier
        {
            get
            {
                if (Extension?.fallStat != null)
                    return pawn.StatOrOne(Extension.fallStat);
                return 1f;
            }
        }

        protected override bool IsFrozen
        {
            get
            {
                if (!ValidAgeRange.ValidValue(pawn.ageTracker.AgeBiologicalYears))
                    return true;
                return base.IsFrozen;
            }
        }

        public override bool ShowOnNeedList 
        {
            get
            {
                if (!ValidAgeRange.ValidValue(pawn.ageTracker.AgeBiologicalYears))
                    return false;
                return base.ShowOnNeedList;
            }
        }

        public override void NeedInterval()
        {
            if (!IsFrozen)
            {
                CurLevel -= FallPerDay / 400f * FallMultiplier;
                if (Extension?.hediffWhenEmpty != null)
                {
                    if (CurLevel <= 0)
                        pawn.AddOrAppendHediffs(Extension.initialSeverity, Extension.risePerDayWhenEmpty / 400f, Extension.hediffWhenEmpty);
                    else
                        pawn.AddOrAppendHediffs(0, (Extension.fallPerDayWhenNotEmpty / 400f) * -1, Extension.hediffWhenEmpty);
                }
            }
        }

        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1, bool drawArrows = true, bool doTooltip = true, Rect? rectForTooltip = null, bool drawLabel = true)
        {
            threshPercents = Extension?.thresholdPercentages ?? new List<float>() { 0.3f };
            base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip, rectForTooltip, drawLabel);
        }
    }
}
