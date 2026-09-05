using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_DuplicatePawn : CompAbilityEffect_WithDuration
    {
        private new CompProperties_AbilityDuplicatePawn Props => (CompProperties_AbilityDuplicatePawn)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var count = Props.count.RandomInRange;
            var duplicator = Find.PawnDuplicator;
            var source = Props.duplicateCaster ? parent.pawn : target.Thing as Pawn;
            if (source == null) return;
            var changeName = !Props.keepName && source.RaceProps.Humanlike;
            var map = source.MapHeld;
            var pos = source.PositionHeld;
            for (var i = 0; i < count; i++)
            {
                Pawn dupe = duplicator.Duplicate(source);
                if (dupe == null) return; // If we can't ever generate a duplicate, no need to stay here
                
                Props.hediffsToGive?.GiveHediffs(parent.pawn, dupe, GetDurationSeconds(parent.pawn).SecondsToTicks(),
                    GetDurationSeconds(dupe).SecondsToTicks(), Props.psychic);
                
                Gender? gender = Props.randomGender ? (Rand.Bool ? Gender.Male : Gender.Female) : Props.gender;
                
                if (!dupe.CheckGender(gender, Props.keepNameOnGenderChange) && changeName)
                    dupe.Name = PawnBioAndNameGenerator.GeneratePawnName(dupe, NameStyle.Full, null, false, dupe.genes.Xenotype);
                
                if (Props.useCasterFaction && dupe.Faction != parent.pawn.Faction)
                    dupe.SetFaction(parent.pawn.Faction, parent.pawn);
                
                if (Props.relation != null)
                    dupe.relations.AddDirectRelation(Props.relation, source);
                
                if (Props.casterRelation != null)
                    dupe.relations.AddDirectRelation(Props.casterRelation, parent.pawn);
                
                GenSpawn.Spawn(dupe, CellFinder.RandomSpawnCellForPawnNear(pos, map, 2), map);
            }
        }
    }
}