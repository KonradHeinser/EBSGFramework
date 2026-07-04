using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompUseEffect_ReplaceImplant: CompUseEffect_InstallImplant
    {
        public new CompProperties_UseEffectReplaceImplant Props => (CompProperties_UseEffectReplaceImplant)props;
        
        public override void DoEffect(Pawn pawn)
        {
            if (pawn.PawnHasAnyOfHediffs(Props.replacedHediffs, out Hediff replaced, pawn.RaceProps.body.GetPartsWithDef(Props.hediffDef.defaultInstallPart).FirstOrFallback()))
            {
                if (replaced.def.spawnThingOnRemoved != null)
                    GenSpawn.Spawn(replaced.def.spawnThingOnRemoved, pawn.PositionHeld, pawn.MapHeld);
                pawn.health.RemoveHediff(replaced);
            }
            base.DoEffect(pawn);
        }
    }
}