using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace EBSGFramework
{
    public class RitualComp
    {
        public RitualCompProperties props;

        public Ritual parent;

        public bool attempt = true;

        public virtual bool ActiveInRitual
        {
            get
            {
                if (!attempt)
                    return false;
                if (props.researchPrerequisite?.IsFinished == false)
                    return false;
                return true;
            }
        }

        public virtual void Initialize(RitualCompProperties props)
        {
            this.props = props;
            attempt = props.attemptDefault;
        }

        public virtual void DoEffects()
        {
        }

        public virtual void Tick()
        {
        }

        public virtual void CompExposeData()
        {
            Scribe_Values.Look(ref attempt, "attempt", true);
        }
    }
}
