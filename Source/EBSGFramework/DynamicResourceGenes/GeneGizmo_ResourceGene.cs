using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EBSGFramework
{
    [StaticConstructorOnStartup]
    public class GeneGizmo_ResourceGene : GeneGizmo_Resource
    {
        private static readonly Texture2D ResourceCostTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.116f, 0.129f, 0.190f));

        private const float TotalPulsateTime = 0.85f;

        private List<Pair<IGeneResourceDrain, float>> tmpDrainGenes = new List<Pair<IGeneResourceDrain, float>>();

        public GeneGizmo_ResourceGene(Gene_Resource gene, List<IGeneResourceDrain> drainGenes, Color barColor, Color barhighlightColor)
            : base(gene, drainGenes, barColor, barhighlightColor)
        {
            draggableBar = true;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
            float num = Mathf.Repeat(Time.time, 0.85f);
            float num2 = 1f;
            if (num < 0.1f)
            {
                num2 = num / 0.1f;
            }
            else if (num >= 0.25f)
            {
                num2 = 1f - (num - 0.25f) / 0.6f;
            }
            if (((MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow)?.LastMouseoverGizmo is Command_Ability command_Ability && gene.Max != 0f)
            {
                foreach (CompAbilityEffect effectComp in command_Ability.Ability.EffectComps)
                {
                    if (effectComp is CompAbilityEffect_ResourceCost compAbilityEffect_ResourceCost && compAbilityEffect_ResourceCost.Props.mainResourceGene == gene.def && compAbilityEffect_ResourceCost.Props.resourceCost > float.Epsilon)
                    {
                        Rect rect = barRect.ContractedBy(3f);
                        float width = rect.width;
                        float num3 = gene.Value / gene.Max;
                        rect.xMax = rect.xMin + width * num3;
                        float num4 = Mathf.Min(compAbilityEffect_ResourceCost.Props.resourceCost / gene.Max, 1f);
                        rect.xMin = Mathf.Max(rect.xMin, rect.xMax - width * num4);
                        GUI.color = new Color(1f, 1f, 1f, num2 * 0.7f);
                        GenUI.DrawTextureWithMaterial(rect, ResourceCostTex, null);
                        GUI.color = Color.white;
                        break;
                    }
                }
            }
            return result;
        }

        protected override void DrawLabel(Rect labelRect, ref bool mouseOverAnyHighlightableElement)
        {
            ResourceGene resourceGene;
            if ((gene.pawn.IsColonistPlayerControlled || gene.pawn.IsPrisonerOfColony) && (resourceGene = gene as ResourceGene) != null)
            {
                labelRect.xMax -= 24f;
                Rect rect = new Rect(labelRect.xMax, labelRect.y, 24f, 24f);
                if (resourceGene.def.HasModExtension<DRGExtension>() && resourceGene.def.GetModExtension<DRGExtension>().iconThing != null) Widgets.DefIcon(rect, resourceGene.def.GetModExtension<DRGExtension>().iconThing);
                else Widgets.DefIcon(rect, ThingDefOf.HemogenPack);
                GUI.DrawTexture(new Rect(rect.center.x, rect.y, rect.width / 2f, rect.height / 2f), resourceGene.resourcePacksAllowed ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
                if (Widgets.ButtonInvisible(rect))
                {
                    resourceGene.resourcePacksAllowed = !resourceGene.resourcePacksAllowed;
                    if (resourceGene.resourcePacksAllowed)
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    }
                    else
                    {
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    }
                }
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                    string onOff = (resourceGene.resourcePacksAllowed ? "On" : "Off").Translate().ToString().UncapitalizeFirst();
                    TooltipHandler.TipRegion(rect, () => "AutoTakeResourceDesc".Translate(resourceGene.ResourceLabel.Named("RESOURCE"),  gene.pawn.Named("PAWN"), resourceGene.PostProcessValue(resourceGene.targetValue).Named("MIN"), onOff.Named("ONOFF")).Resolve(), 828267373);
                    mouseOverAnyHighlightableElement = true;
                }
            }
            base.DrawLabel(labelRect, ref mouseOverAnyHighlightableElement);
        }

        protected override string GetTooltip()
        {
            tmpDrainGenes.Clear();
            string text = $"{gene.ResourceLabel.CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor)}: {gene.ValueForDisplay} / {gene.MaxForDisplay}\n";
            if (gene.pawn.IsColonistPlayerControlled || gene.pawn.IsPrisonerOfColony)
            {
                text = ((!(gene.targetValue <= 0f)) ? (text + (string)("ConsumeResourceBelow".Translate(gene.ResourceLabel) + ": ") + gene.PostProcessValue(gene.targetValue)) : (text + "NeverConsumeResource".Translate().ToString()));
            }
            if (!drainGenes.NullOrEmpty())
            {
                float num = 0f;
                foreach (IGeneResourceDrain drainGene in drainGenes)
                {
                    if (drainGene.CanOffset)
                    {
                        tmpDrainGenes.Add(new Pair<IGeneResourceDrain, float>(drainGene, drainGene.ResourceLossPerDay));
                        num += drainGene.ResourceLossPerDay;
                    }
                }
                if (num != 0f)
                {
                    string text2 = ((num < 0f) ? "RegenerationRate".Translate() : "DrainRate".Translate());
                    text = text + "\n\n" + text2 + ": " + "PerDay".Translate(Mathf.Abs(gene.PostProcessValue(num))).Resolve();
                    foreach (Pair<IGeneResourceDrain, float> tmpDrainGene in tmpDrainGenes)
                    {
                        text = text + "\n  - " + tmpDrainGene.First.DisplayLabel.CapitalizeFirst() + ": " + "PerDay".Translate(gene.PostProcessValue(0f - tmpDrainGene.Second).ToStringWithSign()).Resolve();
                    }
                }
            }
            if (!gene.def.resourceDescription.NullOrEmpty())
            {
                text = text + "\n\n" + gene.def.resourceDescription.Formatted(gene.pawn.Named("PAWN")).Resolve();
            }
            return text;
        }
    }
}
