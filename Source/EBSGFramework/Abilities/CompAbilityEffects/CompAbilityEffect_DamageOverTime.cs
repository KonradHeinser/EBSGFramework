using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_DamageOverTime : CompAbilityEffect
    {
        public new CompProperties_AbilityDamageOverTime Props => (CompProperties_AbilityDamageOverTime)props;

        private Pawn Caster => parent.pawn;

        private int tick = 0;

        public override void CompTick()
        {
            if (!parent.Casting) return;
            base.CompTick();
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
            target?.stances?.stunner?.StopStun();
            tick = 0;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref tick, "tick", 0);
        }
    }
}
