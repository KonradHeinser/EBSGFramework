using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using static HarmonyLib.Code;

namespace EBSGFramework
{
    [StaticConstructorOnStartup]
    public class Gizmo_ShieldStatus : Gizmo
    {
        public CompShieldEquipment shieldComp;

        public static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
        public static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_ShieldStatus(CompShieldEquipment comp) 
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
            Widgets.Label(textRect, shieldComp.Props.gizmoTitle ?? (shieldComp.parent.def.IsApparel ? shieldComp.parent.LabelCap : "ShieldInbuilt".Translate().Resolve()));
            Rect barRect = drawRect;
            barRect.yMin = drawRect.y + drawRect.height / 2f;
            float num = shieldComp.energy / Mathf.Max(1f, shieldComp.parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax));
            Widgets.FillableBar(barRect, num, FullShieldBarTex, EmptyShieldBarTex, false);
            Text.Font = GameFont.Small;
            Widgets.Label(barRect, (shieldComp.energy * 100f).ToString("F0") + " / " + (shieldComp.parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax) * 100f).ToString("F0"));
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(drawRect, shieldComp.Props.gizmoTip ?? "ShieldPersonalTip".Translate());
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
