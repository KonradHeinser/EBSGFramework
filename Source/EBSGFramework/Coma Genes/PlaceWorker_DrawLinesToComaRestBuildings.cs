using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class PlaceWorker_DrawLinesToComaRestBuildings : PlaceWorker
    {
        private static List<Thing> tmpLinkedThings = new List<Thing>();

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            Room room = center.GetRoom(Find.CurrentMap);
            if (room == null)
                return;
            bool isBed = def.IsBed;

            foreach (Region region in room.Regions)
                foreach (Thing item in region.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
                    if (CanShowConnectionTo(def, item, center, isBed))
                        GenDraw.DrawLineBetween(center.ToVector3Shifted(), item.TrueCenter());
        }

        public override void DrawPlaceMouseAttachments(float curX, ref float curY, BuildableDef def, IntVec3 center, Rot4 rot)
        {
            tmpLinkedThings.Clear();
            Room room = center.GetRoom(Find.CurrentMap);
            bool isBed = def is ThingDef thing && thing.IsBed;

            if (room != null)
                foreach (Region region in room.Regions)
                    foreach (Thing item in region.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
                        if (CanShowConnectionTo((ThingDef)def, item, center, isBed) && !tmpLinkedThings.Contains(item))
                        {
                            tmpLinkedThings.Add(item);
                            if (tmpLinkedThings.Count == 1)
                                DrawTextLine(ref curY, "FacilityPotentiallyLinkedTo".Translate() + ":");

                            DrawTextLine(ref curY, "  - " + item.LabelCap);
                        }

            base.DrawPlaceMouseAttachments(curX, ref curY, def, center, rot);
            void DrawTextLine(ref float y, string text)
            {
                float lineHeight = Text.LineHeight;
                Widgets.Label(new Rect(curX, y, 999f, lineHeight), text);
                y += lineHeight;
            }
        }

        private bool CanShowConnectionTo(ThingDef def, Thing t, IntVec3 center, bool defBed)
        {
            if (t.def == def)
                return false;

            // One of them needs to a bed. This is to avoid drawing an excessive number of lines
            if (!defBed && !t.def.IsBed)
                return false;

            CompComaGeneBindable compBindable = t.TryGetComp<CompComaGeneBindable>();
            if (compBindable == null)
                return false;

            if (compBindable.BoundPawn != null)
                return false;

            foreach (CompProperties comp in def.comps)
                if (comp is CompProperties_ComaBindable defComaProps)
                    if (!EBSGUtilities.AnyGeneDefSame(defComaProps.relatedGenes, compBindable.Props.relatedGenes))
                        return false;

            if (!GenSight.LineOfSight(center, t.OccupiedRect().CenterCell, Find.CurrentMap))
                return false;

            return true;
        }
    }
}
