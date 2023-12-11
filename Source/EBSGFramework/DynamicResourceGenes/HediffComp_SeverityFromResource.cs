using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class HediffComp_SeverityFromResource : HediffComp
    {
        private ResourceGene cachedResourceGene;

        public HediffCompProperties_SeverityFromResource Props => (HediffCompProperties_SeverityFromResource)props;

        public override bool CompShouldRemove => Resource == null;

        private ResourceGene Resource
        {
            get
            {
                if (Props.mainResourceGene == null) Log.Error(parent.Label + "is missing the mainResource gene, meaning it can't increase the resource level.");
                else if (cachedResourceGene == null || !cachedResourceGene.Active)
                {
                    cachedResourceGene = (ResourceGene)base.Pawn.genes.GetGene(Props.mainResourceGene);
                }
                return cachedResourceGene;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Props.severityPerHourEmpty > 0f) severityAdjustment += ((Resource.Value > 0f) ? Props.severityPerHourResource : Props.severityPerHourEmpty) / 2500f;
            else severityAdjustment += ((Resource.Value < Resource.Max) ? Props.severityPerHourResource : Props.severityPerHourFull) / 2500f;
        }
    }
}
