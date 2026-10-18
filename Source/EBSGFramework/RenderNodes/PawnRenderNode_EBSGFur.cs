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
            if (props is PawnRenderNodeProperties_EBSG { altColorType: not null } EBSGProps)
                return GraphicDatabase.Get<Graphic_Multi>(pawn.story.furDef.GetFurBodyGraphicPath(pawn), ShaderDatabase.CutoutComplex, Vector2.one, ColorFor(pawn), ColorFromType(pawn, EBSGProps.altColorType.Value, EBSGProps.altColor));
            return base.GraphicFor(pawn);
        }

        public override Color ColorFor(Pawn pawn)
        {
            return ColorFromType(pawn, props.colorType);
        }

        private Color ColorFromType(Pawn pawn, PawnRenderNodeProperties.AttachmentColorType type, Color? altColor = null)
        {
            return type switch
            {
                PawnRenderNodeProperties.AttachmentColorType.Hair => pawn.story.HairColor,
                PawnRenderNodeProperties.AttachmentColorType.Skin => pawn.story.SkinColor,
                _ => altColor ?? props.color ?? Color.magenta
            };
        }
    }
}
