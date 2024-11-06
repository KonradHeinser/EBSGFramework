using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class PawnRenderNode_EBSGFur : PawnRenderNode_Fur
    {
        public PawnRenderNode_EBSGFur(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }

        public override Color ColorFor(Pawn pawn)
        {
            if (props.colorType == PawnRenderNodeProperties.AttachmentColorType.Hair)
                return pawn.story.HairColor;
            if (props.colorType == PawnRenderNodeProperties.AttachmentColorType.Skin)
                return pawn.story.SkinColor;
            if (props.color != null)
                return props.color.Value;
            return Color.magenta;
        }
    }
}
