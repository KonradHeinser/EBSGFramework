using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class PawnRenderNodeProperties_EBSG : PawnRenderNodeProperties
    {
        public bool changing = false;

        public bool multi = false;

        public bool random = false;

        public bool cutoutComplex = false;

        public List<ApparelLayerDef> hiddenByLayers;

        public int interval = 60;

        public string desMale;

        public string desFemale;

        public string desChild;

        public string desThin;

        public string desHulk;

        public string desFat;

        public string desGraphic;

        public bool referenceGender = false;

        public List<AgeBodyLink> ageGraphics;

        private FloatRange ages;

        private bool checkedAges;

        private bool hasDes = false;

        private bool checkedDes = false;

        public bool InAges(Pawn pawn)
        {
            if (ageGraphics.NullOrEmpty()) return false;
            if (!checkedAges)
            {
                float minAge = 9999;
                float maxAge = 0;
                foreach (AgeBodyLink link in ageGraphics)
                {
                    if (minAge > link.ageRange.min)
                        minAge = link.ageRange.min;
                    if (maxAge < link.ageRange.max)
                        maxAge = link.ageRange.max;
                }
                ages = new FloatRange(minAge, maxAge);
                checkedAges = true;
            }
            return ages.Includes(pawn.ageTracker.AgeBiologicalYearsFloat);
        }

        public bool HasDesGraphics()
        {
            if (!checkedDes)
            {
                hasDes = desGraphic != null || desFat != null || desHulk != null || desThin != null || desFemale != null || desMale != null || desChild != null;
                checkedDes = true;
            }
            return hasDes;
        }

        public bool PropsNeeded(Pawn pawn)
        {
            if (changing)
                return true;

            if (!hiddenByLayers.NullOrEmpty())
                return true;

            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated && HasDesGraphics())
                return true;

            if (InAges(pawn))
                return true;

            return false;
        }
    }
}
