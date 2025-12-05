using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class Need_ComaGene : Need_GenericNeed
    {
        public int lastComaTick = -999;

        public List<HediffDef> currentBonuses;

        public bool Comatose => pawn.HasHediff(ComaGene.ComaExtension.comaRestingHediff);

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

        public float RiseMultiplier => ComaGene?.ComaExtension?.riseStat != null ? pawn.StatOrOne(ComaGene.ComaExtension.riseStat) : 1f;

        public new float FallMultiplier => ComaGene?.ComaExtension.fallStat != null ? pawn.StatOrOne(ComaGene.ComaExtension.fallStat) : 1f;

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

                bool exhausted = pawn.HasHediff(ComaGene.ComaExtension.exhaustionHediff);
                if (CurLevel > 0f && exhausted)
                    pawn.RemoveHediffs(ComaGene.ComaExtension.exhaustionHediff);
                else if (CurLevel <= 0f && !exhausted)
                {
                    pawn.RemoveHediffs(null, currentBonuses);
                    pawn.AddOrAppendHediffs(hediff: ComaGene.ComaExtension.exhaustionHediff);
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
