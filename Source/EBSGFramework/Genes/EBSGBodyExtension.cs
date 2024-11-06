using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class EBSGBodyExtension : DefModExtension
    {
        public string desChildHead;

        public string desHead;

        public string desMale;

        public string desFemale;

        public string desChild;

        public string desThin;

        public string desHulk;

        public string desFat;

        public string desBody;

        public List<AgeBodyLink> ageBodies;

        public FloatRange ages;

        public bool checkedAges;

        public bool InAges(Pawn pawn)
        {
            if (ageBodies.NullOrEmpty()) return false;
            if (!checkedAges)
            {
                float minAge = 9999;
                float maxAge = 0;
                foreach (AgeBodyLink link in ageBodies)
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
