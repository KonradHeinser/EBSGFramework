using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class FlyPawnArriving : DropPodIncoming
    {
        private Pawn pawn;

        private bool checkedPawn = false;

        protected Pawn Pawn
        {
            get
            {
                if (pawn == null && !checkedPawn)
                {
                    checkedPawn = true;
                    var things = Contents.GetDirectlyHeldThings();
                    if (!things.NullOrEmpty())
                        foreach (var thing in things)
                            if (thing is Pawn p && !p.DeadOrDowned && !p.IsPrisoner)
                            {
                                pawn = p;
                                break;
                            }
                }

                return pawn;
            }
        }

        public override Graphic Graphic => Pawn?.Graphic ?? base.Graphic;

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (Pawn != null)
            {
                if (def.skyfaller.xPositionCurve != null)
                    drawLoc.x += def.skyfaller.xPositionCurve.Evaluate(TimeInAnimation);
                if (def.skyfaller.zPositionCurve != null)
                    drawLoc.z += def.skyfaller.zPositionCurve.Evaluate(TimeInAnimation);
                Pawn.Drawer.renderer.RenderPawnAt(drawLoc, Pawn.Rotation);
                DrawDropSpotShadow();
            }
            else
                base.DrawAt(drawLoc, flip);
        }
    }
}
