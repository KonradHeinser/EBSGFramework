using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HiveMindGene : Gene
    {
        public HiveMindExtension extension;
        public List<HediffDef> hediffsWhenNoAllies; // This list makes it so things don't need to iterate so much

        public override void PostAdd()
        {
            base.PostAdd();
            if (def.HasModExtension<HiveMindExtension>()) 
            {
                extension = def.GetModExtension<HiveMindExtension>();
                if (!extension.hiveRolesToCheckFor.NullOrEmpty()) BuildNoAllyList();
            }
            else
            {
                Log.Error(def + " is a hivemind gene, but doesn't have usable hivemind information. Removing gene.");
                pawn.genes.RemoveGene(this);
            }
        }

        public override void Tick()
        {
            if (extension == null) extension = def.GetModExtension<HiveMindExtension>();
            if (!pawn.IsHashIntervalTick(200) || extension.hiveRolesToCheckFor.NullOrEmpty()) return; // To avoid some performance issues from constant checking
            if (hediffsWhenNoAllies.NullOrEmpty()) BuildNoAllyList();
            List<Pawn> allies = GetAllies();

            if (allies.NullOrEmpty()) return; // If there's no allies, get out of here because this place is weird
            if (allies.Count == 1) // If this pawn is the only one in the hive, there's no reason to waste time trying to go through everyone's genes in depth, just apply the lonely hediffs
            {
                ApplyHediffs(null, hediffsWhenNoAllies);
            }
            else
            {
                // Make another method that receives the check key and returns an integer with how many people have that key
            }
        }

        private List<Pawn> GetAllies()
        {
            List<Pawn> allies = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
            List<Pawn> tempAllies = allies;
            foreach(Pawn ally in tempAllies)
            {
                bool flag = true;
                if (ally.genes != null)
                {
                    List<Gene> genesListForReading = ally.genes.GenesListForReading;
                    foreach (Gene gene in genesListForReading)
                    {
                        if (gene.def.HasModExtension<HiveMindExtension>() && gene.def.GetModExtension<HiveMindExtension>().hiveKey == extension.hiveKey)
                        {
                            flag = false;
                            break;
                        }
                    }
                }
                if (flag) allies.Remove(ally);
            }
            return allies;
        }

        private void ApplyHediffs(HediffDef hediff = null, List<HediffDef> hediffs = null)
        {
            if (hediff != null && pawn.health.hediffSet.HasHediff(hediff)) pawn.health.AddHediff(hediff);

            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in hediffs)
                {
                    if (pawn.health.hediffSet.HasHediff(hediffDef)) pawn.health.AddHediff(hediff);
                }
            }
        }

        private void BuildNoAllyList() // Builds a static list to use if the pawn is alone
        {
            foreach (HiveRoleToCheckFor hiveRole in extension.hiveRolesToCheckFor)
            {
                foreach (HiveThreshold hiveThreshold in hiveRole.thresholds)
                {
                    if (hiveThreshold.minCount > 1)
                    {
                        // Adds the hediff to the list if it's not already there
                        if (hiveThreshold.hediffWhenTooFew != null && !hediffsWhenNoAllies.Contains(hiveThreshold.hediffWhenTooFew)) hediffsWhenNoAllies.Add(hiveThreshold.hediffWhenTooFew);
                        if (!hiveThreshold.hediffsWhenTooFew.NullOrEmpty())
                        {
                            foreach (HediffDef hediff in hiveThreshold.hediffsWhenTooFew)
                            {
                                if (!hediffsWhenNoAllies.Contains(hediff)) hediffsWhenNoAllies.Add(hediff);
                            }
                        }
                    }
                }
            }
        }
    }
}
