using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_HediffWhileMoving : HediffCompProperties
    {
        public HediffDef hediffWhileMoving;
        public List<HediffDef> hediffsWhileMoving;
        public HediffDef hediffWhileNotMoving;
        public List<HediffDef> hediffsWhileNotMoving;
        public HediffCompProperties_HediffWhileMoving()
        {
            compClass = typeof(HediffComp_HediffWhileMoving);
        }
    }
}
