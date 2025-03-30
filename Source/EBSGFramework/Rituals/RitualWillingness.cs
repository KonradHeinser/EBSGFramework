using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBSGFramework
{
    public enum RitualWillingness
    {
        Colonist, // Colonist
        Downed, // Any downed pawn, friend or foe
        Dead, // Any dead pawn, friend or foe
        Prisoner,
        Slave,
        ColonistOrSlave,
        InColony // Colonist, Slave, or Prisoner
    }
}
