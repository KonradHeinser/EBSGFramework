using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompPowerPlantSleepCasket : CompPowerPlant
    {
        private float cachedPowerOutput;

        protected override float DesiredPowerOutput => cachedPowerOutput;

        private Building_SleepCasket Casket => parent as Building_SleepCasket;

        private CompProperties_SleepCasketPower CasketPower => Props as CompProperties_SleepCasketPower;

        public override void CompTick()
        {
            base.CompTick();
            if (parent.IsHashIntervalTick(50) && 
                (!PowerOn || Casket == null || !Casket.PawnInside || !Casket.IsContentsSuspended))
            {
                cachedPowerOutput = 0f;
                return;
            }
            if (parent.IsHashIntervalTick(250))
            {
                cachedPowerOutput = 0f - Props.PowerConsumption;
                bool pawnInside = false;
                foreach (Thing thing in Casket.InnerContainer)
                    if (thing is Pawn pawn)
                    {
                        if (!CasketPower.statFactors.NullOrEmpty())
                            foreach (StatDef stat in CasketPower.statFactors)
                                cachedPowerOutput *= pawn.StatOrOne(stat);
                        if (!CasketPower.statDivisors.NullOrEmpty())
                            foreach (StatDef stat in CasketPower.statDivisors)
                                cachedPowerOutput /= pawn.StatOrOne(stat);
                        pawnInside = true;
                    }
                if (!pawnInside) cachedPowerOutput = 0;
            }
        }
    }
}
