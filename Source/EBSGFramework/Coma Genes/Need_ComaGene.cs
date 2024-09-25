using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class Need_ComaGene : Need_GenericNeed
    {
        public int lastComaTick = -999;

        public List<HediffDef> currentBonuses;

        public bool Comatose => Find.TickManager.TicksGame <= lastComaTick + 1;

        public override void SetInitialLevel()
        {
            CurLevel = 1f;
        }

        public override int GUIChangeArrow
        {
            get
            {
                if (IsFrozen)
                    return 0;

                if (!Comatose)
                    return -1;

                return 1;
            }
        }

        public float RiseMultiplier
        {
            get
            {
                if (ComaGene != null && ComaGene.Extension?.riseStat != null)
                    return pawn.GetStatValue(ComaGene.Extension.riseStat);
                return 1f;
            }
        }

        [Unsaved(false)]
        private Gene_Coma cachedComaGene;

        private Gene_Coma ComaGene
        {
            get
            {
                if (cachedComaGene == null)
                {
                    cachedComaGene = (Gene_Coma)pawn.genes?.GetGene(Extension.relatedGene);
                }
                return cachedComaGene;
            }
        }

        public override void NeedInterval()
        {
            if (!IsFrozen)
            {
                if (!Comatose)
                    CurLevel -= def.fallPerDay / 400f * FallMultiplier;
                else
                    CurLevel += ComaGene.Extension.gainPerDayComatose / 400f * RiseMultiplier;

                bool exhausted = EBSGUtilities.HasHediff(pawn, ComaGene.Extension.exhaustionHediff);
                if (CurLevel > 0f && exhausted)
                    EBSGUtilities.RemoveHediffs(pawn, ComaGene.Extension.exhaustionHediff);
                else if (CurLevel <= 0f && !exhausted)
                {
                    EBSGUtilities.RemoveHediffs(pawn, null, currentBonuses);
                    EBSGUtilities.AddOrAppendHediffs(pawn, hediff: ComaGene.Extension.exhaustionHediff);
                }
            }
            return;
        }

        public override string GetTipString()
        {
            string text = (LabelCap + ": " + CurLevelPercentage.ToStringPercent()).Colorize(ColoredText.TipSectionTitleColor) + "\n\n";
            if (!Comatose)
            {
                if (Extension?.nextComaTranslateString != null && Extension.shouldComaTranslateString != null)
                {
                    if (CurLevelPercentage > 0.1f)
                    {
                        float num = (CurLevelPercentage - 0.1f) / (1f / 30f);
                        text += EBSGUtilities.TranslateOrLiteral(Extension.nextComaTranslateString, pawn.LabelShortCap, "PeriodDays".Translate(num.ToString("F1"))).CapitalizeFirst();
                    }
                    else
                        text += EBSGUtilities.TranslateOrLiteral(Extension.shouldComaTranslateString, pawn.LabelShortCap).CapitalizeFirst().Colorize(ColorLibrary.RedReadable);

                    text += "\n\n";
                }
            }
            // Need to set up linked building count once gene itself is made
            return text + def.description;
        }

        public Need_ComaGene(Pawn pawn) : base(pawn)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastComaTick, "lastComaTick", -999);
            Scribe_Collections.Look(ref currentBonuses, "currentBonuses");
        }
    }
}
