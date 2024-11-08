using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class PawnRenderNode_EBSGAttachmentHead : PawnRenderNode_AttachmentHead
    {
        public PawnRenderNode_EBSGAttachmentHead(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            if (props is PawnRenderNodeProperties_EBSG EBSGProps)
            {
                if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
                {
                    if (EBSGProps.HasDesGraphics())
                    {
                        if (pawn.DevelopmentalStage == DevelopmentalStage.Baby || pawn.DevelopmentalStage == DevelopmentalStage.Child)
                        {
                            if (EBSGProps.desChild != null)
                            {
                                return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desChild, ShaderFor(pawn), Vector2.one, Color.white);
                            }
                        }
                        else
                        {
                            if (EBSGProps.referenceGender)
                            {
                                if (pawn.gender == Gender.Male && EBSGProps.desMale != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desMale, ShaderFor(pawn), Vector2.one, Color.white);
                                }
                                if (pawn.gender == Gender.Female && EBSGProps.desFemale != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desFemale, ShaderFor(pawn), Vector2.one, Color.white);
                                }
                                if (EBSGProps.desGraphic != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desGraphic, ShaderFor(pawn), Vector2.one, Color.white);
                                }
                            }
                            else
                            {
                                if (pawn.story?.bodyType == BodyTypeDefOf.Male && EBSGProps.desMale != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desMale, ShaderFor(pawn), Vector2.one, Color.white);
                                }
                                if (pawn.story?.bodyType == BodyTypeDefOf.Female && EBSGProps.desFemale != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desFemale, ShaderFor(pawn), Vector2.one, Color.white);
                                }
                                if (pawn.story?.bodyType == BodyTypeDefOf.Fat && EBSGProps.desFat != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desFat, ShaderFor(pawn), Vector2.one, Color.white);
                                }
                                if (pawn.story?.bodyType == BodyTypeDefOf.Hulk && EBSGProps.desHulk != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desHulk, ShaderFor(pawn), Vector2.one, Color.white);
                                }
                                if (pawn.story?.bodyType == BodyTypeDefOf.Thin && EBSGProps.desThin != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desThin, ShaderFor(pawn), Vector2.one, Color.white);
                                }
                                if (EBSGProps.desGraphic != null)
                                {
                                    return GraphicDatabase.Get<Graphic_Multi>(EBSGProps.desGraphic, ShaderFor(pawn), Vector2.one, Color.white);
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
                                    return GraphicDatabase.Get<Graphic_Multi>(link.child, ShaderFor(pawn), Vector2.one, ColorFor(pawn));
                                }
                            }
                            else
                            {
                                if (link.referenceGender)
                                {
                                    if (pawn.gender == Gender.Male && link.male != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.male, ShaderFor(pawn), Vector2.one, ColorFor(pawn));
                                    }
                                    if (pawn.gender == Gender.Female && link.female != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.female, ShaderFor(pawn), Vector2.one, ColorFor(pawn));
                                    }
                                    if (link.graphic != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.graphic, ShaderFor(pawn), Vector2.one, ColorFor(pawn));
                                    }
                                }
                                else
                                {
                                    if (pawn.story?.bodyType == BodyTypeDefOf.Male && link.male != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.male, ShaderFor(pawn), Vector2.one, ColorFor(pawn));
                                    }
                                    if (pawn.story?.bodyType == BodyTypeDefOf.Female && link.female != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.female, ShaderFor(pawn), Vector2.one, ColorFor(pawn));
                                    }
                                    if (pawn.story?.bodyType == BodyTypeDefOf.Fat && link.fat != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.fat, ShaderFor(pawn), Vector2.one, ColorFor(pawn));
                                    }
                                    if (pawn.story?.bodyType == BodyTypeDefOf.Hulk && link.hulk != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.hulk, ShaderFor(pawn), Vector2.one, ColorFor(pawn));
                                    }
                                    if (pawn.story?.bodyType == BodyTypeDefOf.Thin && link.thin != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.thin, ShaderFor(pawn), Vector2.one, ColorFor(pawn));
                                    }
                                    if (link.graphic != null)
                                    {
                                        return GraphicDatabase.Get<Graphic_Multi>(link.graphic, ShaderFor(pawn), Vector2.one, ColorFor(pawn));
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
            if (HasGraphic(tree.pawn) && (EBSGProps == null || EBSGProps.InAges(tree.pawn) || (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated && EBSGProps.HasDesGraphics())))
            {
                Graphic ebsgGraphic = GraphicFor(pawn);
                if (ebsgGraphic != null)
                    yield return ebsgGraphic;
            }
        }
    }
}
