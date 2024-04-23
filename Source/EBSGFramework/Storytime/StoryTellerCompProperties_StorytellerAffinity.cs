using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class StoryTellerCompProperties_StorytellerAffinity : StorytellerCompProperties
    {
        public int anyPawnKilled = 0; // Literally any pawn, even animals

        public int humanoidKilled = 0; // You know where this is going. Insects, animals, mechanoids, colonists, etc

        public int colonistDied = 0;

        public int slaveDied = 0;

        public int prisonerDied = 0;

        public int guestDied = 0;

        public int colonistKidnapped = 0;



        // Affinity Thresholds
        public int adoredAffinity = 1000; // Max affinity level

        public int lovedAffinity = 750;

        public int friendlyAffinity = 500;

        public int respectedAffinity = 250;

        public int disapprovingAffinity = -250;

        public int dislikedAffinity = -500;

        public int hatedAffinity = -750;

        public int despisedAffinity = -1000; // Minimum affinity level

        public StoryTellerCompProperties_StorytellerAffinity()
        {
            compClass = typeof(StoryTellerComp_StorytellerAffinity);
        }
    }
}
