using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class FlyPawnArriving : ActiveTransporter
    {
        public Pawn pawn;

        public override Graphic Graphic => pawn.Graphic;

        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            pawn.Drawer.renderer.RenderPawnAt(DrawPos, Rot4.South);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref pawn, "pawn");
        }
    }
}
