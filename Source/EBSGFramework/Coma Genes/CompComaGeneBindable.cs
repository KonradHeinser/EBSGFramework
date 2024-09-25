using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace EBSGFramework
{
    public class CompComaGeneBindable : ThingComp
    {
        public CompProperties_ComaBindable Props => (CompProperties_ComaBindable)props;

        private Pawn boundPawn;

        public int presenceTicks;

        [Unsaved(false)]
        private Material cachedHoseMat;

        [Unsaved(false)]
        private Sustainer sustainer;

        [Unsaved(false)]
        private CompPowerTrader cachedPowerComp;

        [Unsaved(false)]
        private CompRefuelable cachedRefuelableComp;

        [Unsaved(false)]
        private Gene_Coma cachedComaGene;

        public Pawn BoundPawn => boundPawn;

        public bool CanIncreasePresence
        {
            get
            {
                if (PowerTraderComp != null && !PowerTraderComp.PowerOn)
                {
                    return false;
                }
                if (RefuelableComp != null && !RefuelableComp.HasFuel)
                {
                    return false;
                }
                if (BoundPawn.InBed())
                {
                    Building_Bed building_Bed = BoundPawn.CurrentBed();
                    if (building_Bed == parent)
                    {
                        return true;
                    }
                    CompDeathrestBindable compDeathrestBindable = building_Bed.TryGetComp<CompDeathrestBindable>();
                    if (compDeathrestBindable == null || compDeathrestBindable.BoundPawn != boundPawn)
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }

        private CompPowerTrader PowerTraderComp
        {
            get
            {
                if (cachedPowerComp == null)
                {
                    cachedPowerComp = parent.TryGetComp<CompPowerTrader>();
                }
                return cachedPowerComp;
            }
        }

        private CompRefuelable RefuelableComp
        {
            get
            {
                if (cachedRefuelableComp == null)
                {
                    cachedRefuelableComp = parent.TryGetComp<CompRefuelable>();
                }
                return cachedRefuelableComp;
            }
        }

        public Gene_Coma ComaGene
        {
            get
            {
                if (cachedComaGene == null)
                {
                    cachedComaGene = boundPawn?.genes?.GetFirstGeneOfType<Gene_Coma>();
                }
                return cachedComaGene;
            }
        }

        public void Apply()
        {
            if (boundPawn != null)
            {
                if (Props.hediffToApply != null)
                {
                    boundPawn.health.AddHediff(Props.hediffToApply);
                }
                if (Props.hemogenLimitOffset > 0f)
                {
                    Gene_Hemogen gene_Hemogen = boundPawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
                    gene_Hemogen?.SetMax(gene_Hemogen.Max + Props.hemogenLimitOffset);
                }
            }
        }

        public void Notify_ComaEnded()
        {
            presenceTicks = 0;
            if (parent.Spawned)
            {
                if (sustainer != null && !sustainer.Ended)
                    sustainer.End();
                if (!Props.soundEnd.NullOrUndefined())
                    Props.soundEnd.PlayOneShot(SoundInfo.InMap(parent));
            }
        }

        public override string CompInspectStringExtra()
        {
            string text = null;
            if (boundPawn != null && ComaGene != null)
            {
                text = text + ("BoundTo".Translate() + ": " + boundPawn.NameShortColored).Resolve() + string.Format(" ({0}/{1} {2})", ComaGene.CurrentCapacity, ComaGene.ComaCapacity, "EBSG_ComaRestCapacity".Translate());
                if (Props.displayTimeActive && presenceTicks > 0 && ComaGene.comaRestTicks > 0)
                {
                    float f = Mathf.Clamp01((float)presenceTicks / (float)ComaGene.comaRestTicks);
                    text += string.Format("\n{0}: {1} / {2} ({3})\n{4}", "EBSG_TimeActiveThisComaRest".Translate(), presenceTicks.ToStringTicksToPeriod(true, true), ComaGene.comaRestTicks.ToStringTicksToPeriod(true, true), f.ToStringPercent(), "MinimumNeededToApply".Translate(0.75f.ToStringPercent()));
                }
            }
            else
            {
                text += "WillBindOnFirstUse".Translate();
            }
            return text;
        }

        public void TryIncreasePresence()
        {
            if (!CanIncreasePresence)
            {
                return;
            }
            if (presenceTicks <= 0)
            {
                SoundInfo info = SoundInfo.InMap(parent);
                if (!Props.soundWorking.NullOrUndefined() && (sustainer == null || sustainer.Ended))
                {
                    sustainer = Props.soundWorking.TrySpawnSustainer(info);
                }
                if (!Props.soundStart.NullOrUndefined())
                {
                    Props.soundStart.PlayOneShot(info);
                }
            }
            presenceTicks++;
        }

        public bool CanBindTo(Pawn pawn)
        {
            if (Props.mustBeLayingInToBind && pawn.CurrentBed() != parent)
                return false;
            if (boundPawn != null)
                return boundPawn == pawn;
            return true;
        }

        public void BindTo(Pawn pawn)
        {
            boundPawn = pawn;
        }

        public void Notify_ComaGeneRemoved()
        {
            cachedComaGene = null;
            boundPawn = null;
            presenceTicks = 0;
        }

        public override void PostExposeData()
        {
            Scribe_References.Look(ref boundPawn, "boundPawn");
            Scribe_Values.Look(ref presenceTicks, "presenceTicks", 0);
        }
    }
}
