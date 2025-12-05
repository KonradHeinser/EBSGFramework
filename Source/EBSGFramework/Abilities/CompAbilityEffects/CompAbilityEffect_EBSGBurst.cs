using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_EBSGBurst : CompAbilityEffect
    {
        public new CompProperties_EBSGBurst Props => (CompProperties_EBSGBurst)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            Props.explosion.DoExplosion(parent.pawn, target, parent.pawn.MapHeld);
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            float radius = Props.explosion.radius;
            if (Props.explosion.statRadius != null && parent.pawn.StatOrOne(Props.explosion.statRadius, StatRequirement.Always, 60) >= 0) 
                radius = parent.pawn.StatOrOne(Props.explosion.statRadius, StatRequirement.Always, 60);

            GenDraw.DrawFieldEdges(EBSGUtilities.AffectedCells(parent.pawn, parent.pawn.Map, parent.pawn, radius).ToList(), Valid(target) ? Color.white : Color.red);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return target.Pawn?.TargetCurrentlyAimingAt == parent.pawn;
        }
    }
}
