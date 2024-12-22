using Verse;

namespace EBSGFramework
{
    public class HediffComp_DamageOverTime : HediffComp
    {
        public HediffCompProperties_DamageOverTime Props => (HediffCompProperties_DamageOverTime)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (Props.damage == null)
                EBSGUtilities.AddedHediffError(parent, Pawn);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick(Props.tickInterval))
            {
                BodyPartRecord hitPart = null;
                if (Props.damageAttachedPart && parent.Part != null) hitPart = parent.Part;
                else if (!Props.bodyParts.NullOrEmpty()) hitPart = Pawn.GetSemiRandomPartFromList(Props.bodyParts);

                Pawn.TakeDamage(new DamageInfo(Props.damage, Props.damageAmount, Props.armorPenetration, hitPart: hitPart, spawnFilth: Props.createFilth));
            }
        }
    }
}
