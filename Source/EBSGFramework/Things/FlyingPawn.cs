using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class FlyingPawn : TravelingTransportPods
    {
        public Pawn pawn;

        public override Material Material => MaterialPool.MatFrom(new MaterialRequest(PortraitsCache.Get(pawn, new Vector2(50, 50), Rot4.South)));

        public override void Draw()
        {
            float size = Find.WorldGrid.averageTileSize * 1f;
            Vector3 normalized = DrawPos.normalized;
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(DrawPos + normalized * 0.08f, Quaternion.LookRotation(Vector3.up, normalized), new Vector3(size, 1f, size));
            int layer = WorldCameraManager.WorldLayer;
            Graphics.DrawMesh(MeshPool.plane10, matrix, Material, layer);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref pawn, "pawn");
        }
    }
}
