using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_DamageOnRemoval : HediffComp
    {
        HediffCompProperties_DamageOnRemoval Props => (HediffCompProperties_DamageOnRemoval)props;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (Props.neverWhenDead && Pawn.Dead) return;

            BodyPartRecord bodyPart = null;

            if (parent.Part != null)
                bodyPart = parent.Part;
            else if (!Props.bodyParts.NullOrEmpty())
                bodyPart = Pawn.GetSemiRandomPartFromList(Props.bodyParts);
            else if (Props.trulyRandomPart)
                bodyPart = Pawn.health.hediffSet.GetNotMissingParts().RandomElement();
            else
                bodyPart = Pawn.health.hediffSet.GetNotMissingParts().RandomElementByWeight((arg) => arg.def.hitPoints);

            if (bodyPart == null)
            {
                // If this is not empty, then that means all the valid parts might have just been missing. If it is empty though, then we somehow couldn't find a random part
                if (Props.bodyParts.NullOrEmpty())
                    Log.Error($"{parent.def.defName} was unable to find a valid part to damage");
                return;
            }

            var amount = Props.amount;
            var partHealth = Pawn.health.hediffSet.GetPartHealth(bodyPart);

            if (Props.minHealthRemaining > 0 && amount >= partHealth)
                amount = partHealth - Props.minHealthRemaining; /// Pawn.GetStatValue(StatDefOf.IncomingDamageFactor);

            var result = Pawn.TakeDamage(new DamageInfo(Props.damage, amount, 100, hitPart: bodyPart, spawnFilth: Props.createFilth));
        }
    }
}
