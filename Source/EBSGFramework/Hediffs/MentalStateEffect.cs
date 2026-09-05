using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class MentalStateEffect
    {
        public MentalStateDef mentalState;

        public List<MentalStateDef> mentalStates;

        public float mentalSeverity = 3;

        public bool addSeverityPerHour; // Makes the severity set here get added to the severity as a per hour rate (i.e. mentalSeverity 1 increases severity by 1 per hour)
    }
}
