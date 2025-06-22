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
                if (!Props.brightnessBySeverity) return Props.staticBrightness;
                return Mathf.Min(parent.Severity, 1);
            }
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (Pawn.Spawned && (Props.displayWhileDowned || !Pawn.Downed))
            {
                if (mote == null || mote.Destroyed)
                {
                    mote = MoteMaker.MakeAttachedOverlay(Pawn, Props.moteDef, Vector3.zero);
                    mote.link1.rotateWithTarget = Props.rotateWithPawn;
                }

                if (Props.scaleMoteWithSize)
                    mote.Scale = Pawn.BodySize;
                mote.instanceColor = new Color(Props.color.r, Props.color.g, Props.color.b, Brightness);
                mote.Maintain();
            }
        }
    }
}
