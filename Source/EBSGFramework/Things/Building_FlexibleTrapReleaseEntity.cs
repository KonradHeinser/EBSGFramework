using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI;

namespace EBSGFramework
{
    public class Building_FlexibleTrapReleaseEntity : Building_TrapReleaseEntity
    {
        protected override PawnKindDef PawnToSpawn => Extension?.pawnKind ?? PawnKindDefOf.Muffalo;

        protected override int CountToSpawn => Extension?.countRange.RandomInRange ?? 1;

        private EBSGExtension extension;

        public EBSGExtension Extension
        {
            get 
            {
                if (extension == null)
                    extension = def.GetModExtension<EBSGExtension>();
                return extension; 
            }
        }

        protected override void SpringSub(Pawn p)
        {
            if (MapHeld != null && PositionHeld.IsValid)
            {
                Extension?.sound?.PlayOneShot(new TargetInfo(PositionHeld, MapHeld));
                int count = CountToSpawn;
                if (!Faction.IsPlayer && Extension?.message != null)
                    Messages.Message(Extension.message.TranslateOrFormat(LabelCap, Label, LabelShort, count.ToString()),
                        new LookTargets(PositionHeld, MapHeld), MessageTypeDefOf.NegativeEvent);
                
                PawnGenerationRequest request = new PawnGenerationRequest(PawnToSpawn, Faction,
                    fixedBiologicalAge: Extension?.bioAge.RandomInRange ?? 0, 
                    fixedChronologicalAge: Extension?.chronoAge.RandomInRange ?? 0);
                
                for (int i = 0; i < count; i++)
                {
                    Pawn pawn = PawnGenerator.GeneratePawn(request);
                    
                    GenSpawn.Spawn(pawn, PositionHeld, MapHeld);
                    pawn.mindState.enemyTarget = p;
                    if (Extension?.mentalState != null)
                        pawn.mindState.mentalStateHandler.TryStartMentalState(Extension.mentalState, otherPawn: p);
                }
            }
        }
    }
}
