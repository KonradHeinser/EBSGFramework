using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class PawnRenderNode_EBSGFur : PawnRenderNode_Fur
    {
        public PawnRenderNode_EBSGFur(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            if (props is PawnRenderNodeProperties_EBSG { altFurColor: not null } EBSGProps)
                return GraphicDatabase.Get<Graphic_Multi>(pawn.story.furDef.GetFurBodyGraphicPath(pawn), ShaderDatabase.CutoutComplex, Vector2.one, ColorFor(pawn), ColorFromType(pawn, EBSGProps.altFurColor.Value) ?? ColorFor(pawn));
            return base.GraphicFor(pawn);
        }

        public override Color ColorFor(Pawn pawn)
        {
            return ColorFromType(pawn, props.colorType) ?? props.color ?? Color.magenta;
        }

        private Color? ColorFromType(Pawn pawn, PawnRenderNodeProperties.AttachmentColorType type)
        {
            switch (type)
            {
                case PawnRenderNodeProperties.AttachmentColorType.Hair:
                    return pawn.story.HairColor;
                case PawnRenderNodeProperties.AttachmentColorType.Skin:
                    return pawn.story.SkinColor;
                default:
                    return null;
            }
        }
    }
}
