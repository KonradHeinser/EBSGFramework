using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_TriggerGameCondition : CompAbilityEffect
    {
        public new CompProperties_TriggerGameCondition Props => (CompProperties_TriggerGameCondition)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (Props.gameCondition != null)
                parent.pawn.Map.GameConditionManager.RegisterCondition(GameConditionMaker.MakeCondition(Props.gameCondition, Props.ticks));
            if (!Props.gameConditions.NullOrEmpty())
            {
                foreach (ConditionDuration condition in Props.gameConditions)
                {
                    if (!EBSGUtilities.ConditionOrExclusiveIsActive(condition.condition, parent.pawn.Map))
                    {
                        parent.pawn.Map.GameConditionManager.RegisterCondition(GameConditionMaker.MakeCondition(condition.condition, condition.ticks));
                        if (Props.onlyFirst) break;
                    }
                }
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn caster = parent.pawn;

            bool flag = false;

            if (!base.Valid(target, throwMessages) || caster.Map == null) return false;

            if (Props.gameCondition != null)
            {
                if (!EBSGUtilities.ConditionOrExclusiveIsActive(Props.gameCondition, caster.Map))
                {
                    flag = true;
                }
            }

            if (!Props.gameConditions.NullOrEmpty())
            {
                foreach (ConditionDuration condition in Props.gameConditions)
                {
                    if (Props.onlyFirst)
                    {
                        if (EBSGUtilities.ConditionOrExclusiveIsActive(condition.condition, caster.Map)) continue;
                        flag = true;
                        break;
                    }
                    if (EBSGUtilities.ConditionOrExclusiveIsActive(condition.condition, caster.Map))
                    {
                        if (!Props.skipExisting)
                        {
                            flag = false;
                            break;
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }

            if (!flag && throwMessages)
                Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityGameCondition".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
            return flag;
        }
    }
}
