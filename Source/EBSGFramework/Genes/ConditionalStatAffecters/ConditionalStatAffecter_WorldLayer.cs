using System.Collections.Generic;
using System.Linq;
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
                return invert 
                    ? "LayerMustNotBe".Translate(layers.First().label, layers.First().gerundLabel) 
                    : "LayerMustBe".Translate(layers.First().label, layers.First().gerundLabel);
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
