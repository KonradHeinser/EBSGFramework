using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class EBSGBodyExtension : DefModExtension
    {
        public int interval = 300;

        public string desChildHead;

        public string desFemaleChildHead;

        public string desMaleChildHead;

        public string desHead;

        public string desMaleHead;

        public string desFemaleHead;

        public string desThinHead;

        public string desHulkHead;

        public string desFatHead;

        public string desMale;

        public string desFemale;

        public string desChild;

        public string desFemaleChild;

        public string desMaleChild;

        public string desThin;

        public string desHulk;

        public string desFat;

        public string desBody;

        public bool referenceGender = false;

        public List<AgeBodyLink> ageGraphics;

        private FloatRange ages;

        private bool checkedAges;

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
    }
}
