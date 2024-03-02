using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityChangePerAttack : HediffComp
    {
        private HediffCompProperties_SeverityChangePerAttack Props => (HediffCompProperties_SeverityChangePerAttack)props;

        public override void Notify_PawnUsedVerb(Verb verb, LocalTargetInfo target)
        {
            if (verb.GetDamageDef() == null || verb.verbProps == null) return;
            if (verb.verbProps.IsMeleeAttack)
            {
                parent.Severity += Props.changePerMeleeAttack;
            }
            else if (verb.verbProps.Ranged) parent.Severity += Props.changePerRangedAttack;

            parent.Severity += Props.changePerAttack;
        }
    }
}
