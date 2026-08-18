using Verse;

namespace EBSGFramework
{
    public class HediffComp_GainMemoryOnKill : HediffComp
    {
        private HediffCompProperties_GainMemoryOnKill Props => (HediffCompProperties_GainMemoryOnKill) props;

        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);
            var memory = Props.memory;
            
            if (victim.RaceProps.Humanlike)
            {
                if (Props.humanMemory != null)
                    memory = Props.humanMemory;
            }
            else if (victim.RaceProps.Dryad)
            {
                if (Props.dryadMemory != null)
                    memory = Props.dryadMemory;
            }
            else if (victim.RaceProps.Insect)
            {
                if (Props.insectMemory != null) 
                    memory = Props.insectMemory;
            }
            else if (victim.RaceProps.Animal)
            {
                if (Props.animalMemory != null) 
                    memory = Props.animalMemory;
            }
            else if (victim.IsMechanical())
            {
                if (Props.mechanoidMemory != null) 
                    memory = Props.mechanoidMemory;
            }
            else if (ModsConfig.AnomalyActive && victim.RaceProps.IsAnomalyEntity)
            {
                if (Props.entityMemory != null)
                    memory = Props.entityMemory;
            }
            
            if (memory != null)
                Pawn.needs.mood.thoughts.memories.TryGainMemory(memory, victim);
        }
    }
}