using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SpawnHumanlike : HediffCompProperties
    {
        public bool onInterval = true;

        public bool onDeath = false;

        public bool onRemoval = false;

        public bool spawnRemainingOnRemovalOrDeath = true;

        public bool removeHediffOnFinalSpawn = true;

        public bool killHostOnFinalSpawn = false;

        public bool killHostOnRemoval = false;

        public bool allowInCaravans = true;

        public FloatRange validSeverity = FloatRange.Zero;

        public int maxTotalSpawn = 1; // Setting to -1 makes it continue forever

        public IntRange spawnPerCompletion = new IntRange(1, 1);

        public IntRange completionTicks = new IntRange(600, 600);

        public ThingDef filthOnCompletion;

        public IntRange filthPerSpawn = new IntRange(4, 7);

        public bool sendLetters = false;

        public string letterLabelNote = "born";

        public string letterTextPawnDescription = "became a healthy baby!";

        public PawnKindDef staticPawnKind;

        public bool miscarriageThought = false;

        public HediffDef linkedHediff;

        public ThoughtDef motherMiscarriageThought;

        public ThoughtDef fatherMiscarriageThought;

        public bool bornThought = true;

        public ThoughtDef motherBabyBornThought;

        public ThoughtDef fatherBabyBornThought;

        public XenotypeDef staticXenotype;

        public XenoSource xenotypeSource = XenoSource.Hybrid;

        public DevelopmentalStage developmentalStage = DevelopmentalStage.Adult;

        public DevelopmentalStage devStageForRemovalOrDeath = DevelopmentalStage.Child;

        public HediffCompProperties_SpawnHumanlike()
        {
            compClass = typeof(HediffComp_SpawnHumanlike);
        }
    }
}
