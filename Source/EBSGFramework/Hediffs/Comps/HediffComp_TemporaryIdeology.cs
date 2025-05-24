using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_TemporaryIdeology : HediffComp
    {
        public HediffCompProperties_TemporaryIdeology Props => (HediffCompProperties_TemporaryIdeology)props;

        public Ideo previousIdeo;

        public float previousCertainty;

        private HediffWithTarget ParentWithTarget => parent as HediffWithTarget;

        private Pawn ParentTarget => ParentWithTarget?.target as Pawn;

        public Ideo ideo;

        public bool GetIdeo(out float certainty, bool removing = false)
        {
            certainty = 0f;
            if (Props.factionOfIdeo != null)
            {
                ideo = Find.FactionManager.FirstFactionOfDef(Props.factionOfIdeo)?.ideos?.PrimaryIdeo;
                certainty = Props.certainty != FloatRange.Zero ? Props.certainty.RandomInRange : Pawn.ideo.Certainty;
            }
            else if (ParentTarget?.Ideo != null)
            {
                ideo = ParentTarget.Ideo;
                certainty = ParentTarget.ideo.Certainty;
            }
            else
            {
                Log.Error($"{Def} doesn't have a static faction to pull an ideology from, and lacks a hediff target with an ideology. Removing the hediff to avoid more errors");
                if (!removing)
                    Pawn.health.RemoveHediff(parent);
                return false;
            }
            return true;
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            if (!ModsConfig.IdeologyActive || Pawn.ideo == null)
                return;

            previousIdeo = Pawn.Ideo;
            previousCertainty = Pawn.ideo.Certainty;

            if (GetIdeo(out float certainty))
            {
                Pawn.ideo.SetIdeo(ideo);
                Pawn.ideo.OffsetCertainty(certainty - Pawn.ideo.Certainty);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            // Only checks once every 10 seconds because doing it more often is excessive, and doing it less could create awkward delays
            if (Pawn.IsHashIntervalTick(600) && ModsConfig.IdeologyActive && Pawn.ideo != null)
            {
                // This checks to make sure this is the hediff that is supposed to be messing with stuff right now
                var otherHediffs = new List<Hediff>(Pawn.health.hediffSet.hediffs);
                for (int i = otherHediffs.Count - 1; i >= 0; i--)
                {
                    if (otherHediffs[i] == parent)
                        break;

                    if (otherHediffs[i].TryGetComp<HediffComp_TemporaryIdeology>() != null)
                        return;
                }

                if (GetIdeo(out var certainty))
                {
                    if (Pawn.Ideo != ideo)
                        Pawn.ideo.SetIdeo(ideo);

                    // Condition used to avoid constantly message caused by certainty changes
                    if (Pawn.ideo.Certainty < certainty - 0.2f)
                        Pawn.ideo.OffsetCertainty(certainty - Pawn.ideo.Certainty);
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (!ModsConfig.IdeologyActive || Pawn.ideo == null)
                return;

            if (Props.temporary)
            {
                // This checks if there's another version of this comp that should be taking over
                var otherHediffs = new List<Hediff>(Pawn.health.hediffSet.hediffs);
                for (int i = otherHediffs.Count - 1; i > 0; i--)
                {
                    if (otherHediffs[i] == parent)
                        continue;

                    var tempComp = otherHediffs[i].TryGetComp<HediffComp_TemporaryIdeology>();
                    if (tempComp != null && tempComp.GetIdeo(out float certainty, true))
                    {
                        if (Pawn.Ideo != tempComp.ideo)
                            Pawn.ideo.SetIdeo(tempComp.ideo);
                        if (Pawn.ideo.Certainty != certainty)
                            Pawn.ideo.OffsetCertainty(certainty - Pawn.ideo.Certainty);
                        return;
                    }
                }

                Pawn.ideo.SetIdeo(previousIdeo);
                Pawn.ideo.OffsetCertainty(previousCertainty - Pawn.ideo.Certainty);
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref previousIdeo, "previousIdeo");
            Scribe_Values.Look(ref previousCertainty, "previousCertainty");
        }
    }
}
