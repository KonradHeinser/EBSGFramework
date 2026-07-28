using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Apparel : ConditionalStatAffecter
    {
        public List<ThingDef> apparel;
        
        public CheckType apparelCheck = CheckType.Required;

        public List<string> tags;
        
        public CheckType tagCheck = CheckType.Required;
        
        public List<ThingDef> stuffing;

        public CheckType stuffingCheck = CheckType.Required;
        
        public List<StuffCategoryDef> stuffCategories;
        
        public List<ApparelLayerDef> layers;
        
        public CheckType layerCheck = CheckType.Required;

        public List<BodyPartGroupDef> bodyPartGroups;
        
        public CheckType bodyPartGroupCheck = CheckType.Required;
        
        public bool defaultActive = true; // Used if the pawn can't have apparel
        
        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label.TranslateOrFormat();
            return "EBSG_CorrectConditions".Translate();
        }
        
        public override bool Applies(StatRequest req)
        {
            if (req.Pawn?.apparel == null)
                return defaultActive;
            
            if (!req.Pawn.apparel.AnyApparel)
                return ((stuffing.NullOrEmpty() && stuffCategories.NullOrEmpty()) || stuffingCheck != CheckType.Required) &&
                       (apparel.NullOrEmpty() || apparelCheck != CheckType.Required) && (tags.NullOrEmpty() || tagCheck != CheckType.Required) &&
                       (layers.NullOrEmpty() || layerCheck != CheckType.Required) && (bodyPartGroups.NullOrEmpty() || bodyPartGroupCheck != CheckType.Required);

            // This affecter goes through all the pawn's worn apparel to see if any of them meet all the conditions
            List<Apparel> apparelOfInterest = new List<Apparel>(req.Pawn.apparel.WornApparel);

            if (apparel.Any()) // Start with the defs check because if that exists, it's probably the biggest limiter
            {
                // First get all apparel in the interest list which our list wants to see 
                var defs = apparelOfInterest.Where(a => apparel.Contains(a.def));

                if (defs.Any()) // If there is at least one apparel that works for our purpose, see what kind of check we're doing
                {
                    switch (apparelCheck)
                    {
                        case CheckType.Required: // If we need these defs to appear in the list, remove all the ones that don't have that defName
                            apparelOfInterest.RemoveAll(a => !defs.Contains(a));
                            break;
                        case CheckType.Forbidden: // If we need them to not be in the list, etc. etc.
                            apparelOfInterest.RemoveAll(a => defs.Contains(a));
                            break;
                        case CheckType.None: // These shouldn't happen. If it becomes a problem for some reason, an error message will go here
                        default:
                            break;
                    }
                    if (!apparelOfInterest.Any()) // If there aren't any viable apparel after the removal, we're done here
                        return false;
                }
                else if (apparelCheck == CheckType.Required) // If there weren't any apparel that met the conditions, see if that's going to be a problem
                    return false;
            }

            if (tags.Any())
            {
                var taggedApparel = apparelOfInterest.Where(a => a.def.apparel?.tags?.NullOrEmpty() == false && 
                                                                 tags.ContainsAny(t => a.def.apparel.tags.Contains(t)));

                if (taggedApparel.Any())
                {
                    switch (tagCheck)
                    {
                        case CheckType.Required:
                            apparelOfInterest.RemoveAll(a => !taggedApparel.Contains(a));
                            break;
                        case CheckType.Forbidden:
                            apparelOfInterest.RemoveAll(a => taggedApparel.Contains(a));
                            break;
                        case CheckType.None:
                        default:
                            break;
                    }
                    if (!apparelOfInterest.Any())
                        return false;
                }
                else if (tagCheck == CheckType.Required)
                    return false;
            }

            var stuffCheck = stuffing.Any();
            var categoryCheck = stuffCategories.Any();
            if (stuffCheck || categoryCheck) // All the other checks act pretty much the same, so I'm not going to spend the time writing notes for all of them
            {
                var stuffedApparel = apparelOfInterest.Where(a => a.Stuff != null && 
                    ((stuffCheck && stuffing.Contains(a.Stuff)) || (categoryCheck && stuffCategories.ContainsAny(c => a.Stuff.stuffCategories.Contains(c)))));
                
                if (stuffedApparel.Any())
                {
                    switch (stuffingCheck)
                    {
                        case CheckType.Required:
                            apparelOfInterest.RemoveAll(a => !stuffedApparel.Contains(a));
                            break;
                        case CheckType.Forbidden:
                            apparelOfInterest.RemoveAll(a => stuffedApparel.Contains(a));
                            break;
                        case CheckType.None:
                        default:
                            break;
                    }
                    if (!apparelOfInterest.Any())
                        return false;
                }
                else if (stuffingCheck == CheckType.Required)
                    return false;
            }
            
            if (layers.Any())
            {
                var layeredApparel = apparelOfInterest.Where(a => a.def.apparel.layers.ContainsAny(l => layers.Contains(l)));

                if (layeredApparel.Any())
                {
                    switch (layerCheck)
                    {
                        case CheckType.Required:
                            apparelOfInterest.RemoveAll(a => !layeredApparel.Contains(a));
                            break;
                        case CheckType.Forbidden:
                            apparelOfInterest.RemoveAll(a => layeredApparel.Contains(a));
                            break;
                        case CheckType.None:
                        default:
                            break;
                    }
                    if (!apparelOfInterest.Any())
                        return false;
                }
                else if (layerCheck == CheckType.Required)
                    return false;
            }
            
            if (bodyPartGroups.Any())
            {
                var groupedApparel = apparelOfInterest.Where(a => a.def.apparel.bodyPartGroups.ContainsAny(g => bodyPartGroups.Contains(g)));
                
                if (groupedApparel.Any())
                {
                    switch (bodyPartGroupCheck)
                    {
                        case CheckType.Required:
                            apparelOfInterest.RemoveAll(a => !groupedApparel.Contains(a));
                            break;
                        case CheckType.Forbidden:
                            apparelOfInterest.RemoveAll(a => groupedApparel.Contains(a));
                            break;
                        case CheckType.None:
                        default:
                            break;
                    }
                    if (!apparelOfInterest.Any())
                        return false;
                }
                else if (bodyPartGroupCheck == CheckType.Required)
                    return false;
            }
            
            return true; // At this point, at least one apparel has made it through all the hoops that have been designated, and the condition is active
        }
    }
}