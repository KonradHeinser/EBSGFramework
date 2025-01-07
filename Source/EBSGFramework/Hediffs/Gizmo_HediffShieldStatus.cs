using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    [StaticConstructorOnStartup]
    public class Gizmo_HediffShieldStatus : Gizmo
    {
        public HediffComp_Shield shieldComp;

        public static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
        public static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_HediffShieldStatus(HediffComp_Shield comp)
        {
            Order = -100f;
            shieldComp = comp;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect backgroundRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect drawRect = backgroundRect.ContractedBy(6f);
            Widgets.DrawWindowBackground(backgroundRect);
            Rect textRect = drawRect;
            textRect.height = backgroundRect.height / 2f - 12f;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(textRect, shieldComp.Props.gizmoTitle ?? shieldComp.parent.LabelCap);
            Rect barRect = drawRect;
            barRect.yMin = drawRect.y + drawRect.height / 2f;
            float num = shieldComp.energy / Mathf.Max(1f, shieldComp.MaxEnergy);
            Widgets.FillableBar(barRect, num, FullShieldBarTex, EmptyShieldBarTex, false);
            Text.Font = GameFont.Small;
            Widgets.Label(barRect, (shieldComp.energy * 100f).ToString("F0") + " / " + (shieldComp.MaxEnergy * 100f).ToString("F0"));
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(drawRect, shieldComp.Props.gizmoTip ?? "EBSG_Shield".Translate());
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
