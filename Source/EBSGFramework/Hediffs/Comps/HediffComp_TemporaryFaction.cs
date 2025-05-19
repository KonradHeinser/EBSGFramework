using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_TemporaryFaction : HediffComp
    {
        public HediffCompProperties_TemporaryFaction Props => (HediffCompProperties_TemporaryFaction)props;

        private Faction oldFaction = null;

        private HediffWithTarget parentWithTarget => parent as HediffWithTarget;

        private Pawn parentTarget => parentWithTarget?.target as Pawn;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            oldFaction = Pawn.Faction;

            if (Props.useStatic)
                Pawn.SetFaction(Find.FactionManager.FirstFactionOfDef(Props.staticFaction));
            else if (parentTarget != null)
                Pawn.SetFaction(parentTarget.Faction, parentTarget);
            else
            {
                Log.Error($"{Def} doesn't use static factions, but also doesn't appear to be a HediffWithTarget. No faction can be set, and this hediff will be removed to avoid more errors.");
                Pawn.health.RemoveHediff(parent);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn.IsHashIntervalTick(60) && !Props.useStatic && Pawn.Faction != parentTarget.Faction)
                Pawn.SetFaction(parentTarget.Faction, parentTarget);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (Props.temporary && Pawn.Faction != oldFaction)
                Pawn.SetFaction(oldFaction);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref oldFaction, "oldFaction");
        }
    }
}
