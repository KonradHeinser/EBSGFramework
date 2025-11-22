using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EBSGFramework
{
    [StaticConstructorOnStartup]
    public class GeneGizmo_ResourceGene : GeneGizmo_Resource
    {
        protected Texture2D barTex;

        protected Texture2D barHighlightTex;
        
        protected static readonly Texture2D ResourceCostTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.116f, 0.129f, 0.190f));

        protected List<Pair<IGeneResourceDrain, float>> tmpDrainGenes = new List<Pair<IGeneResourceDrain, float>>();
        
        protected static readonly Texture2D BarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.34f, 0.42f, 0.43f));

        protected static readonly Texture2D BarHighlightTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.43f, 0.54f, 0.55f));

        protected static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

        protected Texture2D iconTex;
        
        protected ThingDef iconThing;
        
        public GeneGizmo_ResourceGene(Gene_Resource gene, List<IGeneResourceDrain> drainGenes, Color barColor, Color barHighlightColor)
            : base(gene, drainGenes, barColor, barHighlightColor)
        {
        }
        
        protected DRGExtension Extension => gene?.def?.GetModExtension<DRGExtension>();

        protected override bool IsDraggable
        {
            get
            {
                if (Extension != null && (Extension.addTargetBar || !Extension.resourcePacks.NullOrEmpty()))
                    return base.IsDraggable;
                return false;
            }
        }

        protected static bool draggingBar;

        protected override bool DraggingBar { get => draggingBar; set => draggingBar = value; }
        
        protected bool initialized;
        
        protected void Initialize()
        {
            if (!initialized)
            {
                initialized = true;
                barTex = BarColor == default ? BarTex : SolidColorMaterials.NewSolidColorTexture(BarColor);
                barHighlightTex = BarHighlightColor == default ? BarHighlightTex : SolidColorMaterials.NewSolidColorTexture(BarHighlightColor);
                InitializeIcon();
            }
        }

        protected void InitializeIcon()
        {
            if (Extension != null)
            {
                if (Extension.iconPath != null)
                    iconTex = ContentFinder<Texture2D>.Get(Extension.iconPath);
                else if (Extension.iconThing != null)
                    iconThing = Extension.iconThing;
                else if (!Extension.resourcePacks.NullOrEmpty())
                    iconThing = Extension.resourcePacks.First();
            }

        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var result = new GizmoResult(GizmoState.Clear);

            if (!IsDraggable)
            {
                if (!initialized)
                    Initialize();

                Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
                Rect rect2 = rect.ContractedBy(8f);
                Widgets.DrawWindowBackground(rect);
                bool mouseOverElement = false;
                Text.Font = GameFont.Small;
                Rect headerRect = rect2;
                headerRect.height = Text.LineHeight;
                DrawHeader(headerRect, ref mouseOverElement);
                barRect = rect2;
                barRect.yMin = headerRect.yMax + 8f;
                Widgets.FillableBar(barRect, ValuePercent, Mouse.IsOver(barRect) ? barHighlightTex : barTex, EmptyBarTex, doBorder: true);
                foreach (float barThreshold in GetBarThresholds())
                {
                    GUI.DrawTexture(new Rect
                    {
                        x = barRect.x + 3f + (barRect.width - 8f) * barThreshold,
                        y = barRect.y + barRect.height - 9f,
                        width = 2f,
                        height = 6f
                    }, (ValuePercent < barThreshold) ? BaseContent.GreyTex : BaseContent.BlackTex);
                }

                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(barRect, BarLabel);
                Text.Anchor = TextAnchor.UpperLeft;
                if (Mouse.IsOver(rect) && !mouseOverElement)
                {
                    Widgets.DrawHighlight(rect);
                    TooltipHandler.TipRegion(rect, GetTooltip, Gen.HashCombineInt(GetHashCode(), 8573612));
                }

                if (!HighlightTag.NullOrEmpty())
                    UIHighlighter.HighlightOpportunity(rect, HighlightTag);
            }
            else
            {
                result = base.GizmoOnGUI(topLeft, maxWidth, parms);
                if (iconThing == null && !iconTex)
                    InitializeIcon();
            }
            
        
            float num = Mathf.Repeat(Time.time, 0.85f);
            float num2 = 1f;
            if (num < 0.1f)
                num2 = num / 0.1f;
            else if (num >= 0.25f)
                num2 = 1f - (num - 0.25f) / 0.6f;

            if (MapGizmoUtility.LastMouseOverGizmo is Command_Ability command_Ability && gene.Max != 0f)
            {
                foreach (CompAbilityEffect effectComp in command_Ability.Ability.EffectComps)
                {
                    bool flag = true;
                    float cost = 0;

                    switch (effectComp)
                    {
                        case CompAbilityEffect_ResourceCost compAbilityEffect_ResourceCost when compAbilityEffect_ResourceCost.Props.mainResourceGene == gene.def && compAbilityEffect_ResourceCost.Props.resourceCost > float.Epsilon:
                            cost = compAbilityEffect_ResourceCost.Props.resourceCost;
                            break;
                        case CompAbilityEffect_ResourceToBattery compAbilityEffect_Battery when compAbilityEffect_Battery.Props.mainResourceGene == gene.def && compAbilityEffect_Battery.MaxCost > 0:
                            cost = compAbilityEffect_Battery.MaxCost;
                            break;
                        case CompAbilityEffect_EnergyBlast compAbilityEffect_Blast when compAbilityEffect_Blast.Props.mainResourceGene == gene.def && compAbilityEffect_Blast.CurrentCost > 0:
                            cost = compAbilityEffect_Blast.CurrentCost;
                            break;
                        case CompAbilityEffect_EnergyBurst compAbilityEffect_Burst when compAbilityEffect_Burst.Props.mainResourceGene == gene.def && compAbilityEffect_Burst.CurrentCost > 0:
                            cost = compAbilityEffect_Burst.CurrentCost;
                            break;
                        default:
                            flag = false;
                            break;
                    }

                    if (flag)
                    {
                        Rect rect = barRect.ContractedBy(3f);
                        float width = rect.width;
                        float num3 = gene.Value / gene.Max;
                        rect.xMax = rect.xMin + width * num3;
                        float num4 = Mathf.Min(cost / gene.Max, 1f);
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

        protected override void DrawHeader(Rect headerRect, ref bool mouseOverElement)
        {
            base.DrawHeader(headerRect, ref mouseOverElement);
            ResourceGene resourceGene;
            if ((gene.pawn.IsColonistPlayerControlled || gene.pawn.IsPrisonerOfColony) && (resourceGene = gene as ResourceGene) != null)
            {
                headerRect.xMax -= 24f;
                Rect rect = new Rect(headerRect.xMax, headerRect.y, 24f, 24f);
                var extension = resourceGene.def.GetModExtension<DRGExtension>();
                if (iconTex || iconThing != null)
                {
                    if (iconTex)
                        GUI.DrawTexture(rect, iconTex);
                    else
                        Widgets.DefIcon(rect, iconThing);
                    if (extension?.resourcePacks.NullOrEmpty() == false)
                    {
                        GUI.DrawTexture(new Rect(rect.center.x, rect.y, rect.width / 2f, rect.height / 2f), resourceGene.resourcePacksAllowed ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
                        if (Widgets.ButtonInvisible(rect))
                        {
                            resourceGene.resourcePacksAllowed = !resourceGene.resourcePacksAllowed;
                            if (resourceGene.resourcePacksAllowed)
                                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                            else
                                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                        }
                        if (Mouse.IsOver(rect))
                        {
                            Widgets.DrawHighlight(rect);
                            string onOff = (resourceGene.resourcePacksAllowed ? "On" : "Off").Translate().ToString().UncapitalizeFirst();
                            TooltipHandler.TipRegion(rect, () => "AutoTakeResourceDesc".Translate(resourceGene.ResourceLabel.Named("RESOURCE"), gene.pawn.Named("PAWN"), resourceGene.PostProcessValue(resourceGene.targetValue).Named("MIN"), onOff.Named("ONOFF")).Resolve(), 828267373);
                            mouseOverElement = true;
                        }
                    }

                }
            }
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
