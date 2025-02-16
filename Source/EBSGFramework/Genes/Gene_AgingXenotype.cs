using Verse;

namespace EBSGFramework
{
    public class Gene_AgingXenotype : HediffAdder
    {
        private bool alreadyChanged = false;

        private AgingXenotypeExtension agingExtension;
        public AgingXenotypeExtension AgingExtension
        {
            get
            {
                if (agingExtension == null)
                    agingExtension = def.GetModExtension<AgingXenotypeExtension>();
                return agingExtension;
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            if (AgingExtension?.xenotypes.NullOrEmpty() != false)
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
            if (!alreadyChanged && pawn.IsHashIntervalTick(2500))
                CheckXenotypes();
        }


        private void CheckXenotypes()
        {
            foreach (XenoRange xeno in AgingExtension.xenotypes) 
                if (xeno.range.Includes(pawn.ageTracker.AgeBiologicalYearsFloat) && pawn.genes.Xenotype != xeno.xenotype)
                {
                    alreadyChanged = true;
                    pawn.AlterXenotype(xeno.xenotype, AgingExtension.filth, AgingExtension.filthAmount, agingExtension.setXenotype, agingExtension.message != null, agingExtension.message);
                    return;
                }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref alreadyChanged, "alreadyChanged", false);
        }
    }
}
