using System.Xml;
using Verse;


namespace EBSGFramework
{
    public class RandomProjectilePackage
    {
        public ThingDef projectile;
        public float weight = 1f;

        public RandomProjectilePackage()
        {
        }

        public RandomProjectilePackage(ThingDef p, float weight)
        {
            this.projectile = p;
            this.weight = weight;

            if (weight < 0)
            {
                Log.Warning($"Enweightered a random shot package where the projectile of {projectile} has a weight below 0.");
                weight = 0f;
            }
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "projectile", xmlRoot.Name);
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
