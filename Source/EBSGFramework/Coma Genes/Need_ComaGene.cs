using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class Need_ComaGene : Need_GenericNeed
    {
        public int lastComaTick = -999;

        public List<HediffDef> currentBonuses;

        public bool Comatose => EBSGUtilities.HasHediff(pawn, ComaGene.ComaExtension.comaRestingHediff);

        private ComaExtension cachedExtension;

        public new ComaExtension Extension
        {
            get
            {
                if (cachedExtension == null && !alreadyChecked)
                {
                    cachedExtension = def.GetModExtension<ComaExtension>();
                    alreadyChecked = true;
                }

                return cachedExtension;
            }
        }

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
                if (ComaGene != null && ComaGene.ComaExtension?.riseStat != null)
                    return pawn.GetStatValue(ComaGene.ComaExtension.riseStat);
                return 1f;
            }
        }

        public new float FallMultiplier
        {
            get
            {
                if (ComaGene?.ComaExtension.fallStat != null)
                    return pawn.GetStatValue(ComaGene.ComaExtension.fallStat);
                return 1f;
            }
        }

        [Unsaved(false)]
        private Gene_Coma cachedComaGene;

        public Gene_Coma ComaGene
        {
            get
            {
                if (cachedComaGene == null)
                    cachedComaGene = pawn.genes?.GetGene(Extension.relatedGene) as Gene_Coma;

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
                    CurLevel += ComaGene.ComaExtension.gainPerDayComatose / 400f * ComaGene.ComaEfficiency;

                bool exhausted = EBSGUtilities.HasHediff(pawn, ComaGene.ComaExtension.exhaustionHediff);
                if (CurLevel > 0f && exhausted)
                    EBSGUtilities.RemoveHediffs(pawn, ComaGene.ComaExtension.exhaustionHediff);
                else if (CurLevel <= 0f && !exhausted)
                {
                    EBSGUtilities.RemoveHediffs(pawn, null, currentBonuses);
                    EBSGUtilities.AddOrAppendHediffs(pawn, hediff: ComaGene.ComaExtension.exhaustionHediff);
                }
            }
            return;
        }

        public override string GetTipString()
        {
            string text = (LabelCap + ": " + CurLevelPercentage.ToStringPercent()).Colorize(ColoredText.TipSectionTitleColor) + "\n\n";
            if (!Comatose)
            {
                if (CurLevelPercentage > 0.1f)
                {
                    float num = (CurLevelPercentage - 0.1f) / (1f / 30f);
                    text += "EBSG_NextComaNeed".Translate(pawn.LabelShortCap, ComaGene.ComaExtension.noun ?? ComaGene.Label, "PeriodDays".Translate(num.ToString("F1"))).Resolve().CapitalizeFirst();
                }
                else
                    text += "EBSG_ShouldComaRestNow".Translate(pawn.LabelShortCap, ComaGene.ComaExtension.noun ?? ComaGene.Label).Resolve().CapitalizeFirst().Colorize(ColorLibrary.RedReadable);

                text += "\n\n";
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
