using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;

namespace EBSGFramework
{
    public class CompUseEffect_HediffModule : CompUseEffect
    {
        public CompProperties_UseEffectHediffModule Props => props as CompProperties_UseEffectHediffModule;

        public string usedSlot;
        public HediffComp_Modular ownerComp;
        public HediffWithComps ownerHediff;
        public List<HediffComp> linkedComps = new List<HediffComp>();
        public List<Hediff> linkedHediffs = new List<Hediff>();

        public HediffComp_Modular OwnerComp
        {
            get
            {
                if (ownerComp != null)
                {
                    return ownerComp;
                }

                if (ownerHediff == null)
                {
                    return null;
                }

                ownerComp = ownerHediff.TryGetComp<HediffComp_Modular>();

                return ownerComp;
            }
        }

        public ModuleSlot GetSlot
        {
            get
            {
                if (OwnerComp?.Props.slots.NullOrEmpty() != false)
                    return null;

                foreach (ModuleSlot slot in OwnerComp.Props.slots)
                    if (slot.slotID == usedSlot)
                        return slot;

                return null;
            }
        }

        public float RemainingCapacityInCurrentSlot
        {
            get
            {
                ModuleSlot slot = GetSlot;
                float num = slot?.capacity ?? 0;

                if (num > 0)
                    foreach (ThingWithComps thing in OwnerComp.moduleHolder)
                    {
                        CompUseEffect_HediffModule moduleComp = thing.TryGetComp<CompUseEffect_HediffModule>();

                        if (moduleComp.usedSlot == slot.slotID)
                            num -= moduleComp.Props.requiredCapacity;
                    }

                return num;
            }
        }

        public void Install(HediffComp_Modular holder)
        {
            Props.installSound?.PlayOneShot(SoundInfo.InMap(holder.Pawn, MaintenanceType.None));

            GenerateComps(holder);

            if (!Props.hediffs.NullOrEmpty())
                foreach (HediffDef hediffDef in Props.hediffs)
                {
                    Hediff hediff = holder.Pawn.CreateComplexHediff(hediffDef.initialSeverity, hediffDef, null, holder.parent.Part);
                    holder.Pawn.health.AddHediff(hediff, holder.parent.Part);
                    linkedHediffs.Add(hediff);
                }
        }

        public bool Remove(HediffComp_Modular holder)
        {
            if (!Props.ejectable)
                return false;

            if (Props.requiredCapacity < 0 && RemainingCapacityInCurrentSlot + Props.requiredCapacity < 0)
                return false;

            Props.ejectSound?.PlayOneShot(SoundInfo.InMap(holder.Pawn, MaintenanceType.None));

            if (!linkedComps.NullOrEmpty())
                foreach (HediffComp comp in linkedComps)
                    holder.parent.comps.Remove(comp);

            holder.Pawn.RemoveAllOfHediffs(linkedHediffs);

            linkedComps.Clear();
            linkedHediffs.Clear();
            ownerComp = null;
            ownerHediff = null;

            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref ownerHediff, "ownerHediff");
            Scribe_Values.Look(ref usedSlot, "usedSlot");
            Scribe_Collections.Look(ref linkedHediffs, "linkedHediffs", LookMode.Reference);
        }

        public void GenerateComps(HediffComp_Modular holder)
        {
            if (!Props.comps.NullOrEmpty())
                foreach (HediffCompProperties comp in Props.comps)
                {
                    HediffComp hediffComp = null;
                    try
                    {
                        hediffComp = (HediffComp)Activator.CreateInstance(comp.compClass);
                        hediffComp.props = comp;
                        hediffComp.parent = holder.parent;
                        holder.parent.comps.Add(hediffComp);
                        linkedComps.Add(hediffComp);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Modular HediffComp could not instantiate or initialize a HediffComp: " + ex);
                        holder.parent.comps.Remove(hediffComp);
                    }
                }
        }

        public HediffStage ModifyStage(int stageIndex, HediffStage stage)
        {
            if (Props.stageOverlays.NullOrEmpty())
                return stage;

            if (Props.stageOverlays.Count == 1)
                return Props.stageOverlays[0].ModifyHediffStage(stage);

            return Props.stageOverlays[stageIndex].ModifyHediffStage(stage);
        }

        public override void DoEffect(Pawn user)
        {
            if (OwnerComp == null || OwnerComp.parent == null || OwnerComp.Pawn == null)
            {
                ownerComp = null;
                ownerHediff = null;
                return;
            }

            OwnerComp.InstallModule(parent);
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            AcceptanceReport result = base.CanBeUsedBy(p);
            if (!result.Accepted)
                return result;

            if (p.health?.hediffSet?.hediffs.NullOrEmpty() == false)
                foreach (Hediff hediff in p.health.hediffSet.hediffs)
                    if (hediff is HediffWithComps hediffWithComps)
                        foreach (HediffComp comp in hediffWithComps.comps)
                            if (comp is HediffComp_Modular compModular)
                            {
                                if (Props.prerequisites?.ValidPawn(p, hediff.Part) == false)
                                    break;

                                return true;
                            }
            
            return "EBSG_RejectApply".Translate() + ": " + "EBSG_NoSlots".Translate();
        }
    }
}
