using Verse;

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
                    cachedResourceGene = (ResourceGene)Pawn.genes.GetGene(Props.mainResourceGene);
                
                return cachedResourceGene;
            }
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (Props.severityPerHourEmpty > 0f) severityAdjustment += ((Resource.Value > 0f) ? Props.severityPerHourResource : Props.severityPerHourEmpty) / 2500f * delta;
            else severityAdjustment += ((Resource.Value < Resource.Max) ? Props.severityPerHourResource : Props.severityPerHourFull) / 2500f * delta;

        }
    }
}
