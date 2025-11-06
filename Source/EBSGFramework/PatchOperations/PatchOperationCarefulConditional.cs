using System.Collections.Generic;
using System.Xml;
using Verse;

namespace EBSGFramework
{
    public class PatchOperationCarefulConditional : PatchOperationPathed
    {
        private PatchOperation match = null;

        private List<CarefulOperation> conditions = null;

        private PatchOperation nomatch = null;

        private bool warnWhenFailed = false; // Creates a warning that says at what point it failed

        protected override bool ApplyWorker(XmlDocument xml)
        {
            string[] pathy = xpath.Split('/');
            string path = "";
            List<string> failPoints = new List<string>();

            if (!conditions.NullOrEmpty())
                foreach (CarefulOperation operation in conditions)
                    failPoints.Add(operation.failPoint);

            foreach (string s in pathy)
            {
                path += s;
                if (xml.SelectSingleNode(path) == null)
                {
                    if (warnWhenFailed)
                        Log.Warning($"CarefulConditional failPoint of {s}\nFull fail path {path}\nAttempted to reach {xpath}\n");
                    if (!failPoints.NullOrEmpty() && failPoints.Contains(s))
                        return conditions[failPoints.IndexOf(s)].operation.Apply(xml);
                    if (nomatch != null)
                        return nomatch.Apply(xml);
                    return match != null || !failPoints.NullOrEmpty();
                }
                path += "/";
            }

            if (match != null)
                return match.Apply(xml);

            return true;
        }
    }
}
