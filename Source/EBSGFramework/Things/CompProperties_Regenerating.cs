using Verse;

namespace EBSGFramework
{
    public class CompProperties_Regenerating : CompProperties
    {
        public int regenerationAmount = 1;

        public int regenerationInterval = 60;

        public CompProperties_Regenerating()
        {
            compClass = typeof(CompRegenerating);
        }
    }
}
