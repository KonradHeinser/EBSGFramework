using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Pollution : ConditionalStatAffecter
    {
        public bool inPollution = true;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (inPollution) return "EBSG_InPollution".Translate();
            return "EBSG_NotInPollution".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Pawn.Spawned)
            {
                if (inPollution)
                    return req.Pawn.Position.IsPolluted(req.Pawn.Map);
                return !req.Pawn.Position.IsPolluted(req.Pawn.Map);
            }

            return false;
        }
    }
}
