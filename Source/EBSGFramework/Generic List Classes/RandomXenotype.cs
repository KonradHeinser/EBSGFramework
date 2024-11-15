using RimWorld;
using Verse;
using System.Xml;

namespace EBSGFramework
{
    public class RandomXenotype
    {
        public float weight = 1f;

        public XenotypeDef xenotype;

        public RandomXenotype()
        {
        }

        public RandomXenotype(XenotypeDef xenotype, float weight)
        {
            this.weight = weight;
            this.xenotype = xenotype;
            if (weight < 0)
            {
                Log.Warning($"Enweightered a xenotype randomizer where the xenotype of {xenotype} has a weight below 0.");
                weight = 0;
            }
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "xenotype", xmlRoot.Name);
            if (num == 1)
            {
                LoadFromSingleNode(xmlRoot.FirstChild);
            }
            else if (num > 1)
            {
                LoadMultipleNodes(xmlRoot);
            }
        }

        private void LoadFromSingleNode(XmlNode node)
        {
            if (node is XmlText xmlText)
            {
                weight = ParseHelper.FromString<float>(xmlText.InnerText);
            }
            else if (node is XmlElement element)
            {
                ParseXmlElement(element);
            }
        }

        private void LoadMultipleNodes(XmlNode xmlRoot)
        {
            foreach (object childNode in xmlRoot.ChildNodes)
            {
                ParseXmlElement(childNode as XmlElement);
            }
        }

        private void ParseXmlElement(XmlElement element)
        {
            if (element.Name == "weight")
            {
                weight = ParseHelper.FromString<float>(element.InnerText);
            }
        }
    }
}
