using System;

namespace EBSGFramework
{
    [Flags]
    public enum ColorPackageGenerator
    {
        // If multiple flags are selected at the same time, pawn generation flags take priority over others
        Default = 1, // Default to pre-selected color
        Faction = 2, // If generated on a pawn, use their faction color. Also uses faction color if apparel itself is assigned a faction
        Ideology = 4, // If generated on a pawn, use their ideology color
        Favorite = 8, // If generated on a pawn, default to their favorite color
        Random = 16, // Default to a random color
        White = 32, // Defaults to white
    }
}
