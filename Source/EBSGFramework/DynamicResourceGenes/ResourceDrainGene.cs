using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ResourceDrainGene : Gene, IGeneResourceDrain
    {
        [Unsaved(false)]
        private ResourceGene cachedResourceGene;

        DRGExtension extension = null;

        public bool extensionAlreadyChecked = false;

        public bool CanOffset
        {
            get
            {
                if (Active)
                {
                    return true;
                }
                return false;
            }
        }


        private const float MinAgeForDrain = 3f;

        public Gene_Resource Resource
        {
            get
            {
                if (!def.HasModExtension<DRGExtension>()) Log.Error(def + "doesn't have the DRG extension, meaning the main resource gene cannot be found.");
                else if (cachedResourceGene == null || !cachedResourceGene.Active)
                {
                    cachedResourceGene = (ResourceGene)pawn.genes.GetGene(def.GetModExtension<DRGExtension>().mainResourceGene);
                }
                return cachedResourceGene;
            }
        }

        public float ResourceLossPerDay => def.resourceLossPerDay;

        public Pawn Pawn => pawn;

        public string DisplayLabel => Label + " (" + "Gene".Translate() + ")";

        public override void Tick()
        {
            base.Tick();
            if (extension == null && !extensionAlreadyChecked)
            {
                extension = def.GetModExtension<DRGExtension>();
                extensionAlreadyChecked = true;
            }
            if (Resource != null) ResourceGene.OffsetResource(pawn, ResourceLossPerDay * -1, cachedResourceGene, extension, false, true);
        }

        public override void PostAdd()
        {
            base.PostAdd();
            EBSGExtension EBSGextension = def.GetModExtension<EBSGExtension>();
            if (EBSGextension != null && !EBSGextension.hediffsToApply.NullOrEmpty())
            {
                HediffAdder.HediffAdding(pawn, this);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo resourceDrainGizmo in GeneResourceDrainUtility.GetResourceDrainGizmos(this))
            {
                yield return resourceDrainGizmo;
            }
        }
    }
}
