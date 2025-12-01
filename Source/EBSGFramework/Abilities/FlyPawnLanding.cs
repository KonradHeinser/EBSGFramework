using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class FlyPawnLanding : ActiveTransporter
    {
        private Pawn pawn;

        private bool checkedPawn;

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

        public override void PostMake()
        {
            base.PostMake();
            if (Contents != null)
                Contents.openDelay = 10;
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (Pawn?.Drawer?.renderer != null)
                Pawn.Drawer.renderer.RenderPawnAt(drawLoc, Pawn.Rotation);
            else
                base.DrawAt(drawLoc, flip);
        }
    }
}