using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_SpawnPawn : CompProperties
    {
        public int maxTotalSpawn = 1;

        public IntRange spawnPerCompletion = new IntRange(1, 1);

        public IntRange completionTicks = new IntRange(600, 600);

        public float initialAge = 0f;

        public bool deleteOnFinalSpawn = true;

        public bool miscarriageThought = true;

        public XenotypeDef staticXenotype;

        public bool useMotherXeno = false;

        public bool useFatherXeno = false;

        public bool useHybrid = true;

        public CompProperties_SpawnPawn()
        {
            compClass = typeof(CompSpawnPawn);
        }
    }
}
