using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class PawnRenderNode_EBSGAttachmentBody : PawnRenderNode
    {
        public PawnRenderNode_EBSGAttachmentBody(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            if (props is PawnRenderNodeProperties_EBSG EBSGProps)
            {
                Shader shader = EBSGProps.cutoutComplex ? ShaderDatabase.CutoutComplex : ShaderFor(pawn);
                if (EBSGProps.changing && gene != null && gene is SpawnAgeLimiter ebsgGene)
                {
                    var texPaths = Props.texPathsFemale.NullOrEmpty() ? Props.texPaths :
                        (pawn.gender == Gender.Female ? Props.texPathsFemale : Props.texPaths);

                    if (ebsgGene.lastChangeTick == -1)
                        ebsgGene.lastChangeTick = Find.TickManager.TicksGame;
                    else if (ebsgGene.lastChangeTick + EBSGProps.interval <= Find.TickManager.TicksGame)
                    {
                        ebsgGene.stage++;
                        if (ebsgGene.stage == texPaths.Count)
                            ebsgGene.stage = 0;
                        ebsgGene.lastChangeTick = Find.TickManager.TicksGame;
                    }

                    if (EBSGProps.multi)
                        return GraphicDatabase.Get<Graphic_Multi>(texPaths[ebsgGene.stage], shader,
                            Vector2.one, ColorFor(pawn));
                    else
                        return GraphicDatabase.Get<Graphic_Single>(texPaths[ebsgGene.stage], shader,
                            Vector2.one, ColorFor(pawn));
                }
                if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
                {
                    if (EBSGProps.HasDesGraphics())
                    {
                        if (pawn.DevelopmentalStage == DevelopmentalStage.Baby || pawn.DevelopmentalStage == DevelopmentalStage.Child)
                        {
                            if (EBSGProps.desChild != null)
                            {
                                return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desChild, shader, Vector2.one, Color.white);
                            }
                        }
                        else
                        {
                            if (EBSGProps.referenceGender)
                            {
                                if (pawn.gender == Gender.Male && EBSGProps.desMale != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desMale, shader, Vector2.one, Color.white);
                                }
                                if (pawn.gender == Gender.Female && EBSGProps.desFemale != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desFemale, shader, Vector2.one, Color.white);
                                }
                                if (EBSGProps.desGraphic != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desGraphic, shader, Vector2.one, Color.white);
                                }
                            }
                            else
                            {
                                if (pawn.story?.bodyType == BodyTypeDefOf.Male && EBSGProps.desMale != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desMale, shader, Vector2.one, Color.white);
                                }
                                if (pawn.story?.bodyType == BodyTypeDefOf.Female && EBSGProps.desFemale != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desFemale, shader, Vector2.one, Color.white);
                                }
                                if (pawn.story?.bodyType == BodyTypeDefOf.Fat && EBSGProps.desFat != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desFat, shader, Vector2.one, Color.white);
                                }
                                if (pawn.story?.bodyType == BodyTypeDefOf.Hulk && EBSGProps.desHulk != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desHulk, shader, Vector2.one, Color.white);
                                }
                                if (pawn.story?.bodyType == BodyTypeDefOf.Thin && EBSGProps.desThin != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desThin, shader, Vector2.one, Color.white);
                                }
                                if (EBSGProps.desGraphic != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desGraphic, shader, Vector2.one, Color.white);
                                }
                            }
                        }
                    }
                }
                else if (!EBSGProps.ageGraphics.NullOrEmpty())
                {
                    foreach (AgeBodyLink link in EBSGProps.ageGraphics)
                    {
                        if (link.ageRange.Includes(pawn.ageTracker.AgeBiologicalYearsFloat))
                        {
                            if (pawn.DevelopmentalStage == DevelopmentalStage.Baby || pawn.DevelopmentalStage == DevelopmentalStage.Child)
                            {
                                if (link.child != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(link.child, shader, Vector2.one, ColorFor(pawn));
                                }
                            }
                            else
                            {
                                if (link.referenceGender)
                                {
                                    if (pawn.gender == Gender.Male && link.male != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.male, shader, Vector2.one, ColorFor(pawn));
                                    }
                                    if (pawn.gender == Gender.Female && link.female != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.female, shader, Vector2.one, ColorFor(pawn));
                                    }
                                    if (link.graphic != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.graphic, shader, Vector2.one, ColorFor(pawn));
                                    }
                                }
                                else
                                {
                                    if (pawn.story?.bodyType == BodyTypeDefOf.Male && link.male != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.male, shader, Vector2.one, ColorFor(pawn));
                                    }
                                    if (pawn.story?.bodyType == BodyTypeDefOf.Female && link.female != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.female, shader, Vector2.one, ColorFor(pawn));
                                    }
                                    if (pawn.story?.bodyType == BodyTypeDefOf.Fat && link.fat != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.fat, shader, Vector2.one, ColorFor(pawn));
                                    }
                                    if (pawn.story?.bodyType == BodyTypeDefOf.Hulk && link.hulk != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.hulk, shader, Vector2.one, ColorFor(pawn));
                                    }
                                    if (pawn.story?.bodyType == BodyTypeDefOf.Thin && link.thin != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.thin, shader, Vector2.one, ColorFor(pawn));
                                    }
                                    if (link.graphic != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.graphic, shader, Vector2.one, ColorFor(pawn));
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }

            return null;
        }

        protected override IEnumerable<Graphic> GraphicsFor(Pawn pawn)
        {
            PawnRenderNodeProperties_EBSG EBSGProps = props as PawnRenderNodeProperties_EBSG;
            if (HasGraphic(tree.pawn) && EBSGProps != null && (EBSGProps.changing || EBSGProps.InAges(tree.pawn) || (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated && EBSGProps.HasDesGraphics())))
            {
                Graphic ebsgGraphic = GraphicFor(pawn);
                if (ebsgGraphic != null)
                    yield return ebsgGraphic;
            }
        }
    }
}
