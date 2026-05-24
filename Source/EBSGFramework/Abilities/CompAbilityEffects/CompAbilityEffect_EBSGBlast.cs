using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_EBSGBlast : CompAbilityEffect
    {
        public new CompProperties_EBSGBlast Props => (CompProperties_EBSGBlast)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (Props.successChance?.Success(parent.pawn, target.Thing) == false)
                return;
                
            Props.explosion.DoExplosion(parent.pawn, target, parent.pawn.MapHeld);
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            var radius = Props.explosion.radius;
            if (Props.explosion.statRadius != null)
            {
                var stat = parent.pawn.StatOrOne(Props.explosion.statRadius, StatRequirement.Always, 60);
                if (stat >= 0)
                    radius = stat;
            }

            GenDraw.DrawFieldEdges(EBSGUtilities.AffectedCells(target, parent.pawn.Map, parent.pawn, radius).ToList(), Valid(target) ? Color.white : Color.red);
        }
    }
}
