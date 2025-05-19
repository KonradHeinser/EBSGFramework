using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_TemporaryFaction : HediffComp
    {
        public HediffCompProperties_TemporaryFaction Props => (HediffCompProperties_TemporaryFaction)props;

        private Faction oldFaction = null;

        private HediffWithTarget ParentWithTarget => parent as HediffWithTarget;

        private Pawn ParentTarget => ParentWithTarget?.target as Pawn;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            oldFaction = Pawn.Faction;
            Log.Message(Pawn.kindDef?.label);
            if (Props.useStatic)
                Pawn.SetFaction(Find.FactionManager.FirstFactionOfDef(Props.staticFaction));
            else if (ParentTarget != null)
                Pawn.SetFaction(ParentTarget.Faction, ParentTarget);
            else
            {
                Log.Error($"{Def} doesn't use static factions, but also doesn't appear to be a HediffWithTarget. No faction can be set, and this hediff will be removed to avoid more errors.");
                Pawn.health.RemoveHediff(parent);
            }
            Log.Message(Pawn.kindDef?.label);
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
            Log.Message(Pawn.kindDef?.label);
            if (Props.temporary && Pawn.Faction != oldFaction)
                Pawn.SetFaction(oldFaction);
            Log.Message(Pawn.kindDef?.label);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref oldFaction, "oldFaction");
        }
    }
}
