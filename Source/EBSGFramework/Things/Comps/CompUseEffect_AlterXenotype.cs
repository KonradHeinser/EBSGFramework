using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompUseEffect_AlterXenotype : CompUseEffect
    {
        public CompProperties_UseEffectAlterXenotype Props => (CompProperties_UseEffectAlterXenotype)props;

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (Props.prerequisites?.ValidPawn(p) == false)
            {
                string failMessage = Props.prerequisites.notMetString?.TranslateOrFormat();
                if (failMessage != null)
                    return failMessage;
                return AcceptanceReport.WasRejected;
            }
            return base.CanBeUsedBy(p);
        }

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            usedBy.AlterXenotype(Props.xenotypes, Props.filth, Props.filthCount, Props.setXenotype, Props.sendMessage);
        }
    }
}
