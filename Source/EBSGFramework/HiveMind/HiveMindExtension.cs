using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HiveMindExtension : DefModExtension
    {
        public string key;
        /// Each hive role/tier needs its own key. This key is what everything else uses to verify the existence of another thing.
        /// An example of how the key can be used, you can have one key called core, and one key called drone.
        /// You can then have the drones check for if anyone in the colony has the core key, and if none do, add a hediff that does whatever you want(include killing them).
        /// If you want to simulate a part being destroyed due to the dissappearance of the core(i.e. heart explodes) this framework also adds the hediff comp HediffComp_DamageBodyParts.
        /// This can go the other direction as well, with the core gaining hediffs based on specific drone types and counts found in the colony.

        public string hiveKey; // This allows for multiple types of hives in the same game. Much like defNames, this should probably be very unique to avoid having two hives end up being treated as the same hive
        public List<HiveRoleToCheckFor> hiveRolesToCheckFor; // If there is supposed to be a maximum or minimum count of this role, include it in this list
    }
}
