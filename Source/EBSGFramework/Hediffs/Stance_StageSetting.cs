using Verse;

namespace EBSGFramework
{
    public class Stance_StageSetting : Stance_Cooldown
    {
        public Stance_StageSetting()
        {
        }

        public Stance_StageSetting(int ticks, LocalTargetInfo focusTarg, Verb verb)
            : base(ticks, focusTarg, verb)
        {
        }
        public override void StanceDraw()
        {
            return;
        }
    }
}
