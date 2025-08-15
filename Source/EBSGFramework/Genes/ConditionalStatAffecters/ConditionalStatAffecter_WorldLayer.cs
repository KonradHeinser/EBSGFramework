using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_WorldLayer : ConditionalStatAffecter
    {
        public List<PlanetLayerDef> layers;

        public bool invert = false;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label.TranslateOrFormat();
            if (layers.Count == 1)
                if (invert)
                    return "EBSG_CorrectLayerOneNo".Translate(layers[0].LabelCap);
                else
                    return "EBSG_CorrectLayerOne".Translate(layers[0].LabelCap);
            return "EBSG_CorrectLayer".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Pawn?.Tile.Valid == true)
                return layers.Contains(req.Pawn.Tile.LayerDef) != invert;
            return false;
        }
    }
}
