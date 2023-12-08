using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HiveMindGene : Gene
    {
        public HiveMindExtension extension = null;
        public List<GeneDef> hiveGenesPresent = null; // Only goes through gene lists once per viable tick
        public List<HediffDef> hediffsWhenNoAllies = null; // This list makes it so things don't need to iterate so much
        public List<HediffDef> allHediffs = null; // This list was created for the basic wipe, but may have unexpected future use
        public List<HediffDef> addedHediffs = null;
        public Dictionary<string, int> hiveCounts = new Dictionary<string, int>();
        public Dictionary<string, int> previousCounts = new Dictionary<string, int>();
        public bool stillAlone = false;
        public bool completeWipe = true;

        public override void PostAdd()
        {
            base.PostAdd();
            if (def.HasModExtension<HiveMindExtension>()) 
            {
                extension = def.GetModExtension<HiveMindExtension>();
                if (!extension.hiveRolesToCheckFor.NullOrEmpty())
                {
                    BuildNoAllyList();
                    BuildAllHediffsList();
                    StartHiveCheck();
                }
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

            if (allHediffs.NullOrEmpty()) Log.Error("It didn't stick");
            else Log.Message("It stuck");

            if (allHediffs.NullOrEmpty()) BuildAllHediffsList();
            if (completeWipe)
            {
                EBSGUtilities.RemoveHediffs(pawn, null, allHediffs); // Wipe slate
                completeWipe = false;
                Log.Message("Wiping everything");
            }
            StartHiveCheck();
        }

        private void StartHiveCheck()
        {
            List<Pawn> allies = GetAllies();
            if (allies.NullOrEmpty()) // If there's no allies, get out of here because this place is weird
            {
                EBSGUtilities.RemoveHediffs(pawn, null, addedHediffs); // Wipe slate
                previousCounts.Clear();
                stillAlone = false; // Prep for less weird scenario
                return;
            }
            if (allies.Count == 1) // If this pawn is the only one in the hive, there's no reason to waste time trying to go through everyone's genes in depth, just apply the lonely hediffs
            {
                if (!stillAlone)
                {
                    EBSGUtilities.RemoveHediffs(pawn, null, addedHediffs); // Wipe slate
                    previousCounts.Clear();
                    addedHediffs = EBSGUtilities.ApplyHediffs(pawn, null, hediffsWhenNoAllies); // Add the lonely hediffs
                    stillAlone = true; // Prep for continued loneliness. No reason to keep removing and adding hediffs if still alone
                    Log.Message("Lonely effects applied");
                }
            }
            else
            {
                BuildHiveCount(allies);
                if (previousCounts == null || previousCounts != hiveCounts) // If there's a change in the hive
                {
                    Log.Message("New hive count");
                    EBSGUtilities.RemoveHediffs(pawn, null, addedHediffs); // Wipe slate
                    foreach (HiveRoleToCheckFor hiveRole in extension.hiveRolesToCheckFor)
                    {
                        if (!hiveCounts.ContainsKey(hiveRole.checkKey)) hiveCounts.Add(hiveRole.checkKey, 0);
                    }
                    stillAlone = false; // Not alone anymore
                }
            }
        }

        private List<Pawn> GetAllies()
        {
            hiveGenesPresent.Clear();
            List<Pawn> allies = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
            List<Pawn> tempAllies = allies;
            foreach(Pawn ally in tempAllies)
            {
                bool flag = true;
                if (!ally.Dead && ally.genes != null)
                {
                    List<Gene> genesListForReading = ally.genes.GenesListForReading;
                    foreach (Gene gene in genesListForReading)
                    {
                        if (gene.def.HasModExtension<HiveMindExtension>() && gene.def.GetModExtension<HiveMindExtension>().hiveKey == extension.hiveKey)
                        {
                            hiveGenesPresent.Add(gene.def);
                            flag = false;
                        }
                    }
                }
                if (flag) allies.Remove(ally);
            }
            return allies;
        }

        private void BuildNoAllyList() // Builds a static list to use if the pawn is alone
        {
            foreach (HiveRoleToCheckFor hiveRole in extension.hiveRolesToCheckFor)
            {
                if (hiveRole.minCount > 1)
                {
                    // Adds the hediff to the list if it's not already there
                    if (hiveRole.hediffWhenTooFew != null && !hediffsWhenNoAllies.Contains(hiveRole.hediffWhenTooFew)) hediffsWhenNoAllies.Add(hiveRole.hediffWhenTooFew);
                    if (!hiveRole.hediffsWhenTooFew.NullOrEmpty())
                    {
                        foreach (HediffDef hediff in hiveRole.hediffsWhenTooFew)
                        {
                            if (!hediffsWhenNoAllies.Contains(hediff)) hediffsWhenNoAllies.Add(hediff);
                        }
                    }
                }
            }
        }

        private void BuildAllHediffsList()
        {
            foreach (HiveRoleToCheckFor hiveRole in extension.hiveRolesToCheckFor)
            {
                if (hiveRole.hediffWhenTooFew != null && !allHediffs.Contains(hiveRole.hediffWhenTooFew)) allHediffs.Add(hiveRole.hediffWhenTooFew);
                if (!hiveRole.hediffsWhenTooFew.NullOrEmpty())
                {
                    foreach (HediffDef hediff in hiveRole.hediffsWhenTooFew)
                    {
                        if (!allHediffs.Contains(hediff)) allHediffs.Add(hediff);
                    }
                }
                if (hiveRole.hediffWhenTooMany != null && !allHediffs.Contains(hiveRole.hediffWhenTooMany)) allHediffs.Add(hiveRole.hediffWhenTooMany);
                if (!hiveRole.hediffsWhenTooMany.NullOrEmpty())
                {
                    foreach (HediffDef hediff in hiveRole.hediffsWhenTooMany)
                    {
                        if (!allHediffs.Contains(hediff)) allHediffs.Add(hediff);
                    }
                }
            }
        }

        private void BuildHiveCount(List<Pawn> allies)
        {
            previousCounts = hiveCounts;
            hiveCounts.Clear();
            foreach (GeneDef gene in hiveGenesPresent)
            {
                HiveMindExtension extension = gene.GetModExtension<HiveMindExtension>();
                if (hiveCounts.NullOrEmpty()) hiveCounts.Add(extension.key, 1);
                else if (hiveCounts.ContainsKey(extension.key)) hiveCounts[extension.key]++;
                else hiveCounts.Add(extension.key, 1);
            }
        }
    }
}
