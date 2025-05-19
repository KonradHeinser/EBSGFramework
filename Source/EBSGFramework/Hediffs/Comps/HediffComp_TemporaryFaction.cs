using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_TemporaryFaction : HediffComp
    {
        public HediffCompProperties_TemporaryFaction Props => (HediffCompProperties_TemporaryFaction)props;

        private Faction oldFaction = null;

        private PawnKindDef oldKindDef = null;

        private HediffWithTarget ParentWithTarget => parent as HediffWithTarget;

        private Pawn ParentTarget => ParentWithTarget?.target as Pawn;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            oldFaction = Pawn.Faction;
            oldKindDef = Pawn.kindDef;

            if (Props.useStatic)
            {
                Faction newFaction = Find.FactionManager.FirstFactionOfDef(Props.staticFaction);
                if (newFaction != Pawn.Faction)
                    Pawn.SetFaction(newFaction);
            }
            else if (ParentTarget != null)
            {
                if (ParentTarget.Faction != Pawn.Faction)
                    Pawn.SetFaction(ParentTarget.Faction, ParentTarget);
            }
            else
            {
                Log.Error($"{Def} doesn't use static factions, but also doesn't appear to be a HediffWithTarget. No faction can be set, and this hediff will be removed to avoid more errors.");
                Pawn.health.RemoveHediff(parent);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn.IsHashIntervalTick(60) && !Props.useStatic && Pawn.Faction != ParentTarget.Faction)
                Pawn.SetFaction(ParentTarget.Faction, ParentTarget);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (Props.temporary && Pawn.Faction != oldFaction)
                Pawn.SetFaction(oldFaction);
            Pawn.ChangeKind(oldKindDef);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref oldFaction, "oldFaction");
            Scribe_Defs.Look(ref oldKindDef, "oldKindDef");
        }
    }
}
