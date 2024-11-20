using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class IngestionOutcomeDoer_RechargeSpawnBabyComp : IngestionOutcomeDoer
    {
        public int chargesGained = 1;

        // Make the preferability Undefined to avoid having other beings try to eat it. Alternatively, try NeverForNutrition
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            if (pawn.HasComp<CompSpawnBaby>())
            {
                CompSpawnBaby spawnBaby = pawn.GetComp<CompSpawnBaby>();
                spawnBaby.spawnLeft += chargesGained;
                spawnBaby.ticksLeft = spawnBaby.Props.completionTicks.RandomInRange;

                CompSpawnBabyRecharger recharger = ingested.TryGetComp<CompSpawnBabyRecharger>();
                if (recharger != null)
                {
                    spawnBaby.mother = recharger.mother;
                    spawnBaby.father = recharger.father;

                    if (recharger.mother != null)
                        spawnBaby.faction = recharger.mother.Faction;
                    else
                        spawnBaby.faction = recharger.father?.Faction;
                }
            }
        }
    }
}
