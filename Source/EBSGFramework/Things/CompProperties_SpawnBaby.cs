﻿using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_SpawnBaby : CompProperties
    {
        public enum XenoSource
        {
            Father,
            Mother,
            Hybrid
        }

        public int maxTotalSpawn = 1; // Setting to -1 makes it continue forever

        public IntRange spawnPerCompletion = new IntRange(1, 1);

        public IntRange completionTicks = new IntRange(600, 600);

        public ThingDef filthOnCompletion;

        public IntRange filthPerSpawn = new IntRange(4, 7);

        public bool deleteOnFinalSpawn = true;

        public bool sendLetters = true;

        public string letterLabelNote = "born";

        public PawnKindDef staticPawnKind;

        public bool miscarriageThought = true;

        public ThoughtDef motherMiscarriageThought;

        public ThoughtDef fatherMiscarriageThought;

        public bool bornThought = true;

        public ThoughtDef motherBabyBornThought;

        public ThoughtDef fatherBabyBornThought;

        public XenotypeDef staticXenotype;

        public XenoSource xenotypeSource = XenoSource.Hybrid;

        public CompProperties_SpawnBaby()
        {
            compClass = typeof(CompSpawnBaby);
        }
    }
}