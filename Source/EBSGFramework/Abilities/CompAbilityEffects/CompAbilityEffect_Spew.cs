using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class CompAbilityEffect_Spew : CompAbilityEffect
    {
        private new CompProperties_AbilitySpew Props => (CompProperties_AbilitySpew)props;

        private Pawn Caster => parent.pawn;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            GenExplosion.DoExplosion(target.Cell, parent.pawn.MapHeld, 0f, Props.damage, Caster, Props.amount, -1f, null, null, null, null,
                Props.filthDef, 1f, 1, null, false, null, 0f, 1, Props.fireChance, false, null, null, null, false, 0.6f, 0f, false, null, 1f, null, AffectedCells(target));
            base.Apply(target, dest);
        }

        public override IEnumerable<PreCastAction> GetPreCastActions()
        {
            if (Props.effecterDef != null)
            {
                yield return new PreCastAction
                {
                    action = delegate (LocalTargetInfo a, LocalTargetInfo b)
                    {
                        parent.AddEffecterToMaintain(Props.effecterDef.Spawn(parent.pawn.Position, a.Cell, parent.pawn.Map), Caster.Position, a.Cell, 17, Caster.MapHeld);
                    },
                    ticksAwayFromCast = 17
                };
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawFieldEdges(AffectedCells(target));
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (Caster.Faction != null)
            {
                foreach (IntVec3 item in AffectedCells(target))
                {
                    List<Thing> thingList = item.GetThingList(Caster.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        if (thingList[i].Faction == Caster.Faction)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private List<IntVec3> AffectedCells(LocalTargetInfo target)
        {
            return EBSGUtilities.GetCone(target, Caster, 0.1f, Props.range, Props.angle, Props.angle);
        }
    }
}
