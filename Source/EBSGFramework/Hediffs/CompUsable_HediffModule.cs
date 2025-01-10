using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class CompUsable_HediffModule : CompUsable
    {
        private Texture2D icon;

        private Color? iconColor;
        public Texture2D Icon
        {
            get
            {
                if (icon == null && Props.floatMenuFactionIcon != null)
                    icon = Find.FactionManager.FirstFactionOfDef(Props.floatMenuFactionIcon)?.def?.FactionIcon;

                return icon;
            }
        }

        public Color IconColor
        {
            get
            {
                if (!iconColor.HasValue && Props.floatMenuFactionIcon != null)
                    iconColor = Find.FactionManager.FirstFactionOfDef(Props.floatMenuFactionIcon)?.Color;

                return iconColor ?? Color.white;
            }
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p, bool forced = false, bool ignoreReserveAndReachable = false)
        {
            AcceptanceReport result = base.CanBeUsedBy(p, forced, ignoreReserveAndReachable);
            if (!result.Accepted)
                return result;

            CompUseEffect_HediffModule parentComp = parent.TryGetComp<CompUseEffect_HediffModule>();

            if (p.health?.hediffSet?.hediffs.NullOrEmpty() == false)
                foreach (Hediff h in p.health.hediffSet.hediffs)
                    if (h is HediffWithComps hediff && !hediff.comps.NullOrEmpty())
                        foreach (HediffComp c in hediff.comps)
                            if (c is HediffComp_Modular comp && !comp.GetOpenSlots(parentComp).NullOrEmpty())
                                return true;
            Log.Message("A");
            return "EBSG_RejectApply".Translate() + ": " + "EBSG_NoSlots".Translate();
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            string label = "EBSG_Install".Translate(parent.Label);

            AcceptanceReport report = CanBeUsedBy(pawn, true, Props.ignoreOtherReservations);
            if (!report.Accepted)
            {
                yield return new FloatMenuOption(label + ((report.Reason != null) ? (" (" + report.Reason + ")") : ""), null);
                yield break;
            }

            if (!pawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
            {
                yield return new FloatMenuOption(label + " (" + "NoPath".Translate() + ")", null);
                yield break;
            }

            if (!pawn.CanReserve(parent, 1, -1, null, Props.ignoreOtherReservations))
            {
                Pawn reservedPawn = pawn.Map.reservationManager.FirstRespectedReserver(parent, pawn, null) ?? pawn.Map.physicalInteractionReservationManager.FirstReserverOf(parent);
                string newLabel = label;

                if (reservedPawn != null)
                {
                    newLabel += " (" + "ReservedBy".Translate(reservedPawn.LabelShort, reservedPawn) + ")";
                }
                else
                {
                    newLabel += " (" + "Reserved".Translate() + ")";
                }

                yield return new FloatMenuOption(newLabel, null);
                yield break;
            }

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                yield return new FloatMenuOption(label + " (" + "Incapable".Translate().CapitalizeFirst() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }

            if (Props.userMustHaveHediff != null && !pawn.HasHediff(Props.userMustHaveHediff))
            {
                yield return new FloatMenuOption(label + " (" + "MustHaveHediff".Translate(Props.userMustHaveHediff) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }

            CompUseEffect_HediffModule parentComp = parent.TryGetComp<CompUseEffect_HediffModule>();
            List<HediffComp_Modular> workingModulars = new List<HediffComp_Modular>();
            bool foundSlots = false;

            if (pawn.health?.hediffSet?.hediffs.NullOrEmpty() == false)
            {
                foreach (Hediff h in pawn.health.hediffSet.hediffs)
                    if (h is HediffWithComps hediff && !hediff.comps.NullOrEmpty())
                    {
                        foreach (HediffComp c in  hediff.comps)
                            if (c is HediffComp_Modular comp)
                            {
                                List<ModuleSlot> slots = comp.GetOpenSlots(parentComp);

                                if (!slots.NullOrEmpty())
                                    foreach (ModuleSlot slot in slots)
                                    {
                                        HediffComp_Modular duplicateComp = comp;

                                        Action action = delegate ()
                                        {
                                            if (pawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, Props.ignoreOtherReservations))
                                                StartModuleJob(pawn, duplicateComp, parentComp, slot, Props.ignoreOtherReservations);
                                        };

                                        string labeled = "EBSG_InstallIn".Translate(parent.Label, hediff.Label, 
                                            hediff.Part?.Label != null ? $"{hediff.Part.Label}, {slot.slotName}" : slot.slotName);
                                        foundSlots = true;

                                        FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(labeled, action, Icon, IconColor, Props.floatMenuOptionPriority), pawn, parent);
                                        yield return floatMenuOption;
                                    }
                            }
                    }
            }
            if (!foundSlots)
                yield return new FloatMenuOption("EBSG_NoSlots".Translate(), null);

            yield break;
        }

        public virtual void StartModuleJob(Pawn pawn, HediffComp_Modular modular, CompUseEffect_HediffModule parentComp, ModuleSlot slot, bool forced = false)
        {
            parentComp.ownerHediff = modular.parent;
            parentComp.usedSlot = slot.slotID;
            TryStartUseJob(pawn, null, forced);
        }
    }
}
