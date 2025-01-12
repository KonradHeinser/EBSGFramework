using RimWorld;

namespace EBSGFramework
{
    public class Thought_Situational_PeopleInColony : Thought_Situational
    {
        public override float MoodOffset()
        {
            int freeColonistsAndPrisonersSpawnedCount = pawn.Map.mapPawns.FreeColonistsAndPrisonersSpawnedCount;

            EBSGThoughtExtension thoughtExtension = def.GetModExtension<EBSGThoughtExtension>();
            if (thoughtExtension?.curve != null) 
                return thoughtExtension.curve.Evaluate(freeColonistsAndPrisonersSpawnedCount);

            EBSGExtension extension = def.GetModExtension<EBSGExtension>();
            if (extension?.peopleToMoodCurve != null)
                return extension.peopleToMoodCurve.Evaluate(freeColonistsAndPrisonersSpawnedCount);

            return base.MoodOffset();
        }
    }
}
