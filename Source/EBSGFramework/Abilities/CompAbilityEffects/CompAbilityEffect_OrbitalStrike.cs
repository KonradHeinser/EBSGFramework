using Verse;
using RimWorld;
using UnityEngine;

namespace EBSGFramework
{
    public class CompAbilityEffect_OrbitalStrike : CompAbilityEffect
    {
        public new CompProperties_OrbitalStrike Props => (CompProperties_OrbitalStrike)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Thing thing = GenSpawn.Spawn(Props.centerMarker, target.Cell, parent.pawn.Map);

            if (thing is CustomizeableOrbitalStrike obj)
            {
                obj.explosionCount = Props.count;
                obj.bombIntervalTicks = Props.interval;
                obj.warmupTicks = Props.warmupTicks;
                obj.explosionRadiusRange = Props.explosionRadius;
                obj.impactAreaRadius = Props.explosionRadius.Average * 2;
                obj.randomFireRadius = Props.targetRadius;
                if (Props.damage != null)
                {
                    obj.damage = Props.damage;
                    obj.damageDef = Props.damage.defName;
                }
                obj.fireChance = Props.fireChance;
                if (Props.preImpactSound != null)
                    obj.preImpactSound = Props.preImpactSound;
                if (Props.projectileTexPath != null)
                    obj.projectileTexPath = Props.projectileTexPath;
                obj.projectileColor = Props.projectileColor;
                obj.instigator = parent.pawn;
                obj.StartStrike();
            }
            else
            {
                Log.Error(parent.def.defName + " is using a centerMarker that doesn't use the EBSGFramework.CustomizeableOrbitalStrike thingClass.");
            }

        }
    }
}
