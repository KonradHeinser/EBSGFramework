using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilitySpawnHumanlike : CompProperties_AbilityEffect
    {
        public AbilityEffectDestination destination = AbilityEffectDestination.Selected;

        public DeathType targetEffect = DeathType.None;
        
        public float bodySizeFactor = -1f;
        
        public IntRange spawnCount = new IntRange(1, 1);

        public ThingDef filthOnCompletion;

        public IntRange filthPerSpawn = new IntRange(4, 7);

        public bool sendLetters = false;

        public string letterLabelNote = "EBSG_Born";

        public string letterTextPawnDescription = "EBSG_BecameAHealthyBaby";

        public PawnKindDef staticPawnKind;
        
        public HediffDef linkedHediff;

        public bool removeLink;
        
        public XenotypeDef staticXenotype;

        public string xenotypeLabel;
        
        public List<GeneDef> staticGenes = new List<GeneDef>();

        public XenoSource xenotypeSource = XenoSource.Hybrid;

        public DevelopmentalStage developmentalStage = DevelopmentalStage.Adult;
        
        public bool bornThought = false;

        public ThoughtDef motherBabyBornThought;

        public ThoughtDef fatherBabyBornThought;

        public InitialRelation relations = InitialRelation.Both;

        public PawnRelationDef motherRelation;
        
        public PawnRelationDef fatherRelation;
        
        public bool noGear = true;

        public CompProperties_AbilitySpawnHumanlike()
        {
            compClass = typeof(CompAbilityEffect_SpawnHumanlike);
        }
    }
}