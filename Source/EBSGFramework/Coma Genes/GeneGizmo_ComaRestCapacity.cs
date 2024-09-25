using RimWorld;
using Verse;
using UnityEngine;

namespace EBSGFramework
{
    public class GeneGizmo_ComaRestCapacity : Gizmo
    {
        public GeneGizmo_ComaRestCapacity(Gene_Coma comaGene)
        {
            gene = comaGene;
            Order = -100f;
        }

        protected Gene_Coma gene;

        private const float Padding = 6f;

        private const float Width = 140f;

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect position = rect.ContractedBy(6f);
            float num = position.height / 3f;
            Widgets.DrawWindowBackground(rect);
            GUI.BeginGroup(position);
            Widgets.Label(new Rect(0f, 0f, position.width, num), gene.LabelCap);
            gene.ComaNeed?.DrawOnGUI(new Rect(0f, num, position.width, num + 2f), int.MaxValue, 2f, false, true, new Rect(0f, 0f, position.width, num * 2f), false);
            Rect rect2 = new Rect(0f, num * 2f, position.width, Text.LineHeight);
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect2, string.Format("{0}: {1} / {2}", "Buildings".Translate().CapitalizeFirst(), gene.CurrentCapacity, gene.ComaCapacity));
            Text.Anchor = TextAnchor.UpperLeft;
            if (Mouse.IsOver(rect2))
            {
                Widgets.DrawHighlight(rect2);
                TooltipHandler.TipRegion(rect2, "EBSG_ComaRestCapacityDesc".Translate() + "\n\n" + "EBSG_PawnIsConnectedToBuildings".Translate(gene.pawn.Named("PAWN"), gene.CurrentCapacity.Named("CURRENT"), gene.DeathrestCapacity.Named("MAX")));
            }
            GUI.EndGroup();
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
