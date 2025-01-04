using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Pollution : ConditionalStatAffecter
    {
        public bool inPollution = true;

        public bool defaultActive;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (inPollution) return "EBSG_InPollution".Translate();
            return "EBSG_NotInPollution".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn && pawn.Spawned)
            {
                if (inPollution)
                    return pawn.Position.IsPolluted(pawn.Map);
                return !pawn.Position.IsPolluted(pawn.Map);
            }

            return defaultActive;
        }
    }
}
