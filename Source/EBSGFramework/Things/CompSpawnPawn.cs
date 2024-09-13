using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompSpawnPawn : ThingComp
    {
        public CompProperties_SpawnPawn Props => (CompProperties_SpawnPawn)props;

        public int spawnLeft;

        public override void PostPostMake()
        {
            base.PostPostMake();
        }

        // Uses rare tick instead of normal ticks in an attempt to alleviate performance costs some
        public override void CompTickRare() // 250 ticks
        {
            base.CompTickRare();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            if (Props.miscarriageThought && spawnLeft != 0)
                return;
        }
    }
}
