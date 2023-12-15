using System;
namespace EBSGFramework
{
    public class RegenSet
    {
        public int ticksToRegrowPart = 0; // If not 0, then the regeneration will return lost parts. This will also return eyes, so this may interact strangly with ideologies the revere blindness
        public int ticksToHealInterval = 100; // Every x ticks, heal an injury for healAmount
        public float healAmount = 0.1f; // Amount to reduce injury severity by
        public int repeatHealCount = 1; // How many times repeat the healing. The healing will target a random part each repeat
        public float minSeverity = 0.0f; // By default the hediff only needs to exist to regenerate
        public float maxSeverity = 999.9f; // By default the hediff only needs to exist to regenerate, unless you have a really weird max value
        public int healTicksPerTick = 1;
        public int regrowTicksPerTick = 1;


        /*
            This regeneration hediff prioritizes regrowing parts first. 
            If ticks to regrow is greater than 0, it will start with checking for lost parts, and if it finds one it will return the lost part after x ticks
                Note: If all lost parts are returned through other methods before this hediff gives a part back, it will scrap the attempt and start trying to do a regular heal
                Note 2: If returning a part results in more missing parts(i.e. return an arm and have missing hand/fingers), then those are NOT regenerated this cycle
                    It's set up this way to make returning certain larger parts take longer without setting up a whole regrowing limb thing. Or because I'm lazy, whichever you want to go for.
                                       
            If ticks to regrow is not greater than 0 or no lost parts are found, it will begin a heal interval as long as ticksToHealInterval is greater than 0
            If it finds injuries at the end of the heal interval, it will randomly select a number of injuries up to repeatHealCount, and reduce the severity by healAmount
                Note: One injury may be selected multiple times, even if there are more injuries than repeatCounts
                               
            Normally regrowing parts stops standard regen for balance purposes, but there are two ways to customize this function in the main comp
                First, you can allow both of them to be active at the same time by switching healWhileRegrowing to true 
                Alternatively, you can make it heal injuries before restoring lost parts by switching prioritizeHeal to true
                    Even if heal is prioritized, the regeneration will still prefer to finish the current regrowth if it starts one before new injuries are received               

            Final part of my spiel, the reason this is set up as a list of regens instead of the normal method mod devs use, severity conditionals
            By default, the regeneration is set to assume it is the only one, and that it always needs to be active
                If minSeverity and maxSeverity are changed, then the regeneration will only trigger if the hediff the comp is attached to is within the new range
                    To prevent weird situations, only the first match is used, so to ensure this is used properly, set min and max on all list items
                With the way it is set up, it can be used both as a generic regenerator, and as a tier regeneration hediff
                    With it being a tiered regeneration, you can do things like make it so parts are only regrown once the hediff reaches a certain level, or higher levels increase the base heal rate
        */
    }
}
