using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompPowerPlantSleepCasket : CompPowerPlant
    {
        private float cachedPowerOutput;

        protected override float DesiredPowerOutput => cachedPowerOutput;

        private Building_SleepCasket Casket => parent as Building_SleepCasket;

        private CompProperties_CasketPower CasketPower => Props as CompProperties_CasketPower;

        public override void CompTick()
        {
            base.CompTick();
            if (!PowerOn || Casket == null || CasketPower == null || Casket.InnerContainer.Count == 0)
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
                                cachedPowerOutput *= pawn.GetStatValue(stat);
                        if (!CasketPower.statDivisors.NullOrEmpty())
                            foreach (StatDef stat in CasketPower.statDivisors)
                                cachedPowerOutput /= pawn.GetStatValue(stat);
                        pawnInside = true;
                    }
                if (!pawnInside) cachedPowerOutput = 0;
            }
        }
    }
}
