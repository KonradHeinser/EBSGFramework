using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_AttachMote : HediffComp
    {
        HediffCompProperties_AttachMote Props => (HediffCompProperties_AttachMote)props;

        private Mote mote;

        private float Brightness
        {
            get
            {
                if (!Props.brightnessBySeverity) return 1f;
                return Mathf.Max(parent.Severity, 1);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn.Spawned && (!Pawn.Downed || Props.displayWhileDowned))
            {
                if (mote == null || mote.Destroyed)
                {
                    mote = MoteMaker.MakeAttachedOverlay(Pawn, Props.moteDef, Vector3.zero);
                    mote.link1.rotateWithTarget = true;
                }

                mote.instanceColor = new Color(Props.color.r, Props.color.g, Props.color.b, Brightness);
                mote.Maintain();
            }
        }
    }
}
