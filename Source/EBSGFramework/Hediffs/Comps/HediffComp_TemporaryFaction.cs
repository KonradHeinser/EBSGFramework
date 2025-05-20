using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace EBSGFramework
{
    public class HediffComp_TemporaryFaction : HediffComp
    {
        public HediffCompProperties_TemporaryFaction Props => (HediffCompProperties_TemporaryFaction)props;

        private Faction oldFaction = null;

        private PawnKindDef oldKindDef = null;

        private HediffWithTarget ParentWithTarget => parent as HediffWithTarget;

        private Pawn ParentTarget => ParentWithTarget?.target as Pawn;

        public Faction faction = null;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            oldFaction = Pawn.Faction;
            oldKindDef = Pawn.kindDef;

            if (Props.useStatic)
                faction = Find.FactionManager.FirstFactionOfDef(Props.staticFaction);
            else if (ParentTarget != null)
            {
                faction = ParentTarget.Faction;
                Pawn.GetLord()?.RemovePawn(Pawn);
                Lord lord = ParentTarget.GetLord();
                lord?.AddPawn(Pawn);
            }
            else
            {
                Log.Error($"{Def} doesn't use static factions, but also doesn't appear to be a HediffWithTarget. No faction can be set, and this hediff will be removed to avoid more errors.");
                Pawn.health.RemoveHediff(parent);
                return;
            }

            if (faction != Pawn.Faction)
                Pawn.SetFaction(faction, ParentTarget);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn.IsHashIntervalTick(60) && !Props.useStatic && Pawn.Faction != ParentTarget.Faction)
            {
                faction = ParentTarget.Faction;
                Pawn.SetFaction(faction, ParentTarget);
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            Pawn.ChangeKind(oldKindDef);
            if (Props.temporary)
            {
                bool flag = true;
                var otherHediffs = new List<Hediff>(Pawn.health.hediffSet.hediffs);
                for (int i = otherHediffs.Count; i > 0; i--)
                {
                    if (otherHediffs[i - 1] == parent)
                        continue;

                    HediffComp_TemporaryFaction tempComp = otherHediffs[i - 1].TryGetComp<HediffComp_TemporaryFaction>();
                    if (tempComp != null)
                    {
                        if (Pawn.Faction != tempComp.faction)
                            Pawn.SetFaction(tempComp.faction, tempComp.ParentTarget);
                        flag = false;
                        break;
                    }
                }
                if (flag && Pawn.Faction != oldFaction)
                    Pawn.SetFaction(oldFaction);

                if (!Props.useStatic)
                    Pawn.GetLord()?.RemovePawn(Pawn);
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref oldFaction, "oldFaction");
            Scribe_Defs.Look(ref oldKindDef, "oldKindDef");
        }
    }
}
