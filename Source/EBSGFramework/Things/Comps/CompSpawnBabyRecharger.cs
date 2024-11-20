using Verse;

namespace EBSGFramework
{
    public class CompSpawnBabyRecharger : ThingComp
    {
        public Pawn mother;

        public Pawn father;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref mother, "mother");
            Scribe_References.Look(ref father, "father");
        }
    }
}
