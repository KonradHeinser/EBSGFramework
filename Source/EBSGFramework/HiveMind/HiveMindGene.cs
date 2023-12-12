using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HiveMindGene : Gene
    {
        public HiveMindExtension extension = null;
        public List<GeneDef> hiveGenesPresent = new List<GeneDef>(); // Only goes through gene lists once per viable tick
        public List<HediffDef> hediffsWhenNoAllies = new List<HediffDef>(); // This list makes it so things don't need to iterate so much
        public List<HediffDef> allHediffs = new List<HediffDef>(); // This list was created for the basic wipe, but may have unexpected future use
        public List<HediffDef> addedHediffs = new List<HediffDef>();
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
                    completeWipe = false;
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

            if (allHediffs.NullOrEmpty()) BuildAllHediffsList();
            if (completeWipe)
            {
                EBSGUtilities.RemoveHediffs(pawn, null, allHediffs); // Wipe slate
                completeWipe = false;
            }
            StartHiveCheck();
        }

        public override void PostRemove()
        {
            base.PostRemove();
            EBSGUtilities.RemoveHediffs(pawn, null, addedHediffs); // Wipe slate
        }

        public void StartHiveCheck()
        {
            List<Pawn> allies = GetAllies();
            if (allies.NullOrEmpty()) // If there's no allies, get out of here because this place is weird
            {
                EBSGUtilities.RemoveHediffs(pawn, null, addedHediffs); // Wipe slate
                previousCounts.Clear();
                addedHediffs.Clear();
                stillAlone = false; // Prep for less weird scenario
                return;
            }
            if (!hiveCounts.NullOrEmpty()) previousCounts = new Dictionary<string, int>(hiveCounts);
            BuildHiveCount(allies);
            if (previousCounts.NullOrEmpty() || !EBSGUtilities.EqualCountingDictionaries(previousCounts, hiveCounts)) // If there's a change in the hive
            {
                if (hiveGenesPresent.Count == 1) // If this pawn is the only one in the hive, there's no reason to waste time trying to go through everyone's genes in depth, just apply the lonely hediffs
                {
                    if (!stillAlone)
                    {
                        EBSGUtilities.RemoveHediffs(pawn, null, addedHediffs); // Wipe slate
                        previousCounts.Clear();
                        addedHediffs = EBSGUtilities.ApplyHediffs(pawn, null, hediffsWhenNoAllies); // Add the lonely hediffs
                        stillAlone = true; // Prep for continued loneliness. No reason to keep removing and adding hediffs if still alone
                    }
                }
                else
                {

                    EBSGUtilities.RemoveHediffs(pawn, null, addedHediffs); // Wipe slate
                    addedHediffs.Clear();
                    foreach (HiveRoleToCheckFor hiveRole in extension.hiveRolesToCheckFor)
                    {
                        List<HediffDef> tempHediffs = new List<HediffDef>();
                        if (hiveCounts[hiveRole.checkKey] < hiveRole.minCount) tempHediffs = EBSGUtilities.ApplyHediffs(pawn, hiveRole.hediffWhenTooFew, hiveRole.hediffsWhenTooFew);
                        else if (hiveRole.minCount <= hiveRole.maxCount && hiveCounts[hiveRole.checkKey] > hiveRole.maxCount) tempHediffs = EBSGUtilities.ApplyHediffs(pawn, hiveRole.hediffWhenTooMany, hiveRole.hediffsWhenTooMany);
                        else tempHediffs = EBSGUtilities.ApplyHediffs(pawn, hiveRole.hediffWhenEnough, hiveRole.hediffsWhenEnough);

                        if (addedHediffs.NullOrEmpty()) addedHediffs = tempHediffs;
                        else if (!tempHediffs.NullOrEmpty()) foreach (HediffDef hediff in tempHediffs) addedHediffs.Add(hediff);
                    }
                    stillAlone = false; // Not alone anymore
                }
            }
        }

        public List<Pawn> GetAllies()
        {
            hiveGenesPresent.Clear();
            List<Pawn> allies = new List<Pawn>(pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction));
            List<Pawn> removeAllies = new List<Pawn>();
            foreach(Pawn ally in allies)
            {
                bool flag = true;
                if (!ally.Dead && ally.genes != null)
                {
                    if (ally.genes?.GetFirstGeneOfType<HiveMindGene>() != null)
                    {
                        List<Gene> genesListForReading = new List<Gene>(ally.genes.GenesListForReading);
                        foreach (Gene gene in genesListForReading)
                        {
                            if (gene.Active && gene.def.HasModExtension<HiveMindExtension>() && gene.def.GetModExtension<HiveMindExtension>().hiveKey == extension.hiveKey)
                            {
                                hiveGenesPresent.Add(gene.def);
                                flag = false;
                                break;
                            }
                        }
                    }
                }
                if (flag) removeAllies.Add(ally);
            }
            foreach (Pawn ally in removeAllies) allies.Remove(ally);
            return allies;
        }

        public void BuildNoAllyList() // Builds a static list to use if the pawn is alone
        {
            foreach (HiveRoleToCheckFor hiveRole in extension.hiveRolesToCheckFor)
            {
                if ((hiveRole.minCount == 1 && hiveRole.checkKey == extension.key) || hiveRole.minCount == 0)
                {
                    if (hiveRole.hediffWhenEnough != null && (hediffsWhenNoAllies.NullOrEmpty() || !hediffsWhenNoAllies.Contains(hiveRole.hediffWhenEnough)))
                    {
                        hediffsWhenNoAllies.Add(hiveRole.hediffWhenEnough);
                    }
                    if (!hiveRole.hediffsWhenEnough.NullOrEmpty())
                    {
                        foreach (HediffDef hediff in hiveRole.hediffsWhenEnough)
                        {
                            if (hediffsWhenNoAllies.NullOrEmpty() || !hediffsWhenNoAllies.Contains(hediff)) hediffsWhenNoAllies.Add(hediff);
                        }
                    }
                }
                else if ((hiveRole.minCount > 1 && hiveRole.checkKey == extension.key) || hiveRole.minCount > 0)
                {
                    // Adds the hediff to the list if it's not already there
                    if (hiveRole.hediffWhenTooFew != null && (hediffsWhenNoAllies.NullOrEmpty() || !hediffsWhenNoAllies.Contains(hiveRole.hediffWhenTooFew)))
                    {
                        hediffsWhenNoAllies.Add(hiveRole.hediffWhenTooFew);
                    }
                    if (!hiveRole.hediffsWhenTooFew.NullOrEmpty())
                    {
                        foreach (HediffDef hediff in hiveRole.hediffsWhenTooFew)
                        {
                            if (hediffsWhenNoAllies.NullOrEmpty() || !hediffsWhenNoAllies.Contains(hediff)) hediffsWhenNoAllies.Add(hediff);
                        }
                    }
                }
            }
        }

        public void BuildAllHediffsList()
        {
            foreach (HiveRoleToCheckFor hiveRole in extension.hiveRolesToCheckFor)
            {
                if (hiveRole.hediffWhenTooFew != null && (allHediffs.NullOrEmpty() || !allHediffs.Contains(hiveRole.hediffWhenTooFew))) allHediffs.Add(hiveRole.hediffWhenTooFew);
                if (!hiveRole.hediffsWhenTooFew.NullOrEmpty())
                {
                    foreach (HediffDef hediff in hiveRole.hediffsWhenTooFew)
                    {
                        if (allHediffs.NullOrEmpty() || !allHediffs.Contains(hediff)) allHediffs.Add(hediff);
                    }
                }
                if (hiveRole.hediffWhenTooMany != null && (allHediffs.NullOrEmpty() || !allHediffs.Contains(hiveRole.hediffWhenTooMany))) allHediffs.Add(hiveRole.hediffWhenTooMany);
                if (!hiveRole.hediffsWhenTooMany.NullOrEmpty())
                {
                    foreach (HediffDef hediff in hiveRole.hediffsWhenTooMany)
                    {
                        if (allHediffs.NullOrEmpty() || !allHediffs.Contains(hediff)) allHediffs.Add(hediff);
                    }
                }
            }
        }

        public void BuildHiveCount(List<Pawn> allies)
        { 
            hiveCounts.Clear();
            foreach (GeneDef gene in hiveGenesPresent)
            {
                HiveMindExtension hiveExtension = gene.GetModExtension<HiveMindExtension>();
                if (hiveCounts.NullOrEmpty()) hiveCounts.Add(hiveExtension.key, 1);
                else if (hiveCounts.ContainsKey(hiveExtension.key)) hiveCounts[hiveExtension.key]++;
                else hiveCounts.Add(hiveExtension.key, 1);
            }
            foreach (HiveRoleToCheckFor hiveRole in extension.hiveRolesToCheckFor) if (!hiveCounts.ContainsKey(hiveRole.checkKey)) hiveCounts.Add(hiveRole.checkKey, 0);
        }
    }
}
