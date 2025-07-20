using RimWorld;
using Verse;
using System;

namespace EBSGFramework
{
    public class CompAbilityEffect_DamageOverTime : CompAbilityEffect
    {
        public new CompProperties_AbilityDamageOverTime Props => (CompProperties_AbilityDamageOverTime)props;

        private Pawn Caster => parent.pawn;

        private int? tick;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            tick = Math.Min(Props.initialTick, Props.tickInterval);
        }
        /* Commented out until Ludeon fixes CompTickInterval
        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);

            if (!parent.Casting) return;
            if (tick == null)
                tick = Math.Min(Props.initialTick, Props.tickInterval);

            if (tick >= Props.tickInterval)
            {
                tick -= Props.tickInterval;
                Thing target = (Caster.stances.curStance as Stance_Busy).focusTarg.Thing;
                BodyPartRecord hitPart = null;
                if (!Props.bodyParts.NullOrEmpty() && target is Pawn t)
                    hitPart = t.GetSemiRandomPartFromList(Props.bodyParts);
                target.TakeDamage(new DamageInfo(Props.damage, Props.damageAmount, Props.armorPenetration, -1, parent.pawn, hitPart, spawnFilth: Props.createFilth));
            }
            else
                tick += delta;
        }*/

        public override void CompTick()
        {
            base.CompTick();

            if (!parent.Casting) return;
            if (tick == null)
                tick = Math.Min(Props.initialTick, Props.tickInterval);

            if (tick >= Props.tickInterval)
            {
                tick -= Props.tickInterval;
                Thing target = (Caster.stances.curStance as Stance_Busy).focusTarg.Thing;
                BodyPartRecord hitPart = null;
                if (!Props.bodyParts.NullOrEmpty() && target is Pawn t)
                    hitPart = t.GetSemiRandomPartFromList(Props.bodyParts);
                target.TakeDamage(new DamageInfo(Props.damage, Props.damageAmount, Props.armorPenetration, -1, parent.pawn, hitPart, spawnFilth: Props.createFilth));
            }
            else
                tick++;
        }

        public void Interrupted(Pawn target)
        {
            tick = Math.Min(Props.initialTick, Props.tickInterval);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref tick, "tick", null);
        }
    }
}
