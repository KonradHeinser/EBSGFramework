using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class Gene_AgingXenotype : Gene
    {
        private AgingXenotypeExtension extension;

        public AgingXenotypeExtension Extension
        {
            get
            {
                if (extension == null)
                    extension = def.GetModExtension<AgingXenotypeExtension>();
                return extension;
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            if (Extension?.xenotypes.NullOrEmpty() != false)
            {
                Log.Error($"{def} is missing a list of xenotypes in AgingXenotypeExtension. Removing gene to avoid more errors");
                pawn.genes.RemoveGene(this);
                return;
            }
            CheckXenotypes();
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn.IsHashIntervalTick(2500))
                CheckXenotypes();
        }


        private void CheckXenotypes()
        {
            foreach (XenoRange xeno in extension.xenotypes) 
                if (xeno.range.Includes(pawn.ageTracker.AgeBiologicalYearsFloat))
                {
                    pawn.AlterXenotype(xeno.xenotype, extension.filth, extension.filthAmount, extension.setXenotype, extension.message != null, extension.message);
                    return;
                }
        }
    }
}
