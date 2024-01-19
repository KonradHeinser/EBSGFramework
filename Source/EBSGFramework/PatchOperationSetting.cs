using Verse;
using System.Xml;

namespace EBSGFramework
{
    public class PatchOperationSetting : PatchOperation
    {
        private PatchOperation active = null;

        private PatchOperation inactive = null;

        public string setting;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (setting == "ageLimitedAgeless")
            {
                if (EBSG_Settings.ageLimitedAgeless && active != null) return active.Apply(xml);
            }
            else if (setting != null) Log.Error("A patch is using a setting that is either mispelled or unhandled");
            else Log.Error("A patch is using this mod's settings, but doesn't specify which one.");
            return true;
        }
    }
}
