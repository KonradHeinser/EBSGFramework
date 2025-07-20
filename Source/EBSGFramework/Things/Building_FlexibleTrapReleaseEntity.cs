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
                if (!Faction.IsPlayer && Extension?.message != null)
                    Extension.message.Translate(this.Named("TRAP"));
                Messages.Message(Extension.message.TranslateOrLiteral(LabelCap, Label, LabelShort),
                    new LookTargets(PositionHeld, MapHeld), MessageTypeDefOf.NegativeEvent);
                int count = CountToSpawn;
                PawnGenerationRequest request = new PawnGenerationRequest(PawnToSpawn, Faction,
                    biologicalAgeRange: Extension?.bioAge ?? FloatRange.Zero, 
                    fixedChronologicalAge: Extension?.chronoAge ?? 0);

                for (int i = 0; i < count; i++)
                {
                    Pawn pawn = PawnGenerator.GeneratePawn(request);
                    pawn.mindState.enemyTarget = p;
                    GenSpawn.Spawn(pawn, PositionHeld, MapHeld);
                }
            }
        }
    }
}
