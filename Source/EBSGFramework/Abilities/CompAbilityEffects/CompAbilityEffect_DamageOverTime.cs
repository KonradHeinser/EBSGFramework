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

        public override void CompTick()
        {
            if (!parent.Casting) return;
            base.CompTick();
            if (tick == null)
                tick = Math.Min(Props.initialTick, Props.tickInterval);
            else
                tick++;
            if (tick == Props.tickInterval)
            {
                tick = 0;
                Thing target = (Caster.stances.curStance as Stance_Busy).focusTarg.Thing;
                BodyPartRecord hitPart = null;
                if (!Props.bodyParts.NullOrEmpty() && target is Pawn t) 
                    hitPart = t.GetSemiRandomPartFromList(Props.bodyParts);
                target.TakeDamage(new DamageInfo(Props.damage, Props.damageAmount, Props.armorPenetration, hitPart: hitPart, spawnFilth: Props.createFilth));
            }
        }

        public void Interrupted(Pawn target)
        {
            tick = null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref tick, "tick", null);
        }
    }
}
