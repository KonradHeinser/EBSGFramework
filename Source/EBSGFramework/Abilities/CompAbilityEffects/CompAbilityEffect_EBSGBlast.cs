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

            Props.explosion.DoExplosion(parent.pawn, target, parent.pawn.MapHeld);
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            float radius = Props.explosion.radius; 
            if (Props.explosion.statRadius != null && parent.pawn.StatOrOne(Props.explosion.statRadius) >= 0) radius = parent.pawn.GetStatValue(Props.explosion.statRadius);

            GenDraw.DrawFieldEdges(EBSGUtilities.AffectedCells(target, parent.pawn.Map, parent.pawn, radius).ToList(), Valid(target) ? Color.white : Color.red);
        }
    }
}
