using RimWorld;

namespace EBSGFramework
{
    public class Thought_MemorySocialRandomOpinion : Thought_MemorySocial
    {
        public override void Init()
        {
            base.Init();
            EBSGThoughtExtension thoughtExtension = def.GetModExtension<EBSGThoughtExtension>();
            opinionOffset += thoughtExtension.range.RandomInRange;
        }
        
        public override bool GroupsWith(Thought other)
        {
            return other is Thought_MemorySocialRandomOpinion && base.GroupsWith(other);
        }
    }
}