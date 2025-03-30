using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class RitualRole
    {
        public IntRange count = new IntRange(1, 1);

        public string id; // Allows for positioning on the building area

        public string label;

        public FloatRange validAges = FloatRange.Zero;

        public SpeciesCategory category = SpeciesCategory.Any;

        public RitualWillingness willingness = RitualWillingness.Colonist;

        public bool AvailableToPawn(Pawn pawn)
        {
            if (!pawn.WithinAges(validAges))
                return false;

            if (pawn.RaceProps.Humanlike)
            {
                if (!category.HasFlag(SpeciesCategory.Humanlike))
                    return false;
            }
            else if (pawn.RaceProps.Animal)
            {
                if (!category.HasFlag(SpeciesCategory.Animal))
                    return false;
            }
            else if (pawn.RaceProps.IsMechanoid)
            {
                if (!category.HasFlag(SpeciesCategory.Mechanoid))
                    return false;
            }
            else if (pawn.RaceProps.Insect)
            {
                if (!category.HasFlag(SpeciesCategory.Insectoid))
                    return false;
            }
            else if (ModsConfig.AnomalyActive && pawn.RaceProps.IsAnomalyEntity)
            {
                if (!category.HasFlag(SpeciesCategory.Entity))
                    return false;
            }

            if (willingness != RitualWillingness.Dead && pawn.Dead)
                return false;

            switch (willingness)
            {
                case RitualWillingness.Colonist:
                    return pawn.IsColonist;
                case RitualWillingness.Downed:
                    return pawn.Downed || pawn.OfTheColony();
                case RitualWillingness.Dead:
                    return pawn.Dead;
                case RitualWillingness.Prisoner:
                    return pawn.IsPrisonerOfColony;
                case RitualWillingness.Slave:
                    return pawn.IsSlaveOfColony;
                case RitualWillingness.ColonistOrSlave:
                    return pawn.IsColonist || pawn.IsSlaveOfColony;
                case RitualWillingness.InColony:
                    return pawn.OfTheColony();
            }
            return true;
        }
    }
}
