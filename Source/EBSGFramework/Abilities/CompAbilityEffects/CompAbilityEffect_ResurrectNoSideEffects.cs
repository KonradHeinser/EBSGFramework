using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_ResurrectNoSideEffects : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn t = (target.Thing as Corpse)?.InnerPawn;
            if (t != null)
            {
                t.TryToRevivePawn();
                Messages.Message("MessagePawnResurrected".Translate(t), t, MessageTypeDefOf.PositiveEvent);
                MoteMaker.MakeAttachedOverlay(t, ThingDefOf.Mote_ResurrectFlash, Vector3.zero);
                if (MechanitorUtility.CanControlMech(parent.pawn, t) && parent.pawn.mechanitor.CanOverseeSubject(t))
                    parent.pawn.relations.AddDirectRelation(PawnRelationDefOf.Overseer, t);

            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true) && base.CanApplyOn(target, dest);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!target.HasThing || !(target.Thing is Corpse corpse))
                return false;

            if (corpse.GetRotStage() == RotStage.Dessicated)
            {
                if (throwMessages)
                    Messages.Message("MessageCannotResurrectDessicatedCorpse".Translate(), corpse, MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }

            return base.Valid(target, throwMessages);
        }
    }
}
