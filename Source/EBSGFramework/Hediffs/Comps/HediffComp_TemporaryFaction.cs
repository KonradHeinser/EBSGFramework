using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace EBSGFramework
{
    public class HediffComp_TemporaryFaction : HediffComp
    {
        public HediffCompProperties_TemporaryFaction Props => (HediffCompProperties_TemporaryFaction)props;

        private Faction oldFaction;

        private PawnKindDef oldKindDef;

        private HediffWithTarget ParentWithTarget => parent as HediffWithTarget;

        private Pawn ParentTarget => ParentWithTarget?.target as Pawn;

        public Faction faction;

        public bool GetFaction(bool removing = false)
        {
            if (Props.useStatic)
                faction = Find.FactionManager.FirstFactionOfDef(Props.staticFaction);
            else if (ParentTarget != null)
                faction = ParentTarget.Faction;
            else
            {
                Log.Error($"{Def} doesn't use a valid static faction, but also doesn't appear to be a HediffWithTarget. No faction can be set, and this hediff will be removed to avoid more errors.");
                if (!removing)
                    Pawn.health.RemoveHediff(parent);
                return false;
            }
            return true;
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            oldFaction = Pawn.Faction;
            oldKindDef = Pawn.kindDef;

            if (GetFaction() && faction != Pawn.Faction)
            {
                Pawn.SetFaction(faction, ParentTarget);
                Pawn.GetLord()?.RemovePawn(Pawn);
                Lord lord = ParentTarget.GetLord();
                lord?.AddPawn(Pawn);
            }
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            // This checks to make sure this is the hediff that is supposed to be messing with stuff right now
            var otherHediffs = new List<Hediff>(Pawn.health.hediffSet.hediffs);
            for (int i = otherHediffs.Count - 1; i >= 0; i--)
            {
                if (otherHediffs[i] == parent)
                    break;

                if (otherHediffs[i].TryGetComp<HediffComp_TemporaryFaction>() != null)
                    return;
            }

            if (GetFaction() && Pawn.Faction != faction)
            {
                Pawn.SetFaction(faction, ParentTarget);
                Pawn.GetLord()?.RemovePawn(Pawn);
                if (!Props.useStatic)
                    ParentTarget.GetLord()?.AddPawn(Pawn);
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            Pawn.ChangeKind(oldKindDef);
            if (Props.temporary)
            {
                // This checks if there's another version of this comp that should be taking over
                var otherHediffs = new List<Hediff>(Pawn.health.hediffSet.hediffs);
                for (int i = otherHediffs.Count - 1; i > 0; i--)
                {
                    if (otherHediffs[i] == parent)
                        continue;

                    var tempComp = otherHediffs[i].TryGetComp<HediffComp_TemporaryFaction>();
                    if (tempComp != null && tempComp.GetFaction(true))
                    {
                        if (Pawn.Faction != tempComp.faction)
                        {
                            Pawn.SetFaction(tempComp.faction, tempComp.ParentTarget);
                            if (!Props.useStatic)
                                Pawn.GetLord()?.RemovePawn(Pawn);
                        }
                        return;
                    }
                }
                if (GetFaction(true) && Pawn.Faction != oldFaction && Pawn.Faction == faction)
                {
                    Pawn.SetFaction(oldFaction);
                    if (!Props.useStatic)
                        Pawn.GetLord()?.RemovePawn(Pawn);
                }
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
