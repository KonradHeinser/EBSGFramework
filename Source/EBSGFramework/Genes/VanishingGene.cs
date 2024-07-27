using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class VanishingGene : HediffAdder
    {
        public int ticksUntilVanish = 30; // This gene class makes it easier to make genes that trigger at the beginning, then disappear

        public override void Tick()
        {
            if (ticksUntilVanish == 0)
            {
                pawn.genes.RemoveGene(pawn.genes.GetGene(def));
                Messages.Message(def + " has been successfully removed!", MessageTypeDefOf.NeutralEvent, false);
            }
            ticksUntilVanish--;
        }
    }
}
