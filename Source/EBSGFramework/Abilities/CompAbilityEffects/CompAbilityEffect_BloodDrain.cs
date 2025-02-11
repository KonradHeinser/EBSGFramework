using System;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompAbilityEffect_BloodDrain : CompAbilityEffect_BloodfeederBite
    {
        public new CompProperties_AbilityBloodDrain Props => (CompProperties_AbilityBloodDrain)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn pawn = target.Pawn;
            Pawn caster = parent.pawn;

            if (pawn != null && (!Props.psychic || pawn.StatOrOne(StatDefOf.PsychicSensitivity) > 0))
            {
                if (pawn != caster || Props.damageSelf == true)
                {
                    SanguophageUtility.DoBite(parent.pawn, pawn, Props.hemogenGain, Props.nutritionGain, Props.targetBloodLoss, Props.resistanceGain, Props.bloodFilthToSpawnRange, Props.thoughtDefToGiveTarget, Props.opinionThoughtDefToGiveTarget);
                }
                Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodfeederMark);
                if (firstHediffOfDef != null)
                {
                    pawn.health.RemoveHediff(firstHediffOfDef);
                }
                if (Props.replacementHediff != null)
                {
                    pawn.health.AddHediff(Props.replacementHediff, ExecutionUtility.ExecuteCutPart(pawn));
                }
                if (Props.hediffToSelf != null)
                {
                    caster.health.AddHediff(Props.hediffToSelf);
                }
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn pawn = target.Pawn;
            if (pawn == null || (Props.psychic && pawn.StatOrOne(StatDefOf.PsychicSensitivity) <= 0))
            {
                return false;
            }
            if (!Props.ignoreResistance)
            {
                if (pawn.Faction != null && !pawn.IsSlaveOfColony && !pawn.IsPrisonerOfColony)
                {
                    if (pawn.Faction.HostileTo(parent.pawn.Faction))
                    {
                        if (!pawn.Downed)
                        {
                            if (throwMessages)
                                Messages.Message("MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY")), pawn, MessageTypeDefOf.RejectInput, historical: false);
                            return false;
                        }
                    }
                    else if (pawn.IsQuestLodger() || pawn.Faction != parent.pawn.Faction)
                    {
                        if (throwMessages)
                            Messages.Message("MessageCannotUseOnOtherFactions".Translate(parent.def.Named("ABILITY")), pawn, MessageTypeDefOf.RejectInput, historical: false);
                        return false;
                    }
                }
                if (pawn.IsWildMan() && !pawn.IsPrisonerOfColony && !pawn.Downed)
                {
                    if (throwMessages)
                        Messages.Message("MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY")), pawn, MessageTypeDefOf.RejectInput, historical: false);
                    return false;
                }
                if (pawn.InMentalState)
                {
                    if (throwMessages)
                        Messages.Message("MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY")), pawn, MessageTypeDefOf.RejectInput, historical: false);
                    return false;
                }
            }
            return true;
        }

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            Pawn pawn = target.Pawn;
            if (!Props.ignoreResistance && pawn != null)
            {
                string text = null;
                if (pawn.HostileTo(parent.pawn) && !pawn.Downed)
                    text += "MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY"));

                float num = BloodlossAfterBite(pawn);
                if (num >= HediffDefOf.BloodLoss.lethalSeverity)
                {
                    if (!text.NullOrEmpty())
                        text += "\n";

                    text += "WillKill".Translate();
                }
                else if (HediffDefOf.BloodLoss.stages[HediffDefOf.BloodLoss.StageAtSeverity(num)].lifeThreatening)
                {
                    if (!text.NullOrEmpty())
                        text += "\n";

                    text += "WillCauseSeriousBloodloss".Translate();
                }
                return text;
            }
            return null;
        }

        private float BloodlossAfterBite(Pawn target)
        {
            if (target.Dead || !target.RaceProps.IsFlesh)
            {
                return 0f;
            }
            float num = Props.targetBloodLoss;
            Hediff firstHediffOfDef = target.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            if (firstHediffOfDef != null)
                num += firstHediffOfDef.Severity;

            return num;
        }
    }
}
