using System.Xml;
using Verse;

namespace EBSGFramework
{
    public class PatchOperationSetting : PatchOperation
    {
        private PatchOperation active = null;

        // private PatchOperation inactive = null;

        public string setting;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (setting == "ageLimitedAgeless")
            {
                if (ModsConfig.BiotechActive && EBSG_Settings.ageLimitedAgeless && active != null) return active.Apply(xml);
            }
            else if (setting == "hideInactiveSkinGenes")
            {
                if (ModsConfig.BiotechActive && EBSG_Settings.hideInactiveSkinGenes && active != null) return active.Apply(xml);
            }
            else if (setting == "hideInactiveHairGenes")
            {
                if (ModsConfig.BiotechActive && EBSG_Settings.hideInactiveHairGenes && active != null) return active.Apply(xml);
            }
            else if (setting == "psychicInsulationBondOpinion")
            {
                if (EBSG_Settings.psychicInsulationBondOpinion && active != null) return active.Apply(xml);
            }
            else if (setting == "psychicInsulationBondMood")
            {
                if (EBSG_Settings.psychicInsulationBondMood && active != null) return active.Apply(xml);
            }
            else if (setting == "superclottingArchite")
            {
                if (EBSG_Settings.superclottingArchite && active != null) return active.Apply(xml);
            }
            else if (setting == "architePsychicInfluencerBondTorn")
            {
                if (EBSG_Settings.architePsychicInfluencerBondTorn && active != null) return active.Apply(xml);
            }
            else if (setting == "noInnateMechlinkPrereq")
            {
                if (EBSG_Settings.noInnateMechlinkPrereq && active != null) return active.Apply(xml);
            }
            else if (setting == "noInnatePsylinkPrereq")
            {
                if (EBSG_Settings.noInnatePsylinkPrereq && active != null) return active.Apply(xml);
            }
            else if (setting != null) Log.Error("A patch is using a setting that is either mispelled or unhandled");
            else Log.Error("A patch is using this mod's settings, but doesn't specify which one.");
            return true;
        }
    }
}
