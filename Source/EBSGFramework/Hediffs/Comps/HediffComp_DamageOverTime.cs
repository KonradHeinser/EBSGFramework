using Verse;

namespace EBSGFramework
{
    public class HediffComp_DamageOverTime : HediffComp
    {
        public HediffCompProperties_DamageOverTime Props => (HediffCompProperties_DamageOverTime)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick(Props.tickInterval))
            {
                BodyPartRecord hitPart = null;
                if (Props.damageAttachedPart && parent.Part != null) hitPart = parent.Part;
                else if (!Props.bodyParts.NullOrEmpty()) hitPart = Pawn.GetSemiRandomPartFromList(Props.bodyParts);

                var amount = Props.damageAmount > 0 ? Props.damageAmount : Props.damage.defaultDamage;
                var armorPenetration = Props.armorPenetration > 0 ? Props.armorPenetration : Props.damage.defaultArmorPenetration;

                Pawn.TakeDamage(new DamageInfo(Props.damage, amount, armorPenetration, 
                    hitPart: hitPart, spawnFilth: Props.createFilth));
            }
        }
    }
}
